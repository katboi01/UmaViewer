using Assets.Scripts.Audio.CriWareFormats.Common;
using System;

namespace ClHcaSharp
{
    internal static class Constants
    {
        public const int Version101 = 0x0101;
        public const int Version102 = 0x0102;
        public const int Version103 = 0x0103;
        public const int Version200 = 0x0200;
        public const int Version300 = 0x0300;

        public const int MinFrameSize = 0x8;
        public const int MaxFrameSize = 0xFFFF;

        public const int Mask = 0x7F7F7F7F;
        public const int Subframes = 8;
        public const int SamplesPerSubframe = 128;
        public const int SamplesPerFrame = Subframes * SamplesPerSubframe;
        public const int MdctBits = 7;

        public const int MinChannels = 1;
        public const int MaxChannels = 16;
        public const int MinSampleRate = 1;
        public const int MaxSampleRate = 0x7FFFFF;

        public const int DefaultRandom = 1;

        public static readonly float MsStereoRatio = BinaryPrimitives.Int32BitsToSingle(0x3F3504F3);
        

    }


    public enum ChannelType
    {
        Discrete = 0,
        StereoPrimary = 1,
        StereoSecondary = 2
    }
}