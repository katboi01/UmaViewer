/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if (UNITY_EDITOR && !UNITY_EDITOR_LINUX) || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_IOS
#define CRIWAREPLUGIN_SUPPORT_MIC
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
 * <summary>Microphone for capturing sound</summary>
 * <remarks>
 * <para header='Description'>A class for capturing input from a physical microphone or sound device.<br/>
 * Performs control such as starting input, getting status and getting data.<br/>
 * You can also get the information about the sound input device using CriWare.CriAtomExMic::GetDevices .<br/></para>
 * </remarks>
 */
public class CriAtomExMic : CriDisposable
{
	/**
	 * <summary>Microphone device information structure</summary>
	 * <remarks>
	 * <para header='Description'>A structure for sound input device information.<br/>
	 * The device information is acquired from CriWare.CriAtomExMic::GetDevices .<br/></para>
	 * </remarks>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct DeviceInfo
	{
		/**
		 * <summary>Device ID</summary>
		 * <remarks>
		 * <para header='Description'>A string which indicates the identifier of the platform audio input device.<br/></para>
		 * </remarks>
		 */
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public string deviceId;
		/**
		 * <summary>Device name</summary>
		 * <remarks>
		 * <para header='Description'>Name information for the platform audio input device.<br/></para>
		 * </remarks>
		 */
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public string deviceName;
		/**
		 * <summary>Device flag</summary>
		 * <remarks>
		 * <para header='Description'>A flag that is referenced when creating an audio input device.<br/></para>
		 * </remarks>
		 */
		public uint deviceFlags;
		/**
		 * <summary>Maximum number of channels</summary>
		 * <remarks>
		 * <para header='Description'>Maximum number of channels supported.<br/></para>
		 * </remarks>
		 */
		public int maxChannels;
		/**
		 * <summary>Maximum sampling frequency</summary>
		 * <remarks>
		 * <para header='Description'>Maximum supported sampling frequency.<br/></para>
		 * </remarks>
		 */
		public int maxSamplingRate;
	}

	/**
	 * <summary>A config structure for creating an AtomEx microphone</summary>
	 * <remarks>
	 * <para header='Description'>A structure for specifying the behavior for creating an AtomEx microphone.<br/>
	 * It is specified as an argument of the CriWare.CriAtomExMic::Create function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::Create'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Config
	{
		/**
		 * <summary>Device ID</summary>
		 * <remarks>
		 * <para header='Description'>A string which indicates the identifier of the platform audio input device.<br/>
		 * Specify CRIATOMMIC_DEFAULT_DEVICE_ID if you don't want to specify one.<br/></para>
		 * </remarks>
		 * <seealso cref='CriAtomExMic::Create'/>
		 * <seealso cref='CriAtomExMic::GetDevices'/>
		 */
		[MarshalAs(UnmanagedType.LPStr)]
		public string deviceId;
		/**
		 * <summary>Creation flag</summary>
		 * <remarks>
		 * <para header='Description'>A flag that is referenced when creating an audio input device.<br/></para>
		 * </remarks>
		 */
		public uint flags;
		/**
		 * <summary>The number of channels</summary>
		 * <remarks>
		 * <para header='Description'>The number of channels required for audio input. The default is 1.<br/>
		 * The creation fails if you specify an unsupported number of channels.<br/></para>
		 * </remarks>
		 * <seealso cref='CriAtomExMic::IsFormatSupported'/>
		 */
		public int numChannels;
		/**
		 * <summary>Sampling rate</summary>
		 * <remarks>
		 * <para header='Description'>The sampling frequency required for audio input. The default is 44100.<br/>
		 * The creation fails if you specify a non-supported sampling frequency.<br/></para>
		 * </remarks>
		 * <seealso cref='CriAtomExMic::IsFormatSupported'/>
		 */
		public int samplingRate;
		/**
		 * <summary>Frame size (number of samples)</summary>
		 * <remarks>
		 * <para header='Description'>The number of samples indicating the size of 1 frame. The default is 256.<br/>
		 * This is the processing unit of the effect for which CriWare.CriAtomExMic::AttachEffect was called.<br/></para>
		 * </remarks>
		 */
		public uint frameSize;
		/**
		 * <summary>Buffer size (ms)</summary>
		 * <remarks>
		 * <para header='Description'>The size of the buffer maintained internally. The default is 50msec.<br/>
		 * This is the processing unit of the effect for which CriWare.CriAtomExMic::AttachEffect was called.<br/></para>
		 * </remarks>
		 */
		public uint bufferingTime;
		/**
		 * <summary>Platform context</summary>
		 * <remarks>
		 * <para header='Description'>It is not used at this time.<br/></para>
		 * </remarks>
		 */
		public IntPtr context;

		/**
		 * <summary>Default setting</summary>
		 * <remarks>
		 * <para header='Description'>The default setting for the config structure.<br/></para>
		 * </remarks>
		 */
		public static Config Default {
			get {
				Config config = new Config();
				config.deviceId = null;
				config.flags = 0;
				config.numChannels = 1;
				config.samplingRate = 44100;
				config.frameSize = 256;
				config.bufferingTime = 50;
				return config;
			}
		}
	}

	/**
	 * <summary>Microphone effect</summary>
	 * <remarks>
	 * <para header='Description'>A class for the effect applied to the input sound of the microphone.<br/>
	 * Returned by CriWare.CriAtomExMic::AttachEffect .<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::SetEffectParameter'/>
	 * <seealso cref='CriAtomExMic::UpdateEffectParameter'/>
	 */
	public class Effect
	{
		public IntPtr handle { get; private set; }
		public IntPtr afxInstance { get; private set; }

		public Effect(IntPtr handle, IntPtr afxInstance)
		{
			this.handle = handle;
			this.afxInstance = afxInstance;
		}
	}

	#region Error Messages
	private const string errorInvalidHandle = "[CRIWARE] Invalid native handle of CriAtomMic.";
	private const string errorInvalidBufferLength = "[CRIWARE] Invalid buffer length for CriAtomMic.ReadData.";
	private const string errorInvalidNumBuffers = "[CRIWARE] Number of buffers are not same with channels of CriAtomMic.";
	private const string errorAlreadyInitialized = "[CRIWARE] CriAtomMic module is already initialized.";
	private const string errorNotInitialized = "[CRIWARE] CriAtomMic module is not initialized.";
	#endregion

	public static bool isInitialized { get; private set; }

	/**
	 * <summary>Initializes the CriAtomMic module</summary>
	 * <remarks>
	 * <para header='Description'>Initializes the CriAtomMic module.<br/>
	 * In order to use the function of the module, you must call this function.<br/>
	 * (You can use the features of the module after calling this function and before calling the CriWare.CriAtomExMic::FinalizeModule function.)<br/></para>
	 * <para header='Note'>After calling this function, be sure to call the corresponding CriWare.CriAtomExMic::FinalizeModule function.<br/>
	 * In addition, this function cannot be called again until the CriWare.CriAtomExMic::FinalizeModule function is called.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::FinalizeModule'/>
	 */
	public static void InitializeModule()
	{
		_initializationCount++;
		if (_initializationCount > 1) {
			return;
		}

#if CRIWAREPLUGIN_SUPPORT_MIC
		if (isInitialized) {
			Debug.LogError(errorAlreadyInitialized);
			return;
		}
		criAtomMicUnity_Initialize();
		isInitialized = true;
#else
		Debug.LogError("[CRIWARE] CriAtomExMic does not support this platform.");
#endif
	}

	/**
	 * <summary>Terminates the CriAtomMic module</summary>
	 * <remarks>
	 * <para header='Description'>Terminates the CriAtomMic module.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::InitializeModule'/>
	 */
	public static void FinalizeModule()
	{
		_initializationCount--;
		if (_initializationCount > 0) {
			return;
		} else if (_initializationCount < 0) {
			_initializationCount = 0;
		}

#if CRIWAREPLUGIN_SUPPORT_MIC
		if (!isInitialized) {
			Debug.LogError(errorNotInitialized);
			return;
		}
		CriDisposableObjectManager.CallOnModuleFinalization(CriDisposableObjectManager.ModuleType.AtomMic);
		criAtomMicUnity_Finalize();
		isInitialized = false;
#endif
	}

	/**
	 * <summary>[iOS] Sets the output category of the microphone input</summary>
	 * <param name='enable'>Enable or disable the setting (True: enable, False: disable)</param>
	 * <remarks>
	 * <para header='Description'>Set the category (AVAudioSessionCategory) to use the microphone on iOS.<br/>
	 * <br/>
	 * The processing performed by calling this function will also be performed within CriWare.CriAtomExMic::InitializeModule and
	 * CriWare.CriAtomExMic::FinalizeModule .<br/>
	 * Therefore, it is usually not neccessary to call this function directly.<br/>
	 * This function should only be used when the AVAudioSessionCategory setting needs to be changed,
	 * in order to use an external microphone input module other than the regular CriAtomExMic module.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::InitializeModule'/>
	 * <seealso cref='CriAtomExMic::FinalizeModule'/>
	 */
	public static void SetupOutputCategoryForMic_IOS(bool enable)
	{
#if !UNITY_EDITOR && UNITY_IOS
		criAtomUnity_SetMicEnabled_IOS(enable);
#endif
	}

	/**
	 * <summary>Gets the microphone device</summary>
	 * <returns>Microphone device array</returns>
	 * <remarks>
	 * <para header='Description'>Gets information on the microphone device.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::GetDefaultDevice'/>
	 */
	public static DeviceInfo[] GetDevices()
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		if (!isInitialized) {
			Debug.LogError(errorNotInitialized);
			return null;
		}

		int numDevices = criAtomMic_GetNumDevices();
		var devices = new DeviceInfo[numDevices];
		for (int i = 0; i < numDevices; i++) {
			criAtomMic_GetDevice(i, out devices[i]);
		}
		return devices;
