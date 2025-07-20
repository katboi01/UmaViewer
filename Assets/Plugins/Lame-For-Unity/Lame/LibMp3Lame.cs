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
#region Attributions
//
// Contents of the LibMp3Lame.NativeMethods class and associated enumerations
// are directly based on the lame.h v1.190, available at:
//		http://lame.cvs.sourceforge.net/viewvc/lame/lame/include/lame.h?revision=1.190&content-type=text%2Fplain
//
// Source lines and comments included where useful/possible.
//
#endregion
using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace NAudio.Lame
{
    /// <summary>LAME encoding presets</summary>
    public enum LAMEPreset : int
    {
        /*values from 8 to 320 should be reserved for abr bitrates*/
        /*for abr I'd suggest to directly use the targeted bitrate as a value*/

        /// <summary>8-kbit ABR</summary>
        ABR_8 = 8,
        /// <summary>16-kbit ABR</summary>
        ABR_16 = 16,
        /// <summary>32-kbit ABR</summary>
        ABR_32 = 32,
        /// <summary>48-kbit ABR</summary>
        ABR_48 = 48,
        /// <summary>64-kbit ABR</summary>
        ABR_64 = 64,
        /// <summary>96-kbit ABR</summary>
        ABR_96 = 96,
        /// <summary>128-kbit ABR</summary>
        ABR_128 = 128,
        /// <summary>160-kbit ABR</summary>
        ABR_160 = 160,
        /// <summary>256-kbit ABR</summary>
        ABR_256 = 256,
        /// <summary>320-kbit ABR</summary>
        ABR_320 = 320,

        /*Vx to match Lame and VBR_xx to match FhG*/
        /// <summary>VBR Quality 9</summary>
        V9 = 410,
        /// <summary>FhG: VBR Q10</summary>
        VBR_10 = 410,
        /// <summary>VBR Quality 8</summary>
        V8 = 420,
        /// <summary>FhG: VBR Q20</summary>
        VBR_20 = 420,
        /// <summary>VBR Quality 7</summary>
        V7 = 430,
        /// <summary>FhG: VBR Q30</summary>
        VBR_30 = 430,
        /// <summary>VBR Quality 6</summary>
        V6 = 440,
        /// <summary>FhG: VBR Q40</summary>
        VBR_40 = 440,
        /// <summary>VBR Quality 5</summary>
        V5 = 450,
        /// <summary>FhG: VBR Q50</summary>
        VBR_50 = 450,
        /// <summary>VBR Quality 4</summary>
        V4 = 460,
        /// <summary>FhG: VBR Q60</summary>
        VBR_60 = 460,
        /// <summary>VBR Quality 3</summary>
        V3 = 470,
        /// <summary>FhG: VBR Q70</summary>
        VBR_70 = 470,
        /// <summary>VBR Quality 2</summary>
        V2 = 480,
        /// <summary>FhG: VBR Q80</summary>
        VBR_80 = 480,
        /// <summary>VBR Quality 1</summary>
        V1 = 490,
        /// <summary>FhG: VBR Q90</summary>
        VBR_90 = 490,
        /// <summary>VBR Quality 0</summary>
        V0 = 500,
        /// <summary>FhG: VBR Q100</summary>
        VBR_100 = 500,

        /*still there for compatibility*/
        /// <summary>R3Mix quality - </summary>
        R3MIX = 1000,
        /// <summary>Standard Quality</summary>
        STANDARD = 1001,
        /// <summary>Extreme Quality</summary>
        EXTREME = 1002,
        /// <summary>Insane Quality</summary>
        INSANE = 1003,
        /// <summary>Fast Standard Quality</summary>
        STANDARD_FAST = 1004,
        /// <summary>Fast Extreme Quality</summary>
        EXTREME_FAST = 1005,
        /// <summary>Medium Quality</summary>
        MEDIUM = 1006,
        /// <summary>Fast Medium Quality</summary>
        MEDIUM_FAST = 1007
    }
}

namespace NAudio.Lame.DLL
{
    /// <summary>MPEG channel mode</summary>
    public enum MPEGMode : uint
    {
        /// <summary>Stereo</summary>
        Stereo = 0,
        /// <summary>Joint Stereo</summary>
        // JointStereo = 1,
        DualChannel = 2,
        // LAME does not support this
        /// <summary>Mono</summary>
        Mono = 3,
        /// <summary>Undefined</summary>
        NotSet = 4
    }


    /// <summary>Assembler optimizations</summary>
    public enum ASMOptimizations : uint
    {
        /// <summary>Use MMX instructions</summary>
        MMX = 1,
        /// <summary>Use AMD 3DNow instructions</summary>
        AMD_3DNow = 2,
        /// <summary>Use SSE instructions</summary>
        SSE = 3
    }

    /// <summary>
    /// Variable BitRate Mode
    /// </summary>
    public enum VBRMode : uint
    {
        /// <summary>No VBR (Constant Bitrate)</summary>
        Off = 0,
        /// <summary>MT Algorithm (Mark Taylor).  Now same as MTRH</summary>
        MT,
        /// <summary>RH Algorithm (Roger Hegemann)</summary>
        RH,
        /// <summary>ABR - Average Bitrate</summary>
        ABR,
        /// <summary>MTRH Algorithm (Mark Taylor &amp; Roger Hegemann)(</summary>
        MTRH,
        /// <summary>Default algorithm: MTRH</summary>
        Default = MTRH
    }

    /// <summary>MPEG wrapper version</summary>
    public enum MPEGVersion : int
    {
        /// <summary>MPEG 2</summary>
        MPEG2 = 0,
        /// <summary>MPEG 1</summary>
        MPEG1 = 1,
        /// <summary>MPEG 2.5</summary>
        MPEG2_5 = 2
    }

    /// <summary>LAME DLL version information</summary>
    [StructLayout(LayoutKind.Sequential)]
    public class LAMEVersion
    {
        /* generic LAME version */
        /// <summary>LAME library major version</summary>
        public int major;
        /// <summary>LAME library minor version</summary>
        public int minor;
        /// <summary>LAME library 'Alpha' version flag</summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool alpha;
        /// <summary>LAME library 'Beta' version flag</summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool beta;

        /// <summary>Psychoacoustic code major version</summary>
        public int psy_major;
        /// <summary>Psychoacoustic code minor version</summary>
        public int psy_minor;
        /// <summary>Psychoacoustic code 'Alpha' version flag</summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool psy_alpha;
        /// <summary>Psychoacoustic code 'Beta' version flag</summary>
        [MarshalAs(UnmanagedType.Bool)]
        public bool psy_beta;

        /* compile time features */
        // const char *features;    /* Don't make assumptions about the contents! */
        private IntPtr features_ptr = IntPtr.Zero;

        /// <summary>Compile-time features string</summary>
        public string features
        {
            get
            {
                if (features_ptr != IntPtr.Zero)
                    return Marshal.PtrToStringAuto(features_ptr);
                return null;
            }
        }
    }

    /// <summary>LAME interface class</summary>
    public class LibMp3Lame : IDisposable
    {
        /// <summary>Constructor</summary>
        public LibMp3Lame()
        {
            context = NativeMethods.lame_init();
            //InitReportFunctions();
        }

        /// <summary>Destructor</summary>
        ~LibMp3Lame()
        {
            Dispose(true);
        }

        /// <summary>Dispose of object</summary>
        public void Dispose()
        {
            Dispose(false);
        }

