using System;
using System.IO;
using System.Text;
using DereTore.Common;

namespace DereTore.Exchange.Archive.ACB {
    internal sealed class UtfReader {

        public UtfReader() {
            _isEncrypted = false;
        }

        public UtfReader(byte seed, byte increment) {
            _seed = seed;
            _increment = increment;
            _isEncrypted = true;
        }

        public bool IsEncrypted { get { return _isEncrypted; } }

        /// <summary>
        ///
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="baseOffset">Offset of the UTF table, starting from the beginning of ACB file.</param>
        /// <param name="size">Requested data size.</param>
        /// <param name="utfOffset">Offset of the bytes to read, starting from the beginning of the UTF table.</param>
        /// <returns></returns>
        public byte[] PeekBytes(Stream stream, long baseOffset, int size, long utfOffset) {
            var data = stream.PeekBytes(baseOffset + utfOffset, size);
            if (!IsEncrypted) {
                return data;
            }
            if (utfOffset < _currentUtfOffset) {
                _currentUtfOffset = 0;
            }
            if (_currentUtfOffset == 0) {
                _currentXor = _seed;
            }
            for (var j = _currentUtfOffset; j < utfOffset; j++) {
                if (j > 0) {
                    _currentXor *= _increment;
                }
                _currentUtfOffset++;
            }
            for (long i = 0; i < size; i++) {
                if ((_currentUtfOffset != 0) || (i > 0)) {
                    _currentXor *= _increment;
                }

                data[i] ^= _currentXor;
                _currentUtfOffset++;
            }
            return data;
        }

        public byte PeekByte(Stream stream, long baseOffset, long utfOffset) {
            var data = stream.PeekByte(baseOffset + utfOffset);
            if (!IsEncrypted) {
                return data;
            }
            if (utfOffset < _currentUtfOffset) {
                _currentUtfOffset = 0;
            }
            if (_currentUtfOffset == 0) {
                _currentXor = _seed;
            }
            for (var j = _currentUtfOffset; j < utfOffset; j++) {
                if (j > 0) {
                    _currentXor *= _increment;
                }
                _currentUtfOffset++;
            }
            if (_currentUtfOffset != 0) {
                _currentXor *= _increment;
            }
            data ^= _currentXor;
            _currentUtfOffset++;
            return data;
        }

        public sbyte PeekSByte(Stream stream, long baseOffset, long utfOffset) {
            unchecked {
                return (sbyte)PeekByte(stream, baseOffset, utfOffset);
            }
        }

        public ushort PeekUInt16(Stream stream, long baseOffset, long utfOffset) {
            var temp = PeekBytes(stream, baseOffset, 2, utfOffset);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToUInt16(temp, 0);
        }

        public short PeekInt16(Stream stream, long baseOffset, long utfOffset) {
            var temp = PeekBytes(stream, baseOffset, 2, utfOffset);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToInt16(temp, 0);
        }

        public uint PeekUInt32(Stream stream, long baseOffset, long utfOffset) {
            var temp = PeekBytes(stream, baseOffset, 4, utfOffset);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToUInt32(temp, 0);
        }

        public int PeekInt32(Stream stream, long baseOffset, long utfOffset) {
            var temp = PeekBytes(stream, baseOffset, 4, utfOffset);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToInt32(temp, 0);
        }

        public ulong PeekUInt64(Stream stream, long baseOffset, long utfOffset) {
            var temp = PeekBytes(stream, baseOffset, 8, utfOffset);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToUInt64(temp, 0);
        }

        public float PeekSingle(Stream stream, long baseOffset, long utfOffset) {
            var temp = PeekBytes(stream, baseOffset, 4, utfOffset);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToSingle(temp, 0);
        }

        public string ReadZeroEndedStringAsAscii(Stream stream, long baseOffset, long utfOffset) {
            if (!IsEncrypted) {
                return stream.PeekZeroEndedStringAsAscii(baseOffset + utfOffset);
            }

            stream.Position = baseOffset + utfOffset;
            if (utfOffset < _currentUtfStringOffset) {
                _currentUtfStringOffset = 0;
            }

            if (_currentUtfStringOffset == 0) {
                _currentStringXor = _seed;
            }
            for (var j = _currentUtfStringOffset; j < utfOffset; j++) {
                if (j > 0) {
                    _currentStringXor *= _increment;
                }
                _currentUtfStringOffset++;
            }

            var asciiVal = new StringBuilder();
            var remained = stream.Length - stream.Position - (baseOffset + utfOffset);
            for (var i = 0; i < remained; i++) {
                _currentStringXor *= _increment;
                _currentUtfStringOffset++;
                var encryptedByte = (byte)stream.ReadByte();
                var decryptedByte = (byte)(encryptedByte ^ _currentStringXor);
                if (decryptedByte == 0) {
                    break;
                } else {
                    asciiVal.Append((char)decryptedByte);
                }
            }
            return asciiVal.ToString();
        }

        private readonly bool _isEncrypted;
        private readonly byte _increment;
        private readonly byte _seed;

        private byte _currentXor;
        private long _currentUtfOffset;
        private byte _currentStringXor;
        private long _currentUtfStringOffset;

    }
}
