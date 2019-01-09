﻿// ReSharper disable SuggestVarOrType_Elsewhere

namespace Ubiquitous
{
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using Models;
    using Traversal;
    using Traversal.Advanced;
    using ColorMap = ArrayPrefix<Traversal.Color>;
    using ColorMapPolicy = Models.IndexedMapPolicy<Traversal.Color>;
    using EdgeEnumerator = ArraySegmentEnumerator<int>;
    using IndexedAdjacencyListGraphPolicy =
        Models.IndexedIncidenceGraphPolicy<Models.AdjacencyListIncidenceGraph, ArraySegmentEnumerator<int>>;

    [MemoryDiagnoser]
    public abstract class DfsTreeBoostBenchmark
    {
        protected DfsTreeBoostBenchmark()
        {
            var colorMapPolicy = new ColorMapPolicy(VertexCount);

            BaselineDfs = BaselineDfs<AdjacencyListIncidenceGraph, int, int, EdgeEnumerator, ColorMap, IndexedDfsStep>
                .Create(default(IndexedAdjacencyListGraphPolicy), colorMapPolicy, default(IndexedDfsStepPolicy));

            DefaultDfs = Dfs<AdjacencyListIncidenceGraph, int, int, EdgeEnumerator, ColorMap, IndexedDfsStep>
                .Create(default(IndexedAdjacencyListGraphPolicy), colorMapPolicy, default(IndexedDfsStepPolicy));
        }

        [Params(10, 100, 1000)]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int VertexCount { get; set; }

        private BaselineDfs<AdjacencyListIncidenceGraph, int, int, EdgeEnumerator, ColorMap, IndexedDfsStep,
                IndexedAdjacencyListGraphPolicy, ColorMapPolicy, IndexedDfsStepPolicy>
            BaselineDfs { get; }

        private Dfs<AdjacencyListIncidenceGraph, int, int, EdgeEnumerator, ColorMap, IndexedDfsStep,
                IndexedAdjacencyListGraphPolicy, ColorMapPolicy, IndexedDfsStepPolicy>
            DefaultDfs { get; }

        private Dfs<AdjacencyListIncidenceGraph, int, int, EdgeEnumerator, ColorMap, IndexedDfsStep,
                IndexedAdjacencyListGraphPolicy, ColorMapPolicy, IndexedDfsStepPolicy>
            CachingDfs { get; set; }

        private AdjacencyListIncidenceGraph Graph { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            Graph = GraphHelper.Default.GetGraph(VertexCount);

            var indexedMapPolicy = new ColorMapPolicy(VertexCount);
            indexedMapPolicy.Warmup();

            CachingDfs = Dfs<AdjacencyListIncidenceGraph, int, int, EdgeEnumerator, ColorMap, IndexedDfsStep>
                .Create(default(IndexedAdjacencyListGraphPolicy), indexedMapPolicy, default(IndexedDfsStepPolicy));
        }

        [Benchmark(Baseline = true)]
        public int BaselineDfsTree()
        {
            int count = 0;
            IEnumerable<IndexedDfsStep> steps = BaselineDfs.Traverse(Graph, 0);
            foreach (IndexedDfsStep _ in steps)
                ++count;

            return count;
        }

        [Benchmark]
        public int DefaultDfsTree()
        {
            int count = 0;
            var steps = DefaultDfs.Traverse(Graph, 0);
            foreach (var _ in steps)
                ++count;

            return count;
        }

        [Benchmark]
        public int CachingDfsTree()
        {
            int count = 0;
            var steps = CachingDfs.Traverse(Graph, 0);
            foreach (var _ in steps)
                ++count;

            return count;
        }
    }
}
