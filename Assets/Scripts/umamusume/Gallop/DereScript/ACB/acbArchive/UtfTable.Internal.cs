using System;
using System.Collections.Generic;
using System.IO;
using DereTore.Common;

namespace DereTore.Exchange.Archive.ACB {
    public partial class UtfTable {

        internal UtfTable(Stream stream, long offset, long size, string acbFileName, bool disposeStream) {
            _acbFileName = acbFileName;
            _stream = stream;
            _offset = offset;
            _size = size;
            _disposeStream = disposeStream;
        }

        internal virtual void Initialize() {
            var stream = _stream;
            var offset = _offset;

            var magic = stream.PeekBytes(offset, 4);
            magic = CheckEncryption(magic);
            if (!AcbHelper.AreDataIdentical(magic, UtfSignature)) {
                throw new FormatException(string.Format("'@UTF' signature (or its encrypted equivalent) is not found in '{0}' at offset 0x{1:x8}.",_acbFileName,offset));
            }
            using (var tableDataStream = GetTableDataStream()) {
                var header = GetUtfHeader(tableDataStream);
                _utfHeader = header;
                _rows = new Dictionary<string, UtfField>[header.RowCount];
                if (header.TableSize > 0) {
                    InitializeUtfSchema(stream, tableDataStream, 0x20);
                }
            }
        }

        protected override void Dispose(bool disposing) {
            if (_disposeStream) {
                try {
                    _stream.Dispose();
                } catch (ObjectDisposedException) {
                }
            }
        }

        private static bool GetKeysForEncryptedUtfTable(byte[] encryptedUtfSignature, out byte seed, out byte increment) {
            for (var s = 0; s <= byte.MaxValue; s++) {
                if ((encryptedUtfSignature[0] ^ s) != UtfSignature[0]) {
                    continue;
                }
                for (var i = 0; i <= byte.MaxValue; i++) {
                    var m = (byte)(s * i);
                    if ((encryptedUtfSignature[1] ^ m) != UtfSignature[1]) {
                        continue;
                    }
                    var t = (byte)i;
                    for (var j = 2; j < UtfSignature.Length; j++) {
                        m *= t;
                        if ((encryptedUtfSignature[j] ^ m) != UtfSignature[j]) {
                            break;
                        }
                        if (j < UtfSignature.Length - 1) {
                            continue;
                        }
                        seed = (byte)s;
                        increment = (byte)i;
                        return true;
                    }
                }
            }
            seed = 0;
            increment = 0;
            return false;
        }

        private byte[] CheckEncryption(byte[] magicBytes) {
            if (AcbHelper.AreDataIdentical(magicBytes, UtfSignature)) {
                _isEncrypted = false;
                _utfReader = new UtfReader();
                return magicBytes;
            } else {
                _isEncrypted = true;
                byte seed, increment;
                var keysFound = GetKeysForEncryptedUtfTable(magicBytes, out seed, out increment);
                if (!keysFound) {
                    throw new FormatException(string.Format("Unable to decrypt UTF table at offset 0x{0:x8}",_offset));
                } else {
                    _utfReader = new UtfReader(seed, increment);
                }
                return UtfSignature;
            }
        }

