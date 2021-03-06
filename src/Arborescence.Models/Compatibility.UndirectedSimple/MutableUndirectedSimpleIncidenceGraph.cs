﻿namespace Arborescence.Models.Compatibility
{
    using System;
    using System.Diagnostics;

    /// <inheritdoc cref="Arborescence.IIncidenceGraph{TVertex, TEdge, TEdges}"/>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET5_0_OR_GREATER
    [Obsolete("Please use Arborescence.Models.MutableUndirectedSimpleIncidenceGraph instead.")]
#endif
    public sealed class MutableUndirectedSimpleIncidenceGraph :
        IIncidenceGraph<int, Endpoints, ArrayPrefixEnumerator<Endpoints>>,
        IGraphBuilder<SimpleIncidenceGraph, int, Endpoints>,
        IDisposable
    {
        private const int DefaultInitialOutDegree = 8;

        private int _edgeCount;
        private int _initialOutDegree = DefaultInitialOutDegree;
        private int _invertedEdgeCount;
        private ArrayPrefix<ArrayPrefix<Endpoints>> _outEdgesByVertex;

        /// <summary>
        /// Initializes a new instance of the <see cref="MutableUndirectedSimpleIncidenceGraph"/> class.
        /// </summary>
        /// <param name="initialVertexCount">The initial number of vertices.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="initialVertexCount"/> is less than zero.
        /// </exception>
        public MutableUndirectedSimpleIncidenceGraph(int initialVertexCount = 0)
        {
            if (initialVertexCount < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(initialVertexCount));

            _outEdgesByVertex = ArrayPrefixBuilder.EnsureSize(_outEdgesByVertex, initialVertexCount, true);
        }

        /// <summary>
        /// Gets the number of vertices.
        /// </summary>
        public int VertexCount => _outEdgesByVertex.Count;

        /// <summary>
        /// Gets the number of edges.
        /// </summary>
        public int EdgeCount => _edgeCount;

        /// <summary>
        /// Gets the initial number of out-edges for each vertex.
        /// </summary>
        public int InitialOutDegree
        {
            get => _initialOutDegree <= 0 ? DefaultInitialOutDegree : _initialOutDegree;
            set => _initialOutDegree = value;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _edgeCount = 0;
            _invertedEdgeCount = 0;
            for (int vertex = 0; vertex < _outEdgesByVertex.Count; ++vertex)
                _outEdgesByVertex[vertex] = ArrayPrefixBuilder.Release(_outEdgesByVertex[vertex], false);
            _outEdgesByVertex = ArrayPrefixBuilder.Release(_outEdgesByVertex, true);
        }

        /// <summary>
        /// Attempts to add the edge with the specified endpoints to the graph.
        /// </summary>
        /// <param name="tail">The tail of the edge.</param>
        /// <param name="head">The head of the edge.</param>
        /// <param name="edge">
        /// When this method returns, contains the added edge, if the edge was added to the graph successfully;
        /// otherwise, the unspecified value.
        /// </param>
        /// <returns>
        /// A value indicating whether the edge was added successfully.
        /// <c>true</c> if both <paramref name="tail"/> and <paramref name="head"/> are non-negative;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool TryAdd(int tail, int head, out Endpoints edge)
        {
            bool result = tail >= 0 && head >= 0;
            edge = result ? UncheckedAdd(tail, head) : default;
            return result;
        }

        /// <inheritdoc/>
        public SimpleIncidenceGraph ToGraph()
        {
            int n = VertexCount;

            int dataLength = 2 + n;
#if NET5_0_OR_GREATER
            int[] data = GC.AllocateUninitializedArray<int>(dataLength);
            Endpoints[] edgesOrderedByTail = GC.AllocateUninitializedArray<Endpoints>(EdgeCount + _invertedEdgeCount);
#else
            int[] data = new int[dataLength];
            var edgesOrderedByTail = new Endpoints[EdgeCount + _invertedEdgeCount];
#endif
            data[0] = n;
            data[1] = EdgeCount;

            Span<int> destUpperBoundByVertex = data.AsSpan(2);
            for (int vertex = 0; vertex < n; ++vertex)
            {
                ReadOnlySpan<Endpoints> currentOutEdges = _outEdgesByVertex[vertex].AsSpan();
                int currentLowerBound = vertex > 0 ? destUpperBoundByVertex[vertex - 1] : 0;
                Span<Endpoints> destCurrentOutEdges =
                    edgesOrderedByTail.AsSpan(currentLowerBound, currentOutEdges.Length);
                currentOutEdges.CopyTo(destCurrentOutEdges);
                destUpperBoundByVertex[vertex] = currentLowerBound + currentOutEdges.Length;
            }

            return new SimpleIncidenceGraph(data, edgesOrderedByTail);
        }

        /// <inheritdoc/>
        public bool TryGetHead(Endpoints edge, out int head)
        {
            head = edge.Head;
            return unchecked((uint)head < (uint)VertexCount);
        }

        /// <inheritdoc/>
        public bool TryGetTail(Endpoints edge, out int tail)
        {
            tail = edge.Tail;
            return unchecked((uint)tail < (uint)VertexCount);
        }

        /// <inheritdoc/>
        public ArrayPrefixEnumerator<Endpoints> EnumerateOutEdges(int vertex)
        {
            if (unchecked((uint)vertex >= (uint)_outEdgesByVertex.Count))
                return ArrayPrefixEnumerator<Endpoints>.Empty;

            ArrayPrefix<Endpoints> outEdges = _outEdgesByVertex[vertex];
            return new ArrayPrefixEnumerator<Endpoints>(outEdges.Array ?? Array.Empty<Endpoints>(), outEdges.Count);
        }

        /// <summary>
        /// Ensures that the graph can hold the specified number of vertices without growing.
        /// </summary>
        /// <param name="vertexCount">The number of vertices.</param>
        public void EnsureVertexCount(int vertexCount)
        {
            if (vertexCount > VertexCount)
                _outEdgesByVertex = ArrayPrefixBuilder.EnsureSize(_outEdgesByVertex, vertexCount, true);
        }

        /// <summary>
        /// Adds the edge with the specified endpoints to the graph.
        /// </summary>
        /// <param name="tail">The tail of the edge.</param>
        /// <param name="head">The head of the edge.</param>
        /// <returns>The added edge.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="tail"/> is less than zero, or <paramref name="head"/> is less than zero.
        /// </exception>
        public Endpoints Add(int tail, int head)
        {
            if (tail < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(tail));

            if (head < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(head));

            return UncheckedAdd(tail, head);
        }

        private Endpoints UncheckedAdd(int tail, int head)
        {
            Debug.Assert(tail >= 0, "tail >= 0");
            Debug.Assert(head >= 0, "head >= 0");

            int newVertexCountCandidate = Math.Max(tail, head) + 1;
            EnsureVertexCount(newVertexCountCandidate);

            if (_outEdgesByVertex[tail].Array is null)
                _outEdgesByVertex[tail] = ArrayPrefixBuilder.Create<Endpoints>(InitialOutDegree);

            var edge = new Endpoints(tail, head);
            _outEdgesByVertex[tail] = ArrayPrefixBuilder.Add(_outEdgesByVertex[tail], edge, false);
            ++_edgeCount;

            if (tail != head)
            {
                if (_outEdgesByVertex[head].Array is null)
                    _outEdgesByVertex[head] = ArrayPrefixBuilder.Create<Endpoints>(InitialOutDegree);

                var invertedEdge = new Endpoints(head, tail);
                _outEdgesByVertex[head] = ArrayPrefixBuilder.Add(_outEdgesByVertex[head], invertedEdge, false);
                ++_invertedEdgeCount;
            }

            return edge;
        }
    }
}
