﻿namespace Arborescence
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Models;
    using Workbench;

    internal abstract class GraphCollection<TGraph, TEdge, TEdges, TGraphBuilder> : IEnumerable<object[]>
        where TGraph : IIncidenceGraph<int, TEdge, TEdges>
        where TGraphBuilder : IGraphBuilder<TGraph, int, TEdge>
    {
        private const int LowerBound = 1;
        private const int UpperBound = 10;

        private static CultureInfo F => CultureInfo.InvariantCulture;

        public IEnumerator<object[]> GetEnumerator()
        {
            for (int i = LowerBound; i < UpperBound; ++i)
            {
                string testCase = i.ToString("D2", CultureInfo.InvariantCulture);
                TGraphBuilder builder = CreateGraphBuilder(0);

                using (TextReader textReader = IndexedGraphs.GetTextReader(testCase))
                {
                    if (textReader == TextReader.Null)
                        continue;

                    IEnumerable<Endpoints> edges = IndexedEdgeListParser.ParseEdges(textReader);
                    foreach (Endpoints edge in edges)
                        builder.TryAdd(edge.Tail, edge.Head, out _);
                }

                TGraph graph = builder.ToGraph();
                string description = $"{{{nameof(testCase)}: {testCase}}}";
                var graphParameter = GraphParameter.Create(graph, description);
                yield return new object[] { graphParameter };
            }

            {
                const int vertexCount = 1;
                const double densityPower = 1.0;
                TGraphBuilder builder = CreateGraphBuilder(1);
                GraphHelpers.PopulateIncidenceGraphBuilder<TGraph, TEdge, TEdges, TGraphBuilder>(
                    builder, vertexCount, densityPower);
                TGraph graph = builder.ToGraph();
                string description =
                    $"{{{nameof(vertexCount)}: {vertexCount.ToString(F)}, {nameof(densityPower)}: {densityPower.ToString(F)}}}";
                var graphParameter = GraphParameter.Create(graph, description);
                yield return new object[] { graphParameter };
            }

            for (int i = 1; i < 7; ++i)
            {
                double power = 0.5 * i;
                int vertexCount = (int)Math.Ceiling(Math.Pow(10.0, power));
                foreach (double densityPower in GraphHelpers.DensityPowers)
                {
                    TGraphBuilder builder = CreateGraphBuilder(1);
                    GraphHelpers.PopulateIncidenceGraphBuilder<TGraph, TEdge, TEdges, TGraphBuilder>(
                        builder, vertexCount, densityPower);
                    TGraph graph = builder.ToGraph();
                    string description =
                        $"{{{nameof(vertexCount)}: {vertexCount.ToString(F)}, {nameof(densityPower)}: {densityPower.ToString(F)}}}";
                    var graphParameter = GraphParameter.Create(graph, description);
                    yield return new object[] { graphParameter };
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected abstract TGraphBuilder CreateGraphBuilder(int initialVertexCount);
    }

    internal sealed class IndexedGraphCollection :
        GraphCollection<IndexedIncidenceGraph, int, ArraySegment<int>.Enumerator, IndexedIncidenceGraph.Builder>
    {
        protected override IndexedIncidenceGraph.Builder CreateGraphBuilder(int initialVertexCount) =>
            new(initialVertexCount);
    }

    internal sealed class FromMutableIndexedGraphCollection :
        GraphCollection<IndexedIncidenceGraph, int, ArraySegment<int>.Enumerator, MutableIndexedIncidenceGraph>
    {
        protected override MutableIndexedIncidenceGraph CreateGraphBuilder(int initialVertexCount) =>
            new(initialVertexCount);
    }

    internal sealed class MutableIndexedGraphCollection : GraphCollection<
        MutableIndexedIncidenceGraph, int, ArraySegment<int>.Enumerator, MutableIndexedIncidenceGraphBuilder>
    {
        protected override MutableIndexedIncidenceGraphBuilder CreateGraphBuilder(int initialVertexCount) =>
            new(initialVertexCount);
    }

    internal sealed class SimpleGraphCollection : GraphCollection<
        SimpleIncidenceGraph, Endpoints, ArraySegment<Endpoints>.Enumerator, SimpleIncidenceGraph.Builder>
    {
        protected override SimpleIncidenceGraph.Builder CreateGraphBuilder(int initialVertexCount) =>
            new(initialVertexCount);
    }

    internal sealed class FromMutableSimpleGraphCollection : GraphCollection<
        SimpleIncidenceGraph, Endpoints, ArraySegment<Endpoints>.Enumerator, MutableSimpleIncidenceGraph>
    {
        protected override MutableSimpleIncidenceGraph CreateGraphBuilder(int initialVertexCount) =>
            new(initialVertexCount);
    }

    internal sealed class MutableSimpleGraphCollection : GraphCollection<
        MutableSimpleIncidenceGraph, Endpoints, ArraySegment<Endpoints>.Enumerator, MutableSimpleIncidenceGraphBuilder>
    {
        protected override MutableSimpleIncidenceGraphBuilder CreateGraphBuilder(int initialVertexCount) =>
            new(initialVertexCount);
    }

    internal sealed class UndirectedSimpleGraphCollection : GraphCollection<
        SimpleIncidenceGraph, Endpoints, ArraySegment<Endpoints>.Enumerator, SimpleIncidenceGraph.UndirectedBuilder>
    {
        protected override SimpleIncidenceGraph.UndirectedBuilder CreateGraphBuilder(int initialVertexCount) =>
            new(initialVertexCount);
    }

    internal sealed class MutableIndexedIncidenceGraphBuilder :
        IGraphBuilder<MutableIndexedIncidenceGraph, int, int>,
        IDisposable
    {
        private MutableIndexedIncidenceGraph? _graph;

        public MutableIndexedIncidenceGraphBuilder(int initialVertexCount) => _graph = new(initialVertexCount);

        public void Dispose()
        {
            if (_graph is null)
                return;

            _graph.Dispose();
            _graph = null;
        }

        public bool TryAdd(int tail, int head, out int edge)
        {
            if (_graph is null)
                throw new ObjectDisposedException(nameof(MutableIndexedIncidenceGraphBuilder));

            return _graph.TryAdd(tail, head, out edge);
        }

        public MutableIndexedIncidenceGraph ToGraph()
        {
            if (_graph is null)
                throw new ObjectDisposedException(nameof(MutableIndexedIncidenceGraphBuilder));

            MutableIndexedIncidenceGraph result = _graph;
            _graph = null;
            return result;
        }
    }

    internal sealed class MutableSimpleIncidenceGraphBuilder :
        IGraphBuilder<MutableSimpleIncidenceGraph, int, Endpoints>,
        IDisposable
    {
        private MutableSimpleIncidenceGraph? _graph;

        public MutableSimpleIncidenceGraphBuilder(int initialVertexCount) => _graph = new(initialVertexCount);

        public void Dispose()
        {
            if (_graph is null)
                return;

            _graph.Dispose();
            _graph = null;
        }

        public bool TryAdd(int tail, int head, out Endpoints edge)
        {
            if (_graph is null)
                throw new ObjectDisposedException(nameof(MutableIndexedIncidenceGraphBuilder));

            return _graph.TryAdd(tail, head, out edge);
        }

        public MutableSimpleIncidenceGraph ToGraph()
        {
            if (_graph is null)
                throw new ObjectDisposedException(nameof(MutableIndexedIncidenceGraphBuilder));

            MutableSimpleIncidenceGraph result = _graph;
            _graph = null;
            return result;
        }
    }
}