        private Stream GetTableDataStream() {
            var stream = _stream;
            var offset = _offset;
            var tableSize = (int)_utfReader.PeekUInt32(stream, offset, 4) + 8;
            if (!IsEncrypted) {
                return AcbHelper.ExtractToNewStream(stream, offset, tableSize);
            }
            // Another reading process. Unlike the one with direct reading, this may encounter UTF table decryption.
            var originalPosition = stream.Position;
            var totalBytesRead = 0;
            var memory = new byte[tableSize];
            var currentIndex = 0;
            var currentOffset = offset;
            do {
                var shouldRead = tableSize - totalBytesRead;
                var buffer = _utfReader.PeekBytes(stream, currentOffset, shouldRead, totalBytesRead);
                Array.Copy(buffer, 0, memory, currentIndex, buffer.Length);
                currentOffset += buffer.Length;
                currentIndex += buffer.Length;
                totalBytesRead += buffer.Length;
            } while (totalBytesRead < tableSize);
            stream.Position = originalPosition;
            var memoryStream = new MemoryStream(memory, false) {
                Capacity = tableSize
            };
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        private static UtfHeader GetUtfHeader(Stream stream) {
            return GetUtfHeader(stream, stream.Position);
        }

        private static UtfHeader GetUtfHeader(Stream stream, long offset) {
            if (offset != stream.Position) {
                stream.Seek(offset, SeekOrigin.Begin);
            }
            var header = new UtfHeader {
                TableSize = stream.PeekUInt32BE(offset + 4),
                Unknown1 = stream.PeekUInt16BE(offset + 8),
                PerRowDataOffset = (uint)stream.PeekUInt16BE(offset + 10) + 8,
                StringTableOffset = stream.PeekUInt32BE(offset + 12) + 8,
                ExtraDataOffset = stream.PeekUInt32BE(offset + 16) + 8,
                TableNameOffset = stream.PeekUInt32BE(offset + 20),
                FieldCount = stream.PeekUInt16BE(offset + 24),
                RowSize = stream.PeekUInt16BE(offset + 26),
                RowCount = stream.PeekUInt32BE(offset + 28)
            };
            header.TableName = stream.PeekZeroEndedStringAsAscii(header.StringTableOffset + header.TableNameOffset);
            return header;
        }

        private void InitializeUtfSchema(Stream sourceStream, Stream tableDataStream, long schemaOffset) {
            var header = _utfHeader;
            var rows = _rows;
            var baseOffset = _offset;
            for (uint i = 0; i < header.RowCount; i++) {
                var currentOffset = schemaOffset;
                var row = new Dictionary<string, UtfField>();
                rows[i] = row;
                long currentRowOffset = 0;
                long currentRowBase = header.PerRowDataOffset + header.RowSize * i;

                for (var j = 0; j < header.FieldCount; j++) {
                    var field = new UtfField {
                        Type = tableDataStream.PeekByte(currentOffset)
                    };

                    long nameOffset = tableDataStream.PeekInt32BE(currentOffset + 1);
                    field.Name = tableDataStream.PeekZeroEndedStringAsAscii(header.StringTableOffset + nameOffset);

                    var union = new NumericUnion();
                    var constrainedStorage = (ColumnStorage)(field.Type & (byte)ColumnStorage.Mask);
                    var constrainedType = (ColumnType)(field.Type & (byte)ColumnType.Mask);
                    switch (constrainedStorage) {
                        case ColumnStorage.Constant:
                        case ColumnStorage.Constant2:
                            var constantOffset = currentOffset + 5;
                            long dataOffset;
                            switch (constrainedType) {
                                case ColumnType.String:
                                    dataOffset = tableDataStream.PeekInt32BE(constantOffset);
                                    field.StringValue = tableDataStream.PeekZeroEndedStringAsAscii(header.StringTableOffset + dataOffset);
                                    currentOffset += 4;
                                    break;
                                case ColumnType.Int64:
                                    union.S64 = tableDataStream.PeekInt64BE(constantOffset);
                                    currentOffset += 8;
                                    break;
                                case ColumnType.UInt64:
                                    union.U64 = tableDataStream.PeekUInt64BE(constantOffset);
                                    currentOffset += 8;
                                    break;
                                case ColumnType.Data:
                                    dataOffset = tableDataStream.PeekUInt32BE(constantOffset);
                                    long dataSize = tableDataStream.PeekUInt32BE(constantOffset + 4);
                                    field.Offset = baseOffset + header.ExtraDataOffset + dataOffset;
                                    field.Size = dataSize;
                                    // don't think this is encrypted, need to check
                                    field.DataValue = sourceStream.PeekBytes(field.Offset, (int)dataSize);
                                    currentOffset += 8;
                                    break;
                                case ColumnType.Double:
                                    union.R64 = tableDataStream.PeekDoubleBE(constantOffset);
                                    currentOffset += 8;
                                    break;
                                case ColumnType.Single:
                                    union.R32 = tableDataStream.PeekSingleBE(constantOffset);
                                    currentOffset += 4;
                                    break;
                                case ColumnType.Int32:
                                    union.S32 = tableDataStream.PeekInt32BE(constantOffset);
                                    currentOffset += 4;
                                    break;
                                case ColumnType.UInt32:
                                    union.U32 = tableDataStream.PeekUInt32BE(constantOffset);
                                    currentOffset += 4;
                                    break;
                                case ColumnType.Int16:
                                    union.S16 = tableDataStream.PeekInt16BE(constantOffset);
                                    currentOffset += 2;
                                    break;
                                case ColumnType.UInt16:
                                    union.U16 = tableDataStream.PeekUInt16BE(constantOffset);
                                    currentOffset += 2;
                                    break;
                                case ColumnType.SByte:
                                    union.S8 = tableDataStream.PeekSByte(constantOffset);
                                    currentOffset += 1;
                                    break;
                                case ColumnType.Byte:
                                    union.U8 = tableDataStream.PeekByte(constantOffset);
                                    currentOffset += 1;
                                    break;
                                default:
                                    throw new FormatException(string.Format("Unknown column type at offset: 0x{0:x8}",currentOffset));
                            }
                            break;
                        case ColumnStorage.PerRow:
                            // read the constant depending on the type
                            long rowDataOffset;
                            switch (constrainedType) {
                                case ColumnType.String:
                                    rowDataOffset = tableDataStream.PeekUInt32BE(currentRowBase + currentRowOffset);
                                    field.StringValue = tableDataStream.PeekZeroEndedStringAsAscii(header.StringTableOffset + rowDataOffset);
                                    currentRowOffset += 4;
                                    break;
                                case ColumnType.Int64:
                                    union.S64 = tableDataStream.PeekInt64BE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 8;
                                    break;
                                case ColumnType.UInt64:
                                    union.U64 = tableDataStream.PeekUInt64BE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 8;
                                    break;
                                case ColumnType.Data:
                                    rowDataOffset = tableDataStream.PeekUInt32BE(currentRowBase + currentRowOffset);
                                    long rowDataSize = tableDataStream.PeekUInt32BE(currentRowBase + currentRowOffset + 4);
                                    field.Offset = baseOffset + header.ExtraDataOffset + rowDataOffset;
                                    field.Size = rowDataSize;
                                    // don't think this is encrypted
                                    field.DataValue = sourceStream.PeekBytes(field.Offset, (int)rowDataSize);
                                    currentRowOffset += 8;
                                    break;
                                case ColumnType.Double:
                                    union.R64 = tableDataStream.PeekDoubleBE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 8;
                                    break;
                                case ColumnType.Single:
                                    union.R32 = tableDataStream.PeekSingleBE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 4;
                                    break;
                                case ColumnType.Int32:
                                    union.S32 = tableDataStream.PeekInt32BE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 4;
                                    break;
                                case ColumnType.UInt32:
                                    union.U32 = tableDataStream.PeekUInt32BE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 4;
                                    break;
                                case ColumnType.Int16:
                                    union.S16 = tableDataStream.PeekInt16BE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 2;
                                    break;
                                case ColumnType.UInt16:
                                    union.U16 = tableDataStream.PeekUInt16BE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 2;
                                    break;
                                case ColumnType.SByte:
                                    union.S8 = tableDataStream.PeekSByte(currentRowBase + currentRowOffset);
                                    currentRowOffset += 1;
                                    break;
                                case ColumnType.Byte:
                                    union.U8 = tableDataStream.PeekByte(currentRowBase + currentRowOffset);
                                    currentRowOffset += 1;
                                    break;
                                default:
                                    throw new FormatException(string.Format("Unknown column type at offset: 0x{0:x8}",currentOffset));
                            }
                            field.ConstrainedType = (ColumnType)field.Type;
                            break;
                        default:
                            throw new FormatException(string.Format("Unknown column storage at offset: 0x{0:x8}",currentOffset));
                    }
                    // Union polyfill
                    field.ConstrainedType = constrainedType;
                    switch (constrainedType) {
                        case ColumnType.String:
                        case ColumnType.Data:
                            break;
                        default:
                            field.NumericValue = union;
                            break;
                    }
                    row.Add(field.Name, field);
                    currentOffset += 5; //  sizeof(CriField.Type + CriField.NameOffset)
                }
            }
        }

        private object GetFieldValue(int rowIndex, string fieldName) {
            var rows = Rows;
            if (rowIndex >= rows.Length) {
                return null;
            }
            var row = rows[rowIndex];
            return row.ContainsKey(fieldName) ? row[fieldName].GetValue() : null;
        }

        internal T? GetFieldValueAsNumber<T>(int rowIndex, string fieldName) where T : struct {
            return (T?)GetFieldValue(rowIndex, fieldName);
        }

        internal string GetFieldValueAsString(int rowIndex, string fieldName) {
            return (string)GetFieldValue(rowIndex, fieldName);
        }

        internal byte[] GetFieldValueAsData(int rowIndex, string fieldName) {
            return (byte[])GetFieldValue(rowIndex, fieldName);
        }

        internal long? GetFieldOffset(int rowIndex, string fieldName) {
            var rows = Rows;
            if (rowIndex >= rows.Length) {
                return null;
            }
            var row = rows[rowIndex];
            if (row.ContainsKey(fieldName)) {
                return row[fieldName].Offset;
            }
            return null;
        }

        internal long? GetFieldSize(int rowIndex, string fieldName) {
            var rows = Rows;
            if (rowIndex >= rows.Length) {
                return null;
            }
            var row = rows[rowIndex];
            if (row.ContainsKey(fieldName)) {
                return row[fieldName].Size;
            }
            return null;
        }

        internal static readonly byte[] UtfSignature = { 0x40, 0x55, 0x54, 0x46 }; // '@UTF'

        private readonly string _acbFileName;
        private readonly Stream _stream;
        private readonly long _offset;
        private readonly long _size;
        private readonly bool _disposeStream;

        private UtfReader _utfReader;
        private bool _isEncrypted;
        private UtfHeader _utfHeader;
        private Dictionary<string, UtfField>[] _rows;

    }
}
