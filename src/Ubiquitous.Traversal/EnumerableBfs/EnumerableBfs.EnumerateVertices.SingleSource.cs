namespace Ubiquitous.Traversal
{
    using System.Collections.Generic;
    using Collections;
    using Internal;

    // ReSharper disable UnusedTypeParameter
    public readonly partial struct EnumerableBfs<TGraph, TVertex, TEdge, TEdgeEnumerator, TExploredSet, TGraphPolicy,
        TExploredSetPolicy>
    {
        public IEnumerator<TVertex> EnumerateVertices(TGraph graph, TVertex source, TExploredSet exploredSet)
        {
            Queue<TVertex> queue = QueueCache<TVertex>.Acquire();
            try
            {
                var queueAdapter = new QueueAdapter<TVertex>(queue);

                ExploredSetPolicy.Add(exploredSet, source);
                queueAdapter.Add(source);

                return EnumerateVerticesCore(graph, queueAdapter, exploredSet);
            }
            finally
            {
                QueueCache<TVertex>.Release(queue);
            }
        }
    }
    // ReSharper restore UnusedTypeParameter
}