#else
		return null;
#endif
	}

	/**
	 * <summary>Gets the number of microphone devices</summary>
	 * <returns>The number of microphone devices</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of microphone devices connected to the terminal.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::GetDevices'/>
	 */
	public static int GetNumDevices()
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		return criAtomMic_GetNumDevices();
#else
		return 0;
#endif
	}

	/**
	 * <summary>Gets the default microphone device</summary>
	 * <returns>Microphone device structure</returns>
	 * <remarks>
	 * <para header='Description'>Gets the information about the default microphone device.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::GetDevices'/>
	 */
	public static DeviceInfo? GetDefaultDevice()
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		if (!isInitialized) {
			Debug.LogError(errorNotInitialized);
			return null;
		}

		var device = new DeviceInfo();
		bool result = criAtomMic_GetDefaultDevice(out device);
		if (result) {
			return device;
		}
#endif
		return null;
	}

	/**
	 * <summary>Gets the format support status</summary>
	 * <param name='config'>Config information</param>
	 * <returns>True: supported, False: not supported</returns>
	 * <remarks>
	 * <para header='Description'>Gets whether the specified format in the config information is supported.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::GetDevices'/>
	 */
	public static bool IsFormatSupported(Config config)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		return criAtomMic_IsFormatSupported(ref config);
