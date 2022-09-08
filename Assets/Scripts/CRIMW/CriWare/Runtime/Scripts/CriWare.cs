/****************************************************************************
 *
 * Copyright (c) 2012 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/**
 * \addtogroup CRIWARE_COMMON_CLASS
 * @{
 */

namespace CriWare {

/**
 * <summary>A class which provides auxiliary functions related to the CRIWARE plug-in.</summary>
 * <remarks>
 * <para header='Description'>Provides an auxiliary method that can be commonly used in platforms.<br/>
 * By using the properties and methods of this class, you can get a path to a special data folder
 * or check the CPU /memory usage by CRIWARE plug-in.</para>
 * </remarks>
 */

public class Common
{
	/* スクリプトバージョン */
	private const string scriptVersionString = "2.39.68";
	private const int scriptVersionNumber = 0x02396800;

	/**
	 * <summary>Whether CriFsInstaller API is supported or not</summary>
	 * <remarks>
	 * <para header='Description'>Used to determine if the CriFsInstaller API is available on the execution environment.</para>
	 * </remarks>
	 */
	public const bool supportsCriFsInstaller =
	#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_IOS || UNITY_TVOS || UNITY_ANDROID || UNITY_WINRT
		true;
	#else
		false;
	#endif

	/**
	 * <summary>Whether CriFsWebInstaller API is supported or not</summary>
	 * <remarks>
	 * <para header='Description'>Used to determine if the CriFsWebInstaller API is available on the execution environment.</para>
	 * </remarks>
	 */
	public const bool supportsCriFsWebInstaller =
	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID
		true;
	#else
		false;
	#endif

	/* ネイティブライブラリ */
	#if UNITY_EDITOR
		public const string pluginName = "cri_ware_unity";
	#elif UNITY_IOS || UNITY_TVOS || UNITY_WEBGL || UNITY_SWITCH
		public const string pluginName = "__Internal";
	#else
		public const string pluginName = "cri_ware_unity";
	#endif

    #if ENABLE_IL2CPP && (UNITY_STANDALONE_WIN || UNITY_WINRT)
        public const CallingConvention pluginCallingConvention = CallingConvention.Cdecl;
    #else
        public const CallingConvention pluginCallingConvention = CallingConvention.Winapi;  /* default */
    #endif

	/**
	 * <summary>The path of the StreamingAssets folder.</summary>
	 * <remarks>
	 * <para header='Description'>This property returns the StreamingAssets folder path. The value cannot be set.</para>
	 * <para header='Note'>In Android environment, this property returns an empty string.
	 * When accessing the files in StreamingAssets using the function of the CRIWARE plug-in,
	 * directly specify the relative path below StreamingAssets in Android environment only.
	 * Note that "/" is not added to the beginning of the path.</para>
	 * </remarks>
	 */
	public static string streamingAssetsPath
	{
		get {
			if (Application.platform == RuntimePlatform.Android) {
				return "";
			}
#if UNITY_SWITCH
			else if (Application.platform == RuntimePlatform.Switch) {
				return "rom:/Data/StreamingAssets";
			}
#elif UNITY_PSP2
			else if (Application.platform == RuntimePlatform.PSP2) {
				string ret = Application.streamingAssetsPath;
				if (ret.StartsWith("/app0/")) {
					var regex = new System.Text.RegularExpressions.Regex("/app0/");
					ret = regex.Replace(ret, "app0:/", 1);
				}
				return ret;
			}
#endif
			else {
				return Application.streamingAssetsPath;
			}
		}
	}

	/**
	 * <summary>The path of the data folder.</summary>
	 * <remarks>
	 * <para header='Description'>This property returns the data folder path. The value cannot be set.</para>
	 * <para header='Note'>In the case of iOS environment, writing files in this folder
	 * may cause problems in AppStore examination.<br/></para>
	 * </remarks>
	 */
	public static string installTargetPath
	{
		get {
	#if !UNITY_EDITOR && UNITY_IOS
			return Application.temporaryCachePath;
	#elif UNITY_EDITOR || !UNITY_SWITCH
			return Application.persistentDataPath;
	#else
			return null;
	#endif
		}
	}

