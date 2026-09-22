using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SvwsIccImporter.UI
{
    public class NavigationPageFactory : IFANavigationPageFactory
    {
        private readonly IServiceProvider services;

        public NavigationPageFactory(IServiceProvider services)
        {
            this.services = services;
        }

        public Control GetPage(Type srcType)
        {
            var control = services.GetRequiredService(srcType) as Control;

            if (control == null)
            {
                throw new InvalidCastException("srcType is not a Control");
            }

            return control;
        }

        public Control GetPageFromObject(object target)
        {
            throw new NotImplementedException();
        }
    }
}
