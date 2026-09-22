using Avalonia.Controls;

namespace SvwsIccImporter.UI
{
    public interface IWindowManager
    {
        public Window? GetFirstOpenedWindow();
    }
}
