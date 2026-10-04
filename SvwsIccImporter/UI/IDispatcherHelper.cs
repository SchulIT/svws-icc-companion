using System;
using System.Threading.Tasks;

namespace SvwsIccImporter.UI
{
    public interface IDispatcherHelper
    {
        void Initialize();

        void InvokeOnUiThread(Action action);

        Task InvokeOnUiThreadAsync(Func<Task> action);
    }
}
