﻿namespace Arborescence.Traversal
{
    // https://www.boost.org/doc/libs/1_73_0/libs/graph/doc/BFSVisitor.html

    /// <summary>
    /// Defines callbacks to be invoked while traversing a graph in a BFS manner.
    /// </summary>
    /// <typeparam name="TGraph">The type of the graph.</typeparam>
    /// <typeparam name="TVertex">The type of the vertex.</typeparam>
    /// <typeparam name="TEdge">The type of the edge.</typeparam>
    public interface IBfsHandler<in TGraph, in TVertex, in TEdge>
    {
        /// <summary>
        /// This is invoked when a vertex is encountered for the first time.
        /// </summary>
        /// <param name="g">The graph.</param>
        /// <param name="v">The vertex.</param>
        void OnDiscoverVertex(TGraph g, TVertex v);

        /// <summary>
        /// This is invoked on a vertex as it is taken from the fringe.
        /// This happens immediately before <see cref="OnExamineEdge"/> is invoked
        /// on each of the out-edges of vertex <paramref name="v"/>.
        /// </summary>
        /// <param name="g">The graph.</param>
        /// <param name="v">The vertex.</param>
        void OnExamineVertex(TGraph g, TVertex v);

        /// <summary>
        /// This is invoked on a vertex after all of its out-edges have been added to the search tree
        /// and all of the adjacent vertices have been discovered
        /// (but before the out-edges of the adjacent vertices have been examined).
        /// </summary>
        /// <param name="g">The graph.</param>
        /// <param name="v">The vertex.</param>
        void OnFinishVertex(TGraph g, TVertex v);

        void OnExamineEdge(TGraph g, TEdge e);
        void OnTreeEdge(TGraph g, TEdge e);
        void OnNonTreeGrayHeadEdge(TGraph g, TEdge e);
        void OnNonTreeBlackHeadEdge(TGraph g, TEdge e);
    }
}
