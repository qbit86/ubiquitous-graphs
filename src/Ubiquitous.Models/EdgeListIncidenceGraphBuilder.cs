namespace Ubiquitous
{
    using System;
    using System.Buffers;
    using static System.Diagnostics.Debug;

    public struct EdgeListIncidenceGraphBuilder : IGraphBuilder<EdgeListIncidenceGraph, int, SourceTargetPair<int>>
    {
        private const int DefaultInitialOutDegree = 4;

        private int _rawInitialOutDegree;
        private ArrayPrefix<ArrayBuilder<SourceTargetPair<int>>> _outEdges;
        private int _edgeCount;

        public EdgeListIncidenceGraphBuilder(int vertexUpperBound)
        {
            _rawInitialOutDegree = DefaultInitialOutDegree;
            ArrayBuilder<SourceTargetPair<int>>[] outEdges =
                ArrayPool<ArrayBuilder<SourceTargetPair<int>>>.Shared.Rent(vertexUpperBound);
            _outEdges = new ArrayPrefix<ArrayBuilder<SourceTargetPair<int>>>(outEdges, vertexUpperBound);
            _edgeCount = 0;
        }

        public int VertexUpperBound => _outEdges.Count;

        public int InitialOutDegree
        {
            get => _rawInitialOutDegree <= 0 ? DefaultInitialOutDegree : _rawInitialOutDegree;
            set => _rawInitialOutDegree = value;
        }

        public bool TryAdd(int source, int target, out SourceTargetPair<int> edge)
        {
            if (source < 0)
            {
                edge = SourceTargetPair.Create(-1, -1);
                return false;
            }

            if (target < 0)
            {
                edge = SourceTargetPair.Create(-2, -2);
                return false;
            }

            int max = Math.Max(source, target);
            if (max >= VertexUpperBound)
                EnsureCapacity(max + 1);

            if (_outEdges[source].Buffer == null)
                _outEdges[source] = new ArrayBuilder<SourceTargetPair<int>>(InitialOutDegree);

            edge = SourceTargetPair.Create(source, target);
            _outEdges[source].Add(edge);
            ++_edgeCount;

            return true;
        }

        // reorderedEdges
        //          ↓↓↓↓↓
        //          [aca][____]
        //          [bcb][^^^^]
        //               ↑↑↑↑↑↑
        //               edgeBounds

        public EdgeListIncidenceGraph ToGraph()
        {
            var storage = new SourceTargetPair<int>[_edgeCount + VertexUpperBound];
            Span<SourceTargetPair<int>> destReorderedEdges = storage.AsSpan(0, _edgeCount);
            Span<SourceTargetPair<int>> destEdgeBounds = storage.AsSpan(_edgeCount, VertexUpperBound);

            for (int s = 0, currentOffset = 0; s != VertexUpperBound; ++s)
            {
                ReadOnlySpan<SourceTargetPair<int>> currentOutEdges = _outEdges[s].AsSpan();
                Span<SourceTargetPair<int>> destOutEdges =
                    destReorderedEdges.Slice(currentOffset, currentOutEdges.Length);
                currentOutEdges.CopyTo(destOutEdges);
                int lowerBound = currentOffset;
                currentOffset += currentOutEdges.Length;
                int upperBound = currentOffset;
                destEdgeBounds[s] = SourceTargetPair.Create(lowerBound, upperBound);

                ArrayPool<SourceTargetPair<int>>.Shared.Return(_outEdges[s].Buffer, true);
            }

            ArrayPool<ArrayBuilder<SourceTargetPair<int>>>.Shared.Return(_outEdges.Array, true);

            _rawInitialOutDegree = DefaultInitialOutDegree;
            _outEdges = ArrayPrefix<ArrayBuilder<SourceTargetPair<int>>>.Empty;
            _edgeCount = 0;

            return new EdgeListIncidenceGraph(VertexUpperBound, storage);
        }

        private void EnsureCapacity(int newVertexUpperBound)
        {
            Assert(newVertexUpperBound > _outEdges.Count);

            if (newVertexUpperBound <= _outEdges.Array.Length)
            {
                Array.Clear(_outEdges.Array, _outEdges.Count, newVertexUpperBound - _outEdges.Count);
                _outEdges = new ArrayPrefix<ArrayBuilder<SourceTargetPair<int>>>(_outEdges.Array, newVertexUpperBound);
            }

            throw new NotImplementedException();
        }
    }
}
