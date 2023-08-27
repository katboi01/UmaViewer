using CriWareFormats.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CriWareFormats
{
    internal class Row
    {
        public string Name { get; set; }
        public ushort Id { get; set; }
    }

    public struct Wave
    {
        public int WaveId { get; set; }
        public long Offset { get; set; }
        public long Length { get; set; }
    }

    public sealed class AwbReader : IDisposable
    {
        private readonly BinaryReader binaryReader;
        private readonly long offset;

        private readonly byte offsetSize;
        private readonly ushort waveIdAlignment;
        private readonly int totalSubsongs;
        private readonly ushort offsetAlignment;
        private readonly ushort subkey;

        private readonly List<Row> cueNames;
        private readonly List<Wave> waves;

        public AwbReader(Stream awbStream, bool isEmbedded) : this(awbStream, 0)
        {
            IsEmbedded = isEmbedded;
        }

        public AwbReader(Stream awbStream) : this(awbStream, 0) { }

        public AwbReader(Stream awbStream, long positionOffset)
        {
            binaryReader = new BinaryReader(awbStream);
            offset = positionOffset;

            binaryReader.BaseStream.Position = offset;

            if (!binaryReader.ReadChars(4).SequenceEqual("AFS2"))
                throw new InvalidDataException("Incorrect magic.");

            binaryReader.BaseStream.Position += 0x1;
            offsetSize = binaryReader.ReadByte();
            waveIdAlignment = binaryReader.ReadUInt16();
            totalSubsongs = binaryReader.ReadInt32();
            offsetAlignment = binaryReader.ReadUInt16();
            subkey = binaryReader.ReadUInt16();

            waves = new List<Wave>(totalSubsongs);

            for (int subsong = 1; subsong <= totalSubsongs; subsong++)
            {
                long currentOffset = 0x10;

                long waveIdOffset = currentOffset + (subsong - 1) * waveIdAlignment;

                binaryReader.BaseStream.Position = offset + waveIdOffset;

                int waveId = binaryReader.ReadUInt16();

                currentOffset += totalSubsongs * waveIdAlignment;

                long subfileOffset = 0;
                long subfileNext = 0;
                long fileSize = binaryReader.BaseStream.Length;

                currentOffset += (subsong - 1) * offsetSize;

                binaryReader.BaseStream.Position = offset + currentOffset;

                switch (offsetSize)
                {
                    case 0x4:
                        subfileOffset = binaryReader.ReadUInt32();
                        subfileNext = binaryReader.ReadUInt32();
                        break;

                    case 0x2:
                        subfileOffset = binaryReader.ReadUInt16();
                        subfileNext = binaryReader.ReadUInt16();
                        break;

                    default:
                        Fail();
                        break;
                }

                subfileOffset += subfileOffset % offsetAlignment > 0 ?
                    offsetAlignment - subfileOffset % offsetAlignment : 0;
                subfileNext += subfileNext % offsetAlignment > 0 && subfileNext < fileSize ?
                    offsetAlignment - subfileNext % offsetAlignment : 0;
                long subfileSize = subfileNext - subfileOffset;

                waves.Add(new Wave()
                {
                    WaveId = waveId,
                    Offset = subfileOffset,
                    Length = subfileSize
                });

                //if (CueNames.Count > 0) Console.Write("{0,-36}", CueNames.Where(row => row.Index == waveId).First().Name);
                //else Console.Write(waveId);

                //Console.WriteLine(subfileOffset);
            }

            cueNames = new List<Row>();
        }

        public ushort Subkey => subkey;

        public bool IsEmbedded { get; }

        public List<Wave> Waves => waves;

        public Stream GetWaveSubfileStream(Wave wave)
        {
            return new SpliceStream(binaryReader.BaseStream, offset + wave.Offset, wave.Length);
        }

        private static void Fail()
        {
            throw new Exception("Failure reading AWB file.");
        }

        public void Dispose()
        {
            binaryReader.Dispose();
        }
    }
}