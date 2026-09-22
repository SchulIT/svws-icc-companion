using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;
using SvwsIccImporter.ViewModel;

namespace SvwsIccImporter.View
{
    public partial class SplashScreen : FAAppWindow
    {
        public SplashScreenViewModel ViewModel { get; }

        public SplashScreen(SplashScreenViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}