#else
		return false;
#endif
	}

	#region Internal Members
	private IntPtr handle = IntPtr.Zero;
	private IntPtr[] bufferPointers = null;
	private GCHandle[] gcHandles = null;
#if CRIWAREPLUGIN_SUPPORT_MIC
	private CriAudioWriteStream outputWriteStream = null;
#endif

	private static int _initializationCount = 0;
	#endregion

	/**
	 * <summary>Creates an AtomEx microphone</summary>
	 * <param name='config'>Config information</param>
	 * <returns>AtomEx microphone</returns>
	 * <remarks>
	 * <para header='Description'>Creates a microphone instance for capturing sound.<br/>
	 * <br/>
	 * Calling the CriWare.CriAtomExMic::Create function creates an AtomEx microphone
	 * and an instance ( CriAtomExMic ) for controlling the microphone is returned.<br/>
	 * <br/>
	 * This function returns null if it failed to open the sound input device.
	 * <br/>
	 * To actually start sound input, call the CriWare.CriAtomExMic::Start function.<br/>
	 * Use the CriWare.CriAtomExMic::ReadData function for getting the captured sound data.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::Destroy'/>
	 */
	public static CriAtomExMic Create(Config? config = null)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		if (!isInitialized) {
			Debug.LogError(errorNotInitialized);
			return null;
		}

		Config internalConfig = (config.HasValue) ? config.Value : Config.Default;
		IntPtr handle = criAtomMic_Create(ref internalConfig, IntPtr.Zero, 0);
		if (handle == IntPtr.Zero) {
			Debug.LogWarning("Failed to open audio input device.");
			return null;
		}
		return new CriAtomExMic(handle);
#else
		return null;
#endif
	}

	private CriAtomExMic(IntPtr handle)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		this.handle = handle;
		int numChannels = this.GetNumChannels();
		this.bufferPointers = new IntPtr[numChannels];
		this.gcHandles = new GCHandle[numChannels];

		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.AtomMic);
