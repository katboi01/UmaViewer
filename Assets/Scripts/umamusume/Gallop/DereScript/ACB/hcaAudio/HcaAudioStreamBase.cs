using System;
using System.IO;

namespace DereTore.Exchange.Audio.HCA {
    public abstract class HcaAudioStreamBase : Stream {

        protected HcaAudioStreamBase(Stream sourceStream, DecodeParams decodeParams) {
            _decodeParams = decodeParams;
        }

        public sealed override void Flush() {
            throw new NotSupportedException();
        }

        public sealed override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public sealed override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        public sealed override bool CanRead { get { return true; } }

        public sealed override bool CanWrite { get { return false; } }

        public bool AllowDisposedOperations { get; set; }

        public bool IsDisposed { get { return _isDisposed; } }

        public HcaDecoder Decoder { get { return _decoder; } }

        public abstract float LengthInSeconds { get; }

        public abstract uint LengthInSamples { get; }

        public HcaInfo HcaInfo { get { return _decoder.HcaInfo; } }

        public DecodeParams DecodeParams { get { return _decodeParams; } }

        protected bool EnsureNotDisposed() {
            if (IsDisposed) {
                if (AllowDisposedOperations) {
                    return false;
                } else {
                    throw new ObjectDisposedException(typeof(HcaAudioStream).Name);
                }
            } else {
                return true;
            }
        }

        protected override void Dispose(bool disposing) {
            _isDisposed = true;
            base.Dispose(disposing);
        }

        protected abstract bool HasMoreData { get; }

        protected HcaDecoder _decoder;
        private bool _isDisposed;
        private readonly DecodeParams _decodeParams;

    }
}
