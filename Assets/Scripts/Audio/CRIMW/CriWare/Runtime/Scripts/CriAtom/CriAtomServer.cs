/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using System;

namespace CriWare {

public class CriAtomServer : CriMonoBehaviour {

	#region Internal Fields
	private static CriAtomServer _instance = null;
#if UNITY_EDITOR
	private bool isApplicationPaused = false;
	private bool isEditorPaused = false;
#endif
	#endregion

	public System.Action<bool> onApplicationPausePreProcess;
	public System.Action<bool> onApplicationPausePostProcess;
	static public bool KeepPlayingSoundOnPause = true;
	static public bool EnableAutoConsumePcmOutput = true;

#if UNITY_EDITOR
	private bool consumingPcmOutput = false;
	private float consumeStartTime = 0.0f;
	private ulong consumedSamples = 0;
	private int samplingRate;
	private int channels;
	private float[][] buffer;
#endif

	public static CriAtomServer instance {
		get {
			CreateInstance();
			return _instance;
		}
	}

	public static void CreateInstance() {
		if (_instance == null) {
			CriWare.Common.managerObject.AddComponent<CriAtomServer>();
		}
	}

	public static void DestroyInstance() {
		if (_instance != null) {
			UnityEngine.GameObject.Destroy(_instance);
		}
	}

	void Awake()
	{
		/* インスタンスは常に１つしか生成されないことを保証する */
		if (_instance == null) {
			_instance = this;
		} else {
			GameObject.Destroy(this);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
#if UNITY_EDITOR
#if UNITY_2017_2_OR_NEWER
		UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		UnityEditor.EditorApplication.pauseStateChanged += OnPauseStateChanged;
#else
		UnityEditor.EditorApplication.playmodeStateChanged += OnPlaymodeStateChange;
#endif
#endif
	}

	protected override void OnDisable()
	{
		base.OnDisable();
#if UNITY_EDITOR
#if UNITY_2017_2_OR_NEWER
		UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		UnityEditor.EditorApplication.pauseStateChanged -= OnPauseStateChanged;
#else
		UnityEditor.EditorApplication.playmodeStateChanged -= OnPlaymodeStateChange;
#endif
#endif

		if (_instance == this) {
			_instance = null;
		}
	}

	public override void CriInternalUpdate()
	{
		CriAtomPlugin.ExecuteQueuedCueLinkCallbacks();
		CriAtomPlugin.ExecuteQueuedEventCallbacks();
		CriAtomPlugin.ExecuteQueuedBeatSyncCallbacks();

		if (EnableAutoConsumePcmOutput) {
			ConsumePcmOutput();
		}
	}

	public override void CriInternalLateUpdate() { }

	private void ConsumePcmOutput()
	{
#if UNITY_EDITOR
		if (!CriAtomPlugin.IsInitializedForPcmOutput()) {
			consumingPcmOutput = false;
			return;
		}

		if (buffer == null) {
			channels = CriAtomPlugin.GetOutputChannels();
			samplingRate = CriAtomPlugin.GetOutputSamplingRate();
			buffer = new float[channels][];
			for (int i = 0; i < channels; i++) {
				buffer[i] = new float[samplingRate];
			}
		}

		if (!consumingPcmOutput) {
			consumeStartTime = Time.time;
			consumedSamples = 0;
			consumingPcmOutput = true;
		}

		int numToConsume = (int)(((Time.time - consumeStartTime) * samplingRate) - consumedSamples);
		consumedSamples += (ulong)CriAtomExAsr.GetPcmOutput(channels, numToConsume, buffer);
#endif
	}

#if UNITY_EDITOR
	private void OnPlaymodeStateChange()
	{
		bool paused = UnityEditor.EditorApplication.isPaused;
		if (!isApplicationPaused && isEditorPaused != paused) {
			ProcessApplicationPause(paused);
			isEditorPaused = paused;
		}
	}

#if UNITY_2017_2_OR_NEWER
	private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
	{
		OnPlaymodeStateChange();
	}
	private void OnPauseStateChanged(UnityEditor.PauseState state)
	{
		OnPlaymodeStateChange();
	}
#endif

#endif

	void OnApplicationPause(bool appPause)
	{
#if UNITY_EDITOR
		if (!isEditorPaused && isApplicationPaused != appPause) {
			ProcessApplicationPause(appPause);
			isApplicationPaused = appPause;
		}
#else
		ProcessApplicationPause(appPause);
#endif
	}

	void ProcessApplicationPause(bool appPause)
	{
		if (onApplicationPausePreProcess != null) {
			onApplicationPausePreProcess(appPause);
		}
#if !UNITY_EDITOR && UNITY_IOS
		if(appPause == false) {
			CriAtomPlugin.CallOnApplicationResume_IOS();
		}
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
		if (!KeepPlayingSoundOnPause) {
			CriAtomPlugin.Pause(appPause);
		}
#else
#if !UNITY_IOS
		CriAtomPlugin.Pause(appPause);
#endif
#endif
		if (onApplicationPausePostProcess != null) {
			onApplicationPausePostProcess(appPause);
		}
	}
}

} //namespace CriWare