        /// <summary>Clean up object, closing LAME context if present</summary>
        /// <param name="final">True if called from destructor, else false</param>
        protected virtual void Dispose(bool final)
        {
            if (context != IntPtr.Zero)
            {
                NativeMethods.lame_close(context);
                context = IntPtr.Zero;
            }
        }

        #region LAME context handle

        private IntPtr context = IntPtr.Zero;

        #endregion

        #region DLL version data

        /// <summary>Lame Version</summary>
        public static string LameVersion { get { return NativeMethods.get_lame_version(); } }

        /// <summary>Lame Short Version</summary>
        public static string LameShortVersion { get { return NativeMethods.get_lame_short_version(); } }

        /// <summary>Lame Very Short Version</summary>
        public static string LameVeryShortVersion { get { return NativeMethods.get_lame_very_short_version(); } }

        /// <summary>Lame Psychoacoustic Version</summary>
        public static string LamePsychoacousticVersion { get { return NativeMethods.get_psy_version(); } }

        /// <summary>Lame URL</summary>
        public static string LameURL { get { return NativeMethods.get_lame_url(); } }

        /// <summary>Lame library bit width - 32 or 64 bit</summary>
        public static string LameOSBitness { get { return NativeMethods.get_lame_os_bitness(); } }

        /// <summary>Get LAME version information</summary>
        /// <returns>LAME version structure</returns>
        public static LAMEVersion GetLameVersion()
        {
            LAMEVersion ver = new LAMEVersion();
            NativeMethods.get_lame_version_numerical(ver);
            return ver;
        }

        #endregion

        #region Properties

        delegate int setFunc<T>(IntPtr p, T val);

        // wrapper function to simplify calling lame_set_* entry points
        void setter<T>(setFunc<T> f, T value, string name = null)
        {
            int res = f(context, value);
            if (res != 0)
            {
                if (string.IsNullOrEmpty(name))
                    name = f.Method.Name;
                throw new Exception(string.Format("libmp3lame: {0}({1}) returned error code: {2}", name, value, res));
            }
        }

        #region Input Stream Description

        /// <summary>Number of samples (optional)</summary>
        public UInt64 NumSamples
        {
            get { return NativeMethods.lame_get_num_samples(context); }
            set { setter(NativeMethods.lame_set_num_samples, value); }
        }

        /// <summary>Input sample rate</summary>
        public int InputSampleRate
        {
            get { return NativeMethods.lame_get_in_samplerate(context); }
            set { setter(NativeMethods.lame_set_in_samplerate, value); }
        }

        /// <summary>Number of channels</summary>
        public int NumChannels
        {
            get { return NativeMethods.lame_get_num_channels(context); }
            set { setter(NativeMethods.lame_set_num_channels, value); }
        }

        /// <summary>Global amplification factor</summary>
        public float Scale
        {
            get { return NativeMethods.lame_get_scale(context); }
            set { setter(NativeMethods.lame_set_scale, value); }
        }

        /// <summary>Left channel amplification</summary>
        public float ScaleLeft
        {
            get { return NativeMethods.lame_get_scale_left(context); }
            set { setter(NativeMethods.lame_set_scale_left, value); }
        }

        /// <summary>Right channel amplification</summary>
        public float ScaleRight
        {
            get { return NativeMethods.lame_get_scale_right(context); }
            set { setter(NativeMethods.lame_set_scale_right, value); }
        }

        /// <summary>Output sample rate</summary>
        public int OutputSampleRate
        {
            get { return NativeMethods.lame_get_out_samplerate(context); }
            set { setter(NativeMethods.lame_set_out_samplerate, value); }
        }

        #endregion

        #region General Control Parameters

        /// <summary>Enable analysis</summary>
        public bool Anaylysis
        {
            get { return NativeMethods.lame_get_analysis(context); }
            set { setter(NativeMethods.lame_set_analysis, value); }
        }

        /// <summary>Write VBR tag to MP3 file</summary>
        public bool WriteVBRTag
        {
            get { return NativeMethods.lame_get_bWriteVbrTag(context) != 0; }
            set { setter(NativeMethods.lame_set_bWriteVbrTag, value ? 1 : 0); }
        }

        /// <summary></summary>
        public bool DecodeOnly
        {
            get { return NativeMethods.lame_get_decode_only(context) != 0; }
            set { setter(NativeMethods.lame_set_decode_only, value ? 1 : 0); }
        }

        /// <summary>Encoding quality</summary>
        public int Quality
        {
            get { return NativeMethods.lame_get_quality(context); }
            set { setter(NativeMethods.lame_set_quality, value); }
        }

        /// <summary>Specify MPEG channel mode, or use best guess if false</summary>
        public MPEGMode Mode
        {
            get { return NativeMethods.lame_get_mode(context); }
            set { setter(NativeMethods.lame_set_mode, value); }
        }

        /// <summary>Force M/S mode</summary>
        public bool ForceMS
        {
            get { return NativeMethods.lame_get_force_ms(context); }
            set { setter(NativeMethods.lame_set_force_ms, value); }
        }

        /// <summary>Use free format</summary>
        public bool UseFreeFormat
        {
            get { return NativeMethods.lame_get_free_format(context); }
            set { setter(NativeMethods.lame_set_free_format, value); }
        }

        /// <summary>Perform replay gain analysis</summary>
        public bool FindReplayGain
        {
            get { return NativeMethods.lame_get_findReplayGain(context); }
            set { setter(NativeMethods.lame_set_findReplayGain, value); }
        }

        /// <summary>Decode on the fly.  Search for the peak sample.  If the ReplayGain analysis is enabled then perform the analysis on the decoded data stream.</summary>
        public bool DecodeOnTheFly
        {
            get { return NativeMethods.lame_get_decode_on_the_fly(context); }
            set { setter(NativeMethods.lame_set_decode_on_the_fly, value); }
        }

        /// <summary>Counters for gapless encoding</summary>
        public int NoGapTotal
        {
            get { return NativeMethods.lame_get_nogap_total(context); }
            set { setter(NativeMethods.lame_set_nogap_total, value); }
        }

        /// <summary>Counters for gapless encoding</summary>
        public int NoGapCurrentIndex
        {
            get { return NativeMethods.lame_get_nogap_currentindex(context); }
            set { setter(NativeMethods.lame_set_nogap_currentindex, value); }
        }

        /// <summary>Output bitrate</summary>
        public int BitRate
        {
            get { return NativeMethods.lame_get_brate(context); }
            set { setter(NativeMethods.lame_set_brate, value); }
        }

        /// <summary>Output compression ratio</summary>
        public float CompressionRatio
        {
            get { return NativeMethods.lame_get_compression_ratio(context); }
            set { setter(NativeMethods.lame_set_compression_ratio, value); }
        }

        /// <summary>Set compression preset</summary>
        public bool SetPreset(LAMEPreset preset)
        {
            int res = NativeMethods.lame_set_preset(context, preset);
            return res == 0;
        }

        /// <summary>Enable/Disable optimizations</summary>
        public bool SetOptimization(ASMOptimizations opt, bool enabled)
        {
            int res = NativeMethods.lame_set_asm_optimizations(context, opt, enabled);
            return res == 0;
        }

        #endregion

        #region Frame parameters

        /// <summary>Set output Copyright flag</summary>
        public bool Copyright
        {
            get { return NativeMethods.lame_get_copyright(context); }
            set { setter(NativeMethods.lame_set_copyright, value); }
        }

        /// <summary>Set output Original flag</summary>
        public bool Original
        {
            get { return NativeMethods.lame_get_original(context); }
            set { setter(NativeMethods.lame_set_original, value); }
        }

