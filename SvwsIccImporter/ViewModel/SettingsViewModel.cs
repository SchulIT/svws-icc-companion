using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SvwsIccImporter.Service;
using SvwsIccImporter.Settings;
using SvwsIccImporter.UI.Dialog;
using SvwsIccImporter.ViewModel.Form;
using System;
using System.Threading.Tasks;

namespace SvwsIccImporter.ViewModel
{
    public partial class SettingsViewModel : ObservableRecipient
    {
        #region Properties

        [ObservableProperty]
        private string busyText = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(TestConnectionCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private bool isBusy = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private bool isSuccessfulConfig = false;

        public SettingsForm Settings { get; } = new SettingsForm();

        #endregion

        #region Commands
        public AsyncRelayCommand TestConnectionCommand { get; private set; }

        public AsyncRelayCommand SaveSettingsCommand { get; private set; }

        #endregion

        #region Services

        private readonly ISettingsManager settingsManager;
        private readonly IDialogHelper dialogHelper;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IRestClientFactory restClientFactory;

        #endregion

        public SettingsViewModel(ISettingsManager settingsManager, IDialogHelper dialogHelper, IHttpClientFactory httpClientFactory, IRestClientFactory restClientFactory)
        {
            this.settingsManager = settingsManager;
            this.dialogHelper = dialogHelper;
            this.httpClientFactory = httpClientFactory;
            this.restClientFactory = restClientFactory;

            TestConnectionCommand = new AsyncRelayCommand(TestConnectionAsync, CanTest);
            SaveSettingsCommand = new AsyncRelayCommand(SaveAsync, CanSave);

            Settings.PropertyChanged += delegate
            {
                IsSuccessfulConfig = false;
            };

            Settings.ErrorsChanged += delegate
            {
                TestConnectionCommand?.NotifyCanExecuteChanged();
                SaveSettingsCommand?.NotifyCanExecuteChanged();
            };
        }

        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;
                BusyText = "Einstellungen speichern";

                var settings = settingsManager.Settings;
                settings.Svws.Server = Settings.SvwsServer;
                settings.Svws.Schema = Settings.SvwsSchema;
                settings.Svws.Username = Settings.SvwsUsername;
                settings.Svws.Password = Settings.SvwsPassword;
                settings.Svws.OnlyVisible = Settings.OnlyVisible;
                settings.Svws.IgnoreCertificateWarnings = Settings.IgnoreCertificateWarnings;

                settings.Icc.Endpoint = Settings.IccEndpoint;
                settings.Icc.Token = Settings.IccToken;

                await settingsManager.SaveSettingsAsync();
            }
            catch (Exception ex)
            {
                dialogHelper.ShowAsync(new ErrorDialog
                {
                    Title = "Fehler",
                    Header = "Fehler beim Speichern",
                    Content = "Die Einstellungen-Datei konnte nicht gespeichert werden.",
                    Exception = ex
                });
            }
            finally
            {
                IsBusy = false;
                BusyText = string.Empty;
            }
        }

        private bool CanSave() => !IsBusy && IsSuccessfulConfig && Settings.HasErrors == false;

        private async Task TestConnectionAsync()
        {
            try
            {
                IsBusy = true;
                BusyText = "Verbinde zur SVWS-Datenbank";

                using (var httpClient = httpClientFactory.CreateHttpClient(Settings.SvwsUsername, Settings.SvwsPassword, Settings.IgnoreCertificateWarnings))
                {
                    var client = restClientFactory.GetClient(Settings.SvwsServer, httpClient);
                    try
                    {
                        var stammdaten = await client.GetSchuleStammdatenAsync(Settings.SvwsSchema);

                        dialogHelper.ShowAsync(new Dialog
                        {
                            Icon = Icon.ShieldSuccessGreenBar,
                            Title = "Erfolg",
                            Header = "Verbindungstest erfolgreich",
                            Content = "Verbindung zur SVWS-Datenbank hergestellt (" + stammdaten.Bezeichnung1 + ")"
                        });
                        IsSuccessfulConfig = true;
                    }
                    catch (Exception e)
                    {
                        dialogHelper.ShowAsync(new ErrorDialog
                        {
                            Title = "Fehler",
                            Header = "Verbindungsfehler",
                            Content = "Es konnte keine Verbindung zur Datenbank hergestellt werden.",
                            Exception = e
                        });
                    }
                }
            }
            finally
            {
                IsBusy = false;
                BusyText = string.Empty;
            }
        }

        private bool CanTest() => !IsBusy && Settings.HasErrors == false;

        public void LoadSettings()
        {
            if(settingsManager.Settings == null)
            {
                return;
            }

            Settings.IccEndpoint = settingsManager.Settings.Icc.Endpoint;
            Settings.IccToken = settingsManager.Settings.Icc.Token;

            Settings.SvwsServer = settingsManager.Settings.Svws.Server;
            Settings.SvwsSchema = settingsManager.Settings.Svws.Schema;
            Settings.SvwsUsername = settingsManager.Settings.Svws.Username;
            Settings.SvwsPassword = settingsManager.Settings.Svws.Password;
            Settings.OnlyVisible = settingsManager.Settings.Svws.OnlyVisible;
            Settings.IgnoreCertificateWarnings = settingsManager.Settings.Svws.IgnoreCertificateWarnings;
        }
    }
}
