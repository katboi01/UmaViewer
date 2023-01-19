using System;
using System.Text;
using DereTore.Common;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    internal sealed partial class UtfTableImage {

        /// <summary>
        /// Some fields are not filled.
        /// </summary>
        /// <returns></returns>
        public UtfHeader GetHeader(out UtfFieldImage[] orderedDataFieldImages) {
            // Basic information
            if (Rows.Count < 1) {
                throw new InvalidOperationException("Rows should not be empty.");
            }
            var fieldCount = Rows[0].Count;
            for (var i = 1; i < Rows.Count; ++i) {
                if (Rows[i].Count != fieldCount) {
                    throw new InvalidOperationException("Number of fields in each row do not match.");
                }
            }

            var header = new UtfHeader {
                TableName = TableName,
                RowCount = (uint)Rows.Count,
                FieldCount = (ushort)fieldCount,
                Unknown1 = 1,
                RowSize = GetSingleRowDataSize(),
                PerRowDataOffset = GetRowDescriptorSize() + SchemaOffset // "per row" data offset, actually
            };
            var perRowDataSize = header.RowCount * header.RowSize;
            header.StringTableOffset = header.PerRowDataOffset + perRowDataSize;
            header.TableNameOffset = 0;

            var stringTableSize = GetStringTableSize();
            header.ExtraDataOffset = header.StringTableOffset + stringTableSize;
            var extraDataSize = GetExtraDataSize(header, out orderedDataFieldImages);

            header.TableSize = header.ExtraDataOffset + extraDataSize;

            return header;
        }

        private void ProcessStringTable() {
            TableNameBytesCache = Encoding.ASCII.GetBytes(TableName).Append(0);
            // Prepare static strings first.
            var row = Rows[0];
            foreach (var fieldImage in row) {
                fieldImage.NameBytesCache = Encoding.ASCII.GetBytes(fieldImage.Name).Append(0);
                switch (fieldImage.Storage) {
                    case ColumnStorage.Constant:
                    case ColumnStorage.Constant2:
                        if (fieldImage.Type == ColumnType.String) {
                            if (string.IsNullOrEmpty(fieldImage.StringValue)) {
                                fieldImage.StringValueBytesCache = EmptyAsciiStringBytes;
                            } else {
                                fieldImage.StringValueBytesCache = Encoding.ASCII.GetBytes(fieldImage.StringValue).Append(0);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            var row0 = row;
            for (var i = 1; i < Rows.Count; ++i) {
                for (var j = 0; j < Rows[i].Count; ++j) {
                    var fieldImage = Rows[i][j];
                    fieldImage.Name = row0[j].Name;
                    fieldImage.NameBytesCache = row0[j].NameBytesCache;
                    switch (fieldImage.Storage) {
                        case ColumnStorage.Constant:
                        case ColumnStorage.Constant2:
                            if (fieldImage.Type == ColumnType.String) {
                                fieldImage.StringValue = row0[j].StringValue;
                                fieldImage.StringValueBytesCache = row0[j].StringValueBytesCache;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            // Per-row strings.
            foreach (var r in Rows) {
                foreach (var fieldImage in r) {
                    if (fieldImage.Storage == ColumnStorage.PerRow && fieldImage.Type == ColumnType.String) {
                        if (string.IsNullOrEmpty(fieldImage.StringValue)) {
                            fieldImage.StringValueBytesCache = EmptyAsciiStringBytes;
                        } else {
                            fieldImage.StringValueBytesCache = Encoding.ASCII.GetBytes(fieldImage.StringValue).Append(0);
                        }
                    }
                }
            }
        }

        private void ProcessDataFields() {
            foreach (var row in Rows) {
                foreach (var fieldImage in row) {
                    if (fieldImage.Type == ColumnType.Data && fieldImage.DataValue == null) {
                        fieldImage.DataValue = EmptyByteArray;
                    }
                }
            }
        }

        private void SetStringFieldRelocations(UtfHeader header) {
            // String table
            var currentStringTableOffset = (uint)TableNameBytesCache.Length;
            var row0 = Rows[0];
            var row = row0;
            // Field names and static strings
            foreach (var fieldImage in row) {
                fieldImage.NameOffset = currentStringTableOffset;
                currentStringTableOffset += (uint)fieldImage.NameBytesCache.Length;
            }
            foreach (var fieldImage in row) {
                switch (fieldImage.Storage) {
                    case ColumnStorage.Constant:
                    case ColumnStorage.Constant2:
                        if (fieldImage.Type == ColumnType.String) {
                            fieldImage.StringOffset = currentStringTableOffset;
                            currentStringTableOffset += (uint)fieldImage.StringValueBytesCache.Length;
                        }
                        break;
                    default:
                        break;
                }
            }
            for (var i = 1; i < Rows.Count; ++i) {
                for (var j = 0; j < Rows[i].Count; ++j) {
                    var fieldImage = Rows[i][j];
                    fieldImage.NameOffset = row0[j].NameOffset;
                    switch (fieldImage.Storage) {
                        case ColumnStorage.Constant:
                        case ColumnStorage.Constant2:
                            if (fieldImage.Type == ColumnType.String) {
                                fieldImage.StringOffset = row0[j].StringOffset;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            // Per-row strings
            foreach (var r in Rows) {
                foreach (var fieldImage in r) {
                    if (fieldImage.Storage == ColumnStorage.PerRow && fieldImage.Type == ColumnType.String) {
                        fieldImage.StringOffset = currentStringTableOffset;
                        currentStringTableOffset += (uint)fieldImage.StringValueBytesCache.Length;
                    }
                }
            }
        }

        private void SetExtraDataFieldRelocations(UtfHeader header, UtfFieldImage[] orderedFieldImages) {
            // This value is already calibrated in GetExtraDataSize(). It will move to a mod16 position if the first item is a table.
            var baseOffset = header.ExtraDataOffset;
            var currentOffset = baseOffset;

            foreach (var fieldImage in orderedFieldImages) {
                if (fieldImage.DataValue.Length > 0) {
                    // For priority, see AddToNewOrderedDataFieldList().
                    if (fieldImage.IsTable) {
                        currentOffset = SerializationHelper.RoundUpAsTable(currentOffset, Alignment);
                    }
                    fieldImage.DataOffset = currentOffset - baseOffset;
                    currentOffset += (uint)fieldImage.DataValue.Length;
                } else {
                    fieldImage.DataOffset = 0;
                }
            }
            var row0 = Rows[0];
            for (var i = 1; i < Rows.Count; ++i) {
                for (var j = 0; j < Rows[i].Count; ++j) {
                    var fieldImage = Rows[i][j];
                    if (fieldImage.Type == ColumnType.Data && (fieldImage.Storage == ColumnStorage.Constant || fieldImage.Storage == ColumnStorage.Constant2)) {
                        fieldImage.DataOffset = row0[j].DataOffset;
                    }
                }
            }
        }

        private static readonly byte[] EmptyByteArray = new byte[0];
        private static readonly byte[] EmptyAsciiStringBytes = new byte[1];
        private static readonly ushort SchemaOffset = 0x20;

    }
}
