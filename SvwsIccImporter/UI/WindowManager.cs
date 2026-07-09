using SvwsIccImporter.View;
using System;
using System.Linq;
using System.Windows;

namespace SvwsIccImporter.UI
{
    public class WindowManager : IWindowManager
    {
        /*public void OpenMainView()
        {
            var view = new MainView();
            view.Show();
            CloseAllOtherWindows(view);
        }*/

        private static void CloseAllOtherWindows()
        {
            CloseAllOtherWindows(null);
        }

        private static void CloseAllOtherWindows(Window? windowToStayOpen)
        {
            foreach (var window in Application.Current.Windows.OfType<Window>())
            {
                if (window != windowToStayOpen)
                {
                    window.Close();
                }
            }
        }

        public Window? GetFirstOpenedWindow()
        {
            return Application.Current.Windows.OfType<Window>().FirstOrDefault();
        }
    }
}
