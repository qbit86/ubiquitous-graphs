﻿namespace Ubiquitous
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Traversal.Advanced;
    using Xunit;
    using Xunit.Abstractions;
    using ColorMap = System.ArraySegment<Traversal.Color>;
    using ColorMapPolicy = IndexedMapPolicy<Traversal.Color>;
    using IndexedAdjacencyListGraphPolicy =
        IndexedIncidenceGraphPolicy<JaggedAdjacencyListGraph, ArrayPrefixEnumerator<int>>;

    internal sealed class DfsStepEqualityComparer : IEqualityComparer<Step<DfsStepKind, int, int>>
    {
        internal static DfsStepEqualityComparer Default { get; } = new DfsStepEqualityComparer();

        public bool Equals(Step<DfsStepKind, int, int> x, Step<DfsStepKind, int, int> y)
        {
            if (x.Kind != y.Kind)
                return false;

            if (x.Vertex != y.Vertex)
                return false;

            if (x.Edge != y.Edge)
                return false;

            return true;
        }

        public int GetHashCode(Step<DfsStepKind, int, int> obj)
        {
            return obj.Kind.GetHashCode() ^ obj.Vertex.GetHashCode() ^ obj.Edge.GetHashCode();
        }
    }

    public class DfsTest
    {
        private const int VertexCount = 100;

        public DfsTest(ITestOutputHelper output)
        {
            var colorMapPolicy = new ColorMapPolicy(VertexCount);

            Dfs = new Dfs<JaggedAdjacencyListGraph, int, int,
                ArrayPrefixEnumerator<int>, ColorMap, IndexedAdjacencyListGraphPolicy, ColorMapPolicy>(
                default(IndexedAdjacencyListGraphPolicy), colorMapPolicy);

            MultipleSourceDfs = new MultipleSourceDfs<JaggedAdjacencyListGraph, int, int,
                IndexCollection, IndexCollectionEnumerator, ArrayPrefixEnumerator<int>,
                ColorMap, IndexedAdjacencyListGraphPolicy, ColorMapPolicy, IndexCollectionEnumerablePolicy>(
                default(IndexedAdjacencyListGraphPolicy), colorMapPolicy, default(IndexCollectionEnumerablePolicy));

            BaselineDfs = new BaselineDfs<JaggedAdjacencyListGraph, int, int,
                ArrayPrefixEnumerator<int>, ColorMap, IndexedAdjacencyListGraphPolicy, ColorMapPolicy>(
                default(IndexedAdjacencyListGraphPolicy), colorMapPolicy);

            BaselineMultipleSourceDfs = new BaselineMultipleSourceDfs<JaggedAdjacencyListGraph, int, int,
                IndexCollection, IndexCollectionEnumerator, ArrayPrefixEnumerator<int>,
                ColorMap, IndexedAdjacencyListGraphPolicy, ColorMapPolicy, IndexCollectionEnumerablePolicy>(
                default(IndexedAdjacencyListGraphPolicy), colorMapPolicy, default(IndexCollectionEnumerablePolicy));

            Output = output;
        }

        private Dfs<JaggedAdjacencyListGraph, int, int, ArrayPrefixEnumerator<int>, ColorMap,
                IndexedAdjacencyListGraphPolicy, ColorMapPolicy>
            Dfs { get; }

        private MultipleSourceDfs<JaggedAdjacencyListGraph, int, int,
                IndexCollection, IndexCollectionEnumerator, ArrayPrefixEnumerator<int>,
                ColorMap, IndexedAdjacencyListGraphPolicy, ColorMapPolicy, IndexCollectionEnumerablePolicy>
            MultipleSourceDfs { get; }

        private BaselineDfs<JaggedAdjacencyListGraph, int, int, ArrayPrefixEnumerator<int>, ColorMap,
                IndexedAdjacencyListGraphPolicy, ColorMapPolicy>
            BaselineDfs { get; }

        private BaselineMultipleSourceDfs<JaggedAdjacencyListGraph, int, int,
                IndexCollection, IndexCollectionEnumerator, ArrayPrefixEnumerator<int>, ColorMap,
                IndexedAdjacencyListGraphPolicy, ColorMapPolicy, IndexCollectionEnumerablePolicy>
            BaselineMultipleSourceDfs { get; }

        private ITestOutputHelper Output { get; }

        [Theory]
        [InlineData(1.0)]
        [InlineData(1.414)]
        [InlineData(1.5)]
        [InlineData(1.618)]
        [InlineData(2.0)]
        public void Baseline_and_boost_implementations_should_match_for_tree(double densityPower)
        {
            // Arrange

            JaggedAdjacencyListGraph graph = CreateGraph(densityPower);
            int vertex = 0;

            // Act

            List<Step<DfsStepKind, int, int>> baselineSteps = BaselineDfs.Traverse(graph, vertex).ToList();
            List<Step<DfsStepKind, int, int>> boostSteps = Dfs.Traverse(graph, vertex).ToList();

            // Assert

            int baselineStepCount = baselineSteps.Count;
            int boostStepCount = boostSteps.Count;
            if (baselineStepCount != boostStepCount)
            {
                Output.WriteLine($"{nameof(baselineStepCount)}: {baselineStepCount}, "
                    + $"{nameof(boostStepCount)}: {boostStepCount}");
            }

            int count = Math.Min(baselineStepCount, boostStepCount);
            for (int i = 0; i != count; ++i)
            {
                Step<DfsStepKind, int, int> baselineStep = baselineSteps[i];
                Step<DfsStepKind, int, int> boostStep = boostSteps[i];

                if (baselineStep == boostStep)
                    continue;

                Output.WriteLine($"{nameof(i)}: {i}, "
                    + $"{nameof(baselineStep)}: {baselineStep}, {nameof(boostStep)}: {boostStep}");
            }

            Assert.Equal(baselineSteps, boostSteps, DfsStepEqualityComparer.Default);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(1.059)]
        [InlineData(1.414)]
        [InlineData(1.618)]
        [InlineData(2.0)]
        public void Baseline_and_boost_implementations_should_match_for_forest(double densityPower)
        {
            // Arrange

            JaggedAdjacencyListGraph graph = CreateGraph(densityPower);
            var vertices = new IndexCollection(graph.VertexCount);

            // Act

            List<Step<DfsStepKind, int, int>> baselineSteps =
                BaselineMultipleSourceDfs.Traverse(graph, vertices).ToList();
            List<Step<DfsStepKind, int, int>> boostSteps = MultipleSourceDfs.Traverse(graph, vertices).ToList();
            int discoveredVertexCount = boostSteps.Count(s => s.Kind == DfsStepKind.DiscoverVertex);
            int expectedStartVertexCount = baselineSteps.Count(s => s.Kind == DfsStepKind.StartVertex);
            int actualStartVertexCount = boostSteps.Count(s => s.Kind == DfsStepKind.StartVertex);

            // Assert

            Assert.Equal(baselineSteps, boostSteps, DfsStepEqualityComparer.Default);
            Assert.Equal(VertexCount, discoveredVertexCount);
            Assert.Equal(expectedStartVertexCount, actualStartVertexCount);
        }

        private JaggedAdjacencyListGraph CreateGraph(double densityPower)
        {
            int edgeCount = (int)Math.Ceiling(Math.Pow(VertexCount, densityPower));

            var builder = new JaggedAdjacencyListGraphBuilder(VertexCount);
            var prng = new Random(1729);

            for (int e = 0; e < edgeCount; ++e)
            {
                int source = prng.Next(VertexCount);
                int target = prng.Next(VertexCount);
                builder.Add(source, target);
            }

            JaggedAdjacencyListGraph result = builder.MoveToIndexedAdjacencyListGraph();
            return result;
        }
    }
}
