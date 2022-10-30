namespace ClHcaSharp
{
    public class HcaInfo
    {
        public int Version { get; set; }
        public int HeaderSize { get; set; }
        public int SamplingRate { get; set; }
        public int ChannelCount { get; set; }
        public int BlockSize { get; set; }
        public int BlockCount { get; set; }
        public int EncoderDelay { get; set; }
        public int EncoderPadding { get; set; }
        public bool LoopEnabled { get; set; }
        public int LoopStartBlock { get; set; }
        public int LoopEndBlock { get; set; }
        public int LoopStartDelay { get; set; }
        public int LoopEndPadding { get; set; }
        public int SamplesPerBlock { get; set; }
        public string Comment { get; set; }
        public bool EncryptionEnabled { get; set; }

        public int SampleCount => BlockCount * SamplesPerBlock - EncoderDelay - EncoderPadding;
        public int LoopStartSample => LoopStartBlock * SamplesPerBlock - EncoderDelay + LoopStartDelay;
        public int LoopEndSample => LoopEndBlock * SamplesPerBlock - EncoderDelay + (SamplesPerBlock - LoopEndPadding);
    }
}