using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    internal sealed partial class UtfTableImage {

        public static string NameOf<T>(Expression<Func<T>> e)
        {
            return ((MemberExpression)e.Body).Member.Name;
        }
        private uint GetRowDescriptorSize() {
            uint totalSize = 0;
            foreach (var row in Rows) {
                foreach (var fieldImage in row) {
                    // Storage descriptor (1 byte) + field name offset (4 bytes)
                    totalSize += 5;
                    var storage = fieldImage.Storage;
                    switch (storage) {
                        case ColumnStorage.Constant:
                        case ColumnStorage.Constant2:
                            var type = fieldImage.Type;
                            switch (type) {
                                case ColumnType.Byte:
                                case ColumnType.SByte:
                                    totalSize += 1;
                                    break;
                                case ColumnType.UInt16:
                                case ColumnType.Int16:
                                    totalSize += 2;
                                    break;
                                case ColumnType.UInt32:
                                case ColumnType.Int32:
                                    totalSize += 4;
                                    break;
                                case ColumnType.UInt64:
                                case ColumnType.Int64:
                                    totalSize += 8;
                                    break;
                                case ColumnType.Single:
                                    totalSize += 4;
                                    break;
                                case ColumnType.Double:
                                    totalSize += 8;
                                    break;
                                case ColumnType.String:
                                    totalSize += 4;
                                    break;
                                case ColumnType.Data:
                                    totalSize += 4 + 4;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(NameOf(() => type));
                            }
                            break;
                        case ColumnStorage.PerRow:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(NameOf(() => storage));
                    }
                }
            }
            return totalSize;
        }

        /// <summary>
        /// Total size of "per row" data is [single row data size] x [row count].
        /// </summary>
        /// <returns></returns>
        private ushort GetSingleRowDataSize() {
            ushort totalSize = 0;
            var row = Rows[0];
            foreach (var fieldImage in row) {
                var storage = fieldImage.Storage;
                switch (storage) {
                    case ColumnStorage.PerRow:
                        var type = fieldImage.Type;
                        switch (type) {
                            case ColumnType.Byte:
                            case ColumnType.SByte:
                                totalSize += 1;
                                break;
                            case ColumnType.UInt16:
                            case ColumnType.Int16:
                                totalSize += 2;
                                break;
                            case ColumnType.UInt32:
                            case ColumnType.Int32:
                                totalSize += 4;
                                break;
                            case ColumnType.UInt64:
                            case ColumnType.Int64:
                                totalSize += 8;
                                break;
                            case ColumnType.Single:
                                totalSize += 4;
                                break;
                            case ColumnType.Double:
                                totalSize += 8;
                                break;
                            case ColumnType.String:
                                totalSize += 4;
                                break;
                            case ColumnType.Data:
                                totalSize += 4 + 4;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(NameOf(() => type));
                        }
                        break;
                    case ColumnStorage.Constant:
                    case ColumnStorage.Constant2:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(NameOf(() => storage));
                }
            }
            return totalSize;
        }

        private uint GetStringTableSize() {
            uint totalSize = 0;
            totalSize += (uint)TableNameBytesCache.Length;
            totalSize += Rows[0].Aggregate<UtfFieldImage, uint>(0, (current, fieldImage) => current + (uint)fieldImage.NameBytesCache.Length);
            for (var i = 0; i < Rows.Count; ++i) {
                var row = Rows[i];
                foreach (var fieldImage in row) {
                    switch (fieldImage.Type) {
                        case ColumnType.String:
                            switch (fieldImage.Storage) {
                                case ColumnStorage.Constant:
                                case ColumnStorage.Constant2:
                                    if (i == 0) {
                                        totalSize += (uint)fieldImage.StringValueBytesCache.Length;
                                    }
                                    break;
                                case ColumnStorage.PerRow:
                                    totalSize += (uint)fieldImage.StringValueBytesCache.Length;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return totalSize;
        }

        private uint GetExtraDataSize(UtfHeader header, out UtfFieldImage[] orderedDataFieldImages) {
            var baseOffset = header.ExtraDataOffset;
            uint totalSize = 0;

            var fieldImagesInNewOrder = new List<UtfFieldImage>();
            var row0 = Rows[0];
            var firstTableDataFieldIndex = -1;
            foreach (var fieldImage in row0) {
                if (fieldImage.Type == ColumnType.Data && (fieldImage.Storage == ColumnStorage.Constant || fieldImage.Storage == ColumnStorage.Constant2)) {
                    AddToNewOrdererdDataFieldList(fieldImagesInNewOrder, fieldImage, ref firstTableDataFieldIndex);
                }
            }
            foreach (var row in Rows) {
                foreach (var fieldImage in row) {
                    if (fieldImage.Type == ColumnType.Data && fieldImage.Storage == ColumnStorage.PerRow) {
                        AddToNewOrdererdDataFieldList(fieldImagesInNewOrder, fieldImage, ref firstTableDataFieldIndex);
                    }
                }
            }

            // If the first field is a table, rebase the whole data field.
            if (firstTableDataFieldIndex == 0) {
                baseOffset = SerializationHelper.RoundUpAsTable(baseOffset, Alignment);
                header.ExtraDataOffset = baseOffset;
            }
            orderedDataFieldImages = fieldImagesInNewOrder.ToArray();

            foreach (var fieldImage in orderedDataFieldImages) {
                var rawOffset = baseOffset;
                if (fieldImage.IsTable && fieldImage.DataValue.Length > 0) {
                    baseOffset = SerializationHelper.RoundUpAsTable(baseOffset, Alignment);
                }
                baseOffset += (uint)fieldImage.DataValue.Length;
                totalSize += (baseOffset - rawOffset);
            }
            return totalSize;
        }

        private static void AddToNewOrdererdDataFieldList(List<UtfFieldImage> fieldImagesInNewOrder, UtfFieldImage fieldImage, ref int firstTableDataFieldIndex) {
            // Tables are always the last fields to store.
            // Priority: [static misc data] > [dynamic static data] > [static table] > [dynamic table]
            if (fieldImage.IsTable) {
                fieldImagesInNewOrder.Add(fieldImage);
                if (firstTableDataFieldIndex < 0) {
                    firstTableDataFieldIndex = fieldImagesInNewOrder.Count - 1;
                }
            } else {
                if (firstTableDataFieldIndex < 0) {
                    fieldImagesInNewOrder.Add(fieldImage);
                } else {
                    fieldImagesInNewOrder.Insert(firstTableDataFieldIndex, fieldImage);
                    ++firstTableDataFieldIndex;
                }
            }
        }

    }
}
