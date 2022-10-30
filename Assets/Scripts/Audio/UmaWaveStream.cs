using NAudio.Wave;
using System;
using UnityEngine;
using System.IO;
using CriWareFormats;
using ClHcaSharp;

namespace UmaMusumeAudio
{
    public class UmaWaveStream : WaveStream
    {
        /*
         * Open Umamusume > umamusume_Data > resources.assets with a hex editor.
         * Check the results when finding the string "StreamingAssets".
         * The key should be near a string named "cri_auth". 
         */
        private const ulong umaMusumeKey = 75923756697503;

        private readonly HcaWaveStream hcaWaveStream;

        public UmaWaveStream(AwbReader awbReader, int waveId)
        {
            Stream awbSubfile = awbReader.GetWaveSubfileStream(awbReader.Waves.Find((wave) => wave.WaveId == waveId));
            hcaWaveStream = new HcaWaveStream(awbSubfile, MixKey(umaMusumeKey, awbReader.Subkey));
        }

        public override WaveFormat WaveFormat => hcaWaveStream.WaveFormat;

        public bool Loop { get => hcaWaveStream.Loop; set { hcaWaveStream.Loop = value; } }

        public long LoopStartSample { get => hcaWaveStream.LoopStartSample; set { hcaWaveStream.LoopStartSample = value; } }

        public long LoopEndSample { get => hcaWaveStream.LoopEndSample; set { hcaWaveStream.LoopEndSample = value; } }

        public override long Length => hcaWaveStream.Length;

        public override long Position { get => hcaWaveStream.Position; set => hcaWaveStream.Position = value; }

        public short[][] SampleBuffer => hcaWaveStream.SampleBuffer;

        public HcaDecoder Decoder => hcaWaveStream.Decoder;
        public HcaInfo Info => hcaWaveStream.Info;

        public override int Read(byte[] buffer, int offset, int count)
        {
            return hcaWaveStream.Read(buffer, offset, count);
        }

        public void ResetLoop() => hcaWaveStream.ResetLoop();

        private static ulong MixKey(ulong key, ushort subkey) =>
            key * (((ulong)subkey << 16) | ((ushort)~subkey + 2u));
    }
}
