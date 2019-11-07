using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ContentRenderingTracker.Internal
{
    public class XhtmlStringViewEngine : IViewEngine
    {
        private readonly XhtmlStringRenderer _xhtmlStringRenderer;
        private ViewEngineResult _emptyResult = new ViewEngineResult(Enumerable.Empty<string>());

        public XhtmlStringViewEngine(XhtmlStringRenderer xhtmlStringRenderer)
        {
            _xhtmlStringRenderer = xhtmlStringRenderer;
        }
        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (partialViewName == "DisplayTemplates/XhtmlString")
                return new ViewEngineResult(_xhtmlStringRenderer, this);

            return _emptyResult;
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return _emptyResult;
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
        }
    }
}
