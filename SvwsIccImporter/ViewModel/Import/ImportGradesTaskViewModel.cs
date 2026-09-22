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
    public class ImportGradesTaskViewModel : ImportTaskViewModelBase
    {

        public ImportGradesTaskViewModel(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager)
        {

        }

        public override async Task<bool> RunAsync(Schuljahresabschnitt abschnitt)
        {
            try
            {
                CurrentStatus = TaskStatus.Running;
                BusyText = "Lade Klassen aus SVWS-Datenbank";

                var client = restClientFactory.GetClient();
                var klassenListe = await client.GetKlassenFuerAbschnittAsync(settingsManager.Settings.Svws.Schema, abschnitt.Id);

                BusyText = $"Transformiere {klassenListe.Count} für den ICC-Import";

                var data = await Task.Run(() =>
                {
                    return klassenListe.Select(g =>
                    { 
                        return new GradeData
                        {
                            Id = g.Kuerzel.Cleanup(),
                            Name = g.Kuerzel.Cleanup(),
                        };
                    })
                    .Where(x => !string.IsNullOrEmpty(x.Id) && !string.IsNullOrEmpty(x.Name))
                    .ToList();
                }).ConfigureAwait(false);

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportGradesAsync(data);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                dialogHelper.ShowAsync(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim Klassen-Import",
                        Content = "Beim Import der Klassen ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }
    }
}
