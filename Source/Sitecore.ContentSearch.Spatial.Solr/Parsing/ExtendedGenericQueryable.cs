using System;
using System.Linq;
using System.Linq.Expressions;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Indexing;
using Sitecore.ContentSearch.Linq.Parsing;
using Sitecore.ContentSearch.Spatial.Solr.Common;

namespace Sitecore.ContentSearch.Spatial.Solr.Parsing
{
    public class ExtendedGenericQueryable<TElement, TQuery> : GenericQueryable<TElement, TQuery>
    {
        //Added IExpressionParser parser to solve build errors.
        public ExtendedGenericQueryable(Index<TElement, TQuery> index, QueryMapper<TQuery> queryMapper, IQueryOptimizer queryOptimizer, FieldNameTranslator fieldNameTranslator, IExpressionParser parser) : 
            base(index, queryMapper, queryOptimizer, fieldNameTranslator, parser)
        {
        }

        //Added IExpressionParser parser to solve build errors.
        protected ExtendedGenericQueryable(Index<TQuery> index, QueryMapper<TQuery> queryMapper, IQueryOptimizer queryOptimizer, Expression expression, Type itemType, FieldNameTranslator fieldNameTranslator,IExpressionParser parser) : 
            base(index, queryMapper, queryOptimizer, expression, itemType, fieldNameTranslator,parser)
        {
        }

        //Added this.Parser to solve build errors.
        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var genericQueryable = new ExtendedGenericQueryable<TElement, TQuery>(this.Index, this.QueryMapper, this.QueryOptimizer, expression, this.ItemType, this.FieldNameTranslator, this.Parser);
            ((IHasTraceWriter)genericQueryable).TraceWriter = ((IHasTraceWriter)this).TraceWriter;
            return genericQueryable;
        }
        
        protected override TQuery GetQuery(Expression expression)
        {
            

            this.Trace(expression, "Expression");
            IndexQuery indexQuery = new ExtendedExpressionParser(typeof(TElement), this.ItemType, this.FieldNameTranslator).Parse(expression);
            this.Trace(indexQuery, "Raw query:");
            IndexQuery optimizedQuery = this.QueryOptimizer.Optimize(indexQuery);
            this.Trace(optimizedQuery, "Optimized query:");
            TQuery nativeQuery = this.QueryMapper.MapQuery(optimizedQuery);
            this.Trace(new GenericDumpable((object)nativeQuery), "Native query:");
            return nativeQuery;
        }
    }
}