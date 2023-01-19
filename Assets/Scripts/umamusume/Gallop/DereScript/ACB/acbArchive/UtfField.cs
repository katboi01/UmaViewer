using System;
using System.Linq.Expressions;

namespace DereTore.Exchange.Archive.ACB {
    public sealed class UtfField {

        public static string NameOf<T>(Expression<Func<T>> e)
        {
            return ((MemberExpression)e.Body).Member.Name;
        }
        internal UtfField() {
        }

        public byte Type { get; set; }
        public string Name { get; set; }
        public ColumnType ConstrainedType { get; set; }
        public NumericUnion NumericValue { get; set; }
        public byte[] DataValue { get; set; }
        public string StringValue { get; set; }
        public long Offset { get; set; }
        public long Size { get; set; }

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
            } else if (value is string) {
                type = ColumnType.String;
                StringValue = (string)value;
            } else if (value is byte[]) {
                type = ColumnType.Data;
                DataValue = (byte[])value;
            } else {
                throw new ArgumentException("Unsupported argument type.", NameOf(() => value));
            }
            ConstrainedType = type;
            Type = (byte)type;
            switch (type) {
                case ColumnType.String:
                case ColumnType.Data:
                    break;
                default:
                    NumericValue = union;
                    break;
            }
        }

        public object GetValue() {
            var constrainedType = ConstrainedType;
            object ret;
            switch (constrainedType) {
                case ColumnType.Byte:
                    ret = NumericValue.U8;
                    break;
                case ColumnType.SByte:
                    ret = NumericValue.S8;
                    break;
                case ColumnType.UInt16:
                    ret = NumericValue.U16;
                    break;
                case ColumnType.Int16:
                    ret = NumericValue.S16;
                    break;
                case ColumnType.UInt32:
                    ret = NumericValue.U32;
                    break;
                case ColumnType.Int32:
                    ret = NumericValue.S32;
                    break;
                case ColumnType.UInt64:
                    ret = NumericValue.U64;
                    break;
                case ColumnType.Int64:
                    ret = NumericValue.S64;
                    break;
                case ColumnType.Single:
                    ret = NumericValue.R32;
                    break;
                case ColumnType.Double:
                    ret = NumericValue.R64;
                    break;
                case ColumnType.String:
                    ret = StringValue;
                    break;
                case ColumnType.Data:
                    ret = DataValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(NameOf(() => constrainedType));
            }
            return ret;
        }
    }
}