        /// <summary>Set error protection.  Uses 2 bytes from each frame for CRC checksum</summary>
        public bool ErrorProtection
        {
            get { return NativeMethods.lame_get_error_protection(context); }
            set { setter(NativeMethods.lame_set_error_protection, value); }
        }

        /// <summary>MP3 'private extension' bit.  Meaningless.</summary>
        public bool Extension
        {
            get { return NativeMethods.lame_get_extension(context); }
            set { setter(NativeMethods.lame_set_extension, value); }
        }

        /// <summary>Enforce strict ISO compliance.</summary>
        public bool StrictISO
        {
            get { return NativeMethods.lame_get_strict_ISO(context); }
            set { setter(NativeMethods.lame_set_strict_ISO, value); }
        }

        #endregion

        #region Quantization/Noise Shaping

        /// <summary>Disable the bit reservoir.</summary>
        public bool DisableReservoir { get { return NativeMethods.lame_get_disable_reservoir(context); } set { setter(NativeMethods.lame_set_disable_reservoir, value); } }

        /// <summary>Set a different "best quantization" function</summary>
        public int QuantComp { get { return NativeMethods.lame_get_quant_comp(context); } set { setter(NativeMethods.lame_set_quant_comp, value); } }

        /// <summary>Set a different "best quantization" function</summary>
        public int QuantCompShort { get { return NativeMethods.lame_get_quant_comp_short(context); } set { setter(NativeMethods.lame_set_quant_comp_short, value); } }

        /// <summary>Set a different "best quantization" function</summary>
        public int ExperimentalX { get { return NativeMethods.lame_get_experimentalX(context); } set { setter(NativeMethods.lame_set_experimentalX, value); } }

        /// <summary>Set a different "best quantization" function</summary>
        public int ExperimentalY { get { return NativeMethods.lame_get_experimentalY(context); } set { setter(NativeMethods.lame_set_experimentalY, value); } }

        /// <summary>Set a different "best quantization" function</summary>
        public int ExperimentalZ { get { return NativeMethods.lame_get_experimentalZ(context); } set { setter(NativeMethods.lame_set_experimentalZ, value); } }

        /// <summary>Set a different "best quantization" function</summary>
        public int ExperimentalNSPsyTune { get { return NativeMethods.lame_get_exp_nspsytune(context); } set { setter(NativeMethods.lame_set_exp_nspsytune, value); } }

        /// <summary>Set a different "best quantization" function</summary>
        public int MSFix { get { return NativeMethods.lame_get_msfix(context); } set { setter(NativeMethods.lame_set_msfix, value); } }

        #endregion

        #region VBR Control

        /// <summary>Set VBR mode</summary>
        public VBRMode VBR { get { return NativeMethods.lame_get_VBR(context); } set { setter(NativeMethods.lame_set_VBR, value); } }

        /// <summary>VBR quality level.  0 = highest, 9 = lowest.</summary>
        public int VBRQualityLevel { get { return NativeMethods.lame_get_VBR_q(context); } set { setter(NativeMethods.lame_set_VBR_q, value); } }

        /// <summary>VBR quality level.  0 = highest, 9 = lowest</summary>
        public float VBRQuality { get { return NativeMethods.lame_get_VBR_quality(context); } set { setter(NativeMethods.lame_set_VBR_quality, value); } }

        /// <summary>ABR average bitrate</summary>
        public int VBRMeanBitrateKbps { get { return NativeMethods.lame_get_VBR_mean_bitrate_kbps(context); } set { setter(NativeMethods.lame_set_VBR_mean_bitrate_kbps, value); } }

        /// <summary>ABR minimum bitrate</summary>
        public int VBRMinBitrateKbps { get { return NativeMethods.lame_get_VBR_min_bitrate_kbps(context); } set { setter(NativeMethods.lame_set_VBR_min_bitrate_kbps, value); } }

        /// <summary>ABR maximum bitrate</summary>
        public int VBRMaxBitrateKbps { get { return NativeMethods.lame_get_VBR_max_bitrate_kbps(context); } set { setter(NativeMethods.lame_set_VBR_max_bitrate_kbps, value); } }

        /// <summary>Strictly enforce minimum bitrate.  Normall it will be violated for analog silence.</summary>
        public bool VBRHardMin { get { return NativeMethods.lame_get_VBR_hard_min(context); } set { setter(NativeMethods.lame_set_VBR_hard_min, value); } }

        #endregion

        #region Filtering control

#pragma warning disable 1591
        public int LowPassFreq { get { return NativeMethods.lame_get_lowpassfreq(context); } set { setter(NativeMethods.lame_set_lowpassfreq, value); } }

        public int LowPassWidth { get { return NativeMethods.lame_get_lowpasswidth(context); } set { setter(NativeMethods.lame_set_lowpasswidth, value); } }

        public int HighPassFreq { get { return NativeMethods.lame_get_highpassfreq(context); } set { setter(NativeMethods.lame_set_highpassfreq, value); } }

        public int HighPassWidth { get { return NativeMethods.lame_get_highpasswidth(context); } set { setter(NativeMethods.lame_set_highpasswidth, value); } }
#pragma warning restore 1591
        #endregion

        #region Internal state variables, read only

#pragma warning disable 1591
        public MPEGVersion Version { get { return NativeMethods.lame_get_version(context); } }

        public int EncoderDelay { get { return NativeMethods.lame_get_encoder_delay(context); } }

        public int EncoderPadding { get { return NativeMethods.lame_get_encoder_padding(context); } }

        public int MFSamplesToEncode { get { return NativeMethods.lame_get_mf_samples_to_encode(context); } }

        public int MP3BufferSize { get { return NativeMethods.lame_get_size_mp3buffer(context); } }

        public int FrameNumber { get { return NativeMethods.lame_get_frameNum(context); } }

        public int TotalFrames { get { return NativeMethods.lame_get_totalframes(context); } }

        public int RadioGain { get { return NativeMethods.lame_get_RadioGain(context); } }

        public int AudiophileGain { get { return NativeMethods.lame_get_AudiophileGain(context); } }

        public float PeakSample { get { return NativeMethods.lame_get_PeakSample(context); } }

        public int NoClipGainChange { get { return NativeMethods.lame_get_noclipGainChange(context); } }

        public float NoClipScale { get { return NativeMethods.lame_get_noclipScale(context); } }
#pragma warning restore 1591
        #endregion

        #endregion

        #region Methods

        /// <summary>Initialize encoder with parameters</summary>
        /// <returns>Success/fail</returns>
        public bool InitParams()
        {
            if (context == IntPtr.Zero)
                throw new InvalidOperationException("InitParams called without initializing context");
            int res = NativeMethods.lame_init_params(context);
            return res == 0;
        }

        /// <summary>Write 16-bit integer PCM samples to encoder</summary>
        /// <param name="samples">PCM sample data.  Interleaved for stereo.</param>
        /// <param name="nSamples">Number of valid samples.</param>
        /// <param name="output">Buffer to write encoded data to</param>
        /// <param name="outputSize">Size of buffer.</param>
        /// <param name="mono">True if mono, false if stereo.</param>
        /// <returns>Number of bytes of encoded data written to output buffer.</returns>
        public int Write(short[] samples, int nSamples, byte[] output, int outputSize, bool mono)
        {
            int rc = -1;
            if (mono)
                rc = NativeMethods.lame_encode_buffer(context, samples, samples, nSamples, output, outputSize);
            else
                rc = NativeMethods.lame_encode_buffer_interleaved(context, samples, nSamples / 2, output, outputSize);

            return rc;
        }

