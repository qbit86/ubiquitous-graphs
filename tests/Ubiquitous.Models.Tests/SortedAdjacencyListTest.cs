namespace Ubiquitous
{
    using System.Collections.Generic;
    using System.Linq;
    using Misnomer;
    using Misnomer.Extensions;
    using Models;
    using Workbench;
    using Xunit;

    public sealed class SortedAdjacencyListTest
    {
        private const int VertexUpperBound = 10;

        [Theory]
        [ClassData(typeof(IndexedGraphTestCollection))]
        public void SortedAdjacencyList_ShouldNotBeLess(string testName)
        {
            // Arrange
            var jaggedAdjacencyListBuilder = new JaggedAdjacencyListIncidenceGraphBuilder(VertexUpperBound);
            JaggedAdjacencyListIncidenceGraph jaggedAdjacencyList =
                BuildHelpers<JaggedAdjacencyListIncidenceGraph, int>.CreateGraph(ref jaggedAdjacencyListBuilder,
                    testName, true);

            var sortedAdjacencyListBuilder = new SortedAdjacencyListIncidenceGraphBuilder(VertexUpperBound);
            SortedAdjacencyListIncidenceGraph sortedAdjacencyList =
                BuildHelpers<SortedAdjacencyListIncidenceGraph, int>.CreateGraph(ref sortedAdjacencyListBuilder,
                    testName, true);

            // Act
            for (int v = 0; v < jaggedAdjacencyList.VertexUpperBound; ++v)
            {
                if (!jaggedAdjacencyList.TryGetOutEdges(v, out ArrayPrefixEnumerator<int> jaggedOutEdgesEnumerator))
                    continue;

                Rist<int> jaggedOutEdges = OneTimeEnumerable<int>.Create(jaggedOutEdgesEnumerator).ToRist();

                bool hasOutEdges = sortedAdjacencyList.TryGetOutEdges(v, out RangeEnumerator outEdgesEnumerator);
                Assert.True(hasOutEdges, $"Should have edges for {nameof(v)}: {v}");

                Rist<int> outEdges = OneTimeEnumerable<int>.Create(outEdgesEnumerator).ToRist();

                IEnumerable<int> difference = jaggedOutEdges.Except(outEdges);

                // Assert
                Assert.Empty(difference);

                outEdges.Dispose();
                jaggedOutEdges.Dispose();
            }
        }

        [Theory]
        [ClassData(typeof(IndexedGraphTestCollection))]
        public void SortedAdjacencyList_ShouldNotBeGreater(string testName)
        {
            // Arrange
            var jaggedAdjacencyListBuilder = new JaggedAdjacencyListIncidenceGraphBuilder(VertexUpperBound);
            JaggedAdjacencyListIncidenceGraph jaggedAdjacencyList =
                BuildHelpers<JaggedAdjacencyListIncidenceGraph, int>.CreateGraph(ref jaggedAdjacencyListBuilder,
                    testName, true);

            var sortedAdjacencyListBuilder = new SortedAdjacencyListIncidenceGraphBuilder(VertexUpperBound);
            SortedAdjacencyListIncidenceGraph sortedAdjacencyList =
                BuildHelpers<SortedAdjacencyListIncidenceGraph, int>.CreateGraph(ref sortedAdjacencyListBuilder,
                    testName, true);

            // Act
            for (int v = 0; v < sortedAdjacencyList.VertexUpperBound; ++v)
            {
                if (!sortedAdjacencyList.TryGetOutEdges(v, out RangeEnumerator outEdgesEnumerator))
                    continue;

                Rist<int> outEdges = OneTimeEnumerable<int>.Create(outEdgesEnumerator).ToRist();

                bool hasOutEdges =
                    jaggedAdjacencyList.TryGetOutEdges(v, out ArrayPrefixEnumerator<int> jaggedOutEdgesEnumerator);
                Assert.True(hasOutEdges, $"Should have edges for {nameof(v)}: {v}");

                Rist<int> jaggedOutEdges = OneTimeEnumerable<int>.Create(jaggedOutEdgesEnumerator).ToRist();

                IEnumerable<int> difference = outEdges.Except(jaggedOutEdges);

                // Assert
                Assert.Empty(difference);

                jaggedOutEdges.Dispose();
                outEdges.Dispose();
            }
        }
    }
}