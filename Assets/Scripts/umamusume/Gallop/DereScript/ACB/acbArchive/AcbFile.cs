using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DereTore.Exchange.Archive.ACB
{
    public sealed partial class AcbFile : UtfTable
    {

        public static AcbFile FromStream(FileStream stream)
        {
            return FromStream(stream, 0, stream.Length, false);
        }

        public static AcbFile FromStream(FileStream stream, bool disposeStream)
        {
            return FromStream(stream, 0, stream.Length, disposeStream);
        }

        public static AcbFile FromStream(FileStream stream, long offset, long size)
        {
            return new AcbFile(stream, offset, size, stream.Name, false);
        }

        public static AcbFile FromStream(FileStream stream, long offset, long size, bool disposeStream)
        {
            return new AcbFile(stream, offset, size, stream.Name, disposeStream);
        }

        public static AcbFile FromStream(FileStream stream, long offset, long size, bool includeCueIdInFileName, bool disposeStream)
        {
            return new AcbFile(stream, offset, size, stream.Name, includeCueIdInFileName, disposeStream);
        }

        public static AcbFile FromStream(Stream stream, string acbFileName, bool disposeStream)
        {
            return FromStream(stream, 0, stream.Length, acbFileName, disposeStream);
        }

        public static AcbFile FromStream(Stream stream, long offset, long size, string acbFileName, bool disposeStream)
        {
            return new AcbFile(stream, offset, size, acbFileName, disposeStream);
        }

        public static AcbFile FromStream(Stream stream, long offset, long size, string acbFileName, bool includeCueIdInFileName, bool disposeStream)
        {
            return new AcbFile(stream, offset, size, acbFileName, includeCueIdInFileName, disposeStream);
        }

        public static AcbFile FromFile(string fileName)
        {
            var fs = File.Open(fileName, FileMode.Open, FileAccess.Read);
            return FromStream(fs, false);
        }

        public Dictionary<string, UtfTable> Tables
        {
            get
            {
                return _tables;
            }
        }

        public AcbCueRecord[] Cues
        {
            get
            {
                return _cues;
            }
        }

        public Afs2Archive InternalAwb
        {
            get
            {
                return _internalAwb;
            }
        }

        public Afs2Archive ExternalAwb
        {
            get
            {
                return _externalAwb;
            }
        }

        public UtfTable GetTable(string tableName)
        {
            if (_tables == null)
            {
                return null;
            }
            var tables = _tables;
            UtfTable table;
            if (tables.ContainsKey(tableName))
            {
                table = tables[tableName];
            }
            else
            {
                table = ResolveTable(tableName);
                if (table != null)
                {
                    tables.Add(tableName, table);
                }
            }
            return table;
        }

        public string[] GetFileNames()
        {
            if (_cues != null)
            {
                _fileNames = Cues.Select(cue => cue.CueName).ToArray();
            }
            return _fileNames;
        }

        public bool FileExists(string fileName)
        {
            return _fileNames != null && _fileNames.Contains(fileName);
        }

        public Stream OpenDataStream(string fileName)
        {
            AcbCueRecord cue;
            try
            {
                cue = Cues.Single(c => c.CueName == fileName);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Format("File '{0}' is not found or it has multiple entries.", fileName), ex);
            }
            return GetDataStreamFromCueInfo(cue, fileName);
        }

        public Stream OpenDataStream(uint cueId)
        {
            AcbCueRecord cue;
            var tempFileName = string.Format("cue #{0}", cueId);
            try
            {
                cue = Cues.Single(c => c.CueId == cueId);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Format("File '{0}' is not found or it has multiple entries.", tempFileName), ex);
            }
            return GetDataStreamFromCueInfo(cue, tempFileName);
        }

        public static string GetSymbolicFileNameFromCueId(uint cueId)
        {
            return string.Format("dat_{0:000000}.bin", cueId);
        }

    }
}
