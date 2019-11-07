using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace ContentRenderingTracker.Internal
{
    public class ContentRendererInterceptor : IContentRenderer
    {
        private readonly IContentRenderer _defaultRenderer;

        public ContentRendererInterceptor(IContentRenderer defaultRenderer)
        {
            _defaultRenderer = defaultRenderer;
        }

        public void Render(HtmlHelper helper, PartialRequest partialRequestHandler, IContentData contentData, TemplateModel templateModel)
        {
            if (contentData is IContent content)
            {
                helper.ViewContext.TrackContent(content);
            }
            _defaultRenderer.Render(helper, partialRequestHandler, contentData, templateModel);
        }

    }
}
