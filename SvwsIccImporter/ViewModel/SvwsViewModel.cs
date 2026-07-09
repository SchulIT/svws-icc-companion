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
        #region Properties

        [ObservableProperty]
        private string schoolName = "nicht verbunden";

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
            }
            catch (Exception ex)
            {
                SchoolName = "Fehler";
            }
        }
    }
}
