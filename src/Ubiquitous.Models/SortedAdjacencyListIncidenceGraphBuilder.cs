namespace Ubiquitous.Models
{
    using System;
    using System.Buffers;
    using static System.Diagnostics.Debug;

    public struct SortedAdjacencyListIncidenceGraphBuilder : IGraphBuilder<SortedAdjacencyListIncidenceGraph, int, int>
    {
        private ArrayBuilder<int> _orderedSources;
        private ArrayBuilder<int> _targets;
        private int _lastSource;

        public SortedAdjacencyListIncidenceGraphBuilder(int vertexUpperBound) : this(vertexUpperBound, 0)
        {
        }

        public SortedAdjacencyListIncidenceGraphBuilder(int vertexUpperBound, int edgeCount)
        {
            if (vertexUpperBound < 0)
                throw new ArgumentOutOfRangeException(nameof(vertexUpperBound));

            if (edgeCount < 0)
                throw new ArgumentOutOfRangeException(nameof(edgeCount));

            _orderedSources = new ArrayBuilder<int>(edgeCount);
            _targets = new ArrayBuilder<int>(edgeCount);
            _lastSource = 0;
            EdgeUpperBounds = new int[vertexUpperBound];
        }

        public int VertexUpperBound => EdgeUpperBounds?.Length ?? 0;

        private int[] EdgeUpperBounds { get; set; }

        public bool TryAdd(int source, int target, out int edge)
        {
            if (EdgeUpperBounds == null)
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

            if (source < _lastSource)
            {
                edge = sbyte.MinValue;
                return false;
            }

            Assert(_orderedSources.Count == _targets.Count);
            int newEdgeIndex = _targets.Count;
            _orderedSources.Add(source);
            _targets.Add(target);

            EdgeUpperBounds[source] = newEdgeIndex + 1;
            _lastSource = source;

            edge = newEdgeIndex;
            return true;
        }

        // Storage layout:
        // vertexUpperBound      targets
        //         ↓↓↓      ↓↓↓↓↓
        //         [4][^^^^][bbc][aac]
        //            ↑↑↑↑↑↑     ↑↑↑↑↑
        //   edgeUpperBounds     orderedSources

        public SortedAdjacencyListIncidenceGraph ToGraph()
        {
            Assert(_orderedSources.Count == _targets.Count);
            var storage = new int[1 + VertexUpperBound + _targets.Count + _orderedSources.Count];
            storage[0] = VertexUpperBound;

            ReadOnlySpan<int> targetsBuffer = _targets.AsSpan();
            targetsBuffer.CopyTo(storage.AsSpan(1 + VertexUpperBound, _targets.Count));
            ArrayPool<int>.Shared.Return(_targets.Buffer);
            _targets = default;

            ReadOnlySpan<int> orderedSourcesBuffer = _orderedSources.AsSpan();
            orderedSourcesBuffer.CopyTo(storage.AsSpan(1 + VertexUpperBound + _targets.Count, _orderedSources.Count));
            ArrayPool<int>.Shared.Return(_orderedSources.Buffer);
            _orderedSources = default;

            // Make EdgeUpperBounds monotonic in case if we skipped some sources.
            for (int v = 1; v < EdgeUpperBounds.Length; ++v)
            {
                if (EdgeUpperBounds[v] < EdgeUpperBounds[v - 1])
                    EdgeUpperBounds[v] = EdgeUpperBounds[v - 1];
            }

            EdgeUpperBounds.CopyTo(storage.AsSpan(1, VertexUpperBound));
            EdgeUpperBounds = null;

            _lastSource = 0;

            return new SortedAdjacencyListIncidenceGraph(storage);
        }
    }
}
