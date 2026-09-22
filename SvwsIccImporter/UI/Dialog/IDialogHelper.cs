using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvwsIccImporter.UI.Dialog
{
    public interface IDialogHelper
    {
        Task ShowAsync(Dialog dialog);
    }
}
