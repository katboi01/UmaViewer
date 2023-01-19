using System.Collections.Generic;
using System.IO;
using DereTore.Common;
using DereTore.Exchange.Audio.HCA.Native;

namespace DereTore.Exchange.Audio.HCA {
    public abstract class HcaReader {

        public Stream SourceStream
        {
            get
            {
                return _sourceStream;
            }
        }

        public HcaInfo HcaInfo { get; private set; }

        public static bool IsHcaStream(Stream stream) {
            var position = stream.Position;
            bool result;
            try {
                // The cipher keys do nothing to validating HCA files.
                new HcaDecoder(stream, DecodeParams.Default);
                result = true;
            } catch (HcaException) {
                result = false;
            }
            stream.Seek(position, SeekOrigin.Begin);
            return result;
        }

        public static bool IsLoopedHcaStream(Stream stream) {
            var position = stream.Position;
            bool result;
            try {
                // The cipher keys do nothing to validating HCA files.
                var decoder = new HcaDecoder(stream, DecodeParams.Default);
                result = decoder.HcaInfo.LoopFlag;
            } catch (HcaException) {
                result = false;
            }
            stream.Seek(position, SeekOrigin.Begin);
            return result;
        }

        internal void ParseHeaders() {
            var stream = SourceStream;
            uint v;
            var hcaInfo = new HcaInfo();
            // HCA
            v = stream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.HCA)) {
                HcaHeader header;
                stream.Read(out header);
                hcaInfo.Version = DereToreHelper.SwapEndian(header.Version);
                hcaInfo.DataOffset = DereToreHelper.SwapEndian(header.DataOffset);
            } else {
                throw new HcaException("Missing HCA signature.", ActionResult.MagicNotMatch);
            }
            // FMT
            v = stream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.FMT)) {
                FormatHeader header;
                stream.Read(out header);
                hcaInfo.ChannelCount = header.Channels;
                hcaInfo.SamplingRate = DereToreHelper.SwapEndian(header.SamplingRate << 8);
                hcaInfo.BlockCount = DereToreHelper.SwapEndian(header.Blocks);
                hcaInfo.FmtR01 = DereToreHelper.SwapEndian(header.R01);
                hcaInfo.FmtR02 = DereToreHelper.SwapEndian(header.R02);
                if (hcaInfo.ChannelCount < 1 || hcaInfo.ChannelCount > 16) {
                    throw new HcaException(string.Format("Channel count should be between 1 and 16, read {0}.", hcaInfo.ChannelCount), ActionResult.InvalidFieldValue);
                }
                if (hcaInfo.SamplingRate < 1 || hcaInfo.SamplingRate > 0x7fffff) {
                    throw new HcaException(string.Format("Sampling rate should be between 1 and {0}, read {1}.", 0x7fffffff, hcaInfo.SamplingRate), ActionResult.InvalidFieldValue);
                }
            } else {
                throw new HcaException("Missing FMT signature.", ActionResult.MagicNotMatch);
            }
            // COMP / DEC
            v = stream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.COMP)) {
                CompressHeader header;
                stream.Read(out header);
                hcaInfo.BlockSize = DereToreHelper.SwapEndian(header.BlockSize);
                hcaInfo.CompR01 = header.R01;
                hcaInfo.CompR02 = header.R02;
                hcaInfo.CompR03 = header.R03;
                hcaInfo.CompR04 = header.R04;
                hcaInfo.CompR05 = header.R05;
                hcaInfo.CompR06 = header.R06;
                hcaInfo.CompR07 = header.R07;
                hcaInfo.CompR08 = header.R08;
                if ((hcaInfo.BlockSize < 8 || hcaInfo.BlockSize > 0xffff) && hcaInfo.BlockSize != 0) {
                    throw new HcaException(string.Format("Block size should be between 8 and {0}, read {hcaInfo.BlockSize}.", 0xffff), ActionResult.InvalidFieldValue);
                }
                if (!(hcaInfo.CompR01 <= hcaInfo.CompR02 && hcaInfo.CompR02 <= 0x1f)) {
                    throw new HcaException(string.Format("CompR01 should be less than or equal to CompR02, and CompR02 should be less than or equal to {0}, read {1} and {2}.", 0x1f, hcaInfo.CompR01, hcaInfo.CompR02), ActionResult.InvalidFieldValue);
                }
            } else if (MagicValues.IsMagicMatch(v, MagicValues.DEC)) {
                DecodeHeader header;
                stream.Read(out header);
                hcaInfo.CompR01 = header.R01;
                hcaInfo.CompR02 = header.R02;
                hcaInfo.CompR03 = header.R04;
                hcaInfo.CompR04 = header.R03;
                hcaInfo.CompR05 = (ushort)(header.Count1 + 1);
                hcaInfo.CompR06 = (ushort)((header.EnableCount2 ? header.Count2 : header.Count1) + 1);
                hcaInfo.CompR07 = (ushort)(hcaInfo.CompR05 - hcaInfo.CompR06);
                hcaInfo.CompR08 = 0;
                if ((hcaInfo.BlockSize < 8 || hcaInfo.BlockSize > 0xffff) && hcaInfo.BlockSize != 0) {
                    throw new HcaException(string.Format("Block size should be between 8 and {0}, read {1}.", 0xffff, hcaInfo.BlockSize), ActionResult.InvalidFieldValue);
                }
                if (!(hcaInfo.CompR01 <= hcaInfo.CompR02 && hcaInfo.CompR02 <= 0x1f)) {
                    throw new HcaException(string.Format("CompR01 should be less than or equal to CompR02, and CompR02 should be less than or equal to {0}, read {1} and {2}.", 0x1f, hcaInfo.CompR01, hcaInfo.CompR02), ActionResult.InvalidFieldValue);
                }
                if (hcaInfo.CompR03 == 0) {
                    hcaInfo.CompR03 = 1;
                }
            } else {
                throw new HcaException("Missing COMP/DEC signature.", ActionResult.MagicNotMatch);
            }
            // VBR
            v = stream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.VBR)) {
                VbrHeader header;
                stream.Read(out header);
                hcaInfo.VbrR01 = DereToreHelper.SwapEndian(header.R01);
                hcaInfo.VbrR02 = DereToreHelper.SwapEndian(header.R02);
                if (!(hcaInfo.BlockSize == 0 && hcaInfo.VbrR01 < 0x01ff)) {
                    throw new HcaException(string.Format("VbrR01 should be less than {0} in VBR HCA.", 0x01ff), ActionResult.InvalidFieldValue);
                }
            } else {
                hcaInfo.VbrR01 = hcaInfo.VbrR02 = 0;
            }
            // ATH
            v = stream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.ATH)) {
                AthHeader header;
                stream.Read(out header);
                hcaInfo.AthType = header.Type;
            } else {
                hcaInfo.AthType = (ushort)(hcaInfo.Version < 0x0200 ? 1 : 0);
            }
            // LOOP
            v = stream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.LOOP)) {
                LoopHeader header;
                stream.Read(out header);
                hcaInfo.LoopStart = DereToreHelper.SwapEndian(header.LoopStart);
                hcaInfo.LoopEnd = DereToreHelper.SwapEndian(header.LoopEnd);
                hcaInfo.LoopR01 = DereToreHelper.SwapEndian(header.R01);
                hcaInfo.LoopR02 = DereToreHelper.SwapEndian(header.R02);
                hcaInfo.LoopFlag = true;
            } else {
                hcaInfo.LoopStart = 0;
                hcaInfo.LoopEnd = 0;
                hcaInfo.LoopR01 = 0;
                hcaInfo.LoopR02 = 0x400;
                hcaInfo.LoopFlag = false;
            }
            // CIPH
            v = stream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.CIPH)) {
                CipherHeader header;
                stream.Read(out header);
                hcaInfo.CipherType = (CipherType)DereToreHelper.SwapEndian(header.Type);
            } else {
                hcaInfo.CipherType = CipherType.NoChipher;
            }
            // RVA
            v = stream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.RVA)) {
                RvaHeader header;
                stream.Read(out header);
                hcaInfo.RvaVolume = DereToreHelper.SwapEndian(header.Volume);
            } else {
                hcaInfo.RvaVolume = 1;
            }
            // COMM
            v = stream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.COMM)) {
                CommentHeader header;
                stream.Read(out header);
                hcaInfo.CommentLength = header.Length;
                var tmpCommentCharList = new List<byte>();
                byte tmpByte;
                do {
                    tmpByte = (byte)stream.ReadByte();
                    tmpCommentCharList.Add(tmpByte);
                } while (tmpByte != 0);
                hcaInfo.Comment = tmpCommentCharList.ToArray();
            } else {
                hcaInfo.CommentLength = 0;
                hcaInfo.Comment = null;
            }
            // PAD (undocumented)
            v = stream.PeekUInt32LE();
            if (MagicValues.IsMagicMatch(v, MagicValues.PAD)) {
                stream.Skip(4); // Length of 'pad '
            }

            if (hcaInfo.CompR03 == 0) {
                hcaInfo.CompR03 = 1;
            }

            if (hcaInfo.CompR01 != 1 || hcaInfo.CompR02 != 0xf) {
                throw new HcaException(string.Format("Expected CompR01=1, CompR02=15, read {0}, {1}.", hcaInfo.CompR01, hcaInfo.CompR02), ActionResult.InvalidFieldValue);
            }
            hcaInfo.CompR09 = HcaHelper.Ceil2((uint)(hcaInfo.CompR05 - (hcaInfo.CompR06 + hcaInfo.CompR07)), hcaInfo.CompR08);
            HcaInfo = hcaInfo;
        }

        protected HcaReader(Stream sourceStream) {
            _sourceStream = sourceStream;
        }

        private readonly Stream _sourceStream;
    }
}
