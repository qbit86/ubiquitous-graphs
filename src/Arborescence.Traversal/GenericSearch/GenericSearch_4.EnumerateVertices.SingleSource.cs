﻿namespace Arborescence.Traversal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
#if DEBUG
    using Debug = System.Diagnostics.Debug;

#endif

    public readonly partial struct GenericSearch<TGraph, TVertex, TEdge, TEdgeEnumerator>
    {
        /// <summary>
        /// Enumerates vertices of the graph in an order specified by the fringe starting from the single source.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="source">The source.</param>
        /// <param name="fringe">The collection of discovered vertices which are not finished yet.</param>
        /// <param name="exploredSet">The set of explored vertices.</param>
        /// <typeparam name="TFringe">The type of the generic queue.</typeparam>
        /// <typeparam name="TExploredSet">The type of the set of explored vertices.</typeparam>
        /// <returns>An enumerator to enumerate the vertices of a search tree.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="graph"/> is <see langword="null"/>,
        /// or <paramref name="fringe"/> is <see langword="null"/>,
        /// or <paramref name="exploredSet"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="IProducerConsumerCollection{TVertex}.TryAdd"/> for <paramref name="fringe"/>
        /// returns <see langword="false"/>.
        /// </exception>
        public IEnumerator<TVertex> EnumerateVertices<TFringe, TExploredSet>(
            TGraph graph, TVertex source, TFringe fringe, TExploredSet exploredSet)
            where TFringe : IProducerConsumerCollection<TVertex>
            where TExploredSet : ISet<TVertex>
        {
            if (graph is null)
                ThrowHelper.ThrowArgumentNullException(nameof(graph));

            if (fringe is null)
                ThrowHelper.ThrowArgumentNullException(nameof(fringe));

            if (exploredSet is null)
                ThrowHelper.ThrowArgumentNullException(nameof(exploredSet));

            return EnumerateVerticesIterator(graph, source, fringe, exploredSet);
        }

        private static IEnumerator<TVertex> EnumerateVerticesIterator<TFringe, TExploredSet>(
            TGraph graph, TVertex source, TFringe fringe, TExploredSet exploredSet)
            where TFringe : IProducerConsumerCollection<TVertex>
            where TExploredSet : ISet<TVertex>
        {
            exploredSet.Add(source);
            yield return source;
            if (!fringe.TryAdd(source))
                throw new InvalidOperationException(nameof(fringe.TryAdd));

            while (fringe.TryTake(out TVertex u))
            {
#if DEBUG
                Debug.Assert(exploredSet.Contains(u));
#endif
                TEdgeEnumerator outEdges = graph.EnumerateOutEdges(u);
                try
                {
                    while (outEdges.MoveNext())
                    {
                        TEdge e = outEdges.Current;
                        if (!graph.TryGetHead(e, out TVertex v))
                            continue;

                        if (exploredSet.Contains(v))
                            continue;

                        exploredSet.Add(v);
                        yield return v;
                        if (!fringe.TryAdd(v))
                            throw new InvalidOperationException(nameof(fringe.TryAdd));
                    }
                }
                finally
                {
                    outEdges.Dispose();
                }
            }
        }
    }
}
