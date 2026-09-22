using Avalonia.Controls;
using SvwsIccImporter.ViewModel;

namespace SvwsIccImporter.View
{
    public partial class ImportPage : ContentPage
    {
        public ImportViewModel ViewModel { get; }

        public ImportPage(ImportViewModel importViewModel)
        {
            ViewModel = importViewModel;
            DataContext = importViewModel;

            InitializeComponent();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            await ViewModel.LoadSectionsAsync();
        }
    }
}