using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using DereTore.Common;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    internal static class SerializationHelper {

        public static uint RoundUpAsTable(uint value, uint alignment) {
            // This action seems weird. But it does exist (see Cue table in CGSS song_1001[oneshin]), I don't know why.
            value = AcbHelper.RoundUpToAlignment(value, 4);
            if (value % alignment == 0) {
                value += alignment;
            }
            return AcbHelper.RoundUpToAlignment(value, alignment);
        }

        public static MemberAbstract[] GetSearchTargetFieldsAndProperties(UtfRowBase tableObject) {
            var type = tableObject.GetType();
            var objFields = type.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
            var validDescriptors = new List<MemberAbstract>();
            foreach (var field in objFields) {
                var caField = field.GetCustomAttributes(typeof(UtfFieldAttribute), false);
                // It is a field that needs serialization.
                if (caField.Length == 1) {
                    var caArchive = field.GetCustomAttributes(typeof(Afs2ArchiveAttribute), false);
                    var ca1 = caField[0] as UtfFieldAttribute;
                    var ca2 = caArchive.Length == 1 ? caArchive[0] as Afs2ArchiveAttribute : null;
                    validDescriptors.Add(new MemberAbstract(field, ca1, ca2));
                }
            }
            validDescriptors.Sort((d1, d2) => d1.FieldAttribute.Order - d2.FieldAttribute.Order);
            return validDescriptors.ToArray();
        }

        public static byte[] GetAfs2ArchiveBytes(ReadOnlyCollection<byte[]> files, uint alignment) {
            if (files.Count == 0) {
                return new byte[0];
            }
            byte[] buffer;
            using (var memory = new MemoryStream()) {
                WriteAfs2ArchiveToStream(files, memory, alignment);
                buffer = new byte[memory.Length];
                Array.Copy(memory.GetBuffer(), buffer, buffer.Length);
            }
            return buffer;
        }

        private static void WriteAfs2ArchiveToStream(ReadOnlyCollection<byte[]> files, Stream stream, uint alignment) {
            var fileCount = (uint)files.Count;
            if (files.Count >= ushort.MaxValue) {
                throw new IndexOutOfRangeException("File count " + fileCount +" exceeds maximum possible value (65535).");
            }
            if (files.Count != 1) {
                throw new NotSupportedException("Currently DereTore does not support more than one file.");
            }
            stream.WriteBytes(Afs2Archive.Afs2Signature);
            const uint version = 0x00020401;
            stream.WriteUInt32LE(version);
            stream.WriteUInt32LE(fileCount);
            stream.WriteUInt32LE(alignment);
            const uint offsetFieldSize = (version >> 8) & 0xff; // version[1], always 4? See Afs2Archive.Initialize().

            // Prepare the fields.
            var afs2HeaderSegmentSize = 0x10 + // General data
                                        2 * fileCount + // Cue IDs
                                        offsetFieldSize * fileCount + // File offsets
                                        sizeof(uint); // Size of last file (U32)
            // Assuming the music file always has ID 0 in Waveform table and Cue table.
            var records = new List<Afs2FileRecord>();
            var currentFileRawOffset = afs2HeaderSegmentSize;
            for (ushort i = 0; i < fileCount; ++i) {
                var record = new Afs2FileRecord {
                    // TODO: Use the Cue table.
                    CueId = i,
                    FileOffsetRaw = currentFileRawOffset,
                    FileOffsetAligned = AcbHelper.RoundUpToAlignment(currentFileRawOffset, alignment)
                };
                records.Add(record);
                currentFileRawOffset = (uint)(record.FileOffsetAligned + files[i].Length);
            }

            var lastFileEndOffset = currentFileRawOffset;
            for (var i = 0; i < files.Count; ++i) {
                stream.WriteUInt16LE(records[i].CueId);
            }
            for (var i = 0; i < files.Count; ++i) {
                stream.WriteUInt32LE((uint)records[i].FileOffsetRaw);
            }
            // TODO: Dynamically judge it. See Afs2Archive.Initialize().
            stream.WriteUInt32LE(lastFileEndOffset);
            for (var i = 0; i < files.Count; ++i) {
                stream.SeekAndWriteBytes(files[i], records[i].FileOffsetAligned);
            }
        }

    }
}
