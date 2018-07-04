﻿namespace Ubiquitous.Traversal.Advanced
{
    using System;
    using System.Collections.Generic;

    public struct DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
        TGraphConcept, TColorMapFactory, TStackFactory>
        where TEdgeEnumerator : IEnumerator<TEdge>
        where TColorMap : IDictionary<TVertex, Color>
        where TStack : IList<DfsStackFrame<TVertex, TEdge, TEdgeEnumerator>>
        where TGraphConcept : IGetOutEdgesConcept<TGraph, TVertex, TEdgeEnumerator>,
        IGetTargetConcept<TGraph, TVertex, TEdge>
        where TColorMapFactory : IFactory<TGraph, TColorMap>
        where TStackFactory : IFactory<TGraph, TStack>
    {
        private TGraphConcept GraphConcept { get; }

        private TColorMapFactory ColorMapFactory { get; }

        private TStackFactory StackFactory { get; }

        public DfsBuilder(TGraphConcept graphConcept, TColorMapFactory colorMapFactory, TStackFactory stackFactory)
        {
            if (graphConcept == null)
                throw new ArgumentNullException(nameof(graphConcept));

            if (colorMapFactory == null)
                throw new ArgumentNullException(nameof(colorMapFactory));

            if (stackFactory == null)
                throw new ArgumentNullException(nameof(stackFactory));

            GraphConcept = graphConcept;
            ColorMapFactory = colorMapFactory;
            StackFactory = stackFactory;
        }

        public Dfs<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
            TGraphConcept, TColorMapFactory, TStackFactory> Create()
        {
            return new Dfs<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
                TGraphConcept, TColorMapFactory, TStackFactory>(
                GraphConcept, ColorMapFactory, StackFactory);
        }
    }


    public struct DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
        TGraphConcept, TColorMapFactory>
        where TEdgeEnumerator : IEnumerator<TEdge>
        where TColorMap : IDictionary<TVertex, Color>
        where TStack : IList<DfsStackFrame<TVertex, TEdge, TEdgeEnumerator>>
        where TGraphConcept : IGetOutEdgesConcept<TGraph, TVertex, TEdgeEnumerator>,
        IGetTargetConcept<TGraph, TVertex, TEdge>
        where TColorMapFactory : IFactory<TGraph, TColorMap>
    {
        private TGraphConcept GraphConcept { get; }

        private TColorMapFactory ColorMapFactory { get; }

        public DfsBuilder(TGraphConcept graphConcept, TColorMapFactory colorMapFactory)
        {
            if (graphConcept == null)
                throw new ArgumentNullException(nameof(graphConcept));

            if (colorMapFactory == null)
                throw new ArgumentNullException(nameof(colorMapFactory));

            GraphConcept = graphConcept;
            ColorMapFactory = colorMapFactory;
        }

        public DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
            TGraphConcept, TColorMapFactory, TStackFactory> WithStackFactory<TStackFactory>()
            where TStackFactory : struct, IFactory<TGraph, TStack>
        {
            return new DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
                TGraphConcept, TColorMapFactory, TStackFactory>(
                GraphConcept, ColorMapFactory, default(TStackFactory));
        }

        public DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
            TGraphConcept, TColorMapFactory, TStackFactory> WithStackFactory<TStackFactory>(
            TStackFactory stackFactory)
            where TStackFactory : IFactory<TGraph, TStack>
        {
            return new DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
                TGraphConcept, TColorMapFactory, TStackFactory>(
                GraphConcept, ColorMapFactory, stackFactory);
        }
    }


    public struct DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack, TGraphConcept>
        where TEdgeEnumerator : IEnumerator<TEdge>
        where TColorMap : IDictionary<TVertex, Color>
        where TStack : IList<DfsStackFrame<TVertex, TEdge, TEdgeEnumerator>>
        where TGraphConcept : IGetOutEdgesConcept<TGraph, TVertex, TEdgeEnumerator>,
        IGetTargetConcept<TGraph, TVertex, TEdge>
    {
        private TGraphConcept GraphConcept { get; }

        public DfsBuilder(TGraphConcept graphConcept)
        {
            if (graphConcept == null)
                throw new ArgumentNullException(nameof(graphConcept));

            GraphConcept = graphConcept;
        }

        public DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
            TGraphConcept, TColorMapFactory> WithColorMapFactory<TColorMapFactory>()
            where TColorMapFactory : struct, IFactory<TGraph, TColorMap>
        {
            return new DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
                TGraphConcept, TColorMapFactory>(GraphConcept, default(TColorMapFactory));
        }

        public DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
            TGraphConcept, TColorMapFactory> WithColorMapFactory<TColorMapFactory>(
            TColorMapFactory colorMapFactory)
            where TColorMapFactory : IFactory<TGraph, TColorMap>
        {
            return new DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack,
                TGraphConcept, TColorMapFactory>(GraphConcept, colorMapFactory);
        }
    }


    public struct DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack>
        where TEdgeEnumerator : IEnumerator<TEdge>
        where TColorMap : IDictionary<TVertex, Color>
        where TStack : IList<DfsStackFrame<TVertex, TEdge, TEdgeEnumerator>>
    {
        public DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack, TGraphConcept>
            WithGraphConcept<TGraphConcept>()
            where TGraphConcept : struct, IGetOutEdgesConcept<TGraph, TVertex, TEdgeEnumerator>,
            IGetTargetConcept<TGraph, TVertex, TEdge>
        {
            return new DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack, TGraphConcept>(
                default(TGraphConcept));
        }

        public DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack, TGraphConcept>
            WithGraphConcept<TGraphConcept>(TGraphConcept graphConcept)
            where TGraphConcept : IGetOutEdgesConcept<TGraph, TVertex, TEdgeEnumerator>,
            IGetTargetConcept<TGraph, TVertex, TEdge>
        {
            return new DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack, TGraphConcept>(
                graphConcept);
        }
    }


    public struct DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap>
        where TEdgeEnumerator : IEnumerator<TEdge>
        where TColorMap : IDictionary<TVertex, Color>
    {
        public DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack> WithStack<TStack>()
            where TStack : IList<DfsStackFrame<TVertex, TEdge, TEdgeEnumerator>>
        {
            return new DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TStack>();
        }
    }


    public struct DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator>
        where TEdgeEnumerator : IEnumerator<TEdge>
    {
        public DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap> WithColorMap<TColorMap>()
            where TColorMap : IDictionary<TVertex, Color>
        {
            return new DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap>();
        }
    }


    public struct DfsBuilder<TGraph, TVertex, TEdge>
    {
        public DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator> WithEdgeEnumerator<TEdgeEnumerator>()
            where TEdgeEnumerator : IEnumerator<TEdge>
        {
            return new DfsBuilder<TGraph, TVertex, TEdge, TEdgeEnumerator>();
        }
    }


    public struct DfsBuilder<TGraph, TVertex>
    {
        public DfsBuilder<TGraph, TVertex, TEdge> WithEdge<TEdge>()
        {
            return new DfsBuilder<TGraph, TVertex, TEdge>();
        }
    }


    public struct DfsBuilder<TGraph>
    {
        public DfsBuilder<TGraph, TVertex> WithVertex<TVertex>()
        {
            return new DfsBuilder<TGraph, TVertex>();
        }
    }


    public struct DfsBuilder
    {
        public static DfsBuilder<TGraph> WithGraph<TGraph>()
        {
            return new DfsBuilder<TGraph>();
        }
    }
}