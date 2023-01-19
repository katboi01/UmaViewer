using System.Collections.Generic;
using System.IO;
using DereTore.Common;

namespace DereTore.Exchange.Archive.ACB {
    public partial class UtfTable : DisposableBase {

        public Stream Stream { get { return _stream; } }

        public string AcbFileName  { get { return  _acbFileName; } }

        public long Offset  { get { return  _offset; } }

        public long Size  { get { return  _size; } }

        public bool IsEncrypted  { get { return  _isEncrypted; } }

        public UtfHeader Header  { get { return  _utfHeader; } }

        public Dictionary<string, UtfField>[] Rows  { get { return  _rows; } }

    }
}
