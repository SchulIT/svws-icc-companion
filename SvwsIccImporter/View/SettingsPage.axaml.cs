using Avalonia.Controls;
using SvwsIccImporter.ViewModel;

namespace SvwsIccImporter.View
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsViewModel ViewModel;

        public SettingsPage(SettingsViewModel settingsViewModel)
        {
            ViewModel = settingsViewModel;
            DataContext = settingsViewModel;

            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ViewModel.LoadSettings();
        }
    }
}