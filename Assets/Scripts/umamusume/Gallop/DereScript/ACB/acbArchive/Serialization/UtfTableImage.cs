using System.Collections.Generic;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    internal sealed partial class UtfTableImage {

        internal UtfTableImage(string name, uint alignment) {
            _Rows = new List<List<UtfFieldImage>>();
            _TableName = name;
            _Alignment = alignment;
        }

        public string TableName { get { return _TableName; } }
        private string _TableName;

        public List<List<UtfFieldImage>> Rows { get { return _Rows; } }
        private List<List<UtfFieldImage>> _Rows;

        public uint Alignment { get { return _Alignment; } }
        private uint _Alignment;

        private byte[] TableNameBytesCache { get; set; }

        public override string ToString() {
            return "UtfTableImage {{" +TableName+"}}";
        }
    }
}
