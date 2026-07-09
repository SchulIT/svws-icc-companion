using SchulIT.IccImport;
using SchulIT.IccImport.Models;
using SchulIT.SvwsClient;
using SvwsIccImporter.Extensions;
using SvwsIccImporter.Service;
using SvwsIccImporter.Settings;
using SvwsIccImporter.UI.Dialog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SvwsIccImporter.ViewModel.Import
{
    [Priority(0)]
    public class ImportLearningManagementSystemsTaskViewModel : ImportTaskViewModelBase
    {
        public ImportLearningManagementSystemsTaskViewModel(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager)
        {

        }

        public override async Task<bool> RunAsync(Schuljahresabschnitt abschnitt)
        {
            try
            {
                CurrentStatus = TaskStatus.Running;
                BusyText = "Lade Lernplattformen aus SVWS-Datenbank";

                var client = restClientFactory.GetClient();
                var lernplattformListe = await client.GetLernplattformenAsync(settingsManager.Settings.Svws.Schema);

                BusyText = $"Transformiere {lernplattformListe.Count} Lernplattformen für den ICC-Import";
                var data = await Task.Run(() =>
                {
                    return lernplattformListe.Select(x =>
                    {
                        return new LearningManagementSystemData
                        {
                            Id = x.Id.ToString(),
                            Name = x.Bezeichnung.Cleanup()
                        };
                    })
                    .Where(x => !string.IsNullOrEmpty(x.Name))
                    .ToList();
                }).ConfigureAwait(false);

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportLearningManagementSystemsAsync(data);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                dialogHelper.Show(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim Lernplattformen-Import",
                        Content = "Beim Import der Lernplattformen ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }
    }
}
