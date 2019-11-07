# A tracker for which content items are used during rendering and a smarter outputcache

## ContentRenderingTracker
# ContentRenderingTracker
I have created a ContentRenderingTrackerContext that holds information about which content items that are used during rendering of the current request. The context can be retreived from either HttpContextBase or ViewContext
through extension methods

*public static ContentRenderingTrackerContext GetTrackingContext(this ViewContext viewContext)*
*public static ContentRenderingTrackerContext GetTrackingContext(this HttpContext httpContext)*

## PropertyFor and Content rendering
To capture which content items that are used during rendering I choosed to intercept *PropertyRenderer*, *ContentAreaRenderer* and *IContentRenderer*. The interceptors basically checks adds references to the used content items
and then delegates teh call to the default component.
There is also a possible to explicitly register content items that are used during rendering (for example when rendering menus) as:
*htmlHelper.ViewContext.TrackContent(content);*

## Personalization
The tracking component also keeps track of if the current rendered content contains a personalized ContentArea of XHtmlString, if so that information is also tracked. Then consumers like output or CDN cache components can act
on that information.

#ContentAwareOutputCache
I also added another project that uses the ContentRenderingTracker component. This project adds a ContentAwareOutputCacheAttribute that can be used instead of the built-in attribute ContentAttributeCacheAttribute. The difference
with the builtin attribute is that this attribute only invalidates the cache when some of the content that it actually depends on changes (unlike the builtin one that invalidates all output caches when some content changes).

## Alloy modifications
I have uploaded a minimal modified Alloy site (AlloyWithOutputCacheAttribute) together with the code to my git hub repository. I added a Block type NowBlock which can be used to verify that personalization of ContentArea and XhtmlString works as expected. 

### Menu
I have changed the Menu in Helpers/HtmlHelpers.cs so it registers which content items that are used in the tracking context. 

#Thoughts
Tne ContentAwareOutputCache project uses the output cache in aspnet. The ContentRenderingTracker should though be possibel to use with other types of distributed caches such as Redis or CDN.

# Disclamier
This is nothing offically supported by EPiServer, you are free to use it as you like at your own risk.