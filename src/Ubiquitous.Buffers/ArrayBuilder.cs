// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Ubiquitous
{
    using System;
    using System.Buffers;
    using System.Diagnostics;

    // https://github.com/dotnet/corefx/blob/master/src/Common/src/System/Collections/Generic/ArrayBuilder.cs

    /// <summary>
    /// Helper type for avoiding allocations while building arrays.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    internal struct ArrayBuilder<T>
    {
        private const int DefaultCapacity = 4;
        private const int MaxCoreClrArrayLength = 0x7fefffff; // For byte arrays the limit is slightly larger

        private static readonly T[] s_emptyArray = new T[0];

        private T[] _array; // Starts out null, initialized on first Add.
        private int _count; // Number of items into _array we're using.

        /// <summary>
        /// Initializes the <see cref="ArrayBuilder{T}"/> with a specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity of the array to allocate.</param>
        public ArrayBuilder(int capacity) : this()
        {
            Debug.Assert(capacity >= 0);
            if (capacity > 0)
                _array = Pool.Rent(capacity);
        }

        public static T[] EmptyArray => s_emptyArray;

        private static ArrayPool<T> Pool => ArrayPool<T>.Shared;

        /// <summary>
        /// Gets the number of items this instance can store without re-allocating,
        /// or 0 if the backing array is <c>null</c>.
        /// </summary>
        public int Capacity => _array?.Length ?? 0;

        /// <summary>Gets the current underlying array.</summary>
        public T[] Buffer => _array;

        /// <summary>
        /// Gets the number of items in the array currently in use.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets or sets the item at a certain index in the array.
        /// </summary>
        /// <param name="index">The index into the array.</param>
        public T this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < _count);
                return _array[index];
            }
        }

        /// <summary>
        /// Adds an item to the backing array, resizing it if necessary.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            if (_count == Capacity)
                EnsureCapacity(_count + 1);

            UncheckedAdd(item);
        }

        /// <summary>
        /// Gets the first item in this builder.
        /// </summary>
        public T First()
        {
            Debug.Assert(_count > 0);
            return _array[0];
        }

        /// <summary>
        /// Gets the last item in this builder.
        /// </summary>
        public T Last()
        {
            Debug.Assert(_count > 0);
            return _array[_count - 1];
        }

        /// <summary>
        /// Creates an array from the contents of this builder.
        /// </summary>
        /// <remarks>
        /// Do not call this method twice on the same builder.
        /// </remarks>
        public T[] ToArray()
        {
            if (_count == 0)
                return s_emptyArray;

            Debug.Assert(_array != null); // Nonzero _count should imply this

            T[] result = _array;
            if (_count < result.Length)
            {
                result = Pool.Rent(_count);
                Array.Copy(_array, 0, result, 0, _count);
                Pool.Return(_array, true);
            }

            // Try to prevent callers from using the ArrayBuilder after ToArray, if _count != 0.
            _count = -1;
            _array = null;

            return result;
        }

        /// <summary>
        /// Adds an item to the backing array, without checking if there is room.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <remarks>
        /// Use this method if you know there is enough space in the <see cref="ArrayBuilder{T}"/>
        /// for another item, and you are writing performance-sensitive code.
        /// </remarks>
        public void UncheckedAdd(T item)
        {
            Debug.Assert(_count < Capacity);

            _array[_count++] = item;
        }

        /// <summary>
        /// Returns the backing array to the pool.
        /// </summary>
        /// <param name="clearArray">
        /// Indicates whether the contents of the backing array should be cleared before reuse.
        /// </param>
        public void Dispose(bool clearArray)
        {
            T[] toReturn = _array;
            this = default;

            if (toReturn != null)
                Pool.Return(toReturn, clearArray);
        }

        private void EnsureCapacity(int minimum)
        {
            Debug.Assert(minimum > Capacity);

            int capacity = Capacity;
            int nextCapacity = capacity == 0 ? DefaultCapacity : 2 * capacity;

            if ((uint)nextCapacity > MaxCoreClrArrayLength)
                nextCapacity = Math.Max(capacity + 1, MaxCoreClrArrayLength);

            nextCapacity = Math.Max(nextCapacity, minimum);

            T[] next = Pool.Rent(nextCapacity);
            if (_count > 0)
            {
                Array.Copy(_array, 0, next, 0, _count);
                Pool.Return(_array, true);
            }

            _array = next;
        }
    }
}