        // float
        /// <summary>Write 32-bit floating point PCM samples to encoder</summary>
        /// <param name="samples">PCM sample data.  Interleaved for stereo.</param>
        /// <param name="nSamples">Number of valid samples.</param>
        /// <param name="output">Buffer to write encoded data to</param>
        /// <param name="outputSize">Size of buffer.</param>
        /// <param name="mono">True if mono, false if stereo.</param>
        /// <returns>Number of bytes of encoded data written to output buffer.</returns>
        public int Write(float[] samples, int nSamples, byte[] output, int outputSize, bool mono)
        {
            int rc = -1;
            if (mono)
                rc = NativeMethods.lame_encode_buffer_ieee_float(context, samples, samples, nSamples, output, outputSize);
            else
                rc = NativeMethods.lame_encode_buffer_interleaved_ieee_float(context, samples, nSamples / 2, output, outputSize);
            return rc;
        }

        /// <summary>Flush encoder output</summary>
        /// <param name="output">Buffer to write encoded data to</param>
        /// <param name="outputSize">Size of buffer.</param>
        /// <returns>Number of bytes of encoded data written to output buffer.</returns>
        public int Flush(byte[] output, int outputSize)
        {
            int res = NativeMethods.lame_encode_flush(context, output, outputSize);
            return Math.Max(0, res);
        }

        #endregion

        internal static class NativeMethods
        {
#if UNITY_EDITOR
    #if UNITY_EDITOR_OSX
            const string libname = @"__Internal";
    #else
            const string libname = @"libmp3lame.dll";
    #endif
#elif UNITY_IOS
            const string libname = @"__Internal";
#elif UNITY_ANDROID
            const string libname = @"mp3lame";
#else
            const string libname = @"libmp3lame.dll";
#endif

            // typedef void (*lame_report_function)(const char *format, va_list ap);

            #region Startup/Shutdown

            /*
			 * REQUIRED:
			 * initialize the encoder.  sets default for all encoder parameters,
			 * returns NULL if some malloc()'s failed
			 * otherwise returns pointer to structure needed for all future
			 * API calls.
			 */
            // lame_global_flags * CDECL lame_init(void);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr lame_init();

            /*
			 * REQUIRED:
			 * final call to free all remaining buffers
			 */
            // int  CDECL lame_close (lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_close(IntPtr context);

            #endregion

            #region LAME information

            /*
			 * OPTIONAL:
			 * get the version number, in a string. of the form:
			 * "3.63 (beta)" or just "3.63".
			 */
            // const char*  CDECL get_lame_version       ( void );
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern string get_lame_version();
            // const char*  CDECL get_lame_short_version ( void );
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern string get_lame_short_version();
            // const char*  CDECL get_lame_very_short_version ( void );
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern string get_lame_very_short_version();
            // const char*  CDECL get_psy_version        ( void );
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern string get_psy_version();
            // const char*  CDECL get_lame_url           ( void );
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern string get_lame_url();
            // const char*  CDECL get_lame_os_bitness    ( void );
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern string get_lame_os_bitness();

            // void CDECL get_lame_version_numerical(lame_version_t *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void get_lame_version_numerical([Out]LAMEVersion ver);

            #endregion

            #region Input Stream Description

            /* number of samples.  default = 2^32-1   */
            // int CDECL lame_set_num_samples(lame_global_flags *, unsigned long);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_num_samples(IntPtr context, UInt64 num_samples);
            // unsigned long CDECL lame_get_num_samples(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern UInt64 lame_get_num_samples(IntPtr context);

            /* input sample rate in Hz.  default = 44100hz */
            // int CDECL lame_set_in_samplerate(lame_global_flags *, int);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_in_samplerate(IntPtr context, int value);
            // int CDECL lame_get_in_samplerate(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_in_samplerate(IntPtr context);

            /* number of channels in input stream. default=2  */
            //int CDECL lame_set_num_channels(lame_global_flags *, int);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_num_channels(IntPtr context, int value);
            //int CDECL lame_get_num_channels(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_num_channels(IntPtr context);

            /*
			  scale the input by this amount before encoding.  default=1
			  (not used by decoding routines)
			*/
            //int CDECL lame_set_scale(lame_global_flags *, float);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_scale(IntPtr context, float value);
            //float CDECL lame_get_scale(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern float lame_get_scale(IntPtr context);

            /*
			  scale the channel 0 (left) input by this amount before encoding.  default=1
			  (not used by decoding routines)
			*/
            // int CDECL lame_set_scale_left(lame_global_flags *, float);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_scale_left(IntPtr context, float value);
            // float CDECL lame_get_scale_left(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern float lame_get_scale_left(IntPtr context);

            /*
			  scale the channel 1 (right) input by this amount before encoding.  default=1
			  (not used by decoding routines)
			*/
            // int CDECL lame_set_scale_right(lame_global_flags *, float);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_scale_right(IntPtr context, float value);
            // float CDECL lame_get_scale_right(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern float lame_get_scale_right(IntPtr context);

            /*
			  output sample rate in Hz.  default = 0, which means LAME picks best value
			  based on the amount of compression.  MPEG only allows:
			  MPEG1    32, 44.1,   48khz
			  MPEG2    16, 22.05,  24
			  MPEG2.5   8, 11.025, 12
			  (not used by decoding routines)
			*/
            // int CDECL lame_set_out_samplerate(lame_global_flags *, int);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_out_samplerate(IntPtr context, int value);
            // int CDECL lame_get_out_samplerate(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_out_samplerate(IntPtr context);

            #endregion

            #region General control parameters

            /* 1=cause LAME to collect data for an MP3 frame analyzer. default=0 */
            // int CDECL lame_set_analysis(lame_global_flags *, int);
            // int CDECL lame_get_analysis(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_analysis(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_analysis(IntPtr context);

            /*
			  1 = write a Xing VBR header frame.
			  default = 1
			  this variable must have been added by a Hungarian notation Windows programmer :-)
			*/
            // int CDECL lame_set_bWriteVbrTag(lame_global_flags *, int);
            // int CDECL lame_get_bWriteVbrTag(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_bWriteVbrTag(IntPtr context, [MarshalAs(UnmanagedType.Bool)] int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_bWriteVbrTag(IntPtr context);

            /* 1=decode only.  use lame/mpglib to convert mp3/ogg to wav.  default=0 */
            // int CDECL lame_set_decode_only(lame_global_flags *, int);
            // int CDECL lame_get_decode_only(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_decode_only(IntPtr context, [MarshalAs(UnmanagedType.Bool)] int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_decode_only(IntPtr context);

            /*
			  internal algorithm selection.  True quality is determined by the bitrate
			  but this variable will effect quality by selecting expensive or cheap algorithms.
			  quality=0..9.  0=best (very slow).  9=worst.
			  recommended:  2     near-best quality, not too slow
							5     good quality, fast
							7     ok quality, really fast
			*/
            // int CDECL lame_set_quality(lame_global_flags *, int);
            // int CDECL lame_get_quality(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_quality(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_quality(IntPtr context);

            /*
			  mode = 0,1,2,3 = stereo, jstereo, dual channel (not supported), mono
			  default: lame picks based on compression ration and input channels
			*/
            // int CDECL lame_set_mode(lame_global_flags *, MPEG_mode);
            // MPEG_mode CDECL lame_get_mode(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_mode(IntPtr context, MPEGMode value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern MPEGMode lame_get_mode(IntPtr context);

            /*
			  force_ms.  Force M/S for all frames.  For testing only.
			  default = 0 (disabled)
			*/
            // int CDECL lame_set_force_ms(lame_global_flags *, int);
            // int CDECL lame_get_force_ms(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_force_ms(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_force_ms(IntPtr context);

