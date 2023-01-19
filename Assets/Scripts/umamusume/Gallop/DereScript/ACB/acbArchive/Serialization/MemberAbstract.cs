using System.Reflection;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    internal sealed class MemberAbstract {

        internal MemberAbstract(FieldInfo fieldInfo, UtfFieldAttribute fieldAttribute, Afs2ArchiveAttribute archiveAttribute) {
            _FieldInfo = fieldInfo;
            _FieldAttribute = fieldAttribute;
            _ArchiveAttribute = archiveAttribute;
        }

        public FieldInfo FieldInfo { get { return _FieldInfo; } }
        private FieldInfo _FieldInfo;

        public UtfFieldAttribute FieldAttribute { get { return _FieldAttribute; } }
        private UtfFieldAttribute _FieldAttribute;

        public Afs2ArchiveAttribute ArchiveAttribute { get { return _ArchiveAttribute; } }
        private Afs2ArchiveAttribute _ArchiveAttribute;

    }
}
