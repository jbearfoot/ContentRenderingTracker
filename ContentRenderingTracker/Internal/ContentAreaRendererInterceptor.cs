using EPiServer.Core;
using EPiServer.Web.Mvc.Html;
using System.Linq;
using System.Web.Mvc;

namespace ContentRenderingTracker.Internal
{
    public class ContentAreaRendererInterceptor : ContentAreaRenderer
    {
        private readonly ContentAreaRenderer _defaultRenderer;

        public ContentAreaRendererInterceptor(ContentAreaRenderer defaultRenderer)
        {
            _defaultRenderer = defaultRenderer;
        }

        public override void Render(HtmlHelper htmlHelper, ContentArea contentArea)
        {
            if (contentArea != null && contentArea.Items.Any(i => (i.AllowedRoles ?? Enumerable.Empty<string>()).Any()))
            {
                htmlHelper.ViewContext.GetTrackingContext().UsesPersonalizedProperties = true;
            }

            _defaultRenderer.Render(htmlHelper, contentArea);
        }
    }
}
