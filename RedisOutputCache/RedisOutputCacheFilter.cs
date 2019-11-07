using ContentRenderingTracker;
using EPiServer.Configuration;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Internal;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RedisOutputCache
{
    public class RedisOutputCacheFilter : IActionFilter, IResultFilter
    {
        public const string CacheHandledKey = "OutputCacheReturned";
        private readonly IDistributedCache _htmlCache;
        private readonly ServiceAccessor<IContentRouteHelper> _contentRouteHelperAccessor;
        private readonly IContentCacheKeyCreator _contentCacheKeyCreator;
        private readonly TimeSpan _duration;

        public RedisOutputCacheFilter(IDistributedCache htmlCache, ServiceAccessor<IContentRouteHelper> contentRouteHelperAccessor, IContentCacheKeyCreator contentCacheKeyCreator, Settings settings)
        {
            _htmlCache = htmlCache;
            _contentRouteHelperAccessor = contentRouteHelperAccessor;
            _contentCacheKeyCreator = contentCacheKeyCreator;
            _duration = settings.HttpCacheExpiration;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (ShouldUseOutputCache(filterContext))
            {
                var cachedResult = _htmlCache.Get(filterContext.HttpContext.Request.RawUrl);
                if (!string.IsNullOrEmpty(cachedResult))
                {
                    filterContext.HttpContext.Items[CacheHandledKey] = true;
                    filterContext.Result = new ContentResult() { Content = cachedResult };
                    return;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (filterContext.HttpContext.Items[CacheHandledKey] == null)
            {
                var currentContext = filterContext.HttpContext.GetTrackingContext();
                if (ShouldUseOutputCache(filterContext) && !currentContext.UsesPersonalizationOrQuery)
                {
                    SetDependecies(filterContext.HttpContext.Request.RawUrl, currentContext);
                    var response = filterContext.HttpContext.Response;
                    response.Filter = new OutputProcessorStream(response.Filter, _htmlCache, filterContext.HttpContext.Request.RawUrl);
                }
            }
        }

        private bool ShouldUseOutputCache(ControllerContext controllerContext)
        {
            var routedContentLink = _contentRouteHelperAccessor().ContentLink;
            return OutputCacheHandler.UseOutputCache(controllerContext.HttpContext.User, controllerContext.HttpContext, _duration) &&
                !controllerContext.IsChildAction && !ContentReference.IsNullOrEmpty(routedContentLink);
        }


        private void SetDependecies(string rawUrl, ContentRenderingTrackerContext contentRenderingContext)
        {
            foreach (var contentLanguageReference in contentRenderingContext.ContentItems)
            {
                //for now we skip content language, meaning a change in one language evicts all language versions
                //_htmlCache.AddDependency(_contentCacheKeyCreator.CreateLanguageCacheKey(contentLanguageReference.ContentLink, contentLanguageReference.Language.Name), rawUrl);
                _htmlCache.AddDependency(_contentCacheKeyCreator.CreateCommonCacheKey(contentLanguageReference.ContentLink), rawUrl);
            }
            foreach (var contentLink in contentRenderingContext.ChildrenListings)
            {
                _htmlCache.AddDependency(_contentCacheKeyCreator.CreateChildrenCacheKey(contentLink, null), rawUrl);
            }
        }
    }

    internal class OutputProcessorStream : MemoryStream
    {
        private readonly Stream _stream;
        private readonly IDistributedCache _cache;
        private readonly string _key;

        public OutputProcessorStream(Stream stream, IDistributedCache cache, string key)
        {
            _stream = stream;
            _cache = cache;
            _key = key;
        }

        public string GetHtml()
        {
            Position = 0;
            var sr = new StreamReader(this);
            return sr.ReadToEnd();
        }

        public override void Close()
        {
            _cache.Set(_key, GetHtml());

            Position = 0;
            CopyTo(_stream);
            _stream.Close();
            base.Close();
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_stream != null)
                    _stream.Dispose();
            }
        }
    }
}
