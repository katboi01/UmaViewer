/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if (UNITY_EDITOR && !UNITY_EDITOR_LINUX) || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_IOS
#define CRIWAREPLUGIN_SUPPORT_AUXIN
#pragma warning disable 0414
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * <summary>AuxIn for reading external sound</summary>
 * <remarks>
 * <para header='Description'>AuxIn can send the sound data external to ADX2 to the DSP bus of ADX2.<br/>
 * Controls the start of playing back input data, gets status, writes input data etc.<br/></para>
 * </remarks>
 */
public class CriAtomExAuxIn : CriDisposable
{
	/**
	 * <summary>A config structure for creating an AuxIn</summary>
	 * <remarks>
	 * <para header='Description'>A structure for specifying the behavior for creating an AuxIn handle for capturing sound.<br/>
	 * Specify it as an argument at the time of creation.<br/>
	 * <br/>
	 * The created AuxIn handle reserves as many internal resources as necessary,
	 * depending on the settings specified in this structure when the handle is created.<br/>
	 * The size of the work area required for the player varies according to the parameters specified in this structure.</para>
	 * </remarks>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Config
	{
		/**
		 * <summary>Maximum number of output channels</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the number of sound channels to be played by the AtomIn player.<br/>
		 * The created AuxIn can play sound data that is "equal to or less than"
		 * the number of channels specified with max_channels.<br/>
		 * The relationship between the value specified as the maximum number of output channels
		 * and the data that can be played back by the created AuxIn is as follows:<br/>
		 * |Maximum number of output channels (specified value) | Data that can be played by the created AuxIn   |
		 * |-------------------------------|-------------------------------|
		 * |1                               | Mono                          |
		 * |2                               | Mono, stereo                   |
		 * |6                               | Mono, stereo, 5.1ch           |
		 * |8                               | Mono, stereo, 5.1ch, 7.1ch       |
		 * <br/></para>
		 * <para header='Note'>For platforms that use hardware resources when outputting sound, it is
		 * possible to reduce the consumption of hardware resources by reducing the number
		 * of output channels.<br/></para>
		 * <para header='Note'>Data exceeding the specified maximum number of output channels cannot be played. <br/>
		 * For example, if the maximum number of output channels is set to 1, the created AuxIn
		 * cannot play stereo sound.<br/>
		 * (It will not be downmixed to monaural and output.)</para>
		 * </remarks>
		 */
		public int maxChannels;

		/**
		 * <summary>Maximum sampling rate</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the sampling rate of the sound played by the AuxIn.<br/>
		 * The created AuxIn can play sound data that is "equal to or less than"
		 * the sampling rate specified with max_sampling_rate.<br/>
		 * <br/></para>
		 * <para header='Note'>By lowering the maximum sampling rate, it is possible to reduce the size of the work memory
		 * required when creating AuxIn.</para>
		 * <para header='Note'>Data that exceeds the specified maximum sampling rate cannot be played. <br/>
		 * For example, if the maximum sampling rate is set to 24000, the created AuxIn
		 * cannot play sound with 48000Hz.<br/>
		 * (It is not down-sampled and output.)</para>
		 * </remarks>
		 */
		public int maxSamplingRate;

		/**
		 * <summary>Sound renderer type</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the type of the sound renderer to be used by AuxIn.<br/>
		 * When set to CriAtomEx.SoundRendererType.Native,
		 * the sound data is sent to the sound output of each platform.<br/>
		 * When set to CriAtomEx.SoundRendererType.Asr,
		 * the sound data is sent to ASR (Atom Sound Renderer).<br/>
		 * (The destination of ASR is specified separately when ASR is initialized.)</para>
		 * </remarks>
		 */
		public CriAtomEx.SoundRendererType soundRendererType;

		public static Config Default {
			get {
				Config config = new Config();
				config.maxChannels = 2;
				config.maxSamplingRate = 48000;
				config.soundRendererType = CriAtomEx.SoundRendererType.Asr;
				return config;
			}
		}
	}

	#region Error Messages
	private const string errorInvalidHandle = "[CRIWARE] Invalid native handle of CriAtomExAuxIn.";
	#endregion

	#region Internal Members
#if CRIWAREPLUGIN_SUPPORT_AUXIN
	private IntPtr handle = IntPtr.Zero;
	private CriAudioReadStream inputReadStream;
