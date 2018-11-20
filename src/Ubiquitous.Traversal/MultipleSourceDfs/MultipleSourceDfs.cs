﻿namespace Ubiquitous.Traversal.Advanced
{
    using System;
    using System.Collections.Generic;

    public struct MultipleSourceDfs<TGraph, TVertex, TEdge, TVertexEnumerable, TVertexEnumerator, TEdgeEnumerator,
        TColorMap, TGraphPolicy, TColorMapPolicy, TVertexEnumerablePolicy>
        where TVertexEnumerable : IEnumerable<TVertex>
        where TVertexEnumerator : IEnumerator<TVertex>
        where TEdgeEnumerator : IEnumerator<TEdge>
        where TGraphPolicy : IGetOutEdgesPolicy<TGraph, TVertex, TEdgeEnumerator>,
        IGetTargetPolicy<TGraph, TVertex, TEdge>
        where TColorMapPolicy : IMapPolicy<TColorMap, TVertex, Color>, IFactory<TColorMap>
        where TVertexEnumerablePolicy : IEnumerablePolicy<TVertexEnumerable, TVertexEnumerator>
    {
        private TGraphPolicy GraphPolicy { get; }

        private TColorMapPolicy ColorMapPolicy { get; }

        private TVertexEnumerablePolicy VertexEnumerablePolicy { get; }

        public MultipleSourceDfs(TGraphPolicy graphPolicy, TColorMapPolicy colorMapPolicy,
            TVertexEnumerablePolicy vertexEnumerablePolicy)
        {
            if (graphPolicy == null)
                throw new ArgumentNullException(nameof(graphPolicy));

            if (colorMapPolicy == null)
                throw new ArgumentNullException(nameof(colorMapPolicy));

            if (vertexEnumerablePolicy == null)
                throw new ArgumentNullException(nameof(vertexEnumerablePolicy));

            GraphPolicy = graphPolicy;
            ColorMapPolicy = colorMapPolicy;
            VertexEnumerablePolicy = vertexEnumerablePolicy;
        }

        public DfsForestStepCollection<TGraph, TVertex, TEdge,
                TVertexEnumerable, TVertexEnumerator, TEdgeEnumerator,
                TColorMap, TGraphPolicy, TColorMapPolicy, TVertexEnumerablePolicy>
            Traverse(TGraph graph, TVertexEnumerable vertexCollection)
        {
            if (vertexCollection == null)
                throw new ArgumentNullException(nameof(vertexCollection));

            return new DfsForestStepCollection<TGraph, TVertex, TEdge,
                TVertexEnumerable, TVertexEnumerator, TEdgeEnumerator,
                TColorMap, TGraphPolicy, TColorMapPolicy, TVertexEnumerablePolicy>(graph,
                vertexCollection, 0, GraphPolicy, ColorMapPolicy, VertexEnumerablePolicy);
        }

        public DfsForestStepCollection<TGraph, TVertex, TEdge,
                TVertexEnumerable, TVertexEnumerator, TEdgeEnumerator,
                TColorMap, TGraphPolicy, TColorMapPolicy, TVertexEnumerablePolicy>
            Traverse(TGraph graph, TVertexEnumerable vertexCollection, int stackCapacity)
        {
            if (vertexCollection == null)
                throw new ArgumentNullException(nameof(vertexCollection));

            return new DfsForestStepCollection<TGraph, TVertex, TEdge,
                TVertexEnumerable, TVertexEnumerator, TEdgeEnumerator,
                TColorMap, TGraphPolicy, TColorMapPolicy, TVertexEnumerablePolicy>(graph,
                vertexCollection, stackCapacity, GraphPolicy, ColorMapPolicy, VertexEnumerablePolicy);
        }
    }
}