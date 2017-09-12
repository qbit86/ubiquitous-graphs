﻿namespace Ubiquitous
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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

    internal static class Program
    {
        private static void Main(string[] args)
        {
            const int vertexCount = 9;
            int edgeCount = (int)Math.Ceiling(Math.Pow(vertexCount, 1.618));

            Console.WriteLine($"{nameof(vertexCount)}: {vertexCount}, {nameof(edgeCount)}: {edgeCount}");

            var builder = new IndexedAdjacencyListGraphBuilder(vertexCount);
            var prng = new Random(1729);

            // Making sure that each vertex has at least one nontrivial out-edge.
            for (int v = 0; v < vertexCount; ++v)
            {
                int source = v;
                int target = (v + prng.Next(1, vertexCount)) % vertexCount;
                builder.Add(SourceTargetPair.Create(source, target));
            }

            // Adding the rest of vertices.
            for (int e = vertexCount; e < edgeCount; ++e)
            {
                int source = prng.Next(vertexCount);
                int target = prng.Next(vertexCount);
                builder.Add(SourceTargetPair.Create(source, target));
            }

            IndexedAdjacencyListGraph graph = builder.ToIndexedAdjacencyListGraph();

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
                var steps = dfs.TraverseNonRecursively<ColorMap, ColorMapFactoryInstance>(graph, 0);
                var vertexKinds = IndexedDictionary.Create(new DfsStepKind[graph.VertexCount]);
                var edgeKinds = IndexedDictionary.Create(new DfsStepKind[graph.EdgeCount]);
                FillEdgeKinds(steps, vertexKinds, edgeKinds);

                SerializeGraphByEdges(graph, vertexKinds, edgeKinds, "Non-recursive DFS", Console.Out);
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

        private static void SerializeGraphByEdges(IndexedAdjacencyListGraph graph,
            IReadOnlyDictionary<int, DfsStepKind> vertexKinds, IReadOnlyDictionary<int, DfsStepKind> edgeKinds,
            string graphName, TextWriter textWriter)
        {
            Assert(graphName != null);
            Assert(textWriter != null);

            textWriter.WriteLine($"digraph \"{graphName}\"{Environment.NewLine}{{");
            try
            {
                textWriter.WriteLine($"    node [shape=circle]");
                for (int v = 0; v < graph.VertexCount; ++v)
                {
                    DfsStepKind vertexKind;
                    if (vertexKinds == null || !vertexKinds.TryGetValue(v, out vertexKind))
                        continue;

                    if (vertexKind == DfsStepKind.StartVertex)
                        textWriter.WriteLine($"    {v} [style=filled]");
                }

                for (int e = 0; e < graph.EdgeCount; ++e)
                {
                    SourceTargetPair<int> endpoints;
                    if (!graph.TryGetEndpoints(e, out endpoints))
                        continue;

                    textWriter.Write($"    {endpoints.Source} -> {endpoints.Target}");

                    DfsStepKind edgeKind;
                    if (edgeKinds == null || !edgeKinds.TryGetValue(e, out edgeKind))
                    {
                        textWriter.WriteLine();
                        continue;
                    }

                    // http://www.graphviz.org/Documentation/dotguide.pdf
                    switch (edgeKind)
                    {
                        case DfsStepKind.TreeEdge:
                            textWriter.WriteLine($" [style=bold]");
                            continue;
                        case DfsStepKind.BackEdge:
                            textWriter.WriteLine($" [style=dashed]");
                            continue;
                        case DfsStepKind.ForwardOrCrossEdge:
                            textWriter.WriteLine($" [style=solid]");
                            continue;
                    }

                    textWriter.WriteLine($" [style=dotted]");
                }
            }
            finally
            {
                textWriter.WriteLine("}");
            }
        }
    }
}