	/**
	 * <summary>The path of the cache data folder.</summary>
	 * <remarks>
	 * <para header='Description'>Returns the path of the cache data folder.<br/>
	 * The files under the directory returned by this property are writable (from the application).</para>
	 * <para header='Note'>Returns a path which may not be persistent on some platforms. <br/>
	 * Whether the path returned by this property is persistent depends on whether AssetBundle caching is enabled / disabled for each platform.</para>
	 * </remarks>
	 */
	public static string installCachePath
	{
		get {
#if !UNITY_EDITOR && UNITY_SWITCH
			return string.Format("{0}:/cri", CriWareSwitch.TemporaryStorageName);
#else // UNITY_EDITOR && UNITY_SWITCH
#if ENABLE_CACHING || !UNITY_2018_4_OR_NEWER
			return Caching.currentCacheForWriting.path;
#else // ENABLE_CACHING || !UNITY_2018_4_ORNEWER
			return Application.temporaryCachePath;
#endif // ENABLE_CACHING || !UNITY_2018_4_ORNEWER
#endif // UNITY_EDITOR && UNITY_SWITCH
		}
	}

	/**
	 * <summary>Determines whether it is used as a relative path from the StreamingAssets folder</summary>
	 * <param name='path'>File path</param>
	 * <returns>Whether it is used as a relative path from the StreamingAssets folder</returns>
	 * <remarks>
	 * <para header='Description'>Determines whether the path is used as a relative path from the StreamingAssets folder in the CRIWARE plug-in.<br/>
	 * The CRIWARE plug-in uses the path that meets all the following conditions as a relative path from the StreamingAssets folder.
	 *   - Is not an absolute path
	 *   - Does not contain ':'</para>
	 * <para header='Note'>Refer to the reference of each API to see if it is actually used as a relative path from the StreamingAssets folder.</para>
	 * </remarks>
	 */
	public static bool IsStreamingAssetsPath(string path)
	{
		return !Path.IsPathRooted(path) && (path.IndexOf(':') < 0);
	}

	private static GameObject _managerObject = null;
	public static GameObject managerObject
	{
		get {
			if (_managerObject == null) {
				_managerObject = GameObject.Find("/CRIWARE");
				if (_managerObject == null) {
					_managerObject = new GameObject("CRIWARE");
				}
				UnityEngine.Object.DontDestroyOnLoad(_managerObject);
			}
			return _managerObject;
		}
	}

	/**
	 * <summary>Gets script version string</summary>
	 * <remarks>
	 * <para header='Description'>This method returns the script version string of CRIWARE.</para>
	 * </remarks>
	 */
	 public static string GetScriptVersionString() {
		return scriptVersionString;
	}

	/**
	 * <summary>Gets script version number</summary>
	 * <remarks>
	 * <para header='Description'>This method returns the script version number of CRIWARE.</para>
	 * </remarks>
	 */
	public static int GetScriptVersionNumber() {
		return scriptVersionNumber;
	}

	/**
	 * <summary>Gets the binary version number</summary>
	 * <remarks>
	 * <para header='Description'>This method returns the binary version number of CRIWARE.
	 * The binary here means the library file (.dll etc.) included in the CRIWARE plug-in.</para>
	 * </remarks>
	 */
	public static int GetBinaryVersionNumber() {
		return CRIWAREC9847394();
	}

