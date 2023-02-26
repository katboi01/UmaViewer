using System;
using System.Collections.Generic;
using System.IO;
using DereTore.Common;

namespace DereTore.Exchange.Archive.ACB {
    public sealed class Afs2Archive : DisposableBase {

        public Afs2Archive(Stream stream, long offset, string fileName, bool disposeStream) {
            _fileName = fileName;
            _stream = stream;
            _streamOffset = offset;
            _disposeStream = disposeStream;
        }

        public void Initialize() {
            var stream = _stream;
            var offset = _streamOffset;
            var acbFileName = _fileName;
            if (!IsAfs2Archive(stream, offset)) {
                throw new FormatException("File '" + acbFileName + "' does not contain a valid AFS2 archive at offset {offset}.");
            }
            var fileCount = (int)stream.PeekUInt32LE(offset + 8);
            if (fileCount > ushort.MaxValue) {
                throw new IndexOutOfRangeException("File count " + fileCount + " exceeds maximum possible value (65535).");
            }
            var files = new Dictionary<int, Afs2FileRecord>(fileCount);
            _files = files;
            var byteAlignment = stream.PeekUInt32LE(offset + 12);
            _byteAlignment = byteAlignment;
            var version = stream.PeekUInt32LE(offset + 4);
            _version = version;
            var offsetFieldSize = (int)(version >> 8) & 0xff; // versionBytes[1], always 4?
            uint offsetMask = 0;
            for (var j = 0; j < offsetFieldSize; j++) {
                offsetMask |= (uint)(0xff << (j * 8));
            }

            const int invalidCueId = -1;
            var previousCueId = invalidCueId;
            var fileOffsetFieldBase = 0x10 + fileCount * 2;
            for (ushort i = 0; i < fileCount; ++i) {
                var currentFileOffsetBase = fileOffsetFieldBase + offsetFieldSize * i;
                var record = new Afs2FileRecord {
                    CueId = stream.PeekUInt16LE(offset + (0x10 + 2 * i)),
                    // TODO: Dynamically judge if the field is U32/U16 or else (see offsetFieldSize).
                    FileOffsetRaw = stream.PeekUInt32LE(offset + currentFileOffsetBase)
                };
                record.FileOffsetRaw &= offsetMask;
                record.FileOffsetRaw += offset;
                record.FileOffsetAligned = AcbHelper.RoundUpToAlignment(record.FileOffsetRaw, ByteAlignment);
                if (i == fileCount - 1) {
                    record.FileLength = stream.PeekUInt32LE(offset + currentFileOffsetBase + offsetFieldSize) + offset - record.FileOffsetAligned;
                }
                if (previousCueId != invalidCueId) {
                    files[previousCueId].FileLength = record.FileOffsetRaw - files[previousCueId].FileOffsetAligned;
                }
                files.Add(record.CueId, record);
                previousCueId = record.CueId;
            }
        }

        public static bool IsAfs2Archive(Stream stream, long offset) {
            var fileSignature = stream.PeekBytes(offset, 4);
            return AcbHelper.AreDataIdentical(fileSignature, Afs2Signature);
        }

        public string FileName { get { return _fileName; } }

        public uint ByteAlignment { get { return _byteAlignment; } }

        public Dictionary<int, Afs2FileRecord> Files { get { return _files; } }

        public uint Version { get { return _version; } } 

        protected override void Dispose(bool disposing) {
            if (_disposeStream) {
                try {
                    _stream.Dispose();
                } catch (ObjectDisposedException) {
                }
            }
        }

        public static readonly byte[] Afs2Signature = { 0x41, 0x46, 0x53, 0x32 }; // 'AFS2'

        private uint _byteAlignment;
        private Dictionary<int, Afs2FileRecord> _files;
        private uint _version;
        private readonly string _fileName;
        private readonly Stream _stream;
        private readonly long _streamOffset;
        private readonly bool _disposeStream;

    }
}
