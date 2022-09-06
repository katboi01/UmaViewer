namespace ClHcaSharp
{
    internal class HcaContext
    {
        public int Version { get; set; }
        public int HeaderSize { get; set; }

        public int ChannelCount { get; set; }
        public int SampleRate { get; set; }
        public int FrameCount { get; set; }
        public int EncoderDelay { get; set; }
        public int EncoderPadding { get; set; }

        public int FrameSize { get; set; }
        public int MinResolution { get; set; }
        public int MaxResolution { get; set; }
        public int TrackCount { get; set; }
        public int ChannelConfig { get; set; }
        public int StereoType { get; set; }
        public int TotalBandCount { get; set; }
        public int BaseBandCount { get; set; }
        public int StereoBandCount { get; set; }
        public int BandsPerHfrGroup { get; set; }
        public int MsStereo { get; set; }
        public int Reserved { get; set; }

        public int VbrMaxFrameSize { get; set; }
        public int VbrNoiseLevel { get; set; }

        public int AthType { get; set; }

        public int LoopStartFrame { get; set; }
        public int LoopEndFrame { get; set; }
        public int LoopStartDelay { get; set; }
        public int LoopEndPadding { get; set; }
        public bool LoopFlag { get; set; }

        public int CiphType { get; set; }
        public ulong KeyCode { get; set; }

        public float RvaVolume { get; set; }

        public int CommentLength { get; set; }
        public string Comment { get; set; }

        public int HfrGroupCount { get; set; }
        public byte[] AthCurve { get; set; }
        public byte[] CipherTable { get; set; }

        public int Random { get; set; }
        public Channel[] Channels { get; set; }
    }
}