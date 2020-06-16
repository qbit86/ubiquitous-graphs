﻿namespace Ubiquitous.Traversal
{
    using System;
    using System.Runtime.CompilerServices;

#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct IndexedSetPolicy : ISetPolicy<byte[], int>
    {
        private const int BitShiftPerByte = 3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int count)
        {
            if (count <= 0)
                return 0;

            uint temp = (uint)(count - 1 + (1 << BitShiftPerByte));
            return (int)(temp >> BitShiftPerByte);
        }

        public bool Contains(byte[] items, int item)
        {
            if (items == null || (uint)item >= (uint)items.Length)
                return false;

            int byteIndex = Div8Rem(item, out int bitIndex);
            byte bitMask = (byte)(1 << bitIndex);

            return (items[byteIndex] & bitMask) != 0;
        }

        public void Add(byte[] items, int item)
        {
            if (items == null || (uint)item >= (uint)items.Length)
                return;

            int byteIndex = Div8Rem(item, out int bitIndex);
            byte bitMask = (byte)(1 << bitIndex);

            items[byteIndex] |= bitMask;
        }

        public void Clear(byte[] items)
        {
            if (items == null)
                return;

            Array.Clear(items, 0, items.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Div8Rem(int number, out int remainder)
        {
            uint quotient = (uint)number >> 3;
            remainder = number & 0b111;
            return (int)quotient;
        }
    }
#pragma warning restore CA1815 // Override equals and operator equals on value types
}
