using ContentRenderingTracker;
using EPiServer;
using EPiServer.Configuration;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Internal;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace ContentAwareOutputCache
{
    public class ContentAwareOutputCacheAttribute : OutputCacheAttribute
    {
        private class ValidationState
        {
            public ValidationState(bool usesPersonaliazationOrQuery, Settings settings)
            {
                UsesPersonalizationOrQuery = usesPersonaliazationOrQuery;
                Settings = settings;
            }
            public bool UsesPersonalizationOrQuery { get; }
            public Settings Settings { get; }
        }

        private bool _initialized;

        //Note: For attributes we cant take dependency through constructor since they are instantiated during assembly scanning
        internal Injected<IContentCacheKeyCreator> CacheKeyCreator;
        internal Injected<Settings> ConfigurationSettings { get; set; }
        internal Injected<IContentLoader> ContentLoader { get; set; }

        public virtual bool Disable { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
            {
                throw new NotSupportedException("ContentAwareOutputCacheAttribute should not be used on child actions.");
            }
            base.OnActionExecuting(filterContext);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (Disable)
            {
                return;
            }

            if (!_initialized)
            {
                //If no values are set on attrbute take default values from config
                if (Duration == 0)
                {
                    Duration = Convert.ToInt32(ConfigurationSettings.Service.HttpCacheExpiration.TotalSeconds);
                }
                if (String.IsNullOrEmpty(VaryByCustom))
                {
                    VaryByCustom = ConfigurationSettings.Service.HttpCacheVaryByCustom;
                }
                _initialized = true;
            }

            var contentLink = filterContext.RequestContext.GetContentLink();
            if (!ContentReference.IsNullOrEmpty(contentLink))
            {
                var versionable = ContentLoader.Service.Get<IContent>(contentLink) as IVersionable;
                if (versionable != null && versionable.StopPublish.HasValue)
                {
                    DateTime now = DateTime.Now;
                    if (versionable.StopPublish < now)
                    {
                        return;
                    }

                    if (versionable.StopPublish.Value < now.AddSeconds(Duration))
                    {
                        Duration = Convert.ToInt32((versionable.StopPublish.Value - now).TotalSeconds);
                    }
                }

                if (OutputCacheHandler.UseOutputCache(filterContext.HttpContext.User, filterContext.HttpContext, new TimeSpan(0, 0, Duration)))
                {
                    base.OnResultExecuting(filterContext);
                }
            }
        }


        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            if (OutputCacheHandler.UseOutputCache(filterContext.HttpContext.User, filterContext.HttpContext, new TimeSpan(0, 0, Duration)))
            {
                var contentRenderingContext = filterContext.HttpContext.GetTrackingContext();
                if (!contentRenderingContext.UsesPersonalizationOrQuery)
                {
                    AddDepedenciesToCache(filterContext.HttpContext);
                }             
                var state = new ValidationState(contentRenderingContext.UsesPersonalizationOrQuery, ConfigurationSettings.Service);
                filterContext.HttpContext.Response.Cache.AddValidationCallback(new HttpCacheValidateHandler(ValidateOutputCache), state);
            }
        }

        private void AddDepedenciesToCache(HttpContextBase httpContext)
        {
            var dependencies = new List<string>();
            var contentRenderingContext = httpContext.GetTrackingContext();
            foreach (var contentLanguageReference in contentRenderingContext.ContentItems)
            {
                dependencies.Add(CacheKeyCreator.Service.CreateLanguageCacheKey(contentLanguageReference.ContentLink, contentLanguageReference.Language.Name));
            }
            foreach (var contentLink in contentRenderingContext.ChildrenListings)
            {
                dependencies.Add(CacheKeyCreator.Service.CreateChildrenCacheKey(contentLink, null));
            }
            httpContext.Cache.Insert(httpContext.Request.RawUrl, DateTime.Now.Ticks, new CacheDependency(null, dependencies.ToArray()));
        }

        public static void ValidateOutputCache(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            var httpContextBase = new HttpContextWrapper(context);
            var state = data as ValidationState;

            if (state.UsesPersonalizationOrQuery || context.Cache[context.Request.RawUrl] == null) 
            {
                validationStatus = HttpValidationStatus.Invalid;
            }
            else if (!OutputCacheHandler.UseOutputCache(context.User, httpContextBase, state.Settings.HttpCacheExpiration))
            {
                validationStatus = HttpValidationStatus.IgnoreThisRequest;
            }
            else
            {
                validationStatus = HttpValidationStatus.Valid;
            }
        }
    }
}
