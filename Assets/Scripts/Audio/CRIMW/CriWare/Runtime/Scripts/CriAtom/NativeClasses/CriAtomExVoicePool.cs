/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if !UNITY_WEBGL
	#define CRIATOMEX_SUPPORT_INSERTION_DSP
	#define CRIATOMEX_SUPPORT_STANDARD_VOICE_POOL
	#define CRIATOMEX_SUPPORT_RAW_PCM_VOICE_POOL
#endif
#if !(UNITY_WEBGL || UNITY_STADIA)
 	#define CRIATOMEX_SUPPORT_WAVE_VOICE_POOL
#endif

using System;
using System.Runtime.InteropServices;

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * <summary>A class for controlling the Voice Pool.</summary>
 * <remarks>
 * <para header='Description'>A class for controlling the Voice Pool.<br/></para>
 * </remarks>
 */
public abstract class CriAtomExVoicePool : CriDisposable
{
	/* @cond DOXYGEN_IGNORE */
	public const int StandardMemoryAsrVoicePoolId       = 0;    /**< Standard memory playback Voice Pool ID by ASR */
	public const int StandardStreamingAsrVoicePoolId    = 1;    /**< Standard streaming playback Voice Pool ID by ASR */
	public const int StandardMemoryNsrVoicePoolId       = 2;    /**< Standard memory playback Voice Pool ID by NSR */
	public const int StandardStreamingNsrVoicePoolId    = 3;    /**< Standard streaming playback Voice Pool ID by NSR */
	/* @endcond */

	/**
	 * <summary>The ID to access the Voice Pool created inside the plug-in</summary>
	 * <seealso cref='CriAtomExVoicePool.GetNumUsedVoices'/>
	 */
	public enum VoicePoolId
	{
		/* 機種共通のボイスプールID */
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_IOS || UNITY_TVOS || UNITY_PS3 || UNITY_PS4 || UNITY_PS5 || UNITY_WINRT || UNITY_XBOXONE || UNITY_GAMECORE_XBOXONE || UNITY_GAMECORE_SCARLETT || UNITY_WEBGL || UNITY_SWITCH || UNITY_STADIA || UNITY_STANDALONE_LINUX
		StandardMemory          = StandardMemoryAsrVoicePoolId,     /**< Model standard memory playback Voice Pool ID */
		StandardStreaming       = StandardStreamingAsrVoicePoolId,  /**< Model standard streaming playback Voice Pool ID */
#elif UNITY_PSP2
		StandardMemory          = StandardMemoryNsrVoicePoolId,     /**< Model standard memory playback Voice Pool ID */
		StandardStreaming       = StandardStreamingNsrVoicePoolId,  /**< Model standard streaming playback Voice Pool ID */
#else
		#error unsupported platform
#endif
		HcaMxMemory             = 4,                                /**< HCA-MX memory playback Voice Pool ID */
		HcaMxStreaming          = 5,                                /**< A Voice Pool ID for HCA-MX streaming playback */

		/* 機種固有のボイスプールID */
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_TVOS || UNITY_PS3 || UNITY_WINRT || UNITY_XBOXONE || UNITY_GAMECORE_XBOXONE || UNITY_GAMECORE_SCARLETT || UNITY_WEBGL || UNITY_SWITCH || UNITY_STADIA || UNITY_STANDALONE_LINUX
#elif UNITY_ANDROID
		LowLatencyMemory        = StandardMemoryNsrVoicePoolId,     /**< [Android] Low latency memory playback Voice Pool ID */
		LowLatencyStreaming     = StandardStreamingNsrVoicePoolId,  /**< [Android] Low latency streaming playback Voice Pool ID */
	/* @cond excludele */
#elif UNITY_PSP2
		Atrac9Memory            = 6,                                /**< [VITA] ATRAC9 memory playback Voice Pool ID */
		Atrac9Streaming         = 7,                                /**< [VITA] ATRAC9 streaming playback Voice Pool ID */
#elif UNITY_PS4 || UNITY_PS5
		Atrac9Memory            = 6,                                /**< [PS4] ATRAC9 memory playback Voice Pool ID */
		Atrac9Streaming         = 7,                                /**< [PS4] ATRAC9 streaming playback Voice Pool ID */
		Audio3dMemory           = 8,                                /**< [PS4] Audio3D memory playback Voice Pool ID */
		Audio3dStreaming        = 9,                                /**< [PS4] Audio3D streaming playback Voice Pool ID */
	/* @endcond */
#else
#error unsupported platform
#endif
	}

