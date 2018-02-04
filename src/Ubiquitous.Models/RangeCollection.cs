﻿namespace Ubiquitous
{
    using System;
    using System.Collections.Generic;

    public struct RangeCollection : IReadOnlyCollection<int>, IEquatable<RangeCollection>
    {
        public int Start { get; }

        public int Count { get; }

        public RangeCollection(int start, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            Start = start;
            Count = count;
        }

        public bool Equals(RangeCollection other)
        {
            if (Start != other.Start || Count != other.Count)
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RangeCollection))
                return false;

            var other = (RangeCollection)obj;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ Count.GetHashCode();
        }

        public Enumerator GetConventionalEnumerator()
        {
            return new Enumerator(this);
        }

        public ForEachEnumerator GetEnumerator()
        {
            return new ForEachEnumerator(this);
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// An effective enumerator similar.
        /// </summary>
        /// <remarks>
        /// It is important that this enumerator does NOT implement <see cref="IDisposable"/>.
        /// We want the iterator to inline when we do foreach and to not result in
        /// a try/finally frame in the client.
        /// See also: https://github.com/dotnet/corefx/blob/master/src/System.Collections.Immutable/src/System/Collections/Immutable/ImmutableArray_1.Enumerator.cs.
        /// </remarks>
        public struct ForEachEnumerator
        {
            private readonly RangeCollection _range;
            private int _current;

            public ForEachEnumerator(RangeCollection range)
            {
                _range = range;
                _current = range.Start - 1;
            }

            public int Current => _current;

            public bool MoveNext()
            {
                ++_current;
                return _current < _range.Start + _range.Count;
            }
        }

        public struct Enumerator : IEnumerator<int>
        {
            private readonly RangeCollection _range;
            private int _current;

            public Enumerator(RangeCollection range)
            {
                _range = range;
                _current = range.Start - 1;
            }

            public void Reset()
            {
                _current = _range.Start - 1;
            }

            object System.Collections.IEnumerator.Current => _current;

            public int Current => _current;

            public bool MoveNext()
            {
                ++_current;
                return _current < _range.Start + _range.Count;
            }

            public void Dispose()
            {
                _current = _range.Start + _range.Count;
            }
        }
    }
}
