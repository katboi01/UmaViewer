using System;
using System.IO;
using System.Linq.Expressions;

namespace DereTore.Common {
    public sealed class EndianBinaryWriter : BinaryWriter {

        public EndianBinaryWriter(Stream stream, Endian endian)
            : base(stream) {
            Endian = endian;
        }
        public static string NameOf<T>(Expression<Func<T>> e)
        {
            return ((MemberExpression)e.Body).Member.Name;
        }

        public Endian Endian { get; set; }

        public void Seek(long position, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                    Position = position;
                    break;
                case SeekOrigin.Current:
                    Position += position;
                    break;
                case SeekOrigin.End:
                    Position = BaseStream.Length - position;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(NameOf(() => origin), origin, null);
            }
        }

        public long Position {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

        public override void Write(short value) {
            if (Endian != SystemEndian.Type) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(int value) {
            if (Endian != SystemEndian.Type) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(long value) {
            if (Endian != SystemEndian.Type) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(ushort value) {
            if (Endian != SystemEndian.Type) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(uint value) {
            if (Endian != SystemEndian.Type) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(ulong value) {
            if (Endian != SystemEndian.Type) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(float value) {
            if (Endian != SystemEndian.Type) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

        public override void Write(double value) {
            if (Endian != SystemEndian.Type) {
                value = DereToreHelper.SwapEndian(value);
                base.Write(value);
            } else {
                base.Write(value);
            }
        }

    }
}