            /* use free_format?  default = 0 (disabled) */
            // int CDECL lame_set_free_format(lame_global_flags *, int);
            // int CDECL lame_get_free_format(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_free_format(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_free_format(IntPtr context);

            /* perform ReplayGain analysis?  default = 0 (disabled) */
            // int CDECL lame_set_findReplayGain(lame_global_flags *, int);
            // int CDECL lame_get_findReplayGain(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_findReplayGain(IntPtr context, [In, MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_findReplayGain(IntPtr context);

            /* decode on the fly. Search for the peak sample. If the ReplayGain
			 * analysis is enabled then perform the analysis on the decoded data
			 * stream. default = 0 (disabled)
			 * NOTE: if this option is set the build-in decoder should not be used */
            // int CDECL lame_set_decode_on_the_fly(lame_global_flags *, int);
            // int CDECL lame_get_decode_on_the_fly(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_decode_on_the_fly(IntPtr context, [In, MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_decode_on_the_fly(IntPtr context);

            /* counters for gapless encoding */
            // int CDECL lame_set_nogap_total(lame_global_flags*, int);
            // int CDECL lame_get_nogap_total(const lame_global_flags*);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_nogap_total(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_nogap_total(IntPtr context);

            // int CDECL lame_set_nogap_currentindex(lame_global_flags* , int);
            // int CDECL lame_get_nogap_currentindex(const lame_global_flags*);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_nogap_currentindex(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_nogap_currentindex(IntPtr context);

            /* set one of brate compression ratio.  default is compression ratio of 11.  */
            // int CDECL lame_set_brate(lame_global_flags *, int);
            // int CDECL lame_get_brate(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_brate(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_brate(IntPtr context);

            // int CDECL lame_set_compression_ratio(lame_global_flags *, float);
            // float CDECL lame_get_compression_ratio(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_compression_ratio(IntPtr context, float value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern float lame_get_compression_ratio(IntPtr context);

            //int CDECL lame_set_preset( lame_global_flags*  gfp, int );
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_preset(IntPtr context, LAMEPreset value);

            //int CDECL lame_set_asm_optimizations( lame_global_flags*  gfp, int, int );
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_asm_optimizations(IntPtr context, ASMOptimizations opt, [MarshalAs(UnmanagedType.Bool)] bool val);

            #endregion

            #region Frame parameters

            /* mark as copyright.  default=0 */
            // int CDECL lame_set_copyright(lame_global_flags *, int);
            // int CDECL lame_get_copyright(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_copyright(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_copyright(IntPtr context);

            /* mark as original.  default=1 */
            // int CDECL lame_set_original(lame_global_flags *, int);
            // int CDECL lame_get_original(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_original(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_original(IntPtr context);

            /* error_protection.  Use 2 bytes from each frame for CRC checksum. default=0 */
            // int CDECL lame_set_error_protection(lame_global_flags *, int);
            // int CDECL lame_get_error_protection(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_error_protection(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_error_protection(IntPtr context);

            /* MP3 'private extension' bit  Meaningless.  default=0 */
            // int CDECL lame_set_extension(lame_global_flags *, int);
            // int CDECL lame_get_extension(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_extension(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_extension(IntPtr context);

            /* enforce strict ISO compliance.  default=0 */
            // int CDECL lame_set_strict_ISO(lame_global_flags *, int);
            // int CDECL lame_get_strict_ISO(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_strict_ISO(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_strict_ISO(IntPtr context);

            #endregion

            #region Quantization/Noise Shaping

            /* disable the bit reservoir. For testing only. default=0 */
            // int CDECL lame_set_disable_reservoir(lame_global_flags *, int);
            // int CDECL lame_get_disable_reservoir(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_disable_reservoir(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_disable_reservoir(IntPtr context);

            /* select a different "best quantization" function. default=0  */
            // int CDECL lame_set_quant_comp(lame_global_flags *, int);
            // int CDECL lame_get_quant_comp(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_quant_comp(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_quant_comp(IntPtr context);

            // int CDECL lame_set_quant_comp_short(lame_global_flags *, int);
            // int CDECL lame_get_quant_comp_short(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_quant_comp_short(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_quant_comp_short(IntPtr context);

            // int CDECL lame_set_experimentalX(lame_global_flags *, int); /* compatibility*/
            // int CDECL lame_get_experimentalX(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_experimentalX(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_experimentalX(IntPtr context);

            /* another experimental option.  for testing only */
            // int CDECL lame_set_experimentalY(lame_global_flags *, int);
            // int CDECL lame_get_experimentalY(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_experimentalY(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_experimentalY(IntPtr context);

            /* another experimental option.  for testing only */
            // int CDECL lame_set_experimentalZ(lame_global_flags *, int);
            // int CDECL lame_get_experimentalZ(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_experimentalZ(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_experimentalZ(IntPtr context);

            /* Naoki's psycho acoustic model.  default=0 */
            // int CDECL lame_set_exp_nspsytune(lame_global_flags *, int);
            // int CDECL lame_get_exp_nspsytune(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_exp_nspsytune(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_exp_nspsytune(IntPtr context);

            // void CDECL lame_set_msfix(lame_global_flags *, double);
            // float CDECL lame_get_msfix(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_msfix(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_msfix(IntPtr context);

            #endregion

            #region VBR control

            /* Types of VBR.  default = vbr_off = CBR */
            // int CDECL lame_set_VBR(lame_global_flags *, vbr_mode);
            // vbr_mode CDECL lame_get_VBR(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_VBR(IntPtr context, VBRMode value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern VBRMode lame_get_VBR(IntPtr context);

            /* VBR quality level.  0=highest  9=lowest  */
            // int CDECL lame_set_VBR_q(lame_global_flags *, int);
            // int CDECL lame_get_VBR_q(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_VBR_q(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_VBR_q(IntPtr context);

            /* VBR quality level.  0=highest  9=lowest, Range [0,...,10[  */
            // int CDECL lame_set_VBR_quality(lame_global_flags *, float);
            // float CDECL lame_get_VBR_quality(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_VBR_quality(IntPtr context, float value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern float lame_get_VBR_quality(IntPtr context);

            /* Ignored except for VBR=vbr_abr (ABR mode) */
            // int CDECL lame_set_VBR_mean_bitrate_kbps(lame_global_flags *, int);
            // int CDECL lame_get_VBR_mean_bitrate_kbps(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_VBR_mean_bitrate_kbps(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_VBR_mean_bitrate_kbps(IntPtr context);

            // int CDECL lame_set_VBR_min_bitrate_kbps(lame_global_flags *, int);
            // int CDECL lame_get_VBR_min_bitrate_kbps(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_VBR_min_bitrate_kbps(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_VBR_min_bitrate_kbps(IntPtr context);

            // int CDECL lame_set_VBR_max_bitrate_kbps(lame_global_flags *, int);
            // int CDECL lame_get_VBR_max_bitrate_kbps(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_VBR_max_bitrate_kbps(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_VBR_max_bitrate_kbps(IntPtr context);

            /*
			  1=strictly enforce VBR_min_bitrate.  Normally it will be violated for
			  analog silence
			*/
            // int CDECL lame_set_VBR_hard_min(lame_global_flags *, int);
            // int CDECL lame_get_VBR_hard_min(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_VBR_hard_min(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_VBR_hard_min(IntPtr context);

            #endregion

            #region Filtering control

            /* freq in Hz to apply lowpass. Default = 0 = lame chooses.  -1 = disabled */
            // int CDECL lame_set_lowpassfreq(lame_global_flags *, int);
            // int CDECL lame_get_lowpassfreq(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_lowpassfreq(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_lowpassfreq(IntPtr context);

