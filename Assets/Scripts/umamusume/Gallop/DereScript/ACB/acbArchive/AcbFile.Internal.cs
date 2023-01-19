using System;
using System.Collections.Generic;
using System.IO;
using DereTore.Common;

namespace DereTore.Exchange.Archive.ACB {
    public partial class AcbFile {

        internal override void Initialize() {
            base.Initialize();
            InitializeAcbTables();
            InitializeCueNameToWaveformTable();
            InitializeAwbArchives();
        }

        protected override void Dispose(bool disposing) {

            if(_internalAwb != null) { _internalAwb.Dispose(); }
            if(_externalAwb != null) { _externalAwb.Dispose(); }
            base.Dispose(disposing);
        }

        private AcbFile(Stream stream, long offset, long size, string acbFileName, bool disposeStream)
           : this(stream, offset, size, acbFileName, false, disposeStream) {
        }

        private AcbFile(Stream stream, long offset, long size, string acbFileName, bool includeCueIdInFileName, bool disposeStream)
            : base(stream, offset, size, acbFileName, disposeStream) {
            _includeCueIdInFileName = includeCueIdInFileName;
            Initialize();
        }

        private void InitializeAcbTables() {
            var stream = Stream;
            long refItemOffset = 0, refItemSize = 0, refCorrection = 0;

            var tables = new Dictionary<string, UtfTable>();
            _tables = tables;

            var cueTable = GetTable("CueTable");
            var waveformTable = GetTable("WaveformTable");
            var synthTable = GetTable("SynthTable");
            var cues = new AcbCueRecord[cueTable.Rows.Length];
            _cues = cues;

            for (var i = 0; i < cues.Length; ++i) {
                var cue = new AcbCueRecord();
                cue.IsWaveformIdentified = false;
                cue.CueId = cueTable.GetFieldValueAsNumber<uint>(i, "CueId").Value;
                cue.ReferenceType = cueTable.GetFieldValueAsNumber<byte>(i, "ReferenceType").Value;
                cue.ReferenceIndex = cueTable.GetFieldValueAsNumber<ushort>(i, "ReferenceIndex").Value;
                cues[i] = cue;

                switch (cue.ReferenceType) {
                    case 2:
                        refItemOffset = synthTable.GetFieldOffset(cue.ReferenceIndex, "ReferenceItems").Value;
                        refItemSize = synthTable.GetFieldSize(cue.ReferenceIndex, "ReferenceItems").Value;
                        refCorrection = refItemSize + 2;
                        break;
                    case 3:
                    case 8:
                        if (i == 0) {
                            var refItemOffsetNullable = synthTable.GetFieldOffset(0, "ReferenceItems");
                            if (refItemOffsetNullable == null) {
                                throw new AcbFieldMissingException("ReferenceItems");
                            }
                            refItemOffset = refItemOffsetNullable.Value;
                            var refItemSizeNullable = synthTable.GetFieldSize(0, "ReferenceItems");
                            if (refItemSizeNullable == null) {
                                throw new AcbFieldMissingException("ReferenceItems");
                            }
                            refItemSize = refItemSizeNullable.Value;
                            refCorrection = refItemSize - 2;
                        } else {
                            refCorrection += 4;
                        }
                        break;
                    default:
                        throw new FormatException(string.Format("Unexpected ReferenceType '{0}' for CueIndex: '{1}.'", cues[i].ReferenceType, i));
                }

                if (refItemSize != 0) {
                    cue.WaveformIndex = stream.PeekUInt16BE(refItemOffset + refCorrection);
                    var isStreamingNullable = waveformTable.GetFieldValueAsNumber<byte>(cue.WaveformIndex, "Streaming");
                    if (isStreamingNullable != null) {
                        cue.IsStreaming = isStreamingNullable.Value != 0;

                        var waveformIdNullable = waveformTable.GetFieldValueAsNumber<ushort>(cue.WaveformIndex, "Id");
                        if (waveformIdNullable != null) {
                            cue.WaveformId = waveformIdNullable.Value;
                        } else if (cue.IsStreaming) {
                            waveformIdNullable = waveformTable.GetFieldValueAsNumber<ushort>(cue.WaveformIndex, "StreamAwbId");
                            if (waveformIdNullable == null) {
                                throw new AcbFieldMissingException("StreamAwbId");
                            }
                            cue.WaveformId = waveformIdNullable.Value;
                        } else {
                            // MemoryAwbId is for ADX2LE encoder.
                            waveformIdNullable = waveformTable.GetFieldValueAsNumber<ushort>(cue.WaveformIndex, "MemoryAwbId");
                            if (waveformIdNullable == null) {
                                throw new AcbFieldMissingException("MemoryAwbId");
                            }
                            cue.WaveformId = waveformIdNullable.Value;
                        }

                        var encTypeNullable = waveformTable.GetFieldValueAsNumber<byte>(cue.WaveformIndex, "EncodeType");
                        if (encTypeNullable == null) {
                            throw new AcbFieldMissingException("EncodeType");
                        }
                        cue.EncodeType = encTypeNullable.Value;

                        cue.IsWaveformIdentified = true;
                    }
                }
            }
        }

