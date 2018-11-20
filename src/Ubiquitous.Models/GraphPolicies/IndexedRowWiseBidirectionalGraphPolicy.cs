namespace Ubiquitous
{
    public readonly struct IndexedRowWiseBidirectionalGraphPolicy<TGraph, TEdges> :
        IGetEndpointsPolicy<TGraph, int, int>,
        IGetOutEdgesPolicy<TGraph, int, TEdges>,
        IGetInEdgesPolicy<TGraph, int, TEdges>
        where TGraph : IGetEndpoints<int, int>, IGetOutEdges<int, TEdges>, IGetInEdges<int, TEdges>
    {
        public bool TryGetEndpoints(TGraph graph, int edge, out SourceTargetPair<int> endpoints)
        {
            return graph.TryGetEndpoints(edge, out endpoints);
        }

        public bool TryGetOutEdges(TGraph graph, int vertex, out TEdges edges)
        {
            return graph.TryGetOutEdges(vertex, out edges);
        }

        public bool TryGetInEdges(TGraph graph, int vertex, out TEdges edges)
        {
            return graph.TryGetInEdges(vertex, out edges);
        }
    }
}
