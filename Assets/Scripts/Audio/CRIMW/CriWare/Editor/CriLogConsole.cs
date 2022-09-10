/****************************************************************************
 *
 * Copyright (c) CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CriWare.Editor {

	public class CriLogConsole : EditorWindow
	{
		private const int logLineCount = 2;
		private const int logBoxHeight = logLineCount * 16;
		private const string PATTERN_W = "W[0-9]{8,12}M{0,1}";
		private const string PATTERN_E = "E[0-9]{8,12}M{0,1}";

		private Rect upperArea;
		private Rect lowerArea;
		private Rect dividingLine;
		private Rect menuBar;

		private Texture2D selectedBoxBg;
		private Texture2D oddBoxBg;
		private Texture2D infoIcon;
		private Texture2D errorIcon;
		private Texture2D warningIcon;

		private float sizeRatio = 0.5f;
		private float middleLineHeight = 10f;
		private float menuBarHeight = 20f;

		private bool repaintDividingLine;
		private bool isManualButtonActive = false;
		private bool needToResetScrollBar = true;

		private Vector2 lowerPanelScroll;
		private Vector2 upperPanelScrollPos;

		private GUIStyle dividingLineStyle;
		private GUIStyle lowerAreaStyle;
		private GUIStyle upperAreaLogStyle;

		private string manualServerUrl = "https://docs.criware.jp/error/common/";
		private string[] optionLanguage = { "jpn", "eng" };
		private int optionLanguageIndex;
		private List<LogData> logDataList;
		private LogData selectedLog;

		[MenuItem("Window/CRIWARE/CRI Log Console")]
		private static void OpenWindow() {
			CriLogConsole criLogConsole = GetWindow<CriLogConsole>();
			criLogConsole.titleContent = new GUIContent("CRI Log Console");
		}

		private class LogData {
			public bool isSelected;
			public string logContent;
			public LogType type;
			public string errorNumber;

			public LogData(bool isSelected, string logContent, LogType type, string errorNumber) {
				this.isSelected = isSelected;
				this.logContent = logContent;
				this.type = type;
				this.errorNumber = errorNumber;
			}

			public LogData(bool isSelected, string logContent, LogType type) {
				this.isSelected = isSelected;
				this.logContent = logContent;
				this.type = type;

				var matchW = Regex.Match(logContent, PATTERN_W);
				var matchE = Regex.Match(logContent, PATTERN_E);
				if(matchW.Success) {
					errorNumber = matchW.ToString();
				} else if(matchE.Success) {
					errorNumber = matchE.ToString();
				} else {
					errorNumber = "";
				}
			}
		}

		private void OnEnable() {
			errorIcon = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;
			warningIcon = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;
			infoIcon = EditorGUIUtility.Load("icons/console.infoicon.png") as Texture2D;

			dividingLineStyle = new GUIStyle();
			upperAreaLogStyle = new GUIStyle();
			lowerAreaStyle = new GUIStyle();
			dividingLineStyle.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;
			upperAreaLogStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
			lowerAreaStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
			lowerAreaStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/projectbrowsericonareabg.png") as Texture2D;

			oddBoxBg = EditorGUIUtility.Load("builtin skins/darkskin/images/cn entrybackodd.png") as Texture2D;
			selectedBoxBg = EditorGUIUtility.Load("builtin skins/darkskin/images/menuitemhover.png") as Texture2D;

			if (logDataList == null) {
				logDataList = new List<LogData>();
			}
			selectedLog = null;
			Application.logMessageReceived += OnLogMessageReceived;
		}

		private void Awake() {
			logDataList = new List<LogData>();
			ParseUnityLogFile(CriWareErrorHandler.logPrefix);
		}

		private void OnDisable() {
			Application.logMessageReceived -= OnLogMessageReceived;
		}

		private void OnDestroy() {
			Application.logMessageReceived -= OnLogMessageReceived;
		}

		private void OnGUI() {
			GenerateManualBar();
			GenerateUpperArea();
			GenerateLowerArea();
			GenerateDividingLine();
			DividingLineEvent(Event.current);
			if (GUI.changed) {
				Repaint();
			}
		}

		private void GenerateManualBar() {
			menuBar = new Rect(0, 0, position.width, menuBarHeight);
			GUILayout.BeginArea(menuBar, EditorStyles.toolbar);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Clear"), EditorStyles.toolbarButton, GUILayout.Width(50))) {
				logDataList.Clear();
				selectedLog = null;
				isManualButtonActive = false;
			}
			GUILayout.Space(5);

			GUI.enabled = isManualButtonActive;
			if (GUILayout.Button(new GUIContent("Open Online Manual"), EditorStyles.toolbarButton, GUILayout.Width(130))) {
				if (selectedLog != null) {
					ShowManualFromBrowser(selectedLog.errorNumber);
				}
			}
			GUI.enabled = true;

			optionLanguageIndex = EditorGUILayout.Popup("Language", optionLanguageIndex, optionLanguage);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		private void GenerateUpperArea() {
			upperArea = new Rect(0, menuBarHeight, position.width, (position.height * sizeRatio) - menuBarHeight);
			GUILayout.BeginArea(upperArea);
			if (needToResetScrollBar) {
				upperPanelScrollPos = new Vector2(0, position.height * sizeRatio);
				needToResetScrollBar = false;
			}
			using (var upperPanelScope = new EditorGUILayout.ScrollViewScope(upperPanelScrollPos, GUILayout.ExpandHeight(true))) {
				upperPanelScrollPos = upperPanelScope.scrollPosition;
				int listNumInView = ((int)(position.height * sizeRatio) - (int)menuBarHeight) / logBoxHeight;
				int firstLogId = Mathf.Clamp((int)upperPanelScrollPos.y / logBoxHeight, 0, Mathf.Max(0, logDataList.Count - listNumInView));
				GUILayout.Space(logBoxHeight * firstLogId);
				for (int i = firstLogId; i < Mathf.Min(firstLogId + listNumInView, logDataList.Count); ++i) {
					string InfoShownOnBox = "";
					StringReader rs = new System.IO.StringReader(logDataList[i].logContent);
					for (int x = 0; x < logLineCount; ++x) {
						InfoShownOnBox = InfoShownOnBox + rs.ReadLine();
						if (x != logLineCount - 1) {
							InfoShownOnBox = InfoShownOnBox + "\r\n";
						}
					}
					if (GenerateLogBox(InfoShownOnBox, logDataList[i].type, i % 2 == 0, logDataList[i].isSelected)) {
						if (selectedLog != null) {
							selectedLog.isSelected = false;
						}
						logDataList[i].isSelected = true;
						selectedLog = logDataList[i];
						if (selectedLog.errorNumber != "") {
							isManualButtonActive = true;
						} else {
							isManualButtonActive = false;
						}
					}
				}
				GUILayout.Space(Mathf.Max(0, logDataList.Count - firstLogId - listNumInView) * logBoxHeight);
			}
			GUILayout.EndArea();
		}

		private void GenerateLowerArea() {
			lowerArea = new Rect(0, position.height * sizeRatio, position.width, position.height * (1 - sizeRatio));
			GUILayout.BeginArea(lowerArea);
			lowerPanelScroll = GUILayout.BeginScrollView(lowerPanelScroll);
			if (selectedLog != null) {
				GUILayout.TextArea(selectedLog.logContent, lowerAreaStyle);
			}
			if (isManualButtonActive) {
				if (GUILayout.Button(new GUIContent("Open Online Manual"), GUILayout.Width(130))) {
					ShowManualFromBrowser(selectedLog.errorNumber);
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		private void GenerateDividingLine() {
			dividingLine = new Rect(0, (position.height * sizeRatio) - middleLineHeight, position.width, middleLineHeight * 2);
			GUILayout.BeginArea(new Rect(dividingLine.position + (Vector2.up * middleLineHeight), new Vector2(position.width, 2)), dividingLineStyle);
			GUILayout.EndArea();
			EditorGUIUtility.AddCursorRect(dividingLine, MouseCursor.ResizeVertical);
		}

		private void DividingLineEvent(Event mouseEvent) {
			if (mouseEvent.type == EventType.MouseDown && mouseEvent.button == 0 && dividingLine.Contains(mouseEvent.mousePosition)) {
				sizeRatio = mouseEvent.mousePosition.y / position.height;
				Repaint();
			}
		}

		private bool GenerateLogBox(string content, LogType boxType, bool isOdd, bool isSelected) {
			Texture2D icon = null;
			upperAreaLogStyle.normal.background = isSelected ? selectedBoxBg : oddBoxBg;
			switch (boxType) {
				case LogType.Error:
					icon = errorIcon;
					break;
				case LogType.Warning:
					icon = warningIcon;
					break;
				case LogType.Log:
					icon = infoIcon;
					break;
				default:
					break;
			}
			GUIContent buttonContent = new GUIContent(content, icon);
			return GUILayout.Button(buttonContent, upperAreaLogStyle, GUILayout.ExpandWidth(true), GUILayout.Height(logBoxHeight));
		}

		private bool IsCriLogCheck(string log) {
			return log.Contains(CriWareErrorHandler.logPrefix);
		}

		private void OnLogMessageReceived(string condition, string stackTrace, LogType type) {
			bool isCri = IsCriLogCheck(condition);
			if (!isCri) {
				return;
			}
			string templogstring = condition + "\n" + stackTrace;
			LogData tmpLog = new LogData(false, templogstring, type);
			logDataList.Add(tmpLog);
			needToResetScrollBar = true;
			Repaint();
		}

		private void ShowManualFromBrowser(string errorNum) {
			Application.OpenURL(GenerateManualUrl(errorNum));
		}

		private string GenerateManualUrl(string errorNum) {
			return manualServerUrl + optionLanguage[optionLanguageIndex] + "/cri_external_error_list.html#" + errorNum;
		}

		private void ParseUnityLogFile(string keyword) {
			bool foundCriLog = false;
			string criTmpFilePath = System.IO.Path.Combine(Path.GetTempPath() + "/Editor.log");
			try {
				File.Copy(Application.consoleLogPath, criTmpFilePath);
				string parsedLog = "";
				foreach (string line in File.ReadLines(criTmpFilePath)) {
					if (!foundCriLog) {
						if (line.Contains(keyword)) {
							parsedLog = line + "\r\n";
							foundCriLog = true;
						}
					} else {
						if (line != "") {
							parsedLog = parsedLog + line + "\r\n";
						} else {
							CategorizeLog(parsedLog);
							foundCriLog = false;
						}
					}
				}
			} catch (IOException copyError) {
				Debug.Log(copyError.Message);
				return;
			}

			try {
				File.Delete(criTmpFilePath);
			} catch (IOException deleteError) {
				Debug.Log(deleteError.Message);
			}
		}

		private void CategorizeLog(string log) {
			var matchW = Regex.Match(log, PATTERN_W);
			var matchE = Regex.Match(log, PATTERN_E);
			if(matchW.Success) {
				logDataList.Add(new LogData(false, log, LogType.Warning, matchW.ToString()));
			} else if(matchE.Success) {
				logDataList.Add(new LogData(false, log, LogType.Error, matchE.ToString()));
			} else {
				logDataList.Add(new LogData(false, log, LogType.Log, ""));
			}
		}
	}

} //namespace CriWare.Editor