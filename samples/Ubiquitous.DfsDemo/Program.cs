﻿// ReSharper disable SuggestVarOrType_Elsewhere

namespace Ubiquitous
{
    using System;
    using System.Collections.Generic;
    using Traversal.Advanced;
    using static System.Diagnostics.Debug;
    using ColorMap = System.ArraySegment<Traversal.Color>;
    using StepMap = System.ArraySegment<Traversal.Advanced.DfsStepKind>;
    using ColorMapPolicy = IndexedMapPolicy<Traversal.Color>;
    using IndexedAdjacencyListGraphPolicy =
        IndexedIncidenceGraphPolicy<JaggedAdjacencyListGraph, ArrayPrefixEnumerator<int>>;

    internal static partial class Program
    {
        private static void Main()
        {
            const int vertexCount = 10;
            int edgeCount = (int)Math.Ceiling(Math.Pow(vertexCount, 1.5));

            Console.WriteLine($"{nameof(vertexCount)}: {vertexCount}, {nameof(edgeCount)}: {edgeCount}");

            var builder = new JaggedAdjacencyListGraphBuilder(vertexCount);
            var prng = new Random(1729);

            for (int e = 0; e < edgeCount; ++e)
            {
                int source = prng.Next(vertexCount);
                int target = prng.Next(vertexCount);
                builder.Add(SourceTargetPair.Create(source, target));
            }

            JaggedAdjacencyListGraph graph = builder.MoveToIndexedAdjacencyListGraph();

            var vertices = new IndexCollection(graph.VertexCount);
            var indexedMapPolicy = new ColorMapPolicy(graph.VertexCount);

            {
                var dfs = new BaselineMultipleSourceDfs<JaggedAdjacencyListGraph, int, int,
                    IndexCollection, IndexCollection.Enumerator, ArrayPrefixEnumerator<int>,
                    ColorMap, IndexedAdjacencyListGraphPolicy, ColorMapPolicy, IndexCollectionEnumerablePolicy>(
                    default(IndexedAdjacencyListGraphPolicy), indexedMapPolicy,
                    default(IndexCollectionEnumerablePolicy));

                IEnumerable<Step<DfsStepKind, int, int>> steps = dfs.Traverse(graph, vertices);
                StepMap vertexKinds = new StepMap(new DfsStepKind[graph.VertexCount]);
                StepMap edgeKinds = new StepMap(new DfsStepKind[graph.EdgeCount]);
                FillEdgeKinds(steps, vertexKinds, edgeKinds);

                SerializeGraphByEdges(graph, vertexKinds, edgeKinds, "Recursive DFS Forest", Console.Out);
            }

            {
                var dfs = new MultipleSourceDfs<JaggedAdjacencyListGraph, int, int,
                    IndexCollection, IndexCollection.Enumerator, ArrayPrefixEnumerator<int>,
                    ColorMap, IndexedAdjacencyListGraphPolicy, ColorMapPolicy, IndexCollectionEnumerablePolicy>(
                    default(IndexedAdjacencyListGraphPolicy), indexedMapPolicy,
                    default(IndexCollectionEnumerablePolicy));

                var steps = dfs.Traverse(graph, vertices);
                StepMap vertexKinds = new StepMap(new DfsStepKind[graph.VertexCount]);
                StepMap edgeKinds = new StepMap(new DfsStepKind[graph.EdgeCount]);
                FillEdgeKinds(steps, vertexKinds, edgeKinds);

                SerializeGraphByEdges(graph, vertexKinds, edgeKinds, "Boost DFS Forest", Console.Out);
            }
        }

        private static void FillEdgeKinds(IEnumerable<Step<DfsStepKind, int, int>> steps,
            StepMap vertexKinds, StepMap edgeKinds)
        {
            Assert(steps != null);

            foreach (Step<DfsStepKind, int, int> step in steps)
            {
                switch (step.Kind)
                {
                    case DfsStepKind.TreeEdge:
                    case DfsStepKind.BackEdge:
                    case DfsStepKind.ForwardOrCrossEdge:
                        edgeKinds[step.Edge] = step.Kind;
                        break;
                    case DfsStepKind.StartVertex:
                        vertexKinds[step.Vertex] = step.Kind;
                        break;
                    default:
                        continue;
                }
            }
        }
    }
}
