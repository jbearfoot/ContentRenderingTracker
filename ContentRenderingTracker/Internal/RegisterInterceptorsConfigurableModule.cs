using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ContentRenderingTracker.Internal
{
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    internal class RegisterInterceptorsConfigurableModule : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.ConfigurationComplete += (o, e) =>
            {
                e.Services.Intercept<PropertyRenderer>((locator, defaultRenderer) =>
                    new PropertyRendererInterceptor(defaultRenderer));

                e.Services.Intercept<ContentAreaRenderer>((locator, defaultRendered) =>
                    new ContentAreaRendererInterceptor(defaultRendered));

                e.Services.Intercept<IContentRenderer>((locator, defaultRenderer) =>
                    new ContentRendererInterceptor(defaultRenderer));
            };
        }

        public void Initialize(InitializationEngine context)
        {
            ViewEngines.Engines.Add(new XhtmlStringViewEngine(new XhtmlStringRenderer()));
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
