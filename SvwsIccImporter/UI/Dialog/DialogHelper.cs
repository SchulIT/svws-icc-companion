using System.Windows.Forms;
using System.Windows.Interop;

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

        public void Show(Dialog dialog)
        {
            var taskDialogPage = new TaskDialogPage
            {
                Text = dialog.Content,
                Heading = dialog.Header,
                Caption = dialog.Title,
                Icon = GetIcon(dialog.Icon)
            };

            var errorDialog = dialog as ErrorDialog;
            if (errorDialog != null)
            {
                taskDialogPage.Icon = TaskDialogIcon.Error;
                taskDialogPage.Expander = new TaskDialogExpander
                {
                    Text = errorDialog.Exception?.Message,
                    Expanded = true
                };
            }

            /*var confirmDialog = dialog as ConfirmDialog;
            if (confirmDialog != null)
            {
                var buttonContinue = TaskDialogButton.Continue;
                var buttonClose = TaskDialogButton.Close;

                buttonContinue.Click += (s, e) =>
                {
                    confirmDialog.ConfirmAction?.Invoke();
                };

                buttonClose.Click += (s, e) =>
                {
                    confirmDialog.CancelAction?.Invoke();
                };
            }*/

            dispatcherHelper.InvokeOnUiThread(() => TaskDialog.ShowDialog(new WindowInteropHelper(windowManager.GetFirstOpenedWindow()).Handle, taskDialogPage));
        }

        private static TaskDialogIcon GetIcon(Icon icon)
        {
            return icon switch
            {
                Icon.None => TaskDialogIcon.None,
                Icon.Information => TaskDialogIcon.Information,
                Icon.Warning => TaskDialogIcon.Warning,
                Icon.Shield => TaskDialogIcon.Shield,
                Icon.ShieldBlueBar => TaskDialogIcon.ShieldBlueBar,
                Icon.ShieldGrayBar => TaskDialogIcon.ShieldGrayBar,
                Icon.ShieldWarningYellowBar => TaskDialogIcon.ShieldWarningYellowBar,
                Icon.ShieldErrorRedBar => TaskDialogIcon.ShieldErrorRedBar,
                Icon.ShieldSuccessGreenBar => TaskDialogIcon.ShieldSuccessGreenBar,
                _ => TaskDialogIcon.None,
            };
        }
    }
}
