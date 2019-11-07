using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentRenderingTracker
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ContentLanguageReference: IEquatable<ContentLanguageReference>
    {
        public ContentLanguageReference(IContent content)
            :this(content.ContentLink, (content as ILocale)?.Language)
        { }
        public ContentLanguageReference(ContentReference contentLink, CultureInfo lanugage)
        {
            ContentLink = contentLink;
            Language = lanugage ?? CultureInfo.InvariantCulture;
        }

        public ContentReference ContentLink { get; }
        public CultureInfo Language { get; }

        public bool Equals(ContentLanguageReference other)
        {
            if (other is object)
            {
                return ContentLink.Equals(other.ContentLink) && Language.Equals(other.Language);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + ContentLink.GetHashCode();
                hash = hash * 23 + Language.GetHashCode();
                return hash;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay { get { return $"ID = {ContentLink}, Langauge = {Language}"; } }
    }

    public class ContentRenderingTrackerContext
    {
        private HashSet<ContentLanguageReference> _contentItems = new HashSet<ContentLanguageReference>();
        private HashSet<ContentReference> _childrenListings = new HashSet<ContentReference>();

        public IEnumerable<ContentLanguageReference> ContentItems => _contentItems;
        public IEnumerable<ContentReference> ChildrenListings => _childrenListings;

        public bool UsesContentQuery { get; set; } = false;

        public bool UsesPersonalizedProperties { get; set; } = false;

        public bool UsesPersonalizationOrQuery => UsesContentQuery || UsesPersonalizedProperties;

        public void AddChildrenListingDependency(ContentReference contentLink) => _childrenListings.Add(contentLink);

        public void AddDependency(ContentLanguageReference contentLink) => _contentItems.Add(contentLink);
    }

}