	/**
	 * <summary>Gets the binary version required by the script</summary>
	 * <remarks>
	 * <para header='Description'>This method returns the runtime version number required by the CRIWARE script.</para>
	 * </remarks>
	 */
	public static int GetRequiredBinaryVersionNumber() {
#if true
		return 0x02396800;
#else
#if UNITY_EDITOR
		switch (Application.platform) {
			case RuntimePlatform.WindowsEditor:
				return 0x02396800;
			case RuntimePlatform.OSXEditor:
				return 0x02396800;
			case RuntimePlatform.LinuxEditor:
				return 0x02396800;
			default:
				return 0x02396800;
		}
#elif UNITY_STANDALONE_WIN
		return 0x02396800;
#elif UNITY_STANDALONE_OSX
		return 0x02396800;
#elif UNITY_IOS
		return 0x02396800;
#elif UNITY_TVOS
		return 0x02396800;
#elif UNITY_ANDROID
		return 0x02396800;
#elif UNITY_PSP2
		return 0x02396800;
#elif UNITY_PS3
		return 0x02396800;
#elif UNITY_PS4
		return 0x02396800;
#elif UNITY_PS5
		return 0x02396800;
#elif UNITY_XBOXONE
		return 0x02396800;
#elif UNITY_GAMECORE_XBOXONE
		return 0x02396800;
#elif UNITY_GAMECORE_SCARLETT
		return 0x02396800;
#elif UNITY_WINRT
		return 0x02396800;
#elif UNITY_WEBGL
		return 0x02396800;
#elif UNITY_SWITCH
		return 0x02396800;
#elif UNITY_STADIA
        return 0x02396800;
#elif UNITY_STANDALONE_LINUX
		return 0x02396800;
#else
		return 0x02396800;
#endif
#endif
    }

    /**
	 * <summary>Checks the consistency between binary version and script version</summary>
	 * <remarks>
	 * <para header='Description'>This method checks if the current binary matches the version number required by the script.<br/>
	 * If they match, the integrity check is considered successful and True is returned.<br/>
	 * If they don't match, it is considered as a failure, an error message is output to the console, and False is returned.</para>
	 * </remarks>
	 */
    public static bool CheckBinaryVersionCompatibility() {
		if (GetBinaryVersionNumber() == GetRequiredBinaryVersionNumber()) {
			return true;
		} else {
			Debug.LogError("CRI runtime library is not compatible. Confirm the version number.");
			return false;
		}
	}

	/**
	 * <summary>Gets the memory usage of CRI FileSystem</summary>
	 * <remarks>
	 * <para header='Description'>This method returns the memory usage of CRI FileSystem.</para>
	 * </remarks>
	 */
	public static uint GetFsMemoryUsage()
	{
		return CriFsPlugin.CRIWARE4A9B2613();
	}

	/**
	 * <summary>Gets the memory usage of CRI Atom</summary>
	 * <remarks>
	 * <para header='Description'>This method returns the memory usage of CRI Atom.</para>
	 * </remarks>
	 */
	public static uint GetAtomMemoryUsage()
	{
		return CriAtomPlugin.CRIWARE581F45FE();
	}

	/**
	 * <summary>Gets the memory usage of CRI Mana</summary>
	 * <remarks>
	 * <para header='Description'>This method returns the memory usage of CRI Mana.</para>
	 * </remarks>
	 */
	public static uint GetManaMemoryUsage()
	{
#if !UNITY_EDITOR && UNITY_WEBGL
        return 0;
#else
		return CriManaPlugin.CRIWARE065392A8();
#endif
	}

	/**
	 * <summary>CPU usage of the CRIWARE plug-in</summary>
	 */
	public struct CpuUsage
	{
		public float last;
		public float average;
		public float peak;
	}

	/**
	 * <summary>Gets the CPU usage of the CRIWARE plug-in</summary>
	 * <remarks>
	 * <para header='Description'>This method returns the CPU usage by the native library of
	 * CRIWARE plug-in. The return value is a CpuUsage structure.</para>
	 * </remarks>
	 */
	 public static CpuUsage GetAtomCpuUsage()
	{
		return CriAtomPlugin.GetCpuUsage();
	}

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(pluginName, CallingConvention = pluginCallingConvention)]
	public static extern int CRIWAREC9847394();

	[DllImport(pluginName, CallingConvention = pluginCallingConvention)]
	public static extern void criWareUnity_SetRenderingEventOffsetForMana(int offset);
	#else
	public static int CRIWAREC9847394() { return GetRequiredBinaryVersionNumber(); }
	public static void criWareUnity_SetRenderingEventOffsetForMana(int offset) { }
	#endif
	#endregion

} // end of class

} //namespace CriWare
/** @} */

/* --- end of file --- */
