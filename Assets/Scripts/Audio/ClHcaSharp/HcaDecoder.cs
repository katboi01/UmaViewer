using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static ClHcaSharp.Constants;
using static ClHcaSharp.Header;
using static ClHcaSharp.Tables;

namespace ClHcaSharp
{
    public class HcaDecoder
    {
        private readonly HcaContext hca;

        public HcaDecoder(Stream hcaStream, ulong key)
        {
            hca = new HcaContext();

            InitializeTables();

            DecodeHeader(hca, hcaStream);
            SetKey(key);
        }

        public HcaInfo GetInfo()
        {
            HcaInfo info = new HcaInfo()
            {
                Version = hca.Version,
                HeaderSize = hca.HeaderSize,
                SamplingRate = hca.SampleRate,
                ChannelCount = hca.ChannelCount,
                BlockSize = hca.FrameSize,
                BlockCount = hca.FrameCount,
                EncoderDelay = hca.EncoderDelay,
                EncoderPadding = hca.EncoderPadding,
                LoopEnabled = hca.LoopFlag,
                LoopStartBlock = hca.LoopStartFrame,
                LoopEndBlock = hca.LoopEndFrame,
                LoopStartDelay = hca.LoopStartDelay,
                LoopEndPadding = hca.LoopEndPadding,
                SamplesPerBlock = SamplesPerFrame,
                Comment = hca.Comment,
                EncryptionEnabled = hca.CiphType == 56
            };
            return info;
        }

        public void SetKey(ulong key)
        {
            hca.KeyCode = key;
            hca.CipherTable = Cipher.Init(hca.CiphType, hca.KeyCode);
        }

        public int TestBlock(byte[] data)
        {
            const int frameSamples = Subframes * SamplesPerSubframe;
            const float scale = 32768.0f;

            int status;
            int clips = 0;
            int blanks = 0;
            int[] channelBlanks = new int[MaxChannels];

            byte[] buffer = data;

            {
                bool isEmpty = true;

                for (int i = 0x02; i < data.Length - 0x02; i++)
                {
                    if (buffer[i] != 0)
                    {
                        isEmpty = false;
                        break;
                    }
                }

                if (isEmpty) return 0;
            }

            status = DecodeBlockUnpack(data);
            if (status < 0) return -1;

            {
                int bitsMax = hca.FrameSize * 8;
                int byteStart;

                if (status + 14 > bitsMax)
                    return -1;
                    //throw new Exception("BitReader error.");

                byteStart = (status / 8) + (status % 8 > 0 ? 0x01 : 0);

                for (int i = byteStart; i < hca.FrameSize - 0x02; i++)
                {
                    if (buffer[i] != 0)
                        return -1;
                }
            }

            DecodeBlockTransform();
            for (int ch = 0; ch < hca.ChannelCount; ch++)
            {
                for (int sf = 0; sf < Subframes; sf++)
                {
                    for (int s = 0; s < SamplesPerSubframe; s++)
                    {
                        float fSample = hca.Channels[ch].Wave[sf][s];

                        if (fSample > 1.0f || fSample < -1.0f)
                            clips++;
                        else
                        {
                            int pSample = (int)(fSample * scale);
                            if (pSample == 0 || pSample == -1)
                            {
                                blanks++;
                                channelBlanks[ch]++;
                            }
                        }
                    }
                }
            }

            if (clips == 1)
                clips++;
            if (clips > 1)
                return clips;

            if (blanks == hca.ChannelCount * frameSamples)
                return 0;

            if (hca.ChannelCount >= 2)
            {
                if (channelBlanks[0] == frameSamples && channelBlanks[1] != frameSamples)
                    return 3;
            }

            return 1;
        }

        public void ReadSamples16(short[][] samples)
        {
            for (int subframe = 0; subframe < Subframes; subframe++)
            {
                for (int sample = 0; sample < SamplesPerSubframe; sample++)
                {
                    for (int channel = 0; channel < hca.ChannelCount; channel++)
                    {
                        float f = hca.Channels[channel].Wave[subframe][sample];
                        int sInt = (int)(32768 * f);
                        if (sInt > 32767)
                            sInt = 32767;
                        else if (sInt < -32767)
                            sInt = -32767;
                        samples[channel][(SamplesPerSubframe * subframe) + sample] = (short)sInt;
                    }
                }
            }
        }

