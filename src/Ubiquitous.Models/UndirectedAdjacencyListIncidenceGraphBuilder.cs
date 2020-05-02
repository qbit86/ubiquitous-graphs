﻿namespace Ubiquitous.Models
{
    using System;
    using System.Buffers;
    using static System.Diagnostics.Debug;

#pragma warning disable CA1815 // Override equals and operator equals on value types
    public struct UndirectedAdjacencyListIncidenceGraphBuilder :
        IGraphBuilder<UndirectedAdjacencyListIncidenceGraph, int, int>
    {
        private const int DefaultInitialOutDegree = 8;

        private int _initialOutDegree;
        private ArrayBuilder<int> _sources;
        private ArrayBuilder<int> _targets;
        private ArrayPrefix<ArrayBuilder<int>> _outEdges;

        public UndirectedAdjacencyListIncidenceGraphBuilder(int initialVertexCount, int edgeCapacity = 0)
        {
            if (initialVertexCount < 0)
                throw new ArgumentOutOfRangeException(nameof(initialVertexCount));

            if (edgeCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(edgeCapacity));

            _initialOutDegree = DefaultInitialOutDegree;
            int effectiveEdgeCapacity = Math.Max(2 * edgeCapacity, DefaultInitialOutDegree);
            _sources = new ArrayBuilder<int>(effectiveEdgeCapacity);
            _targets = new ArrayBuilder<int>(effectiveEdgeCapacity);
            ArrayBuilder<int>[] outEdges = Pool.Rent(initialVertexCount);
            Array.Clear(outEdges, 0, initialVertexCount);
            _outEdges = ArrayPrefix.Create(outEdges, initialVertexCount);
        }

        private static ArrayPool<ArrayBuilder<int>> Pool => ArrayPool<ArrayBuilder<int>>.Shared;

        public int VertexCount => _outEdges.Count;

        public int InitialOutDegree
        {
            get => _initialOutDegree <= 0 ? DefaultInitialOutDegree : _initialOutDegree;
            set => _initialOutDegree = 2 * value;
        }

        public void EnsureVertexCount(int vertexCount)
        {
            if (vertexCount > VertexCount)
                _outEdges = ArrayPrefixBuilder.Resize(_outEdges, vertexCount, true);
        }

        public bool TryAdd(int source, int target, out int edge)
        {
            if (source < 0)
            {
                edge = default;
                return false;
            }

            if (target < 0)
            {
                edge = default;
                return false;
            }

            int max = Math.Max(source, target);
            EnsureVertexCount(max + 1);

            Assert(_sources.Count == _targets.Count);
            _sources.Add(source);
            _targets.Add(target);

            if (_outEdges[source].Buffer == null)
                _outEdges[source] = new ArrayBuilder<int>(InitialOutDegree);

            int newEdgeIndex = _targets.Count;
            _outEdges.Array[source].Add(newEdgeIndex);
            _outEdges.Array[target].Add(~newEdgeIndex);

            edge = newEdgeIndex;
            return true;
        }

        public UndirectedAdjacencyListIncidenceGraph ToGraph() => throw new NotImplementedException();
    }
#pragma warning restore CA1815 // Override equals and operator equals on value types
}
