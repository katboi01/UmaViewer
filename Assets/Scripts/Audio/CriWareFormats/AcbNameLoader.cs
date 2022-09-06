using CriWareFormats.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriWareFormats
{
    struct CueName
    {
        public ushort CueIndex;
        public string Name;
    }

    struct Cue
    {
        public byte ReferenceType;
        public ushort ReferenceIndex;
    }

    struct BlockSequence
    {
        public ushort NumTracks;
        public uint TrackIndexOffset;
        public uint TrackIndexSize;
        public ushort NumBlocks;
        public uint BlockIndexOffset;
        public uint BlockIndexSize;
    }

    struct Block
    {
        public ushort NumTracks;
        public uint TrackIndexOffset;
        public uint TrackIndexSize;
    }

    struct Sequence
    {
        public ushort NumTracks;
        public uint TrackIndexOffset;
        public uint TrackIndexSize;
        public byte Type;
    }

    struct Track
    {
        public ushort EventIndex;
    }

    struct TrackCommand
    {
        public uint CommandOffset;
        public uint CommandSize;
    }

    struct Synth
    {
        public byte Type;
        public uint ReferenceItemsOffset;
        public uint ReferenceItemsSize;
    }

    struct Waveform
    {
        public ushort Id;
        public ushort PortNo;
        public byte Streaming;
    }

    public class AcbNameLoader
    {
        private Stream acbStream;

        private UtfTable header;
        private UtfTable cueNames;

        private BinaryReaderEndian cueReader;
        private BinaryReaderEndian cueNameReader;
        private BinaryReaderEndian blockSequenceReader;
        private BinaryReaderEndian blockReader;
        private BinaryReaderEndian sequenceReader;
        private BinaryReaderEndian trackReader;
        private BinaryReaderEndian trackCommandReader;
        private BinaryReaderEndian synthReader;
        private BinaryReaderEndian waveformReader;

        private Cue[] cue;
        private CueName[] cueName;
        private BlockSequence[] blockSequence;
        private Block[] block;
        private Sequence[] sequence;
        private Track[] track;
        private TrackCommand[] trackCommand;
        private Synth[] synth;
        private Waveform[] waveform;

        private int cueRows;
        private int cueNameRows;
        private int blockSequenceRows;
        private int blockRows;
        private int sequenceRows;
        private int trackRows;
        private int trackCommandRows;
        private int synthRows;
        private int waveFormRows;

        private bool isMemory;
        private int targetWaveId;
        private int targetPort;

        private int synthDepth;
        private int sequenceDepth;

        private short cueNameIndex;
        private string cueNameName;
        private int awbNameCount;
        private List<short> awbNameList;
        private string name;

        public AcbNameLoader(Stream acb)
        {
            acbStream = acb;
            header = new UtfTable(acb, (uint)acbStream.Position);
        }

        bool OpenUtfSubtable(out BinaryReaderEndian tableReader, out UtfTable table, string tableName, out int rows)
        {
            if (!header.Query(0, tableName, out VLData data))
                throw new ArgumentException("Error reading table.");

            tableReader = new BinaryReaderEndian(acbStream);
            table = new UtfTable(tableReader.BaseStream, data.Offset, out rows, out string _);

            return true;
        }

        void AddAcbName(byte streaming)
        {
            if (cueNameName.Length == 0)
                return;

            for (int i = 0; i < awbNameCount; i++)
            {
                if (awbNameList[i] == cueNameIndex)
                    return;
            }

            if (awbNameCount > 0)
            {
                name += "; ";
                name += cueNameName;
            }
            else
                name = cueNameName;

            if (streaming == 2 && isMemory)
                name += " [pre]";

            awbNameList.Add(cueNameIndex);
            awbNameCount++;
        }

        void PreloadAcbWaveForm()
        {
            ref int rows = ref waveFormRows;

            if (rows != 0) return;
            if (!OpenUtfSubtable(out waveformReader, out UtfTable table, "WaveformTable", out rows))
                throw new Exception("Failure opening Waveform table.");
            if (rows == 0) return;

            waveform = new Waveform[rows];

            int cId = table.GetColumn("Id");
            int cMemoryAwbId = table.GetColumn("MemoryAwbId");
            int cStreamAwbId = table.GetColumn("StreamAwbId");
            int cStreamAwbPortNo = table.GetColumn("StreamAwbPortNo");
            int cStreaming = table.GetColumn("Streaming");

            for (int i = 0; i < rows; i++)
            {
                ref Waveform r = ref waveform[i];

                if (!table.Query(i, cId, out r.Id))
                {
                    if (isMemory)
                    {
                        table.Query(i, cMemoryAwbId, out r.Id);
                        waveform[i].PortNo = 0xFFFF;
                    }
                    else
                    {
                        table.Query(i, cStreamAwbId, out r.Id);
                        table.Query(i, cStreamAwbPortNo, out r.PortNo);
                    }
                }
                else
                    waveform[i].PortNo = 0xFFFF;

                table.Query(i, cStreaming, out r.Streaming);
            }
        }

        void LoadAcbWaveForm(ushort index)
        {
            PreloadAcbWaveForm();

            if (index > waveFormRows)
                throw new ArgumentOutOfRangeException(nameof(index));

            ref Waveform r = ref waveform[index];

            if (r.Id != targetWaveId)
                return;

            if (targetPort >= 0 && r.PortNo != 0xFFFF && r.PortNo != targetPort)
                return;

            if ((isMemory && r.Streaming == 1) || (!isMemory && r.Streaming == 0))
                return;

            AddAcbName(r.Streaming);

            return;
        }

        void PreloadAcbSynth()
        {
            ref int rows = ref synthRows;

            if (rows != 0) return;
            if (!OpenUtfSubtable(out synthReader, out UtfTable table, "SynthTable", out rows))
                throw new Exception("Failure opening Synth table.");
            if (rows == 0) return;

            synth = new Synth[rows];

            int cType = table.GetColumn("Type");
            int cReferenceItems = table.GetColumn("ReferenceItems");

            for (int i = 0; i < rows; i++)
            {
                ref Synth r = ref synth[i];

                table.Query(i, cType, out r.Type);
                table.Query(i, cReferenceItems, out r.ReferenceItemsOffset, out r.ReferenceItemsSize);
            }
        }

        void LoadAcbSynth(ushort index)
        {
            PreloadAcbSynth();

            if (index > synthRows)
                throw new ArgumentOutOfRangeException(nameof(index));

            ref Synth r = ref synth[index];

            synthDepth++;

            if (synthDepth > 3)
                throw new Exception("Synth depth too high");

            int count = (int)(r.ReferenceItemsSize / 4);
            for (int i = 0; i < count; i++)
            {
                synthReader.BaseStream.Position = r.ReferenceItemsOffset + i * 4;

                ushort itemType = synthReader.ReadUInt16BE();
                ushort itemIndex = synthReader.ReadUInt16BE();

                switch (itemType)
                {
                    case 0:
                        count = 0;
                        break;

                    case 1:
                        LoadAcbWaveForm(itemIndex);
                        break;

                    case 2:
                        LoadAcbSynth(itemIndex);
                        break;

                    case 3:
                        LoadAcbSequence(itemIndex);
                        break;

                    case 6:
                    default:
                        count = 0;
                        break;
                }
            }

            synthDepth--;
        }

        void LoadAcbCommandTlvs(BinaryReaderEndian reader, uint commandOffset, uint commandSize)
        {
            uint pos = 0;
            uint maxPos = commandSize;

            while (pos < maxPos)
            {
                reader.BaseStream.Position = commandOffset + pos;

                ushort tlvCode = reader.ReadUInt16BE();
                ushort tlvSize = reader.ReadByte();

                pos += 3;

                switch (tlvCode)
                {
                    case 2000:
                    case 2003:
                        if (tlvSize < 4)
                            break;

                        reader.BaseStream.Position = commandOffset + pos;

                        ushort tlvType = reader.ReadUInt16BE();
                        ushort tlvIndex = reader.ReadUInt16BE();

                        switch (tlvType)
                        {
                            case 2:
                                LoadAcbSynth(tlvIndex);
                                break;

                            case 3:
                                LoadAcbSequence(tlvIndex);
                                break;

                            default:
                                maxPos = 0;
                                break;
                        }

                        break;

                    default:
                        break;
                }

                pos += tlvSize;
            }
        }

        void PreloadAcbTrackCommand()
        {
            ref int rows = ref trackCommandRows;

            if (rows != 0) return;
            if (!OpenUtfSubtable(out trackCommandReader, out UtfTable table, "TrackEventTable", out rows))
            {
                if (!OpenUtfSubtable(out trackCommandReader, out table, "CommandTable", out rows))
                    throw new Exception("Failure opening Command table.");
            }
            if (rows == 0) return;

            trackCommand = new TrackCommand[rows];

            int cCommand = table.GetColumn("Command");

            for (int i = 0; i < rows; i++)
            {
                ref TrackCommand r = ref trackCommand[i];

                table.Query(i, cCommand, out r.CommandOffset, out r.CommandSize);
            }
        }

        void LoadAcbTrackCommand(ushort index)
        {
            PreloadAcbTrackCommand();

            if (index > trackCommandRows)
                throw new ArgumentOutOfRangeException(nameof(index));

            LoadAcbCommandTlvs(
                trackCommandReader,
                trackCommand[index].CommandOffset,
                trackCommand[index].CommandSize);
        }

        void PreloadAcbTrack()
        {
            ref int rows = ref trackRows;

            if (rows != 0) return;
            if (!OpenUtfSubtable(out trackReader, out UtfTable table, "TrackTable", out rows))
                throw new Exception("Failure opening Track table.");
            if (rows == 0) return;

            track = new Track[rows];

            int cEventIndex = table.GetColumn("EventIndex");

            for (int i = 0; i < rows; i++)
            {
                ref Track r = ref track[i];

                table.Query(i, cEventIndex, out r.EventIndex);
            }
        }

        void LoadAcbTrack(ushort index)
        {
            PreloadAcbTrack();

            if (index > trackRows)
                throw new ArgumentOutOfRangeException(nameof(index));

            ref Track r = ref track[index];

            if (r.EventIndex == 65535)
                return;

            LoadAcbTrackCommand(r.EventIndex);
        }

        void PreloadAcbSequence()
        {
            ref int rows = ref sequenceRows;

            if (rows != 0) return;
            if (!OpenUtfSubtable(out sequenceReader, out UtfTable table, "SequenceTable", out rows))
                throw new Exception("Failure opening Sequence table.");
            if (rows == 0) return;

            sequence = new Sequence[rows];

            int cNumTracks = table.GetColumn("NumTracks");
            int cTrackIndex = table.GetColumn("TrackIndex");
            int cType = table.GetColumn("Type");

            for (int i = 0; i < rows; i++)
            {
                ref Sequence r = ref sequence[i];

                table.Query(i, cNumTracks, out r.NumTracks);
                table.Query(i, cTrackIndex, out r.TrackIndexOffset, out r.TrackIndexSize);
                table.Query(i, cType, out r.Type);
            }
        }

        void LoadAcbSequence(uint index)
        {
            PreloadAcbSequence();

            if (index > sequenceRows)
                throw new ArgumentOutOfRangeException(nameof(index));

            ref Sequence r = ref sequence[index];

            sequenceDepth++;

            if (sequenceDepth > 3)
                throw new Exception("Sequence depth too high.");

            if (r.NumTracks * 2 > r.TrackIndexSize)
                throw new Exception("Wrong Sequence.TrackIndex size.");

            switch (r.Type)
            {
                default:
                    for (int i = 0; i < r.NumTracks; i++)
                    {
                        sequenceReader.BaseStream.Position = r.TrackIndexOffset + i * 2;

                        short trackIndexIndex = sequenceReader.ReadInt16BE();
                        LoadAcbTrack((ushort)trackIndexIndex);
                    }
                    break;
            }

            sequenceDepth--;
        }

        void PreloadAcbBlock()
        {
            ref int rows = ref blockRows;

            if (rows != 0) return;
            if (!OpenUtfSubtable(out blockReader, out UtfTable table, "BlockTable", out rows))
                throw new Exception("Failure opening Block table.");
            if (rows == 0) return;

            block = new Block[rows];

            int cNumTracks = table.GetColumn("NumTracks");
            int cTrackIndex = table.GetColumn("TrackIndex");

            for (int i = 0; i < rows; i++)
            {
                ref Block r = ref block[i];
                table.Query(i, cNumTracks, out r.NumTracks);
                table.Query(i, cTrackIndex, out VLData data);
                r.TrackIndexOffset = data.Offset;
                r.TrackIndexSize = data.Size;
            }
        }

        void LoadAcbBlock(ushort index)
        {
            PreloadAcbBlock();

            if (index > blockRows)
                throw new ArgumentOutOfRangeException(nameof(index));

            ref Block r = ref block[index];

            if (r.NumTracks * 2 > r.TrackIndexSize)
                throw new Exception("Wrong Block.TrackIndex size.");

            for (int i = 0; i < r.NumTracks; i++)
            {
                blockReader.BaseStream.Position = r.TrackIndexOffset + i * 2;

                short trackIndexIndex = blockReader.ReadInt16BE();
                LoadAcbTrack((ushort)trackIndexIndex);
            }
        }

        void PreloadAcbBlockSequence()
        {
            ref int rows = ref blockSequenceRows;

            if (rows != 0) return;
            if (!OpenUtfSubtable(out blockSequenceReader, out UtfTable table, "BlockSequenceTable", out rows))
                throw new Exception("Failure opening BlockSequence table.");
            if (rows == 0) return;

            blockSequence = new BlockSequence[rows];

            int cNumTracks = table.GetColumn("NumTracks");
            int cTrackIndex = table.GetColumn("TrackIndex");
            int cNumBlocks = table.GetColumn("NumBlocks");
            int cBlockIndex = table.GetColumn("BlockIndex");

            for (int i = 0; i < rows; i++)
            {
                ref BlockSequence r = ref blockSequence[i];

                table.Query(i, cNumTracks, out r.NumTracks);
                table.Query(i, cTrackIndex, out r.TrackIndexOffset, out r.TrackIndexSize);
                table.Query(i, cNumBlocks, out r.NumBlocks);
                table.Query(i, cBlockIndex, out r.BlockIndexOffset, out r.BlockIndexSize);
            }
        }

        void LoadAcbBlockSequence(ushort index)
        {
            PreloadAcbBlockSequence();

            if (index > blockSequenceRows)
                throw new ArgumentOutOfRangeException(nameof(index));

            ref BlockSequence r = ref blockSequence[index];

            if (r.NumTracks * 2 > r.TrackIndexSize)
                throw new Exception("Wrong BlockSequence.TrackIndex size.");

            for (int i = 0; i < r.NumTracks; i++)
            {
                blockSequenceReader.BaseStream.Position = r.TrackIndexOffset + i * 2;

                short trackIndexIndex = blockSequenceReader.ReadInt16();
                LoadAcbTrack((ushort)trackIndexIndex);
            }

            if (r.NumBlocks * 2 > r.BlockIndexSize)
                throw new Exception("Wrong BlockSequence.BlockIndex size.");

            for (int i = 0; i < r.NumBlocks; i++)
            {
                blockSequenceReader.BaseStream.Position = r.BlockIndexOffset + i * 2;

                short blockIndexIndex = blockSequenceReader.ReadInt16();
                LoadAcbBlock((ushort)blockIndexIndex);
            }
        }

        void PreloadAcbCue()
        {
            ref int rows = ref cueRows;

            if (rows != 0) return;
            if (!OpenUtfSubtable(out cueReader, out UtfTable table, "CueTable", out rows))
                throw new Exception("Failure opening Cue table.");
            if (rows == 0) return;

            cue = new Cue[rows];

            int cReferenceType = table.GetColumn("ReferenceType");
            int cReferenceIndex = table.GetColumn("ReferenceIndex");

            for (int i = 0; i < rows; i++)
            {
                ref Cue r = ref cue[i];

                table.Query(i, cReferenceType, out r.ReferenceType);
                table.Query(i, cReferenceIndex, out r.ReferenceIndex);
            }
        }

        void LoadAcbCue(ushort index)
        {
            PreloadAcbCue();

            if (index > cueRows)
                throw new ArgumentOutOfRangeException(nameof(index));

            ref Cue r = ref cue[index];

            switch (r.ReferenceType)
            {
                case 1:
                    LoadAcbWaveForm(r.ReferenceIndex);
                    break;

                case 2:
                    LoadAcbSynth(r.ReferenceIndex);
                    break;

                case 3:
                    LoadAcbSequence(r.ReferenceIndex);
                    break;

                case 8:
                    LoadAcbBlockSequence(r.ReferenceIndex);
                    break;

                default:
                    break;
            }
        }

        void PreloadAcbCueName()
        {
            ref UtfTable table = ref cueNames;
            ref int rows = ref cueNameRows;

            if (rows != 0) return;
            if (!OpenUtfSubtable(out cueNameReader, out table, "CueNameTable", out rows))
                throw new Exception("Failure opening CueName table.");
            if (rows == 0) return;

            cueName = new CueName[rows];

            int cCueIndex = table.GetColumn("CueIndex");
            int cCueName = table.GetColumn("CueName");

            for (int i = 0; i < rows; i++)
            {
                ref CueName r = ref cueName[i];

                table.Query(i, cCueIndex, out r.CueIndex);
                table.Query(i, cCueName, out r.Name);
            }
        }

        void LoadAcbCueName(ushort index)
        {
            PreloadAcbCueName();

            if (index > cueNameRows)
                throw new ArgumentOutOfRangeException(nameof(index));

            ref CueName r = ref cueName[index];

            cueNameIndex = (short)index;
            cueNameName = r.Name;

            LoadAcbCue(r.CueIndex);
        }

        public string LoadWaveName(int waveId, int port, bool memory)
        {
            targetWaveId = waveId;
            targetPort = port;
            isMemory = memory;

            name = "";
            awbNameCount = 0;
            awbNameList = new List<short>();

            PreloadAcbCueName();
            for (ushort i = 0; i < cueNameRows; i++)
            {
                LoadAcbCueName(i);
            }

            return name;
        }
    }
}
