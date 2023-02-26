using System;
using System.IO;
using DereTore.Common;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    internal sealed partial class UtfTableImage {

        public void WriteTo(Stream bufferStream) {
            Rows.ForEach(row => {
                row.Sort((t1, t2) => {
                    if (!ReferenceEquals(t1, t2) && t1.Order == t2.Order) {
                        throw new InvalidOperationException("Each field must have different order.");
                    }
                    return t1.Order - t2.Order;
                });
            });
            ProcessStringTable();
            ProcessDataFields();
            UtfFieldImage[] orderedDataFieldImages;
            var header = GetHeader(out orderedDataFieldImages);
            WriteHeaderTo(header, bufferStream);
            SetStringFieldRelocations(header);
            SetExtraDataFieldRelocations(header, orderedDataFieldImages);
            WriteFieldDescriptorsTo(header, bufferStream);
            WritePerRowDataTo(header, bufferStream);
            WriteStringTableTo(header, bufferStream);
            WriteExtraDataTo(header, bufferStream);
        }

        private static void WriteHeaderTo(UtfHeader header, Stream bufferStream) {
            bufferStream.SeekAndWriteBytes(UtfTable.UtfSignature, 0);
            bufferStream.SeekAndWriteUInt32BE(header.TableSize - 8, 4);
            bufferStream.SeekAndWriteUInt16BE(1, 8);
            bufferStream.SeekAndWriteUInt16BE((ushort)(header.PerRowDataOffset - 8), 10);
            bufferStream.SeekAndWriteUInt32BE(header.StringTableOffset - 8, 12);
            bufferStream.SeekAndWriteUInt32BE(header.ExtraDataOffset - 8, 16);
            bufferStream.SeekAndWriteUInt32BE(header.TableNameOffset, 20);
            bufferStream.SeekAndWriteUInt16BE(header.FieldCount, 24);
            bufferStream.SeekAndWriteUInt16BE(header.RowSize, 26);
            bufferStream.SeekAndWriteUInt32BE(header.RowCount, 28);
        }

        private void WriteFieldDescriptorsTo(UtfHeader header, Stream bufferStream) {
            bufferStream.Seek(SchemaOffset, SeekOrigin.Begin);
            var row = Rows[0];
            foreach (var fieldImage in row) {
                bufferStream.WriteByte((byte)((byte)fieldImage.Storage | (byte)fieldImage.Type));
                bufferStream.WriteUInt32BE(fieldImage.NameOffset);
                if (fieldImage.Storage == ColumnStorage.Constant || fieldImage.Storage == ColumnStorage.Constant2) {
                    fieldImage.WriteValueTo(bufferStream);
                }
            }
        }

        private void WritePerRowDataTo(UtfHeader header, Stream bufferStream) {
            bufferStream.Seek(header.PerRowDataOffset, SeekOrigin.Begin);
            foreach (var row in Rows) {
                foreach (var fieldImage in row) {
                    if (fieldImage.Storage == ColumnStorage.PerRow) {
                        fieldImage.WriteValueTo(bufferStream);
                    }
                }
            }
        }

        private void WriteStringTableTo(UtfHeader header, Stream bufferStream) {
            bufferStream.Seek(header.StringTableOffset, SeekOrigin.Begin);
            bufferStream.WriteBytes(TableNameBytesCache);
            var row0 = Rows[0];
            foreach (var fieldImage in row0) {
                bufferStream.WriteBytes(fieldImage.NameBytesCache);
            }
            foreach (var fieldImage in row0) {
                if ((fieldImage.Storage == ColumnStorage.Constant || fieldImage.Storage == ColumnStorage.Constant2) && fieldImage.Type == ColumnType.String) {
                    bufferStream.WriteBytes(fieldImage.StringValueBytesCache);
                }
            }
            foreach (var row in Rows) {
                foreach (var fieldImage in row) {
                    if (fieldImage.Type == ColumnType.String && fieldImage.Storage == ColumnStorage.PerRow) {
                        bufferStream.WriteBytes(fieldImage.StringValueBytesCache);
                    }
                }
            }
        }

        private void WriteExtraDataTo(UtfHeader header, Stream bufferStream) {
            var baseOffset = header.ExtraDataOffset;
            for (var i = 0; i < Rows.Count; ++i) {
                var row = Rows[i];
                foreach (var fieldImage in row) {
                    if (fieldImage.Type == ColumnType.Data) {
                        var shouldWrite = false;
                        switch (fieldImage.Storage) {
                            case ColumnStorage.Constant:
                            case ColumnStorage.Constant2:
                                shouldWrite = i == 0;
                                break;
                            case ColumnStorage.PerRow:
                                shouldWrite = true;
                                break;
                            default:
                                break;
                        }
                        if (shouldWrite && fieldImage.DataValue.Length > 0) {
                            bufferStream.SeekAndWriteBytes(fieldImage.DataValue, baseOffset + fieldImage.DataOffset);
                        }
                    }
                }
            }
        }

    }
}
