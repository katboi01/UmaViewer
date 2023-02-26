using System;
using System.IO;
using DereTore.Common;
using System.Linq.Expressions;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    internal sealed class UtfFieldImage : IComparable<UtfFieldImage> {

        public static string NameOf<T>(Expression<Func<T>> e)
        {
            return ((MemberExpression)e.Body).Member.Name;
        }
        internal UtfFieldImage() {
        }

        public ColumnStorage Storage { get; set; }
        public ColumnType Type { get; set; }
        public string Name { get; set; }
        public NumericUnion NumericValue { get; set; }
        public byte[] DataValue { get; set; }
        public string StringValue { get; set; }
        public int Order { get; set; }
        /// <summary>
        /// Table is a subtype of 'data' type. It requires strict byte alignment.
        /// </summary>
        public bool IsTable { get; set; }

        public byte[] StringValueBytesCache { get; set; }
        public byte[] NameBytesCache { get; set; }
        public uint NameOffset { get; set; }
        public uint StringOffset { get; set; }
        public uint DataOffset { get; set; }

        public void SetValue(object value) {
            ColumnType type;
            var union = new NumericUnion();
            if (value is byte) {
                type = ColumnType.Byte;
                union.U8 = (byte)value;
            } else if (value is sbyte) {
                type = ColumnType.SByte;
                union.S8 = (sbyte)value;
            } else if (value is ushort) {
                type = ColumnType.UInt16;
                union.U16 = (ushort)value;
            } else if (value is short) {
                type = ColumnType.Int16;
                union.S16 = (short)value;
            } else if (value is uint) {
                type = ColumnType.UInt32;
                union.U32 = (uint)value;
            } else if (value is int) {
                type = ColumnType.Int32;
                union.S32 = (int)value;
            } else if (value is ulong) {
                type = ColumnType.UInt64;
                union.U64 = (ulong)value;
            } else if (value is long) {
                type = ColumnType.Int64;
                union.S64 = (long)value;
            } else if (value is float) {
                type = ColumnType.Single;
                union.R32 = (float)value;
            } else if (value is double) {
                type = ColumnType.Double;
                union.R64 = (double)value;
            } else if (value is string) {
                type = ColumnType.String;
                StringValue = (string)value;
            } else if (value is byte[]) {
                type = ColumnType.Data;
                DataValue = (byte[])value;
            } else {
                throw new ArgumentException("Unsupported argument type.", NameOf(() => value));
            }
            Type = type;
            switch (type) {
                case ColumnType.String:
                case ColumnType.Data:
                    break;
                default:
                    NumericValue = union;
                    break;
            }
        }

        public void SetNullValue(ColumnType type) {
            switch (type) {
                case ColumnType.String:
                    StringValue = null;
                    Type = ColumnType.String;
                    break;
                case ColumnType.Data:
                    DataValue = null;
                    Type = ColumnType.Data;
                    break;
                default:
                    throw new ArgumentException("Cannot set null value with type " +type+".", NameOf(() => type));
            }
        }

        public void WriteValueTo(Stream stream) {
            switch (Type) {
                case ColumnType.Byte:
                    stream.WriteByte(NumericValue.U8);
                    break;
                case ColumnType.SByte:
                    stream.WriteSByte(NumericValue.S8);
                    break;
                case ColumnType.UInt16:
                    stream.WriteUInt16BE(NumericValue.U16);
                    break;
                case ColumnType.Int16:
                    stream.WriteInt16BE(NumericValue.S16);
                    break;
                case ColumnType.UInt32:
                    stream.WriteUInt32BE(NumericValue.U32);
                    break;
                case ColumnType.Int32:
                    stream.WriteInt32BE(NumericValue.S32);
                    break;
                case ColumnType.UInt64:
                    stream.WriteUInt64BE(NumericValue.U64);
                    break;
                case ColumnType.Int64:
                    stream.WriteInt64BE(NumericValue.S64);
                    break;
                case ColumnType.Single:
                    stream.WriteSingleBE(NumericValue.R32);
                    break;
                case ColumnType.Double:
                    stream.WriteDoubleBE(NumericValue.R64);
                    break;
                case ColumnType.String:
                    stream.WriteUInt32BE(StringOffset);
                    break;
                case ColumnType.Data:
                    stream.WriteUInt32BE(DataOffset);
                    stream.WriteUInt32BE((uint)DataValue.Length);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(NameOf(() => Type));
            }
        }

        public int CompareTo(UtfFieldImage other) {
            return this.Order - other.Order;
        }

        public override string ToString() {
            return "UtfFieldImage {{" +Name + "}}";
        }
    }
}
