/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System;
using System.Runtime.InteropServices;

/**
 * \addtogroup CRIWARE_COMMON_CLASS
 * @{
 */

namespace CriWare {

/**
 * <summary>A class which provides the function for setting encrypted data playback.</summary>
 * <remarks>
 * <para header='Description'>Provides the function to set the decryption when playing encrypted data.<br/>
 * The decryption function can be initialized by calling the functions provided by this class.</para>
 * </remarks>
 */
public static class CriWareDecrypter {

	/**
	 * <summary>Initialization (config specified)</summary>
	 * <param name='config'>Initialization config</param>
	 * <returns>Whether the initialization succeeded</returns>
	 * <remarks>
	 * <para header='Description'>Initializes the Decrytper.<br/>
	 * This function is called automatically when CriWareInitializer is set.<br/></para>
	 * <para header='Note'>Call this function after initializing the FileSystem library.</para>
	 * </remarks>
	 */
	public static bool Initialize(CriWareDecrypterConfig config) {
		return Initialize(config.key, config.authenticationFile,
							config.enableAtomDecryption, config.enableManaDecryption);
	}

	/**
	 * <summary>Initialization (parameter specified)</summary>
	 * <param name='key'>Encryption key</param>
	 * <param name='authenticationFile'>Authentication file path (absolute path or relative path from SreamingAssets)</param>
	 * <param name='enableAtomDecryption'>Whether to decode the sound data</param>
	 * <param name='enableManaDecryption'>Whether to decrypt the movie data</param>
	 * <returns>Whether the initialization succeeded</returns>
	 * <remarks>
	 * <para header='Description'>Initializes the Decrytper.<br/>
	 * This function is called automatically when CriWareInitializer is set.<br/></para>
	 * <para header='Note'>Call this function after initializing the FileSystem library.</para>
	 * </remarks>
	 */
	public static bool Initialize(string key, string authenticationFile, bool enableAtomDecryption, bool enableManaDecryption) {
		if (!CriFsPlugin.IsLibraryInitialized()) {
			return false;
		}

		/* バージョン番号が不正なライブラリには暗号キーを伝えない */
		/* 備考）不正に差し替えられたsoファイルを使用している可能性あり。 */
		bool isCorrectVersion = CriWare.Common.CheckBinaryVersionCompatibility();
		if (isCorrectVersion == false) {
			return false;
		}

		ulong decryptionKey = (key.Length == 0) ? 0 : System.Convert.ToUInt64(key);
		string authenticationPath = authenticationFile;
		if (CriWare.Common.IsStreamingAssetsPath(authenticationPath)) {
			authenticationPath = System.IO.Path.Combine(CriWare.Common.streamingAssetsPath, authenticationPath);
		}

		temporalStorage = decryptionKey ^ 0x00D47EB533AEF7E5UL;
		CRIWAREB9DA24BC(enableAtomDecryption, enableManaDecryption, CallbackFromNative, IntPtr.Zero);
		temporalStorage = 0;

		return true;
	}

	/* 変数の一時的な格納場所 */
	private static ulong temporalStorage = 0;

	#region Private Methods
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate ulong CallbackFromNativeDelegate(System.IntPtr ptr1);

	[AOT.MonoPInvokeCallback(typeof(CallbackFromNativeDelegate))]
	private static ulong CallbackFromNative(System.IntPtr ptr1)
	{
		return temporalStorage;
	}
	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern int CRIWAREB9DA24BC(
		bool enable_atom_decryption, bool enable_mana_decryption, CallbackFromNativeDelegate func, IntPtr obj
	);
	#else
	public static int CRIWAREB9DA24BC(
		bool enable_atom_decryption, bool enable_mana_decryption, CallbackFromNativeDelegate func, IntPtr obj
		) { return 0; }
	#endif
	#endregion
} // end of class

} //namespace CriWare
/** @} */

/* --- end of file --- */
