using Sitecore.ContentSearch.Abstractions.Factories;
using Sitecore.ContentSearch.Linq.Factories;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.SolrProvider;
using Sitecore.ContentSearch.SolrProvider.Factories;

namespace Sitecore.ContentSearch.Spatial.Solr.Provider
{
    public class SolrSearchIndexWithSpatial : SolrSearchIndex
    {
        //public string IndexName;
        //public string core;

        public SolrSearchIndexWithSpatial(string name, string core, IIndexPropertyStore propertyStore, string @group) : base(name, core, propertyStore, @group)
        {
            //this.IndexName = name;
            //this.core = core;
        }

        public SolrSearchIndexWithSpatial(string name, string core, IIndexPropertyStore propertyStore) : base(name, core, propertyStore)
        {
        }

        public override IProviderSearchContext CreateSearchContext(Security.SearchSecurityOptions options = Security.SearchSecurityOptions.EnableSecurityCheck)
        {
            IQueryableFactory queryableFactory = new DefaultQueryableFactory();
            ILinqToIndexFactory linqToIndexFactory = new SolrLinqToIndexFactory(queryableFactory);
            //ISearchIndex searchIndex = new SolrSearchIndex(IndexName,);
            return new SolrSearchWithSpatialContext(this,linqToIndexFactory,options);
        }
    }
}
