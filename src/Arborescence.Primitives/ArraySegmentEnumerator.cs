namespace Arborescence
{
    using System.Collections;
    using System.Collections.Generic;

    // https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/ArraySegment.cs

#pragma warning disable CA1710 // Identifiers should have correct suffix
    /// <inheritdoc cref="System.Collections.Generic.IEnumerator{T}"/>
    public struct ArraySegmentEnumerator<T> : IEnumerator<T>, IEnumerable<T>
    {
        private readonly T[] _array;
        private readonly int _start;
        private readonly int _end;
        private int _current;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArraySegmentEnumerator{T}"/> structure.
        /// </summary>
        /// <param name="array">The array containing the range of elements to enumerate.</param>
        /// <param name="start">The inclusive start index of the range.</param>
        /// <param name="endExclusive">The exclusive end index of the range.</param>
        public ArraySegmentEnumerator(T[] array, int start, int endExclusive)
        {
            if (array is null || unchecked((uint)start > (uint)array.Length || (uint)endExclusive > (uint)array.Length))
                ThrowHelper.ThrowArraySegmentEnumeratorCtorValidationFailedExceptions(array, start, endExclusive);

            _array = array;
            _start = start;
            _end = endExclusive;
            _current = start - 1;
        }

        /// <inheritdoc/>
        public T Current
        {
            get
            {
                if (_current < _start)
                    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumNotStarted();
                if (_current >= _end)
                    ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumEnded();
                return _array[_current];
            }
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (_current < _end)
            {
                _current++;
                return _current < _end;
            }

            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator for the range of elements.</returns>
        public ArraySegmentEnumerator<T> GetEnumerator()
        {
            ArraySegmentEnumerator<T> ator = this;
            ator._current = _start - 1;
            return ator;
        }

        object IEnumerator.Current => Current;

        void IEnumerator.Reset()
        {
            _current = _start - 1;
        }

        /// <inheritdoc/>
        public void Dispose() { }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this;
        }
    }
#pragma warning restore CA1710 // Identifiers should have correct suffix
}
