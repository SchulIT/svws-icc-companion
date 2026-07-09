using System.Windows;

namespace SvwsIccImporter.UI
{
    public interface IWindowManager
    {
        public Window? GetFirstOpenedWindow();
    }
}
