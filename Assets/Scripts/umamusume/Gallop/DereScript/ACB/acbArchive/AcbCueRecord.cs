namespace DereTore.Exchange.Archive.ACB {
    public sealed class AcbCueRecord {

        internal AcbCueRecord() {
        }

        public uint CueId { get; set; }
        public byte ReferenceType { get; set; }
        public ushort ReferenceIndex { get; set; }

        public bool IsWaveformIdentified { get; set; }
        public ushort WaveformIndex { get; set; }
        public ushort WaveformId { get; set; }
        public byte EncodeType { get; set; }
        public bool IsStreaming { get; set; }

        public string CueName { get; set; }

    }
}
