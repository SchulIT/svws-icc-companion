using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SvwsIccImporter.ViewModel;
using System;

namespace SvwsIccImporter.View
{
    public partial class MainView : FAAppWindow
    {
        public SvwsViewModel SvwsViewModel { get; }

        public MainView(IFANavigationPageFactory navigationPageFactory, SvwsViewModel svwsViewModel)
        {
            SvwsViewModel = svwsViewModel;
            DataContext = svwsViewModel;

            InitializeComponent();
            ContentFrame.NavigationPageFactory = navigationPageFactory;
            ContentFrame.Navigated += OnContentFrameNavigated;

            NavigationView.ItemInvoked += OnNavigationViewItemInvoked;
            Loaded += OnLoaded;
        }

        private void OnContentFrameNavigated(object sender, FluentAvalonia.UI.Navigation.FANavigationEventArgs e)
        {
            var page = e.Content as Control;

            foreach (FANavigationViewItemBase item in NavigationView.MenuItems)
            {
                if (item.Tag == page.GetType())
                {
                    NavigationView.SelectedItem = item;
                }
            }

            foreach (FANavigationViewItemBase item in NavigationView.FooterMenuItems)
            {
                if (item.Tag == page.GetType())
                {
                    NavigationView.SelectedItem = item;
                }
            }
        }

        private async void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(ImportPage));
            await SvwsViewModel.LoadSchoolInformation();
        }

        private void OnNavigationViewItemInvoked(object? sender, FANavigationViewItemInvokedEventArgs e)
        {
            if(e.InvokedItemContainer is FANavigationViewItem) {
                ContentFrame.Navigate(e.InvokedItemContainer.Tag as Type, e.RecommendedNavigationTransitionInfo);
            }
        }
    }
}