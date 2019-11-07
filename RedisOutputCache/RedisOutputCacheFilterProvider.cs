using EPiServer.Configuration;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RedisOutputCache
{
    public class RedisOutputCacheFilterProvider : IFilterProvider
    {
        private readonly IDistributedCache _htmlCache;
        private readonly ServiceAccessor<IContentRouteHelper> _contentRouteHelperAccessor;
        private readonly IContentCacheKeyCreator _contentCacheKeyCreator;
        private readonly Settings _settings;

        public RedisOutputCacheFilterProvider(IDistributedCache htmlCache, ServiceAccessor<IContentRouteHelper> contentRouteHelperAccessor, IContentCacheKeyCreator contentCacheKeyCreator, Settings settings)
        {
            _htmlCache = htmlCache;
            _contentRouteHelperAccessor = contentRouteHelperAccessor;
            _contentCacheKeyCreator = contentCacheKeyCreator;
            _settings = settings;
        }

        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            return new[] { new Filter(new RedisOutputCacheFilter(_htmlCache, _contentRouteHelperAccessor, _contentCacheKeyCreator, _settings), FilterScope.First, 0) };
        }
    }
}