#endif
	#endregion

	/**
	 * <summary>Creates an AuxIn</summary>
	 * <param name='config'>A config structure for creating an AuxIn</param>
	 * <returns>AtomAuxIn handle</returns>
	 * <remarks>
	 * <para header='Description'>Creates an AuxIn for capturing sound.<br/>
	 * AuxIn can send the sound data external to ADX2 to the DSP bus of ADX2.<br/>
	 * <br/>
	 * To start playing a sound, call the CriWare.CriAtomExAuxIn::Start function.<br/>
	 * You pass the sound to be captured to AuxIn through the callback function
	 * specified to CriWare.CriAtomExAuxIn::SetInputReadStream .<br/></para>
	 * <para header='Note'>This function is a return-on-complete function. The time it takes depends on the platform.<br/>
	 * Create/discard microphone at a timing when load fluctuations is accepted
	 * such as when switching scenes.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAuxIn::Dispose'/>
	 */
	public CriAtomExAuxIn(Config? config = null)
	{
#if CRIWAREPLUGIN_SUPPORT_AUXIN
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
		Config internalConfig = (config.HasValue) ? config.Value : Config.Default;
		this.handle = criAtomAuxIn_Create(ref internalConfig, IntPtr.Zero, 0);
#else
		Debug.LogError("[CRIWARE] CriAtomExAuxIn does not support this platform.");
#endif
	}

	~CriAtomExAuxIn(){
		Dispose();
	}

	/**
	 * <summary>Discards an AuxIn</summary>
	 * <remarks>
	 * <para header='Description'>Discard an AuxIn.<br/></para>
	 * <para header='Note'>This function is a return-on-complete function. The time it takes depends on the platform.<br/>
	 * Create/discard microphone at a timing when load fluctuations is accepted
	 * such as when switching scenes.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAuxIn::CriAtomExAuxIn'/>
	 */
	public override void Dispose()
	{
#if CRIWAREPLUGIN_SUPPORT_AUXIN
		if (this.handle != IntPtr.Zero) {
			criAtomAuxIn_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}
		GC.SuppressFinalize(this);
		CriDisposableObjectManager.Unregister(this);
#endif
	}

	/**
	 * <summary>Starts the playback of AuxIn</summary>
	 * <remarks>
	 * <para header='Description'>Starts the playback of AuxIn.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAuxIn::Stop'/>
	 */
	public void Start()
	{
#if CRIWAREPLUGIN_SUPPORT_AUXIN
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		criAtomAuxIn_Start(this.handle);
#endif
	}

	/**
	 * <summary>Stops the playback of AuxIn</summary>
	 * <remarks>
	 * <para header='Description'>Stops the playback of AuxIn.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAuxIn::Start'/>
	 */
	public void Stop()
	{
#if CRIWAREPLUGIN_SUPPORT_AUXIN
		if (this.handle == IntPtr.Zero) {
			return;
		}
		criAtomAuxIn_Stop(this.handle);
#endif
	}

	/**
	 * <summary>Set the format</summary>
	 * <param name='numChannels'>The number of channels</param>
	 * <param name='samplingRate'>Sampling frequency</param>
	 * <remarks>
	 * <para header='Description'>Sets the format of the sound to be played.<br/>
	 * It must be set before calling CriWare.CriAtomExAuxIn::Start .<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAuxIn::GetFormat'/>
	 */
	public void SetFormat(int numChannels, int samplingRate)
	{
#if CRIWAREPLUGIN_SUPPORT_AUXIN
		criAtomAuxIn_SetFormat(this.handle, numChannels, samplingRate);
#endif
	}

	/**
	 * <summary>Gets the format</summary>
	 * <param name='numChannels'>The number of channels</param>
	 * <param name='samplingRate'>Sampling frequency</param>
	 * <remarks>
	 * <para header='Description'>Gets the format information set by CriWare.CriAtomExAuxIn::SetFormat .<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAuxIn::SetFormat'/>
	 */
	public void GetFormat(out int numChannels, out int samplingRate)
	{
#if CRIWAREPLUGIN_SUPPORT_AUXIN
		criAtomAuxIn_GetFormat(this.handle, out numChannels, out samplingRate);
#else
		numChannels = 0;
		samplingRate = 0;
#endif
	}

	/**
	 * <summary>Volume setting</summary>
	 * <param name='volume'>Volume value</param>
	 * <remarks>
	 * <para header='Description'>Sets the volume of the AuxIn sound.<br/>
	 * <br/>
	 * The volume value is a scale factor for the amplitude of the sound data (the unit is not decibel).<br/>
	 * For example, if you specify 1.0f, the original sound is played at its unmodified volume.<br/>
	 * If you specify 0.5f, the sound is played at the volume by halving the amplitude (-6dB)
	 * of the original waveform.<br/>
	 * If you specify 0.0f, the sound is muted (silent).</para>
	 * </remarks>
	 */
	public void SetVolume(float volume)
	{
#if CRIWAREPLUGIN_SUPPORT_AUXIN
		criAtomAuxIn_SetVolume(this.handle, volume);
#endif
	}

	/**
	 * <summary>Sets the frequency adjustment ratio</summary>
	 * <param name='frequencyRatio'>Frequency adjustment ratio (0.25f to 4.0f)</param>
	 * <remarks>
	 * <para header='Description'>Sets the sound frequency adjustment ratio of AuxIn.<br/>
	 * The frequency adjustment ratio is the ratio of the sound data frequency to the playback frequency and is equivalent to the playback speed multiplication factor.<br/>
	 * If the frequency ratio exceeds 1.0f, the sound data is played faster than the original sound,
	 * and if the frequency ratio is less than 1.0f, the sound data is played slower than the original sound.<br/>
	 * <br/>
	 * The frequency ratio also affects the pitch of the sound.<br/>
	 * For example, if the sound is played with the frequency ratio of 1.0f, the sound data is played back at the pitch of the original sound,
	 * but if the frequency ratio is changed to 2.0f, the pitch goes up by one octave.<br/>
	 * (Because the playback speed is doubled.)<br/></para>
	 * <para header='Note'>If the frequency ratio higher than 1.0f is set, the sound data to be played is
	 * consumed faster than usual, so it is necessary to supply the sound data faster.<br/>
	 * When setting the frequency ratio higher than 1.0f, set the maximum sampling rate
	 * specified when creating the AuxIn considering the frequency ratio.<br/>
	 * (You must set maxSamplingRate in CriWare.CriAtomExAuxIn::Config structure
	 * when creating AuxIn to a value calculated with
	 * "sampling rate of original sound x frequency ratio")<br/></para>
	 * </remarks>
	 */
	public void SetFrequencyRatio(float frequencyRatio)
	{
#if CRIWAREPLUGIN_SUPPORT_AUXIN
		criAtomAuxIn_SetFrequencyRatio(this.handle, frequencyRatio);
#endif
	}

	/**
	 * <summary>Sets the Bus Send Level</summary>
	 * <param name='busName'>Bus name</param>
	 * <param name='level'>Level value（0.0f~1.0f）</param>
	 * <remarks>
	 * <para header='Description'>Sets the Bus Send Level of the AuxIn sound.<br/>
	 * The Bus Send Level is a mechanism for specifying how much sound should be sent to which bus.<br/>
	 * <br/>
	 * For the second argument, specify the bus name in the DSP bus setting.<br/>
	 * The third argument specifies the level (volume) when sending.<br/>
	 * <br/>
	 * If the bus specified by the bus name as the second argument does not exist in the DSP bus settings being applied, the setting is treated as invalid.<br/>
	 * The range and handling of the send level values are the same as for volume. Refer to the CriWare.CriAtomExAuxIn::SetVolume function.</para>
	 * </remarks>
	 */
	public void SetBusSendLevel(string busName, float level)
	{
#if CRIWAREPLUGIN_SUPPORT_AUXIN
		criAtomAuxIn_SetBusSendLevelByName(this.handle, busName, level);
#endif
	}

	/**
	 * <summary>Sets the read stream</summary>
	 * <param name='stream'>Read stream</param>
	 * <remarks>
	 * <para header='Description'>Sets the read stream in the input direction of AuxIn.<br/>
	 * The callback function is called from a separate thread on most platforms,
	 * so the callee must be implemented as thread-safe.</para>
	 * </remarks>
	 */
	public void SetInputReadStream(CriAudioReadStream stream)
	{
#if CRIWAREPLUGIN_SUPPORT_AUXIN
		this.inputReadStream = stream;
		criAtomAuxIn_SetInputReadStream(this.handle, stream.callbackFunction, stream.callbackPointer);
#endif
	}

	#region DLL Import
