﻿namespace Ubiquitous.Traversal
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class Step
    {
        public static GenericStep<TStepKind, TVertex, TEdge> Create<TStepKind, TVertex, TEdge>(
            TStepKind kind, TVertex vertex, TEdge edge)
        {
            return new GenericStep<TStepKind, TVertex, TEdge>(kind, vertex, edge);
        }

        internal static string StepToString(object kind, object vertex, object edge)
        {
            var s = new StringBuilder();
            s.Append('[');

            s.Append(kind);
            s.Append(", ");

            if (vertex != null)
                s.Append(vertex);

            s.Append(", ");

            if (edge != null)
                s.Append(edge);

            s.Append(']');

            return s.ToString();
        }
    }

    public readonly struct GenericStep<TStepKind, TVertex, TEdge> : IEquatable<GenericStep<TStepKind, TVertex, TEdge>>
    {
        public TStepKind Kind { get; }

        public TVertex Vertex { get; }

        public TEdge Edge { get; }

        public GenericStep(TStepKind kind, TVertex vertex, TEdge edge)
        {
            Kind = kind;
            Vertex = vertex;
            Edge = edge;
        }

        public override string ToString()
        {
            return Step.StepToString(Kind, Vertex, Edge);
        }

        public bool Equals(GenericStep<TStepKind, TVertex, TEdge> other)
        {
            EqualityComparer<TStepKind> kindComparer = EqualityComparer<TStepKind>.Default;
            if (!kindComparer.Equals(Kind, other.Kind))
                return false;

            EqualityComparer<TVertex> vertexComparer = EqualityComparer<TVertex>.Default;
            if (!vertexComparer.Equals(Vertex, other.Vertex))
                return false;

            EqualityComparer<TEdge> edgeComparer = EqualityComparer<TEdge>.Default;
            if (!edgeComparer.Equals(Edge, other.Edge))
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is GenericStep<TStepKind, TVertex, TEdge> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Kind.GetHashCode();
        }

        public static bool operator ==(GenericStep<TStepKind, TVertex, TEdge> left, GenericStep<TStepKind, TVertex, TEdge> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GenericStep<TStepKind, TVertex, TEdge> left, GenericStep<TStepKind, TVertex, TEdge> right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct GenericStepPolicy<TStepKind, TVertex, TEdge>
        : IStepPolicy<TStepKind, TVertex, TEdge, GenericStep<TStepKind, TVertex, TEdge>>
    {
        public GenericStep<TStepKind, TVertex, TEdge> CreateVertexStep(TStepKind kind, TVertex vertex)
        {
            return Step.Create(kind, vertex, default(TEdge));
        }

        public GenericStep<TStepKind, TVertex, TEdge> CreateEdgeStep(TStepKind kind, TEdge edge)
        {
            return Step.Create(kind, default(TVertex), edge);
        }
    }
}