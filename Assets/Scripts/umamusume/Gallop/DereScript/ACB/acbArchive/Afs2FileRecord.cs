namespace DereTore.Exchange.Archive.ACB {
    public sealed class Afs2FileRecord {

        internal Afs2FileRecord() {
        }

        public ushort CueId { get; set; }
        public long FileOffsetRaw { get; set; }
        public long FileOffsetAligned { get; set; }
        public long FileLength { get; set; }
        public string FileName { get; set; }

    }
}
