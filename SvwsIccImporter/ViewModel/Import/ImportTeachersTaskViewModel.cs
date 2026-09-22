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
    [Priority(10)]
    public class ImportTeachersTaskViewModel : ImportTaskViewModelBase
    {

        public ImportTeachersTaskViewModel(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager)
        {

        }

        public override async Task<bool> RunAsync(Schuljahresabschnitt abschnitt)
        {
            try
            {
                CurrentStatus = TaskStatus.Running;
                var client = restClientFactory.GetClient();

                BusyText = "Lade Fächer aus der SVWS-Datenbank";
                var faecher = (await client.GetFaecherAsync(settingsManager.Settings.Svws.Schema)).ToDictionary(x => x.Kuerzel);

                BusyText = "Lade Lehrbefähigungen aus SVWS-Datenbank";
                var lehrbefaehigungen = (await client.GetKatalogLehrerLehrbefaehigungenAsync(settingsManager.Settings.Svws.Schema)).ToDictionary(x => x.Id);

                BusyText = "Lade Lehrkräfte aus SVWS-Datenbank";
                var teachers = await client.GetLehrerAsync(settingsManager.Settings.Svws.Schema);

                if(settingsManager.Settings.Svws.OnlyVisible)
                {
                    teachers = teachers.Where(x => x.IstSichtbar).ToList();
                }

                BusyText = $"Transformiere {teachers.Count} Lehrkräfte für den ICC-Import";

                var iccTeachers = new List<TeacherData>();
                var idx = 0;

                foreach (var teacher in teachers)
                {
                    idx++;

                    BusyText = $"Lade Personaldaten für {teacher.Kuerzel} ({idx}/{teachers.Count})";
                    var personaldaten = await client.GetLehrerPersonaldatenAsync(settingsManager.Settings.Svws.Schema, teacher.Id);

                    if(!personaldaten.Abschnittsdaten.Any(x => x.IdSchuljahresabschnitt == abschnitt.Id))
                    {
                        // Lehrkraft ist dem aktuellen Abschnitt nicht zugeordnet
                        continue;
                    }

                    BusyText = $"Lade Stammdaten für {teacher.Kuerzel} ({idx}/{teachers.Count})";
                    var stammdaten = await client.GetLehrerStammdatenAsync(settingsManager.Settings.Svws.Schema, teacher.Id);

                    var subjects = new List<string>();

                    foreach(var l in personaldaten.Lehraemter)
                    {
                        foreach (var b in l.Lehrbefaehigungen)
                        {
                            if (!lehrbefaehigungen.ContainsKey(b.IdLehrbefaehigung))
                            {
                                continue;
                            }

                            var kuerzel = lehrbefaehigungen[b.IdLehrbefaehigung].Kuerzel;

                            if (!faecher.ContainsKey(kuerzel))
                            {
                                continue;
                            }

                            subjects.Add(faecher[kuerzel].Id.ToString());
                        }
                    }

                    iccTeachers.Add(new TeacherData
                    {
                        Id = stammdaten.Id.ToString(),
                        Acronym = stammdaten.Kuerzel.Cleanup(),
                        Firstname = stammdaten.Vorname.Cleanup(),
                        Lastname = stammdaten.Nachname.Cleanup(),
                        Email = stammdaten.EmailDienstlich.Cleanup(),
                        Gender = stammdaten.Geschlecht.ToIccGender(),
                        Title = stammdaten.Titel?.Cleanup(),
                        Subjects = subjects.Distinct().ToList(),
                        Birthday = stammdaten.Geburtsdatum.ToDateTime(),
                    });
                }

                iccTeachers = iccTeachers
                    .Where(x => !string.IsNullOrEmpty(x.Acronym) && !string.IsNullOrEmpty(x.Firstname) && !string.IsNullOrEmpty(x.Lastname) && !string.IsNullOrEmpty(x.Email))
                    .ToList();

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportTeachersAsync(iccTeachers, (int)abschnitt.Abschnitt, (int)abschnitt.Schuljahr);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                dialogHelper.ShowAsync(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim Lehrkräfte-Import",
                        Content = "Beim Import der Lehrkräfte ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }
    }
}
