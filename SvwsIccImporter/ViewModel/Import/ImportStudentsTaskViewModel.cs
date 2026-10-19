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
    [Priority(20)]
    public partial class ImportStudentsTaskViewModel : ImportTaskViewModelBase
    {

        public ImportStudentsTaskViewModel(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
            : base(dialogHelper, restClientFactory, importer, settingsManager, "students", "Lernende")
        {
            
        }

        public override async Task<bool> RunAsync(Schuljahresabschnitt abschnitt)
        {
            try
            {
                CurrentStatus = TaskStatus.Running;
                BusyText = "Lade Lernende aus SVWS-Datenbank";

                var client = restClientFactory.GetClient();
                var schuelerListe = await client.GetSchuelerFuerAbschnittAsync(settingsManager.Settings.Svws.Schema, abschnitt.Id);

                BusyText = $"Filtere {schuelerListe.Count} Kind(er) auf den aktuellen Abschnitt";
                var schuelerListeAktuellerAbschnitt = schuelerListe.Where(x => x.IdSchuljahresabschnitt == abschnitt.Id).ToArray();
                var schuelerCount = schuelerListeAktuellerAbschnitt.Count();
                
                var schuelerStammdatenRequests = new List<Func<Task<SchuelerStammdaten>>>();
                var schuelerEinwilligungenRequests = new List<Func<Task<ICollection<SchuelerEinwilligung>>>>();

                foreach (var schueler in schuelerListeAktuellerAbschnitt)
                {
                    schuelerStammdatenRequests.Add(() => client.GetSchuelerStammdatenAsync(settingsManager.Settings.Svws.Schema, schueler.Id));
                    schuelerEinwilligungenRequests.Add(() => client.GetSchuelerEinwilligungenAsync(settingsManager.Settings.Svws.Schema, schueler.Id));
                }
                
                BusyText = $"Hole Stammdaten für {schuelerCount} Kind(er)";
                var schuelerStammdatenListe = await BatchRequest.SendRequestAsync(schuelerStammdatenRequests);
                
                BusyText = $"Hole Datenschutzeinwilligungen für {schuelerCount} Kind(er)";
                var einwilligungen = await BatchRequest.SendRequestAsync(schuelerEinwilligungenRequests);
                var einwilligungenUnfolded = einwilligungen.SelectMany(x => x).ToList();
                
                var students = new List<StudentData>();

                foreach (var stammdaten in schuelerStammdatenListe)
                {
                    BusyText = $"Hole Datenschutzeinwilligungen für {stammdaten.Nachname}, {stammdaten.Vorname}";
                    var datenschutzEinwilligungen = einwilligungenUnfolded.Where(x => x.IdSchueler == stammdaten.Id).ToList();
                    
                    students.Add(new StudentData
                    {
                        Id = stammdaten.Id.ToString(),
                        Firstname = stammdaten.Vorname.Cleanup(),
                        Lastname = stammdaten.Nachname.Cleanup(),
                        Email = stammdaten.EmailSchule.Cleanup(),
                        Gender = stammdaten.Geschlecht.ToIccGender(),
                        Status = GetStatus(stammdaten.Status),
                        Birthday = stammdaten.Geburtsdatum.ToDateTime(),
                        ApprovedPrivacyCategories = datenschutzEinwilligungen.Where(x => x.Status == true).Select(x => x.IdEinwilligungsart.ToString()).ToList()
                    });
                }

                students = students
                    .Where(x => !string.IsNullOrEmpty(x.Firstname) && !string.IsNullOrEmpty(x.Lastname) && !string.IsNullOrEmpty(x.Email) && x.Birthday != null)
                    .ToList();

                BusyText = "Importiere ins ICC";
                var response = await importer.ImportStudentsAsync(students, (int)abschnitt.Abschnitt, (int)abschnitt.Schuljahr);

                return HandleResponse(response, dialogHelper);
            }
            catch (Exception ex)
            {
                await dialogHelper.ShowAsync(
                    new ErrorDialog
                    {
                        Title = "Fehler",
                        Header = "Fehler beim SuS-Import",
                        Content = "Beim Import der Lernenden ist ein Fehler aufgetreten",
                        Exception = ex
                    });

                CurrentStatus = TaskStatus.Failure;
                return false;
            }
        }

        private string GetStatus(int status)
        {
            return status switch
            {
                0 => "Neuaufnahme",
                1 => "Warteliste",
                2 => "Aktive",
                3 => "Beurlaubt",
                6 => "Externe",
                8 => "Abitur",
                9 => "Abgaenger",
                _ => "Unbekannt"
            };
        }
    }
}