	/**
	 * <summary>Pitch shifter DSP operation mode</summary>
	 * <remarks>
	 * <para header='Description'>Specifies the pitch shift processing method (algorithm).</para>
	 * </remarks>
	 * <seealso cref='CriAtomExVoicePool.AttachDspPitchShifter'/>
	 * <seealso cref='CriAtomExPlayer.SetDspParameter'/>
	 */
	public enum PitchShifterMode : int {
		Music       = 0,
		Vocal       = 1,
		SoundEffect = 2,
		Speech      = 3
	};

	/**
	 * <summary>Structure for representing the usage of Voices in the Voice Pool</summary>
	 * <seealso cref='CriAtomExVoicePool.GetNumUsedVoices'/>
	 */
	public struct UsedVoicesInfo
	{
		public int numUsedVoices;   /**< The number of Voices in use */
		public int numPoolVoices;   /**< The number of Voices in the Voice Pool */
	}

	/**
	 * <summary>Gets the usage of Voices in the Voice Pool</summary>
	 * <param name='voicePoolId'>Voice Pool ID</param>
	 * <returns>Voice usage</returns>
	 * <remarks>
	 * <para header='Description'>Gets the Voice usage of the specified Voice Pool.</para>
	 * <para header='Note'>Use this function only for debugging purposes.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExVoicePool::VoicePoolId'/>
	 * <seealso cref='CriAtomExVoicePool::UsedVoicesInfo'/>
	 */
	static public UsedVoicesInfo GetNumUsedVoices(VoicePoolId voicePoolId)
	{
		UsedVoicesInfo info;
		CRIWARE3042ECD3((int)voicePoolId, out info.numUsedVoices, out info.numPoolVoices);
		return info;
	}


	public IntPtr nativeHandle {get {return this._handle;} }
	public uint identifier {get {return this._identifier;} }
	public int numVoices {get {return this._numVoices; } }
	public int maxChannels {get {return this._maxChannels; } }
	public int maxSamplingRate {get {return this._maxSamplingRate; } }

	/**
	 * <summary>Discards the Voice Pool</summary>
	 * <remarks>
	 * <para header='Description'>Discards the Voice Pool object.<br/>
	 * Be sure to discard the created object using this API. Otherwise, a resource leak occurs.</para>
	 * </remarks>
	 */
	public override void Dispose()
	{
		CriDisposableObjectManager.Unregister(this);
		if (this._handle != IntPtr.Zero) {
			CriAtomExVoicePool.criAtomExVoicePool_Free(this._handle);
			this._handle = IntPtr.Zero;
		}
		GC.SuppressFinalize(this);
	}

