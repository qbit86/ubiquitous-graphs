namespace Arborescence.Traversal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public readonly partial struct EnumerableBfs<TGraph, TVertex, TEdge, TEdgeEnumerator>
    {
        public IEnumerator<TEdge> EnumerateEdges<TVertexEnumerator, TExploredSet>(
            TGraph graph, TVertexEnumerator sources, TExploredSet exploredSet)
            where TVertexEnumerator : IEnumerator<TVertex>
            where TExploredSet : ISet<TVertex>
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            var queue = new Internal.Queue<TVertex>();
            try
            {
                while (sources.MoveNext())
                {
                    TVertex source = sources.Current;
                    exploredSet.Add(source);
                    queue.Add(source);
                }

                while (queue.TryTake(out TVertex u))
                {
#if DEBUG
                    Debug.Assert(exploredSet.Contains(u));
#endif
                    TEdgeEnumerator outEdges = graph.EnumerateOutEdges(u);
                    while (outEdges.MoveNext())
                    {
                        TEdge e = outEdges.Current;
                        if (!graph.TryGetHead(e, out TVertex v))
                            continue;

                        if (exploredSet.Contains(v))
                            continue;

                        yield return e;
                        exploredSet.Add(v);
                        queue.Add(v);
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
}
