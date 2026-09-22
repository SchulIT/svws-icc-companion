using CommunityToolkit.Mvvm.ComponentModel;
using SvwsIccImporter.Service;
using SvwsIccImporter.Settings;
using SvwsIccImporter.UI;
using SvwsIccImporter.UI.Dialog;
using System;
using System.Threading.Tasks;

namespace SvwsIccImporter.ViewModel
{
    public partial class SplashScreenViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string progressText = string.Empty;

        #region Services

        private readonly IRestClientFactory clientFactory;
        private readonly ISettingsManager settingsManager;
        private readonly IDialogHelper dialogHelper;
        private readonly IWindowManager windowManager;

        #endregion

        public SplashScreenViewModel(IRestClientFactory clientFactory, ISettingsManager settingsManager, IDialogHelper dialogHelper, IWindowManager windowManager)
        {
            this.clientFactory = clientFactory;
            this.settingsManager = settingsManager;
            this.dialogHelper = dialogHelper;
            this.windowManager = windowManager;
        }

        public async Task Initialize()
        {
            try
            {
                ProgressText = "Einstellungen lesen...";
                await settingsManager.LoadSettingsAsync();

                if (IsSettingsValid())
                {
                    ProgressText = "Verbinde zur SVWS-Datenbank...";
                    var client = clientFactory.GetClient();
                    await client.GetSchuleStammdatenAsync(settingsManager.Settings.Svws.Schema);
                }
                else
                {
                    await dialogHelper.ShowAsync(new Dialog
                    {
                        Icon = Icon.ShieldWarningYellowBar,
                        Title = "Einstellungen unvollständig",
                        Header = "Einstellungen unvollständig",
                        Content = "Die Einstellungen sind unvollständig. Bitte zunächst auf der Einstellungen-Seite alle relevanten Einstellungen vornehmen."
                    });
                }
            } catch(Exception ex)
            {
                await dialogHelper.ShowAsync(new ErrorDialog
                {
                    Title = "Fehler",
                    Header = "Fehler beim Abrufen",
                    Content = "Beim Laden ist ein Fehler aufgetreten. Bitte auf der Einstellungen-Seite alle Einstellungen prüfen und sicherstellen, dass der Server erreichbar ist.",
                    Exception = ex
                });
            }
        }

        private bool IsSettingsValid()
        {
            return !string.IsNullOrEmpty(settingsManager.Settings?.Icc.Endpoint)
                && !string.IsNullOrEmpty(settingsManager.Settings?.Icc.Token)
                && !string.IsNullOrEmpty(settingsManager.Settings?.Svws.Server)
                && !string.IsNullOrEmpty(settingsManager.Settings?.Svws.Schema)
                && !string.IsNullOrEmpty(settingsManager.Settings?.Svws.Username);
        }
    }
}
