using SchulIT.IccImport;
using SchulIT.IccImport.Models;
using SchulIT.SvwsClient;
using SvwsIccImporter.Extensions;
using SvwsIccImporter.Service;
using SvwsIccImporter.Settings;
using SvwsIccImporter.UI.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvwsIccImporter.ViewModel.Import
{
    [Priority(20)]
    public partial class ImportStudentsTaskViewModel : ImportTaskViewModelBase
    {

        public ImportStudentsTaskViewModel(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
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

                BusyText = $"Hole Stammdaten für {schuelerCount} Kind(er)";
                var schuelerStammdatenListe = await client.GetSchuelerStammdatenMultipleAsync(settingsManager.Settings.Svws.Schema, schuelerListeAktuellerAbschnitt.Select(x => x.Id).ToList());

                BusyText = $"Hole Datenschutz-Einwilligungen für {schuelerCount} Kind(er)";
                var schuelerDatenschutzDaten = await client.GetSchuelerEinwilligungenBySchuelerIdsAsync(settingsManager.Settings.Svws.Schema, schuelerListeAktuellerAbschnitt.Select(x => x.Id).ToList());

                var students = new List<StudentData>();

                foreach (var stammdaten in schuelerStammdatenListe)
                {
                    students.Add(new StudentData
                    {
                        Id = stammdaten.Id.ToString(),
                        Firstname = stammdaten.Vorname.Cleanup(),
                        Lastname = stammdaten.Nachname.Cleanup(),
                        Email = stammdaten.EmailSchule.Cleanup(),
                        Gender = stammdaten.Geschlecht.ToIccGender(),
                        Status = stammdaten.Status.ToString(),
                        Birthday = stammdaten.Geburtsdatum.ToDateTime(),
                        ApprovedPrivacyCategories = schuelerDatenschutzDaten.Where(x => x.IdSchueler == stammdaten.Id).Where(x => x.Status == true).Select(x => x.IdEinwilligungsart.ToString()).ToList()
                    });
                }

                students = students
                    .Where(x => !string.IsNullOrEmpty(x.Firstname) && !string.IsNullOrEmpty(x.Lastname) && !string.IsNullOrEmpty(x.Email) && x.Birthday != null)
                    .ToList();

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportStudentsAsync(students, (int)abschnitt.Abschnitt, (int)abschnitt.Schuljahr);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                dialogHelper.Show(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim SuS-Import",
                        Content = "Beim Import der Lernenden ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }
    }
}