        public void ReadSamples16(short[] samples)
        {
            int sampleIndex = 0;
            for (int subframe = 0; subframe < Subframes; subframe++)
            {
                for (int sample = 0; sample < SamplesPerSubframe; sample++)
                {
                    for (int channel = 0; channel < hca.ChannelCount; channel++)
                    {
                        float f = hca.Channels[channel].Wave[subframe][sample];
                        int sInt = (int)(32768 * f);
                        if (sInt > 32767)
                            sInt = 32767;
                        else if (sInt < -32767)
                            sInt = -32767;
                        samples[sampleIndex++] = (short)sInt;
                    }
                }
            }
        }

        public float[]  ReadSamples16()
        {
            List<float> data = new List<float>();
            for (int subframe = 0; subframe < Subframes; subframe++)
            {
                for (int sample = 0; sample < SamplesPerSubframe; sample++)
                {
                    for (int channel = 0; channel < hca.ChannelCount; channel++)
                    {
                        float f = hca.Channels[channel].Wave[subframe][sample];
                        data.Add(f);
                    }
                }
            }
            return data.ToArray();
        }

        public void DecodeReset()
        {
            if (hca != null)
            {
                hca.Random = DefaultRandom;

                for (int i = 0; i < hca.ChannelCount; i++)
                {
                    Channel channel = hca.Channels[i];

                    Array.Clear(channel.ImdctPrevious, 0, channel.ImdctPrevious.Length);
                }
            }
        }

        public int DecodeBlock(byte[] data)
        {
            int result = DecodeBlockUnpack(data);

            if (result < 0) return result;

            DecodeBlockTransform();

            return result;
        }

        private int DecodeBlockUnpack(byte[] data)
        {
            if (data.Length < hca.FrameSize)
                throw new ArgumentException("Data is less than expected frame size.");

            BitReader bitReader = new BitReader(data);

            ushort sync = (ushort)bitReader.Read(16);
            if (sync != 0xFFFF) throw new InvalidDataException("Sync error.");

            if (Crc.Crc16Checksum(data) > 0) throw new InvalidDataException("Checksum error.");

            Cipher.Decrypt(hca.CipherTable, data);

            int frameAcceptableNoiseLevel = bitReader.Read(9);
            int frameEvaluationBoundary = bitReader.Read(7);

            int packedNoiseLevel = (frameAcceptableNoiseLevel << 8) - frameEvaluationBoundary;

            for (int channel = 0; channel < hca.ChannelCount; channel++)
            {
                UnpackScaleFactors(hca.Channels[channel], bitReader, hca.HfrGroupCount, hca.Version);
                UnpackIntensity(hca.Channels[channel], bitReader, hca.HfrGroupCount, hca.Version);
                CalculateResolution(hca.Channels[channel], packedNoiseLevel, hca.AthCurve, hca.MinResolution, hca.MaxResolution);
                CalculateGain(hca.Channels[channel]);
            }

            for (int subframe = 0; subframe < Subframes; subframe++)
            {
                for (int channel = 0; channel < hca.ChannelCount; channel++)
                {
                    DequantizeCoefficients(hca.Channels[channel], bitReader, subframe);
                }
            }

            return bitReader.Bit;
        }

        private void DecodeBlockTransform()
        {
            for (int subframe = 0; subframe < Subframes; subframe++)
            {
                for (int channel = 0; channel < hca.ChannelCount; channel++)
                {
                    int random = hca.Random;
                    ReconstructNoise(hca.Channels[channel], hca.MinResolution, hca.MsStereo, ref random, subframe);
                    hca.Random = random;
                    ReconstructHighFrequency(hca.Channels[channel], hca.HfrGroupCount, hca.BandsPerHfrGroup,
                                             hca.StereoBandCount, hca.BaseBandCount, hca.TotalBandCount, hca.Version, subframe);
                }

                if (hca.StereoBandCount > 0)
                {
                    for (int ch = 0; ch < hca.ChannelCount - 1; ch++)
                    {
                        ApplyIntensityStereo(hca.Channels, ch * 2, subframe, hca.BaseBandCount, hca.TotalBandCount);
                        ApplyMsStereo(hca.Channels, ch * 2, hca.MsStereo, hca.BaseBandCount, hca.TotalBandCount, subframe);
                    }
                }

                for (int channel = 0; channel < hca.ChannelCount; channel++)
                {
                    ImdctTransform(hca.Channels[channel], subframe);
                }
            }
        }

