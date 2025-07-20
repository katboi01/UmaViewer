#region MIT license
// 
// MIT license
//
// Copyright (c) 2013 Corey Murtagh
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.IO;
using System.Runtime.InteropServices;
using NAudio.Lame.DLL;
using NAudio.Wave.WZT;

namespace NAudio.Lame
{
    /// <summary>MP3 encoding class, uses libmp3lame DLL to encode.</summary>
    public class LameMP3FileWriter : Stream
    {
        /// <summary>Union class for fast buffer conversion</summary>
        /// <remarks>
        /// <para>
        /// Because of the way arrays work in .NET, all of the arrays will have the same
        /// length value.  To prevent unaware code from trying to read/write from out of
        /// bounds, allocation is done at the grain of the Least Common Multiple of the
        /// sizes of the contained types.  In this case the LCM is 8 bytes - the size of
        /// a double or a long - which simplifies allocation.
        /// </para><para>
        /// This means that when you ask for an array of 500 bytes you will actually get 
        /// an array of 63 doubles - 504 bytes total.  Any code that uses the length of 
        /// the array will see only 63 bytes, shorts, etc.
        /// </para><para>
        /// CodeAnalysis does not like this class, with good reason.  It should never be
        /// exposed beyond the scope of the MP3FileWriter.
        /// </para>
        /// </remarks>
        // uncomment to suppress CodeAnalysis warnings for the ArrayUnion class:
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable", Justification = "This design breaks portability, but is never exposed outside the class.  Tested on 32-bit and 64-bit.")]
        [StructLayout(LayoutKind.Explicit)]
        private class ArrayUnion
        {
            /// <summary>Size array in bytes</summary>
            [FieldOffset(0)]
            public readonly int nBytes;

            [FieldOffset(16)]
            public readonly byte[] bytes;

            [FieldOffset(16)]
            public readonly short[] shorts;

            [FieldOffset(16)]
            public readonly int[] ints;

            [FieldOffset(16)]
            public readonly long[] longs;

            [FieldOffset(16)]
            public readonly float[] floats;

            [FieldOffset(16)]
            public readonly double[] doubles;

            // True sizes of the various array types, calculated from number of bytes

            public int nShorts { get { return nBytes / 2; } }
            public int nInts { get { return nBytes / 4; } }
            public int nLongs { get { return nBytes / 8; } }
            public int nFloats { get { return nBytes / 4; } }
            public int nDoubles { get { return nBytes / 8; } }

            /// <summary>Initialize array to hold the requested number of bytes</summary>
            /// <param name="reqBytes">Minimum byte count of array</param>
            /// <remarks>
            /// Since all arrays will have the same apparent count, allocation
            /// is done on the array with the largest data type.  This helps
            /// to prevent out-of-bounds reads and writes by methods that do
            /// not know about the union.
            /// </remarks>
            public ArrayUnion(int reqBytes)
            {
                // Calculate smallest number of doubles required to store the 
                // requested byte count
                int reqDoubles = (reqBytes + 7) / 8;

                this.doubles = new double[reqDoubles];
                this.nBytes = reqDoubles * 8;
            }
        };

        #region Properties
        // LAME library context 
        private LibMp3Lame _lame;

        // Format of input wave data
        private readonly WaveFormat inputFormat;

        // Output stream to write encoded data to
        private Stream outStream;

        // Flag to control whether we should dispose of output stream 
        private bool disposeOutput = false;
        #endregion

        #region Structors
        /// <summary>Create MP3FileWriter to write to a file on disk</summary>
        /// <param name="outFileName">Name of file to create</param>
        /// <param name="format">Input WaveFormat</param>
        /// <param name="quality">LAME quality preset</param>
        public LameMP3FileWriter(string outFileName, WaveFormat format, LAMEPreset quality)
            : this(File.Create(outFileName), format, quality)
        {
            this.disposeOutput = true;
        }

