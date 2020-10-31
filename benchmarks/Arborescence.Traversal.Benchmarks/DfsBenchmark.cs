namespace Arborescence
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using Models;
    using Traversal;
    using EdgeEnumerator = System.ArraySegment<int>.Enumerator;

    [MemoryDiagnoser]
    public abstract class DfsBenchmark
    {
        private readonly DummyHandler<IndexedIncidenceGraph> _handler =
            new DummyHandler<IndexedIncidenceGraph>();

        private byte[] _colorMap = Array.Empty<byte>();

        protected DfsBenchmark()
        {
            InstantDfs = default;
            RecursiveDfs = default;
            EnumerableDfs = default;
            ReverseDfs = default;
        }

        [Params(10, 100, 1000, 10000)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int VertexCount { get; set; }

        private InstantDfs<IndexedIncidenceGraph, int, int, EdgeEnumerator, byte[],
                IndexedIncidenceGraphPolicy, IndexedColorMapPolicy>
            InstantDfs { get; }

        private RecursiveDfs<IndexedIncidenceGraph, int, int, EdgeEnumerator, byte[],
                IndexedIncidenceGraphPolicy, IndexedColorMapPolicy>
            RecursiveDfs { get; }

        private EnumerableDfs<IndexedIncidenceGraph, int, int, EdgeEnumerator, byte[],
                IndexedIncidenceGraphPolicy, IndexedSetPolicy>
            EnumerableDfs { get; }

        private ReverseDfs<IndexedIncidenceGraph, int, int, EdgeEnumerator, byte[],
                IndexedIncidenceGraphPolicy, IndexedSetPolicy>
            ReverseDfs { get; }

        private IndexedIncidenceGraph Graph { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            Graph = GraphHelper.Default.GetGraph(VertexCount);

            _colorMap = ArrayPool<byte>.Shared.Rent(Graph.VertexCount);
            _handler.Reset();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            ArrayPool<byte>.Shared.Return(_colorMap, true);
            _colorMap = Array.Empty<byte>();
        }

        [Benchmark(Baseline = true)]
        public int InstantDfsSteps()
        {
            Array.Clear(_colorMap, 0, _colorMap.Length);
            InstantDfs.Traverse(Graph, 0, _colorMap, _handler);
            return _handler.Count;
        }

        [Benchmark]
        public int RecursiveDfsSteps()
        {
            Array.Clear(_colorMap, 0, _colorMap.Length);
            RecursiveDfs.Traverse(Graph, 0, _colorMap, _handler);
            return _handler.Count;
        }

        [Benchmark]
        public int EnumerableDfsEdges()
        {
            Array.Clear(_colorMap, 0, _colorMap.Length);
            using IEnumerator<int> steps = EnumerableDfs.EnumerateEdges(Graph, 0, _colorMap);
            int count = 0;
            while (steps.MoveNext())
                ++count;

            return count;
        }

        [Benchmark]
        public int EnumerableDfsVertices()
        {
            Array.Clear(_colorMap, 0, _colorMap.Length);
            using IEnumerator<int> steps = EnumerableDfs.EnumerateVertices(Graph, 0, _colorMap);
            int count = 0;
            while (steps.MoveNext())
                ++count;

            return count;
        }

        [Benchmark]
        public int ReverseDfsEdges()
        {
            Array.Clear(_colorMap, 0, _colorMap.Length);
            using IEnumerator<int> steps = ReverseDfs.EnumerateEdges(Graph, 0, _colorMap);
            int count = 0;
            while (steps.MoveNext())
                ++count;

            return count;
        }

        [Benchmark]
        public int ReverseDfsVertices()
        {
            Array.Clear(_colorMap, 0, _colorMap.Length);
            using IEnumerator<int> steps = ReverseDfs.EnumerateVertices(Graph, 0, _colorMap);
            int count = 0;
            while (steps.MoveNext())
                ++count;

            return count;
        }
    }
}
