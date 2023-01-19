using System.IO;

namespace DereTore.Exchange.Audio.HCA {
    public interface IWaveWriter {

        SamplingMode SamplingMode { get; }

        uint BytesPerSample { get; }

        /// <summary>
        /// Decode an audio sample to buffer.
        /// </summary>
        /// <param name="f">Audio sample value.</param>
        /// <param name="buffer">Decode buffer for output.</param>
        /// <param name="offset">Offset of the desired starting byte in buffer.</param>
        /// <returns>Bytes written.</returns>
        uint DecodeToBuffer(float f, byte[] buffer, uint offset);

        uint DecodeToStream(float f, Stream stream);

    }
}
