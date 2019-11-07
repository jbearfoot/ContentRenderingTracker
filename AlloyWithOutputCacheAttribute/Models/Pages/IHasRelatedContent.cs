using EPiServer.Core;

namespace AlloyWithOutputCacheAttribute.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}
