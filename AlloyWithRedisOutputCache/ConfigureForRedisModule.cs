using System;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Configuration;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using RedisOutputCache;
using RedisOutputCache.Internal;

namespace AlloyWithRedisOutputCache
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class ConfigureForRedisModule : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IDistributedCache>(s => new RedisDitributedCache("localhost:6379"));
        }

        public void Initialize(InitializationEngine context)
        {
            FilterProviders.Providers.Add(new RedisOutputCacheFilterProvider(context.Locate.Advanced.GetInstance<IDistributedCache>(), 
                () => context.Locate.Advanced.GetInstance<IContentRouteHelper>(), 
                context.Locate.Advanced.GetInstance<IContentCacheKeyCreator>(),
                context.Locate.Advanced.GetInstance<Settings>()));
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}