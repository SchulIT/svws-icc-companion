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
    [Priority(21)]
    public class ImportStudyGroupMembershipTaskViewModel : ImportTaskViewModelBase, IResetViewModel
    {
        private readonly ICachedUnterrichtResolver unterrichtResolver;
        private readonly INameResolver nameResolver;
        private readonly IIdResolver idResolver;

        public ImportStudyGroupMembershipTaskViewModel(ICachedUnterrichtResolver unterrichtResolver, INameResolver nameResolver, IIdResolver idResolver, IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
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

                if(settingsManager.Settings.Svws.OnlyVisible)
                {
                    tuitions = tuitions
                        .Where(x => x.IstSichtbar)
                        .ToList();

                    // no need to filter for teachers as they are not relevant in this import
                }

                BusyText = $"Transformiere Unterrichte für den ICC-Import";

                var memberships = await Task.Run(() =>
                {
                    return tuitions
                        .SelectMany(t =>
                        {
                            return t.Kinder.Select(x =>
                            {
                                return new StudyGroupMembershipData
                                {
                                    Student = x.KindId.ToString(),
                                    StudyGroup = idResolver.ResolveIdForStudyGroup(t),
                                    Type = x.Art.Cleanup()
                                };
                            });
                        })
                        .Where(x => !string.IsNullOrEmpty(x.Student) && !string.IsNullOrEmpty(x.StudyGroup))
                        .DistinctBy(x => string.Join('-', x.Student, x.StudyGroup))
                        .ToList();
                }).ConfigureAwait(false);

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportStudyGroupMembershipsAsync(memberships, (int)abschnitt.Abschnitt, (int)abschnitt.Schuljahr);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                dialogHelper.Show(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim Lerngruppen-Mitgliedschaften-Import",
                        Content = "Beim Import der Lerngruppen-Mitgliedschaften ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }
    }
}
