using ClHcaSharp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmaMusumeAudio
{
    public class HcaWaveStream : WaveStream
    {
        private readonly Stream hcaFileStream;
        private readonly BinaryReader hcaFileReader;
        private readonly HcaDecoder decoder;
        private readonly HcaInfo info;
        private readonly long dataStart;
        private readonly object positionLock = new object();

        private readonly short[][] sampleBuffer;

        private long samplePosition;

        public HcaWaveStream(Stream hcaFile, ulong key)
        {
            hcaFileStream = hcaFile;
            hcaFileReader = new BinaryReader(hcaFile);
            decoder = new HcaDecoder(hcaFile, key);
            info = decoder.GetInfo();
            dataStart = hcaFile.Position;

            sampleBuffer = new short[info.ChannelCount][];
            for (int i = 0; i < info.ChannelCount; i++)
            {
                sampleBuffer[i] = new short[info.SamplesPerBlock];
            }

            Loop = info.LoopEnabled;
            LoopStartSample = info.LoopStartSample;
            LoopEndSample = info.LoopEndSample;

            WaveFormat = new WaveFormat(info.SamplingRate, info.ChannelCount);

            samplePosition = info.EncoderDelay;
            FillBuffer(samplePosition);
        }

        public HcaInfo Info => Info1;

        public bool Loop { get; set; }

        public long LoopStartSample { get; set; }

        public long LoopEndSample { get; set; }

        public override WaveFormat WaveFormat { get; }

        public override long Length => Info1.SampleCount * Info1.ChannelCount * sizeof(short);

        public override long Position
        {
            get
            {
                lock (positionLock)
                {
                    return (samplePosition - Info1.EncoderDelay) * Info1.ChannelCount * sizeof(short);
                }
            }
            set
            {
                lock (positionLock)
                {
                    samplePosition = value / Info1.ChannelCount / sizeof(short);
                    samplePosition += Info1.EncoderDelay;

                    if (Position < Length) FillBuffer(samplePosition);
                }
            }
        }

        public HcaInfo Info1 => info;

        public short[][] SampleBuffer => sampleBuffer;

        public HcaDecoder Decoder => decoder;

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (positionLock)
            {
                int read = 0;

                for (int i = 0; i < count / Info1.ChannelCount / sizeof(short); i++)
                {
                    if (samplePosition - Info1.EncoderDelay >= LoopEndSample && Loop)
                    {
                        samplePosition = LoopStartSample + Info1.EncoderDelay;
                        FillBuffer(samplePosition);
                    }
                    else if (Position >= Length) break;

                    if (samplePosition % Info1.SamplesPerBlock == 0) FillBuffer(samplePosition);

                    for (int j = 0; j < Info1.ChannelCount; j++)
                    {
                        int bufferOffset = (i * Info1.ChannelCount + j) * sizeof(short);
                        buffer[offset + bufferOffset] = (byte)SampleBuffer[j][samplePosition % Info1.SamplesPerBlock];
                        buffer[offset + bufferOffset + 1] = (byte)(SampleBuffer[j][samplePosition % Info1.SamplesPerBlock] >> 8);

                        read += sizeof(short);
                    }

                    samplePosition++;
                }

                return read;
            }
        }

        public void ResetLoop()
        {
            Loop = Info1.LoopEnabled;
            LoopStartSample = Info1.LoopStartSample;
            LoopEndSample = Info1.LoopEndSample;
        }

        private void FillBuffer(long sample)
        {
            int block = (int)(sample / Info1.SamplesPerBlock);
            FillBuffer(block);
        }

        private void FillBuffer(int block)
        {
            if (block >= 0) hcaFileStream.Position = dataStart + block * Info1.BlockSize;

            if (hcaFileStream.Position < hcaFileStream.Length)
            {
                byte[] blockBytes = hcaFileReader.ReadBytes(Info1.BlockSize);
                if (blockBytes.Length > 0)
                {
                    Decoder.DecodeBlock(blockBytes);
                    Decoder.ReadSamples16(SampleBuffer);
                }
            }
        }

        //private void FillBuffer()
        //{
        //    if (hcaFileStream.Position >= hcaFileStream.Length)
        //        hcaFileStream.Position = dataStart;

        //    byte[] blockBytes = hcaFileReader.ReadBytes(info.BlockSize);
        //    if (blockBytes.Length > 0)
        //    {
        //        decoder.DecodeBlock(blockBytes);
        //        decoder.ReadSamples16(sampleBuffer);
        //    }
        //    else
        //    {
        //        for (int i = 0; i < sampleBuffer.Length; i++)
        //        {
        //            for (int j = 0; j < sampleBuffer[i].Length; j++)
        //            {
        //                sampleBuffer[i][j] = 0;
        //            }
        //        }
        //    }
        //}
    }
}
