﻿namespace Ubiquitous.Traversal.Advanced
{
    using System.Collections;
    using System.Collections.Generic;
    using static System.Diagnostics.Debug;

    internal struct BaselineDfsStepCollection<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TGraphConcept>
        : IEnumerable<Step<DfsStepKind, TVertex, TEdge>>
        where TEdgeEnumerator : IEnumerator<TEdge>
        where TColorMap : IDictionary<TVertex, Color>
        where TGraphConcept : IGetOutEdgesConcept<TGraph, TVertex, TEdgeEnumerator>,
        IGetTargetConcept<TGraph, TVertex, TEdge>
    {
        private TColorMap _colorMap;

        private TGraph Graph { get; }

        private TVertex StartVertex { get; }

        private TGraphConcept GraphConcept { get; }

        public BaselineDfsStepCollection(TGraph graph, TVertex startVertex, TColorMap colorMap,
            TGraphConcept graphConcept)
        {
            Assert(colorMap != null);
            Assert(graphConcept != null);

            Graph = graph;
            StartVertex = startVertex;
            _colorMap = colorMap;
            GraphConcept = graphConcept;
        }

        public IEnumerator<Step<DfsStepKind, TVertex, TEdge>> GetEnumerator()
        {
            return ProcessVertexCoroutine(StartVertex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerator<Step<DfsStepKind, TVertex, TEdge>> result = GetEnumerator();
            return result;
        }

        private IEnumerator<Step<DfsStepKind, TVertex, TEdge>> ProcessVertexCoroutine(TVertex vertex)
        {
            _colorMap[vertex] = Color.Gray;
            yield return Step.Create(DfsStepKind.DiscoverVertex, vertex, default(TEdge));

            if (GraphConcept.TryGetOutEdges(Graph, vertex, out TEdgeEnumerator edges) && edges != null)
            {
                while (edges.MoveNext())
                {
                    IEnumerator<Step<DfsStepKind, TVertex, TEdge>> steps = ProcessEdgeCoroutine(edges.Current);
                    while (steps.MoveNext())
                        yield return steps.Current;
                }
            }

            _colorMap[vertex] = Color.Black;
            yield return Step.Create(DfsStepKind.FinishVertex, vertex, default(TEdge));
        }

        private IEnumerator<Step<DfsStepKind, TVertex, TEdge>> ProcessEdgeCoroutine(TEdge edge)
        {
            yield return Step.Create(DfsStepKind.ExamineEdge, default(TVertex), edge);

            if (GraphConcept.TryGetTarget(Graph, edge, out TVertex target))
            {
                if (!_colorMap.TryGetValue(target, out Color neighborColor))
                    neighborColor = Color.None;

                switch (neighborColor)
                {
                    case Color.None:
                    case Color.White:
                        yield return Step.Create(DfsStepKind.TreeEdge, default(TVertex), edge);
                        IEnumerator<Step<DfsStepKind, TVertex, TEdge>> steps = ProcessVertexCoroutine(target);
                        while (steps.MoveNext())
                            yield return steps.Current;
                        break;
                    case Color.Gray:
                        yield return Step.Create(DfsStepKind.BackEdge, default(TVertex), edge);
                        break;
                    default:
                        yield return Step.Create(DfsStepKind.ForwardOrCrossEdge, default(TVertex), edge);
                        break;
                }
            }

            yield return Step.Create(DfsStepKind.FinishEdge, default(TVertex), edge);
        }
    }
}