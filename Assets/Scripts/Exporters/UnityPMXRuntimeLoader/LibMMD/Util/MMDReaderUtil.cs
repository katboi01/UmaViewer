using System;
using System.IO;
using System.Text;
using LibMMD.Reader;
using UnityEngine;

namespace LibMMD.Util
{
    public static class MMDReaderUtil
    {
        const float WorldSizeAmplifier = 0.08f;

        public static string ReadStringFixedLength(BinaryReader reader, int length, Encoding encoding)
        {
            if (length < 0)
            {
                throw new MMDFileParseException("pmx string length is negative");
            }
            if (length == 0)
            {
                return "";
            }
            var bytes = reader.ReadBytes(length);
            var str = encoding.GetString(bytes);
            var end = str.IndexOf("\0", StringComparison.Ordinal);
            if (end >= 0)
            {
                str = str.Substring(0, end);
            }
            return str;
        }

        public static string ReadSizedString(BinaryReader reader, Encoding encoding)
        {
            var length = reader.ReadInt32();
            return ReadStringFixedLength(reader, length, encoding);
        }

        public static Vector4 ReadVector4(BinaryReader reader)
        {
            var ret = new Vector4();
            ret[0] = MathUtil.NanToZero(reader.ReadSingle());
            ret[1] = MathUtil.NanToZero(reader.ReadSingle());
            ret[2] = MathUtil.NanToZero(reader.ReadSingle());
            ret[3] = MathUtil.NanToZero(reader.ReadSingle());
            return ret;
        }

        public static Quaternion ReadQuaternion(BinaryReader reader)
        {
            var ret = new Quaternion();
            ret.x = -MathUtil.NanToZero(reader.ReadSingle());
            ret.y = MathUtil.NanToZero(reader.ReadSingle());
            ret.z = -MathUtil.NanToZero(reader.ReadSingle());
            ret.w = MathUtil.NanToZero(reader.ReadSingle());
            return ret;
        }

        public static Vector3 ReadVector3(BinaryReader reader)
        {
            return ReadAmpVector3(reader, WorldSizeAmplifier);
        }

        public static Vector3 ReadAmpVector3(BinaryReader reader, float amp)
        {
            var ret = new Vector3();
            ret[0] = -MathUtil.NanToZero(reader.ReadSingle()) * amp;
            ret[1] = MathUtil.NanToZero(reader.ReadSingle()) * amp;
            ret[2] = -MathUtil.NanToZero(reader.ReadSingle()) * amp;
            return ret;
        }

        public static Vector3 ReadRawCoordinateVector3(BinaryReader reader)
        {
            var ret = new Vector3();
            ret[0] = MathUtil.NanToZero(reader.ReadSingle()) * WorldSizeAmplifier;
            ret[1] = MathUtil.NanToZero(reader.ReadSingle()) * WorldSizeAmplifier;
            ret[2] = MathUtil.NanToZero(reader.ReadSingle()) * WorldSizeAmplifier;
            return ret;
        }


        public static Vector2 ReadVector2(BinaryReader reader)
        {
            var ret = new Vector2();
            ret[0] = MathUtil.NanToZero(reader.ReadSingle());
            ret[1] = MathUtil.NanToZero(reader.ReadSingle());
            return ret;
        }

        public static int ReadIndex(BinaryReader reader, int size)
        {
            switch (size)
            {
                case 1:
                    return reader.ReadSByte();
                case 2:
                    return reader.ReadUInt16();
                case 4:
                    return reader.ReadInt32();
                default:
                    throw new MMDFileParseException("invalid index size: " + size);
            }
        }

        public static Color ReadColor(BinaryReader reader, bool readA)
        {
            var ret = new Color
            {
                r = reader.ReadSingle(),
                g = reader.ReadSingle(),
                b = reader.ReadSingle(),
                a = readA ? reader.ReadSingle() : 1.0f
            };
            return ret;
        }

        public static bool Eof(BinaryReader binaryReader)
        {
            var bs = binaryReader.BaseStream;
            return (bs.Position == bs.Length);
        }

    }
}