        /// <summary>Create MP3FileWriter to write to supplied stream</summary>
        /// <param name="outStream">Stream to write encoded data to</param>
        /// <param name="format">Input WaveFormat</param>
        /// <param name="quality">LAME quality preset</param>
        public LameMP3FileWriter(Stream outStream, WaveFormat format, LAMEPreset quality)
            : base()
        {
            // sanity check
            if (outStream == null)
                throw new ArgumentNullException("outStream");
            if (format == null)
                throw new ArgumentNullException("format");

            // check for unsupported wave formats
            if (format.Channels != 1 && format.Channels != 2)
                throw new ArgumentException(string.Format("Unsupported number of channels {0}", format.Channels), "format");
            if (format.Encoding != WaveFormatEncoding.Pcm && format.Encoding != WaveFormatEncoding.IeeeFloat)
                throw new ArgumentException(string.Format("Unsupported encoding format {0}", format.Encoding.ToString()), "format");
            if (format.Encoding == WaveFormatEncoding.Pcm && format.BitsPerSample != 16)
                throw new ArgumentException(string.Format("Unsupported PCM sample size {0}", format.BitsPerSample), "format");
            if (format.Encoding == WaveFormatEncoding.IeeeFloat && format.BitsPerSample != 32)
                throw new ArgumentException(string.Format("Unsupported Float sample size {0}", format.BitsPerSample), "format");
            if (format.SampleRate < 8000 || format.SampleRate > 48000)
                throw new ArgumentException(string.Format("Unsupported Sample Rate {0}", format.SampleRate), "format");

            // select encoder function that matches data format
            if (format.Encoding == WaveFormatEncoding.Pcm)
            {
                if (format.Channels == 1)
                    _encode = encode_pcm_16_mono;
                else
                    _encode = encode_pcm_16_stereo;
            }
            else
            {
                if (format.Channels == 1)
                    _encode = encode_float_mono;
                else
                    _encode = encode_float_stereo;
            }

            // Set base properties
            this.inputFormat = format;
            this.outStream = outStream;
            this.disposeOutput = false;

            // Allocate buffers based on sample rate
            this.inBuffer = new ArrayUnion(format.AverageBytesPerSecond);
            this.outBuffer = new byte[format.SampleRate * 5 / 4 + 7200];

            // Initialize lame library
            this._lame = new LibMp3Lame();

            this._lame.InputSampleRate = format.SampleRate;
            this._lame.NumChannels = format.Channels;

            this._lame.SetPreset(quality);

            this._lame.InitParams();
        }


        /// <summary>Create MP3FileWriter to write to a file on disk</summary>
        /// <param name="outFileName">Name of file to create</param>
        /// <param name="format">Input WaveFormat</param>
        /// <param name="bitRate">Output bit rate in kbps</param>
        public LameMP3FileWriter(string outFileName, WaveFormat format, int bitRate)
            : this(File.Create(outFileName), format, bitRate)
        {
            this.disposeOutput = true;
        }

        /// <summary>Create MP3FileWriter to write to supplied stream</summary>
        /// <param name="outStream">Stream to write encoded data to</param>
        /// <param name="format">Input WaveFormat</param>
        /// <param name="bitRate">Output bit rate in kbps</param>
        public LameMP3FileWriter(Stream outStream, WaveFormat format, int bitRate)
            : base()
        {
            // sanity check
            if (outStream == null)
                throw new ArgumentNullException("outStream");
            if (format == null)
                throw new ArgumentNullException("format");

            // check for unsupported wave formats
            if (format.Channels != 1 && format.Channels != 2)
                throw new ArgumentException(string.Format("Unsupported number of channels {0}", format.Channels), "format");
            if (format.Encoding != WaveFormatEncoding.Pcm && format.Encoding != WaveFormatEncoding.IeeeFloat)
                throw new ArgumentException(string.Format("Unsupported encoding format {0}", format.Encoding.ToString()), "format");
            if (format.Encoding == WaveFormatEncoding.Pcm && format.BitsPerSample != 16)
                throw new ArgumentException(string.Format("Unsupported PCM sample size {0}", format.BitsPerSample), "format");
            if (format.Encoding == WaveFormatEncoding.IeeeFloat && format.BitsPerSample != 32)
                throw new ArgumentException(string.Format("Unsupported Float sample size {0}", format.BitsPerSample), "format");
            if (format.SampleRate < 8000 || format.SampleRate > 48000)
                throw new ArgumentException(string.Format("Unsupported Sample Rate {0}", format.SampleRate), "format");

            // select encoder function that matches data format
            if (format.Encoding == WaveFormatEncoding.Pcm)
            {
                if (format.Channels == 1)
                    _encode = encode_pcm_16_mono;
                else
                    _encode = encode_pcm_16_stereo;
            }
            else
            {
                if (format.Channels == 1)
                    _encode = encode_float_mono;
                else
                    _encode = encode_float_stereo;
            }

            // Set base properties
            this.inputFormat = format;
            this.outStream = outStream;
            this.disposeOutput = false;

            // Allocate buffers based on sample rate
            this.inBuffer = new ArrayUnion(format.AverageBytesPerSecond);
            this.outBuffer = new byte[format.SampleRate * 5 / 4 + 7200];

            // Initialize lame library
            this._lame = new LibMp3Lame();

            this._lame.InputSampleRate = format.SampleRate;
            this._lame.NumChannels = format.Channels;

            this._lame.BitRate = bitRate;

            this._lame.InitParams();
        }