        private void InitializeCueNameToWaveformTable() {
            var cueNameTable = GetTable("CueNameTable");
            var cues = Cues;
            var includeCueIdInFileName = _includeCueIdInFileName;
            var cueNameToWaveform = new Dictionary<string, ushort>();
            //_cueNameToWaveform = cueNameToWaveform;

            for (var i = 0; i < cueNameTable.Rows.Length; ++i) {
                var cueIndexNullable = cueNameTable.GetFieldValueAsNumber<ushort>(i, "CueIndex");
                if (cueIndexNullable == null) {
                    throw new AcbFieldMissingException("CueIndex");
                }
                var cueIndex = cueIndexNullable.Value;
                var cue = cues[cueIndex];
                if (cue.IsWaveformIdentified) {
                    var cueName = cueNameTable.GetFieldValueAsString(i, "CueName");
                    if (cueName == null) {
                        throw new AcbFieldMissingException("CueName");
                    }
                    cueName += GetExtensionForEncodeType(cue.EncodeType);
                    if (includeCueIdInFileName) {
                        cueName = cue.CueId.ToString("D5") + "_" + cueName;
                    }
                    cue.CueName = cueName;
                    cueNameToWaveform.Add(cueName, cue.WaveformId);
                }
            }
        }

        private void InitializeAwbArchives() {
            var internalAwbSize = GetFieldSize(0, "AwbFile");
            if (internalAwbSize.HasValue && internalAwbSize.Value > 0) {
                _internalAwb = GetInternalAwbArchive();
            }
            var externalAwbSize = GetFieldSize(0, "StreamAwbAfs2Header");
            if (externalAwbSize.HasValue && externalAwbSize.Value > 0) {
                _externalAwb = GetExternalAwbArchive();
            }
        }

        private Stream GetDataStreamFromCueInfo(AcbCueRecord cue, string fileNameForErrorInfo) {
            if (!cue.IsWaveformIdentified) {
                throw new InvalidOperationException(string.Format("File '{0}' is not identified.", fileNameForErrorInfo));
            }
            if (cue.IsStreaming) {
                var externalAwb = ExternalAwb;
                if (externalAwb == null) {
                    throw new InvalidOperationException(string.Format("External AWB does not exist for streaming file '{0}'.", fileNameForErrorInfo));
                }
                if (!externalAwb.Files.ContainsKey(cue.WaveformId)) {
                    throw new InvalidOperationException(string.Format("Waveform ID {0} is not found in AWB file {1}.", cue.WaveformId, externalAwb.FileName));
                }
                var targetExternalFile = externalAwb.Files[cue.WaveformId];
                using (var fs = File.Open(externalAwb.FileName, FileMode.Open, FileAccess.Read)) {
                    return AcbHelper.ExtractToNewStream(fs, targetExternalFile.FileOffsetAligned, (int)targetExternalFile.FileLength);
                }
            } else {
                var internalAwb = InternalAwb;
                if (internalAwb == null) {
                    throw new InvalidOperationException(string.Format("Internal AWB is not found for memory file '{0}' in '{1}'.", fileNameForErrorInfo, AcbFileName));
                }
                if (!internalAwb.Files.ContainsKey(cue.WaveformId)) {
                    throw new InvalidOperationException(string.Format("Waveform ID {0} is not found in internal AWB in {1}.", cue.WaveformId, AcbFileName));
                }
                var targetInternalFile = internalAwb.Files[cue.WaveformId];
                return AcbHelper.ExtractToNewStream(Stream, targetInternalFile.FileOffsetAligned, (int)targetInternalFile.FileLength);
            }
        }

