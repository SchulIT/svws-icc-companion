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
using System.Xml;

namespace SvwsIccImporter.ViewModel.Import
{
    [Priority(0)]
    public class ImportPrivacyCategoriesTaskViewModel : ImportTaskViewModelBase
    {

        public ImportPrivacyCategoriesTaskViewModel(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager)
        {

        }

        public override async Task<bool> RunAsync(Schuljahresabschnitt abschnitt)
        {
            try
            {
                CurrentStatus = TaskStatus.Running;
                BusyText = "Lade Datenschutzkategorien aus SVWS-Datenbank";

                var client = restClientFactory.GetClient();
                var kategorienListe = await client.GetEinwilligungsartenAsync(settingsManager.Settings.Svws.Schema);

                if(settingsManager.Settings.Svws.OnlyVisible)
                {
                    kategorienListe = kategorienListe.Where(x => x.IstSichtbar).ToList();
                }

                BusyText = $"Transformiere {kategorienListe.Count} für den ICC-Import";
                var data = await Task.Run(() =>
                {
                    return kategorienListe.Where(x => x.IstSichtbar).Select(c =>
                    {
                        return new PrivacyCategoryData
                        {
                            Id = c.Id.ToString(),
                            Label = c.Bezeichnung.Cleanup(),
                            Description = c.Beschreibung.Cleanup()
                        };
                    })
                    .Where(x => !string.IsNullOrEmpty(x.Label))
                    .ToList();
                }).ConfigureAwait(false);

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportPrivacyCategoriesAsync(data);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                dialogHelper.ShowAsync(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim Datenschutzkategorien-Import",
                        Content = "Beim Import der Datenschutzkategorien ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }
    }
}
