using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Security;
using EPiServer.Web;
using EPiServer.Web.Mvc.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ContentRenderingTracker.Internal
{
    public class XhtmlStringRenderer : IView
    {
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var xhtmlString = viewContext.ViewData.Model as XhtmlString;
            if (xhtmlString != null)
            {
                var roleSecurityDescriptors = xhtmlString.Fragments
                    .OfType<ISecurable>()
                    .Select(s => s.GetSecurityDescriptor())
                    .OfType<IRoleSecurityDescriptor>();

                if (roleSecurityDescriptors.Any(rs => (rs.RoleIdentities ?? Enumerable.Empty<string>()).Any()))
                {
                    viewContext.GetTrackingContext().UsesPersonalizedProperties = true;
                }
                    

                var htmlHelper = new HtmlHelper(viewContext, new ViewPage());
                writer.Write(htmlHelper.XhtmlString(xhtmlString));
            }
        }
    }
}
