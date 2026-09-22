using SchulIT.IccImport;
using SvwsIccImporter.Service;
using SvwsIccImporter.Settings;
using SvwsIccImporter.UI.Dialog;
using System.Linq;
using System;
using System.Threading.Tasks;
using SchulIT.IccImport.Models;
using SvwsIccImporter.Extensions;
using SchulIT.SvwsClient;

namespace SvwsIccImporter.ViewModel.Import
{
    [Priority(30)]
    public class ImportTuitionsTaskViewModel : ImportTaskViewModelBase, IResetViewModel
    {
        private readonly ICachedUnterrichtResolver unterrichtResolver;
        private readonly INameResolver nameResolver;
        private readonly IIdResolver idResolver;


        public ImportTuitionsTaskViewModel(ICachedUnterrichtResolver unterrichtResolver, INameResolver nameResolver, IIdResolver idResolver, IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager)
        {
            this.unterrichtResolver = unterrichtResolver;
            this.nameResolver = nameResolver;
            this.idResolver = idResolver;
        }

        public void Reset()
        {
            unterrichtResolver.ClearCache();
        }

        public override async Task<bool> RunAsync(Schuljahresabschnitt abschnitt)
        {
            try
            {
                BusyText = "Rekonstruiere Unterrichte aus SVWS-Datenbank";
                var tuitions = await unterrichtResolver.ResolveCachedAsync(restClientFactory.GetClient(), settingsManager.Settings.Svws.Schema, abschnitt.Id);

                if (settingsManager.Settings.Svws.OnlyVisible)
                {
                    tuitions = tuitions
                        .Where(x => x.IstSichtbar)
                        .Select(x =>
                        { 
                            x.Lehrkraefte = x.Lehrkraefte.Where(x => x.IstSichtbar).ToList();
                            return x;
                        })
                        .ToList();
                }

                BusyText = $"Transformiere Unterrichte für den ICC-Import";
                var data = await Task.Run(() =>
                {
                    return tuitions
                        .Select(x => new TuitionData
                        {
                            Id = idResolver.ResolveIdForTuition(x).Cleanup(),
                            Name = nameResolver.ResolveName(x).Cleanup(),
                            StudyGroup = idResolver.ResolveIdForStudyGroup(x).Cleanup(),
                            Teachers = x.Lehrkraefte.Select(x => x.Kuerzel.Cleanup()).Where(x => !string.IsNullOrEmpty(x)).ToList(),
                            Subject = x.Fach.Kuerzel.Cleanup()
                        })
                        .Where(x => !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Subject) && !string.IsNullOrEmpty(x.StudyGroup))
                        .DistinctBy(x => x.Id)
                        .ToList();
                }).ConfigureAwait(false);

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportTuitionsAsync(data, (int)abschnitt.Abschnitt, (int)abschnitt.Schuljahr);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                dialogHelper.ShowAsync(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim Unterrichte-Import",
                        Content = "Beim Import der Unterrichte ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }
    }
}
