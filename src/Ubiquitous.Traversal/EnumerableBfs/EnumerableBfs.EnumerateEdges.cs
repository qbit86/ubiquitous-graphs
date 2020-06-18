namespace Ubiquitous.Traversal
{
    using System.Collections.Generic;

    // ReSharper disable UnusedTypeParameter
    public readonly partial struct EnumerableBfs<
        TGraph, TVertex, TEdge, TEdgeEnumerator, TExploredSet, TGraphPolicy, TExploredSetPolicy>
    {
        private IEnumerator<TEdge> EnumerateEdgesCore(
            TGraph graph, Internal.Queue<TVertex> queue, TExploredSet exploredSet)
        {
            try
            {
                while (queue.TryTake(out TVertex u))
                {
                    TEdgeEnumerator outEdges = GraphPolicy.EnumerateOutEdges(graph, u);
                    while (outEdges.MoveNext())
                    {
                        TEdge e = outEdges.Current;
                        if (!GraphPolicy.TryGetHead(graph, e, out TVertex v))
                            continue;

                        bool vExplored = ExploredSetPolicy.Contains(exploredSet, v);
                        if (!vExplored)
                        {
                            yield return e;
                            ExploredSetPolicy.Add(exploredSet, v);
                            queue.Add(v);
                        }
                    }
                }
            }
            finally
            {
                // The Dispose call will happen on the original value of the local if it is the argument to a using statement.
                queue.Dispose();
            }
        }
    }
    // ReSharper restore UnusedTypeParameter
}
