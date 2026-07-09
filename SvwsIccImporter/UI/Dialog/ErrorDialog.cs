using System;

namespace SvwsIccImporter.UI.Dialog
{
    public class ErrorDialog : Dialog
    {
        public Exception? Exception { get; set; }

        public ErrorDialog()
        {
            Icon = Icon.ShieldErrorRedBar;
        }
    }
}
