using SvwsIccImporter.ViewModel;
using Wpf.Ui.Abstractions.Controls;

namespace SvwsIccImporter.View
{
    public partial class AboutPage : INavigableView<AboutViewModel>
    {
        public AboutViewModel ViewModel { get; }

        public AboutPage(AboutViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.LoadLibraries();
        }
    }
}
