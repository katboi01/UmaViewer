/****************************************************************************
 *
 * Copyright (c) CRI Middleware Co., Ltd.
 *
 ****************************************************************************/
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
#define CRIWAREPLUGIN_SUPPORT_OUTPUTDEVICE_OBSERVER
#endif

using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

/**
 * \addtogroup CRIWARE_COMMON_CLASS
 * @{
 */

namespace CriWare {


/**
 * <summary>A component that monitors the connection status of audio output devices.</summary>
 * <remarks>
 * <para header='Description'>Use it by adding it to any GameObject.<br/>
 * You can monitor the connection status of the audio output device on your smartphone device and acquire the status externally.<br/>
 * By registering a delegate, you can also receive callbacks when the connection status changes.<br/>
 * To use this component, it is necessary to initialize the Atom library first.</para>
 * <para header='Note'>Currently, the functions of this component only work on smartphones (Android / iOS).<br/>
 * Please wait for future updates for support for other platforms.</para>
 * </remarks>
 */
public class CriAtomOutputDeviceObserver : CriMonoBehaviour
{
	/**
	 * <summary>Type of audio output device</summary>
	 * <remarks>
	 * <para header='Description'>Types of device to which audio is output from the application.</para>
	 * </remarks>
	 * <seealso cref='CriAtomOutputDeviceObserver::DeviceType'/>
	 */
	public enum OutputDeviceType {
		BuiltinSpeaker,     /**< Internal Speaker */
		WiredDevice,        /**< Wired device (wired headset, etc.) */
		WirelessDevice,     /**< Wireless device (Bluetooth headset, etc.) */
	}


	/**
	 * <summary>Connection state change callback delegate type</summary>
	 * <param name='isConnected'>Output device connection status (false = disconnected, true = connected)</param>
	 * <param name='deviceType'>Output device type</param>
	 * <remarks>
	 * <para header='Description'>This is the type of the callback function that is called when the connection status of the audio output device changes.</para>
	 * </remarks>
	 * <seealso cref='CriAtomOutputDeviceObserver::OnDeviceConnectionChanged'/>
	 */
	public delegate void DeviceConnectionChangeCallback(bool isConnected, OutputDeviceType deviceType);


	/**
	 * <summary>Connection state change callback delegate</summary>
	 * <remarks>
	 * <para header='Description'>This is a callback function that is called when the connection status of the audio output device changes.<br/>
	 * Called from the application's main thread.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExOutputDeviceObserver::DeviceConnectionChangeCallback'/>
	 */
	public static event DeviceConnectionChangeCallback OnDeviceConnectionChanged {
		add {
			_onDeviceConnectionChanged += value;
			if (instance) {
				value(IsDeviceConnected, DeviceType);
			}
		}
		remove {
			_onDeviceConnectionChanged -= value;
		}
	}

	/**
	 * <summary>Gets device connection status</summary>
	 * <returns>Whether connected (false = disconnected, true = connected)</returns>
	 * <remarks>
	 * <para header='Description'>Returns whether an audio output device is connected to the machine.<br/>
	 * Returns true if the output destination is a device other than the internal speaker.</para>
	 * </remarks>
	 */
	public static bool IsDeviceConnected {
		get {
			if (instance == null) {
				return false;
			}
#if !UNITY_EDITOR && UNITY_IOS
			return UnsafeNativeMethods.criAtomUnity_IsOutputDeviceConnected_IOS();
#elif !UNITY_EDITOR && UNITY_ANDROID
			return instance.isConnected;
#else
			return false;
#endif
		}
	}

	/**
	 * <summary>Gets the output device type</summary>
	 * <returns>Output device type</returns>
	 * <remarks>
	 * <para header='Description'>Gets the type of the current audio output device.</para>
	 * </remarks>
	 */
	public static OutputDeviceType DeviceType {
		get {
			if (instance == null) {
				return OutputDeviceType.BuiltinSpeaker;
			}
#if !UNITY_EDITOR && UNITY_IOS
			return UnsafeNativeMethods.criAtomUnity_GetOutputDeviceType_IOS();
#elif !UNITY_EDITOR && UNITY_ANDROID
			return instance.deviceType;
#else
			return OutputDeviceType.BuiltinSpeaker;
#endif
		}
	}

	#region Internal Members
	[SerializeField] bool dontDestroyOnLoad = false;
	bool lastIsConnected = false;
	bool isConnected = false;
	OutputDeviceType lastDeviceType = OutputDeviceType.BuiltinSpeaker;
	OutputDeviceType deviceType = OutputDeviceType.BuiltinSpeaker;
	static CriAtomOutputDeviceObserver instance = null;
	static event DeviceConnectionChangeCallback _onDeviceConnectionChanged = null;
#if !UNITY_EDITOR && UNITY_ANDROID
	static UnityEngine.AndroidJavaObject checker = null;
#endif
	#endregion