	/**
	 * <summary>Gets the usage of Voices in the Voice Pool</summary>
	 * <returns>Voice usage</returns>
	 * <remarks>
	 * <para header='Description'>Gets the Voice usage.</para>
	 * <para header='Note'>Use this function only for debugging purposes.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExVoicePool::UsedVoicesInfo'/>
	 */
	public UsedVoicesInfo GetNumUsedVoices()
	{
		UsedVoicesInfo info;
		if (this._handle != IntPtr.Zero) {
			criAtomExVoicePool_GetNumUsedVoices(this._handle, out info.numUsedVoices, out info.numPoolVoices);
		} else {
			info = new UsedVoicesInfo();
		}
		return info;
	}

#if CRIATOMEX_SUPPORT_INSERTION_DSP
	/**
	 * <summary>Attaches the time stretch DSP</summary>
	 * <remarks>
	 * <para header='Description'>Adds a time stretch DSP to the Voice Pool.</para>
	 * <para header='Note'>This function is a return-on-complete function.<br/>
	 * Calling this function blocks the server processing of the Atom library for a while.<br/>
	 * If this function is called during sound playback, problems such as sound interruption may occur,
	 * so call this function at a timing when load fluctuations is accepted such as when switching scenes.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExVoicePool::DetachInsertionDsp'/>
	 */
	public void AttachDspTimeStretch()
	{
		if (this._handle == IntPtr.Zero)
			return;

		ExTimeStretchConfig config;
		config.numDsp = this._numVoices;
		config.maxChannels = this._maxChannels;
		config.maxSamplingRate = this._maxSamplingRate;
		config.config.reserved = 0;
		criAtomExVoicePool_AttachDspTimeStretch(this._handle, ref config, IntPtr.Zero, 0);
	}

	/**
	 * <summary>Attaches the pitch shifter DSP</summary>
	 * <param name='mode'>Pitch shift mode</param>
	 * <param name='windosSize'>Window size</param>
	 * <param name='overlapTimes'>The number of overlaps</param>
	 * <remarks>
	 * <para header='Description'>Adds a pitch shifter DSP to the Voice Pool.</para>
	 * <para header='Note'>This function is a return-on-complete function.<br/>
	 * Calling this function blocks the server processing of the Atom library for a while.<br/>
	 * If this function is called during sound playback, problems such as sound interruption may occur,
	 * so call this function at a timing when load fluctuations is accepted such as when switching scenes.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExVoicePool::DetachInsertionDsp'/>
	 */
	public void AttachDspPitchShifter(PitchShifterMode mode = PitchShifterMode.Music, int windosSize = 1024, int overlapTimes = 4)
	{
		if (this._handle == IntPtr.Zero)
			return;

		ExPitchShifterConfig config;
		config.numDsp = this._numVoices;
		config.maxChannels = this._maxChannels;
		config.maxSamplingRate = this._maxSamplingRate;
		config.config.mode = (int)mode;
		config.config.windowSize = windosSize;
		config.config.overlapTimes = overlapTimes;
		criAtomExVoicePool_AttachDspPitchShifter(this._handle, ref config, IntPtr.Zero, 0);
	}

	/**
	 * <summary>Detaches DSP</summary>
	 * <remarks>
	 * <para header='Description'>Removes the DSP added to the Voice Pool.</para>
	 * <para header='Note'>This function is a return-on-complete function.<br/>
	 * Calling this function blocks the server processing of the Atom library for a while.<br/>
	 * If this function is called during sound playback, problems such as sound interruption may occur,
	 * so call this function at a timing when load fluctuations is accepted such as when switching scenes.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExVoicePool::AttachDspPitchShifter'/>
	 * <seealso cref='CriAtomExVoicePool::AttachDspTimeStretch'/>
	 */
	public void DetachDsp()
	{
		if (this._handle == IntPtr.Zero)
			return;

		criAtomExVoicePool_DetachDsp(this._handle);
	}
#endif

	#region Internal Members

