using SvwsIccImporter.ViewModel;
using System;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SvwsIccImporter.View
{
    public partial class MainView : INavigationWindow
    {
        public SvwsViewModel SvwsViewModel { get; }

        public MainView(INavigationService navigationService, SvwsViewModel svwsViewModel)
        {
            SvwsViewModel = svwsViewModel;

            DataContext = this;
            SystemThemeWatcher.Watch(this);

            InitializeComponent();
            TrailingContent.DataContext = svwsViewModel;

            navigationService.SetNavigationControl(RootNavigation);
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await SvwsViewModel.LoadSchoolInformation();
        }

        public void CloseWindow() => Close();

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public void ShowWindow() => Show();
    }
}