	#region Internal Functions
	private void Awake() {
		if (instance != null) {
			Destroy(this);
			return;
		}

		if (!CriAtomPlugin.IsLibraryInitialized()) {
			Debug.LogError("[CRIWARE] Atom library is not initialized. Cannot setup CriAtomExOutputDeviceObserver.");
			Destroy(this);
			return;
		}

		instance = this;

#if CRIWAREPLUGIN_SUPPORT_OUTPUTDEVICE_OBSERVER
#if !UNITY_EDITOR && UNITY_IOS
		bool isStarted = UnsafeNativeMethods.criAtomUnity_StartOutputDeviceObserver_IOS();
		if (!isStarted) {
			Debug.LogError("[CRIWARE] CriAtomOutputDeviceObserver cannot start while Atom library is not initialized.");
		}
#elif !UNITY_EDITOR && UNITY_ANDROID
		UnityEngine.AndroidJavaClass jc = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
		UnityEngine.AndroidJavaObject activity = jc.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");
		
		if (checker == null) {
			checker = new UnityEngine.AndroidJavaObject("com.crimw.crijavaclasses.CriOutputDeviceObserver", activity, this.gameObject.name, "CallbackFromObserver_ANDROID");
		}
		if (checker == null) {
			Debug.LogError("[CRIWARE] Cannot load CriOutputDeviceObserver class in library.");
		}
		checker.Call("Start", activity);
		CheckOutputDevice_ANDROID();
#endif
		isConnected = lastIsConnected = IsDeviceConnected;
		deviceType = lastDeviceType = DeviceType;
		if (_onDeviceConnectionChanged != null) {
			_onDeviceConnectionChanged(isConnected, deviceType);
		}
#elif !UNITY_EDITOR
		Debug.Log("[CRIWARE] CriAtomOutputDeviceObserver is not supported on this platform.");
#endif
		if (this.dontDestroyOnLoad) {
			GameObject.DontDestroyOnLoad(this.gameObject);
		}
	}


	private void OnDestroy() {
		if (instance != this) {
			return;
		}
		instance = null;

#if CRIWAREPLUGIN_SUPPORT_OUTPUTDEVICE_OBSERVER
#if !UNITY_EDITOR && UNITY_IOS
		UnsafeNativeMethods.criAtomUnity_StopOutputDeviceObserver_IOS();
#elif !UNITY_EDITOR && UNITY_ANDROID
		UnityEngine.AndroidJavaClass jc = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
		UnityEngine.AndroidJavaObject activity = jc.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");
		if (activity != null && checker != null) {
			checker.Call("Stop", activity);
		}
		checker = null;
#endif
#endif
	}


	public override void CriInternalUpdate() {
		isConnected = IsDeviceConnected;
		deviceType = DeviceType;

		if ((isConnected != lastIsConnected ||
			deviceType != lastDeviceType) &&
			_onDeviceConnectionChanged != null) {
			_onDeviceConnectionChanged(isConnected, deviceType);
		}
		lastIsConnected = isConnected;
		lastDeviceType = deviceType;
	}


	public override void CriInternalLateUpdate() {
	}

#if !UNITY_EDITOR && UNITY_ANDROID
	/* [ANDROID] Callback from CriOutputDeviceObserver class */
	private void CallbackFromObserver_ANDROID(string message) {
		if (message[0] == 'a') {
			CheckOutputDevice_ANDROID();
		} else if (message[0] == 'b') {
			StartCoroutine("CoroutineForCheck_ANDROID");
		}
	}

	private void CheckOutputDevice_ANDROID() {
		if (checker == null) {
			return;
		}

		UnityEngine.AndroidJavaClass jc = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
		UnityEngine.AndroidJavaObject activity = jc.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");
		int device = checker.Call<int>("CheckOutputDeviceType", activity);
		deviceType = (OutputDeviceType)device;
		isConnected = (deviceType != OutputDeviceType.BuiltinSpeaker);

	}

	private IEnumerator CoroutineForCheck_ANDROID() {
		const float waitSec = 2.0f;
		float time = 0.0f;
		while (time < waitSec) {
			yield return null;
			time += Time.deltaTime;
		}
		CheckOutputDevice_ANDROID();
	}

#endif
	#endregion

	#region Dll Import
	private static class UnsafeNativeMethods
	{
#if !CRIWARE_ENABLE_HEADLESS_MODE
#if !UNITY_EDITOR && UNITY_IOS
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern bool criAtomUnity_StartOutputDeviceObserver_IOS();
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern void criAtomUnity_StopOutputDeviceObserver_IOS();
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern bool criAtomUnity_IsOutputDeviceConnected_IOS();
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		internal static extern OutputDeviceType criAtomUnity_GetOutputDeviceType_IOS();
#endif
#else
#if !UNITY_EDITOR && UNITY_IOS
		internal static bool criAtomUnity_StartOutputDeviceObserver_IOS() { return false; }
		internal static void criAtomUnity_StopOutputDeviceObserver_IOS() { }
		internal static bool criAtomUnity_IsOutputDeviceConnected_IOS() { return false; }
		internal static OutputDeviceType criAtomUnity_GetOutputDeviceType_IOS() { return OutputDeviceType.BuiltinSpeaker; }
#endif
#endif
	}
	#endregion

} // end of class

} //namespace CriWare
/** @} */
/* end of file */