        private Afs2Archive GetInternalAwbArchive() {
            var internalAwbOffset = GetFieldOffset(0, "AwbFile");
            if (internalAwbOffset == null) {
                throw new AcbFieldMissingException("AwbFile");
            }
            var internalAwb = new Afs2Archive(Stream, internalAwbOffset.Value, AcbFileName, false);
            internalAwb.Initialize();
            return internalAwb;
        }

        private Afs2Archive GetExternalAwbArchive() {
            var acbFileName = AcbFileName;
            var awbDirPath = Path.GetDirectoryName(acbFileName);
            var awbBaseFileName = Path.GetFileNameWithoutExtension(acbFileName);
            string[] awbFiles = null;
            string awbMask1 = null, awbMask2 = null, awbMask3 = null;

            if (awbFiles == null || awbFiles.Length < 1) {
                awbMask1 = string.Format(AwbFileNameFormats.Format1, awbBaseFileName);
                awbFiles = Directory.GetFiles(awbDirPath, awbMask1, SearchOption.TopDirectoryOnly);
            }
            if (awbFiles == null || awbFiles.Length < 1) {
                awbMask2 = string.Format(AwbFileNameFormats.Format2, awbBaseFileName);
                awbFiles = Directory.GetFiles(awbDirPath, awbMask2, SearchOption.TopDirectoryOnly);
            }
            if (awbFiles == null || awbFiles.Length < 1) {
                awbMask3 = string.Format(AwbFileNameFormats.Format3, awbBaseFileName);
                awbFiles = Directory.GetFiles(awbDirPath, awbMask3, SearchOption.TopDirectoryOnly);
            }
            if (awbFiles.Length < 1) {
                throw new FileNotFoundException(string.Format("Cannot find AWB file. Please verify corresponding AWB file is named '{0}', '{1}', or '{2}'.", awbMask1, awbMask2, awbMask3));
            }
            if (awbFiles.Length > 1) {
                throw new FileNotFoundException(string.Format("More than one matching AWB file for this ACB. Please verify only one AWB file is named '{0}', '{1}', or '{2}'.", awbMask1, awbMask2, awbMask3));
            }

            var externalAwbHash = GetFieldValueAsData(0, "StreamAwbHash");
            var fs = File.Open(awbFiles[0], FileMode.Open, FileAccess.Read);
            var awbHash = AcbHelper.GetMd5Checksum(fs);
            Afs2Archive archive;
            if (AcbHelper.AreDataIdentical(awbHash, externalAwbHash)) {
                archive = new Afs2Archive(fs, 0, fs.Name, true);
                archive.Initialize();
            } else {
                fs.Dispose();
                throw new FormatException(string.Format("Checksum of AWB file '{0}' ({1}) does not match MD5 checksum inside ACB file '{2}' ({3}).", fs.Name, BitConverter.ToString(awbHash), acbFileName, BitConverter.ToString(externalAwbHash)));
            }
            return archive;
        }

        private UtfTable ResolveTable(string tableName) {
            var tableOffset = GetFieldOffset(0, tableName);
            if (!tableOffset.HasValue) {
                return null;
            }
            var tableSize = GetFieldSize(0, tableName);
            if (!tableSize.HasValue) {
                return null;
            }
            var table = new UtfTable(Stream, tableOffset.Value, tableSize.Value, AcbFileName, false);
            table.Initialize();
            return table;
        }

        private static string GetExtensionForEncodeType(byte encodeType) {
            string ext;
            switch ((WaveformEncodeType)encodeType) {
                case WaveformEncodeType.Adx:
                    ext = ".adx";
                    break;
                case WaveformEncodeType.Hca:
                    ext = ".hca";
                    break;
                case WaveformEncodeType.Atrac3:
                    ext = ".at3";
                    break;
                case WaveformEncodeType.Vag:
                    ext = ".vag";
                    break;
                case WaveformEncodeType.BcWav:
                    ext = ".bcwav";
                    break;
                case WaveformEncodeType.NintendoDsp:
                    ext = ".dsp";
                    break;
                default:
                    ext = string.Format(".et-{0}.bin", encodeType.ToString("D2"));
                    break;
            }
            return ext;
        }

        private readonly bool _includeCueIdInFileName;

        private Dictionary<string, UtfTable> _tables;
        //private Dictionary<string, ushort> _cueNameToWaveform;
        private AcbCueRecord[] _cues;
        private Afs2Archive _internalAwb;
        private Afs2Archive _externalAwb;
        private string[] _fileNames;

    }
}
