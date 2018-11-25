namespace Ubiquitous
{
    using System;
    using static System.Diagnostics.Debug;

    public struct AdjacencyListIncidenceGraphBuilder : IGraphBuilder<AdjacencyListIncidenceGraph, int, int>
    {
        private ArrayBuilder<int> _sources;
        private ArrayBuilder<int> _targets;

        public AdjacencyListIncidenceGraphBuilder(int vertexUpperBound) : this(vertexUpperBound, 0)
        {
        }

        public AdjacencyListIncidenceGraphBuilder(int vertexUpperBound, int edgeCount)
        {
            if (vertexUpperBound < 0)
                throw new ArgumentOutOfRangeException(nameof(vertexUpperBound));

            if (edgeCount < 0)
                throw new ArgumentOutOfRangeException(nameof(edgeCount));

            _sources = new ArrayBuilder<int>(edgeCount);
            _targets = new ArrayBuilder<int>(edgeCount);
            VertexUpperBound = vertexUpperBound;
            OutEdges = new ArrayBuilder<int>[vertexUpperBound];
        }

        public int VertexUpperBound { get; set; }

        private ArrayBuilder<int>[] OutEdges { get; set; }

        public bool TryAdd(int source, int target, out int edge)
        {
            if (OutEdges == null)
            {
                edge = int.MinValue;
                return false;
            }

            if ((uint)source >= (uint)VertexUpperBound)
            {
                edge = -1;
                return false;
            }

            if ((uint)target >= (uint)VertexUpperBound)
            {
                edge = -2;
                return false;
            }

            Assert(_sources.Count == _targets.Count);
            int newEdgeIndex = _targets.Count;
            _sources.Add(source);
            _targets.Add(target);

            if (OutEdges[source].Buffer == null)
                OutEdges[source] = new ArrayBuilder<int>(1);

            OutEdges[source].Add(newEdgeIndex);

            edge = newEdgeIndex;
            return true;
        }

        // Storage layout:
        // vertexUpperBound    reorderedEdges     sources
        //              ↓↓↓             ↓↓↓↓↓     ↓↓↓↓↓
        //              [4][_^|_^|_^|_^][021][bcb][aca]
        //                 ↑↑↑↑↑↑↑↑↑↑↑↑↑     ↑↑↑↑↑
        //                    edgeBounds     targets

        public AdjacencyListIncidenceGraph ToGraph()
        {
            Assert(_sources.Count == _targets.Count);
            var storage = new int[1 + 2 * VertexUpperBound + _sources.Count + _targets.Count + _sources.Count];

            storage[0] = VertexUpperBound;

            ReadOnlySpan<ArrayBuilder<int>> outEdges = OutEdges.AsSpan();
            Span<int> destEdgeBounds = storage.AsSpan(1, 2 * VertexUpperBound);
            Span<int> destReorderedEdges = storage.AsSpan(1 + 2 * VertexUpperBound, _sources.Count);

            for (int s = 0, currentBound = 0; s != outEdges.Length; ++s)
            {
                ReadOnlySpan<int> currentOutEdges = outEdges[s].AsSpan();
                currentOutEdges.CopyTo(destReorderedEdges.Slice(currentBound, currentOutEdges.Length));
                int finalLeftBound = 1 + 2 * VertexUpperBound + currentBound;
                destEdgeBounds[2 * s] = finalLeftBound;
                destEdgeBounds[2 * s + 1] = finalLeftBound + currentOutEdges.Length;
                currentBound += currentOutEdges.Length;
            }

            Span<int> destTargets = storage.AsSpan(1 + 2 * VertexUpperBound + _sources.Count, _targets.Count);
            _targets.AsSpan().CopyTo(destTargets);

            Span<int> destSources = storage.AsSpan(1 + 2 * VertexUpperBound + _sources.Count + _targets.Count,
                _sources.Count);
            _sources.AsSpan().CopyTo(destSources);

            _sources = default;
            _targets = default;
            OutEdges = null;
            VertexUpperBound = 0;

            return new AdjacencyListIncidenceGraph(storage);
        }
    }
}
