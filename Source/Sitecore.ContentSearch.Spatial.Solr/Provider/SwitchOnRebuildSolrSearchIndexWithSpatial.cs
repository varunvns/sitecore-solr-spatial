using Sitecore.ContentSearch.Abstractions.Factories;
using Sitecore.ContentSearch.Linq.Factories;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.SolrProvider;
using Sitecore.ContentSearch.SolrProvider.Factories;

namespace Sitecore.ContentSearch.Spatial.Solr.Provider
{
    public class SwitchOnRebuildSolrSearchIndexWithSpatial : SwitchOnRebuildSolrSearchIndex
    {
        //Added ILinqToIndexFactory linkLinqToIndexFactory to avoid build errors. This might fails as well.
        public SwitchOnRebuildSolrSearchIndexWithSpatial(string name, string core, string rebuildcore, IIndexPropertyStore propertyStore) : base(name, core, rebuildcore, propertyStore)
        {
        }

        public override IProviderSearchContext CreateSearchContext(Security.SearchSecurityOptions options = Security.SearchSecurityOptions.EnableSecurityCheck)
        {
            IQueryableFactory iQueryableFactory = new DefaultQueryableFactory();
            ILinqToIndexFactory linqToIndexFactory = new SolrLinqToIndexFactory(iQueryableFactory);
            return new SolrSearchWithSpatialContext(this, linqToIndexFactory, options);
        }
    }
}
