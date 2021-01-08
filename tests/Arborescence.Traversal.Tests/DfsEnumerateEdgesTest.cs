namespace Arborescence
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Misnomer;
    using Models;
    using Traversal;
    using Xunit;
    using EdgeEnumerator = System.ArraySegment<int>.Enumerator;
    using Graph = Models.IndexedIncidenceGraph;

    public sealed class DfsEnumerateEdgesTest
    {
        private EagerDfs<Graph, int, int, EdgeEnumerator> EagerDfs { get; }

        private EnumerableDfs<Graph, int, int, EdgeEnumerator> EnumerableDfs { get; }

        private void EnumerateEdgesCore(Graph graph, bool multipleSource)
        {
            // Arrange

            byte[] mapBackingStore = ArrayPool<byte>.Shared.Rent(Math.Max(graph.VertexCount, 1));
            Array.Clear(mapBackingStore, 0, mapBackingStore.Length);
            IndexedColorDictionary eagerColorMap = new(mapBackingStore);
            byte[] setBackingStore = ArrayPool<byte>.Shared.Rent(Math.Max(graph.VertexCount, 1));
            Array.Clear(setBackingStore, 0, setBackingStore.Length);
            IndexedSet set = new(setBackingStore);

            using Rist<int> eagerSteps = new(graph.VertexCount);
            using Rist<int> enumerableSteps = new(graph.VertexCount);
            DfsHandler<Graph, int, int> dfsHandler = CreateDfsHandler(eagerSteps);

            // Act

            if (multipleSource)
            {
                if (graph.VertexCount < 3)
                    return;

                int sourceCount = graph.VertexCount / 3;
                IndexEnumerator sources = new(sourceCount);

                EagerDfs.Traverse(graph, sources, eagerColorMap, dfsHandler);
                using IEnumerator<int> edges = EnumerableDfs.EnumerateEdges(graph, sources, set);
                enumerableSteps.AddEnumerator(edges);
            }
            else
            {
                int source = graph.VertexCount >> 1;
                EagerDfs.Traverse(graph, source, eagerColorMap, dfsHandler);
                using IEnumerator<int> edges = EnumerableDfs.EnumerateEdges(graph, source, set);
                enumerableSteps.AddEnumerator(edges);
            }

            // Assert

            int eagerStepCount = eagerSteps.Count;
            int enumerableStepCount = enumerableSteps.Count;
            Assert.Equal(eagerStepCount, enumerableStepCount);

            int count = eagerStepCount;
            for (int i = 0; i < count; ++i)
            {
                int eagerStep = eagerSteps[i];
                int enumerableStep = enumerableSteps[i];

                if (eagerStep == enumerableStep)
                    continue;

                Assert.Equal(eagerStep, enumerableStep);
            }

            // Cleanup

            ArrayPool<byte>.Shared.Return(mapBackingStore);
            ArrayPool<byte>.Shared.Return(setBackingStore);
        }

        private static DfsHandler<Graph, int, int> CreateDfsHandler(IList<int> treeEdges)
        {
            Debug.Assert(treeEdges != null, "treeEdges != null");

            DfsHandler<Graph, int, int> result = new();
            result.TreeEdge += (_, e) => treeEdges.Add(e);
            return result;
        }

        [Theory]
        [ClassData(typeof(IndexedGraphCollection))]
        internal void EnumerateEdges_SingleSource(GraphParameter<Graph> p)
        {
            EnumerateEdgesCore(p.Graph, false);
        }

        [Theory]
        [ClassData(typeof(IndexedGraphCollection))]
        internal void EnumerateEdges_MultipleSource(GraphParameter<Graph> p)
        {
            EnumerateEdgesCore(p.Graph, true);
        }
    }
}
