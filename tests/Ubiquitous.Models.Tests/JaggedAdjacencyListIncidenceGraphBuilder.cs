﻿namespace Ubiquitous.Models
{
    using System;
    using System.Buffers;
    using static System.Diagnostics.Debug;

    public struct JaggedAdjacencyListIncidenceGraphBuilder : IGraphBuilder<JaggedAdjacencyListIncidenceGraph, int, int>
    {
        private const int DefaultInitialOutDegree = 4;

        private int _initialOutDegree;
        private ArrayBuilder<int> _sources;
        private ArrayBuilder<int> _targets;
        private ArrayPrefix<ArrayPrefix<int>> _outEdges;

        public JaggedAdjacencyListIncidenceGraphBuilder(int initialVertexUpperBound) : this(initialVertexUpperBound, 0)
        {
        }

        public JaggedAdjacencyListIncidenceGraphBuilder(int initialVertexUpperBound, int edgeCapacity)
        {
            if (initialVertexUpperBound < 0)
                throw new ArgumentOutOfRangeException(nameof(initialVertexUpperBound));

            if (edgeCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(edgeCapacity));

            _initialOutDegree = DefaultInitialOutDegree;
            int effectiveEdgeCapacity = Math.Max(edgeCapacity, DefaultInitialOutDegree);
            _sources = new ArrayBuilder<int>(effectiveEdgeCapacity);
            _targets = new ArrayBuilder<int>(effectiveEdgeCapacity);
            ArrayPrefix<int>[] outEdges = ArrayPool<ArrayPrefix<int>>.Shared.Rent(initialVertexUpperBound);
            Array.Clear(outEdges, 0, initialVertexUpperBound);
            _outEdges = new ArrayPrefix<ArrayPrefix<int>>(outEdges, initialVertexUpperBound);
        }

        private static ArrayPool<int> Pool => ArrayPool<int>.Shared;

        public int VertexUpperBound => _outEdges.Count;

        public int InitialOutDegree
        {
            get => _initialOutDegree <= 0 ? DefaultInitialOutDegree : _initialOutDegree;
            set => _initialOutDegree = value;
        }

        public bool TryAdd(int source, int target, out int edge)
        {
            if (source < 0)
            {
                edge = -1;
                return false;
            }

            if (target < 0)
            {
                edge = -2;
                return false;
            }

            int max = Math.Max(source, target);
            if (max >= VertexUpperBound)
            {
                int newVertexUpperBound = max + 1;
                int oldCount = _outEdges.Count;
                _outEdges = ArrayPrefixBuilder.Resize(_outEdges, newVertexUpperBound, true);
                Array.Clear(_outEdges.Array, oldCount, newVertexUpperBound - oldCount);
            }

            Assert(_sources.Count == _targets.Count);
            int newEdgeIndex = _targets.Count;
            _sources.Add(source);
            _targets.Add(target);

            if (_outEdges[source].Array == null)
                _outEdges[source] = new ArrayPrefix<int>(Pool.Rent(InitialOutDegree), 0);

            _outEdges[source] = ArrayPrefixBuilder.Add(_outEdges[source], newEdgeIndex, true);

            edge = newEdgeIndex;
            return true;
        }

        public JaggedAdjacencyListIncidenceGraph ToGraph()
        {
            Assert(_sources.Count == _targets.Count);
            ReadOnlySpan<int> targetsBuffer = new Span<int>(_targets.Buffer, 0, _targets.Count);
            ReadOnlySpan<int> sourcesBuffer = new Span<int>(_sources.Buffer, 0, _sources.Count);
            int[] endpoints = _targets.Count > 0 ? new int[_targets.Count * 2] : ArrayBuilder<int>.EmptyArray;
            if (endpoints.Length > 0)
            {
                targetsBuffer.CopyTo(endpoints.AsSpan(0, _targets.Count));
                sourcesBuffer.CopyTo(endpoints.AsSpan(_targets.Count, _sources.Count));
            }

            var outEdges = new ArrayPrefix<int>[_outEdges.Count];
            _outEdges.CopyTo(outEdges);

            if (_outEdges.Array != null)
                ArrayPool<ArrayPrefix<int>>.Shared.Return(_outEdges.Array, true);
            _outEdges = ArrayPrefix<ArrayPrefix<int>>.Empty;

            _sources.Dispose(false);
            _targets.Dispose(false);

            return new JaggedAdjacencyListIncidenceGraph(endpoints, outEdges);
        }
    }
}
