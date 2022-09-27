/****************************************************************************
 *
 * Copyright (c) 2016 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/*---------------------------
 * Asr Support Defines
 *---------------------------*/
#if !UNITY_PSP2
#define CRIWARE_SUPPORT_ASR
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
 * <summary>ASR Rack</summary>
 */
public partial class CriAtomExAsrRack : CriDisposable
{
	#region Data Types
	/**
	 * <summary>A config structure for creating an ASR Rack</summary>
	 * <remarks>
	 * <para header='Description'>A structure for specifying the behavior of CriAtomExAsrRack.<br/>
	 * You pass this structure as an argument when creating a module (the CriWare.CriAtomExAsrRack::CriAtomExAsrRack function).<br/></para>
	 * <para header='Note'>Please change the default configuration obtained from CriWare.CriAtomExAsrRack::defaultConfig when needed.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::CriAtomExAsrRack'/>
	 * <seealso cref='CriAtomExAsrRack::defaultConfig'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Config
	{
		/**
		 * <summary>Frequency of server process</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the frequency of running the server process</para>
		 * <para header='Note'>Set the same value as the one set to CriAtomConfig::serverFrequency for CriWareInitializer.</para>
		 * </remarks>
		 */
		public float serverFrequency;

		/**
		 * <summary>The number of buses</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the number of buses that ASR creates.<br/>
		 * The bus is responsible for mixing sounds and managing the DSPeffects.</para>
		 * </remarks>
		 */
		public int numBuses;

		/**
		 * <summary>The number of output channels</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the number of output channels of the ASR Rack.<br/>
		 * Specify 6ch or more channels when using pan 3D or 3D Positioning features.</para>
		 * </remarks>
		 */
		public int outputChannels;

		/**
		 * <summary>Output sampling rate</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the output and processing sampling rate of the ASR Rack.<br/>
		 * Normally, specify the sampling rate of the sound device on the target machine.</para>
		 * <para header='Note'>Lowering it lowers the processing load but lowers the sound quality.</para>
		 * </remarks>
		 */
		public int outputSamplingRate;

		/**
		 * <summary>Sound renderer type</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the type of the output destination sound renderer of the ASR Rack.<br/>
		 * If you specify CriAtomEx.SoundRendererType.Native for soundRendererType,
		 * the sound data is transferred to each platform's default output.</para>
		 * </remarks>
		 */
		public CriAtomEx.SoundRendererType soundRendererType;

		/**
		 * <summary>Destination ASR Rack ID</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the ASR Rack ID of the ASR Rack's output destination.<br/>
		 * Valid only when you specify CriAtomEx.SoundRendererType.Asr for soundRendererType.</para>
		 * </remarks>
		 */
		public int outputRackId;

