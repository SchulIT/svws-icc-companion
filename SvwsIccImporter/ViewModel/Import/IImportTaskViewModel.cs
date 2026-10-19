using SchulIT.SvwsClient;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SvwsIccImporter.ViewModel.Import
{
    public interface IImportTaskViewModel : INotifyPropertyChanged
    {
        public TaskStatus CurrentStatus { get; set; }

        public string BusyText { get; }

        public bool IsChecked { get; }

        public Task<bool> RunAsync(Schuljahresabschnitt abschnitt);
        
        public bool IsDisabled { get; }
        
        public string DisplayName { get; }
        
        public string Identifier { get; }
    }
}
