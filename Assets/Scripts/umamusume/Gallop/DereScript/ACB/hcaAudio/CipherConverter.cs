using System;
using System.IO;
using System.Runtime.InteropServices;
using DereTore.Common;
using DereTore.Exchange.Audio.HCA.Native;

namespace DereTore.Exchange.Audio.HCA {
    public sealed class CipherConverter : HcaReader {

        public CipherConverter(Stream sourceStream, Stream outputStream, CipherConfig ccFrom, CipherConfig ccTo)
            : base(sourceStream) {
            _outputStream = outputStream;
            _ccFrom = ccFrom;
            _ccTo = ccTo;
        }

        public void Convert() {
            ParseHeaders();
            InitializeCiphers();
            UpdateHeader();
            ConvertData();
        }

        /// <summary>
        /// Whether to encrypt the header signatures ('HCA ', 'fmt ', 'ciph', etc.). Usually decoders recognize both types of header signatures.
        /// </summary>
        public bool EncryptHeaderSignatures { get; set; }

        private void UpdateHeader() {
            var dataOffset = HcaInfo.DataOffset;
            var buffer = new byte[dataOffset];
            var sourceStream = SourceStream;
            var outputStream = _outputStream;

            sourceStream.Seek(0, SeekOrigin.Begin);
            sourceStream.Read(buffer, 0, buffer.Length);
            sourceStream.Seek(0, SeekOrigin.Begin);

            uint v;
            // HCA
            v = sourceStream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.HCA)) {
                ProcessHeaderSignature(sourceStream, buffer);
                sourceStream.Skip(Marshal.SizeOf(typeof(HcaHeader)));
            }
            // FMT
            v = sourceStream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.FMT)) {
                ProcessHeaderSignature(sourceStream, buffer);
                sourceStream.Skip(Marshal.SizeOf(typeof(FormatHeader)));
            }
            // COMP / DEC
            v = sourceStream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.COMP)) {
                ProcessHeaderSignature(sourceStream, buffer);
                sourceStream.Skip(Marshal.SizeOf(typeof(CompressHeader)));
            } else if (MagicValues.IsMagicMatch(v, MagicValues.DEC)) {
                ProcessHeaderSignature(sourceStream, buffer);
                sourceStream.Skip(Marshal.SizeOf(typeof(DecodeHeader)));
            }
            // VBR
            v = sourceStream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.VBR)) {
                ProcessHeaderSignature(sourceStream, buffer);
                sourceStream.Skip(Marshal.SizeOf(typeof(VbrHeader)));
            }
            // ATH
            v = sourceStream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.ATH)) {
                ProcessHeaderSignature(sourceStream, buffer);
                sourceStream.Skip(Marshal.SizeOf(typeof(AthHeader)));
            }
            // LOOP
            v = sourceStream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.LOOP)) {
                ProcessHeaderSignature(sourceStream, buffer);
                sourceStream.Skip(Marshal.SizeOf(typeof(LoopHeader)));
            }
            // CIPH
            v = sourceStream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.CIPH)) {
                ProcessHeaderSignature(sourceStream, buffer);
                var cipherOffset = (int)(sourceStream.Position + 4);
                var u = (ushort)_ccTo.CipherType;
                if (BitConverter.IsLittleEndian) {
                    u = DereToreHelper.SwapEndian(u);
                }
                var cipherTypeBytes = BitConverter.GetBytes(u);
                buffer[cipherOffset] = cipherTypeBytes[0];
                buffer[cipherOffset + 1] = cipherTypeBytes[1];
            }
            // RVA
            v = sourceStream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.RVA)) {
                ProcessHeaderSignature(sourceStream, buffer);
                sourceStream.Skip(Marshal.SizeOf(typeof(RvaHeader)));
            }
            // COMM
            v = sourceStream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.COMM)) {
                ProcessHeaderSignature(sourceStream, buffer);
                sourceStream.Skip(Marshal.SizeOf(typeof(CommentHeader)));
                byte tmpByte;
                do {
                    tmpByte = (byte)sourceStream.ReadByte();
                } while (tmpByte != 0);
            }
            // PAD (undocumented)
            v = sourceStream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.PAD)) {
                ProcessHeaderSignature(sourceStream, buffer);
                sourceStream.Skip(4);
            }

            FixDataBlock(buffer);
            outputStream.Write(buffer, 0, buffer.Length);
        }

        private void ConvertData() {
            var hcaInfo = HcaInfo;
            var sourceStream = SourceStream;
            var outputStream = _outputStream;
            var dataOffset = hcaInfo.DataOffset;
            var totalBlockCount = hcaInfo.BlockCount;
            var buffer = new byte[hcaInfo.BlockSize];

            sourceStream.Seek(dataOffset, SeekOrigin.Begin);
            for (var i = 0; i < totalBlockCount; ++i) {
                var read = sourceStream.Read(buffer, 0, buffer.Length);
                if (read < buffer.Length) {
                    throw new IOException("Something went wrong while trying to read data.");
                }
                var r = ConvertBlock(buffer, outputStream);
                if (!r) {
                    throw new HcaException("Block conversion failed.", ActionResult.DecodeFailed);
                }
            }
        }

        private void InitializeCiphers() {
            _cipherFrom = new Cipher();
            _cipherFrom.Initialize(HcaInfo.CipherType, _ccFrom.Key1, _ccFrom.Key2);
            _cipherTo = new Cipher();
            _cipherTo.Initialize(_ccTo.CipherType, _ccTo.Key1, _ccTo.Key2);
        }

        private bool ConvertBlock(byte[] blockData, Stream outputStream) {
            var checksum = HcaHelper.Checksum(blockData, 0);
            if (checksum != 0) {
                return false;
            }
            _cipherFrom.Decrypt(blockData);
            var dataClass = new DataBits(blockData, (uint)blockData.Length);
            var magic = dataClass.GetBit(16);
            if (magic != 0xffff) {
                return false;
            }
            _cipherTo.Encrypt(blockData);
            FixDataBlock(blockData);
            outputStream.Write(blockData, 0, blockData.Length);
            return true;
        }

        private static void FixDataBlock(byte[] blockData) {
            var length = blockData.Length;
            var sum = HcaHelper.Checksum(blockData, 0, length - 2);
            if (BitConverter.IsLittleEndian) {
                sum = DereToreHelper.SwapEndian(sum);
            }
            var sumBytes = BitConverter.GetBytes(sum);
            blockData[length - 2] = sumBytes[0];
            blockData[length - 1] = sumBytes[1];
        }

        private static void OrBytes(byte[] blockData, int offset, int length) {
            var end = offset + length;
            for (var i = offset; i < blockData.Length && i < end; ++i) {
                blockData[i] = (byte)(blockData[i] & 0xf0);
            }
        }

        private void ProcessHeaderSignature(Stream stream, byte[] headerData) {
            if (!EncryptHeaderSignatures) {
                return;
            }
            var position = (int)stream.Position;
            OrBytes(headerData, position, 4);
        }

        private readonly Stream _outputStream;
        private readonly CipherConfig _ccFrom;
        private readonly CipherConfig _ccTo;
        private Cipher _cipherFrom;
        private Cipher _cipherTo;

    }
}
