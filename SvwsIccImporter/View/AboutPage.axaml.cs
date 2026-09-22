using Avalonia.Controls;
using SvwsIccImporter.ViewModel;
using System;
using System.Threading.Tasks;

namespace SvwsIccImporter.View
{
    public partial class AboutPage : ContentPage
    {
        public AboutViewModel ViewModel { get; }

        public AboutPage(AboutViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OpenProjectItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ViewModel.LoadLibraries();
        }

        private async void OnOpenProjectItemClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var uri = new Uri(ViewModel.ProjectUrl);
            await TopLevel.GetTopLevel(this)?.Launcher.LaunchUriAsync(uri);
        }
    }
}