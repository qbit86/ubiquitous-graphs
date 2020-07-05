namespace Arborescence.Models
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct EdgeBidirectionalGraphPolicy<TGraph, TEdges> :
        ITailPolicy<TGraph, int, Endpoints<int>>,
        IHeadPolicy<TGraph, int, Endpoints<int>>,
        IOutEdgesPolicy<TGraph, int, TEdges>,
        IInEdgesPolicy<TGraph, int, TEdges>
        where TGraph : IBidirectionalGraph<int, Endpoints<int>, TEdges>
    {
        public bool TryGetTail(TGraph graph, Endpoints<int> edge, out int tail) =>
            graph.TryGetTail(edge, out tail);

        public bool TryGetHead(TGraph graph, Endpoints<int> edge, out int head) =>
            graph.TryGetHead(edge, out head);

        public TEdges EnumerateOutEdges(TGraph graph, int vertex) => graph.EnumerateOutEdges(vertex);

        public TEdges EnumerateInEdges(TGraph graph, int vertex) => graph.EnumerateInEdges(vertex);
    }
#pragma warning restore CA1815 // Override equals and operator equals on value types
}