#endif
	}

	~CriAtomExMic(){
		Dispose();
	}

	/**
	 * <summary>Discards the microphone</summary>
	 * <remarks>
	 * <para header='Description'>Discard the microphone for capturing sound.<br/></para>
	 * <para header='Note'>This function is a return-on-complete function. The time it takes depends on the platform.<br/>
	 * If this function is called at a timing when the screen needs to be updated such as in a game loop,
	 * the process is blocked in the unit of milliseconds, resulting in dropped frames.<br/>
	 * Create/discard microphone at a timing when load fluctuations is accepted
	 * such as when switching scenes.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::Create'/>
	 */
	public override void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		CriDisposableObjectManager.Unregister(this);

		if (this.handle != IntPtr.Zero) {
			criAtomMic_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}
#endif
	}

	/**
	 * <summary>Starts microphone sound capturing</summary>
	 * <remarks>
	 * <para header='Description'>Starts capturing sounds by the microphone.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::Create'/>
	 * <seealso cref='CriAtomExMic::Stop'/>
	 */
	public void Start()
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		criAtomMic_Start(this.handle);
#endif
	}

	/**
	 * <summary>Stops the microphone sound capturing</summary>
	 * <remarks>
	 * <para header='Description'>Stops capturing sound by the microphone.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::Create'/>
	 * <seealso cref='CriAtomExMic::Start'/>
	 */
	public void Stop()
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		criAtomMic_Stop(this.handle);
#endif
	}

	/**
	 * <summary>Gets the number of microphone channels</summary>
	 * <returns>The number of channels</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of microphone channels.<br/></para>
	 * </remarks>
	 */
	public int GetNumChannels()
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		return criAtomMic_GetNumChannels(this.handle);
#else
		return 0;
#endif
	}

	/**
	 * <summary>Gets the microphone sampling frequency</summary>
	 * <returns>Sampling frequency</returns>
	 * <remarks>
	 * <para header='Description'>Gets the sampling frequency of the microphone.<br/></para>
	 * </remarks>
	 */
	public int GetSamplingRate()
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		return criAtomMic_GetSamplingRate(this.handle);
#else
		return 0;
#endif
	}

	/**
	 * <summary>Gets the number of samples of buffered data</summary>
	 * <returns>The number of samples buffered</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of microphone input data samples existing in the internal buffer.<br/></para>
	 * </remarks>
	 */
	public uint GetNumBufferedSamples()
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		return criAtomMic_GetNumBufferedSamples(this.handle);
#else
		return 0;
#endif
	}

	/**
	 * \deprecated
	 * This is a deprecated API that will be removed.
	 * Please consider using CriAtomExMic.GetNumBufferedSamples instead.
	*/
	[System.Obsolete("Use CriAtomExMic.GetNumBufferedSamples")]
	public uint GetNumBufferredSamples()
	{
		return GetNumBufferedSamples();
	}

	/**
	 * <summary>Gets the microphone availability</summary>
	 * <returns>Whether it is available (True: available, False: not available)</returns>
	 * <remarks>
	 * <para header='Description'>Checks the status of the audio input device to see if it is available.<br/></para>
	 * </remarks>
	 */
	public bool IsAvailable()
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		return this.handle != IntPtr.Zero ? criAtomMic_IsAvailable(this.handle) : false;
#else
		return false;
#endif
	}

	/**
	 * <summary>Reads the microphone input data (monaural)</summary>
	 * <param name='bufferMono'>Data buffer</param>
	 * <returns>The number of samples that could be acquired</returns>
	 * <remarks>
	 * <para header='Description'>Gets the data input to the microphone.<br/>
	 * If this function is not called regularly and the internal buffer becomes full,
	 * the data that cannot be fed to the buffer is discarded.</para>
	 * <para header='Note'>This function is valid only when you call CriWare.CriAtomExMic::Start by setting the number of
	 * channels to 1.<br/>
	 * Otherwise, the assertion fails.</para>
	 * </remarks>
	 */
	public uint ReadData(float[] bufferMono)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		return ReadData(bufferMono, (uint)bufferMono.Length);
#else
		return 0;