	~CriAtomExVoicePool()
	{
		Dispose();
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	protected struct PlayerConfig
	{
		public int maxChannels;
		public int maxSamplingRate;
		public bool streamingFlag;
		public int soundRendererType;
		public int decodeLatency;
		private IntPtr context;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	protected struct VoicePoolConfig
	{
		public uint identifier;
		public int numVoices;
		public PlayerConfig playerConfig;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct PitchShifterConfig {
		public int mode;
		public int windowSize;
		public int overlapTimes;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct ExPitchShifterConfig {
		public int numDsp;
		public int maxChannels;
		public int maxSamplingRate;
		public PitchShifterConfig config;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct TimeStretchConfig {
		public int reserved;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct ExTimeStretchConfig {
		public int numDsp;
		public int maxChannels;
		public int maxSamplingRate;
		public TimeStretchConfig config;
	}

	protected IntPtr _handle = IntPtr.Zero;
	protected uint _identifier = 0;
	protected int _numVoices = 0;
	protected int _maxChannels = 0;
	protected int _maxSamplingRate = 0;

	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE3042ECD3(int voice_pool_id, out int num_used_voices, out int num_pool_voices);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExVoicePool_GetNumUsedVoices(IntPtr pool, out int num_used_voices, out int num_pool_voices);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void criAtomExVoicePool_Free(IntPtr pool);

	#if CRIATOMEX_SUPPORT_INSERTION_DSP
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExVoicePool_AttachDspTimeStretch(IntPtr pool, ref ExTimeStretchConfig config, IntPtr work, int work_size) ;

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExVoicePool_AttachDspPitchShifter(IntPtr pool, ref ExPitchShifterConfig config, IntPtr work, int work_size) ;

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExVoicePool_DetachDsp(IntPtr pool);
	#endif
	#else
	private static void CRIWARE3042ECD3(int voice_pool_id, out int num_used_voices, out int num_pool_voices)
		{ num_used_voices = num_pool_voices = 0; }
	private static void criAtomExVoicePool_GetNumUsedVoices(IntPtr pool, out int num_used_voices, out int num_pool_voices)
		{ num_used_voices = num_pool_voices = 0; }
	public static void criAtomExVoicePool_Free(IntPtr pool) { }
	#if CRIATOMEX_SUPPORT_INSERTION_DSP
	private static void criAtomExVoicePool_AttachDspTimeStretch(IntPtr pool, ref ExTimeStretchConfig config, IntPtr work, int work_size) { }
	private static void criAtomExVoicePool_AttachDspPitchShifter(IntPtr pool, ref ExPitchShifterConfig config, IntPtr work, int work_size) { }
	private static void criAtomExVoicePool_DetachDsp(IntPtr pool) { }
	#endif
	#endif
	#endregion
}

#if CRIATOMEX_SUPPORT_STANDARD_VOICE_POOL

/**
 * <summary>Standard Voice Pool</summary>
 */
public class CriAtomExStandardVoicePool: CriAtomExVoicePool
{
	/**
	 * <summary>Creates an additional standard Voice Pool</summary>
	 * <param name='numVoices'>The number of Voices</param>
	 * <param name='maxChannels'>Maximum number of channels</param>
	 * <param name='maxSamplingRate'>Maximum sampling rate</param>
	 * <param name='streamingFlag'>Streaming playback flag</param>
	 * <param name='identifier'>Voice Pool identifier</param>
	 * <returns>Standard Voice Pool</returns>
	 * <remarks>
	 * <para header='Description'>Creates an additional standard Voice Pool.<br/>
	 * If you want to play a sound with six or more channels, create a Voice Pool using this API.<br/>
	 * If false is specified for streamingFlag, a voice pool for in-memory playback will be created. <br/>
	 * If you specify true for streamingFlag, the created voice pool will be capable of streaming playback in addition to in-memory playback. <br/>
	 * maxSamplingRate specifies the maximum sampling rate of the materials in the Cue to be played, which uses the voice pool being created. <br/>
	 * If the audio pitch is expected to change, please select a maximum sampling rate based on the highest pitch. <br/>
	 * If you want to use the time-stretch feature, you need to double the maximum sampling rate. <br/></para>
	 * <para header='Note'>Be sure to call the Dispose function to destroy the object after the playback is complete.<br/>
	 * If you want to explicitly set a particular CriAtomExPlayer to get voices from the created voice pool,<br/>
	 * please create the voice pool with a non-zero identifier (0 is the default identifier),<br/>
	 * and call the CriAtomExPlayer::SetVoicePoolIdentifier function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetVoicePoolIdentifier'/>
	 */
	public CriAtomExStandardVoicePool(int numVoices, int maxChannels, int maxSamplingRate, bool streamingFlag, uint identifier = 0)
	{
		this._identifier = identifier;
		this._numVoices = numVoices;
		this._maxChannels = maxChannels;
		this._maxSamplingRate = maxSamplingRate;

		VoicePoolConfig config = new VoicePoolConfig();
		config.identifier = identifier;
		config.numVoices = numVoices;
		config.playerConfig.maxChannels = maxChannels;
		config.playerConfig.maxSamplingRate = maxSamplingRate;
		config.playerConfig.streamingFlag = streamingFlag;
		config.playerConfig.soundRendererType = (int)CriAtomEx.SoundRendererType.Asr;
		config.playerConfig.decodeLatency = 0;
		this._handle = criAtomExVoicePool_AllocateStandardVoicePool(ref config, IntPtr.Zero, 0);
		if (this._handle == IntPtr.Zero) {
			throw new Exception("CriAtomExStandardVoicePool() failed.");
		}

		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	}

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName)]
	private static extern IntPtr criAtomExVoicePool_AllocateStandardVoicePool(ref VoicePoolConfig config, IntPtr work, int work_size);
	#else
	private static IntPtr criAtomExVoicePool_AllocateStandardVoicePool(ref VoicePoolConfig config, IntPtr work, int work_size) { return new IntPtr(1); }
	#endif
	#endregion
}

#endif

#if CRIATOMEX_SUPPORT_WAVE_VOICE_POOL

/**
 * <summary>Wave Voice Pool</summary>
 */
public class CriAtomExWaveVoicePool: CriAtomExVoicePool
{
	/**
	 * <summary>Creates a Wave Voice Pool</summary>
	 * <param name='numVoices'>The number of Voices</param>
	 * <param name='maxChannels'>Maximum number of channels</param>
	 * <param name='maxSamplingRate'>Maximum sampling rate</param>
	 * <param name='streamingFlag'>Streaming playback flag</param>
	 * <param name='identifier'>Voice Pool identifier</param>
	 * <returns>Wave Voice Pool</returns>
	 * <remarks>
	 * <para header='Description'>Calling this function pools the Voices that can be played by Wave.<br/>
	 * When you play Wave data (or a Cue that contains Wave data) in the AtomExPlayer,
	 * the AtomExPlayer gets the Voice from the created Wave Voice Pool and plays it.<br/>
	 * After playing, be sure to discard the object using the Dispose function.<br/>
	 * If you want to explicitly set a specific CriAtomExPlayer to get a Voice from the created Voice Pool,
	 * create a Voice with a specifying non-zero value as identifier, and call the CriAtomExPlayer::SetVoicePoolIdentifier
	 * function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetVoicePoolIdentifier'/>
	 */
	public CriAtomExWaveVoicePool(int numVoices, int maxChannels, int maxSamplingRate, bool streamingFlag, uint identifier = 0)
	{
		this._identifier = identifier;
		this._numVoices = numVoices;
		this._maxChannels = maxChannels;
		this._maxSamplingRate = maxSamplingRate;

		VoicePoolConfig config = new VoicePoolConfig();
		config.identifier = identifier;
		config.numVoices = numVoices;
		config.playerConfig.maxChannels = maxChannels;
		config.playerConfig.maxSamplingRate = maxSamplingRate;
		config.playerConfig.streamingFlag = streamingFlag;
		config.playerConfig.soundRendererType = (int)CriAtomEx.SoundRendererType.Asr;
		config.playerConfig.decodeLatency = 0;
		this._handle = criAtomExVoicePool_AllocateWaveVoicePool(ref config, IntPtr.Zero, 0);
		if (this._handle == IntPtr.Zero) {
			throw new Exception("CriAtomExWaveVoicePool() failed.");
		}

		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	}

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName)]
	private static extern IntPtr criAtomExVoicePool_AllocateWaveVoicePool(ref VoicePoolConfig config, IntPtr work, int work_size);
	#else
	private static IntPtr criAtomExVoicePool_AllocateWaveVoicePool(ref VoicePoolConfig config, IntPtr work, int work_size) { return new IntPtr(1); }
	#endif
	#endregion
}

#endif

#if  CRIATOMEX_SUPPORT_RAW_PCM_VOICE_POOL

/**
 * <summary>RawPCM Voice Pool</summary>
 */
public class CriAtomExRawPcmVoicePool: CriAtomExVoicePool
{
	/**
	 * <summary>RawPCM format</summary>
	 * <remarks>
	 * <para header='Description'>The data format to be played back using the RawPCM Voice Pool.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExVoicePool.CriAtomExRawPcmVoicePool'/>
	 */
	public enum RawPcmFormat {
		Sint16  = 0,
		Float32 = 1
	}

	/**
	 * <summary>Creates an RawPCM Voice Pool</summary>
	 * <param name='numVoices'>The number of Voices</param>
	 * <param name='maxChannels'>Maximum number of channels</param>
	 * <param name='maxSamplingRate'>Maximum sampling rate</param>
	 * <param name='format'>RawPCM format</param>
	 * <param name='identifier'>Voice Pool identifier</param>
	 * <returns>RawPCM Voice Pool</returns>
	 * <remarks>
	 * <para header='Description'>Calling this function pools the Voices that can be played by RawPCM.<br/>
	 * When you play RawPCM data (or a Cue that contains RawPCM data) in the AtomExPlayer,
	 * the AtomExPlayer gets the Voice from the created RawPCM Voice Pool and plays it.<br/>
	 * After playing, be sure to discard the object using the Dispose function.<br/>
	 * If you want to explicitly set a specific CriAtomExPlayer to get a Voice from the created Voice Pool,
	 * create a Voice with a specifying non-zero value as identifier, and call the CriAtomExPlayer::SetVoicePoolIdentifier
	 * function.</para>
	 * <para header='Note'>Currently, it is not possible to change the format of a RawPCM Voice Pool once created.<br/>
	 * Make sure to create it by specifying the format that matches the data to be played.<br/>
	 * Even if the number of channels or and sampling rate is specified for CriWare.CriAtomExPlayer ,
	 * playback is done based on the format set to the Voice Pool.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetVoicePoolIdentifier'/>
	 */
	public CriAtomExRawPcmVoicePool(int numVoices, int maxChannels, int maxSamplingRate, RawPcmFormat format, uint identifier = 0)
	{
		this._identifier = identifier;
		this._numVoices = numVoices;
		this._maxChannels = maxChannels;
		this._maxSamplingRate = maxSamplingRate;

		RawPcmVoicePoolConfig config = new RawPcmVoicePoolConfig();
		config.identifier = identifier;
		config.numVoices = numVoices;
		config.playerConfig.maxChannels = maxChannels;
		config.playerConfig.maxSamplingRate = maxSamplingRate;
		config.playerConfig.format = format;
		config.playerConfig.soundRendererType = (int)CriAtomEx.SoundRendererType.Asr;
		config.playerConfig.decodeLatency = 0;
		this._handle = criAtomExVoicePool_AllocateRawPcmVoicePool(ref config, IntPtr.Zero, 0);
		if (this._handle == IntPtr.Zero) {
			throw new Exception("CriAtomExRawPcmVoicePool() failed.");
		}

		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	protected struct RawPcmPlayerConfig {
		public RawPcmFormat format;
		public int maxChannels;
		public int maxSamplingRate;
		public int soundRendererType;
		public int decodeLatency;
		private IntPtr context;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	protected struct RawPcmVoicePoolConfig {
		public uint identifier;
		public int numVoices;
		public RawPcmPlayerConfig playerConfig;
	}

	#region DLL Import
#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName)]
	private static extern IntPtr criAtomExVoicePool_AllocateRawPcmVoicePool(ref RawPcmVoicePoolConfig config, IntPtr work, int work_size);
#else
	private static IntPtr criAtomExVoicePool_AllocateRawPcmVoicePool(ref RawPcmVoicePoolConfig config, IntPtr work, int work_size) { return new IntPtr(1); }
#endif
	#endregion
}

#endif

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
