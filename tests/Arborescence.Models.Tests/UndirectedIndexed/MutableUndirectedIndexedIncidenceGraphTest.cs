namespace Arborescence
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Xunit;
    using Graph = Models.MutableUndirectedIndexedIncidenceGraph;
    using EdgeEnumerator = System.ArraySegment<int>.Enumerator;

    public sealed class MutableUndirectedIndexedIncidenceGraphTest
    {
        private static CultureInfo F => CultureInfo.InvariantCulture;

        private static IEqualityComparer<HashSet<Endpoints>> HashSetEqualityComparer { get; } =
            HashSet<Endpoints>.CreateSetComparer();

        private static bool TryGetEndpoints(Graph graph, int edge, out Endpoints endpoints)
        {
            bool hasTail = graph.TryGetTail(edge, out int tail);
            bool hasHead = graph.TryGetHead(edge, out int head);
            endpoints = new Endpoints(tail, head);
            return hasTail && hasHead;
        }

#pragma warning disable CA1707 // Identifiers should not contain underscores
        [Theory]
        [ClassData(typeof(GraphDefinitionCollection))]
        internal void Graph_SizeShouldMatch(GraphDefinitionParameter p)
        {
            // Arrange
            using var graph = new Graph(p.VertexCount, p.Edges.Count);
            foreach (Endpoints endpoints in p.Edges)
                graph.Add(endpoints.Tail, endpoints.Head);

            // Assert
            Assert.Equal(p.VertexCount, graph.VertexCount);
            Assert.Equal(p.Edges.Count, graph.EdgeCount);
        }

        [Theory]
        [ClassData(typeof(GraphDefinitionCollection))]
        internal void Graph_ShouldContainSameSetOfEdges(GraphDefinitionParameter p)
        {
            // Arrange
            using var graph = new Graph(p.VertexCount, p.Edges.Count);
            foreach (Endpoints endpoints in p.Edges)
                graph.Add(endpoints.Tail, endpoints.Head);

            HashSet<Endpoints> expectedEdgeSet = p.Edges.ToHashSet();
            foreach (Endpoints edge in p.Edges)
            {
                if (edge.Tail == edge.Head)
                    continue;
                var invertedEdge = new Endpoints(edge.Head, edge.Tail);
                expectedEdgeSet.Add(invertedEdge);
            }

            // Act
            var actualEdgeSet = new HashSet<Endpoints>();
            for (int vertex = 0; vertex < graph.VertexCount; ++vertex)
            {
                EdgeEnumerator outEdges = graph.EnumerateOutEdges(vertex);
                while (outEdges.MoveNext())
                {
                    int edge = outEdges.Current;
                    bool hasEndpoints = TryGetEndpoints(graph, edge, out Endpoints endpoints);
                    if (!hasEndpoints)
                    {
                        Assert.True(hasEndpoints,
                            $"{nameof(edge)}: {edge.ToString(F)}, {nameof(endpoints)}: {endpoints.ToString()}");
                    }

                    actualEdgeSet.Add(endpoints);
                }
            }

            // Assert
            Assert.Equal(expectedEdgeSet, actualEdgeSet, HashSetEqualityComparer);
        }

        [Theory]
        [ClassData(typeof(GraphDefinitionCollection))]
        internal void Graph_OutEdgesShouldHaveSameTail(GraphDefinitionParameter p)
        {
            // Arrange
            using var graph = new Graph(p.VertexCount, p.Edges.Count);
            foreach (Endpoints endpoints in p.Edges)
                graph.Add(endpoints.Tail, endpoints.Head);

            // Act
            for (int vertex = 0; vertex < graph.VertexCount; ++vertex)
            {
                EdgeEnumerator outEdges = graph.EnumerateOutEdges(vertex);
                while (outEdges.MoveNext())
                {
                    int edge = outEdges.Current;
                    bool hasTail = graph.TryGetTail(edge, out int tail);
                    if (!hasTail)
                        Assert.True(hasTail);

                    // Assert
                    Assert.Equal(vertex, tail);
                }
            }
        }
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }
}
