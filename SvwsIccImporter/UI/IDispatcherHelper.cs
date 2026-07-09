using System;

namespace SvwsIccImporter.UI
{
    public interface IDispatcherHelper
    {
        void Initialize();

        void InvokeOnUiThread(Action action);
    }
}