            /* width of transition band, in Hz.  Default = one polyphase filter band */
            // int CDECL lame_set_lowpasswidth(lame_global_flags *, int);
            // int CDECL lame_get_lowpasswidth(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_lowpasswidth(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_lowpasswidth(IntPtr context);

            /* freq in Hz to apply highpass. Default = 0 = lame chooses.  -1 = disabled */
            // int CDECL lame_set_highpassfreq(lame_global_flags *, int);
            // int CDECL lame_get_highpassfreq(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_highpassfreq(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_highpassfreq(IntPtr context);

            /* width of transition band, in Hz.  Default = one polyphase filter band */
            // int CDECL lame_set_highpasswidth(lame_global_flags *, int);
            // int CDECL lame_get_highpasswidth(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_highpasswidth(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_highpasswidth(IntPtr context);

            #endregion

            #region Psychoacoustics and other advanced settings

            /* only use ATH for masking */
            // int CDECL lame_set_ATHonly(lame_global_flags *, int);
            // int CDECL lame_get_ATHonly(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_ATHonly(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_ATHonly(IntPtr context);

            /* only use ATH for short blocks */
            // int CDECL lame_set_ATHshort(lame_global_flags *, int);
            // int CDECL lame_get_ATHshort(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_ATHshort(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_ATHshort(IntPtr context);

            /* disable ATH */
            // int CDECL lame_set_noATH(lame_global_flags *, int);
            // int CDECL lame_get_noATH(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_noATH(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_noATH(IntPtr context);

            /* select ATH formula */
            // int CDECL lame_set_ATHtype(lame_global_flags *, int);
            // int CDECL lame_get_ATHtype(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_ATHtype(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_ATHtype(IntPtr context);

            /* lower ATH by this many db */
            // int CDECL lame_set_ATHlower(lame_global_flags *, float);
            // float CDECL lame_get_ATHlower(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_ATHlower(IntPtr context, float value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern float lame_get_ATHlower(IntPtr context);

            /* select ATH adaptive adjustment type */
            // int CDECL lame_set_athaa_type( lame_global_flags *, int);
            // int CDECL lame_get_athaa_type( const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_athaa_type(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_athaa_type(IntPtr context);

            /* adjust (in dB) the point below which adaptive ATH level adjustment occurs */
            // int CDECL lame_set_athaa_sensitivity( lame_global_flags *, float);
            // float CDECL lame_get_athaa_sensitivity( const lame_global_flags* );
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_athaa_sensitivity(IntPtr context, float value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern float lame_get_athaa_sensitivity(IntPtr context);

            /*
			  allow blocktypes to differ between channels?
			  default: 0 for jstereo, 1 for stereo
			*/
            // int CDECL lame_set_allow_diff_short(lame_global_flags *, int);
            // int CDECL lame_get_allow_diff_short(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_allow_diff_short(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_allow_diff_short(IntPtr context);

            /* use temporal masking effect (default = 1) */
            // int CDECL lame_set_useTemporal(lame_global_flags *, int);
            // int CDECL lame_get_useTemporal(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_useTemporal(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_useTemporal(IntPtr context);

            /* use temporal masking effect (default = 1) */
            // int CDECL lame_set_interChRatio(lame_global_flags *, float);
            // float CDECL lame_get_interChRatio(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_interChRatio(IntPtr context, float value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern float lame_get_interChRatio(IntPtr context);

            /* disable short blocks */
            // int CDECL lame_set_no_short_blocks(lame_global_flags *, int);
            // int CDECL lame_get_no_short_blocks(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_no_short_blocks(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_no_short_blocks(IntPtr context);

            /* force short blocks */
            // int CDECL lame_set_force_short_blocks(lame_global_flags *, int);
            // int CDECL lame_get_force_short_blocks(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_force_short_blocks(IntPtr context, [MarshalAs(UnmanagedType.Bool)] bool value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool lame_get_force_short_blocks(IntPtr context);

            /* Input PCM is emphased PCM (for instance from one of the rarely
			   emphased CDs), it is STRONGLY not recommended to use this, because
			   psycho does not take it into account, and last but not least many decoders
			   ignore these bits */
            // int CDECL lame_set_emphasis(lame_global_flags *, int);
            // int CDECL lame_get_emphasis(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_emphasis(IntPtr context, int value);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_emphasis(IntPtr context);

            #endregion

            #region Internal state variables, read only

            /* version  0=MPEG-2  1=MPEG-1  (2=MPEG-2.5)     */
            // int CDECL lame_get_version(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern MPEGVersion lame_get_version(IntPtr context);

            /* encoder delay   */
            // int CDECL lame_get_encoder_delay(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_encoder_delay(IntPtr context);

            /*
			  padding appended to the input to make sure decoder can fully decode
			  all input.  Note that this value can only be calculated during the
			  call to lame_encoder_flush().  Before lame_encoder_flush() has
			  been called, the value of encoder_padding = 0.
			*/
            // int CDECL lame_get_encoder_padding(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_encoder_padding(IntPtr context);

            /* size of MPEG frame */
            // int CDECL lame_get_framesize(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_framesize(IntPtr context);

            /* number of PCM samples buffered, but not yet encoded to mp3 data. */
            // int CDECL lame_get_mf_samples_to_encode( const lame_global_flags*  gfp );
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_mf_samples_to_encode(IntPtr context);

            /*
			  size (bytes) of mp3 data buffered, but not yet encoded.
			  this is the number of bytes which would be output by a call to
			  lame_encode_flush_nogap.  NOTE: lame_encode_flush() will return
			  more bytes than this because it will encode the reamining buffered
			  PCM samples before flushing the mp3 buffers.
			*/
            // int CDECL lame_get_size_mp3buffer( const lame_global_flags*  gfp );
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_size_mp3buffer(IntPtr context);

            /* number of frames encoded so far */
            // int CDECL lame_get_frameNum(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_frameNum(IntPtr context);

            /*
			  lame's estimate of the total number of frames to be encoded
			   only valid if calling program set num_samples
			*/
            // int CDECL lame_get_totalframes(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_totalframes(IntPtr context);

            /* RadioGain value. Multiplied by 10 and rounded to the nearest. */
            // int CDECL lame_get_RadioGain(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_RadioGain(IntPtr context);

            /* AudiophileGain value. Multipled by 10 and rounded to the nearest. */
            // int CDECL lame_get_AudiophileGain(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_AudiophileGain(IntPtr context);

            /* the peak sample */
            // float CDECL lame_get_PeakSample(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern float lame_get_PeakSample(IntPtr context);

            /* Gain change required for preventing clipping. The value is correct only if
			   peak sample searching was enabled. If negative then the waveform
			   already does not clip. The value is multiplied by 10 and rounded up. */
            // int CDECL lame_get_noclipGainChange(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_get_noclipGainChange(IntPtr context);

            /* user-specified scale factor required for preventing clipping. Value is
			   correct only if peak sample searching was enabled and no user-specified
			   scaling was performed. If negative then either the waveform already does
			   not clip or the value cannot be determined */
            // float CDECL lame_get_noclipScale(const lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern float lame_get_noclipScale(IntPtr context);

            #endregion

            #region Processing

            /*
		 * REQUIRED:
		 * sets more internal configuration based on data provided above.
		 * returns -1 if something failed.
		 */
            // int CDECL lame_init_params(lame_global_flags *);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_init_params(IntPtr context);

            /*
			 * input pcm data, output (maybe) mp3 frames.
			 * This routine handles all buffering, resampling and filtering for you.
			 *
			 * return code     number of bytes output in mp3buf. Can be 0
			 *                 -1:  mp3buf was too small
			 *                 -2:  malloc() problem
			 *                 -3:  lame_init_params() not called
			 *                 -4:  psycho acoustic problems
			 *
			 * The required mp3buf_size can be computed from num_samples,
			 * samplerate and encoding rate, but here is a worst case estimate:
			 *
			 * mp3buf_size in bytes = 1.25*num_samples + 7200
			 *
			 * I think a tighter bound could be:  (mt, March 2000)
			 * MPEG1:
			 *    num_samples*(bitrate/8)/samplerate + 4*1152*(bitrate/8)/samplerate + 512
			 * MPEG2:
			 *    num_samples*(bitrate/8)/samplerate + 4*576*(bitrate/8)/samplerate + 256
			 *
			 * but test first if you use that!
			 *
			 * set mp3buf_size = 0 and LAME will not check if mp3buf_size is
			 * large enough.
			 *
			 * NOTE:
			 * if gfp->num_channels=2, but gfp->mode = 3 (mono), the L & R channels
			 * will be averaged into the L channel before encoding only the L channel
			 * This will overwrite the data in buffer_l[] and buffer_r[].
			 *
			*/
            // int CDECL lame_encode_buffer (
            //		lame_global_flags*  gfp,           /* global context handle         */
            //		const short int     buffer_l [],   /* PCM data for left channel     */
            //		const short int     buffer_r [],   /* PCM data for right channel    */
            //		const int           nsamples,      /* number of samples per channel */
            //		unsigned char*      mp3buf,        /* pointer to encoded MP3 stream */
            //		const int           mp3buf_size ); /* number of valid octets in this stream */
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_buffer(IntPtr context,
                                                          [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I2)]
				short[] buffer_l,
                                                          [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I2)]
				short[] buffer_r,
                                                          int nSamples,
                                                          [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                          int mp3buf_size
            );

