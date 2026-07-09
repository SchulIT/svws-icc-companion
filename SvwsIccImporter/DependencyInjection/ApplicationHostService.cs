using Microsoft.Extensions.Hosting;
using SvwsIccImporter.View;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui;

namespace SvwsIccImporter.DependencyInjection
{
    public class ApplicationHostService(IServiceProvider serviceProvider) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!System.Windows.Application.Current.Windows.OfType<MainView>().Any())
            {
                var navigationWindow = serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow;
                navigationWindow!.ShowWindow();

                navigationWindow.Navigate(typeof(ImportPage));
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
