using System;
using System.IO;
using UnityEngine;

public class BSVReader
{
    public enum ReadMode
    {
        Memory = 0,
        Stream = 1
    }

    private enum Phase : byte
    {
        Header = 0,
        Body = 1,
        End = 2
    }

    private enum HeaderPhaseApriori : byte
    {
        Start = 0,
        RowCount = 1,
        MaxRowSize = 2
    }

    public enum Format : byte
    {
        Apriori = 0,
        AnonymousSchema = 1
    }

    public static ulong ReadVLQ(byte[] buf, ref int offset, int max_bytes = 8)
    {
        ulong n = 0;
        int count = 0;
        while (count < max_bytes)
        {
            count++;
            var val = (buf[offset] & 127Lu);
            n |= val;
            var isEnd = ((buf[offset] & 128) == 0);
            offset++;
            if (isEnd)
            {
                return n;
            }
            else
            {
                n = n << 7;
            }
        }
        return n;
    }
    public static ulong ReadUNum(byte[] buf, ref int offset, int bytes)
    {
        var arraySegment = new ArraySegment<byte>(buf, offset, bytes);
        offset += bytes;
        ulong n = 0;
        for(int i = arraySegment.Offset; i < arraySegment.Offset+arraySegment.Count; i++)
        {
            n = n << 8;
            n |= arraySegment.Array[i];
        }
        return n;
    }
    public static ArraySegment<byte> ReadText(byte[] buf, ref int offset)
    {
        int start = offset;
        while (offset < buf.Length &&buf[offset] != 0)
        {
            offset++;
        }
        var arraySegment = new ArraySegment<byte>(buf, start, offset - start);
        offset++;
        return arraySegment;
    }
    public static ArraySegment<byte> ReadFixedBlob(byte[] buf, ref int offset, int bytes)
    {
        var arraySegment = new ArraySegment<byte>(buf, offset, bytes);
        offset += bytes;
        return arraySegment;
    }
    public static ArraySegment<byte> ReadFixedText(byte[] buf, ref int offset, int bytes)
    {
        var arraySegment = new ArraySegment<byte>(buf, offset, bytes);
        offset += bytes;
        return arraySegment;
    }
    public static ArraySegment<byte> ReadBlob(byte[] buf, ref int offset)
    {
        ulong size = ReadVLQ(buf, ref offset);
        var arraySegment = new ArraySegment<byte>(buf, offset, (int)size);
        offset += (int)size;
        return arraySegment;
    }

    public static T[] GetArray<T>(ArraySegment<T> array)
    {
        T[] subArray = new T[array.Count];
        Buffer.BlockCopy(array.Array, array.Offset, subArray, 0, array.Count);
        return subArray;
    }

    internal static IBSVReader Init(string path, bool isCompressed,ReadMode mode = ReadMode.Memory)
    {
        try
        {
            byte[] input = isCompressed? LZ4Util.DecompressFromFile(path): File.ReadAllBytes(path);

            if (mode == ReadMode.Stream)
            {
                return new AprioriBSVReader(input, 2);
            }
            else
            {
                return new AnonymousSchemaBSVReader(input, 2);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Read Manifest File Error :" + e);
            return null;
        }
    }

    public interface IBSVReader : IDisposable
    {
        public abstract ulong GetRowCount();
        public abstract Format GetFormat();

        public abstract bool ReadLine<T>(ILineParser<T> parser, ref T dat);
    }

    public interface ILineParser<T>
    {
        public abstract void Parse(byte[] buf, ref int offset, ref T dat);
    }

    internal abstract class BaseBSVReader : IBSVReader, IDisposable
    {
        protected readonly byte[] buf;
        protected int offset;
        protected ulong rowCount;
        protected ulong maxRowSize;
        protected ulong row;
        protected readonly Format format;
        public ulong GetRowCount() { return rowCount; }
        public Format GetFormat() { return format; }
        public BaseBSVReader(Format format, byte[] buf, int offset)
        {
            this.format = format;
            this.buf = buf;
            this.offset = offset;
        }
        public bool ReadLine<T>(ILineParser<T> parser, ref T dat)
        {
            if (offset < buf.Length)
            {
                parser.Parse(buf, ref offset, ref dat);
                return true;
            }
            else return false;
        }
        public abstract void Dispose();
    }

    internal class AnonymousSchemaBSVReader : BaseBSVReader
    {
        private readonly ushort headerSize;
        private readonly uint schemaCount;
        private readonly uint schemaVersion;
        private readonly Schema[] schemas;

        internal uint GetSchemaCount() { return schemaCount; }

        internal uint GetSchemaVersion() { return schemaVersion; }

        public AnonymousSchemaBSVReader(byte[] buf, int offset) : base(Format.AnonymousSchema, buf, offset)
        {
            headerSize = (ushort)ReadUNum(buf, ref this.offset, 2);
            rowCount = ReadVLQ(buf, ref this.offset);
            maxRowSize = ReadVLQ(buf, ref this.offset);
            schemaVersion = (uint)ReadVLQ(buf, ref this.offset);
            schemaCount = (uint)ReadVLQ(buf, ref this.offset);
            if (schemaCount > 0)
            {
                schemas = new Schema[schemaCount];
                for (int i = 0; i < schemaCount; i++)
                {
                    Schema.Type type = (Schema.Type)ReadUNum(buf, ref this.offset, 1);
                    ulong? fixSize = null;
                    if (type == Schema.Type.UNumFixed || type == Schema.Type.TextFixed || type == Schema.Type.BlobFixed)
                    {
                        fixSize = ReadVLQ(buf, ref this.offset);
                    }
                    schemas[i] = new Schema(type, fixSize);
                }
            }
        }

        public override void Dispose() { }
    }

    public class Schema
    {
        public enum Type : byte
        {
            UShort = 16,
            UInt = 17,
            ULong = 18,
            UNum = 32,
            UNumFixed = 33,
            Blob = 48,
            BlobFixed = 49,
            Text = 64,
            TextFixed = 65
        }
        public readonly Type type;
        public readonly ulong? fixedSize;

        public Schema(Type type, ulong? fixedSize) { }
    }

    internal class AprioriBSVReader : BaseBSVReader
    {
        public AprioriBSVReader(byte[] buf, int offset) : base(Format.Apriori, buf, offset) { }
        public override void Dispose() { }
    }
}

