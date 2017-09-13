﻿namespace Ubiquitous
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static System.Diagnostics.Debug;
    using ColorMap = IndexedDictionary<Color, Color[]>;
    using StepMap = IndexedDictionary<DfsStepKind, DfsStepKind[]>;

    internal struct ColorMapFactoryInstance : IFactoryConcept<IndexedAdjacencyListGraph, ColorMap>
    {
        public ColorMap Acquire(IndexedAdjacencyListGraph graph)
        {
            return IndexedDictionary.Create(new Color[graph.VertexCount]);
        }

        public void Release(IndexedAdjacencyListGraph graph, ColorMap value)
        {
        }
    }

    internal static partial class Program
    {
        private static void Main(string[] args)
        {
            const int vertexCount = 8;
            int edgeCount = (int)Math.Ceiling(Math.Pow(vertexCount, 1.618));

            Console.WriteLine($"{nameof(vertexCount)}: {vertexCount}, {nameof(edgeCount)}: {edgeCount}");

            var builder = new IndexedAdjacencyListGraphBuilder(vertexCount);
            var prng = new Random(1729);

            for (int e = 0; e < edgeCount; ++e)
            {
                int source = prng.Next(vertexCount);
                int target = prng.Next(vertexCount);
                builder.Add(SourceTargetPair.Create(source, target));
            }

            IndexedAdjacencyListGraph graph = builder.MoveToIndexedAdjacencyListGraph();

            var dfs = new Dfs<IndexedAdjacencyListGraph, int, int, IEnumerable<int>,
                IndexedAdjacencyListGraphInstance, IndexedAdjacencyListGraphInstance>();

            {
                var steps = dfs.TraverseRecursively<IEnumerable<int>, ColorMap, ColorMapFactoryInstance>(
                    graph, Enumerable.Range(0, graph.VertexCount));
                var vertexKinds = IndexedDictionary.Create(new DfsStepKind[graph.VertexCount]);
                var edgeKinds = IndexedDictionary.Create(new DfsStepKind[graph.EdgeCount]);
                FillEdgeKinds(steps, vertexKinds, edgeKinds);

                SerializeGraphByEdges(graph, vertexKinds, edgeKinds, "Recursive DFS Forest", Console.Out);
            }

            {
                var steps = dfs.TraverseNonRecursively<IEnumerable<int>, ColorMap, ColorMapFactoryInstance>(
                    graph, Enumerable.Range(0, graph.VertexCount));
                var vertexKinds = IndexedDictionary.Create(new DfsStepKind[graph.VertexCount]);
                var edgeKinds = IndexedDictionary.Create(new DfsStepKind[graph.EdgeCount]);
                FillEdgeKinds(steps, vertexKinds, edgeKinds);

                SerializeGraphByEdges(graph, vertexKinds, edgeKinds, "Non-recursive DFS Forest", Console.Out);
            }
        }

        private static void FillEdgeKinds(IEnumerable<Step<DfsStepKind, int, int>> steps,
            StepMap vertexKinds, StepMap edgeKinds)
        {
            Assert(steps != null);

            foreach (var step in steps)
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