#if CRIWAREPLUGIN_SUPPORT_AUXIN
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomAuxIn_Create([In] ref Config config, IntPtr work, int work_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomAuxIn_Destroy(IntPtr aux_in);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomAuxIn_Start(IntPtr aux_in);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomAuxIn_Stop(IntPtr aux_in);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomAuxIn_SetVolume(IntPtr aux_in, float volume);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomAuxIn_SetFrequencyRatio(IntPtr aux_in, float ratio);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomAuxIn_SetBusSendLevelByName(IntPtr aux_in, string bus_name, float level);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomAuxIn_SetFormat(IntPtr aux_in, int num_channels, int sampling_rate);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomAuxIn_GetFormat(IntPtr aux_in, out int num_channels, out int sampling_rate);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomAuxIn_SetInputReadStream(IntPtr aux_in, IntPtr stream_cbfunc, IntPtr stream_ptr);
	#else
	private static IntPtr criAtomAuxIn_Create([In] ref Config config, IntPtr work, int work_size) { return new IntPtr(1); }
	private static void criAtomAuxIn_Destroy(IntPtr aux_in) { }
	private static void criAtomAuxIn_Start(IntPtr aux_in) { }
	private static void criAtomAuxIn_Stop(IntPtr aux_in) { }
	private static void criAtomAuxIn_SetVolume(IntPtr aux_in, float volume) { }
	private static void criAtomAuxIn_SetFrequencyRatio(IntPtr aux_in, float ratio) { }
	private static void criAtomAuxIn_SetBusSendLevelByName(IntPtr aux_in, string bus_name, float level) { }
	private static void criAtomAuxIn_SetFormat(IntPtr aux_in, int num_channels, int sampling_rate) { }
	private static void criAtomAuxIn_GetFormat(IntPtr aux_in, out int num_channels, out int sampling_rate) { num_channels = sampling_rate = 0; }
	private static void criAtomAuxIn_SetInputReadStream(IntPtr aux_in, IntPtr stream_cbfunc, IntPtr stream_ptr) { }
	#endif
#endif
	#endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
