// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Ubiquitous.Traversal
{
    using System;
    using System.Collections.Generic;
    using static System.Diagnostics.Debug;

    internal struct DfsVertexIterator<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap,
        TGraphPolicy, TColorMapPolicy>
        where TEdgeEnumerator : IEnumerator<TEdge>
        where TGraphPolicy : IOutEdgesPolicy<TGraph, TVertex, TEdgeEnumerator>,
        IGetTargetPolicy<TGraph, TVertex, TEdge>
        where TColorMapPolicy : IMapPolicy<TColorMap, TVertex, Color>
    {
        internal DfsVertexStep<TVertex> _current;
        private int _state;
        private TGraphPolicy _graphPolicy;
        private TColorMapPolicy _colorMapPolicy;
        private TGraph _graph;
        private TVertex _currentVertex;
        private TColorMap _colorMap;

        internal DfsVertexIterator(TGraphPolicy graphPolicy, TColorMapPolicy colorMapPolicy,
            TGraph graph, TVertex startVertex, TColorMap colorMap)
        {
            Assert(graphPolicy != null, "graphPolicy != null");
            Assert(colorMapPolicy != null, "colorMapPolicy != null");

            _current = new DfsVertexStep<TVertex>(DfsStepKind.None, startVertex);
            _state = 1;

            _graphPolicy = graphPolicy;
            _colorMapPolicy = colorMapPolicy;

            _graph = graph;
            _currentVertex = startVertex;
            _colorMap = colorMap;
        }

        internal DfsVertexIterator<
            TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TGraphPolicy, TColorMapPolicy> Create(
            TVertex startVertex)
        {
            return new DfsVertexIterator<
                TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TGraphPolicy, TColorMapPolicy>(
                _graphPolicy, _colorMapPolicy, _graph, startVertex, _colorMap);
        }

        internal bool MoveNext()
        {
            Assert(_state > 0, "_state > 0");
            switch (_state)
            {
                case 1:
                    _current = new DfsVertexStep<TVertex>(DfsStepKind.None, _currentVertex);
                    _state = 2;
                    return true;
                case 2:
                    _current = new DfsVertexStep<TVertex>(DfsStepKind.DiscoverVertex, _currentVertex);
                    _state = 3;
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

// ReSharper restore FieldCanBeMadeReadOnly.Local