#endif
	}

	/**
	 * <summary>Reads the microphone input data (monaural)</summary>
	 * <param name='bufferMono'>Data buffer</param>
	 * <param name='numToRead'>The number of samples to read</param>
	 * <returns>The number of samples that could be acquired</returns>
	 * <remarks>
	 * <para header='Description'>Gets the data input to the microphone.<br/>
	 * If this function is not called regularly and the internal buffer becomes full,
	 * the data that cannot be fed to the buffer is discarded.</para>
	 * <para header='Note'>This function is valid only when you call CriWare.CriAtomExMic::Start by setting the number of
	 * channels to 1.<br/>
	 * Otherwise, the assertion fails.</para>
	 * </remarks>
	 */
	public uint ReadData(float[] bufferMono, uint numToRead)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		Debug.Assert(this.bufferPointers.Length == 1, errorInvalidNumBuffers);
		Debug.Assert(bufferMono.Length >= numToRead && numToRead > 0, errorInvalidBufferLength);

		if (this.outputWriteStream != null) {
			return 0;
		}

		var gch = GCHandle.Alloc(bufferMono, GCHandleType.Pinned);
		this.bufferPointers[0] = gch.AddrOfPinnedObject();

		uint result = InternalReadDataFromBufferPointers(numToRead);
		InternalClearBuffers();
		return result;
#else
		return 0;
#endif
	}

	/**
	 * <summary>Reads the microphone input data (stereo)</summary>
	 * <param name='bufferL'>Data buffer (L channel)</param>
	 * <param name='bufferR'>Data buffer (R channel)</param>
	 * <returns>The number of samples that could be acquired</returns>
	 * <remarks>
	 * <para header='Description'>Gets the data input to the microphone.<br/>
	 * If this function is not called regularly and the internal buffer becomes full,
	 * the data that cannot be fed to the buffer is discarded.</para>
	 * <para header='Note'>This function is valid only when you call CriWare.CriAtomExMic::Start by setting the number of
	 * channels to 2.<br/>
	 * Otherwise, the assertion fails.<br/>
	 * In addition, make sure that each data buffer has the same array length.</para>
	 * </remarks>
	 */
	public uint ReadData(float[] bufferL, float[] bufferR)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(bufferL.Length == bufferR.Length, errorInvalidBufferLength);

		return ReadData(bufferL, bufferR, (uint)bufferL.Length);
#else
		return 0;
#endif
	}

	/**
	 * <summary>Reads the microphone input data (stereo)</summary>
	 * <param name='bufferL'>Data buffer (L channel)</param>
	 * <param name='bufferR'>Data buffer (R channel)</param>
	 * <param name='numToRead'>The number of samples to read</param>
	 * <returns>The number of samples that could be acquired</returns>
	 * <remarks>
	 * <para header='Description'>Gets the data input to the microphone.<br/>
	 * If this function is not called regularly and the internal buffer becomes full,
	 * the data that cannot be fed to the buffer is discarded.</para>
	 * <para header='Note'>This function is valid only when you call CriWare.CriAtomExMic::Start by setting the number of
	 * channels to 2.<br/>
	 * Otherwise, the assertion fails.<br/>
	 * In addition, make sure that array size of all data buffers is equal to or larger than numToRead.</para>
	 * </remarks>
	 */
	public uint ReadData(float[] bufferL, float[] bufferR, uint numToRead)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		Debug.Assert(this.bufferPointers.Length == 2, errorInvalidNumBuffers);
		Debug.Assert(bufferL.Length >= numToRead && bufferR.Length >= numToRead, errorInvalidBufferLength);

		if (this.outputWriteStream != null) {
			return 0;
		}

		this.gcHandles[0] = GCHandle.Alloc(bufferL, GCHandleType.Pinned);
		this.gcHandles[1] = GCHandle.Alloc(bufferR, GCHandleType.Pinned);
		for (int i = 0; i < this.bufferPointers.Length; i++) {
			this.bufferPointers[i] = this.gcHandles[i].AddrOfPinnedObject();
		}
		for (int i = 2; i < this.bufferPointers.Length; i++) {
			this.bufferPointers[i] = IntPtr.Zero;
		}

		uint result = InternalReadDataFromBufferPointers(numToRead);
		InternalClearBuffers();

		return result;
#else
		return 0;
#endif
	}

	/**
	 * <summary>Reads the microphone input data (multi-channel)</summary>
	 * <param name='buffers'>Data buffer array</param>
	 * <returns>The number of samples that could be acquired</returns>
	 * <remarks>
	 * <para header='Description'>Gets the data input to the microphone.<br/>
	 * If this function is not called regularly and the internal buffer becomes full,
	 * the data that cannot be fed to the buffer is discarded.</para>
	 * <para header='Note'>The length of the data buffer array passed to this function must be equal to or larger than
	 * the number of CriAtomExMic::Config channels passed to CriAtomExMic::Start .<br/>
	 * Otherwise, the assertion fails.<br/>
	 * In addition, make sure that array size of all data buffers is equal to or larger than numToRead.</para>
	 * </remarks>
	 */
	public uint ReadData(float[][] buffers)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(buffers != null);
		Debug.Assert(buffers.Length >= this.bufferPointers.Length, errorInvalidNumBuffers);

		int bufferLength = buffers[0].Length;
		for (int i = 1; i < this.bufferPointers.Length; i++) {
			Debug.Assert(buffers[i].Length == bufferLength, errorInvalidNumBuffers);
		}

		return ReadData(buffers, (uint)bufferLength);
