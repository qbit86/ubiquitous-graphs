namespace Ubiquitous.Traversal
{
    using System;
    using Advanced;

    public readonly struct IndexedDfsStep : IEquatable<IndexedDfsStep>
    {
        private static uint s_kindMask = 0x000000FF;

        private readonly uint _storage;

        public IndexedDfsStep(DfsStepKind kind, int value)
        {
            _storage = ((uint)value << 8) | (s_kindMask & (uint)kind);
        }

        DfsStepKind Kind => (DfsStepKind)(s_kindMask & _storage);

        int Value => (int)(_storage >> 8);

        public bool Equals(IndexedDfsStep other)
        {
            return _storage == other._storage;
        }

        public override string ToString()
        {
            return $"[{Kind}, {Value}]";
        }

        public override bool Equals(object obj)
        {
            return obj is IndexedDfsStep other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _storage.GetHashCode();
        }

        public static bool operator ==(IndexedDfsStep left, IndexedDfsStep right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IndexedDfsStep left, IndexedDfsStep right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct IndexedDfsStepPolicy
        : IStepPolicy<DfsStepKind, int, int, IndexedDfsStep>
    {
        public IndexedDfsStep CreateVertexStep(DfsStepKind kind, int vertex)
        {
            return new IndexedDfsStep(kind, vertex);
        }

        public IndexedDfsStep CreateEdgeStep(DfsStepKind kind, int edge)
        {
            return new IndexedDfsStep(kind, edge);
        }
    }
}
