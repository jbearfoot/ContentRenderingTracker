using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Mvc.Html;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;

namespace ContentRenderingTracker.Internal
{
    public class PropertyRendererInterceptor : PropertyRenderer
    {
        private readonly PropertyRenderer _defaultRenderer;
        private readonly static ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> _contentProperties = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        public PropertyRendererInterceptor(PropertyRenderer defaultRenderer)
        {
            _defaultRenderer = defaultRenderer;
        }

        public override MvcHtmlString PropertyFor<TModel, TValue>(HtmlHelper<TModel> html, string viewModelPropertyName, object additionalViewData, object editorSettings, Expression<Func<TModel, TValue>> expression, Func<string, MvcHtmlString> displayForAction)
        {
            var metaDataModel = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            html.ViewContext.TrackContentItems(GetContentFromViewModel(metaDataModel));
            return _defaultRenderer.PropertyFor<TModel, TValue>(html, viewModelPropertyName, additionalViewData, editorSettings, expression, displayForAction);
        }

        private IEnumerable<(ContentReference contentLink, CultureInfo language)> GetContentFromViewModel(ModelMetadata modelMetadata)
        { 
            //Check if property represents some content
            if (modelMetadata.Model is ContentReference contentLink)
            {
                yield return (contentLink, null);
            }

            //Check if container is IContent
            if (typeof(IContent).IsAssignableFrom(modelMetadata.ContainerType) && modelMetadata.Container != null)
            {
                if (modelMetadata.Container is IContent content)
                {
                    yield return (content.ContentLink, (content as ILocale)?.Language);
                }
                else
                {
                    //A common use case as in for example Alloy is to have a view model with content as a property.
                    //this could be performance optimized by compiling reflection access as expressions
                    foreach (var property in _contentProperties.GetOrAdd(modelMetadata.Container.GetType(), t => t.GetProperties().Where(p => typeof(IContent).IsAssignableFrom(p.PropertyType))))
                    {
                        if (typeof(IContent).IsAssignableFrom(property.PropertyType))
                        {
                            var contentProperty = property.GetValue(modelMetadata.Container, null) as IContent;
                            if (contentProperty is object)
                            {
                                yield return (contentProperty.ContentLink, (contentProperty as ILocale)?.Language);
                            }
                        }
                    }
                }
            }
        }




        //This other method are overriden in case that if there is a custom ContentAreaRender that gets called instead of base for this class
        public override MvcHtmlString BeginEditSection(HtmlHelper helper, string htmlElement, string propertyKey, string propertyName, object htmlAttributes)
            => _defaultRenderer.BeginEditSection(helper, htmlElement, propertyKey, propertyName, htmlAttributes);

        public override EditContainer CreateEditElement(HtmlHelper helper, string epiPropertyKey, string epiPropertyName, string editElementName, string editElementCssClass, Func<string> renderSettingsAttributeWriter, Func<string> editorSettingsAttributeWriter, TextWriter writer)
            => _defaultRenderer.CreateEditElement(helper, epiPropertyKey, epiPropertyName, editElementName, editElementCssClass, renderSettingsAttributeWriter, editorSettingsAttributeWriter, writer);

        public override MvcHtmlString EditAttributes(HtmlHelper helper, string propertyKey, string propertyName)
            => _defaultRenderer.EditAttributes(helper, propertyKey, propertyName);
    }
}
