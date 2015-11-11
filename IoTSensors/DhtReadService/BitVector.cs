using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhtReadService
{
    internal sealed class BitVector
    {
        const uint _variableLength = 16;
        UInt16[] vector;
        uint bitLength;
        public BitVector(uint length)
        {
            bitLength = length;
            vector = new ushort[(length / _variableLength) + 1];
        }

        public bool this[uint index]
        {
            get { return getBit(index); }
            set { setBit(index, value); }
        }

        bool getBit(uint index)
        {
            uint arrayIndex = index / _variableLength;
            uint bitIndex = index % _variableLength;
            UInt16 mask = (UInt16)(1 << (int)bitIndex);
            return mask == (vector[arrayIndex] & mask);
        }

        public uint[] ToUintValues()
        {
            long length = (bitLength / 32) + ((bitLength % 32) > 0 ? 1 : 0);
            var values = new uint[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = (uint)vector[i * 2];
                if (vector.Length > ((i * 2) + 1))
                    values[i] += (uint)
                    vector[(i * 4) + 1] << 16;
            }
            return values;
        }

        public ulong[] ToULongValues()
        {
            long length = (bitLength / 64) + ((bitLength % 64) > 0 ? 1 : 0);
            var values = new ulong[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = (ulong)vector[i * 4];
                if (vector.Length > ((i * 4) + 1))
                    values[i] += (ulong)
                    vector[(i * 4) + 1] << 16;
                if (vector.Length > ((i * 4) + 2))
                    values[i] += (ulong)
                     vector[(i * 4) + 2] << 32;
                if (vector.Length > ((i * 4) + 3))
                    values[i] += (ulong)
                      vector[(i * 4) + 3] << 48;
            }
            return values;
        }

        public uint Length { get { return bitLength; } }

        void setBit(uint index, bool value)
        {
            uint arrayIndex = index / _variableLength;
            uint bitIndex = index % _variableLength;
            UInt16 mask = (UInt16)(1 << (int)bitIndex);
            if (value)
            {
                vector[arrayIndex] |= mask;
            }
            else
            {
                vector[arrayIndex] &= (ushort)~mask;
            }
        }
    }
}
