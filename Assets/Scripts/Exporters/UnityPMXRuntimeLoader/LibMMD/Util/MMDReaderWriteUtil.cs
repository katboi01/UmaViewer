using System;
using System.IO;
using System.Text;
using LibMMD.Reader;
using UnityEngine;

namespace LibMMD.Util
{
    public static class MMDReaderWriteUtil
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

        public static void WriteStringFixedLength(BinaryWriter writer, string str, int length, Encoding encoding)
        {
            if (length < 0)
            {
                throw new MMDFileParseException("pmx string length is negative");
            }
            
            if(length > 0)
            {
                var bytes = encoding.GetBytes(str);
                writer.Write(bytes);
            }
        }


        public static string ReadSizedString(BinaryReader reader, Encoding encoding)
        {
            var length = reader.ReadInt32();
            return ReadStringFixedLength(reader, length, encoding);
        }

        public static void WriteSizedString(BinaryWriter writer, string str, Encoding encoding)
        {
            var length = encoding.GetByteCount(str);
            writer.Write(length);
            WriteStringFixedLength(writer, str, length, encoding);
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

        public static void WriteVector4(BinaryWriter writer, Vector4 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
            writer.Write(vector.w);
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
        public static void WriteQuaternion(BinaryWriter writer, Quaternion quaternion)
        {
            writer.Write(-quaternion.x);
            writer.Write(quaternion.y);
            writer.Write(-quaternion.z);
            writer.Write(quaternion.w);
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

        public static void WriteVector3(BinaryWriter writer, Vector3 vector, bool useSizeAmplifier = true)
        {
            WriteAmpVector3(writer, vector, useSizeAmplifier ? WorldSizeAmplifier : 1);
        }

        public static void WriteAmpVector3(BinaryWriter writer, Vector3 vector, float amp)
        {
            writer.Write(-vector.x / amp);
            writer.Write(vector.y / amp);
            writer.Write(-vector.z / amp);
        }


        public static Vector3 ReadRawCoordinateVector3(BinaryReader reader)
        {
            var ret = new Vector3();
            ret[0] = MathUtil.NanToZero(reader.ReadSingle()) * WorldSizeAmplifier;
            ret[1] = MathUtil.NanToZero(reader.ReadSingle()) * WorldSizeAmplifier;
            ret[2] = MathUtil.NanToZero(reader.ReadSingle()) * WorldSizeAmplifier;
            return ret;
        }

        public static void WriteRawCoordinateVector3(BinaryWriter writer, Vector3 vector)
        {
            writer.Write(vector.x / WorldSizeAmplifier); // x
            writer.Write(vector.y / WorldSizeAmplifier); // y
            writer.Write(vector.z / WorldSizeAmplifier); // z
        }


        public static Vector2 ReadVector2(BinaryReader reader)
        {
            var ret = new Vector2();
            ret[0] = MathUtil.NanToZero(reader.ReadSingle());
            ret[1] = MathUtil.NanToZero(reader.ReadSingle());
            return ret;
        }

        public static void WriteVector2(BinaryWriter writer, Vector2 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
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

        public static void WriteIndex(BinaryWriter writer, int index, int size)
        {
            switch (size)
            {
                case 1:
                    writer.Write((sbyte)index);
                    break;
                case 2:
                    writer.Write((ushort)index);
                    break;
                case 4:
                    writer.Write(index);
                    break;
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

        public static void WriteColor(BinaryWriter writer, Color color, bool writeA)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            if (writeA)
            {
                writer.Write(color.a);
            }
        }

        public static bool Eof(BinaryReader binaryReader)
        {
            var bs = binaryReader.BaseStream;
            return (bs.Position == bs.Length);
        }

    }
}