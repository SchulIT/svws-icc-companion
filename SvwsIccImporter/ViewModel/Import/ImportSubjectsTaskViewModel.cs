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
    public class ImportSubjectsTaskViewModel : ImportTaskViewModelBase
    {

        public ImportSubjectsTaskViewModel(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager)
        {

        }

        public override async Task<bool> RunAsync(Schuljahresabschnitt abschnitt)
        {

            try
            {
                CurrentStatus = TaskStatus.Running;
                BusyText = "Lade Fächer aus SVWS-Datenbank";

                var client = restClientFactory.GetClient();
                var subjects = await client.GetFaecherAsync(settingsManager.Settings.Svws.Schema);

                if(settingsManager.Settings.Svws.OnlyVisible)
                {
                    subjects = subjects.Where(x => x.IstSichtbar).ToList();
                }

                BusyText = $"Transformiere {subjects.Count} Fächer für den ICC-Import";
                var data = await Task.Run(() =>
                {
                    return subjects.Select(f =>
                    {
                        return new SubjectData
                        {
                            Id = f.Id.ToString(),
                            Abbreviation = f.Kuerzel.Cleanup(),
                            Name = f.Bezeichnung.Cleanup()
                        };
                    })
                    .Where(x => !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Abbreviation))
                    .ToList();
                }).ConfigureAwait(false);

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportSubjectsAsync(data);

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
