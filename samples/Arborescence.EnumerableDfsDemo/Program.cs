﻿namespace Arborescence
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Models;
    using Traversal;
    using Workbench;

    internal static class Program
    {
        private static CultureInfo F => CultureInfo.InvariantCulture;

        private static void Main()
        {
            SimpleIncidenceGraph.Builder builder = new(0, 31);

            using (TextReader textReader = IndexedGraphs.GetTextReader("09"))
            {
                IEnumerable<Endpoints> edges = IndexedEdgeListParser.ParseEdges(textReader);
                foreach (Endpoints edge in edges)
                    builder.Add(edge.Tail, edge.Head);
            }

            SimpleIncidenceGraph graph = builder.ToGraph();
            Console.Write($"{nameof(graph.VertexCount)}: {graph.VertexCount.ToString(F)}");
            Console.WriteLine($", {nameof(graph.EdgeCount)}: {graph.EdgeCount.ToString(F)}");

            TextWriter w = Console.Out;

            EnumerableDfs<SimpleIncidenceGraph, int, Endpoints, ArraySegment<Endpoints>.Enumerator> dfs = default;

            w.WriteLine($"digraph \"{dfs.GetType().Name}\" {{");
            w.WriteLine("  node [shape=circle style=dashed fontname=\"Times-Italic\"]");

            // Enumerate vertices.
            for (int v = 0; v < graph.VertexCount; ++v)
            {
                w.Write(v == 0 ? "  " : " ");
                w.Write(V(v));
            }

            w.WriteLine();

            static IEnumerator<int> EnumerateSources() { yield return 3; }
            IEnumerator<int> sources = EnumerateSources();
            byte[] setBackingStore = ArrayPool<byte>.Shared.Rent(graph.VertexCount);
            Array.Clear(setBackingStore, 0, setBackingStore.Length);
            IndexedSet enumerableExploredSet = new(setBackingStore);
            HashSet<Endpoints> treeEdges = new(graph.EdgeCount);
            using IEnumerator<Endpoints> steps = dfs.EnumerateEdges(graph, sources, enumerableExploredSet);
            while (steps.MoveNext())
            {
                Endpoints e = steps.Current;
                w.WriteLine($"  {E(graph, e)} [style=bold]");
                treeEdges.Add(e);

                if (graph.TryGetHead(e, out int v))
                    w.WriteLine($"  {V(v)} [style=solid]");
            }

            ArrayPool<byte>.Shared.Return(setBackingStore);

            // Enumerate sources.
            w.WriteLine();
            sources = EnumerateSources();
            while (sources.MoveNext())
            {
                int v = sources.Current;
                w.WriteLine($"  {V(v)} [style=filled]");
            }

            // Enumerate rest of edges.
            w.WriteLine();
            for (int v = 0; v < graph.VertexCount; ++v)
            {
                ArraySegment<Endpoints>.Enumerator outEdges = graph.EnumerateOutEdges(v);
                while (outEdges.MoveNext())
                {
                    Endpoints e = outEdges.Current;
                    if (treeEdges.Contains(e))
                        continue;

                    w.WriteLine($"  {E(graph, e)} [style=dotted]");
                }
            }

            w.WriteLine("}");
        }

        private static string V(int v) => Base32.ToString(v);

        private static string E(SimpleIncidenceGraph g, Endpoints e)
        {
            string head = g.TryGetHead(e, out int h) ? V(h) : "?";
            string tail = g.TryGetTail(e, out int t) ? V(t) : "?";
            return string.Concat(tail, " -> ", head);
        }
    }
}
