using CommunityToolkit.Mvvm.ComponentModel;
using SchulIT.IccImport;
using SchulIT.IccImport.Response;
using SchulIT.SvwsClient;
using SvwsIccImporter.Service;
using SvwsIccImporter.Settings;
using SvwsIccImporter.UI.Dialog;
using System.Threading.Tasks;

namespace SvwsIccImporter.ViewModel.Import
{
    public abstract partial class ImportTaskViewModelBase : ObservableRecipient, IImportTaskViewModel
    { 
        [ObservableProperty]
        private TaskStatus currentStatus = TaskStatus.None;

        [ObservableProperty]
        private string busyText = string.Empty;

        [ObservableProperty]
        private bool isEnabled = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        protected readonly IDialogHelper dialogHelper;
        protected readonly IRestClientFactory restClientFactory;
        protected readonly IIccImporter importer;
        protected readonly ISettingsManager settingsManager;

        public ImportTaskViewModelBase(IDialogHelper dialogHelper, IRestClientFactory restClientFactory, IIccImporter importer, ISettingsManager settingsManager)
        {
            this.dialogHelper = dialogHelper;
            this.restClientFactory = restClientFactory;
            this.importer = importer;
            this.settingsManager = settingsManager;
        }

        public abstract Task<bool> RunAsync(Schuljahresabschnitt abschnitt);

        protected bool HandleResponse(IResponse response, IDialogHelper dialogHelper)
        {
            if (response is ImportResponse)
            {
                var importResponse = (ImportResponse)response;
                BusyText = $"{importResponse.AddedCount} hinzugefügt, {importResponse.UpdatedCount} aktualisiert, {importResponse.RemovedCount} gelöscht und {importResponse.IgnoredEntities.Count} ignoriert.";

                CurrentStatus = TaskStatus.Success;
                ErrorMessage = string.Empty;
                return true;
            }
            else if (response is ErrorResponse)
            {
                var errorResponse = (ErrorResponse)response;

                // TODO: Mitteilung anzeigen statt Exception?!
                CurrentStatus = TaskStatus.Failure;
                BusyText = "Fehler";
                ErrorMessage = errorResponse.Message;

                throw new System.Exception(errorResponse.Message);
            }

            return false;
        }
    }
}