        private static void UnpackScaleFactors(Channel channel, BitReader bitReader, int hfrGroupCount, int version)
        {
            int csCount = channel.CodedCount;
            int extraCount;
            byte deltaBits = (byte)bitReader.Read(3);

            if (channel.Type == ChannelType.StereoSecondary || hfrGroupCount <= 0 || version <= Version200)
                extraCount = 0;
            else
            {
                extraCount = hfrGroupCount;
                csCount += extraCount;

                if (csCount > SamplesPerSubframe)
                    return;
                    //throw new InvalidDataException("Invalid scale count.");
            }

            if (deltaBits >= 6)
            {
                for (int i = 0; i < csCount; i++)
                {
                    channel.ScaleFactors[i] = (byte)bitReader.Read(6);
                }
            }
            else if (deltaBits > 0)
            {
                byte expectedDelta = (byte)((1 << deltaBits) - 1);
                byte value = (byte)bitReader.Read(6);

                channel.ScaleFactors[0] = value;
                for (int i = 1; i < csCount; i++)
                {
                    byte delta = (byte)bitReader.Read(deltaBits);

                    if (delta == expectedDelta)
                        value = (byte)bitReader.Read(6);
                    else
                    {
                        int scaleFactorTest = value + (delta - (expectedDelta >> 1));
                        if (scaleFactorTest < 0 || scaleFactorTest >= 64)
                            return;
                            //throw new InvalidDataException("Invalid scale factor.");

                        value = (byte)(value - (expectedDelta >> 1) + delta);
                        value = (byte)(value & 0x3F);
                    }

                    channel.ScaleFactors[i] = value;
                }
            }
            else
            {
                for (int i = 0; i < SamplesPerSubframe; i++)
                {
                    channel.ScaleFactors[i] = 0;
                }
            }

            for (int i = 0; i < extraCount; i++)
            {
                channel.ScaleFactors[SamplesPerSubframe - 1 - i] = channel.ScaleFactors[csCount - i];
            }
        }

        private static void UnpackIntensity(Channel channel, BitReader bitReader, int hfrGroupCount, int version)
        {
            if (channel.Type == ChannelType.StereoSecondary)
            {
                if (version <= Version200)
                {
                    byte value = (byte)bitReader.Peek(4);

                    channel.Intensity[0] = value;
                    if (value < 15)
                    {
                        bitReader.Skip(4);
                        for (int i = 1; i < Subframes; i++)
                        {
                            channel.Intensity[i] = (byte)bitReader.Read(4);
                        }
                    }
                }
                else
                {
                    byte value = (byte)bitReader.Peek(4);

                    if (value < 15)
                    {
                        bitReader.Skip(4);

                        byte deltaBits = (byte)bitReader.Read(2);

                        channel.Intensity[0] = value;
                        if (deltaBits == 3)
                        {
                            for (int i = 1; i < Subframes; i++)
                            {
                                channel.Intensity[i] = (byte)bitReader.Read(4);
                            }
                        }
                        else
                        {
                            byte bMax = (byte)((2 << deltaBits) - 1);
                            byte bits = (byte)(deltaBits + 1);

                            for (int i = 1; i < Subframes; i++)
                            {
                                byte delta = (byte)bitReader.Read(bits);
                                if (delta == bMax)
                                    value = (byte)bitReader.Read(4);
                                else
                                {
                                    value = (byte)(value - (bMax >> 1) + delta);
                                    if (value > 15)
                                        return;
                                        //throw new InvalidDataException("Intensity value out of range.");
                                }

                                channel.Intensity[i] = value;
                            }
                        }
                    }
                    else
                    {
                        bitReader.Skip(4);
                        for (int i = 0; i < Subframes; i++)
                        {
                            channel.Intensity[i] = 7;
                        }
                    }
                }
            }
            else
            {
                if (version <= Version200)
                {
                    byte[] hfrScales = channel.ScaleFactors;
                    int hfrScalesOffset = 128 - hfrGroupCount;

                    for (int i = 0; i < hfrGroupCount; i++)
                    {
                        hfrScales[hfrScalesOffset + i] = (byte)bitReader.Read(6);
                    }
                }
            }
        }

        private static void CalculateResolution(Channel channel, int packedNoiseLevel, byte[] athCurve, int minResolution, int maxResolution)
        {
            int crCount = channel.CodedCount;
            int noiseCount = 0;
            int validCount = 0;

            for (int i = 0; i < crCount; i++)
            {
                byte newResolution = 0;
                byte scaleFactor = channel.ScaleFactors[i];

                if (scaleFactor > 0)
                {
                    int noiseLevel = athCurve[i] + ((packedNoiseLevel + i) >> 8);
                    int curvePosition = noiseLevel + 1 - ((5 * scaleFactor) >> 1);

                    if (curvePosition < 0)
                        newResolution = 15;
                    else if (curvePosition <= 65)
                        newResolution = InvertTable[curvePosition];
                    else
                        newResolution = 0;

                    if (newResolution > maxResolution)
                        newResolution = (byte)maxResolution;
                    else if (newResolution < minResolution)
                        newResolution = (byte)minResolution;

                    if (newResolution < 1)
                    {
                        channel.Noises[noiseCount] = (byte)i;
                        noiseCount++;
                    }
                    else
                    {
                        channel.Noises[SamplesPerSubframe - 1 - validCount] = (byte)i;
                        validCount++;
                    }
                }
                channel.Resolution[i] = newResolution;
            }

            channel.NoiseCount = noiseCount;
            channel.ValidCount = validCount;

            Array.Clear(channel.Resolution, crCount, SamplesPerSubframe - crCount);
        }

