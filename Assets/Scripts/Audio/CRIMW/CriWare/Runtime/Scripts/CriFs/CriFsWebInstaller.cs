/****************************************************************************
 *
 * Copyright (c) 2016 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
	#define CRIFSWEBINSTALLER_SUPPORTED
#endif

using UnityEngine;
using System;
using System.Runtime.InteropServices;


/**
 * \addtogroup CRIFS_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * <summary>A module which performs installation to the local storage by HTTP.</summary>
 * <remarks>
 * <para header='Description'>Used to install content on the Web server to the local storage.</para>
 * <para header='Note'>On iOS, this function works on iOS7 or later.</para>
 * <para header='Note'>Before creating an instance of CriWare.CriFsWebInstaller, the module must be initialized by calling the CriWare.CriFsWebInstaller::InitializeModule method.</para>
 * </remarks>
 */
public class CriFsWebInstaller : CriDisposable
{
	#region Data Types
	/**
	 * <summary>Status</summary>
	 * <seealso cref='CriFsWebInstaller::GetStatusInfo'/>
	 */
	public enum Status : int
	{
		Stop,       /**< Stopped */
		Busy,       /**< Processing in progress */
		Complete,   /**< Completed */
		Error,      /**< Error */
	}

	/**
	 * <summary>Error type</summary>
	 * <remarks>
	 * <para header='Description'>Indicates the error type of the installer handle.<br/>
	 * Information can be obtained using the CriWare.CriFsWebInstaller::GetStatusInfo function.</para>
	 * </remarks>
	 * <seealso cref='CriFsWebInstaller::GetStatusInfo'/>
	 */
	public enum Error : int
	{
		None,       /**< No error */
		Timeout,    /**< Timeout error */
		Memory,     /**< Memory allocation failed */
		LocalFs,    /**< Local file system error */
		DNS,        /**< DNS error */
		Connection, /**< Connection error */
		SSL,        /**< SSL error */
		HTTP,       /**< HTTP error */
		Internal,   /**< Internal error */
	}

	/**
	 * <summary>Status information</summary>
	 * <remarks>
	 * <para header='Description'>Represents the detailed status including CriWare.CriFsWebInstaller::Status,<br/>
	 * which can be obtained by calling the CriWare.CriFsWebInstaller::GetStatusInfo function.</para>
	 * </remarks>
	 * <seealso cref='CriFsWebInstaller::StatusInfo'/>
	 * <seealso cref='CriFsWebInstaller::GetStatusInfo'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct StatusInfo
	{
		/**
		 * <summary>Installer handle status</summary>
		 * <seealso cref='CriFsWebInstaller::Status'/>
		 */
		public Status status;

		/**
		 * <summary>Error state of the installer handle</summary>
		 * <remarks>
		 * <para header='Description'>A value other than CriFsWebInstaller::Error.None is stored
		 * when CriFsWebInstaller::StatusInfo.status != CriFsWebInstaller::Status.Error.<br/>
		 * When an error occurs, handle appropriately according to the error type.</para>
		 * </remarks>
		 * <seealso cref='CriFsWebInstaller::Error'/>
		 */
		public Error error;

		/**
		 * <summary>HTTP status code</summary>
		 * <remarks>
		 * <para header='Description'>HTTP status code is stored in either of the following cases.<br/>
		 *   - CriFsWebInstaller::StatusInfo.status == CriFsWebInstaller::Status.Complete<br/>
		 *   - CriFsWebInstaller::StatusInfo.status == CriFsWebInstaller::Status.Error and
		 *     CriFsWebInstaller::StatusInfo.error == CriFsWebInstaller::Error.HTTP<br/>
		 * 
		 * In other cases, a negative value ( CriFsWebInstaller.InvalidHttpStatusCode ) is stored.</para>
		 * </remarks>
		 * <seealso cref='CriFsWebInstaller.InvalidHttpStatusCode'/>
		 */
		public int httpStatusCode;

