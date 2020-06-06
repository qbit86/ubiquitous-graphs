namespace Ubiquitous.Models
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct IndexedBidirectionalGraphPolicy<TGraph, TEdges> :
        IGetSourcePolicy<TGraph, int, int>,
        IGetTargetPolicy<TGraph, int, int>,
        IOutEdgesPolicy<TGraph, int, TEdges>,
        IInEdgesPolicy<TGraph, int, TEdges>
        where TGraph : IBidirectionalGraph<int, int, TEdges>
    {
        public bool TryGetTail(TGraph graph, int edge, out int source) => graph.TryGetTail(edge, out source);

        public bool TryGetHead(TGraph graph, int edge, out int target) => graph.TryGetHead(edge, out target);

        public TEdges EnumerateOutEdges(TGraph graph, int vertex) => graph.EnumerateOutEdges(vertex);

        public TEdges EnumerateInEdges(TGraph graph, int vertex) => graph.EnumerateInEdges(vertex);
    }
#pragma warning restore CA1815 // Override equals and operator equals on value types
}
