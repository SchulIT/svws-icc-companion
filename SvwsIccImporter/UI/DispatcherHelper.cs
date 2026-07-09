using System;
using System.Windows.Threading;

namespace SvwsIccImporter.UI
{
    public class DispatcherHelper : IDispatcherHelper
    {
        private Dispatcher dispatcher;

        public DispatcherHelper()
        {
            Initialize();
        }

        public void Initialize()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void InvokeOnUiThread(Action action)
        {
            dispatcher?.Invoke(action);
        }
    }
}
