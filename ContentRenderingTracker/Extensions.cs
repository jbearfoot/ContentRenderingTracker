using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ContentRenderingTracker
{
    public static class Extensions
    {
        private const string ItemsKey = "ContentTrackingContext";
        public static ContentRenderingTrackerContext GetTrackingContext(this HttpContextBase httpContext)
        {
            var context = httpContext.Items[ItemsKey] as ContentRenderingTrackerContext;
            if (context == null)
            {
                context = new ContentRenderingTrackerContext();
                httpContext.Items[ItemsKey] = context;
            }
            return context;
        }

        public static void TrackContent(this HttpContextBase httpContext, IContent content) => httpContext.GetTrackingContext().AddDependency(new ContentLanguageReference(content));
        public static void TrackContent(this HttpContextBase httpContext, ContentReference contentLink, CultureInfo language = null) => 
            httpContext.GetTrackingContext().AddDependency(new ContentLanguageReference(contentLink, language));
        public static void TrackChildrenListing(this HttpContextBase httpContext, ContentReference contentLink) 
            => httpContext.GetTrackingContext().AddChildrenListingDependency(contentLink);
        public static void TrackContentItems(this HttpContextBase httpContext, IEnumerable<IContent> contentItems)
            => httpContext.TrackContentItems(contentItems.Select(c => (c.ContentLink, (c as ILocale)?.Language)));

        public static void TrackContentItems(this HttpContextBase httpContext, IEnumerable<(ContentReference contentLink, CultureInfo language)> contentItems)
        {
            var context = httpContext.GetTrackingContext();
            foreach (var content in contentItems)
            {
                context.AddDependency(new ContentLanguageReference(content.contentLink, content.language));
            }
        }

        public static ContentRenderingTrackerContext GetTrackingContext(this ViewContext viewContext) => viewContext.HttpContext.GetTrackingContext();

        public static void TrackContent(this ViewContext viewContext, IContent content) => viewContext.HttpContext.TrackContent(content);
        public static void TrackContent(this ViewContext viewContext, ContentReference contentLink, CultureInfo language = null) =>
            viewContext.HttpContext.TrackContent(contentLink, language);
        public static void TrackChildrenListing(this ViewContext viewContext, ContentReference contentLink) => 
            viewContext.HttpContext.TrackChildrenListing(contentLink);
        public static void TrackContentItems(this ViewContext viewContext, IEnumerable<IContent> contentLinks) => viewContext.HttpContext.TrackContentItems(contentLinks);
        public static void TrackContentItems(this ViewContext viewContext, IEnumerable<(ContentReference contentLink, CultureInfo language)> contentItems) => viewContext.HttpContext.TrackContentItems(contentItems);
    }
}
