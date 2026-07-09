using SchulIT.IccImport;
using SchulIT.IccImport.Models;
using SchulIT.SvwsClient;
using SvwsIccImporter.Service;
using SvwsIccImporter.Settings;
using SvwsIccImporter.UI.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvwsIccImporter.ViewModel.Import
{
    [Priority(23)]
    public class ImportStudentLearningManagementSystemsTaskViewModel : ImportTaskViewModelBase
    {
        public ImportStudentLearningManagementSystemsTaskViewModel(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager)
        {

        }

        public override async Task<bool> RunAsync(Schuljahresabschnitt abschnitt)
        {
            try
            {
                CurrentStatus = TaskStatus.Running;
                BusyText = "Lade Lernende aus SVWS-Datenbank";

                var client = restClientFactory.GetClient();
                var schuelerListe = await client.GetSchuelerFuerAbschnittAsync(settingsManager.Settings.Svws.Schema, abschnitt.Id);

                BusyText = $"Filtere {schuelerListe.Count} Kind(er) auf den aktuellen Abschnitt";
                var schuelerListeAktuellerAbschnitt = schuelerListe.Where(x => x.IdSchuljahresabschnitt == abschnitt.Id).ToArray();
                var schuelerCount = schuelerListeAktuellerAbschnitt.Count();

                BusyText = $"Transformiere {schuelerCount} Lernende für den ICC-Import";

                var data = new List<StudentLearningManagementSystemData>();
                var idx = 0;

                foreach(var schueler in schuelerListeAktuellerAbschnitt)
                {
                    var einwilligungen = await client.GetSchuelerLernplattformenAsync(settingsManager.Settings.Svws.Schema, schueler.Id);

                    foreach(var einwilligung in einwilligungen)
                    {
                        data.Add(new StudentLearningManagementSystemData
                        {
                            Student = schueler.Id.ToString(),
                            Lms = einwilligung.IdLernplattform.ToString(),
                            Username = einwilligung.Benutzername,
                            Password = einwilligung.InitialKennwort,
                            IsConsented = einwilligung.EinwilligungNutzung,
                            IsConsentObtained = einwilligung.EinwilligungAbgefragt,
                            IsAudioConsented = einwilligung.EinwilligungAudiokonferenz,
                            IsVideoConsented = einwilligung.EinwilligungVideokonferenz
                        });
                    }

                    idx++;

                    BusyText = $"Transformiere {schuelerCount} Lernende für den ICC-Import ({idx}/{schuelerCount})";
                }

                data = data
                        .Where(x => !string.IsNullOrEmpty(x.Student) && !string.IsNullOrEmpty(x.Lms))
                        .DistinctBy(x => string.Join('-', x.Student, x.Lms))
                        .ToList();

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportLearningManagementSystemStudentsAsync(data);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                dialogHelper.Show(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim Fächer-Import",
                        Content = "Beim Import der Fächer ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }
    }
}
