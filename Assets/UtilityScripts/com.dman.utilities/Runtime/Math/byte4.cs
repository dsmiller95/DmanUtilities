using System;

namespace Dman.Utilities.Math
{
    [Serializable]
    public struct byte4
    {
        uint internalValue;

        private const uint BYTE_MASK = 0x000000FF;
        public static readonly byte4 ZERO = new byte4((uint)0);
        public byte4(uint value)
        {
            this.internalValue = value;
        }
        public byte4(byte x, byte y, byte z, byte w)
        {
            internalValue = (uint)(
                x |
                y << 8 |
                z << 16 |
                w << 24);
        }

        public byte x => this[0];
        public byte y => this[1];
        public byte z => this[2];
        public byte w => this[3];

        public byte this[int i]
        {
            get => (byte)((internalValue >> (i * 8)) & BYTE_MASK);
            set
            {
                var shiftedMask = BYTE_MASK << (i * 8);
                internalValue = (internalValue & ~shiftedMask) | (uint)(value << (i * 8));
            }
        }
        public static explicit operator uint(byte4 b) => b.internalValue;
        public static explicit operator byte4(uint i) => new byte4(i);


        public override string ToString()
        {
            return $"byte4({x}, {y}, {z}, {w})";
        }
    }
}
