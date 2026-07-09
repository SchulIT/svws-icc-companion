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
    public class ImportStudyGroupsTaskViewModel : ImportTaskViewModelBase, IResetViewModel
    {

        private readonly ICachedUnterrichtResolver unterrichtResolver;
        private readonly INameResolver nameResolver;
        private readonly IIdResolver idResolver;

        public ImportStudyGroupsTaskViewModel(ICachedUnterrichtResolver unterrichtResolver, INameResolver nameResolver, IIdResolver idResolver, IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
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
                        .ToList();

                    // no need to filter for teachers as they are not relevant in this import
                }

                BusyText = $"Transformiere Lerngruppen für den ICC-Import";
                var studyGroups = await Task.Run(() =>
                {
                    return tuitions
                        .Select(x => new StudyGroupData
                        {
                            Id = idResolver.ResolveIdForStudyGroup(x).Cleanup(),
                            Name = nameResolver.ResolveName(x).Cleanup(),
                            Type = x.KursId == null ? "grade" : "course",
                            Grades = x.Klassen
                                .Select(x => x.Name.Cleanup())
                                .ToList()

                        })
                        .Where(x => !string.IsNullOrEmpty(x.Name))
                        .DistinctBy(x => x.Id)
                        .ToList();
                }).ConfigureAwait(false);

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportStudyGroupsAsync(studyGroups, (int)abschnitt.Abschnitt, (int)abschnitt.Schuljahr);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                dialogHelper.Show(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim Lerngruppen-Import",
                        Content = "Beim Import der Lerngruppen ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }
    }
}
