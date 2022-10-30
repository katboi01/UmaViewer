/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/
using UnityEngine;
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
 * <summary>Sound output data analysis module (for each player/source/bus)</summary>
 * <remarks>
 * <para header='Description'>Performs sound output analysis for each CriAtomSource/CriAtomExPlayer or for each bus.<br/>
 * Provides features such as Level Meter.<br/></para>
 * <para header='Note'>When attached to CriAtomSource/CriAtomExPlayer, analysis is not possible
 * when HCA-MX or platform-specific sound compression codec is used.<br/>
 * Use HCA or ADX codecs.</para>
 * </remarks>
 */

public class CriAtomExOutputAnalyzer : CriDisposable
{
	public IntPtr nativeHandle {get {return this.handle;} }

	/**
	 * <summary>Maximum number of spectrum analyzer bands</summary>
	 * <remarks>
	 * <para header='Description'>Maximum number of bands that the spectrum analyzer can output.</para>
	 * </remarks>
	 */
	public const int MaximumSpectrumBands = 512;

	/**
	 * <summary>Waveform acquisition callback</summary>
	 * <remarks>
	 * <para header='Description'>A callback for acquiring the output waveform data.<br/></para>
	 * </remarks>
	 */
	public delegate void PcmCaptureCallback(float[] dataL, float[] dataR, int numChannels, int numData);

	/**
	 * <summary>Sound output data analysis module config structure</summary>
	 * <remarks>
	 * <para header='Description'>A config specified when creating the analysis module.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExOutputAnalyzerCriWare.CriAtomExOutputAnalyzer'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Config
	{
		/**
		 * <summary>Whether to enable the Level Meter</summary>
		 * <remarks>
		 * <para header='Description'>Enables the Level Meter.</para>
		 * </remarks>
		 */
		public bool enableLevelmeter;

		/**
		 * <summary>Whether to enable the spectrum analyzer</summary>
		 * <remarks>
		 * <para header='Description'>Enables the spectrum analyzer.
		 * When using the function, specify True in this flag and
		 * set numSpectrumAnalyzerBands to a positive number equal to or
		 * smaller than MaximumSpectrumBands.</para>
		 * </remarks>
		 */
		public bool enableSpectrumAnalyzer;

		/**
		 * <summary>Whether to enable acquisition of the waveform data</summary>
		 * <remarks>
		 * <para header='Description'>Enables the acquisition of output data.
		 * When using the function, specify True in this flag and
		 * set numCapturedPcmSamples to a positive number.</para>
		 * </remarks>
		 */
		public bool enablePcmCapture;

		/**
		 * <summary>Whether to enable the waveform data acquisition callback</summary>
		 * <remarks>
		 * <para header='Description'>Enables the callback for acquiring the output data.<br/>
		 * When using the function, specify True in this flag and
		 * call ExecutePcmCaptureCallback regularly in MonoBehaviour.Update etc.<br/></para>
		 * </remarks>
		 */
		public bool enablePcmCaptureCallback;

		/**
		 * <summary>The number of spectrum analyzer bands</summary>
		 */
		public int numSpectrumAnalyzerBands;