#else
		return 0;
#endif
	}

	/**
	 * <summary>Reads the microphone input data (multi-channel)</summary>
	 * <param name='buffers'>Data buffer array</param>
	 * <param name='numToRead'>The number of samples to read</param>
	 * <returns>The number of samples that could be acquired</returns>
	 * <remarks>
	 * <para header='Description'>Gets the data input to the microphone.<br/>
	 * If this function is not called regularly and the internal buffer becomes full,
	 * the data that cannot be fed to the buffer is discarded.</para>
	 * <para header='Note'>The length of the data buffer array passed to this function must be equal to the
	 * number of CriAtomExMic::Config channels passed to CriWare.CriAtomExMic::Start .<br/>
	 * Otherwise, the assertion fails.<br/>
	 * In addition, make sure that array size of each data buffer is equal to or larger than numToRead.</para>
	 * </remarks>
	 */
	public uint ReadData(float[][] buffers, uint numToRead)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		Debug.Assert(buffers != null);
		Debug.Assert(buffers.Length >= this.bufferPointers.Length);
		Debug.Assert(numToRead > 0);

		for (int i = 0; i < this.bufferPointers.Length; i++) {
			Debug.Assert(buffers[i].Length >= numToRead);
		}

		if (this.outputWriteStream != null) {
			return 0;
		}

		for (int i = 0; i < this.bufferPointers.Length; i++) {
			this.gcHandles[i] = GCHandle.Alloc(buffers[i], GCHandleType.Pinned);
			this.bufferPointers[i] = this.gcHandles[i].AddrOfPinnedObject();
		}

		uint result = InternalReadDataFromBufferPointers(numToRead);
		InternalClearBuffers();

		return result;
#else
		return 0;
#endif
	}

	/**
	 * <summary>Sets the light stream</summary>
	 * <param name='stream'>Light stream</param>
	 * <remarks>
	 * <para header='Description'>Sets the output light stream to the microphone.<br/>
	 * The light stream callback function is called when microphone captures input.<br/>
	 * The callback function is called from a separate thread on most platforms,
	 * so the callee must be implemented as thread-safe.</para>
	 * </remarks>
	 */
	public void SetOutputWriteStream(CriAudioWriteStream stream)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		this.outputWriteStream = stream;
		if (stream != null) {
			criAtomMic_SetOutputWriteStream(this.handle, stream.callbackFunction, stream.callbackPointer);
		} else {
			criAtomMic_SetOutputWriteStream(this.handle, IntPtr.Zero, IntPtr.Zero);
		}
#endif
	}

	/**
	 * <summary>Gets the read stream</summary>
	 * <returns>Read stream callback function</returns>
	 * <remarks>
	 * <para header='Description'>Gets the read stream of the microphone in the output direction.<br/>
	 * When calling the read stream callback function, it is necessary to specify the AtomEx microphone handle as the first argument.<br/>
	 * If the read stream callback function is not called regularly and the internal buffer becomes full,
	 * the data that cannot be fed to the buffer is discarded.</para>
	 * </remarks>
	 */
	public CriAudioReadStream GetOutputReadStream()
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		return new CriAudioReadStream(criAtomMic_GetOutputReadStream(), this.handle);
#else
		return null;
#endif
	}

	/**
	 * <summary>Adds an effect</summary>
	 * <param name='afxInterface'>CriAfx interface</param>
	 * <param name='configParameters'>Config parameter array for creating CriAfx</param>
	 * <returns>Microphone effect</returns>
	 * <remarks>
	 * <para header='Description'>Adds an effect to the microphone.<br/>
	 * The effect you add must be created using the CriAfx interface.<br/>
	 * <br/>
	 * If this function is called while the microphone is operating, the effect is suddenly activated,
	 * which may cause noise in the sound.</para>
	 * </remarks>
	 */
	public Effect AttachEffect(IntPtr afxInterface, float[] configParameters)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		IntPtr effectHandle = criAtomMic_AttachEffect(this.handle, afxInterface,
			configParameters, (uint)configParameters.Length, IntPtr.Zero, 0);
		if (effectHandle == IntPtr.Zero) {
			return null;
		}
		return new Effect(effectHandle, criAtomMic_GetEffectInstance(this.handle, effectHandle));
