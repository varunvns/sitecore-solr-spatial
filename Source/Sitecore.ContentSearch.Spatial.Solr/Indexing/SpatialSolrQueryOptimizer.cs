//using Sitecore.ContentSearch.Linq.Nodes;
using Sitecore.ContentSearch.Linq.Solr;
using Sitecore.ContentSearch.Spatial.Solr.Nodes;

namespace Sitecore.ContentSearch.Spatial.Solr.Indexing
{
    public class SpatialSolrQueryOptimizer : SolrQueryOptimizer
    {
        protected override Sitecore.ContentSearch.Linq.Nodes.QueryNode Visit(Sitecore.ContentSearch.Linq.Nodes.QueryNode node, SolrQueryOptimizerState state)
        {
            if (node.NodeType == Sitecore.ContentSearch.Linq.Nodes.QueryNodeType.Custom)
            {
                if (node is WithinRadiusNode)
                {
                    return VisitWithinRadius((WithinRadiusNode)node, state);
                }
            }
           
            return base.Visit(node, state);
        }

        private Sitecore.ContentSearch.Linq.Nodes.QueryNode VisitWithinRadius(WithinRadiusNode radiusNode, SolrQueryOptimizerState state)
        {
            return new WithinRadiusNode(this.Visit(radiusNode.SourceNode, state), radiusNode.Field, radiusNode.Lat, radiusNode.Lon, radiusNode.Radius);
        }
    }
}