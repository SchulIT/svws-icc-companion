using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace SvwsIccImporter.UI
{
    public class WindowManager : IWindowManager
    {
        private readonly IClassicDesktopStyleApplicationLifetime appLifetime;
        
        public WindowManager(IClassicDesktopStyleApplicationLifetime appLifetime)
        {
            this.appLifetime = appLifetime;
        }

        public Window? GetFirstOpenedWindow()
        {
            return appLifetime.MainWindow;
        }
    }
}