#else
		return null;
#endif
	}

	/**
	 * <summary>Removes an effect</summary>
	 * <param name='effect'>Microphone effect</param>
	 * <remarks>
	 * <para header='Description'>Removes a microphone effect.<br/>
	 * <br/>
	 * If this function is called while the microphone is operating, the effect is suddenly deactivated,
	 * which may cause noise in the sound.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::AttachEffect'/>
	 */
	public void DetachEffect(Effect effect)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		criAtomMic_DetachEffect(this.handle, effect.handle);
#endif
	}

	/**
	 * <summary>Sets effect parameters</summary>
	 * <param name='effect'>Microphone effect</param>
	 * <param name='parameterIndex'>Parameter index</param>
	 * <param name='parameterValue'>Parameter value</param>
	 * <remarks>
	 * <para header='Description'>Sets parameters to the microphone effect.<br/>
	 * This sets the corresponding parameter value by specifying its index.<br/>
	 * <br/>
	 * Parameters are not reflected to the effect just by calling this function,
	 * so it is necessary to finally call criAtomMic_UpdateEffectParameters to reflect the parameters.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::UpdateEffectParameters'/>
	 */
	public void SetEffectParameter(Effect effect, int parameterIndex, float parameterValue)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		criAtomMic_SetEffectParameter(this.handle, effect.handle, (uint)parameterIndex, parameterValue);
#endif
	}

	/**
	 * <summary>Gets effect parameters</summary>
	 * <param name='effect'>Microphone effect</param>
	 * <param name='parameterIndex'>Parameter index</param>
	 * <returns>Parameter value</returns>
	 * <remarks>
	 * <para header='Description'>Gets microphone effect parameters.<br/>
	 * By specifying a parameter index, the value of the corresponding parameter is returned.<br/></para>
	 * </remarks>
	 */
	public float GetEffectParameter(Effect effect, int parameterIndex)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		return criAtomMic_GetEffectParameter(this.handle, effect.handle, (uint)parameterIndex);
#else
		return 0.0f;
#endif
	}

	/**
	 * <summary>Effect Bypass setting</summary>
	 * <param name='effect'>Microphone effect</param>
	 * <param name='bypass'>Bypass setting</param>
	 * <remarks>
	 * <para header='Description'>Specifies that the microphone effects be bypassed.<br/>
	 * If you specify True for Bypass, the effects are deactivated.<br/></para>
	 * </remarks>
	 */
	public void SetEffectBypass(Effect effect, bool bypass)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		criAtomMic_SetEffectBypass(this.handle, effect.handle, bypass);
#endif
	}

	/**
	 * <summary>Sets effect parameters</summary>
	 * <param name='effect'>Microphone effect</param>
	 * <remarks>
	 * <para header='Description'>Sets parameters to the microphone effect.<br/>
	 * This sets the corresponding parameter value by specifying its index.<br/>
	 * <br/>
	 * Parameters are not reflected to the effect just by calling this function,
	 * so it is necessary to finally call criAtomMic_UpdateEffectParameters to reflect the parameters.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExMic::UpdateEffectParameters'/>
	 */
	public void UpdateEffectParameters(Effect effect)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		Debug.Assert(this.handle != IntPtr.Zero, errorInvalidHandle);
		criAtomMic_UpdateEffectParameters(this.handle, effect.handle);
#endif
	}

	private uint InternalReadDataFromBufferPointers(uint numToRead)
	{
#if CRIWAREPLUGIN_SUPPORT_MIC
		return criAtomMic_ReadData(this.handle, this.bufferPointers, numToRead);
#else
		return 0;
#endif
	}

	private void InternalClearBuffers()
	{
		for (int i = 0; i < bufferPointers.Length; i++) {
			if (this.gcHandles[i].IsAllocated) {
				this.gcHandles[i].Free();
			}
			this.bufferPointers[i] = IntPtr.Zero;
		}
	}

	#region DLL Import