        // Close LAME instance and output stream on dispose
        /// <summary>Dispose of object</summary>
        /// <param name="final">True if called from destructor, false otherwise</param>
        protected override void Dispose(bool final)
        {
            if (_lame != null && outStream != null)
                Flush();

            if (_lame != null)
            {
                _lame.Dispose();
                _lame = null;
            }

            if (outStream != null && disposeOutput)
            {
                outStream.Dispose();
                outStream = null;
            }

            base.Dispose(final);
        }
        #endregion

        /// <summary>Get internal LAME library instance</summary>
        /// <returns>LAME library instance</returns>
        public LibMp3Lame GetLameInstance()
        {
            return _lame;
        }

        #region Internal encoder operations
        // Input buffer
        private ArrayUnion inBuffer = null;

        /// <summary>Current write position in input buffer</summary>
        private int inPosition;

        /// <summary>Output buffer, size determined by call to Lame.beInitStream</summary>
        protected byte[] outBuffer;

        long InputByteCount = 0;
        long OutputByteCount = 0;

        // encoder write functions, one for each supported input wave format

        private int encode_pcm_16_mono()
        {
            return _lame.Write(inBuffer.shorts, inPosition / 2, outBuffer, outBuffer.Length, true);
        }

        private int encode_pcm_16_stereo()
        {
            return _lame.Write(inBuffer.shorts, inPosition / 2, outBuffer, outBuffer.Length, false);
        }

        private int encode_float_mono()
        {
            return _lame.Write(inBuffer.floats, inPosition / 4, outBuffer, outBuffer.Length, true);
        }

        private int encode_float_stereo()
        {
            return _lame.Write(inBuffer.floats, inPosition / 4, outBuffer, outBuffer.Length, false);
        }

        // Selected encoding write function
        delegate int delEncode();
        delEncode _encode = null;

        // Pass data to encoder
        private void Encode()
        {
            // check if encoder closed
            if (outStream == null || _lame == null)
                throw new InvalidOperationException("Output Stream closed.");

            // If no data to encode, do nothing
            if (inPosition < inputFormat.Channels * 2)
                return;

            // send to encoder
            int rc = _encode();

            if (rc > 0)
            {
                outStream.Write(outBuffer, 0, rc);
                OutputByteCount += rc;
            }

            InputByteCount += inPosition;
            inPosition = 0;
        }
        #endregion

        #region Stream implementation
        /// <summary>Write-only stream.  Always false.</summary>
        public override bool CanRead { get { return false; } }
        /// <summary>Non-seekable stream.  Always false.</summary>
        public override bool CanSeek { get { return false; } }
        /// <summary>True when encoder can accept more data</summary>
        public override bool CanWrite { get { return outStream != null && _lame != null; } }

        /// <summary>Dummy Position.  Always 0.</summary>
        public override long Position
        {
            get { return 0; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>Dummy Length.  Always 0.</summary>
        public override long Length
        {
            get { return 0; }
        }

        /// <summary>Add data to output buffer, sending to encoder when buffer full</summary>
        /// <param name="buffer">Source buffer</param>
        /// <param name="offset">Offset of data in buffer</param>
        /// <param name="count">Length of data</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int blockSize = Math.Min(inBuffer.nBytes - inPosition, count);
                Buffer.BlockCopy(buffer, offset, inBuffer.bytes, inPosition, blockSize);

                inPosition += blockSize;
                count -= blockSize;
                offset += blockSize;

                if (inPosition >= inBuffer.nBytes)
                    Encode();
            }
        }

        /// <summary>Finalise compression, add final output to output stream and close encoder</summary>
        public override void Flush()
        {
            // write remaining data
            if (inPosition > 0)
                Encode();

            // finalize compression
            int rc = _lame.Flush(outBuffer, outBuffer.Length);
            if (rc > 0)
                outStream.Write(outBuffer, 0, rc);

            // Cannot continue after flush, so clear output stream
            if (disposeOutput)
                outStream.Dispose();
            outStream = null;
        }

        /// <summary>Reading not supported.  Throws NotImplementedException.</summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>Setting length not supported.  Throws NotImplementedException.</summary>
        /// <param name="value">Length value</param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>Seeking not supported.  Throws NotImplementedException.</summary>
        /// <param name="offset">Seek offset</param>
        /// <param name="origin">Seek origin</param>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
