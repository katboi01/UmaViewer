/****************************************************************************
 *
 * Copyright (c) CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace CriWare.Editor { 

	public class CriAtomEditorSettingsProvider : SettingsProvider
	{
		static readonly string settingPath = "Project/CRIWARE/Editor/Atom Preview";

		public CriAtomEditorSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

		public override void OnGUI(string searchContext) {
			EditorGUI.indentLevel++;
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Library Initialization Settings for Audio Previewing in the Editor", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EditorGUILayout.Space();
			CriAtomEditorSettings.Instance.EditorInstance.OnInspectorGUI();
			EditorGUI.indentLevel -= 2;
		}

		[SettingsProvider]
		static SettingsProvider Create() {
			var provider = new CriAtomEditorSettingsProvider(settingPath, SettingsScope.Project);
			return provider;
		}
	} //class CriAtomEditorSettingsProvider

	public class CriAtomEditorSettings : ScriptableObject
	{
		static readonly string SettingsDirPath = "Assets/CriData/Settings";
		static CriAtomEditorSettings instance = null;

		private UnityEditor.Editor editorInstance = null;
		internal UnityEditor.Editor EditorInstance {
			get {
				if (editorInstance == null) {
					editorInstance = UnityEditor.Editor.CreateEditor(this);
				}
				return editorInstance;
			}
		}

		private bool hasSettingsChanged = false;
		internal void SetChangeFlag() { hasSettingsChanged = true; }
		internal bool GetChangeStatusOnce() {
			bool currentChangeStatus = hasSettingsChanged;
			hasSettingsChanged = false;
			return currentChangeStatus;
		}

		[SerializeField]
		private bool trySceneSettings = false;
		internal bool TrySceneSettings { get { return trySceneSettings; } }

		[SerializeField]
		private CriAtomConfig atomConfig = new CriAtomConfig();
		internal CriAtomConfig AtomConfig { get { return atomConfig; } }

		internal static CriAtomEditorSettings Instance {
			get {
				if (instance == null) {
					var guids = AssetDatabase.FindAssets("t:" + typeof(CriAtomEditorSettings).Name);
					if (guids.Length <= 0) {
						if (!System.IO.Directory.Exists(SettingsDirPath)) {
							System.IO.Directory.CreateDirectory(SettingsDirPath);
						}
						instance = CreateInstance<CriAtomEditorSettings>();
						AssetDatabase.CreateAsset(instance, System.IO.Path.Combine(SettingsDirPath, typeof(CriAtomEditorSettings).Name + ".asset"));
					} else {
						var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
						if (guids.Length > 1) {
							Debug.LogWarning("[CRIWARE] Multiple setting files founded. Using " + assetPath);
						}
						instance = AssetDatabase.LoadAssetAtPath<CriAtomEditorSettings>(assetPath);
					}
				}
				return instance;
			}
		}

		internal void ResetConfig() {
			atomConfig = new CriAtomConfig();
		}
	} //class CriAtomEditorSettings

	[CustomEditor(typeof(CriAtomEditorSettings))]
	public class CriAtomEditorSettingsEditor : UnityEditor.Editor
	{
		private SerializedProperty trySceneSettingsProp;
		private List<SerializedProperty> atomConfigProps = new List<SerializedProperty>();

		private void OnEnable() {
			trySceneSettingsProp = serializedObject.FindProperty("trySceneSettings");

			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.maxVirtualVoices"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.maxVoiceLimitGroups"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.maxCategories"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.maxSequenceEventsPerFrame"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.maxBeatSyncCallbacksPerFrame"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.maxCueLinkCallbacksPerFrame"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.standardVoicePoolConfig"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.hcaMxVoicePoolConfig"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.outputSamplingRate"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.serverFrequency"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.asrOutputChannels"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.useRandomSeedWithTime"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.categoriesPerPlayback"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.maxFaders"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.maxBuses"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.maxParameterBlocks"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.vrMode"));
			atomConfigProps.Add(serializedObject.FindProperty("atomConfig.pcBufferingTime"));
		}

		public override void OnInspectorGUI() {
			const float LABEL_WIDTH = 250;
			float prevLabelWidth;

			serializedObject.Update();
			prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = LABEL_WIDTH;
			EditorGUILayout.PropertyField(trySceneSettingsProp);
			if (trySceneSettingsProp.boolValue) {
				EditorGUILayout.HelpBox("Will use project settings if CriLibraryInitializer does not exist in the scene.", MessageType.Info);
			}
			EditorGUILayout.Space();
			foreach (var prop in atomConfigProps) {
				EditorGUILayout.PropertyField(prop);
			}
			EditorGUIUtility.labelWidth = prevLabelWidth;
			if (serializedObject.hasModifiedProperties) {
				(target as CriAtomEditorSettings).SetChangeFlag();
			}
			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space();
			if (GUILayout.Button("Reset to Default")) {
				(target as CriAtomEditorSettings).ResetConfig();
				(target as CriAtomEditorSettings).SetChangeFlag();
			}
		}
	} //class CriAtomEditorSettingsEditor

} //namespace CriWare.Editor