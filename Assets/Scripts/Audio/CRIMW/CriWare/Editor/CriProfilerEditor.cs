/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CriWare {

[Serializable]
public sealed class CriProfilerEditor : EditorWindow
{
	private static CriProfiler profiler = CriProfiler.GetSingleton();

	#region Unity Event Callbacks

	[MenuItem ("Window/CRIWARE/CRI Profiler")]
	static void InitWindow() {
		CriProfilerEditor window = (CriProfilerEditor)EditorWindow.GetWindow(typeof(CriProfilerEditor));
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
		window.titleContent = new GUIContent("CRI Profiler");
#else
		window.title = "CRI Profiler";
#endif
		window.Show();
	}

	private struct PlaymodeState {
		public readonly bool isPlaying;
		public readonly bool isPaused;
		public readonly bool isPlayingOrWillChangePlaymode;
		public PlaymodeState(bool isPlaying, bool isPaused, bool isPlayingOrWillChangePlaymode) {
			this.isPlaying = isPlaying;
			this.isPaused = isPaused;
			this.isPlayingOrWillChangePlaymode = isPlayingOrWillChangePlaymode;
		}
	}
	private PlaymodeState prevPlaymodeState = new PlaymodeState(false, false, false);

	private void OnStateChanged() {
		if (EditorApplication.isPlayingOrWillChangePlaymode == false) {
			profiler.StopProfiling();
		} else if (startWithUnityPlayer == true && EditorApplication.isPlaying == true && prevPlaymodeState.isPlaying == false) {
			this.remoteProfiling = false;
			profiler.ipAddressString = DEFAULT_IP_ADDR;
			profiler.StartProfiling(saveLogFile, logFileDir);
		}
		if (this.pauseWithUnityPlayer == true) {
			this.isWindowPaused = EditorApplication.isPaused;
		}
		prevPlaymodeState = new PlaymodeState(EditorApplication.isPlaying, EditorApplication.isPaused, EditorApplication.isPlayingOrWillChangePlaymode);
	}

#if UNITY_2017_2_OR_NEWER
	private void OnPlayModeStateChanged(PlayModeStateChange state) {
		OnStateChanged();
	}
	private void OnPauseStateChanged(PauseState state) {
		OnStateChanged();
	}
#endif

	private void OnEnable() {
#if UNITY_2017_2_OR_NEWER
		UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		UnityEditor.EditorApplication.pauseStateChanged += OnPauseStateChanged;
#else
		UnityEditor.EditorApplication.playmodeStateChanged += OnStateChanged;
#endif
	}

	private void OnDisable() {
		if (profiler != null) {
			profiler.StopProfiling();
		}
		DestroyResources();

#if UNITY_2017_2_OR_NEWER
		UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		UnityEditor.EditorApplication.pauseStateChanged -= OnPauseStateChanged;
#else
		UnityEditor.EditorApplication.playmodeStateChanged -= OnStateChanged;
#endif
	}

	#endregion Unity Event Callbacks

	#region Data Slice Definition

	private struct DataSlicePerformance {
		public readonly Single cpuLoad;
		public readonly Single[] cpuLoadHistory;
		public readonly UInt32 maxVoices;
		public readonly UInt32 usedVoices;
		public readonly UInt32[] voiceUsageHistory;
		public readonly UInt32 maxStandardVoices;
		public readonly int usedStandardVoices;
		public readonly UInt32 maxHcaMxVoices;
		public readonly int usedHcaMxVoices;
		public readonly UInt32 usedStreams;
		public readonly Single totalBps;
		public DataSlicePerformance(CriProfiler profiler) {
			this.cpuLoad = profiler.cpu_load;
			this.cpuLoadHistory = profiler.CpuLoadHistory;
			this.maxVoices =
				profiler.maxVoice_stdStreaming +
				profiler.maxVoice_hcamxStreaming +
				profiler.maxVoice_stdOnMemory +
				profiler.maxVoice_hcamxOnMemory;
			this.usedVoices = profiler.num_used_voices;
			this.voiceUsageHistory = profiler.VoiceUsageHistory;
			this.maxStandardVoices = profiler.maxVoice_stdOnMemory + profiler.maxVoice_stdStreaming;
			this.usedStandardVoices = profiler.GetVoicePoolUsage(CriProfiler.VoicePoolFormat.Standard);
			this.maxHcaMxVoices = profiler.maxVoice_hcamxOnMemory + profiler.maxVoice_hcamxStreaming;
			this.usedHcaMxVoices = profiler.GetVoicePoolUsage(CriProfiler.VoicePoolFormat.HcaMx);
			this.usedStreams = profiler.num_used_streams;
			this.totalBps = profiler.total_bps;
		}
	}

	private struct DataSlicePlayback {
		public readonly CriProfiler.PlaybackInfo[] playbackList;
		public readonly StringBuilder playbackListString;
		public readonly string cuenameLastPlayed;
		public DataSlicePlayback(CriProfiler profiler) {
			this.playbackList = profiler.PlaybackList;
			this.playbackListString = new StringBuilder();
			for (int i = 0; i < playbackList.Length; ++i) {
				playbackListString.Append(playbackList[i].cueName + " ");
			}
			this.cuenameLastPlayed = string.IsNullOrEmpty(profiler.cuename_lastPlayed) ? "---" : profiler.cuename_lastPlayed + " (" + profiler.cuesheetName_lastPlayed + ")";
		}
	}
	private struct DataSliceLoudness {
		public readonly Single momentary;
		public readonly Single shortTerm;
		public readonly Single integrated;
		public DataSliceLoudness(CriProfiler profiler) {
			this.momentary = profiler.loudness_momentary;
			this.shortTerm = profiler.loudness_shortTerm;
			this.integrated = profiler.loudness_integrated;
		}
	}
	private struct DataSliceLevels {
		public readonly CriProfiler.LevelInfo[] levels;
		public readonly int outputCh;
		public DataSliceLevels(CriProfiler profiler) {
			this.levels = profiler.LevelsAllCh;
			this.outputCh = profiler.numChMasterOut;
		}
	}

	private struct DataSliceLocalRuntime {
		public readonly uint memUsageAtom;
		public readonly uint memUsageFs;
		public DataSliceLocalRuntime(bool isPlaying) {
			if (isPlaying == true) {
				memUsageAtom = CriAtomPlugin.IsLibraryInitialized() ? CriWare.Common.GetAtomMemoryUsage() : 0;
				memUsageFs = CriFsPlugin.IsLibraryInitialized() ? CriWare.Common.GetFsMemoryUsage() : 0;
			} else {
				memUsageAtom = 0;
				memUsageFs = 0;
			}
		}
	}

	#endregion Data Slice Definition

	#region Internal Fields

	/* configurations */
	private const Single DB_MIN = -96.0f;
	private const Single DB_MAX = 20.0f;
	private const Single DEFAULT_DB_FLOOR = -48.0f;
	private const Single DEFAULT_DB_CEILING = 0;
	private const Single DEFAULT_DB_TARGET = -24.0f;
	private const Single DB_RANGE_PLUSMINUS_MAX = 20.0f;
	private const Single DB_RANGE_PLUSMINUS_MIN = 0;
	private const Single DEFAULT_DB_RANGE_PLUSMINUS = 2.0f;
	private const Single DEFAULT_DB_WARNING = -6.0f;
	private const Single DEFAULT_LEVEL_FLOOR = -48.0f;
	private const Single DEFAULT_LEVEL_CEILING = 0;
	private const Single DEFAULT_LEVEL_WARNING = -6.0f;
	private const int DEFAULT_LEVEL_CAL_INTVL = 6;
	private const int DEFAULT_LEVEL_CAL_INTVL_MIN = 1;
	private const int DEFAULT_LEVEL_CAL_INTVL_MAX = 24;
	private const MultiChannelMeterType DEFAULT_LEVEL_TYPE = MultiChannelMeterType.Peak;
	private const string DEFAULT_IP_ADDR = "127.0.0.1";
	[SerializeField] private bool startWithUnityPlayer = true;
	[SerializeField] private bool pauseWithUnityPlayer = true;
	[SerializeField] private bool saveLogFile = false;
	[SerializeField] private string logFileDir = "";
	[SerializeField] private bool remoteProfiling = false;
	[SerializeField] private bool isWindowPaused = false;
	[SerializeField] private Single currentDbFloor = DEFAULT_DB_FLOOR;
	[SerializeField] private Single currentDbCeiling = DEFAULT_DB_CEILING;
	[SerializeField] private Single currentDbTarget = DEFAULT_DB_TARGET;
	[SerializeField] private Single currentDbRangePlusMinus = DEFAULT_DB_RANGE_PLUSMINUS;
	[SerializeField] private Single currentDbWarning = DEFAULT_DB_WARNING;
	[SerializeField] private Single currentLevelFloor = DEFAULT_LEVEL_FLOOR;
	[SerializeField] private Single currentLevelCeiling = DEFAULT_LEVEL_CEILING;
	[SerializeField] private Single currentLevelWarning = DEFAULT_LEVEL_WARNING;
	[SerializeField] private int currentLevelCalInterval = DEFAULT_LEVEL_CAL_INTVL;
	[SerializeField] private MultiChannelMeterType currentLevelType = DEFAULT_LEVEL_TYPE;

	/* GUI constants */
	private const float LABEL_WIDTH = 148.0f;
	private const float CHART_INDENT_WIDTH = 15.0f;
	private const float GUI_INDENT_WIDTH = 15.0f;
	private const int TEXT_SIZE_CALIBRATOR = 10;
	private const int TEXT_SIZE_LABEL = 11;
	private const int TEXT_SIZE_CUE_LIST = 12;
	private const int TEXT_SIZE_GIANT = 20;
	private const int LOG_ITEM_HEIGHT = 20;
	static private Vector2 textOffsetChartTitle = new Vector2(3.0f, 2.0f);
	static private Vector2 textOffsetCalibrator = new Vector2(-4.0f, -6.0f);
	static private Color colorWarningHigh = new Color(224 / 255.0f, 44 / 255.0f, 44 / 255.0f);
	static private Color colorWarningHighTolerant = new Color(244 / 255.0f, 170 / 255.0f, 44 / 255.0f);
	static private Color colorMidRange = new Color(107 / 255.0f, 224 / 255.0f, 44 / 255.0f);
	static private Color colorWarningLow = new Color(44 / 255.0f, 107 / 255.0f, 224 / 255.0f);
	static private Color colorGrid = new Color(0.2f, 0.2f, 0.2f);
	private const int CNT_CUSTOM_STYLES = 20;
	static private GUIStyle[] customStyles = new GUIStyle[CNT_CUSTOM_STYLES];
	static private Texture2D[] customStyleTex = new Texture2D[CNT_CUSTOM_STYLES];
	private GUIStyle barBorderStyle { get { return InitializeStyleWithBgColor(0, Color.gray); } }
	private GUIStyle barBackgroundStyle { get { return InitializeStyleWithBgColor(1, Color.black); } }
	private GUIStyle barStyle { get { return InitializeStyleWithBgColor(2, Color.gray); } }
	private GUIStyle barWarningHighStyle { get { return InitializeStyleWithBgColor(3, colorWarningHigh); } }
	private GUIStyle barWarningHighTolerantStyle { get { return InitializeStyleWithBgColor(4, colorWarningHighTolerant); } }
	private GUIStyle barMidRangeStyle { get { return InitializeStyleWithBgColor(5, colorMidRange); } }
	private GUIStyle barWarningLowStyle { get { return InitializeStyleWithBgColor(6, colorWarningLow); } }
	private GUIStyle barTextStyle { get { return InitializeTextStyle(7, TEXT_SIZE_LABEL, Color.white, TextAnchor.UpperLeft); } }
	private GUIStyle barTextShadowStyle { get { return InitializeTextStyle(8, TEXT_SIZE_LABEL, Color.black, TextAnchor.UpperLeft); } }
	private GUIStyle meterTextWarningHighStyle { get { return InitializeTextStyle(9, TEXT_SIZE_GIANT, colorWarningHigh, TextAnchor.MiddleCenter); } }
	private GUIStyle meterTextWarningHighTolerantStyle { get { return InitializeTextStyle(10, TEXT_SIZE_GIANT, colorWarningHighTolerant, TextAnchor.MiddleCenter); } }
	private GUIStyle meterTextMidRangeStyle { get { return InitializeTextStyle(11, TEXT_SIZE_GIANT, colorMidRange, TextAnchor.MiddleCenter); } }
	private GUIStyle meterTextWarningLowStyle { get { return InitializeTextStyle(12, TEXT_SIZE_GIANT, colorWarningLow, TextAnchor.MiddleCenter); } }
	private GUIStyle multiChMeterTextStyle { get { return InitializeTextStyle(13, TEXT_SIZE_LABEL, Color.white, TextAnchor.MiddleCenter); } }
	private GUIStyle cueListStyle { get { return InitializeTextStyle(14, TEXT_SIZE_CUE_LIST, Color.white, TextAnchor.UpperLeft, s => {
		s.fontStyle = FontStyle.Bold;
		s.wordWrap = true;
	}); } }
	private GUIStyle chartLabelStyle { get { return InitializeTextStyle(15, TEXT_SIZE_CALIBRATOR, Color.gray, TextAnchor.UpperRight); } }
	private GUIStyle timelineHeaderStyle { get { return InitializeStyleWithBgColor(16, new Color(0.15f, 0.15f, 0.15f)); } }
	private GUIStyle timelineStyle { get { return InitializeStyleWithBgColor(17, new Color(0.1f, 0.1f, 0.1f)); } }
	private GUIStyle timelineButtonStyle { get { return InitializeStyleWithGradColor(18, new Color(0.8f, 0.8f, 0.8f), new Color(0.7f, 0.7f, 0.7f), s => {
		s.fixedHeight = 18;
		s.alignment = TextAnchor.MiddleCenter;
		s.clipping = TextClipping.Clip;
		s.margin = new RectOffset(1, 1, 1, 1);
		s.padding = new RectOffset(1, 1, 1, 1);
	}); } }
	private GUIStyle timelineCueSheetStyle { get { return InitializeStyleWithGradColor(19, new Color(0.4f, 0.4f, 0.4f), new Color(0.3f, 0.3f, 0.3f), s => {
		s.alignment = TextAnchor.MiddleCenter;
		s.clipping = TextClipping.Clip;
		s.fontStyle = FontStyle.Bold;
		s.normal.textColor = Color.white;
		s.margin = new RectOffset(1, 1, 1, 1);
		s.padding = new RectOffset(1, 1, 1, 1);
	}); } }
	private delegate void AdditionalStyleDelegate(GUIStyle style);
	static private Color[] chartGradientColors = new Color[] {
		Color.green,
		Color.green * 0.8f + Color.yellow * 0.2f,
		Color.green * 0.6f + Color.yellow * 0.4f,
		Color.green * 0.4f + Color.yellow * 0.6f,
		Color.green * 0.2f + Color.yellow * 0.8f,
		Color.yellow,
		Color.yellow * 0.8f + Color.red * 0.2f,
		Color.yellow * 0.6f + Color.red * 0.4f,
		Color.yellow * 0.4f + Color.red * 0.6f,
		Color.yellow * 0.2f + Color.red * 0.8f,
		Color.red
	};

	/* GUI variables */
	[SerializeField] private bool foldTogglerConfig = false;
	[SerializeField] private bool foldTogglerExperimental = false;
	[SerializeField] private bool foldTogglerPerformance = true;
	[SerializeField] private bool foldTogglerPlayingList = true;
	[SerializeField] private bool foldTogglerLoudness = true;
	[SerializeField] private bool foldTogglerLoudnessAppearance = false;
	[SerializeField] private bool foldTogglerLevels = true;
	[SerializeField] private bool foldTogglerLevelAppearance = false;
	[SerializeField] private bool foldTogglerTimeline = true;
	[SerializeField] private bool foldTogglerLogList = true;
	[SerializeField] private bool timelineAutoScroll = true;
	private Vector2 monitorPanelScrollPos;
	private Vector2 timelinePanelScrollPos;
	private Vector2 logListPanelScrollPos;
	private Vector2 logListScrollPos;
	private Vector2 textareaScrollViewPostion;
	private int logItemCount = 0;
	private bool isLogItemCountChanged = false;
	private int logViewCount = 0;
	private int logFirstViewIndex = 0;
	private bool refreshLogListOnce = false;
	private List<byte[]> logViewSubList = null;
	private Color currentGuiColor;
	private Rect timelineWinRect;
	private Shader m_shader;
	private Material m_mat;
	private Material mat {
		get {
			if (m_shader == null) {
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
				m_shader = Shader.Find("Hidden/Internal-Colored");
#else
				m_shader = Shader.Find("Sprites/Default");
#endif
			}
			if (m_mat == null && m_shader != null) {
				m_mat = new Material(m_shader);
			} else if (m_mat.shader != m_shader) {
				DestroyImmediate(m_mat);
				m_mat = new Material(m_shader);
			}
			return m_mat;
		}
	}
	private StringBuilder gStrBuilder = new StringBuilder(50);
	private StringBuilder gStrBuilderSub = new StringBuilder(50);

	/* Data slices */
	private DataSlicePerformance perfData = new DataSlicePerformance(profiler);
	private DataSlicePlayback playbackData = new DataSlicePlayback(profiler);
	private DataSliceLoudness loudnessData = new DataSliceLoudness(profiler);
	private DataSliceLevels levelsData = new DataSliceLevels(profiler);
	private DataSliceLocalRuntime runtimeData = new DataSliceLocalRuntime(false);
	private Dictionary<string, CriProfiler.CueSheetGroup> timeline = null;
	private ulong firstTimeStamp = 0;
	private ulong currentTimeStamp = 0;
	private CriProfiler.Playback? currentTimelinePlayback = null;

	private GUIStyle InitializeStyleWithBgColor(int id, Color color, AdditionalStyleDelegate func = null) {
		if (customStyleTex[id] == null) {
			customStyleTex[id] = new Texture2D(1, 1, TextureFormat.RGB24, false);
			customStyleTex[id].SetPixel(1, 1, color);
			customStyleTex[id].Apply();
		}
		if (customStyles[id] == null) {
			customStyles[id] = new GUIStyle();
			if (func != null) {
				func(customStyles[id]);
			}
		}
		customStyles[id].normal.background = customStyleTex[id];
		return customStyles[id];
	}

	private GUIStyle InitializeStyleWithGradColor(int id, Color color1, Color color2, AdditionalStyleDelegate func = null) {
		if (customStyleTex[id] == null) {
			customStyleTex[id] = new Texture2D(1, 4, TextureFormat.RGB24, false);
			customStyleTex[id].SetPixel(1, 1, color2);
			customStyleTex[id].SetPixel(1, 2, color1);
			customStyleTex[id].SetPixel(1, 3, color1);
			customStyleTex[id].SetPixel(1, 4, color2);
			customStyleTex[id].Apply();
		}
		if (customStyles[id] == null) {
			customStyles[id] = new GUIStyle();
			if (func != null) {
				func(customStyles[id]);
			}
		}
		customStyles[id].normal.background = customStyleTex[id];
		return customStyles[id];
	}

	private GUIStyle InitializeTextStyle(int id, int size, Color color, TextAnchor anchor, AdditionalStyleDelegate func = null) {
		if (customStyles[id] == null) {
			customStyles[id] = new GUIStyle();
			customStyles[id].normal.textColor = color;
			customStyles[id].fontSize = size;
			customStyles[id].alignment = anchor;
			if (func != null) {
				func(customStyles[id]);
			}
		}
		return customStyles[id];
	}

	private void DestroyResources() {
		if (m_mat != null) {
			DestroyImmediate(m_mat);
		}
	}

	#endregion Internal Fields

	#region Const strings on GUI
	private const string S_DIVIDER = " / ";
	private const string S_NAN = "-";
	private readonly GUIContent S_LOGFILEDIR = new GUIContent("Log file directory", "leave empty to use default location");
	private readonly GUIContent GC_LEVELRANGE = new GUIContent("Disp. Range (dB)");
	private readonly string[] S_MultiChMeterButton = { "Peak", "RMS" };
	private readonly string[] S_ChannelNames = { "L", "R", "C", "LFE", "Ls", "Rs", "Ex1", "Ex2" };
	#endregion Const strings on GUI

	#region OnGUI

	private void OnGUI() {
		GUILayoutOption buttonHeightSetting = GUILayout.Height(25);
		this.currentGuiColor = GUI.color;

		using (var globalHorizontal = new GUILayout.HorizontalScope()) {
			using (var leftPaneVertical = new GUILayout.VerticalScope(GUILayout.Width(430))) {
				GUILayout.Space(5.0f);
				using (var startControlHorizontal = new GUILayout.HorizontalScope()) {
					GUI.enabled = EditorApplication.isPlaying || remoteProfiling;
					if (profiler.IsProfiling == false) {
						if (GUILayout.Button("Start", buttonHeightSetting)) {
							profiler.StartProfiling(saveLogFile, logFileDir);
							this.isWindowPaused = false;
						}
					} else {
						if (GUILayout.Button("Stop", buttonHeightSetting)) {
							profiler.StopProfiling();
						}
					}
					GUI.enabled = profiler.IsProfiling;
					if (this.isWindowPaused == false) {
						if (GUILayout.Button("Pause", buttonHeightSetting)) {
							this.isWindowPaused = true;
						}
					} else {
						if (GUILayout.Button("Resume", buttonHeightSetting)) {
							this.isWindowPaused = false;
						}
					}
					GUI.enabled = true;
				}
				using (var monitorScrollView = new GUILayout.ScrollViewScope(monitorPanelScrollPos)) {
					monitorPanelScrollPos = monitorScrollView.scrollPosition;

					DrawConfigurationInfo();

					GUILayout.Label("Monitoring", EditorStyles.whiteBoldLabel);

					DrawrPerformanceInfo();
					DrawrPlaybackInfo();
					DrawLoudnessInfo();
					DrawLevelsInfo();

					GUILayout.Space(20.0f);
				}
			}

			using (var rightPaneVertical = new GUILayout.VerticalScope()) {
				DrawTimeline();
				DrawLogList();
			}
		}

		this.Repaint();
	}

	private void DrawConfigurationInfo() {
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		foldTogglerConfig = CriFoldout(foldTogglerConfig, "Configuration");
		if (foldTogglerConfig) {
			EditorGUI.indentLevel++;
			startWithUnityPlayer = EditorGUILayout.Toggle("Start with Player", startWithUnityPlayer);
			pauseWithUnityPlayer = EditorGUILayout.Toggle("Pause with Player", pauseWithUnityPlayer);
			saveLogFile = EditorGUILayout.Toggle("Save log file", saveLogFile);
			GUI.enabled = saveLogFile;
			EditorGUI.indentLevel++;
			logFileDir = EditorGUILayout.TextField(S_LOGFILEDIR, profiler.LogFileSavePath);
			EditorGUI.indentLevel--;
			GUI.enabled = true;
			foldTogglerExperimental = CriFoldout(foldTogglerExperimental, "Experimental");
			if (foldTogglerExperimental) {
				EditorGUI.indentLevel++;
				GUI.enabled = (profiler.IsProfiling == false);
				remoteProfiling = EditorGUILayout.Toggle("Remote Profiling", remoteProfiling);
				if (remoteProfiling == false) {
					profiler.ipAddressString = DEFAULT_IP_ADDR;
				}
				GUI.enabled = remoteProfiling && (profiler.IsProfiling == false);
				profiler.ipAddressString = EditorGUILayout.TextField("IP Address", profiler.ipAddressString);
				GUI.enabled = true;
				EditorGUI.indentLevel--;
			}
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndVertical();
	}

	private void DrawrPerformanceInfo() {
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		foldTogglerPerformance = CriFoldout(foldTogglerPerformance, "Performance");
		if (foldTogglerPerformance) {
			if (isWindowPaused == false) {
				perfData = new DataSlicePerformance(profiler);
				if (this.remoteProfiling == false) {
					runtimeData = new DataSliceLocalRuntime(EditorApplication.isPlaying);
				}
			}
			EditorGUI.indentLevel++;
			Rect chartRect = EditorGUILayout.GetControlRect(GUILayout.Height(80));
			chartRect.xMin += CHART_INDENT_WIDTH;
			LineGraph(chartRect, ClearedStr.Append("CPU Usage: ").AppendFormat("{0:F2}", perfData.cpuLoad).Append("%").ToString(), perfData.cpuLoadHistory, 100.0f, 5, 5.0f, true);
			chartRect = EditorGUILayout.GetControlRect(GUILayout.Height(80));
			chartRect.xMin += CHART_INDENT_WIDTH;
			LineGraph(chartRect, ClearedStr.Append("Number of Voices: ").Append(perfData.usedVoices).Append(S_DIVIDER).Append(perfData.maxVoices).ToString(), Array.ConvertAll(perfData.voiceUsageHistory, x => (float)x), perfData.maxVoices, 5, 4.0f, false);
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("Standard Pool Usage", GUILayout.Width(LABEL_WIDTH));
				SimpleBarGraph(EditorGUILayout.GetControlRect(), perfData.usedStandardVoices, 0, perfData.maxStandardVoices,
					ClearedStr.Append(perfData.usedStandardVoices >= 0 ? perfData.usedStandardVoices.ToString() : S_NAN)
					.Append(S_DIVIDER)
					.Append(perfData.maxStandardVoices > 0 ? perfData.maxStandardVoices.ToString() : S_NAN)
					.ToString());
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("HCA-MX Pool Usage", GUILayout.Width(LABEL_WIDTH));
				SimpleBarGraph(EditorGUILayout.GetControlRect(), perfData.usedHcaMxVoices, 0, perfData.maxHcaMxVoices,
					ClearedStr.Append(perfData.usedHcaMxVoices >= 0 ? perfData.usedHcaMxVoices.ToString() : S_NAN)
					.Append(S_DIVIDER)
					.Append(perfData.maxHcaMxVoices > 0 ? perfData.maxHcaMxVoices.ToString() : S_NAN)
					.ToString());
			}
			GUILayout.EndHorizontal();
			if (this.remoteProfiling == false) {
				EditorGUILayout.LabelField("Atom Memory Usage", DataSize2String(runtimeData.memUsageAtom));
				EditorGUILayout.LabelField("FS Memory Usage", DataSize2String(runtimeData.memUsageFs));
			}
			EditorGUILayout.LabelField("Streaming Usage", ClearedStr.Append(perfData.usedStreams).Append(" voice(s) (").AppendFormat("{0:F2}", perfData.totalBps / 1000000).Append(" Mbps)").ToString());
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndVertical();
	}

	private void DrawrPlaybackInfo() {
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		foldTogglerPlayingList = CriFoldout(foldTogglerPlayingList, "Playing");
		if (foldTogglerPlayingList) {
			if (isWindowPaused == false) {
				playbackData = new DataSlicePlayback(profiler);
			}
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(GUI_INDENT_WIDTH);
				textareaScrollViewPostion = GUILayout.BeginScrollView(new Vector2(textareaScrollViewPostion.x, Mathf.Infinity), EditorStyles.helpBox, GUILayout.Height(60));
				GUILayout.Label(playbackData.playbackListString.ToString(), cueListStyle, GUILayout.ExpandHeight(true));
				GUILayout.EndScrollView();
			}
			GUILayout.EndHorizontal();
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField("Last Played", ClearedStr.Append("[ ").Append(playbackData.cuenameLastPlayed).Append(" ]").ToString());
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndVertical();
	}

	private void DrawLoudnessInfo() {
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		foldTogglerLoudness = CriFoldout(foldTogglerLoudness, "Loudness");
		if (foldTogglerLoudness) {
			if (isWindowPaused == false) {
				loudnessData = new DataSliceLoudness(profiler);
			}
			Rect chartRect;
			GUILayoutOption[] numberRectGUISettings = { GUILayout.Height(60), GUILayout.MinWidth(60) };
			GUILayoutOption barHeightSetting = GUILayout.Height(15);
			const float paddingOffset = 2.0f;
			GUILayout.BeginHorizontal();
			{
				chartRect = EditorGUILayout.GetControlRect(numberRectGUISettings);
				chartRect.x += CHART_INDENT_WIDTH;
				NumericMeter(chartRect, "Momentary  LKFS", loudnessData.momentary);
				chartRect = EditorGUILayout.GetControlRect(numberRectGUISettings);
				chartRect.x += CHART_INDENT_WIDTH - paddingOffset;
				NumericMeter(chartRect, "Short Term  LKFS", loudnessData.shortTerm);
				chartRect = EditorGUILayout.GetControlRect(numberRectGUISettings);
				chartRect.xMin += CHART_INDENT_WIDTH - paddingOffset * 2;
				NumericMeter(chartRect, "Integrated  LKFS", loudnessData.integrated);
			}
			GUILayout.EndHorizontal();
			chartRect = EditorGUILayout.GetControlRect(barHeightSetting);
			chartRect.xMin += CHART_INDENT_WIDTH;
			LoudnessBarGraph(chartRect, loudnessData.momentary, currentDbFloor, currentDbCeiling, currentDbTarget, currentDbRangePlusMinus, currentDbWarning, "Momentary");
			chartRect = EditorGUILayout.GetControlRect(barHeightSetting);
			chartRect.xMin += CHART_INDENT_WIDTH;
			LoudnessBarGraph(chartRect, loudnessData.shortTerm, currentDbFloor, currentDbCeiling, currentDbTarget, currentDbRangePlusMinus, currentDbWarning, "Short Term");
			chartRect = EditorGUILayout.GetControlRect(barHeightSetting);
			chartRect.xMin += CHART_INDENT_WIDTH;
			LoudnessBarGraph(chartRect, loudnessData.integrated, currentDbFloor, currentDbCeiling, currentDbTarget, currentDbRangePlusMinus, currentDbWarning, "Integrated");
			EditorGUI.indentLevel++;
			foldTogglerLoudnessAppearance = CriFoldout(foldTogglerLoudnessAppearance, "Appearance");
			if (foldTogglerLoudnessAppearance) {
				EditorGUI.indentLevel++;
				EditorGUILayout.MinMaxSlider(GC_LEVELRANGE, ref currentDbFloor, ref currentDbCeiling, DB_MIN, DB_MAX);
				EditorGUILayout.LabelField(" ", ClearedStr.Append("Min: ").AppendFormat("{0:F2}", currentDbFloor).Append(S_DIVIDER).Append("Max: ").AppendFormat("{0:F2}", currentDbCeiling).ToString());
				currentDbTarget = EditorGUILayout.Slider("Target (dB)", currentDbTarget, DB_MIN, DB_MAX);
				currentDbRangePlusMinus = EditorGUILayout.Slider("Tgt. Range (+/-dB)", currentDbRangePlusMinus, DB_RANGE_PLUSMINUS_MIN, DB_RANGE_PLUSMINUS_MAX);
				currentDbWarning = EditorGUILayout.Slider("Warning (dB)", currentDbWarning, DB_MIN, DB_MAX);
				GUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("", GUILayout.Width(LABEL_WIDTH));
					if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) {
						currentDbFloor = DEFAULT_DB_FLOOR;
						currentDbCeiling = DEFAULT_DB_CEILING;
						currentDbTarget = DEFAULT_DB_TARGET;
						currentDbRangePlusMinus = DEFAULT_DB_RANGE_PLUSMINUS;
						currentDbWarning = DEFAULT_DB_WARNING;
					}
				}
				GUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
			}
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndVertical();
	}

	private void DrawLevelsInfo() {
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		foldTogglerLevels = CriFoldout(foldTogglerLevels, "Peak / RMS");
		if (foldTogglerLevels) {
			if (isWindowPaused == false) {
				levelsData = new DataSliceLevels(profiler);
			}
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(GUI_INDENT_WIDTH);
				currentLevelType = (MultiChannelMeterType)GUILayout.Toolbar((int)currentLevelType, S_MultiChMeterButton);
			}
			GUILayout.EndHorizontal();
			Rect chartRect = EditorGUILayout.GetControlRect(GUILayout.Height(200));
			chartRect.xMin += CHART_INDENT_WIDTH;
			MultiChannelMeter(chartRect, levelsData.levels, levelsData.outputCh, currentLevelType, currentLevelFloor, currentLevelCeiling, currentLevelWarning, S_MultiChMeterButton[(int)currentLevelType]);
			EditorGUI.indentLevel++;
			foldTogglerLevelAppearance = CriFoldout(foldTogglerLevelAppearance, "Appearance");
			if (foldTogglerLevelAppearance) {
				EditorGUI.indentLevel++;
				EditorGUILayout.MinMaxSlider(GC_LEVELRANGE, ref currentLevelFloor, ref currentLevelCeiling, DB_MIN, DB_MAX);
				EditorGUILayout.LabelField(" ", ClearedStr.Append("Min: ").AppendFormat("{0:F2}", currentLevelFloor).Append(S_DIVIDER).Append("Max: ").AppendFormat("{0:F2}", currentLevelCeiling).ToString());
				currentLevelWarning = EditorGUILayout.Slider("Warning (dB)", currentLevelWarning, DB_MIN, DB_MAX);
				currentLevelCalInterval = EditorGUILayout.IntSlider("Grid Interval (dB)", currentLevelCalInterval, DEFAULT_LEVEL_CAL_INTVL_MIN, DEFAULT_LEVEL_CAL_INTVL_MAX);
				GUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("", GUILayout.Width(LABEL_WIDTH));
					if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) {
						currentLevelFloor = DEFAULT_LEVEL_FLOOR;
						currentLevelCeiling = DEFAULT_LEVEL_CEILING;
						currentLevelWarning = DEFAULT_LEVEL_WARNING;
						currentLevelCalInterval = DEFAULT_LEVEL_CAL_INTVL;
					}
				}
				GUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
			}
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndVertical();
	}

	private void DrawTimeline() {
		const float timeScaleFromMicroSec = 0.00005f;

		if (Event.current.type == EventType.Layout) {
			if (this.isWindowPaused == false && profiler.IsProfiling) {
				this.timeline = profiler.TimelineDataSlice;
				this.firstTimeStamp = profiler.timestampStart;
				this.currentTimeStamp = profiler.timestamp;
			}
		}

		using (var scope = new GUILayout.HorizontalScope("ToolBar")) {
			this.foldTogglerTimeline = GUILayout.Toggle(this.foldTogglerTimeline, "Timeline", EditorStyles.toolbarButton, GUILayout.Width(100));
			GUILayout.Label(currentTimelinePlayback.HasValue ? currentTimelinePlayback.Value.cuename + " (" + currentTimelinePlayback.Value.cuesheet + ") started at " + MicroSec2String(currentTimelinePlayback.Value.timestamp) : " ");
			this.timelineAutoScroll = GUILayout.Toggle(this.timelineAutoScroll, "Auto Scroll", EditorStyles.toolbarButton, GUILayout.Width(100));
		}

		if (this.foldTogglerTimeline == false) { return; }

		GUI.color = Color.white;
		EditorGUILayout.BeginHorizontal();

		var heightStyle = this.foldTogglerLogList ? GUILayout.MinHeight(300) : GUILayout.ExpandHeight(true);

		/* CueSheet Group */
		using (var scrollView = new GUILayout.ScrollViewScope(new Vector2(0, timelinePanelScrollPos.y), false, false, GUIStyle.none, GUIStyle.none, timelineHeaderStyle, heightStyle, GUILayout.Width(100))) {
			GUILayout.Space(0);
			if (timeline != null && timeline.Count > 0) foreach (var elem in timeline) {
				GUI.color = elem.Value.cuesheetColor;
					GUILayout.Label(elem.Value.name, timelineCueSheetStyle, GUILayout.Height((timelineButtonStyle.fixedHeight + 1) * elem.Value.TotalLaneCount - 1));
				GUI.color = Color.white;
			}
			GUILayout.Space(18);
		}
		/* Timeline */
		using (var scrollView = new GUILayout.ScrollViewScope(timelinePanelScrollPos, timelineStyle, heightStyle)) {
			float timelinePixelLength = (this.currentTimeStamp - this.firstTimeStamp) * timeScaleFromMicroSec;
			timelinePanelScrollPos = (this.timelineAutoScroll && this.isWindowPaused == false && profiler.IsProfiling) ? new Vector2(Mathf.Max(0, timelinePixelLength + 12 - this.timelineWinRect.width), scrollView.scrollPosition.y) : scrollView.scrollPosition;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(timelinePixelLength));
			if (timeline == null || timeline.Count <= 0) {
				GUILayout.Space(timelinePixelLength);
			} else {
				foreach (var cuesheetGroup in timeline) {
					EditorGUILayout.BeginVertical();
					foreach (var cuelane in cuesheetGroup.Value.cueLaneDict) {
						EditorGUILayout.BeginVertical();
						for (int j = 0; j < cuelane.Value.playbackLaneList.Count; ++j) {
							EditorGUILayout.BeginHorizontal();
							var pbList = cuelane.Value.playbackLaneList[j].playbackList;
							bool firstPlaybackDrawn = false;
							if (timelinePanelScrollPos.x > 0) {
								GUILayout.Space(timelinePanelScrollPos.x);
							}
							for (int k = 0; k < pbList.Count; ++k) {
								float posRelative = ((long)pbList[k].timestamp - (long)this.firstTimeStamp) * timeScaleFromMicroSec - timelinePanelScrollPos.x;

								if (posRelative > this.timelineWinRect.width) { break; } /* skip invisible */

								if (pbList[k].isStart) {
									float currentLength = 0;
									float priorSpaceWidth = 0;

									if (k < pbList.Count - 1) {
										if (((long)pbList[k + 1].timestamp - (long)this.firstTimeStamp) * timeScaleFromMicroSec - timelinePanelScrollPos.x < 0) { /* skip invisible */
											k++;
											continue;
										}
										currentLength = ((long)pbList[k + 1].timestamp - (long)pbList[k].timestamp) * timeScaleFromMicroSec;
									} else {
										currentLength = ((long)this.currentTimeStamp - (long)pbList[k].timestamp) * timeScaleFromMicroSec;
									}
									currentLength = Mathf.Min(currentLength + Mathf.Min(0, posRelative), this.timelineWinRect.width - Mathf.Max(0, posRelative));

									if (firstPlaybackDrawn == false) {
										priorSpaceWidth = Mathf.Max(0, posRelative);
									} else {
										priorSpaceWidth = ((long)pbList[k].timestamp - (long)pbList[k - 1].timestamp) * timeScaleFromMicroSec;
									}

									GUILayout.Space(priorSpaceWidth);
									GUI.color = cuelane.Value.cueColor;
									DrawPlaybackEventButton(pbList[k], currentLength);
									GUI.color = Color.white;
									firstPlaybackDrawn = true;
								} else if (firstPlaybackDrawn == false && posRelative > 0) {
									GUI.color = cuelane.Value.cueColor;
									DrawPlaybackEventButton(pbList[k], posRelative);
									GUI.color = Color.white;
									firstPlaybackDrawn = true;
								}
							}

							if (firstPlaybackDrawn == false) {
								GUILayout.Label("", timelineButtonStyle, GUILayout.Width(0)); /* empty lane */
							}
							EditorGUILayout.EndHorizontal();
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndVertical();
				}
			}
			EditorGUILayout.EndVertical();
			GUILayout.Box("", barBorderStyle, GUILayout.Width(1), GUILayout.ExpandHeight(true));
			GUILayout.Box("", barBorderStyle, GUILayout.Width(2), GUILayout.Height(30));
			GUILayout.Space(9);
			EditorGUILayout.EndHorizontal();
		}
		if (Event.current.type == EventType.Repaint) {
			this.timelineWinRect = GUILayoutUtility.GetLastRect();
		}

		EditorGUILayout.EndHorizontal();
		GUI.color = this.currentGuiColor;
	}

	private void DrawPlaybackEventButton(CriProfiler.Playback playback, float width) {
		if (GUILayout.Button(playback.cuename, timelineButtonStyle, GUILayout.Width(width - 1), GUILayout.MinWidth(-100))) {
			this.currentTimelinePlayback = playback;
			this.timelineAutoScroll = false;
		}
	}

	private void DrawLogList() {
		if (Event.current.type == EventType.Layout && ((this.isWindowPaused == false && profiler.IsProfiling) || refreshLogListOnce)) {
			refreshLogListOnce = false;
			int logListlen = profiler.GetLogList().Count;
			if (this.logItemCount != logListlen) {
				this.logItemCount = logListlen;
				this.isLogItemCountChanged = true;
			}
			this.logViewCount = (int)position.height / LOG_ITEM_HEIGHT;
			this.logFirstViewIndex = Mathf.Clamp((int)logListScrollPos.y / LOG_ITEM_HEIGHT, 0, Mathf.Max(0, logItemCount - logViewCount));
			this.logViewSubList = profiler.GetLogList(logFirstViewIndex, logViewCount);
		}

		using (var scope = new GUILayout.HorizontalScope("ToolBar", GUILayout.ExpandWidth(true))) {
			this.foldTogglerLogList = GUILayout.Toggle(this.foldTogglerLogList, "Event Log", EditorStyles.toolbarButton, GUILayout.Width(100));
			GUILayout.Label(logItemCount.ToString() + " lines", EditorStyles.miniLabel);
			if (GUILayout.Button("Open Log File", EditorStyles.toolbarButton, GUILayout.Width(120))) {
				var path = EditorUtility.OpenFilePanel("Open Log File", profiler.LogFileSavePath, CriProfiler.LOG_FILE_EXTENSION);
				if (string.IsNullOrEmpty(path) == false) {
					profiler.ClearLog();
					profiler.LoadLogFromFile(path);
					refreshLogListOnce = true;
				}
			}
			if (GUILayout.Button("Clear Log", EditorStyles.toolbarButton, GUILayout.Width(100))) {
				profiler.ClearLog();
				refreshLogListOnce = true;
			}
		}

		if (this.foldTogglerLogList == false) { return; }

		using (var scrollView = new GUILayout.ScrollViewScope(logListPanelScrollPos, "CN Box")) {
			logListPanelScrollPos = scrollView.scrollPosition;
			if (this.isLogItemCountChanged) {
				this.isLogItemCountChanged = false;
				this.logListScrollPos = new Vector2(0, logItemCount * LOG_ITEM_HEIGHT);
			}
			logListScrollPos = GUILayout.BeginScrollView(logListScrollPos);
			{
				/* only render visible UI elements */
				if (logViewSubList != null) {
					GUILayout.Space(logFirstViewIndex * LOG_ITEM_HEIGHT);
					for (int i = 0; i < logViewSubList.Count; i++) {
						GUILayout.Label(profiler.FilteredLog2String(logViewSubList[i]), EditorStyles.textArea, GUILayout.Height(LOG_ITEM_HEIGHT));
					}
					GUILayout.Space(Mathf.Max(0, logItemCount - logFirstViewIndex - logViewCount) * LOG_ITEM_HEIGHT);
				}
			}
			GUILayout.EndScrollView();
		}
	}

	#endregion OnGUI

	#region Drawing Functions

	private void SimpleBarGraph(UnityEngine.Rect rect, Single val, Single min, Single max, String text) {
		Single ratio = Value2Ratio(val, min, max);

		UnityEngine.GUI.Box(rect, "", barBorderStyle);
		rect.xMin++;
		rect.yMin++;
		rect.xMax--;
		rect.yMax--;
		UnityEngine.GUI.Box(rect, "", barBackgroundStyle);
		if (Event.current.type == EventType.Repaint) {
			if (ratio > 0.0f) {
				Rect barRect = rect;
				if (ratio > 0.999999f) {
					UnityEngine.GUI.Box(barRect, "", barWarningHighStyle);
				} else {
					barRect.width *= ratio;
					UnityEngine.GUI.Box(barRect, "", barStyle);
				}
			}
		}
		rect.xMin += 2;
		UnityEngine.GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, barTextShadowStyle);
		UnityEngine.GUI.Label(rect, text, barTextStyle);
	}

	private void LoudnessBarGraph(
		UnityEngine.Rect rect,
		Single val,
		Single min,
		Single max,
		Single valTarget,
		Single valRangePlusMinus,
		Single valWarningHigh,
		String text)
	{
		Single ratio = Value2Ratio(val, min, max);
		Single ratioTarget = Value2Ratio(valTarget, min, max);
		Single ratioRange = Value2Ratio(min + valRangePlusMinus, min, max);
		Single ratioWarning = Value2Ratio(valWarningHigh, min, max);

		UnityEngine.GUI.Box(rect, "", barBorderStyle);
		rect.xMin++;
		rect.yMin++;
		rect.xMax--;
		rect.yMax--;
		UnityEngine.GUI.Box(rect, "", barBackgroundStyle);
		if (Event.current.type == EventType.Repaint) {
			if (ratio > 0.0f) {
				Rect barRect = rect;
				if (ratio > ratioWarning) {
					barRect.width = rect.width * ratio;
					UnityEngine.GUI.Box(barRect, "", barWarningHighStyle);
				}
				if (ratio > ratioTarget + ratioRange) {
					barRect.width = rect.width * Mathf.Min(ratio, ratioWarning);
					UnityEngine.GUI.Box(barRect, "", barWarningHighTolerantStyle);
				}
				if (ratio > ratioTarget - ratioRange) {
					barRect.width = rect.width * Mathf.Min(ratio, ratioTarget + ratioRange);
					UnityEngine.GUI.Box(barRect, "", barMidRangeStyle);
				}
				if (ratio > 0) {
					barRect.width = rect.width * Mathf.Max(0, Mathf.Min(ratio, ratioTarget - ratioRange));
					UnityEngine.GUI.Box(barRect, "", barWarningLowStyle);
				}
			}
		}
		rect.xMin += 2;
		UnityEngine.GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, barTextShadowStyle);
		UnityEngine.GUI.Label(rect, text, barTextStyle);
	}

	/* Positive values only */
	private void LineGraph(Rect rect, string title, float[] data, float maxVal, int sections, float minInterVal, bool adaptiveScale) {
		const float titleHeight = 20;
		const float labelWidth = 30;
		const float labelHeight = 10.0f;
		float cellHeight = 0;
		int count = 0;
		Rect labelRect;

		float currentMaxVal = maxVal;
		if(adaptiveScale == true) {
			currentMaxVal = Mathf.Min(Mathf.Max(data) * 2.0f, maxVal);
		}
		float interVal = (int)(currentMaxVal / sections);
		if (interVal < minInterVal) {
			interVal = minInterVal;
		}

		GUI.Box(rect, "", barBorderStyle);
		rect.xMin++;
		rect.yMin++;
		rect.xMax--;
		rect.yMax--;
		GUI.Box(rect, "", barBackgroundStyle);
		labelRect = new Rect(rect.x + textOffsetChartTitle.x, rect.y + textOffsetChartTitle.y, rect.width, titleHeight);
		GUI.Label(labelRect, title, barTextStyle);
		rect.xMin += labelWidth;
		rect.yMin += titleHeight;
		GUI.Box(rect, "", barBorderStyle);
		rect.xMin++;
		rect.yMin++;
		GUI.Box(rect, "", barBackgroundStyle);

		if (mat != null && Event.current.type == EventType.Repaint) {
			GUI.BeginClip(rect);
			GL.PushMatrix();
			{
				mat.SetPass(0);

				/* grid */
				GL.Begin(GL.LINES);
				cellHeight = rect.height * interVal / currentMaxVal;
				count = (int)(currentMaxVal / interVal);
				if(currentMaxVal % interVal == 0) {
					count -= 1;
				}
				GL.Color(colorGrid);
				float lineYPos = 0;
				for (int i = 1; i <= count; i++) {
					lineYPos = rect.height - i * cellHeight;
					GL.Vertex3(0, lineYPos, 0);
					GL.Vertex3(rect.width, lineYPos, 0);
				}
				GL.End();

                /* data */
#if UNITY_5_6_OR_NEWER
                GL.Begin(GL.LINE_STRIP);
                for (int i = 0; i < data.Length; i++) {
					GL.Color(ColorSelector(data[i], maxVal));
					GL.Vertex3((rect.width / (data.Length - 1)) * i, rect.height * (1 - data[i] / currentMaxVal), 0);
				}
				GL.End();
#else
                GL.Begin(GL.LINES);
                for (int i = 1; i < data.Length; i++)
                {
                    GL.Color(ColorSelector(data[i - 1], maxVal));
                    GL.Vertex3((rect.width / (data.Length - 1)) * (i - 1), rect.height * (1 - data[i - 1] / currentMaxVal), 0);
                    GL.Color(ColorSelector(data[i], maxVal));
                    GL.Vertex3((rect.width / (data.Length - 1)) * i, rect.height * (1 - data[i] / currentMaxVal), 0);
                }
                GL.End();
#endif
			}
			GL.PopMatrix();
			GUI.EndClip();

			/* labels*/
			rect.xMin -= labelWidth;
			labelRect = new Rect(rect.x + textOffsetCalibrator.x, rect.y + textOffsetCalibrator.y, labelWidth, labelHeight);
			GUI.Label(labelRect, currentMaxVal.ToString("F0"), chartLabelStyle);
			for (int i = 1; i <= count; i++) {
				labelRect.y = rect.y + rect.height - i * cellHeight + textOffsetCalibrator.y;
				if (labelRect.y > rect.y) {
					GUI.Label(labelRect, (interVal * i).ToString("F0"), chartLabelStyle);
				}
			}
		} /* if Event == Repaint */
	}

	private void NumericMeter(UnityEngine.Rect rect, String meterLabel, Single meterVal) {
		const Single titleHeight = 20;
		Rect labelRect;
		Rect numRect;

		UnityEngine.GUI.Box(rect, "", barBorderStyle);
		rect.xMin++;
		rect.yMin++;
		rect.xMax--;
		rect.yMax--;
		UnityEngine.GUI.Box(rect, "", barBackgroundStyle);

		labelRect = new Rect(rect.x + textOffsetChartTitle.x, rect.y + textOffsetChartTitle.y, rect.width, titleHeight);
		GUI.Label(labelRect, meterLabel, barTextStyle);
		numRect = new Rect(rect.center.x, rect.center.y, 10, 10);
		if (meterVal > currentDbWarning) {
			GUI.Label(numRect, Dbfs2String(meterVal, "F1"), meterTextWarningHighStyle);
		} else if (meterVal > currentDbTarget + currentDbRangePlusMinus) {
			GUI.Label(numRect, Dbfs2String(meterVal, "F1"), meterTextWarningHighTolerantStyle);
		} else if (meterVal > currentDbTarget - currentDbRangePlusMinus) {
			GUI.Label(numRect, Dbfs2String(meterVal, "F1"), meterTextMidRangeStyle);
		} else {
			GUI.Label(numRect, Dbfs2String(meterVal, "F1"), meterTextWarningLowStyle);
		}
	}

	private enum MultiChannelMeterType {
		Peak,
		RMS
	}
	private void MultiChannelMeter(
		UnityEngine.Rect rect,
		CriProfiler.LevelInfo[] info,
		int numCh,
		MultiChannelMeterType type,
		float min,
		float max,
		float warningThresh,
		string title)
	{
		float warningRatio = (warningThresh - min) / (max - min);

		const float barInterval = 20.0f;
		const float calWidth = 30.0f;
		const float calHeight = 10.0f;
		const float titleHeight = 20.0f;
		const float headHeight = 20.0f;
		const float footHeight = 20.0f;
		const float baseValue = 0.0f;
		Color barColor = Color.green;
		Color barWarningColor = Color.red;
		Color holderColor = Color.green;
		Color holderWarningColor = Color.red;
		Rect chartRect;

		GUI.Box(rect, "", barBorderStyle);
		rect.xMin++;
		rect.yMin++;
		rect.xMax--;
		rect.yMax--;
		GUI.Box(rect, "", barBackgroundStyle);

		chartRect = rect;
		chartRect.xMin += calWidth;
		chartRect.xMax++;
		chartRect.yMin += titleHeight + headHeight;
		chartRect.yMax -= footHeight;
		GUI.Box(chartRect, "", barBorderStyle);
		chartRect.xMin++;
		chartRect.yMin++;
		chartRect.xMax--;
		chartRect.yMax--;
		GUI.Box(chartRect, "", barBackgroundStyle);

		/* title */
		GUI.Label(new Rect(rect.x + textOffsetChartTitle.x, rect.y + textOffsetChartTitle.y, rect.width, rect.height), title, barTextStyle);
		rect.yMin += titleHeight;

		/* horizontal labels */
		float barWidth = (chartRect.width - barInterval * (numCh + 1)) / numCh;
		float labelXStart = calWidth + 1 + barInterval;
		if (info != null && info.Length >= numCh) {
			for (int i = 0; i < numCh; ++i) {
				switch (type) {
					case MultiChannelMeterType.Peak:
						GUI.Label(new Rect(rect.x + labelXStart, rect.y, barWidth, headHeight), Dbfs2String(Level2Db(info[i].levelPeak), "F1"), multiChMeterTextStyle);
						break;
					case MultiChannelMeterType.RMS:
						GUI.Label(new Rect(rect.x + labelXStart, rect.y, barWidth, headHeight), Dbfs2String(Level2Db(info[i].levelRms), "F1"), multiChMeterTextStyle);
						break;
					default:
						break;
				}
				GUI.Label(new Rect(rect.x + labelXStart, rect.y + rect.height - footHeight, barWidth, footHeight), i < S_ChannelNames.Length ? S_ChannelNames[i] : "CH" + (i + 1), multiChMeterTextStyle);
				labelXStart += barInterval + barWidth;
			}
		}

		/* vertical labels */
		rect.yMin += headHeight + 1;
		rect.yMax -= footHeight + 1;
		float cellHeight = rect.height * currentLevelCalInterval / (currentLevelCeiling - currentLevelFloor);

		float firstCellInterval = ((currentLevelCeiling - baseValue) % currentLevelCalInterval + currentLevelCalInterval) % currentLevelCalInterval;
		if (firstCellInterval == 0) {
			firstCellInterval = currentLevelCalInterval;
		}
		float firstCellHeight = rect.height * firstCellInterval / (currentLevelCeiling - currentLevelFloor);

		float remainingInterval = currentLevelCeiling - firstCellInterval - currentLevelFloor;
		int cellCount = (int)(remainingInterval / currentLevelCalInterval);
		if (remainingInterval % currentLevelCalInterval == 0) {
			cellCount -= 1;
		}

		GUI.Label(new Rect(rect.x + textOffsetCalibrator.x, rect.y + textOffsetCalibrator.y, calWidth, calHeight), currentLevelCeiling.ToString("F0"), chartLabelStyle);
		GUI.Label(new Rect(rect.x + textOffsetCalibrator.x, rect.y + rect.height + textOffsetCalibrator.y, calWidth, calHeight), currentLevelFloor.ToString("F0"), chartLabelStyle);
		for (int i=0; i < cellCount + 1; ++i) {
			GUI.Label(new Rect(rect.x + textOffsetCalibrator.x, rect.y + firstCellHeight + cellHeight * i + textOffsetCalibrator.y, calWidth, calHeight), (currentLevelCeiling - firstCellInterval - currentLevelCalInterval * i).ToString("F0"), chartLabelStyle);
		}

		/* GL drawing */
		if (mat != null && Event.current.type == EventType.Repaint) {
			GUI.BeginClip(chartRect);
			GL.PushMatrix();
			{
				mat.SetPass(0);

				/* grid */
				float lineYPos = 0;
				GL.Begin(GL.LINES);
				GL.Color(colorGrid);
				for (int i = 0; i < cellCount + 1; ++i) {
					lineYPos = firstCellHeight + i * cellHeight;
					GL.Vertex3(0, lineYPos, 0);
					GL.Vertex3(chartRect.width, lineYPos, 0);
				}
				GL.End();

				/* bars */
				if (info != null && info.Length >= numCh && barInterval * (numCh + 1) < chartRect.width) {
					float barHeight;
					float barWarningHeight;
					float barXStart;
					for (int i = 0; i < numCh; ++i) {
						float heightRatio = 0;
						switch (type) {
							case MultiChannelMeterType.Peak:
								heightRatio = ((Level2Db(info[i].levelPeak) - currentLevelFloor) / (currentLevelCeiling - currentLevelFloor));
								break;
							case MultiChannelMeterType.RMS:
								heightRatio = ((Level2Db(info[i].levelRms) - currentLevelFloor) / (currentLevelCeiling - currentLevelFloor));
								break;
							default:
								break;
						}
						if (heightRatio > 1.0f) {
							heightRatio = 1.0f;
						}
						if (heightRatio < 0) {
							heightRatio = 0;
						}
						barHeight = chartRect.height * heightRatio;
						barWarningHeight = chartRect.height * warningRatio;
						barXStart = (barInterval + barWidth) * i + barInterval;
						GL.Begin(GL.QUADS);
						if (heightRatio > warningRatio) {
							GL.Color(barWarningColor);
							GL.Vertex3(barXStart, chartRect.height, 0);
							GL.Vertex3(barXStart + barWidth, chartRect.height, 0);
							GL.Vertex3(barXStart + barWidth, chartRect.height - barHeight, 0);
							GL.Vertex3(barXStart, chartRect.height - barHeight, 0);
						}
						GL.Color(barColor);
						GL.Vertex3(barXStart, chartRect.height, 0);
						GL.Vertex3(barXStart + barWidth, chartRect.height, 0);
						GL.Vertex3(barXStart + barWidth, chartRect.height - Mathf.Max(0, Mathf.Min(barHeight, barWarningHeight)), 0);
						GL.Vertex3(barXStart, chartRect.height - Mathf.Max(0, Mathf.Min(barHeight, barWarningHeight)), 0);
						GL.End();

						/* holders */
						if (type == MultiChannelMeterType.Peak) {
							heightRatio = ((Level2Db(info[i].levelPeakHold) - currentLevelFloor) / (currentLevelCeiling - currentLevelFloor));
							if (heightRatio > 1.0f) {
								heightRatio = 1.0f;
							}
							if (heightRatio < 0) {
								heightRatio = 0;
							}
							barHeight = chartRect.height * heightRatio;
							GL.Begin(GL.LINES);
							if (heightRatio > warningRatio) {
								GL.Color(holderWarningColor);
							} else {
								GL.Color(holderColor);
							}
							GL.Vertex3(barXStart, chartRect.height - barHeight, 0);
							GL.Vertex3(barXStart + barWidth, chartRect.height - barHeight, 0);
							GL.End();
						}
					}
				}
			}
			GL.PopMatrix();
			GUI.EndClip();
		}
	}

	#endregion Drawing Functions

	#region Utilities

	static private bool CriFoldout(bool foldout, string content) {
#if UNITY_5_5_OR_NEWER
		return EditorGUILayout.Foldout(foldout, content, true);
#else
        return EditorGUILayout.Foldout(foldout, content);
#endif
	}

	static private Single Level2Db(Single level) {
		return Mathf.Log10(level) * 20.0f;
	}

	static private Single Value2Ratio(Single val, Single min, Single max) {
		if (max - min == 0.0f) {
			return 0.0f;
		}
		if (val > max) {
			return 1.0f;
		}
		if (val < min) {
			return 0.0f;
		}
		return (val - min) / (max - min);
	}

	static private string Dbfs2String(Single dbVal, string format) {
		if (dbVal < Single.MinValue) {
			return "---";
		} else {
			return dbVal.ToString(format);
		}
	}

	static private string MicroSec2String(ulong microSec) {
		ulong hour = microSec / 3600000000;
		ulong min = (microSec % 3600000000) / 60000000;
		ulong sec = (microSec % 60000000) / 1000000;
		ulong msec = (microSec % 1000000) / 1000;
		return string.Format("{0}:{1}:{2}.{3}", hour, min, sec, msec);
	}

	/* Only to use on the first layer of OnGUI */
	private StringBuilder ClearedStr {
		get {
			this.gStrBuilder.Length = 0;
			return this.gStrBuilder;
		}
	}

	private string DataSize2String(float sizeByte) {
		gStrBuilderSub.Length = 0;
		if (sizeByte > 524288.0f) {
			return gStrBuilderSub.AppendFormat("{0:F2}", sizeByte / 1048576.0f).Append(" MB").ToString();
		} else if (sizeByte > 512.0f) {
			return gStrBuilderSub.AppendFormat("{0:F2}", sizeByte / 1024.0f).Append(" KB").ToString();
		} else {
			return gStrBuilderSub.AppendFormat("{0:F0}", sizeByte).Append(" Byte").ToString();
		}
	}

	private Color ColorSelector(float sample, float maxVal) {
		int index = (int)((chartGradientColors.Length - 1) * (maxVal == 0 ? 0 : sample / maxVal));
		Mathf.Clamp(index, 0, chartGradientColors.Length - 1);

		return (chartGradientColors.Length > 0) ? chartGradientColors[index] : Color.white;
	}

	#endregion Utilities
}

} //namespace CriWare