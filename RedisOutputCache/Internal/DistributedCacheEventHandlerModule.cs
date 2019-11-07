using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisOutputCache.Internal
{
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class DistributedCacheEventHandlerModule : IInitializableModule
    {
        private IDistributedCache _htmlCache;
        private IContentEvents _contentEvents;
        private IContentLoader _contentLoader;
        private IContentSecurityRepository _contentSecurityRepository;
        private IContentCacheKeyCreator _contentCacheKeyCreator;
        private const string ClearChildrenListingKey = "RedisClearChildrenKey";

        public void Initialize(InitializationEngine context)
        {
            _contentEvents = context.Locate.Advanced.GetInstance<IContentEvents>();
            _contentSecurityRepository = context.Locate.Advanced.GetInstance<IContentSecurityRepository>();
            _contentLoader = context.Locate.Advanced.GetInstance<IContentLoader>();
            _contentCacheKeyCreator = context.Locate.Advanced.GetInstance<IContentCacheKeyCreator>();
            _htmlCache = context.Locate.Advanced.GetInstance<IDistributedCache>();

            _contentEvents.CreatedContent += ContentCreated;
            _contentEvents.MovedContent += MovedContent;
            _contentEvents.PublishingContent += PublishingContent;
            _contentEvents.PublishedContent += PublishedContent;
            _contentEvents.DeletedContent += DeletedContent;

            _contentSecurityRepository.ContentSecuritySaved += ContentSecuritySaved;
        }

        public void Uninitialize(InitializationEngine context)
        {
            _contentEvents.CreatedContent -= ContentCreated;
            _contentEvents.MovedContent -= MovedContent;
            _contentEvents.PublishingContent -= PublishingContent;
            _contentEvents.PublishedContent -= PublishedContent;
            _contentEvents.DeletedContent -= DeletedContent;

            _contentSecurityRepository.ContentSecuritySaved -= ContentSecuritySaved;
        }

        private void ContentSecuritySaved(object sender, ContentSecurityEventArg e)
        {
            ContentChanged(e.ContentLink);
            ChildrenListingChanged(_contentLoader.Get<IContent>(e.ContentLink).ParentLink);
        }

        private void DeletedContent(object sender, DeleteContentEventArgs e)
        {
            ChildrenListingChanged(ContentReference.IsNullOrEmpty(e.TargetLink) ? e.ContentLink : e.TargetLink);
            foreach (var item in e.DeletedDescendents)
                ContentChanged(item);
        }

        private void PublishingContent(object sender, ContentEventArgs e)
        {
            var versionable = e.Content as IVersionable;
            if (versionable == null || versionable.IsPendingPublish)
                e.Items[ClearChildrenListingKey] = true;
        }


        private void PublishedContent(object sender, ContentEventArgs e)
        {
            ContentChanged(e.ContentLink);
            if (e.Items.Contains(ClearChildrenListingKey))
                ChildrenListingChanged(e.Content.ParentLink);
        }

        private void MovedContent(object sender, ContentEventArgs e)
        {
            ChildrenListingChanged(e.TargetLink);
            ChildrenListingChanged((e as MoveContentEventArgs).OriginalParent);
        }

        private void ContentCreated(object sender, ContentEventArgs e)
        {
            ChildrenListingChanged(e.TargetLink);
        }

        internal void ContentChanged(ContentReference contentLink)
        {
            RemoveKey(_contentCacheKeyCreator.CreateCommonCacheKey(contentLink.ToReferenceWithoutVersion()));
        }

        internal void ChildrenListingChanged(ContentReference contentLink)
        {
            RemoveKey(_contentCacheKeyCreator.CreateChildrenCacheKey(contentLink.ToReferenceWithoutVersion(), null));
        }

        private void RemoveKey(string key)
        {
            var affectedKeys = new HashSet<string>();
            CollectDependencyKeys(affectedKeys, key);

            _htmlCache.Remove(affectedKeys);
        }

        private void CollectDependencyKeys(HashSet<string> dependencies, string key)
        {
            dependencies.Add(key);
            var keyDependencies = _htmlCache.GetDependencies(key);

            foreach (var dependencyKey in keyDependencies)
            {
                if (dependencyKey.StartsWith("/"))
                {
                    dependencies.Add(dependencyKey);
                }
                else
                {
                    CollectDependencyKeys(dependencies, dependencyKey);
                }
            }              
        }

    }
}
