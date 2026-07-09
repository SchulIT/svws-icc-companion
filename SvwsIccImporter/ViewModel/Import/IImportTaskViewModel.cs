using SchulIT.SvwsClient;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SvwsIccImporter.ViewModel.Import
{
    public interface IImportTaskViewModel : INotifyPropertyChanged
    {
        public TaskStatus CurrentStatus { get; set; }

        public string BusyText { get; }

        public bool IsEnabled { get; }

        public Task<bool> RunAsync(Schuljahresabschnitt abschnitt);
    }
}
