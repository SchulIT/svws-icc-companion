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
using SvwsClient.Tool;

namespace SvwsIccImporter.ViewModel.Import
{
    [Priority(10)]
    public class ImportTeachersTaskViewModel : ImportTaskViewModelBase
    {

        public ImportTeachersTaskViewModel(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager, "teachers", "Lehrkräfte")
        {
            
        }

        public override async Task<bool> RunAsync(Schuljahresabschnitt abschnitt)
        {
            try
            {
                CurrentStatus = TaskStatus.Running;
                var client = restClientFactory.GetClient();

                BusyText = "Lade Lehrkräfte aus der SVWS-Datenbank";
                var faecher = (await client.GetFaecherAsync(settingsManager.Settings.Svws.Schema)).ToDictionary(x => x.Kuerzel);

                BusyText = "Lade Lehrbefähigungen aus SVWS-Datenbank";
                var lehrbefaehigungen = (await client.GetKatalogLehrerLehrbefaehigungenAsync(settingsManager.Settings.Svws.Schema)).ToDictionary(x => x.Id);

                BusyText = "Lade Lehrkräfte aus SVWS-Datenbank";
                var teachers = await client.GetLehrerAsync(settingsManager.Settings.Svws.Schema);

                if(settingsManager.Settings.Svws.OnlyVisible)
                {
                    teachers = teachers.Where(x => x.IstSichtbar).ToList();
                }

                var personaldatenRequests = new List<Func<Task<LehrerPersonaldaten>>>();
                var stammdatenRequests = new List<Func<Task<LehrerStammdaten>>>();

                foreach (var teacher in teachers)
                {
                    personaldatenRequests.Add(() => client.GetLehrerPersonaldatenAsync(settingsManager.Settings.Svws.Schema, teacher.Id));
                    stammdatenRequests.Add(() => client.GetLehrerStammdatenAsync(settingsManager.Settings.Svws.Schema, teacher.Id));
                }

                BusyText = $"Lade Personaldaten für {teachers.Count} Lehrkräfte";
                var personaldatenListe = await BatchRequest.SendRequestAsync(personaldatenRequests);

                BusyText = $"Lade Stammdaten für {teachers.Count} Lehrkräfte";
                var stammdatenListe = await BatchRequest.SendRequestAsync(stammdatenRequests);
                
                
                BusyText = $"Transformiere {teachers.Count} Lehrkräfte für den ICC-Import";
                var iccTeachers = new List<TeacherData>();
                var idx = 0;

                foreach (var teacher in teachers)
                {
                    idx++;

                    BusyText = $"Lade Personaldaten für {teacher.Kuerzel} ({idx}/{teachers.Count})";
                    var personaldaten = personaldatenListe.FirstOrDefault(x => x.Id == teacher.Id);

                    if (personaldaten == null)
                    {
                        continue;
                    }
                    
                    if(!personaldaten.Abschnittsdaten.Any(x => x.IdSchuljahresabschnitt == abschnitt.Id))
                    {
                        // Lehrkraft ist dem aktuellen Abschnitt nicht zugeordnet
                        continue;
                    }

                    BusyText = $"Lade Stammdaten für {teacher.Kuerzel} ({idx}/{teachers.Count})";
                    var stammdaten = stammdatenListe.FirstOrDefault(x => x.Id == teacher.Id);

                    if (stammdaten == null)
                    {
                        continue;
                    }
                    
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
                await dialogHelper.ShowAsync(
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
