using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SvwsIccImporter.Service;
using SvwsIccImporter.Settings;
using System;
using System.Threading.Tasks;

namespace SvwsIccImporter.ViewModel
{
    public partial class SvwsViewModel : ObservableRecipient
    {
        public int MinMajorVersion = 1;
        public int MinMinorVersion = 5;
        public int MinPatchVersion = 0;
        
        #region Properties

        [ObservableProperty]
        private string schoolName = "nicht verbunden";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSupportedVersion))] 
        [NotifyPropertyChangedFor(nameof(UnsupportedServerVersionMessage))] 
        private string serverVersion = string.Empty;

        public bool IsSupportedVersion
        {
            get
            {
                if (ServerVersion == string.Empty)
                {
                    return false;
                }

                Version version;
                if (!Version.TryParse(ServerVersion, out version))
                {
                    return false;
                }

                if (version.Major != MinMajorVersion)
                {
                    return false;
                }

                if (version.Minor != MinMinorVersion)
                {
                    return false;
                }

                if (version.Build < MinPatchVersion)
                {
                    return false;
                }

                return true;
            }
        }

        public string UnsupportedServerVersionMessage
        {
            get
            {
                return
                    $"Die Server-Version {ServerVersion} wurde nicht mit dieser Version des Import-Tools getestet. Da die SVWS-API nicht semantisch versioniert ist, muss davon ausgegangen werden, dass API-Endpunkte geändert oder gelöscht wurden. Bitte eine aktuellere Version des SVWS ICC Importers herunterladen.";
            }
        }

        #endregion

        #region Commands
        public AsyncRelayCommand LoadSchoolCommand { get; private set; }
        #endregion

        #region Services

        private readonly IRestClientFactory restClientFactory;
        private readonly ISettingsManager settingsManager;

        #endregion

        public SvwsViewModel(IRestClientFactory restClientFactory, ISettingsManager settingsManager)
        {
            this.restClientFactory = restClientFactory;
            this.settingsManager = settingsManager;
            LoadSchoolCommand = new AsyncRelayCommand(LoadSchoolInformation);

            this.settingsManager.SettingsSaved += OnSettingsSaved;
        }

        private async void OnSettingsSaved(ISettingsManager manager, SettingsSavedEventArgs args)
        {
            await LoadSchoolInformation();
        }

        public async Task LoadSchoolInformation()
        {
            if (string.IsNullOrEmpty(settingsManager.Settings.Svws.Schema))
            {
                return;
            }

            try
            {
                var client = restClientFactory.GetClient();
                var stammdaten = await client.GetSchuleStammdatenAsync(settingsManager.Settings.Svws.Schema);
                SchoolName = string.IsNullOrEmpty(stammdaten.Bezeichnung1?.Trim()) ? "nicht verbunden" : stammdaten.Bezeichnung1.Trim();

                var version = await client.GetServerVersionAsync();
                ServerVersion = version;
            }
            catch (Exception ex)
            {
                SchoolName = "Fehler";
                ServerVersion = string.Empty;
            }
        }
    }
}
