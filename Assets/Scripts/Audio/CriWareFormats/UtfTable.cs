using CriWareFormats.Common;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace CriWareFormats
{
    [Flags]
    public enum ColumnFlag : byte
    {
        Name = 0x10,
        Default = 0x20,
        Row = 0x40,
        Undefined = 0x80
    }

    public enum ColumnType : byte
    {
        Byte = 0x00,
        SByte = 0x01,
        UInt16 = 0x02,
        Int16 = 0x03,
        UInt32 = 0x04,
        Int32 = 0x05,
        UInt64 = 0x06,
        Int64 = 0x07,
        Float = 0x08,
        Double = 0x09,
        String = 0x0A,
        VLData = 0x0B,
        UInt128 = 0x0C,
        Undefined = 0xFF
    }

    public struct Column
    {
        public ColumnFlag Flag;
        public ColumnType Type;
        public string Name;
        public uint Offset;

        public override string ToString()
        {
            return Name;
        }
    }

    public struct VLData
    {
        public uint Offset;
        public uint Size;

        public override string ToString()
        {
            return $"Offset: {Offset}, Length: {Size}";
        }
    }

    public struct Result
    {
        public ColumnType Type;
        public object Value;
    }

    public sealed class UtfTable
    {
        private readonly BinaryReaderEndian binaryReader;
        private readonly uint tableOffset;

        private readonly uint tableSize;
        private readonly ushort version;
        private readonly ushort rowsOffset;
        private readonly uint stringsOffset;
        private readonly uint dataOffset;
        private readonly uint nameOffset;

        private readonly ushort columns;
        private readonly ushort rowWidth;
        private readonly uint rows;

        private readonly byte[] schemaBuffer;
        private readonly Column[] schema;

        private readonly uint schemaOffset;
        private readonly uint schemaSize;
        private readonly uint rowsSize;
        private readonly uint dataSize;
        private readonly uint stringsSize;

        private readonly byte[] stringTable;
        private readonly string tableName;

        public UtfTable(Stream utfTableStream, out int utfTableRows, out string utfTableRowName) :
            this(utfTableStream, 0, out utfTableRows, out utfTableRowName)
        { }

        public UtfTable(Stream utfTableStream, uint offset) :
            this(utfTableStream, offset, out int _, out string _)
        { }

        public UtfTable(Stream utfTableStream, uint offset, out int utfTableRows, out string rowName)
        {
            binaryReader = new BinaryReaderEndian(utfTableStream);
            tableOffset = offset;

            binaryReader.BaseStream.Position = offset;

            if (!binaryReader.ReadChars(4).SequenceEqual("@UTF".ToCharArray()))
                throw new InvalidDataException("Incorrect magic.");
            tableSize = binaryReader.ReadUInt32BE() + 0x08;
            version = binaryReader.ReadUInt16BE();
            rowsOffset = (ushort)(binaryReader.ReadUInt16BE() + 0x08);
            stringsOffset = binaryReader.ReadUInt32BE() + 0x08;
            dataOffset = binaryReader.ReadUInt32BE() + 0x08;
            nameOffset = binaryReader.ReadUInt32BE();
            columns = binaryReader.ReadUInt16BE();
            rowWidth = binaryReader.ReadUInt16BE();
            rows = binaryReader.ReadUInt32BE();

            schemaOffset = 0x20;
            schemaSize = rowsOffset - schemaOffset;
            rowsSize = stringsOffset - rowsOffset;
            stringsSize = dataOffset - stringsOffset;
            dataSize = tableSize - dataOffset;

            if (version != 0x00 && version != 0x01)
                throw new InvalidDataException("Unknown @UTF version.");
            if (tableOffset + tableSize > utfTableStream.Length)
                throw new InvalidDataException("Table size exceeds bounds of file.");
            if (rowsOffset > tableSize || stringsOffset > tableSize || dataOffset > tableSize)
                throw new InvalidDataException("Offsets out of bounds.");
            if (stringsSize <= 0 || nameOffset > stringsSize)
                throw new InvalidDataException("Invalid string table size.");
            if (Columns <= 0)
                throw new InvalidDataException("Table has no columns.");

            schemaBuffer = new byte[schemaSize];
            binaryReader.BaseStream.Position = tableOffset + schemaOffset;
            if (binaryReader.Read(schemaBuffer, 0, (int)schemaSize) != schemaSize)
                throw new InvalidDataException("Failed to read schema.");

            stringTable = new byte[stringsSize];
            binaryReader.BaseStream.Position = tableOffset + stringsOffset;
            if (binaryReader.Read(stringTable, 0, (int)stringsSize) != stringsSize)
                throw new InvalidDataException("Failed to read string table.");

            uint columnOffset = 0;
            uint schemaPos = 0;

            tableName = GetStringFromTable(nameOffset);

            schema = new Column[columns];

            BinaryReaderEndian bytesReader = new BinaryReaderEndian(new MemoryStream(schemaBuffer) { Position = 0 });
            for (int i = 0; i < columns; i++)
            {
                bytesReader.BaseStream.Position = schemaPos;

                byte info = bytesReader.ReadByte();
                uint nameOffset = bytesReader.ReadUInt32BE();

                if (nameOffset > stringsSize)
                    throw new InvalidDataException("String offset out of bounds.");
                schemaPos += 0x1 + 0x4;

                bytesReader.BaseStream.Position = schemaPos;

                schema[i] = new Column()
                {
                    Flag = (ColumnFlag)(info & 0xF0),
                    Type = (ColumnType)(info & 0x0F),
                    Name = "",
                    Offset = 0
                };

                if (schema[i].Flag == 0 ||
                    !schema[i].Flag.HasFlag(ColumnFlag.Name) ||
                     schema[i].Flag.HasFlag(ColumnFlag.Default) && schema[i].Flag.HasFlag(ColumnFlag.Row) ||
                     schema[i].Flag.HasFlag(ColumnFlag.Undefined))
                    throw new InvalidDataException("Unknown column flag combo found.");

                uint valueSize; 
                switch (schema[i].Type)
                {
                    case ColumnType.Byte:
                    case ColumnType.SByte:
                        valueSize = 0x1;
                        break;
                    case ColumnType.UInt16:
                    case ColumnType.Int16:
                        valueSize = 0x2;
                        break;
                    case ColumnType.UInt32:
                    case ColumnType.Int32:
                    case ColumnType.Float:
                    case ColumnType.String:
                        valueSize = 0x4;
                        break;
                    case ColumnType.UInt64:
                    case ColumnType.Int64:
                    case ColumnType.VLData:
                        valueSize = 0x8;
                        break;
                    default:
                        throw new InvalidDataException("Invalid column type.");
                };

                if (schema[i].Flag.HasFlag(ColumnFlag.Name))
                    schema[i].Name = GetStringFromTable(nameOffset);

                if (schema[i].Flag.HasFlag(ColumnFlag.Default))
                {
                    schema[i].Offset = schemaPos;
                    schemaPos += valueSize;

                    bytesReader.BaseStream.Position = schemaPos;

                }

                if (schema[i].Flag.HasFlag(ColumnFlag.Row))
                {
                    schema[i].Offset = columnOffset;
                    columnOffset += valueSize;
                }
            }

            utfTableRows = (int)rows;
            rowName = GetStringFromTable(nameOffset);

            bytesReader.Dispose();
        }

        public ushort Columns => columns;

        public uint Rows => rows;

        public Column[] Schema => schema;

        public string TableName => tableName;

        public int GetColumn(string columnName)
        {
            for (int i = 0; i < columns; i++)
            {
                Column column = schema[i];

                if (column.Name is null || !column.Name.Equals(columnName))
                    continue;

                return i;
            }

            return -1;
        }

        private bool Query(int row, int column, out Result result)
        {
            result = new Result();

            if (row >= rows || row < 0)
                //throw new ArgumentOutOfRangeException(nameof(row));
                return false;
            if (column >= columns || column < 0)
                //throw new ArgumentOutOfRangeException(nameof(column));
                return false;

            Column col = schema[column];
            uint dataOffset = 0;
            BinaryReaderEndian bytesReader = null;

            result.Type = col.Type;

            if (col.Flag.HasFlag(ColumnFlag.Default))
            {
                if (schemaBuffer != null)
                {
                    bytesReader = new BinaryReaderEndian(new MemoryStream(schemaBuffer));
                    bytesReader.BaseStream.Position = col.Offset;
                }
                else
                {
                    dataOffset = tableOffset + schemaOffset + col.Offset;
                }
            }
            else if (col.Flag.HasFlag(ColumnFlag.Row))
            {
                dataOffset = (uint)(tableOffset + rowsOffset + row * rowWidth + col.Offset);
            }
            else
                throw new InvalidDataException("Invalid flag.");

            binaryReader.BaseStream.Position = dataOffset;

            switch (col.Type)
            {
                case ColumnType.Byte:
                    result.Value = (bytesReader != null) ? bytesReader.ReadByte() : binaryReader.ReadByte();
                    break;
                case ColumnType.SByte:
                    result.Value = (bytesReader != null) ? bytesReader.ReadSByte() : binaryReader.ReadSByte();
                    break;
                case ColumnType.UInt16:
                    result.Value = (bytesReader != null) ? bytesReader.ReadUInt16BE() : binaryReader.ReadUInt16BE();
                    break;
                case ColumnType.Int16:
                    result.Value = (bytesReader != null) ? bytesReader.ReadInt16BE() : binaryReader.ReadInt16BE();
                    break;
                case ColumnType.UInt32:
                    result.Value = (bytesReader != null) ? bytesReader.ReadUInt32BE() : binaryReader.ReadUInt32BE();
                    break;
                case ColumnType.Int32:
                    result.Value = (bytesReader != null) ? bytesReader.ReadInt32BE() : binaryReader.ReadInt32BE();
                    break;
                case ColumnType.UInt64:
                    result.Value = (bytesReader != null) ? bytesReader.ReadUInt64BE() : binaryReader.ReadUInt64BE();
                    break;
                case ColumnType.Int64:
                    result.Value = (bytesReader != null) ? bytesReader.ReadInt64BE() : binaryReader.ReadInt64BE();
                    break;
                case ColumnType.Float:
                    result.Value = (bytesReader != null) ? bytesReader.ReadSingleBE() : binaryReader.ReadSingleBE();
                    break;
                //case ColumnType.Double:
                //    break;
                case ColumnType.String:
                    uint nameOffset = (bytesReader != null) ? bytesReader.ReadUInt32BE() : binaryReader.ReadUInt32BE();
                    if (nameOffset > stringsSize)
                        throw new InvalidDataException("Name offset out of bounds.");
                    result.Value = GetStringFromTable(nameOffset);
                    break;
                case ColumnType.VLData:
                    if ((bytesReader != null))
                    {
                        result.Value = new VLData()
                        {
                            Offset = bytesReader.ReadUInt32BE(),
                            Size = bytesReader.ReadUInt32BE()
                        };
                    }
                    else
                    {
                        result.Value = new VLData()
                        {
                            Offset = binaryReader.ReadUInt32BE(),
                            Size = binaryReader.ReadUInt32BE()
                        };
                    }
                    break;
                //case ColumnType.UInt128:
                //    break;
                default:
                    return false;
            }

            return true;
        }

        public bool Query<T>(int row, int column, out T value)
        {
            bool valid = Query(row, column, out Result result);

            bool enumParseResult = Enum.TryParse<ColumnType>(typeof(T).Name, out ColumnType type);

            if (!valid || !enumParseResult || result.Type != type)
            {
                value = default;
                return false;
            }

            value = (T)result.Value;

            if (value is VLData vlData)
            {
                vlData.Offset += tableOffset + dataOffset;
                value = (T)(object)vlData;
            }

            return true;
        }

        public bool Query<T>(int row, string columnName, out T value) =>
            Query(row, GetColumn(columnName), out value);

        public bool Query(int row, int column, out uint offset, out uint size)
        {
            if (!Query(row, column, out VLData data))
            {
                offset = 0;
                size = 0;

                return false;
            }

            offset = data.Offset;
            size = data.Size;

            return true;
        }

        public bool Query(int row, string columnName, out uint offset, out uint size)
            => Query(row, GetColumn(columnName), out offset, out size);

        private string GetStringFromTable(uint offset)
        {
            if (offset > stringsSize)
                throw new InvalidDataException("Invalid string offset.");

            StringBuilder stringBuilder = new StringBuilder();

            int i = 0;
            while (i < stringsSize)
            {
                stringBuilder.Append((char)stringTable[offset + i++]);
                if (stringTable[offset + i] == '\0') break;
            }

            return stringBuilder.ToString();
        }

        public UtfTable OpenSubtable(string tableName)
        {
            if (!Query(0, tableName, out VLData tableValueData))
                throw new ArgumentException("Subtable does not exist.");

            binaryReader.BaseStream.Position = tableValueData.Offset;

            return new UtfTable(
                binaryReader.BaseStream,
                tableValueData.Offset,
                out int _,
                out string _);
        }
    }
}