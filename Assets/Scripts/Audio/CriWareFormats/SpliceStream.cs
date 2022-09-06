using System;
using System.IO;

namespace CriWareFormats
{
    public class SpliceStream : Stream
    {
        private readonly Stream innerStream;
        private readonly long realPosition;

        private long internalPosition;

        private readonly object positionLock = new object();

        public SpliceStream(Stream sourceStream, long offset, long length)
        {
            innerStream = sourceStream;
            realPosition = offset;
            internalPosition = 0;

            Length = length;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length { get; }

        public override long Position
        {
            get
            {
                lock (positionLock)
                {
                    return internalPosition;
                }
            }
            set
            {
                lock (positionLock)
                {
                    long checkValue = value + realPosition;
                    if (value < 0 || value >= Length) throw new ArgumentOutOfRangeException(nameof(value));
                    internalPosition = value;

                    if (innerStream.CanSeek) innerStream.Position = checkValue;
                }
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (positionLock)
            {
                if (innerStream.CanRead)
                {
                    long restore = innerStream.Position;
                    innerStream.Position = realPosition + internalPosition;
                    int read = innerStream.Read(buffer, offset, count);
                    internalPosition += read;
                    innerStream.Position = restore;
                    return read;
                }
                else return 0;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            lock (positionLock)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        if (offset >= Length) throw new ArgumentOutOfRangeException(nameof(offset));
                        internalPosition = offset;
                        break;

                    case SeekOrigin.Current:
                        if (internalPosition + offset >= Length) throw new ArgumentOutOfRangeException(nameof(offset));
                        internalPosition += offset;
                        break;

                    case SeekOrigin.End:
                        if (internalPosition - offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
                        internalPosition = Length;
                        internalPosition -= offset;
                        break;

                    default:
                        break;
                }

                return internalPosition;
            }
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