		/**
		 * <summary>The number of waveform data samples acquired at one time</summary>
		 */
		public int numCapturedPcmSamples;
	};

	/**
	 * <summary>Creates a sound output data analysis module</summary>
	 * <returns>Sound output data analysis module</returns>
	 * <remarks>
	 * <para header='Description'>Create an output Voice data analysis module. <br/>
	 * Use the analysis module you created by attaching it to a CriAtomSource, CriAtomExPlayer, or bus. <br/>
	 * Perform analysis of the level meter etc. on the attached audio output. <br/><code>
	 * // 解析モジュールの作成例
	 *
	 * // コンフィグでSpectrumAnalyzerを有効にし、バンド数を指定
	 * CriAtomExOutputAnalyzer.Config config = new CriAtomExOutputAnalyzer.Config();
	 * config.enableSpectrumAnalyzer = true;
	 * config.numSpectrumAnalyzerBands = 16;
	 *
	 * // 出力データ解析モジュールを作成
	 * this.analyzer = new CriAtomExOutputAnalyzer(config);
	 * </code>
	 * </para>
	 * <para header='Note'>Only one CriAtomSource/CriAtomExPlayer/ bus can be attached to the analysis module.<br/>
	 * If you want to reuse the analysis module, detach it.<br/></para>
	 * <para header='Note'>Unmanaged resources are reserved when creating a sound output data analysis module.<br/>
	 * When you no longer need the analysis module, be sure to call the CriAtomExOutputAnalyzer.Dispose method.</para>
	 * </remarks>
	 */
	public CriAtomExOutputAnalyzer(Config config)
	{
		InitializeWithConfig(config);
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	}

	/**
	 * <summary>Discards the output data analysis module</summary>
	 * <remarks>
	 * <para header='Description'>Discards the output data analysis module.<br/>
	 * At the time of calling this function, all the resources allocated in the plug-in when creating the output data analysis module are released.<br/>
	 * To prevent memory leak, call this method when the output data analysis module is no longer required.<br/></para>
	 * <para header='Note'>This function is a return-on-complete function.<br/>
	 * If there are any attached AtomExPlayers, they are detached in this function.<br/>
	 * Note that the sounds being played by the target AtomExPlayers are forcibly stopped.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExOutputAnalyzer::CriAtomExOutputAnalyzer'/>
	 */
	public override void Dispose()
	{
		this.Dispose(true);
	}

	protected void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		if (this.handle != IntPtr.Zero) {
			/* アタッチ済みのプレーヤがあればデタッチ */
			this.DetachExPlayer();
			this.DetachDspBus();

			/* ネイティブリソースの破棄 */
			criAtomExOutputAnalyzer_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}

		if (disposing) {
			GC.SuppressFinalize(this);
		}
	}

	/**
	 * <summary>Attaching the AtomExPlayer</summary>
	 * <returns>Whether the attach was successful (success: True, failure: False)</returns>
	 * <remarks>
	 * <para header='Description'>Attaches the AtomExPlayer that analyzes the output data.<br/>
	 * It is not possible to attach multiple AtomExPlayers.
	 * If you attach another AtomExPlayer while one is attached, the attached AtomExPlayer is detached.<br/>
	 * <br/>
	 * To attach CriAtomSource, use CriAtomSource::AttachToOutputAnalyzer.</para>
	 * <para header='Note'>Attachment must be done before starting playback. Attachment fails after starting playback.<br/>
	 * <br/>
	 * If you discard the AtomExPlayer attached using this function before detaching it,
	 * an access violation occurs at the time of detachment.<br/>
	 * Be sure to first detach the AtomExPlayer before discarding it.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExOutputAnalyzer::DetachExPlayer'/>
	 * <seealso cref='CriAtomSource::AttachToOutputAnalyzer'/>
	 */
	public bool AttachExPlayer(CriAtomExPlayer player)
	{
		if (player == null || !player.isAvailable ||
			this.handle == IntPtr.Zero) {
			return false;
		}

		/* アタッチ済みならデタッチ */
		this.DetachExPlayer();
		this.DetachDspBus();

		/* プレーヤの状態をチェック */
		CriAtomExPlayer.Status status = player.GetStatus();
		if (status != CriAtomExPlayer.Status.Stop) {
			return false;
		}

		criAtomExOutputAnalyzer_AttachExPlayer(this.handle, player.nativeHandle);
		this.player = player;

		return true;
	}

	/**
	 * <summary>Detaches the AtomExPlayer</summary>
	 * <remarks>
	 * <para header='Description'>Detaches the AtomExPlayer that analyzes the output data.<br/>
	 * After detaching, the subsequent analysis will not be done.</para>
	 * <para header='Note'>If this function is called while the player already attached is playing a sound,
	 * the playback is forcibly stopped before detaching.<br/>
	 * <br/>
	 * An access violation occurs if the attached AtomExPlayer has already been discarded.<br/>
	 * Be sure to call this function or CriAtomExOutputAnalyzer::Dispose before
	 * discarding the AtomExPlayer.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExOutputAnalyzer::AttachExPlayer'/>
	 * <seealso cref='CriAtomExOutputAnalyzer::Dispose'/>
	 */
	public void DetachExPlayer()
	{
		if (this.player == null || !this.player.isAvailable ||
			this.handle == IntPtr.Zero) {
			return;
		}

		CriAtomExPlayer.Status status = this.player.GetStatus();
		if (status != CriAtomExPlayer.Status.Stop) {
			/* 音声再生中にデタッチは行えないため、強制的に停止 */
			Debug.LogWarning("[CRIWARE] Warning: CriAtomExPlayer is forced to stop for detaching CriAtomExOutputAnalyzer.");
			this.player.StopWithoutReleaseTime();
		}

		criAtomExOutputAnalyzer_DetachExPlayer(this.handle, this.player.nativeHandle);
		this.player = null;
	}

	/**
	 * <summary>Attaching the DSP bus</summary>
	 * <returns>Whether the attach was successful (success: True, failure: False)</returns>
	 * <remarks>
	 * <para header='Description'>Attach a DSP bus for output data analysis.<br/>
	 * It is not possible to attach multiple DSP buses.
	 * If you attach another DSP bus while one is attached, the attached DSP bus is detached.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExOutputAnalyzer::DetachDspBus'/>
	 */
	public bool AttachDspBus(string busName)
	{
		if (busName == null || this.handle == IntPtr.Zero) {
			return false;
		}

		/* アタッチ済みのプレーヤがあればデタッチ */
		this.DetachExPlayer();
		this.DetachDspBus();

	#if !UNITY_PSP2
		criAtomExOutputAnalyzer_AttachDspBusByName(this.handle, busName);
		this.busName = busName;
		return true;
	#else
		Debug.LogError("[CRIWARE] Error: CriAtomExOutputAnalyzer cannot be attached to bus on this platform.");
		return false;
	#endif
	}

	/**
	 * <summary>Detaches the DSP bus</summary>
	 * <remarks>
	 * <para header='Description'>Detach a DSP bus for output data analysis.<br/>
	 * After detaching, the subsequent analysis will not be done.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExOutputAnalyzer::AttachDspBus'/>
	 * <seealso cref='CriAtomExOutputAnalyzer::Dispose'/>
	 */
	public void DetachDspBus()
	{
		if (this.busName == null || this.handle == IntPtr.Zero) {
			return;
		}

		criAtomExOutputAnalyzer_DetachDspBusByName(this.handle, busName);
		this.busName = null;
	}

	/**
	 * <summary>Gets the RMS level of sound output being attached</summary>
	 * <param name='channel'>Channel number</param>
	 * <returns>RMS level</returns>
	 * <remarks>
	 * <para header='Description'>Gets the RMS level of sound output being attached.<br/>
	 * When using this function, create a module setting enableLevelmeter to True in Config.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExOutputAnalyzer::AttachExPlayer'/>
	 * <seealso cref='CriAtomExOutputAnalyzer::AttachDspBus'/>
	 */
	public float GetRms(int channel)
	{
		if ((this.player == null && this.busName == null)
			|| this.handle == IntPtr.Zero) {
			return 0.0f;
		}

		/* プレーヤが再生状態でなければレベルを落とす */
		if (this.player != null &&
			this.player.GetStatus() != CriAtomExPlayer.Status.Playing &&
			this.player.GetStatus() != CriAtomExPlayer.Status.Prep) {
			return 0.0f;
		}

		return criAtomExOutputAnalyzer_GetRms(this.handle, channel);
	}

	/**
	 * <summary>Gets the spectrum analysis result</summary>
	 * <param name='levels'>Analysis result (amplitude of each band)</param>
	 * <remarks>
	 * <para header='Description'>Gets the amplitude for each band analyzed by the spectrum analyzer.<br/>
	 * The number of elements in the array is the number of bands specified when the module was created.<br/>
	 * When using this function, create a module setting enableSpectrumAnalyzer to True and a numSpectrumAnalyzerBands to
	 * a positive number equal to or smaller than MaximumSpectrumBands in Config.
	 * If you want to display the analysis result like a commercial spectrum analyzer,
	 * you need to convert the value returned by this function into a decibel value.<br/></para>
	 * </remarks>
	 * <example><code>
	 * // 例：スペクトル解析結果を取得するコンポーネント
	 * public class SpectrumLevelMeter : MonoBehaviour {
	 *  private CriAtomExOutputAnalyzer analyzer;
	 *  void Start() {
	 *      // 引数 config については省略。モジュールの作成時に指定したバンド数は 8 とする
	 *      this.analyzer = new CriAtomExOutputAnalyzer(config);
	 *      // CriAtomExPlayer のアタッチについては省略
	 *  }
	 *
	 *  void Update() {
	 *      // 音声再生中の実行
	 *      float[] levels = new float[8];
	 *      analyzer.GetSpectrumLevels (ref levels);
	 *      // levelsの0帯域目の振幅値をデシベル値に変換
	 *      float db = 20.0f * Mathf.Log10(levels[0]);
	 *      Debug.Log (db);
	 *  }
	 * }
	 * </code></example>
	 * <seealso cref='CriAtomExOutputAnalyzer::AttachExPlayer'/>
	 * <seealso cref='CriAtomExOutputAnalyzer::AttachDspBus'/>
	 */
	public void GetSpectrumLevels(ref float[] levels)
	{
		if ((this.player == null && this.busName == null) || this.handle == IntPtr.Zero) {
			return;
		}

		if (levels == null || levels.Length < numBands) {
			levels = new float[numBands];
		}

		IntPtr ret = criAtomExOutputAnalyzer_GetSpectrumLevels(this.handle);
		Marshal.Copy(ret, levels, 0, numBands);
	}

	/**
	 * <summary>Gets the waveform data of the sound output being attached</summary>
	 * <param name='data'>Output data</param>
	 * <param name='ch'>Channel</param>
	 * <remarks>
	 * <para header='Description'>Gets the waveform data of the sound output being attached.<br/>
	 * When using this function, create a module by setting enablePcmCapture to True
	 * and a numCapturedPcmSamples to a positive number in Config.<br/></para>
	 * <para header='Note'>If the argument array is not long enough, it is allocated in the function.<br/>
	 * To avoid unnecessary GC, pass an array with a length longer than
	 * the number of data samples specified in the initialization config as an argument.
	 * Currently, the only channels that can be acquired are L/R. For ch, specify 0 or 1.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExOutputAnalyzer::AttachExPlayer'/>
	 * <seealso cref='CriAtomExOutputAnalyzer::AttachDspBus'/>
	 */
	public void GetPcmData(ref float[] data, int ch)
	{
		if ((this.player == null && this.busName == null) || this.handle == IntPtr.Zero) {
			return;
		}

		if (data == null || data.Length < numCapturedPcmSamples) {
			data = new float[numCapturedPcmSamples];
		}

		IntPtr ret = criAtomExOutputAnalyzer_GetPcmData(this.handle, ch);
		if (ret != IntPtr.Zero) {
			Marshal.Copy(ret, data, 0, numCapturedPcmSamples);
		}
	}

	/**
	 * <summary>Registers the waveform data acquisition callback</summary>
	 * <remarks>
	 * <para header='Description'>Register the callback for acquiring output data.<br/>
	 * When using callback for acquiring the waveform data, register the callback using this function
	 * before calling ExecutePcmCaptureCallback.<br/></para>
	 * </remarks>
	 */
	public void SetPcmCaptureCallback(PcmCaptureCallback callback)
	{
		this.userPcmCaptureCallback = callback;
	}

	/**
	 * <summary>Calls the waveform data acquisition callback</summary>
	 * <remarks>
	 * <para header='Description'>Call the callback for acquiring the output data.<br/>
	 * When this function is called, the callback is called multiple times with the output difference data
	 * from the last execution as an argument.<br/></para>
	 * <para header='Note'>When using a callback for getting the waveform data, call this function regularly.<br/>
	 * If this function is not called for a long time, there may be a loss of acquired waveform data.<br/></para>
	 * </remarks>
	 */
	public void ExecutePcmCaptureCallback()
	{
	#if !(!UNITY_EDITOR && UNITY_WEBGL)
		if (CriAtomExOutputAnalyzer.InternalCallbackFunctionPointer == IntPtr.Zero) {
			CriAtomExOutputAnalyzer.DelegateObject = new InternalPcmCaptureCallback(CriAtomExOutputAnalyzer.Callback);
			CriAtomExOutputAnalyzer.InternalCallbackFunctionPointer = Marshal.GetFunctionPointerForDelegate(CriAtomExOutputAnalyzer.DelegateObject);
		}

		CriAtomExOutputAnalyzer.UserPcmCaptureCallback = this.userPcmCaptureCallback;
		CriAtomExOutputAnalyzer.DataL = this.dataL;
		CriAtomExOutputAnalyzer.DataR = this.dataR;

		criAtomExOutputAnalyzer_ExecuteQueuedPcmCapturerCallbacks(this.handle, CriAtomExOutputAnalyzer.InternalCallbackFunctionPointer);

		CriAtomExOutputAnalyzer.UserPcmCaptureCallback = null;
		CriAtomExOutputAnalyzer.DataL = null;
		CriAtomExOutputAnalyzer.DataR = null;
	#endif
	}

	/**
	 * \deprecated
	 * This is a deprecated API that will be removed.
	 * Please consider using SetPcmCaptureCallback(PcmCaptureCallback) and ExecutePcmCaptureCallback() instead.
	*/
	[System.Obsolete("Use SetPcmCaptureCallback(PcmCaptureCallback) and ExecutePcmCaptureCallback()")]
	public void ExecutePcmCaptureCallback(PcmCaptureCallback callback)
	{
		this.userPcmCaptureCallback = callback;
		ExecutePcmCaptureCallback();
	}

	#region Internal Members
	protected CriAtomExOutputAnalyzer() {
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	}

	~CriAtomExOutputAnalyzer()
	{
		this.Dispose(false);
	}

	protected void InitializeWithConfig(Config config) {
		/* ネイティブリソースの作成 */
		this.handle = criAtomExOutputAnalyzer_Create(ref config);
		if (this.handle == IntPtr.Zero) {
			throw new Exception("criAtomExOutputAnalyzer_Create() failed.");
		}

		/* コンフィグ指定の記憶 */
		{
			this.numBands = config.numSpectrumAnalyzerBands;
			this.numCapturedPcmSamples = config.numCapturedPcmSamples;
			if (config.enablePcmCaptureCallback) {
	#if !UNITY_EDITOR && UNITY_WEBGL
				Debug.LogError("[CRIWARE] PCM capture callback is not supported for this platform.");
	#else
				if (this.dataL == null) {
					this.dataL = new float[pcmCapturerNumMaxData];
					this.dataR = new float[pcmCapturerNumMaxData];
				}
	#endif
			}
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	protected delegate void InternalPcmCaptureCallback(IntPtr dataL, IntPtr dataR, int numChannels, int numData);

	protected IntPtr handle = IntPtr.Zero;
	protected CriAtomExPlayer player = null;
	protected string busName = null;
	protected int numBands = 8;
	protected int numCapturedPcmSamples = 4096;
	protected PcmCaptureCallback userPcmCaptureCallback = null;
	protected float [] dataL, dataR;
	protected const int pcmCapturerNumMaxData = 512;

	[AOT.MonoPInvokeCallback(typeof(InternalPcmCaptureCallback))]
	private static void Callback(IntPtr ptrL, IntPtr ptrR, int numChannels, int numData)
	{
		if (DataL == null)
			return;

		Marshal.Copy(ptrL, DataL, 0, numData);
		if (numChannels > 1) {
			Marshal.Copy(ptrR, DataR, 0, numData);
		}
		if (UserPcmCaptureCallback != null) {
			UserPcmCaptureCallback(DataL, DataR, numChannels, numData);
		}
	}

	protected static IntPtr InternalCallbackFunctionPointer = IntPtr.Zero;
	protected static InternalPcmCaptureCallback DelegateObject;
	protected static float [] DataL, DataR;
	protected static PcmCaptureCallback UserPcmCaptureCallback = null;
	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	protected static extern IntPtr criAtomExOutputAnalyzer_Create([In] ref Config config);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	protected static extern void criAtomExOutputAnalyzer_Destroy(IntPtr analyzer);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	protected static extern void criAtomExOutputAnalyzer_AttachExPlayer(IntPtr analyzer, IntPtr player);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	protected static extern void criAtomExOutputAnalyzer_DetachExPlayer(IntPtr analyzer, IntPtr player);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	protected static extern void criAtomExOutputAnalyzer_AttachDspBusByName(IntPtr analyzer, string busName);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	protected static extern void criAtomExOutputAnalyzer_DetachDspBusByName(IntPtr analyzer, string busName);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	protected static extern float criAtomExOutputAnalyzer_GetRms(IntPtr analyzer, int channel);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	protected static extern IntPtr criAtomExOutputAnalyzer_GetSpectrumLevels(IntPtr analyzer);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	protected static extern IntPtr criAtomExOutputAnalyzer_GetPcmData(IntPtr analyzer, int ch);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	protected static extern void criAtomExOutputAnalyzer_ExecuteQueuedPcmCapturerCallbacks(IntPtr analyzer, IntPtr callback);
	#else
	protected static IntPtr criAtomExOutputAnalyzer_Create([In] ref Config config) { return new IntPtr(1); }
	protected static void criAtomExOutputAnalyzer_Destroy(IntPtr analyzer) {     }
	protected static void criAtomExOutputAnalyzer_AttachExPlayer(IntPtr analyzer, IntPtr player) { }
	protected static void criAtomExOutputAnalyzer_DetachExPlayer(IntPtr analyzer, IntPtr player) { }
	protected static void criAtomExOutputAnalyzer_AttachDspBusByName(IntPtr analyzer, string busName) { }
	protected static void criAtomExOutputAnalyzer_DetachDspBusByName(IntPtr analyzer, string busName) { }
	protected static float criAtomExOutputAnalyzer_GetRms(IntPtr analyzer, int channel) { return 0.0f; }
	protected static IntPtr criAtomExOutputAnalyzer_GetSpectrumLevels(IntPtr analyzer) { return IntPtr.Zero; }
	protected static IntPtr criAtomExOutputAnalyzer_GetPcmData(IntPtr analyzer, int ch) { return IntPtr.Zero; }
	protected static void criAtomExOutputAnalyzer_ExecuteQueuedPcmCapturerCallbacks(IntPtr analyzer, IntPtr callback) { }
	#endif
	#endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
