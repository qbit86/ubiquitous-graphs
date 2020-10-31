namespace Arborescence.Traversal
{
    using System.Collections.Generic;

    public readonly partial struct ReverseDfs<
        TGraph, TVertex, TEdge, TEdgeEnumerator, TExploredSet, TGraphPolicy, TExploredSetPolicy>
    {
        /// <summary>
        /// Enumerates edges of the graph in a depth-first order starting from the single source.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="source">The source.</param>
        /// <param name="exploredSet">The set of explored vertices.</param>
        /// <returns>An enumerator to enumerate the edges of the the graph.</returns>
        public IEnumerator<TEdge> EnumerateEdges(TGraph graph, TVertex source, TExploredSet exploredSet)
        {
            var stack = new Internal.Stack<EdgeInfo<TVertex, TEdge>>();
            try
            {
                stack.Add(new EdgeInfo<TVertex, TEdge>(source));

                while (stack.TryTake(out EdgeInfo<TVertex, TEdge> stackFrame))
                {
                    TVertex u = stackFrame.ExploredVertex;
                    if (ExploredSetPolicy.Contains(exploredSet, u))
                        continue;

                    if (stackFrame.TryGetInEdge(out TEdge inEdge))
                        yield return inEdge;
                    ExploredSetPolicy.Add(exploredSet, u);

                    TEdgeEnumerator outEdges = GraphPolicy.EnumerateOutEdges(graph, u);
                    while (outEdges.MoveNext())
                    {
                        TEdge e = outEdges.Current;
                        if (!GraphPolicy.TryGetHead(graph, e, out TVertex v))
                            continue;

                        stack.Add(new EdgeInfo<TVertex, TEdge>(v, e));
                    }
                }
            }
            finally
            {
                stack.Dispose();
            }
        }
    }
}
