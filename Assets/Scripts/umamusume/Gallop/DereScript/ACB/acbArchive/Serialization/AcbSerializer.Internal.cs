using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    public partial class AcbSerializer {
        
        private byte[] GetTableBytes<T>(T[] tableRows) where T : UtfRowBase {
            var tableImage = PrepareTable(tableRows);
            byte[] buffer;
            using (var memory = new MemoryStream()) {
                tableImage.WriteTo(memory);
                //memory.Capacity = (int)AcbHelper.RoundUpToAlignment((int)memory.Length, Alignment);
                buffer = new byte[memory.Length];
                //memory.GetBuffer().CopyTo(buffer, 0);
                Array.Copy(memory.GetBuffer(), buffer, buffer.Length);
            }
            return buffer;
        }

        private UtfTableImage PrepareTable<T>(T[] tableRows) where T : UtfRowBase {
            var tableName = GetTableName(tableRows);
            var tableImage = new UtfTableImage(tableName, Alignment);
            foreach (var tableRow in tableRows) {
                var targetMembers = SerializationHelper.GetSearchTargetFieldsAndProperties(tableRow);
                var tableImageRow = new List<UtfFieldImage>();
                tableImage.Rows.Add(tableImageRow);
                // TODO: Save misc data first, then the tables.
                foreach (var member in targetMembers) {
                    var fieldInfo = member.FieldInfo;
                    var fieldType = fieldInfo.FieldType;
                    var fieldAttribute = member.FieldAttribute;
                    CheckFieldType(fieldType);
                    var fieldValue = member.FieldInfo.GetValue(tableRow);
                    var fieldImage = new UtfFieldImage {
                        Order = fieldAttribute.Order,
                        Name = string.IsNullOrEmpty(fieldAttribute.FieldName) ? fieldInfo.Name : fieldAttribute.FieldName,
                        Storage = fieldAttribute.Storage,
                    };
                    // Empty tables are treated as empty data.
                    if (IsTypeRowList(fieldType) && fieldValue != null && ((UtfRowBase[])fieldValue).Length > 0) {
                        var tableBytes = GetTableBytes((UtfRowBase[])fieldValue);
                        fieldImage.SetValue(tableBytes);
                        fieldImage.IsTable = true;
                    } else if (fieldType == typeof(byte[]) && member.ArchiveAttribute != null) {
                        var files = new List<byte[]> {
                            (byte[])fieldValue
                        };
                        var archiveBytes = SerializationHelper.GetAfs2ArchiveBytes(files.AsReadOnly(), Alignment);
                        fieldImage.SetValue(archiveBytes);
                        fieldImage.IsTable = true;
                    } else {
                        if (fieldValue == null) {
                            fieldImage.SetNullValue(MapFromRawType(fieldType));
                        } else {
                            fieldImage.SetValue(fieldValue);
                        }
                    }
                    tableImageRow.Add(fieldImage);
                }
            }
            return tableImage;
        }

        private static void CheckFieldType(Type type) {
            if (!SupportedTypes.Contains(type) && !IsTypeRowList(type)) {
                throw new InvalidCastException("Unsupported type: '" + type.FullName + "'");
            }
        }

        private static bool IsTypeRowList(Type type) {
            return type.IsArray && type.HasElementType && type.GetElementType().IsSubclassOf(typeof(UtfRowBase));
        }

        private static ColumnType MapFromRawType(Type rawType) {
            if (IsTypeRowList(rawType)) {
                return ColumnType.Data;
            }
            var index = SupportedTypes.IndexOf(rawType);
            return (ColumnType)index;
        }

        private static string GetTableName<T>(T[] tableRows) where T : UtfRowBase {
            if (tableRows == null) {
                throw new ArgumentNullException(NameOf(() => tableRows));
            }
            if (tableRows.Length < 1) {
                throw new ArgumentException("There should be at least one row in a table.", NameOf(()  => tableRows));
            }
            // Assuming all the rows are of the same type.
            var tableType = tableRows[0].GetType();
            var tableAttributes = tableType.GetCustomAttributes(typeof(UtfTableAttribute), false);
            if (tableAttributes.Length == 1) {
                return ((UtfTableAttribute)tableAttributes[0]).Name;
            } else {
                var s = tableType.Name;
                if (s.EndsWith("Table")) {
                    s = s.Substring(0, s.Length - 5);
                }
                return s;
            }
        }

        private static readonly Type[] SupportedTypes = {
            typeof(byte), typeof(sbyte), typeof(ushort), typeof(short), typeof(uint), typeof(int), typeof(ulong), typeof(long),
            typeof(float),typeof(double),
            typeof(string),
            typeof(byte[])
        };
    }
}
