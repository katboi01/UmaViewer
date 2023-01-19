namespace DereTore.Exchange.Archive.ACB {
    public sealed class UtfHeader {

        internal UtfHeader() {
        }

        public uint TableSize { get; set; }
        public ushort Unknown1 { get; set; }
        public uint PerRowDataOffset { get; set; }
        public uint StringTableOffset { get; set; }
        public uint ExtraDataOffset { get; set; }
        public uint TableNameOffset { get; set; }
        public string TableName { get; set; }
        public ushort FieldCount { get; set; }
        public ushort RowSize { get; set; }
        public uint RowCount { get; set; }

    }
}