        private static void CalculateGain(Channel channel)
        {
            int cgCount = channel.CodedCount;
            for (int i = 0; i < cgCount; i++)
            {
                float scaleFactorScale = DequantizerScalingTable[channel.ScaleFactors[i]];
                float resolutionScale = DequantizerRangeTable[channel.Resolution[i]];
                channel.Gain[i] = scaleFactorScale * resolutionScale;
            }
        }

        private static void DequantizeCoefficients(Channel channel, BitReader bitReader, int subframe)
        {
            int ccCount = channel.CodedCount;

            for (int i = 0; i < ccCount; i++)
            {
                float qc;
                byte resolution = channel.Resolution[i];
                byte bits = MaxBitTable[resolution];
                int code = bitReader.Read(bits);

                if (resolution > 7)
                {
                    int signedCode = (1 - ((code & 1) << 1)) * (code >> 1);
                    if (signedCode == 0)
                        bitReader.Skip(-1);
                    qc = signedCode;
                }
                else
                {
                    int index = (resolution << 4) + code;
                    int skip = ReadBitTable[index] - bits;
                    bitReader.Skip(skip);
                    qc = ReadValueTable[index];
                }

                channel.Spectra[subframe][i] = channel.Gain[i] * qc;
            }

            Array.Clear(channel.Spectra[subframe], ccCount, SamplesPerSubframe - ccCount);
        }

        private static void ReconstructNoise(Channel channel, int minResolution, int msStereo, ref int random, int subframe)
        {
            if (minResolution > 0) return;
            if (channel.ValidCount <= 0 || channel.NoiseCount <= 0) return;
            if (msStereo != 0 && channel.Type == ChannelType.StereoPrimary) return;

            for (int i = 0; i < channel.NoiseCount; i++)
            {
                random = 0x343FD * random + 0x269EC3;

                int randomIndex = SamplesPerSubframe - channel.ValidCount + (((random & 0x7FFF) * channel.ValidCount) >> 15);

                int noiseIndex = channel.Noises[i];
                int validIndex = channel.Noises[randomIndex];

                int sfNoise = channel.ScaleFactors[noiseIndex];
                int sfValid = channel.ScaleFactors[validIndex];
                int scIndex = (sfNoise - sfValid + 62) & ~((sfNoise - sfValid + 62) >> 31);

                channel.Spectra[subframe][noiseIndex] = ScaleConversionTable[scIndex] * channel.Spectra[subframe][validIndex];
            }
        }

        private static void ReconstructHighFrequency(Channel channel, int hfrGroupCount, int bandsPerHfrGroup,
                                             int stereoBandCount, int baseBandCount, int totalBandCount, int version, int subframe)
        {
            if (bandsPerHfrGroup == 0) return;
            if (channel.Type == ChannelType.StereoSecondary) return;

            int groupLimit;
            int startBand = stereoBandCount + baseBandCount;
            int highBand = startBand;
            int lowBand = startBand - 1;

            int hfrScalesOffset = 128 - hfrGroupCount;
            byte[] hfrScales = channel.ScaleFactors;

            if (version <= Version200)
                groupLimit = hfrGroupCount;
            else
            {
                groupLimit = hfrGroupCount >= 0 ? hfrGroupCount : hfrGroupCount + 1;
                groupLimit >>= 1;
            }

            for (int group = 0; group < hfrGroupCount; group++)
            {
                int lowBandSub = group < groupLimit ? 1 : 0;

                for (int i = 0; i < bandsPerHfrGroup; i++)
                {
                    if (highBand >= totalBandCount || lowBand < 0) break;

                    int scIndex = hfrScales[hfrScalesOffset + group];
                    scIndex &= ~(scIndex >> 31);

                    channel.Spectra[subframe][highBand] = ScaleConversionTable[scIndex] * channel.Spectra[subframe][lowBand];

                    highBand++;
                    lowBand -= lowBandSub;
                }
            }

            channel.Spectra[subframe][highBand - 1] = 0.0f;
        }

