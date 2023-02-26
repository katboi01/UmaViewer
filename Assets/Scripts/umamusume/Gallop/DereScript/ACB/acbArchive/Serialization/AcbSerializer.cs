using System;
using System.IO;
using System.Linq.Expressions;
using DereTore.Common;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    public sealed partial class AcbSerializer {

        public static string NameOf<T>(Expression<Func<T>> e)
        {
            return ((MemberExpression)e.Body).Member.Name;
        }

        public AcbSerializer() {
            Alignment = 32;
        }

        public void Serialize<T>(T[] tableRows, Stream serializationStream) where T : UtfRowBase {
            if (tableRows == null) {
                throw new ArgumentNullException(NameOf(() => tableRows));
            }
            var tableData = GetTableBytes(tableRows).RoundUpTo(Alignment);
            serializationStream.WriteBytes(tableData);
        }

        public uint Alignment {
            get {
                return _alignment;
            }
            set {
                if (value <= 0 || value % 16 != 0) {
                    throw new ArgumentException("Alignment should be a positive integer, also a multiple of 16.", NameOf(() => value));
                }
                _alignment = value;
            }
        }

        private uint _alignment;

    }
}
