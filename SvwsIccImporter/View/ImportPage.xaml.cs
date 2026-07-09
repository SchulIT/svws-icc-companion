using SvwsIccImporter.ViewModel;
using Wpf.Ui.Abstractions.Controls;

namespace SvwsIccImporter.View
{
    public partial class ImportPage : INavigableView<ImportViewModel>
    {
        public ImportViewModel ViewModel { get; }

        public ImportPage(ImportViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await ViewModel.LoadSectionsAsync();
        }
    }
}
