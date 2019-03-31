using System.Linq;
using Sitecore.ContentSearch.Abstractions;
using Sitecore.ContentSearch.Abstractions.Factories;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Factories;
using Sitecore.ContentSearch.Linq.Parsing;
using Sitecore.ContentSearch.Linq.Solr;
using Sitecore.ContentSearch.Pipelines.QueryGlobalFilters;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.SolrProvider;
using Sitecore.ContentSearch.SolrProvider.Converters;
using Sitecore.ContentSearch.Spatial.Solr.Indexing;
using Sitecore.ContentSearch.Spatial.Solr.Parsing;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Diagnostics;

namespace Sitecore.ContentSearch.Spatial.Solr.Provider
{
    public class SolrSearchWithSpatialContext : SolrSearchContext, IProviderSearchContext
    {
        private readonly SolrSearchIndex index;
        private readonly SearchSecurityOptions securityOptions;
        private readonly IContentSearchConfigurationSettings contentSearchSettings;
        private ISettings settings;

        //Added ILinqToIndexFactory linkLinqToIndexFactory to avoid build errors. This might fails as well.
        public SolrSearchWithSpatialContext(SolrSearchIndex index, ILinqToIndexFactory linkLinqToIndexFactory, SearchSecurityOptions options = SearchSecurityOptions.EnableSecurityCheck)
            : base(index, linkLinqToIndexFactory, options)
        {
            Assert.ArgumentNotNull((object)index, "index");
            Assert.ArgumentNotNull((object)options, "options");
            this.index = index;
            this.contentSearchSettings = this.index.Locator.GetInstance<IContentSearchConfigurationSettings>();
            this.settings = this.index.Locator.GetInstance<ISettings>();
            this.securityOptions = options;
        }

        public new IQueryable<TItem> GetQueryable<TItem>()
        {
            return this.GetQueryable<TItem>(new IExecutionContext[0]);
        }

        public new IQueryable<TItem> GetQueryable<TItem>(IExecutionContext executionContext)
        {
            return this.GetQueryable<TItem>(new IExecutionContext[1]
              {
                executionContext
              });
        }

        public new IQueryable<TItem> GetQueryable<TItem>(params IExecutionContext[] executionContexts)
        {
            QueryOptimizer<SolrQueryOptimizerState> queryOptimizer = new SpatialSolrQueryOptimizer();
            IIndexValueFormatter indexFieldFormatter = new SolrIndexFieldStorageValueFormatter();
            IQueryableFactory queryableFactory = new DefaultQueryableFactory();

            IFieldQueryTranslatorMap<IFieldQueryTranslator> iFieldQueryTranslatorMap = new VirtualFieldProcessorMap();

            var indexParameters = new SolrIndexParameters(indexFieldFormatter, iFieldQueryTranslatorMap,
                new FieldNameTranslator(), executionContexts.First());

            QueryMapper<SolrCompositeQuery> queryMapper = new SolrSpatialQueryMapper(indexParameters);
            //This might fail.
            IExpressionParser expressionParser = new ExtendedExpressionParser(typeof(TItem),typeof(TItem) ,new FieldNameTranslator());
            
            var linqToSolrIndex = new LinqToSolrIndexWithSpatial<TItem>(this, executionContexts, queryOptimizer, queryMapper, indexFieldFormatter, queryableFactory, expressionParser);
            if (this.contentSearchSettings.EnableSearchDebug())
                ((IHasTraceWriter)linqToSolrIndex).TraceWriter = new LoggingTraceWriter(SearchLog.Log);

            var queryable = linqToSolrIndex.GetQueryable();
            if (typeof(TItem).IsAssignableFrom(typeof(SearchResultItem)))
            {
                var globalFiltersArgs = new QueryGlobalFiltersArgs(linqToSolrIndex.GetQueryable(), typeof(TItem), executionContexts.ToList());
                //changed Sitecore.Abstractions.ICorePipeline to Sitecore.Abstractions.BaseCorePipelineManager as it is deprecated.
                this.Index.Locator.GetInstance<Sitecore.Abstractions.BaseCorePipelineManager>().Run("contentSearch.getGlobalLinqFilters", globalFiltersArgs);
                queryable = (IQueryable<TItem>)globalFiltersArgs.Query;
            }
            return queryable;
        }
    }
}