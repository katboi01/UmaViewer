using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using Assets.Scripts.Audio.CriWareFormats.Common;

namespace CriWareFormats.Common
{
    public class BinaryReaderEndian : BinaryReader

    {
        public BinaryReaderEndian(Stream input) : base(input)
        {
        }

        public short ReadInt16BE()
        {
            ushort le = base.ReadUInt16();
            return (short)BinaryPrimitives.ReverseEndianness(le);
        }

        public ushort ReadUInt16BE()
        {
            ushort le = base.ReadUInt16();
            return BinaryPrimitives.ReverseEndianness(le);
        }

        public int ReadInt32BE()
        {
            uint le = base.ReadUInt32();
            return (int)BinaryPrimitives.ReverseEndianness(le);
        }

        public uint ReadUInt32BE()
        {
            uint le = base.ReadUInt32();
            return BinaryPrimitives.ReverseEndianness(le);
        }

        public long ReadInt64BE()
        {
            ulong le = base.ReadUInt64();
            return (long)BinaryPrimitives.ReverseEndianness(le);
        }

        public ulong ReadUInt64BE()
        {
            ulong le = base.ReadUInt64();
            return BinaryPrimitives.ReverseEndianness(le);
        }

        public float ReadSingleBE()
        {
            float le = base.ReadSingle();
            byte[] floatBytes = BitConverter.GetBytes(le);
            byte[] reversed = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                reversed[i] = floatBytes[3 - i];
            }
            return BitConverter.ToSingle(reversed, 0);
        }


    }
}
