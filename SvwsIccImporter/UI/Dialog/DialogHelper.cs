using FluentAvalonia.UI.Controls;
using System.Threading.Tasks;

namespace SvwsIccImporter.UI.Dialog
{
    public class DialogHelper : IDialogHelper
    {
        private readonly IWindowManager windowManager;
        private readonly IDispatcherHelper dispatcherHelper;

        public DialogHelper(IWindowManager windowManager, IDispatcherHelper dispatcherHelper)
        {
            this.windowManager = windowManager;
            this.dispatcherHelper = dispatcherHelper;
        }

        public async Task ShowAsync(Dialog dialog)
        {
            var taskDialog = new FATaskDialog
            {
                Title = dialog.Title,
                Header = dialog.Header,
                Content = dialog.Content,
                IconSource = new FASymbolIconSource{ Symbol = FASymbol.Alert },
                Buttons = {
                    FATaskDialogButton.OKButton
                }
            };

            var errorDialog = dialog as ErrorDialog;
            if (errorDialog != null)
            {
                taskDialog.FooterVisibility = FATaskDialogFooterVisibility.Always;
                taskDialog.Footer = errorDialog.Exception?.Message;
                taskDialog.IconSource = new FASymbolIconSource { Symbol = FASymbol.ReportHacked };
            }

            taskDialog.XamlRoot = windowManager.GetFirstOpenedWindow();
            await taskDialog.ShowAsync();
        }
    }
}
