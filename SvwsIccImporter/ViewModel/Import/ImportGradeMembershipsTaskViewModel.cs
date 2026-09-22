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
    [Priority(21)]
    public class ImportGradeMembershipsTaskViewModel : ImportTaskViewModelBase
    {

        public ImportGradeMembershipsTaskViewModel(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager)
        {

        }

        public override async Task<bool> RunAsync(Schuljahresabschnitt abschnitt)
        {
            try
            {
                CurrentStatus = TaskStatus.Running;
                BusyText = "Lade Lernende aus der SVWS-Datenbank";

                var client = restClientFactory.GetClient();
                var klassenListe = await client.GetKlassenFuerAbschnittAsync(settingsManager.Settings.Svws.Schema, abschnitt.Id);

                BusyText = $"Transformiere Klassenmitgliedschaften für den ICC-Import";
                var data = new List<GradeMembershipData>();

                await Task.Run(() =>
                {
                    foreach (var klasse in klassenListe)
                    {
                        foreach (var kind in klasse.Schueler)
                        {
                            data.Add(new GradeMembershipData
                            {
                                Grade = klasse.Kuerzel.Cleanup(),
                                Student = kind.Id.ToString()
                            });
                        }
                    }
                }).ConfigureAwait(false);

                BusyText = "Importe ins ICC";
                var response = await importer.ImportGradeMembershipsAsync(data, (int)abschnitt.Abschnitt, (int)abschnitt.Schuljahr);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                dialogHelper.ShowAsync(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim Klassenmitgliedschaften-Import",
                        Content = "Beim Import der Klassenmitgliedschaften ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }
    }
}
