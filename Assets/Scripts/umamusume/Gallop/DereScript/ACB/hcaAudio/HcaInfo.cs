namespace DereTore.Exchange.Audio.HCA {
    public struct HcaInfo {

        public ushort Version;
        public uint DataOffset;
        public uint ChannelCount;
        public uint SamplingRate;
        public uint BlockCount;
        public ushort FmtR01;
        public ushort FmtR02;
        public uint BlockSize;
        public ushort CompR01;
        public ushort CompR02;
        public ushort CompR03;
        public ushort CompR04;
        public ushort CompR05;
        public ushort CompR06;
        public ushort CompR07;
        public ushort CompR08;
        public uint CompR09;
        public ushort VbrR01;
        public ushort VbrR02;
        public ushort AthType;
        public uint LoopStart;
        public uint LoopEnd;
        public ushort LoopR01;
        public ushort LoopR02;
        public bool LoopFlag;
        public CipherType CipherType;
        public float RvaVolume;
        public uint CommentLength;
        public byte[] Comment;

    }
}
