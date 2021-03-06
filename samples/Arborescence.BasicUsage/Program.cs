﻿namespace Arborescence
{
    using System;
    using System.Collections.Generic;
    using Models;
    using Traversal.Specialized;

    internal static class Program
    {
        private static void Main()
        {
            SimpleIncidenceGraph.Builder builder = new();
            builder.Add(2, 0);
            builder.Add(4, 3);
            builder.Add(0, 4);
            builder.Add(3, 2);
            builder.Add(4, 4);
            builder.Add(0, 2);
            builder.Add(2, 4);
            SimpleIncidenceGraph graph = builder.ToGraph();

            EnumerableBfs<SimpleIncidenceGraph, Endpoints, ArraySegment<Endpoints>.Enumerator> bfs;

            using IEnumerator<Endpoints> edges = bfs.EnumerateEdges(graph, source: 3, vertexCount: graph.VertexCount);
            while (edges.MoveNext())
                Console.WriteLine(edges.Current);
        }
    }
}
