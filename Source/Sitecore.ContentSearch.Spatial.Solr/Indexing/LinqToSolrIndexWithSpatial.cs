using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Factories;
using Sitecore.ContentSearch.Linq.Parsing;
using Sitecore.ContentSearch.Linq.Solr;
using Sitecore.ContentSearch.SolrProvider;
using Sitecore.ContentSearch.Spatial.Solr.Parsing;

namespace Sitecore.ContentSearch.Spatial.Solr.Indexing
{
    public class LinqToSolrIndexWithSpatial<TItem> : LinqToSolrIndex<TItem>
    {

        private readonly QueryMapper<SolrCompositeQuery> queryMapper;
        private readonly QueryOptimizer<SolrQueryOptimizerState> queryOptimizer;
        private readonly ExpressionParser expressionParser;

        public LinqToSolrIndexWithSpatial(SolrSearchContext context, IExecutionContext executionContext, QueryOptimizer<SolrQueryOptimizerState> queryOptimizer, QueryMapper<SolrCompositeQuery> queryMapper, IIndexValueFormatter indexFieldFormatter, IQueryableFactory queryableFactory, IExpressionParser expressionParser)
            : this(context, new IExecutionContext[] { executionContext }, queryOptimizer, queryMapper, indexFieldFormatter, queryableFactory, expressionParser)
        {

        }

        public LinqToSolrIndexWithSpatial(SolrSearchContext context, IExecutionContext[] executionContexts, QueryOptimizer<SolrQueryOptimizerState> queryOptimizer, QueryMapper<SolrCompositeQuery> queryMapper, IIndexValueFormatter indexFieldFormatter, IQueryableFactory queryableFactory, IExpressionParser expressionParser) : base(context, executionContexts, queryOptimizer, queryMapper, indexFieldFormatter, queryableFactory, expressionParser)
        {
            var parameters =
                new SolrIndexParameters(
                     context.Index.Configuration.IndexFieldStorageValueFormatter,
                    context.Index.Configuration.VirtualFields,
                     context.Index.FieldNameTranslator, executionContexts,
                     context.Index.Configuration.FieldMap);
            this.queryMapper = new SolrSpatialQueryMapper(parameters);
            this.queryOptimizer = new SpatialSolrQueryOptimizer();
        }

        // called from the base class using reflection!!!!
        private TResult ApplyScalarMethods<TResult, TDocument>(SolrCompositeQuery compositeQuery,
                                                               object processedResults,
                                                               object results)
        {
            var type = typeof(LinqToSolrIndex<>).MakeGenericType(typeof(TItem));
            MethodInfo baseMethod = type.GetMethod("ApplyScalarMethods", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(typeof(TResult), typeof(TDocument));

            var ret = baseMethod.Invoke(this, new object[] { compositeQuery, processedResults, results });
            return (TResult)ret;
        }

        protected override QueryMapper<SolrCompositeQuery> QueryMapper
        {
            get { return this.queryMapper; }
        }

        protected override IQueryOptimizer QueryOptimizer
        {
            get { return this.queryOptimizer; }
        }


        public override IQueryable<TItem> GetQueryable()
        {
            IQueryable<TItem> queryable = new ExtendedGenericQueryable<TItem, SolrCompositeQuery>(this, this.QueryMapper, this.QueryOptimizer, this.FieldNameTranslator, this.expressionParser);
            (queryable as IHasTraceWriter).TraceWriter = ((IHasTraceWriter)this).TraceWriter;
            foreach (IPredefinedQueryAttribute predefinedQueryAttribute in Enumerable.ToList(Enumerable.SelectMany(GetTypeInheritance(typeof(TItem)), t => Enumerable.Cast<IPredefinedQueryAttribute>(t.GetCustomAttributes(typeof(IPredefinedQueryAttribute), true)))))
                queryable = predefinedQueryAttribute.ApplyFilter<TItem>(queryable, this.ValueFormatter);
            return queryable;
        }

        private IEnumerable<Type> GetTypeInheritance(Type type)
        {
            yield return type;
            for (Type baseType = type.BaseType; baseType != (Type)null; baseType = baseType.BaseType)
                yield return baseType;
        }
    }
}