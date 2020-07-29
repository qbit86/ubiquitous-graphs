namespace Arborescence.Models
{
    using System;
    using System.Diagnostics;

    public readonly partial struct UndirectedIndexedIncidenceGraph
    {
#pragma warning disable CA1034 // Nested types should not be visible
        /// <inheritdoc/>
        public sealed class Builder : IGraphBuilder<UndirectedIndexedIncidenceGraph, int, int>
        {
            private ArrayPrefix<int> _headByEdge;
            private ArrayPrefix<int> _tailByEdge;
            private int _vertexCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            /// <param name="initialVertexCount">The initial number of vertices.</param>
            /// <param name="edgeCapacity">The initial capacity of the internal backing storage for the edges.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <paramref name="initialVertexCount"/> is less than zero, or <paramref name="edgeCapacity"/> is less than zero.
            /// </exception>
            public Builder(int initialVertexCount, int edgeCapacity = 0)
            {
                if (initialVertexCount < 0)
                    throw new ArgumentOutOfRangeException(nameof(initialVertexCount));

                if (edgeCapacity < 0)
                    throw new ArgumentOutOfRangeException(nameof(edgeCapacity));

                _headByEdge = ArrayPrefixBuilder.Create<int>(edgeCapacity);
                _tailByEdge = ArrayPrefixBuilder.Create<int>(edgeCapacity);
                _vertexCount = initialVertexCount;
            }

            private static bool NeedsReordering { get; } = true;

            /// <inheritdoc/>
            public bool TryAdd(int tail, int head, out int edge)
            {
                if (tail < 0 || head < 0)
                {
                    edge = default;
                    return false;
                }

                edge = _tailByEdge.Count;

                int newVertexCountCandidate = Math.Max(tail, head) + 1;
                if (newVertexCountCandidate > _vertexCount)
                    _vertexCount = newVertexCountCandidate;

                Debug.Assert(_tailByEdge.Count == _headByEdge.Count, "_tailByEdge.Count == _headByEdge.Count");
                _tailByEdge = ArrayPrefixBuilder.Add(_tailByEdge, tail, false);
                _headByEdge = ArrayPrefixBuilder.Add(_headByEdge, head, false);
                return true;
            }

            /// <inheritdoc/>
            public UndirectedIndexedIncidenceGraph ToGraph()
            {
                int n = _vertexCount;
                int m = _tailByEdge.Count;
                Debug.Assert(_tailByEdge.Count == _headByEdge.Count, "_tailByEdge.Count == _headByEdge.Count");

                int dataLength = 2 + n + m + m + m + m;
#if NET5
                int[] data = GC.AllocateUninitializedArray<int>(dataLength);
#else
                var data = new int[dataLength];
#endif
                data[0] = n;
                data[1] = m;

                Span<int> destReorderedEdges = data.AsSpan(2 + n, m + m);
                for (int edge = 0; edge < m; ++edge)
                {
                    destReorderedEdges[edge] = edge;
                    destReorderedEdges[m + edge] = ~edge;
                }

#if false
                if (NeedsReordering)
                    Array.Sort(data, 2 + n, m + m, new EdgeComparer(_tailByEdge.Array));

                Span<int> destUpperBoundByVertex = data.AsSpan(2, n);
                destUpperBoundByVertex.Clear();
                for (int edge = 0; edge < m; ++edge)
                {
                    int tail = _tailByEdge[edge];
                    ++destUpperBoundByVertex[tail];
                }
                for (int edge = 0; edge < m; ++edge)
                {
                    int head = _headByEdge[edge];
                    ++destUpperBoundByVertex[head];
                }

                for (int vertex = 1; vertex < n; ++vertex)
                    destUpperBoundByVertex[vertex] += destUpperBoundByVertex[vertex - 1];

                Span<int> destHeadByEdge = data.AsSpan(2 + n + m, m);
                _headByEdge.AsSpan().CopyTo(destHeadByEdge);

                Span<int> destTailByEdge = data.AsSpan(2 + n + m + m, m);
                _tailByEdge.AsSpan().CopyTo(destTailByEdge);

                _currentMaxTail = 0;
                _headByEdge = ArrayPrefixBuilder.Release(_headByEdge, false);
                _tailByEdge = ArrayPrefixBuilder.Release(_tailByEdge, false);
                _vertexCount = 0;

                return new UndirectedIndexedIncidenceGraph(data);
#else
                throw new NotImplementedException();
#endif
            }
        }
#pragma warning restore CA1034 // Nested types should not be visible
    }
}
