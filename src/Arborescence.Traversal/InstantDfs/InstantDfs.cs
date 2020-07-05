namespace Arborescence.Traversal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly partial struct InstantDfs<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap,
        TGraphPolicy, TColorMapPolicy>
        where TEdgeEnumerator : IEnumerator<TEdge>
        where TGraphPolicy : IOutEdgesPolicy<TGraph, TVertex, TEdgeEnumerator>, IHeadPolicy<TGraph, TVertex, TEdge>
        where TColorMapPolicy : IMapPolicy<TColorMap, TVertex, Color>
    {
        public InstantDfs(TGraphPolicy graphPolicy, TColorMapPolicy colorMapPolicy)
        {
            if (graphPolicy == null)
                throw new ArgumentNullException(nameof(graphPolicy));

            if (colorMapPolicy == null)
                throw new ArgumentNullException(nameof(colorMapPolicy));

            GraphPolicy = graphPolicy;
            ColorMapPolicy = colorMapPolicy;
        }

        private TGraphPolicy GraphPolicy { get; }
        private TColorMapPolicy ColorMapPolicy { get; }

        private void TraverseCore<THandler>(TGraph graph, TVertex u, TColorMap colorMap, THandler handler,
            Func<TGraph, TVertex, bool> terminationCondition)
            where THandler : IDfsHandler<TGraph, TVertex, TEdge>
        {
            Debug.Assert(handler != null, "handler != null");
            Debug.Assert(terminationCondition != null, "terminationCondition != null");

            ColorMapPolicy.AddOrUpdate(colorMap, u, Color.Gray);
            handler.OnDiscoverVertex(graph, u);

            if (terminationCondition(graph, u))
            {
                ColorMapPolicy.AddOrUpdate(colorMap, u, Color.Black);
                handler.OnFinishVertex(graph, u);
                return;
            }

            var stack = new Internal.Stack<StackFrame<TVertex, TEdge, TEdgeEnumerator>>();
            try
            {
                TEdgeEnumerator outEdges = GraphPolicy.EnumerateOutEdges(graph, u);
                stack.Add(new StackFrame<TVertex, TEdge, TEdgeEnumerator>(u, outEdges));

                while (stack.TryTake(out StackFrame<TVertex, TEdge, TEdgeEnumerator> stackFrame))
                {
                    u = stackFrame.Vertex;
                    if (stackFrame.TryGetEdge(out TEdge inEdge))
                        handler.OnFinishEdge(graph, inEdge);

                    TEdgeEnumerator edges = stackFrame.EdgeEnumerator;
                    while (edges.MoveNext())
                    {
                        TEdge e = edges.Current;
                        if (!GraphPolicy.TryGetHead(graph, e, out TVertex v))
                            continue;

                        handler.OnExamineEdge(graph, e);
                        Color color = GetColorOrDefault(colorMap, v);
                        if (color == Color.None || color == Color.White)
                        {
                            handler.OnTreeEdge(graph, e);
                            stack.Add(new StackFrame<TVertex, TEdge, TEdgeEnumerator>(u, e, edges));
                            u = v;
                            ColorMapPolicy.AddOrUpdate(colorMap, u, Color.Gray);
                            handler.OnDiscoverVertex(graph, u);

                            edges = GraphPolicy.EnumerateOutEdges(graph, u);
                            if (terminationCondition(graph, u))
                            {
                                ColorMapPolicy.AddOrUpdate(colorMap, u, Color.Black);
                                handler.OnFinishVertex(graph, u);
                                return;
                            }
                        }
                        else
                        {
                            if (color == Color.Gray)
                                handler.OnBackEdge(graph, e);
                            else
                                handler.OnForwardOrCrossEdge(graph, e);
                            handler.OnFinishEdge(graph, e);
                        }
                    }

                    ColorMapPolicy.AddOrUpdate(colorMap, u, Color.Black);
                    handler.OnFinishVertex(graph, u);
                }
            }
            finally
            {
                stack.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Color GetColorOrDefault(TColorMap colorMap, TVertex vertex) =>
            ColorMapPolicy.TryGetValue(colorMap, vertex, out Color result) ? result : Color.None;
    }
#pragma warning restore CA1815 // Override equals and operator equals on value types
}