		/**
		 * <summary>Pointer to platform-specific parameters</summary>
		 * <remarks>
		 * <para header='Description'>Specifies a pointer to platform-specific parameters.<br/>
		 * When using it as the argument to the CriAtomExAsrRack::CriAtomExAsrRack function,
		 * specify IntPtr.Zero because it is overwritten by the second argument PlatformContext.</para>
		 * </remarks>
		 */
		public IntPtr context;
	}

	/**
	 * <summary>A platform-specific config structure for creating an ASR Rack</summary>
	 * <remarks>
	 * <para header='Description'>A structure for specifying the behavior of CriAtomExAsrRack.<br/>
	 * You pass this structure as an argument when creating a module (the CriWare.CriAtomExAsrRack::CriAtomExAsrRack function).<br/>
	 * For details, refer to the platform-specific manual.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::CriAtomExAsrRack'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct PlatformConfig
	{
	#if !UNITY_EDITOR && UNITY_PS4
		public int userId;
		public CriWarePS4.AudioPortType portType;
		public CriWarePS4.AudioPortAttribute portAttr;
	#elif !UNITY_EDITOR && UNITY_SWITCH
		public UInt32 npadId;
	#else
		public byte reserved;
	#endif
	}

	/**
	 * <summary>Performance information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting the performance information.<br/>
	 * Used in the CriWare.CriAtomExAsrRack::GetPerformanceInfo and CriWare.CriAtomExAsrRack::GetPerformanceInfoByRackId methods.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::GetPerformanceInfo'/>
	 * <seealso cref='CriAtomExAsrRack::GetPerformanceInfoByRackId'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct PerformanceInfo
	{
		public UInt32 processCount;            /**< Number of signal generation processes */
		public UInt32 lastProcessTime;         /**< Latest processing time (in microseconds) */
		public UInt32 maxProcessTime;          /**< Maximum processing time (in microseconds) */
		public UInt32 averageProcessTime;      /**< Average processing time (in microseconds) */
		public UInt32 lastProcessInterval;     /**< Latest processing interval (in microseconds) */
		public UInt32 maxProcessInterval;      /**< Maximum processing interval (in microseconds) */
		public UInt32 averageProcessInterval;  /**< Average processing interval (in microseconds) */
		public UInt32 lastProcessSamples;      /**< Latest number of samples generated in a single processing */
		public UInt32 maxProcessSamples;       /**< Maximum number of samples generated in a single processing */
		public UInt32 averageProcessSamples;   /**< Average number of samples generated in a single processing */
	}
	#endregion

	/**
	 * <summary>Creating an ASR Rack</summary>
	 * <param name='config'>Config structure</param>
	 * <param name='platformConfig'>Platform-specific parameter structure</param>
	 * <returns>ASR Rack</returns>
	 * <remarks>
	 * <para header='Description'>Creates an ASR Rack.<br/>
	 * Be sure to discard the ASR Rack created by this function using the Dispose function.</para>
	 * </remarks>
	 */
	public CriAtomExAsrRack(Config config, PlatformConfig platformConfig)
	{
	#if CRIWARE_SUPPORT_ASR
		this._rackId = CRIWAREB4A7C956(ref config, ref platformConfig);
		if (config.context != IntPtr.Zero) {
			Marshal.FreeHGlobal(config.context);
		}
		if (this._rackId == -1) {
			throw new Exception("CriAtomExAsrRack() failed.");
		}

		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	#else
		this._rackId = -1;
	#endif
	}
	
	/**
	 * <summary>Attaching the DSP bus settings</summary>
	 * <param name='settingName'>Name of the DSP bus setting</param>
	 * <remarks>
	 * <para header='Description'>Build DSP buses from the DSP bus settings and attach them to the ASR Rack. <br/>
	 * To execute this function, it is necessary to register the ACF information with
	 * the CriAtomEx::RegisterAcf function in advance. <br/></para>
	 * <para header='Note'>This function is a return-on-complete function.<br/>
	 * Calling this function blocks the server processing of the Atom library for a while.<br/>
	 * If this function is called during sound playback, problems such as sound interruption may occur,
	 * so call this function at a timing when load fluctuations is accepted such as when switching scenes.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::DetachDspBusSetting'/>
	 * <seealso cref='CriAtomEx::RegisterAcf'/>
	 */
	public void AttachDspBusSetting(string settingName)
	{
		criAtomExAsrRack_AttachDspBusSetting(this.rackId, settingName, IntPtr.Zero, 0);
	}

	/**
	 * <summary>Detaches the DSP bus settings</summary>
	 * <remarks>
	 * <para header='Description'>Detaches the DSP bus settings.<br/></para>
	 * <para header='Note'>This function is a return-on-complete function.<br/>
	 * Calling this function blocks the server processing of the Atom library for a while.<br/>
	 * If this function is called during sound playback, problems such as sound interruption may occur,
	 * so call this function at a timing when load fluctuations is accepted such as when switching scenes.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::AttachDspBusSetting'/>
	 */
	public void DetachDspBusSetting()
	{
		criAtomExAsrRack_DetachDspBusSetting(this.rackId);
	}

	/**
	 * <summary>Applies the DSP bus snapshot</summary>
	 * <param name='snapshotName'>DSP bus snapshot name</param>
	 * <param name='timeMs'>The time (in milliseconds) until the snapshot is fully reflected</param>
	 * <remarks>
	 * <para header='Description'>Applies the DSP bus snapshot to the ASR Rack. <br/>
	 * When this function is called, the parameter values progressively change to 
	 * the values saved in the snapshot.
	 * It takes timeMs milliseconds to complete the change.</para>
	 * </remarks>
	 */
	public void ApplyDspBusSnapshot(string snapshotName, int timeMs)
	{
		criAtomExAsrRack_ApplyDspBusSnapshot(this.rackId, snapshotName, timeMs);
	}

	/**
	 * <summary>Gets the snapshot's name</summary>
	 * <param name='rackId'>Rack ID</param>
	 * <returns>Snapshot name</returns>
	 * <remarks>
	 * <para header='Description'>Gets the current snapshot name. Returns null if none was set.<br/></para>
	 * </remarks>
	 */
	public static string GetAppliedDspBusSnapshotName(int rackId)
	{
		string snapshotName;
		IntPtr ptr = criAtomExAsrRack_GetAppliedDspBusSnapshotName(rackId);
		if (ptr == IntPtr.Zero) {
			return null;
		}
		snapshotName = Marshal.PtrToStringAnsi(ptr);
		return snapshotName;
	}

	/**
	 * <summary>Gets the snapshot's name</summary>
	 * <returns>Snapshot name</returns>
	 * <remarks>
	 * <para header='Description'>Gets the current snapshot name. Returns null if none was set.<br/></para>
	 * </remarks>
	 */
	public string GetAppliedDspBusSnapshotName()
	{
		string snapshotName;
		IntPtr ptr = criAtomExAsrRack_GetAppliedDspBusSnapshotName(this.rackId);
		if (ptr == IntPtr.Zero) {
			return null;
		}
		snapshotName = Marshal.PtrToStringAnsi(ptr);
		return snapshotName;
	}

	/**
	 * <summary>Gets the performance information for the ASR rack</summary>
	 * <returns>ASR Rack performance information</returns>
	 * <remarks>
	 * <para header='Description'>Gets performance information from the current ASR Rack instance.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::ResetPerformanceMonitor'/>
	 * <seealso cref='CriAtomExAsrRack::GetPerformanceInfoByRackId'/>
	 */
	public PerformanceInfo GetPerformanceInfo()
	{
		PerformanceInfo info = new PerformanceInfo();
		if(this._rackId < 0) {
			UnityEngine.Debug.LogError("[CRIWARE] This ASR Rack is not initialized.");
			return info;
		}

		criAtomExAsrRack_GetPerformanceInfo(this._rackId, out info);
		return info;
	}

	/**
	 * <summary>Gets the performance information for the ASR rack</summary>
	 * <param name='rackId'>Rack ID</param>
	 * <returns>ASR Rack performance information</returns>
	 * <remarks>
	 * <para header='Description'>Gets performance information for the ASR Rack with the specified ID.<br/>
	 * If no Rack ID is specified, performance information for the default ASR Rack (created
	 * during the initialization of the library) will be returned.<br/>
	 * If an invalid Rack ID is specified, all the members of the structure returned will be set to 0.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::ResetPerformanceMonitorByRackId'/>
	 * <seealso cref='CriAtomExAsrRack::GetPerformanceInfo'/>
	 */
	public static PerformanceInfo GetPerformanceInfoByRackId(int rackId = CriAtomExAsrRack.defaultRackId)
	{
		PerformanceInfo info = new PerformanceInfo();
		criAtomExAsrRack_GetPerformanceInfo(rackId, out info);
		return info;
	}

	/**
	 * <summary>Resets the performance measurements for the ASR rack</summary>
	 * <remarks>
	 * <para header='Description'>Resets the performance measurement of the current ASR Rack instance.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::GetPerformanceInfo'/>
	 * <seealso cref='CriAtomExAsrRack::ResetPerformanceMonitorByRackId'/>
	 */
	public void ResetPerformanceMonitor()
	{
		criAtomExAsrRack_ResetPerformanceMonitor(this._rackId);
	}

	/**
	 * <summary>Resets the performance measurements for the ASR rack</summary>
	 * <param name='rackId'>Rack ID</param>
	 * <remarks>
	 * <para header='Description'>Resets the performance measurement of the ASR Rack with the specified ID. <br/>
	 * If no Rack ID is specified, the performance information of the default ASR Rack
	 * (created during the initialization of the library) will be reset. <br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::GetPerformanceInfoByRackId'/>
	 * <seealso cref='CriAtomExAsrRack::ResetPerformanceMonitor'/>
	 */
	public static void ResetPerformanceMonitorByRackId(int rackId = CriAtomExAsrRack.defaultRackId)
	{
		criAtomExAsrRack_ResetPerformanceMonitor(rackId);
	}

	/**
	 * <summary>Setting the AISAC control value by name</summary>
	 * <param name='rackId'>Rack ID</param>
	 * <param name='controlName'>AISAC control name</param>
	 * <param name='value'>AISAC control value</param>
	 * <remarks>
	 * <para header='Description'>Sets the AISAC control value by name.</para>
	 * </remarks>
	 */
	public static void SetAisacControl(int rackId, string controlName, float value)
	{
		criAtomExAsrRack_SetAisacControlByName(rackId, controlName, value);
	}

	/**
	 * <summary>Setting AISAC control value by specifying an ID</summary>
	 * <param name='rackId'>Rack ID</param>
	 * <param name='controlId'>AISAC control ID</param>
	 * <param name='value'>AISAC control value</param>
	 * <remarks>
	 * <para header='Description'>Sets the AISAC control value by specifying an ID.</para>
	 * </remarks>
	 */
	public static  void SetAisacControl(int rackId, int controlId, float value)
	{
		criAtomExAsrRack_SetAisacControlById(rackId, (ushort)controlId, value);
	}

	/**
	 * <summary>Discards an ASR Rack</summary>
	 * <remarks>
	 * <para header='Description'>Discards an ASR Rack.</para>
	 * </remarks>
	 */
	public override void Dispose()
	{
	#if CRIWARE_SUPPORT_ASR
		CriDisposableObjectManager.Unregister(this);

		if (this._rackId != -1) {
			criAtomExAsrRack_Destroy(this._rackId);
			this._rackId = -1;
		}
	#endif
		GC.SuppressFinalize(this);
	}

	public int rackId {
		get { return this._rackId; }
	}

	#region Static Properties
	/**
	 * <summary>Default configuration</summary>
	 * <remarks>
	 * <para header='Description'>Default config.</para>
	 * <para header='Note'>Change the default configuration obtained using this property</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::CriAtomExAsrRack'/>
	 */
	public static Config defaultConfig {
		get {
			Config config;
			config.serverFrequency = 60.0f;
			config.numBuses = 8;
			config.soundRendererType = CriAtomEx.SoundRendererType.Native;
			config.outputRackId = 0;
			config.context = System.IntPtr.Zero;
	#if !UNITY_EDITOR && UNITY_PS4
			config.outputChannels = 8;
			config.outputSamplingRate = 48000;
	#elif !UNITY_EDITOR && UNITY_IOS || UNITY_ANDROID
			config.outputChannels = 2;
			config.outputSamplingRate = 44100;
	#elif !UNITY_EDITOR && UNITY_PSP2
			config.outputChannels = 2;
			config.outputSamplingRate = 48000;
	#else
			config.outputChannels = 6;
			config.outputSamplingRate = 48000;
	#endif
			return config;
		}
	}

	/**
	 * <summary>Default ASR Rack ID</summary>
	 * <remarks>
	 * <para header='Description'>Default ASR Rack ID.
	 * When returning to normal output or discarding the generated ASR Rack, use this constant
	 * for each player to specify the ASR Rack ID.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetAsrRackId'/>
	 * <seealso cref='CriMana::Player::SetAsrRackId'/>
	 */
	public const int defaultRackId = 0;

	#endregion


	#region internal members
	~CriAtomExAsrRack()
	{
		this.Dispose();
	}

	private int _rackId = -1;
	#endregion

	#region DLL Import
	#if CRIWARE_SUPPORT_ASR

	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int CRIWAREB4A7C956([In] ref Config config, [In] ref PlatformConfig platformConfig);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_Destroy(Int32 rackId);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_AttachDspBusSetting(Int32 rackId, string setting, IntPtr work, Int32 workSize);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_DetachDspBusSetting(Int32 rackId);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomExAsrRack_GetAppliedDspBusSnapshotName(int rackId);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_ApplyDspBusSnapshot(Int32 rackId, string snapshotName, Int32 timeMs);
	#else
	private static int CRIWAREB4A7C956([In] ref Config config, [In] ref PlatformConfig platformConfig) { return 0; }
	private static void criAtomExAsrRack_Destroy(Int32 rackId) { }
	private static void criAtomExAsrRack_AttachDspBusSetting(Int32 rackId, string setting, IntPtr work, Int32 workSize) { }
	private static void criAtomExAsrRack_DetachDspBusSetting(Int32 rackId) { }
	private static void criAtomExAsrRack_ApplyDspBusSnapshot(Int32 rackId, string snapshotName, Int32 timeMs) { }
	private static IntPtr criAtomExAsrRack_GetAppliedDspBusSnapshotName(int rackId) { return IntPtr.Zero; }
	#endif

	#if !CRIWARE_ENABLE_HEADLESS_MODE && !UNITY_WEBGL
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_GetPerformanceInfo(Int32 rackId, out PerformanceInfo perfInfo);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_ResetPerformanceMonitor(Int32 rackId);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_SetAisacControlById(Int32 rackId, ushort controlId, float value);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_SetAisacControlByName(Int32 rackId, string controlName, float value);
	#else
	private static void criAtomExAsrRack_GetPerformanceInfo(Int32 rackId, out PerformanceInfo perfInfo) { perfInfo = new PerformanceInfo(); }
	private static void criAtomExAsrRack_ResetPerformanceMonitor(Int32 rackId) { }
	private static void criAtomExAsrRack_SetAisacControlById(Int32 rackId, ushort controlId, float value) { }
	private static void criAtomExAsrRack_SetAisacControlByName(Int32 rackId, string controlName, float value) { }
	#endif

	#endif
	#endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