#if CRIWAREPLUGIN_SUPPORT_MIC
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomMicUnity_Initialize();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomMicUnity_Finalize();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomMic_GetNumDevices();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomMic_GetDevice(int index, out DeviceInfo info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomMic_GetDefaultDevice(out DeviceInfo info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomMic_IsFormatSupported([In] ref Config config);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomMic_Create([In] ref Config config, IntPtr work, int work_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomMic_Destroy(IntPtr mic);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomMic_Start(IntPtr mic);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomMic_Stop(IntPtr mic);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomMic_GetNumChannels(IntPtr mic);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomMic_GetSamplingRate(IntPtr mic);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern uint criAtomMic_GetNumBufferedSamples(IntPtr mic);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomMic_IsAvailable(IntPtr mic);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern uint criAtomMic_ReadData(IntPtr mic, IntPtr[] data, uint num_samples);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomMic_SetOutputWriteStream(IntPtr mic, IntPtr stream_cbfunc, IntPtr stream_ptr);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomMic_GetOutputReadStream();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomMic_CalculateWorkSizeForEffect(IntPtr mic,
		IntPtr afx_interface, float[] config_parameters, uint num_config_parameters);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomMic_AttachEffect(IntPtr mic,
		IntPtr afx_interface, float[] config_parameters, uint num_config_parameters,
		IntPtr work, int work_size);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomMic_DetachEffect(IntPtr mic, IntPtr effect);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomMic_GetEffectInstance(IntPtr mic, IntPtr effect);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomMic_SetEffectBypass(IntPtr mic, IntPtr effect, bool bypass);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomMic_SetEffectParameter(IntPtr mic, IntPtr effect,
		uint parameter_index, float parameter_value);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern float criAtomMic_GetEffectParameter(IntPtr mic, IntPtr effect, uint parameter_index);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomMic_UpdateEffectParameters(IntPtr mic, IntPtr effect);

#if !UNITY_EDITOR && UNITY_IOS
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomUnity_SetMicEnabled_IOS(bool enabled);
#endif
	#else
	private static void criAtomMicUnity_Initialize() { }
	private static void criAtomMicUnity_Finalize() { }
	private static int criAtomMic_GetNumDevices() { return 0; }
	private static bool criAtomMic_GetDevice(int index, out DeviceInfo info) { info = new DeviceInfo(); return false; }
	private static bool criAtomMic_GetDefaultDevice(out DeviceInfo info) { info = new DeviceInfo(); return false; }
	private static bool criAtomMic_IsFormatSupported([In] ref Config config) { return false; }
	private static IntPtr criAtomMic_Create([In] ref Config config, IntPtr work, int work_size) { return IntPtr.Zero; }
	private static void criAtomMic_Destroy(IntPtr mic) { }
	private static void criAtomMic_Start(IntPtr mic) { }
	private static void criAtomMic_Stop(IntPtr mic) { }
	private static int criAtomMic_GetNumChannels(IntPtr mic) { return 0; }
	private static int criAtomMic_GetSamplingRate(IntPtr mic) { return 0; }
	private static uint criAtomMic_GetNumBufferedSamples(IntPtr mic) { return 0u; }
	private static bool criAtomMic_IsAvailable(IntPtr mic) { return false; }
	private static uint criAtomMic_ReadData(IntPtr mic, IntPtr[] data, uint num_samples) { return 0u; }
	private static void criAtomMic_SetOutputWriteStream(IntPtr mic, IntPtr stream_cbfunc, IntPtr stream_ptr) { }
	private static IntPtr criAtomMic_GetOutputReadStream() { return IntPtr.Zero; }
	private static int criAtomMic_CalculateWorkSizeForEffect(IntPtr mic,
		IntPtr afx_interface, float[] config_parameters, uint num_config_parameters) { return 0; }
	private static IntPtr criAtomMic_AttachEffect(IntPtr mic,
		IntPtr afx_interface, float[] config_parameters, uint num_config_parameters,
		IntPtr work, int work_size) { return IntPtr.Zero; }
	private static void criAtomMic_DetachEffect(IntPtr mic, IntPtr effect) { }
	private static IntPtr criAtomMic_GetEffectInstance(IntPtr mic, IntPtr effect) { return IntPtr.Zero; }
	private static void criAtomMic_SetEffectBypass(IntPtr mic, IntPtr effect, bool bypass) { }
	private static void criAtomMic_SetEffectParameter(IntPtr mic, IntPtr effect,
		uint parameter_index, float parameter_value) { }
	private static float criAtomMic_GetEffectParameter(IntPtr mic, IntPtr effect, uint parameter_index) { return 0.0f; }
	private static void criAtomMic_UpdateEffectParameters(IntPtr mic, IntPtr effect) { }
#if !UNITY_EDITOR && UNITY_IOS
	private static void criAtomUnity_SetMicEnabled_IOS(bool enabled) { }
#endif
	#endif
#endif
	#endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
