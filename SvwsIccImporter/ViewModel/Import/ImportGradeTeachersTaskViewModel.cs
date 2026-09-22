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
    [Priority(11)]
    public class ImportGradeTeachersTaskViewModel : ImportTaskViewModelBase
    {

        public ImportGradeTeachersTaskViewModel(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager)
        {

        }

        public override async Task<bool> RunAsync(Schuljahresabschnitt abschnitt)
        {
            try
            {
                CurrentStatus = TaskStatus.Running;
                var client = restClientFactory.GetClient();

                BusyText = "Lade Lehrkräfte aus der SVWS-Datenbank";
                var lehrerListe = await client.GetLehrerAsync(settingsManager.Settings.Svws.Schema);

                if(settingsManager.Settings.Svws.OnlyVisible)
                {
                    lehrerListe = lehrerListe.Where(x => x.IstSichtbar).ToList();
                }

                var lehrkraefte = lehrerListe.ToDictionary(x => x.Id);

                BusyText = "Lade Klassenleitungen aus der SVWS-Datenbank";
                var klassenListe = await client.GetKlassenFuerAbschnittAsync(settingsManager.Settings.Svws.Schema, abschnitt.Id);       

                BusyText = $"Transformiere Klassenleitungen für den ICC-Import";
                var data = new List<GradeTeacherData>();

                await Task.Run(() =>
                {
                    foreach(var klasse in klassenListe)
                    {
                        foreach (var l in klasse.KlassenLeitungen)
                        {
                            if(!lehrkraefte.ContainsKey(l))
                            {
                                continue;
                            }

                            data.Add(new GradeTeacherData()
                            {
                                Grade = klasse.Kuerzel.Cleanup(),
                                Teacher = lehrkraefte[l].Kuerzel.Cleanup(),
                                Type = "primary"
                            });
                        }
                    }
                }).ConfigureAwait(false);

                BusyText = "Importe ins ICC";
                var response = await importer.ImportGradeTeachersAsync(data, (int)abschnitt.Abschnitt, (int)abschnitt.Schuljahr);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                dialogHelper.ShowAsync(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim Klassenleitungs-Import",
                        Content = "Beim Import der Klassenleitungen ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }
    }
}