		/**
		 * <summary>The size of the installation target (byte)</summary>
		 * <remarks>
		 * <para header='Description'>The size of the installation target (byte) is stored.<br/>
		 * A negative value ( CriFsWebInstaller.InvalidContentsSize ) is stored if the size of the installation target is unknown.<br/>
		 * A valid value is stored when the transfer via HTTP starts.</para>
		 * </remarks>
		 * <seealso cref='CriFsWebInstaller.InvalidContentsSize'/>
		 * <seealso cref='CriFsWebInstaller::StatusInfo.receivedSize'/>
		 */
		public long contentsSize;

		/**
		 * <summary>Received size (byte)</summary>
		 * <seealso cref='CriFsWebInstaller::StatusInfo.contentsSize'/>
		 */
		public long receivedSize;
	}

	/**
	 * <summary>Module configuration</summary>
	 * <remarks>
	 * <para header='Description'>A structure for specifying the behavior of CriFsWebInstaller.<br/>
	 * You pass this structure as an argument when initializing a module (the CriWare.CriFsWebInstaller::InitializeModule function).<br/></para>
	 * <para header='Note'>Please get the default configuration with CriWare.CriFsWebInstaller::defaultModuleConfig , change it as needed, and pass it to the CriWare.CriFsWebInstaller::InitializeModule function. <br/></para>
	 * </remarks>
	 * <seealso cref='CriFsWebInstaller::InitializeModule'/>
	 * <seealso cref='CriFsWebInstaller::defaultModuleConfig'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct ModuleConfig
	{
		/**
		 * <summary>Sets the maximum number of installers used at the same time</summary>
		 * <remarks>
		 * <para header='Description'>CriFsWebInstallers cannot be generated at the same time exceeding this number.</para>
		 * </remarks>
		 */
		public uint numInstallers;

		[MarshalAs(UnmanagedType.LPStr)]
		/**
		 * <summary>Sets the HTTP proxy server host name</summary>
		 * <remarks>
		 * <para header='Description'>Set the host name of the proxy server used by CriFsWebInstaller.<br/>
		 * If set to null, proxy server will not be used.</para>
		 * </remarks>
		 */
		public string proxyHost;

		/**
		 * <summary>Sets the HTTP proxy server port</summary>
		 * <remarks>
		 * <para header='Description'>Set the port of the proxy server used by CriFsWebInstaller.<br/>
		 * This value has an effect only if CriFsWebInstaller::ModuleConfig.proxyHost != null.</para>
		 * </remarks>
		 */
		public ushort proxyPort;

		/**
		 * <summary>User-Agent setting</summary>
		 * <remarks>
		 * <para header='Description'>Set when overwriting the default User-Agent.
		 * If set to null, the default User-Agent is used.</para>
		 * </remarks>
		 */
		[MarshalAs(UnmanagedType.LPStr)]
		public string userAgent;

		/**
		 * <summary>Sets the timeout time (in seconds)</summary>
		 * <remarks>
		 * <para header='Description'>A timeout error ( CriFsWebinstaller::Error.Timeout ) occurs if the received size does not change over this time.</para>
		 * </remarks>
		 * <seealso cref='CriFsWebInstaller::StatusInfo.error'/>
		 * <seealso cref='CriFsWebInstaller::Error.Timeout'/>
		 */
		public uint inactiveTimeoutSec;

		/**
		 * <summary>Enables the insecure HTTPS communication</summary>
		 * <remarks>
		 * <para header='Description'>Insecure HTTPS communication is allowed when this flag is set to true.<br/>
		 * Please set it to true only if a valid server certificate cannot be provided during the application's development.</para>
		 * <para header='Note'>  - To allow insecure HTTPS internet connection on Apple platforms,
		 *     set this flag to true, and either disable ATS (App Transport Security)
		 *     or set up for exceptions.</para>
		 * </remarks>
		 */
		public bool allowInsecureSSL;

		/* <summary>CRCの有効化</summary>
		 * \par 説明：
		 * CRI_TRUE の場合のみ、CRCの計算をします。
		 */
		public bool crcEnabled;

		/**
		 * <summary>Platform-specific settings</summary>
		 */
		public ModulePlatformConfig platformConfig;
	}

	#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
	public struct ModulePlatformConfig
	{
		public byte reserved;

		public static ModulePlatformConfig defaultConfig {
			get {
				ModulePlatformConfig config;
				config.reserved = 0;
				return config;
			}
		}
	}
	#elif UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX)
	public struct ModulePlatformConfig
	{
		public byte reserved;

		public static ModulePlatformConfig defaultConfig {
			get {
				ModulePlatformConfig config;
				config.reserved = 0;
				return config;
			}
		}
	}
	#elif UNITY_IOS
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct ModulePlatformConfig
	{
		public byte reserved;

		public static ModulePlatformConfig defaultConfig {
			get {
				ModulePlatformConfig config;
				config.reserved = 0;
				return config;
			}
		}
	}
	#elif UNITY_ANDROID
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct ModulePlatformConfig
	{
		public byte reserved;

		public static ModulePlatformConfig defaultConfig {
			get {
				ModulePlatformConfig config;
				config.reserved = 0;
				return config;
			}
		}
	}
	#else
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct ModulePlatformConfig
	{
		public static ModulePlatformConfig defaultConfig {
			get {
				ModulePlatformConfig config;
				return config;
			}
		}
	}
	#endif
	#endregion

	#region Static Properties
	public static bool isInitialized { get; private set; }
	public static bool isCrcEnabled { get; private set; }

	/**
	 * <summary>Default module configuration</summary>
	 * <remarks>
	 * <para header='Description'>Default module config.</para>
	 * <para header='Note'>Change the default configuration obtained using this property
	 * and specify it in the CriWare.CriFsWebInstaller::InitializeModule function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsWebInstaller::InitializeModule'/>
	 */
	public static ModuleConfig defaultModuleConfig {
		get {
			ModuleConfig config;
			config.numInstallers        = 2;
			config.proxyHost            = null;
			config.proxyPort            = 0;
			config.userAgent            = null;
			config.inactiveTimeoutSec   = 300;
			config.allowInsecureSSL     = false;
			config.crcEnabled           = false;
			config.platformConfig       = ModulePlatformConfig.defaultConfig;
			return config;
		}
	}
	#endregion

	#region Constant Variables
	/**
	 * <summary>Invalid HTTP status code</summary>
	 * <remarks>
	 * <para header='Description'>A constant that represents an invalid HTTP status code.<br/>
	 * It is set when the installation fails due to a reason other than HTTP.<br/>
	 * This value is guaranteed to be negative.</para>
	 * </remarks>
	 * <seealso cref='CriFsWebInstaller::StatusInfo.httpStatusCode'/>
	 */
	public const int    InvalidHttpStatusCode   = -1;

	/**
	 * <summary>Invalid content size</summary>
	 * <remarks>
	 * <para header='Description'>It is set when the size of the installation target cannot be acquired.<br/>
	 * This value is guaranteed to be negative.</para>
	 * </remarks>
	 * <seealso cref='CriFsWebInstaller::StatusInfo.contentsSize'/>
	 */
	public const long   InvalidContentsSize     = -1;
	#endregion


	#if CRIFSWEBINSTALLER_SUPPORTED
	#region Private Variables
	private IntPtr  handle      = IntPtr.Zero;
	#endregion

	public CriFsWebInstaller()
	{
		criFsWebInstaller_Create(out this.handle);
		if (this.handle == IntPtr.Zero)
		{
			throw new Exception("criFsWebInstaller_Create() failed.");
		}
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.FsWeb);
	}

	~CriFsWebInstaller()
	{
		this.Dispose(false);
	}

	/**
	 * <summary>Discards the installer.</summary>
	 * <remarks>
	 * <para header='Note'>If you discard the installer during the installation process,
	 * the process may be blocked in this function for a long time.<br/></para>
	 * </remarks>
	 */
	public override void Dispose()
	{
		this.Dispose(true);
		System.GC.SuppressFinalize(this);
	}


	/**
	 * <summary>Installs the file.</summary>
	 * <param name='url'>Installation source URL</param>
	 * <param name='dstPath'>Installation destination file path name</param>
	 * <remarks>
	 * <para header='Description'>Starts installing files.<br/>
	 * This function returns immediately.<br/>
	 * To get the copy completion status, use the CriWare.CriFsWebInstaller::GetStatusInfo function.</para>
	 * <para header='Note'>  - If the installation destination file exists, error CriFsWebInstaller.Error.LocalFs occurs.
	 *   - If the installation destination folder does not exist, the error CriFsWebInstaller.Error.LocalFs will occur.</para>
	 * </remarks>
	 * <seealso cref='CriFsWebInstaller::GetStatusInfo'/>
	 */
	public void Copy(string url, string dstPath)
	{
		criFsWebInstaller_Copy(this.handle, url, dstPath);
	}

	/**
	 * <summary>Stops the installation process.</summary>
	 * <remarks>
	 * <para header='Description'>Stops the processing.<br/>
	 * This function returns immediately.<br/>
	 * Use the CriWare.CriFsWebInstaller::GetStatusInfo function to get the stop completion status.</para>
	 * </remarks>
	 * <seealso cref='CriFsInstaller::GetStatus'/>
	 */
	public void Stop()
	{
		if (this.handle != IntPtr.Zero) {
			criFsWebInstaller_Stop(this.handle);
		}
	}

	/**
	 * <summary>Gets the status information.</summary>
	 * <returns>Status information</returns>
	 * <seealso cref='CriFsWebInstaller::StatusInfo'/>
	 */
	public StatusInfo GetStatusInfo()
	{
		StatusInfo statusInfo;
		if (this.handle != IntPtr.Zero) {
			criFsWebInstaller_GetStatusInfo(this.handle, out statusInfo);
		} else {
			statusInfo.status   = Status.Stop;
			statusInfo.error    = Error.Internal;
			statusInfo.httpStatusCode   = InvalidHttpStatusCode;
			statusInfo.contentsSize     = InvalidContentsSize;
			statusInfo.receivedSize     = 0;
		}
		return statusInfo;
	}

	/**
	 * <summary>Acquire the CRC32 calculation result</summary>
	 * <param name='ret_val'>For CRC result storage</param>
	 * <remarks>
	 * <para header='Description'>Returns a checksum that is valid only in the Status.Complete state. <br/>
	 * If it is acquired in a state other than Status.Complete, the CRC result will be 0. <br/>
	 * This function can be used only when ModuleConfig.crcEnabled=true . <br/></para>
	 * </remarks>
	 * <seealso cref='CriFsWebInstaller::StatusInfo'/>
	 */
	public bool GetCRC32(out uint ret_val){
		int criErr = criFsWebInstaller_GetCRC32(this.handle, out ret_val);
		// '0' means "OK".
		return (criErr == 0);
	}

	#region Static Methods
	/**
	 * <summary>Initializes the CriFsWebInstaller module</summary>
	 * <param name='config'>Configuration</param>
	 * <remarks>
	 * <para header='Description'>Initializes the CriFsWebInstaller module.<br/>
	 * In order to use the function of the module, you must call this function.<br/>
	 * (You can use the features of the module after calling this function
	 *  and before calling the CriWare.CriFsWebInstaller::FinalizeModule function.)<br/></para>
	 * <para header='Note'>After calling this function, be sure to call the corresponding CriWare.CriFsWebInstaller::FinalizeModule function.<br/>
	 * In addition, this function cannot be called again until the CriWare.CriFsWebInstaller::FinalizeModule function is called.<br/></para>
	 * </remarks>
	 * <seealso cref='CriFsWebInstaller::ModuleConfig'/>
	 * <seealso cref='CriFsWebInstaller::FinalizeModule'/>
	 */
	public static void InitializeModule(ModuleConfig config)
	{
		if (isInitialized) {
			UnityEngine.Debug.LogError("[CRIWARE] CriFsWebInstaller module is already initialized.");
			return;
		}
		Type type = GetCriFsWebInstallerCurlExpansionClass();
		if (type != null) {
			System.Reflection.MethodInfo method_setup_curl_context = type.GetMethod("SetupCurlContext");
			if (method_setup_curl_context == null) {
				UnityEngine.Debug.LogError("[CRIWARE] ERROR: CriFsWebInstallerCurl.SetupCurlContext method is not found.");
			} else {
					method_setup_curl_context.Invoke(null, new object[]{true});
			}
		}
		CriFsPlugin.InitializeLibrary();
		criFsWebInstaller_Initialize(ref config);
		isCrcEnabled = config.crcEnabled;
		isInitialized = true;
	}

	private static Type GetCriFsWebInstallerCurlExpansionClass() {
		Type type = Type.GetType("CriWare.CriFsWebInstallerCurl, CriMw.CriWare.FsModuleCurl.Runtime");
		return type;
	}

	/**
	 * <summary>Terminates the CriFsWebInstaller module</summary>
	 * <remarks>
	 * <para header='Description'>Terminates the CriFsWebInstaller module.<br/></para>
	 * <para header='Note'>  - This function cannot be called before calling the CriWare.FsWebInstaller::InitializeModule function.<br/>
	 *   - All CriWare.FsWebInstaller must be discarded.</para>
	 * </remarks>
	 * <seealso cref='CriFsWebInstaller::InitializeModule'/>
	 */
	public static void FinalizeModule()
	{
		if (!isInitialized) {
			UnityEngine.Debug.LogError("[CRIWARE] CriFsWebInstaller module is not initialized.");
			return;
		}
		CriDisposableObjectManager.CallOnModuleFinalization(CriDisposableObjectManager.ModuleType.FsWeb);
		criFsWebInstaller_Finalize();
		CriFsPlugin.FinalizeLibrary();
		isInitialized = false;
	}

	/**
	 * <summary>Runs the server process</summary>
	 * <remarks>
	 * <para header='Description'>Executes the server processing. It should be run regularly.<br/></para>
	 * </remarks>
	 */
	public static void ExecuteMain()
	{
		criFsWebInstaller_ExecuteMain();
	}

	/**
	 * <summary>Changes the information in the HTTP request header.</summary>
	 * <param name='field'>Field name</param>
	 * <param name='value'>Field value</param>
	 * <remarks>
	 * <para header='Description'>Changes the information in the HTTP request header.<br/>
	 * This function must be called after calling the CriWare.CriFsWebInstaller::InitializeModule function.<br/>
	 * Call this function before invoking the installation.<br/>
	 * If the field name is already registered, the field value will be overwritten.<br/>
	 * If null is passed as the field value, the field is removed.<br/></para>
	 * </remarks>
	 */
	public static bool SetRequestHeader(string field, string value){
		int ret = criFsWebInstaller_SetRequestHeader(field, value);
		return (ret == 0);
	}
	#endregion

	#region Private Methods
	private void Dispose(bool disposing)
	{
		CriDisposableObjectManager.Unregister(this);

		if (this.handle != IntPtr.Zero) {
			var statusInfo = this.GetStatusInfo();
			if (statusInfo.status != Status.Stop) {
				this.Stop();
				while (true) {
					ExecuteMain();
					statusInfo = this.GetStatusInfo();
					if (statusInfo.status == Status.Stop) {
						break;
					}
					System.Threading.Thread.Sleep(1);
				}
			}
			criFsWebInstaller_Destroy(this.handle);
			this.handle = IntPtr.Zero;
		}
	}
	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsWebInstaller_Initialize([In] ref ModuleConfig config);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsWebInstaller_Finalize();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsWebInstaller_ExecuteMain();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsWebInstaller_Create(out IntPtr installer);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsWebInstaller_Destroy(IntPtr installer);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsWebInstaller_Copy(IntPtr installer, string url, string dstPath);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsWebInstaller_Stop(IntPtr installer);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsWebInstaller_GetStatusInfo(IntPtr installer, out StatusInfo status);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsWebInstaller_GetCRC32(IntPtr installer, out uint crc32);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criFsWebInstaller_SetRequestHeader (string field, string value);

	#else
	private static int criFsWebInstaller_Initialize([In] ref ModuleConfig config) { return 0; }
	private static int criFsWebInstaller_Finalize() { return 0; }
	private static int criFsWebInstaller_ExecuteMain() { return 0; }
	private static int criFsWebInstaller_Create(out IntPtr installer) { installer = new IntPtr(1); return 0; }
	private static int criFsWebInstaller_Destroy(IntPtr installer) { return 0; }
	private static int criFsWebInstaller_Copy(IntPtr installer, string url, string dstPath) { return 0; }
	private static int criFsWebInstaller_Stop(IntPtr installer) { return 0; }
	private static int criFsWebInstaller_GetStatusInfo(IntPtr installer, out StatusInfo status) { status = new StatusInfo(); return 0; }
	private static int criFsWebInstaller_GetCRC32(IntPtr installer, out uint crc32) { crc32 = 0u; return 0; }
	private static int criFsWebInstaller_SetRequestHeader (string field, string value){ return 0; }
	#endif
	#endregion

	#else
	#region Internal Variables
	private bool errorOccured = false;
	#endregion

	public CriFsWebInstaller()
	{
		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Fs);
	}

	~CriFsWebInstaller()
	{
		this.Dispose(false);
	}

	public override void Dispose()
	{
		CriDisposableObjectManager.Unregister(this);
		this.Dispose(true);
		System.GC.SuppressFinalize(this);
	}

	public void Copy(string url, string dstPath)
	{
		Debug.LogError("[CRIWARE] CriWebInstaller does not support this platform.");
		errorOccured = true;
	}

	public void Stop()
	{
		errorOccured = false;
	}

	public StatusInfo GetStatusInfo()
	{
		StatusInfo statusInfo;
		if (errorOccured) {
			statusInfo.status   = Status.Error;
			statusInfo.error    = Error.None;
		} else {
			statusInfo.status   = Status.Stop;
			statusInfo.error    = Error.Internal;
		}
		statusInfo.httpStatusCode   = InvalidHttpStatusCode;
		statusInfo.contentsSize     = InvalidContentsSize;
		statusInfo.receivedSize     = 0;
		return statusInfo;
	}

	public bool GetCRC32(out uint ret_val){
		ret_val = 0;
		return false;
	}

	#region Static Methods
	public static void InitializeModule(ModuleConfig config)
	{
		if (isInitialized) {
			UnityEngine.Debug.LogError("[CRIWARE] CriFsWebInstaller module is already initialized.");
			return;
		}
		CriFsPlugin.InitializeLibrary();
		isInitialized = true;
	}

	public static void FinalizeModule()
	{
		if (!isInitialized) {
			UnityEngine.Debug.LogError("[CRIWARE] CriFsWebInstaller module is not initialized.");
			return;
		}
		CriFsPlugin.FinalizeLibrary();
		isInitialized = false;
	}

	public static void ExecuteMain()
	{
	}
	#endregion

	#region Private Methods
	private void Dispose(bool disposing)
	{
	}

	private void UnsupportedError()
	{
	}
	#endregion
	#endif
}

} //namespace CriWare
/**
 * @}
 */
