using System;
using Wpf.Ui.Abstractions;

namespace SvwsIccImporter.DependencyInjection
{
    public class NavigationViewPageProvider(IServiceProvider serviceProvider) : INavigationViewPageProvider
    {
        public object? GetPage(Type pageType)
        {
            return serviceProvider.GetService(pageType);
        }
    }
}
