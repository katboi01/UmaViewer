using System;
using System.Linq.Expressions;

namespace DereTore.Exchange.Audio.HCA {
    partial class HcaDecoder {

        public static string NameOf<T>(Expression<Func<T>> e)
        {
            return ((MemberExpression)e.Body).Member.Name;
        }
        internal uint DecodeBlock(byte[] waveDataBuffer, uint blockIndex) {
            if (waveDataBuffer == null) {
                throw new ArgumentNullException(NameOf(() => waveDataBuffer));
            }
            var waveBlockSize = GetMinWaveDataBufferSize();
            if (waveDataBuffer.Length < waveBlockSize) {
                throw new HcaException(ErrorMessages.GetBufferTooSmall(waveBlockSize, waveDataBuffer.Length), ActionResult.BufferTooSmall);
            }
            TransformWaveDataBlocks(SourceStream, waveDataBuffer, blockIndex, 1, GetProperWaveWriter());
            return 1;
        }

        internal uint DecodeBlocks(byte[] waveDataBuffer, uint startBlockIndex) {
            if (waveDataBuffer == null) {
                throw new ArgumentNullException(NameOf(() => waveDataBuffer));
            }
            var waveBlockSize = GetMinWaveDataBufferSize();
            if (waveDataBuffer.Length < waveBlockSize) {
                throw new HcaException(ErrorMessages.GetBufferTooSmall(waveBlockSize, waveDataBuffer.Length), ActionResult.BufferTooSmall);
            }
            var hcaInfo = HcaInfo;
            var numBlocksToDecode = (uint)(waveDataBuffer.Length / waveBlockSize);
            if (startBlockIndex + numBlocksToDecode >= hcaInfo.BlockCount) {
                numBlocksToDecode = hcaInfo.BlockCount - startBlockIndex;
            }
            if (numBlocksToDecode == 0) {
                return 0;
            }
            TransformWaveDataBlocks(SourceStream, waveDataBuffer, startBlockIndex, numBlocksToDecode, GetProperWaveWriter());
            return numBlocksToDecode;
        }

    }
}