        private static void ApplyIntensityStereo(Channel[] channelPair, int channelOffset, int subframe, int baseBandCount, int totalBandCount)
        {
            if (channelPair[channelOffset + 0].Type != ChannelType.StereoPrimary) return;

            float ratioL = IntensityRatioTable[channelPair[channelOffset + 1].Intensity[subframe]];
            float ratioR = 2.0f - ratioL;
            float[] spectraL = channelPair[channelOffset + 0].Spectra[subframe];
            float[] spectraR = channelPair[channelOffset + 1].Spectra[subframe];

            for (int band = baseBandCount; band < totalBandCount; band++)
            {
                float coefL = spectraL[band] * ratioL;
                float coefR = spectraR[band] * ratioR;
                spectraL[band] = coefL;
                spectraR[band] = coefR;
            }
        }

        private static void ApplyMsStereo(Channel[] channelPair, int channelOffset, int msStereo, int baseBandCount, int totalBandCount, int subframe)
        {
            if (msStereo != 0) return;
            if (channelPair[channelOffset + 0].Type != ChannelType.StereoPrimary) return;

            float ratio = MsStereoRatio;
            float[] spectraL = channelPair[channelOffset + 0].Spectra[subframe];
            float[] spectraR = channelPair[channelOffset + 1].Spectra[subframe];

            for (int band = baseBandCount; band < totalBandCount; band++)
            {
                float coefL = (spectraL[band] + spectraR[band]) * ratio;
                float coefR = (spectraL[band] - spectraR[band]) * ratio;
                spectraL[band] = coefL;
                spectraR[band] = coefR;
            }
        }

        private static void ImdctTransform(Channel channel, int subframe)
        {
            const int size = SamplesPerSubframe;
            const int half = SamplesPerSubframe / 2;
            const int mdctBits = MdctBits;

            int count1 = 1;
            int count2 = half;
            float[] temp1 = channel.Spectra[subframe];
            float[] temp2 = channel.Temp;
            int temp1Index = 0;

            for (int i = 0; i < mdctBits; i++)
            {
                float[] swap;
                float[] d = temp2;
                int d1Index = 0;
                int d2Index = count2;

                for (int j = 0; j < count1; j++)
                {
                    for (int k = 0; k < count2; k++)
                    {
                        float a = temp1[temp1Index++];
                        float b = temp1[temp1Index++];
                        d[d1Index++] = a + b;
                        d[d2Index++] = a - b;
                    }

                    d1Index += count2;
                    d2Index += count2;
                }

                temp1Index -= SamplesPerSubframe;
                swap = temp1;
                temp1 = temp2;
                temp2 = swap;

                count1 <<= 1;
                count2 >>= 1;
            }

            count1 = half;
            count2 = 1;
            temp1 = channel.Temp;
            temp2 = channel.Spectra[subframe];

            for (int i = 0; i < mdctBits; i++)
            {
                int sinTableIndex = 0;
                int cosTableIndex = 0;

                float[] swap;
                float[] d = temp2;
                float[] s = temp1;
                int d1Index = 0;
                int d2Index = count2 * 2 - 1;
                int s1Index = 0;
                int s2Index = count2;

                for (int j = 0; j < count1; j++)
                {
                    for (int k = 0; k < count2; k++)
                    {
                        float a = s[s1Index++];
                        float b = s[s2Index++];
                        float sin = SinTable[i][sinTableIndex++];
                        float cos = CosTable[i][cosTableIndex++];
                        d[d1Index++] = a * sin - b * cos;
                        d[d2Index--] = a * cos + b * sin;
                    }

                    s1Index += count2;
                    s2Index += count2;
                    d1Index += count2;
                    d2Index += count2 * 3;
                }

                swap = temp1;
                temp1 = temp2;
                temp2 = swap;

                count1 >>= 1;
                count2 <<= 1;
            }

            float[] dct = channel.Spectra[subframe];
            float[] prev = channel.ImdctPrevious;
            for (int i = 0; i < half; i++)
            {
                channel.Wave[subframe][i] = ImdctWindow[i] * dct[i + half] + prev[i];
                channel.Wave[subframe][i + half] = ImdctWindow[i + half] * dct[size - 1 - i] - prev[i + half];
                channel.ImdctPrevious[i] = ImdctWindow[size - 1 - i] * dct[half - i - 1];
                channel.ImdctPrevious[i + half] = ImdctWindow[half - i - 1] * dct[i];
            }
        }
    }
}