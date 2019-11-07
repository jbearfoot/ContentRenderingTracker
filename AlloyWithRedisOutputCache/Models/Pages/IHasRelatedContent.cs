using EPiServer.Core;

namespace AlloyWithRedisOutputCache.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}