            /*
			 * as above, but input has L & R channel data interleaved.
			 * NOTE:
			 * num_samples = number of samples in the L (or R)
			 * channel, not the total number of samples in pcm[]
			 */
            // int CDECL lame_encode_buffer_interleaved(
            //		lame_global_flags*  gfp,           /* global context handlei        */
            //		short int           pcm[],         /* PCM data for left and right
            //											  channel, interleaved          */
            //		int                 num_samples,   /* number of samples per channel, _not_ number of samples in pcm[] */
            //		unsigned char*      mp3buf,        /* pointer to encoded MP3 stream */
            //		int                 mp3buf_size ); /* number of valid octets in this stream */
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_buffer_interleaved(IntPtr context,
                                                                      [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I2)]
				short[] pcm,
                                                                      int num_samples,
                                                                      [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                                      int mp3buf_size
            );

            /* as lame_encode_buffer, but for 'float's.
			 * !! NOTE: !! data must still be scaled to be in the same range as
			 * short int, +/- 32768
			 */
            // int CDECL lame_encode_buffer_float(
            //		lame_global_flags*  gfp,           /* global context handle         */
            //		const float         pcm_l [],      /* PCM data for left channel     */
            //		const float         pcm_r [],      /* PCM data for right channel    */
            //		const int           nsamples,      /* number of samples per channel */
            //		unsigned char*      mp3buf,        /* pointer to encoded MP3 stream */
            //		const int           mp3buf_size ); /* number of valid octets in this stream */
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_buffer_float(IntPtr context,
                                                                [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]
				float[] pcm_l,
                                                                [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]
				float[] pcm_r,
                                                                int nSamples,
                                                                [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                                int mp3buf_size
            );

            /* as lame_encode_buffer, but for 'float's.
			 * !! NOTE: !! data must be scaled to +/- 1 full scale
			 */
            // int CDECL lame_encode_buffer_ieee_float(
            //		lame_t          gfp,
            //		const float     pcm_l [],          /* PCM data for left channel     */
            //		const float     pcm_r [],          /* PCM data for right channel    */
            //		const int       nsamples,
            //		unsigned char * mp3buf,
            //		const int       mp3buf_size);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_buffer_ieee_float(IntPtr context,
                                                                     [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]
				float[] pcm_l,
                                                                     [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]
				float[] pcm_r,
                                                                     int nSamples,
                                                                     [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                                     int mp3buf_size
            );

            // int CDECL lame_encode_buffer_interleaved_ieee_float(
            //		lame_t          gfp,
            //		const float     pcm[],             /* PCM data for left and right channel, interleaved */
            //		const int       nsamples,
            //		unsigned char * mp3buf,
            //		const int       mp3buf_size);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_buffer_interleaved_ieee_float(IntPtr context,
                                                                                 [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]
			
				float[] pcm,
                                                                                 int nSamples,
                                                                                 [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                                                 int mp3buf_size
            );

            /* as lame_encode_buffer, but for 'double's.
			 * !! NOTE: !! data must be scaled to +/- 1 full scale
			 */
            // int CDECL lame_encode_buffer_ieee_double(
            //		lame_t          gfp,
            //		const double    pcm_l [],          /* PCM data for left channel     */
            //		const double    pcm_r [],          /* PCM data for right channel    */
            //		const int       nsamples,
            //		unsigned char * mp3buf,
            //		const int       mp3buf_size);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_buffer_ieee_double(IntPtr context,
                                                                      [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R8)]			
				double[] pcm_l,
                                                                      [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R8)]
				double[] pcm_r,
                                                                      int nSamples,
                                                                      [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                                      int mp3buf_size
            );

            // int CDECL lame_encode_buffer_interleaved_ieee_double(
            //		lame_t          gfp,
            //		const double    pcm[],             /* PCM data for left and right channel, interleaved */
            //		const int       nsamples,
            //		unsigned char * mp3buf,
            //		const int       mp3buf_size);
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_buffer_interleaved_ieee_double(IntPtr context,
                                                                                  [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R8)]
				double[] pcm,
                                                                                  int nSamples,
                                                                                  [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                                                  int mp3buf_size
            );

            /* as lame_encode_buffer, but for long's
			 * !! NOTE: !! data must still be scaled to be in the same range as
			 * short int, +/- 32768
			 *
			 * This scaling was a mistake (doesn't allow one to exploit full
			 * precision of type 'long'.  Use lame_encode_buffer_long2() instead.
			 *
			 */
            // int CDECL lame_encode_buffer_long(
            //		lame_global_flags*  gfp,			/* global context handle         */
            //		const long     buffer_l [],			/* PCM data for left channel     */
            //		const long     buffer_r [],			/* PCM data for right channel    */
            //		const int           nsamples,		/* number of samples per channel */
            //		unsigned char*      mp3buf,			/* pointer to encoded MP3 stream */
            //		const int           mp3buf_size );	/* number of valid octets in this stream */
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_buffer_long(IntPtr context,
                                                               [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I8)]
				long[] buffer_l,
                                                               [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I8)]
				long[] buffer_r,
                                                               int nSamples,
                                                               [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                               int mp3buf_size
            );

