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
    [Priority(0)]
    public class ImportStudyGroupsTaskViewModel : ImportTaskViewModelBase, IResetViewModel
    {

        private readonly ICachedUnterrichtResolver unterrichtResolver;
        private readonly INameResolver nameResolver;
        private readonly IIdResolver idResolver;

        public ImportStudyGroupsTaskViewModel(ICachedUnterrichtResolver unterrichtResolver, INameResolver nameResolver, IIdResolver idResolver, IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager, "studygroups", "Lerngruppen")
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

                BusyText = "Lade Klassen aus SVWS-Datenbank";
                var grades = await restClientFactory.GetClient().GetListKlassenListeEintragBySchuljahresabschnittAsync(settingsManager.Settings.Svws.Schema, abschnitt.Id);
                

                BusyText = $"Transformiere Lerngruppen für den ICC-Import";
                var studyGroups = new List<StudyGroupData>();
                
                // KLASSEN
                foreach (var grade in grades.Where(x => x.AnzahlZugeordneteSchueler > 0))
                {
                    studyGroups.Add(new StudyGroupData
                    {
                        Id = grade.Kuerzel.Cleanup(),
                        Name = grade.Kuerzel.Cleanup(),
                        Type = "grade",
                        Grades = new List<string>{ grade.Kuerzel.Cleanup() }
                    });
                }

                // KURSE
                foreach (var tuition in tuitions.Where(x => x.KursId != null))
                {
                    studyGroups.Add(new StudyGroupData
                    {
                        Id = idResolver.ResolveIdForStudyGroup(tuition).Cleanup(),
                        Name = nameResolver.ResolveName(tuition).Cleanup(),
                        Type = "course",
                        Grades = tuition.Klassen
                            .Select(x => x.Name.Cleanup())
                            .ToList()
                    });
                }

                studyGroups = studyGroups
                    .Where(x => !string.IsNullOrEmpty(x.Name))
                    .DistinctBy(x => x.Id)
                    .ToList();

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportStudyGroupsAsync(studyGroups, (int)abschnitt.Abschnitt, (int)abschnitt.Schuljahr);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                await dialogHelper.ShowAsync(
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
