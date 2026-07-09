using SvwsIccImporter.ViewModel;
using Wpf.Ui.Abstractions.Controls;

namespace SvwsIccImporter.View
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel settingsViewModel)
        {
            ViewModel = settingsViewModel;
            DataContext = this;

            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.LoadSettings();
        }
    }
}
