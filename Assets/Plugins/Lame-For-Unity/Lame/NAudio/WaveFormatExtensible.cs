﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using NAudio.Dmo.WZT;

namespace NAudio.Wave.WZT
{
    /// <summary>
    /// WaveFormatExtensible
    /// http://www.microsoft.com/whdc/device/audio/multichaud.mspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]	
    public class WaveFormatExtensible : WaveFormat
    {        
        short wValidBitsPerSample; // bits of precision, or is wSamplesPerBlock if wBitsPerSample==0
        int dwChannelMask; // which channels are present in stream
        Guid subFormat;

        /// <summary>
        /// Parameterless constructor for marshalling
        /// </summary>
        WaveFormatExtensible()
        {
        }

        /// <summary>
        /// Creates a new WaveFormatExtensible for PCM or IEEE
        /// </summary>
        public WaveFormatExtensible(int rate, int bits, int channels)
            : base(rate, bits, channels)
        {
            waveFormatTag = WaveFormatEncoding.Extensible;
            extraSize = 22;
            wValidBitsPerSample = (short) bits;
            for (int n = 0; n < channels; n++)
            {
                dwChannelMask |= (1 << n);
            }
            if (bits == 32)
            {
                // KSDATAFORMAT_SUBTYPE_IEEE_FLOAT
                subFormat = AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT; // new Guid("00000003-0000-0010-8000-00aa00389b71");
            }
            else
            {
                // KSDATAFORMAT_SUBTYPE_PCM
                subFormat = AudioMediaSubtypes.MEDIASUBTYPE_PCM; // new Guid("00000001-0000-0010-8000-00aa00389b71");
            }

        }

        /// <summary>
        /// SubFormat (may be one of AudioMediaSubtypes)
        /// </summary>
        public Guid SubFormat { get { return subFormat; } }

        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="writer"></param>
        public override void Serialize(System.IO.BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(wValidBitsPerSample);
            writer.Write(dwChannelMask);
            byte[] guid = subFormat.ToByteArray();
            writer.Write(guid, 0, guid.Length);
        }

        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0} wBitsPerSample:{1} dwChannelMask:{2} subFormat:{3} extraSize:{4}",
                base.ToString(),
                wValidBitsPerSample,
                dwChannelMask,
                subFormat,
                extraSize);
        }
    }
}