            /* Same as lame_encode_buffer_long(), but with correct scaling.
			 * !! NOTE: !! data must still be scaled to be in the same range as
			 * type 'long'.   Data should be in the range:  +/- 2^(8*size(long)-1)
			 *
			 */
            // int CDECL lame_encode_buffer_long2(
            //		lame_global_flags*  gfp,           /* global context handle         */
            //		const long     buffer_l [],       /* PCM data for left channel     */
            //		const long     buffer_r [],       /* PCM data for right channel    */
            //		const int           nsamples,      /* number of samples per channel */
            //		unsigned char*      mp3buf,        /* pointer to encoded MP3 stream */
            //		const int           mp3buf_size ); /* number of valid octets in this stream */
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_buffer_long2(IntPtr context,
                                                                [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I8)]
				long[] buffer_l,
                                                                [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I8)]
				long[] buffer_r,
                                                                int nSamples,
                                                                [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                                int mp3buf_size
            );

            /* as lame_encode_buffer, but for int's
			 * !! NOTE: !! input should be scaled to the maximum range of 'int'
			 * If int is 4 bytes, then the values should range from
			 * +/- 2147483648.
			 *
			 * This routine does not (and cannot, without loosing precision) use
			 * the same scaling as the rest of the lame_encode_buffer() routines.
			 *
			 */
            // int CDECL lame_encode_buffer_int(
            //		lame_global_flags*  gfp,           /* global context handle         */
            //		const int      buffer_l [],       /* PCM data for left channel     */
            //		const int      buffer_r [],       /* PCM data for right channel    */
            //		const int           nsamples,      /* number of samples per channel */
            //		unsigned char*      mp3buf,        /* pointer to encoded MP3 stream */
            //		const int           mp3buf_size ); /* number of valid octets in this stream */
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_buffer_int(IntPtr context,
                                                              [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)]
				int[] buffer_l,
                                                              [In]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)]
				int[] buffer_r,
                                                              int nSamples,
                                                              [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                              int mp3buf_size
            );

            /*
			 * REQUIRED:
			 * lame_encode_flush will flush the intenal PCM buffers, padding with
			 * 0's to make sure the final frame is complete, and then flush
			 * the internal MP3 buffers, and thus may return a
			 * final few mp3 frames.  'mp3buf' should be at least 7200 bytes long
			 * to hold all possible emitted data.
			 *
			 * will also write id3v1 tags (if any) into the bitstream
			 *
			 * return code = number of bytes output to mp3buf. Can be 0
			 */
            // int CDECL lame_encode_flush(
            //		lame_global_flags *  gfp,    /* global context handle                 */
            //		unsigned char*       mp3buf, /* pointer to encoded MP3 stream         */
            //		int                  size);  /* number of valid octets in this stream */
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_flush(IntPtr context,
                                                         [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                         int mp3buf_size
            );

            /*
			 * OPTIONAL:
			 * lame_encode_flush_nogap will flush the internal mp3 buffers and pad
			 * the last frame with ancillary data so it is a complete mp3 frame.
			 *
			 * 'mp3buf' should be at least 7200 bytes long
			 * to hold all possible emitted data.
			 *
			 * After a call to this routine, the outputed mp3 data is complete, but
			 * you may continue to encode new PCM samples and write future mp3 data
			 * to a different file.  The two mp3 files will play back with no gaps
			 * if they are concatenated together.
			 *
			 * This routine will NOT write id3v1 tags into the bitstream.
			 *
			 * return code = number of bytes output to mp3buf. Can be 0
			 */
            // int CDECL lame_encode_flush_nogap(
            //		lame_global_flags *  gfp,    /* global context handle                 */
            //		unsigned char*       mp3buf, /* pointer to encoded MP3 stream         */
            //		int                  size);  /* number of valid octets in this stream */
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_encode_flush_nogap(IntPtr context,
                                                               [In, Out]//[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1)]
				byte[] mp3buf,
                                                               int mp3buf_size
            );


            /*
			 * OPTIONAL:
			 * Normally, this is called by lame_init_params().  It writes id3v2 and
			 * Xing headers into the front of the bitstream, and sets frame counters
			 * and bitrate histogram data to 0.  You can also call this after
			 * lame_encode_flush_nogap().
			 */
            //int CDECL lame_init_bitstream(
            //		lame_global_flags *  gfp);    /* global context handle                 */
            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_init_bitstream(IntPtr context);

            #endregion

            #region Reporting callbacks

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi)]
            internal delegate void delReportFunction(string fmt, IntPtr args);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_errorf(IntPtr context, delReportFunction fn);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_debugf(IntPtr context, delReportFunction fn);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int lame_set_msgf(IntPtr context, delReportFunction fn);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void lame_print_config(IntPtr context);

            [DllImport(libname, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void lame_print_internals(IntPtr context);


            #endregion


            #region 'printf' support for reporting functions

            //[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ThrowOnUnmappableChar = true, BestFitMapping = false)]
            //internal static extern int _vsnprintf_s(
            //    [In, Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder str,
            //    int sizeOfBuffer,
            //    int count,
            //    [In, MarshalAs(UnmanagedType.LPStr)] String format,
            //    [In] IntPtr va_args);

            //internal static string printf(string format, IntPtr va_args)
            //{
            //    StringBuilder sb = new StringBuilder(4096);
            //    int res = NativeMethods._vsnprintf_s(sb, sb.Capacity, sb.Capacity - 2, format.Replace("\t", "\xFF"), va_args);
            //    return sb.ToString().Replace("\xFF", "\t");
            //}

            #endregion
        }

        ///// <summary>Print out LAME configuration to standard output, or to registered output function</summary>
        //public void PrintConfig()
        //{
        //    NativeMethods.lame_print_config(context);
        //}

        ///// <summary>Print out LAME internals to standard output, or to registered output function</summary>
        //public void PrintInternals()
        //{
        //    NativeMethods.lame_print_internals(context);
        //}

        #region Reporting function support

  //      /// <summary>Delegate for receiving output messages</summary>
  //      /// <param name="text">Text to output</param>
		//public delegate void ReportFunction(string text);

  //      private static ReportFunction rptError = null;
  //      private static ReportFunction rptDebug = null;
  //      private static ReportFunction rptMsg = null;

  //      [MonoPInvokeCallback(typeof(ReportFunction))]
  //      private static void error_proxy(string format, IntPtr va_args)
  //      {
  //          string text = NativeMethods.printf(format, va_args);
  //          if (rptError != null)
  //              rptError(text);
  //      }

  //      [MonoPInvokeCallback(typeof(ReportFunction))]
  //      private static void debug_proxy(string format, IntPtr va_args)
  //      {
  //          string text = NativeMethods.printf(format, va_args);
  //          if (rptDebug != null)
  //              rptDebug(text);
  //      }

  //      [MonoPInvokeCallback(typeof(ReportFunction))]
  //      private static void msg_proxy(string format, IntPtr va_args)
  //      {
  //          string text = NativeMethods.printf(format, va_args);
  //          if (rptMsg != null)
  //              rptMsg(text);
  //      }

  //      private void InitReportFunctions()
  //      {
  //          NativeMethods.lame_set_errorf(context, error_proxy);
  //          NativeMethods.lame_set_debugf(context, debug_proxy);
  //          NativeMethods.lame_set_msgf(context, msg_proxy);
  //      }

  //      /// <summary>Set reporting function for error output from LAME library</summary>
  //      /// <param name="fn">Reporting function</param>
  //      public static void SetErrorFunc(ReportFunction fn)
  //      {
  //          rptError = fn;
  //      }

  //      /// <summary>Set reporting function for debug output from LAME library</summary>
  //      /// <param name="fn">Reporting function</param>
  //      public static void SetDebugFunc(ReportFunction fn)
  //      {
  //          rptDebug = fn;
  //      }

  //      /// <summary>Set reporting function for message output from LAME library</summary>
  //      /// <param name="fn">Reporting function</param>
  //      public static void SetMsgFunc(ReportFunction fn)
  //      {
  //          rptMsg = fn;
  //      }

        #endregion
    }
}
