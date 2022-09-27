/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.IO;
#if (UNITY_5 || UNITY_2017_1_OR_NEWER) && (UNITY_IOS || UNITY_TVOS)
using UnityEditor.iOS.Xcode;
#endif

namespace CriWare {

public class CriWareBuildPostprocessor : ScriptableObject
{
	private static string prefsFilePath;
	public bool iosAddDependencyFrameworks  = true;
	public bool iosReorderLibraryLinkingsForVp9 = true;

	[MenuItem("GameObject/CRIWARE/Create CriWareBuildPostprocessorPrefs.asset")]
	public static void Create()
	{
		CriWareBuildPostprocessor instance = LoadExistingAsset();
		if (instance) {
			Debug.LogError("[CRIWARE] Preferences file of CriWareBuildPostprocessor already exists.");
			Selection.activeObject = instance;
			return;
		}

		var scobj = ScriptableObject.CreateInstance<CriWareBuildPostprocessor>();
		if (scobj == null) {
			Debug.Log("[CRIWARE] Failed to create CriWareBuildPostprocessor");
			return;
		}

		var script = MonoScript.FromScriptableObject(scobj);
		prefsFilePath = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(script)), "CriWareBuildPostprocessorPrefs.asset");

		AssetDatabase.CreateAsset(scobj, prefsFilePath);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log("[CRIWARE] Created the preferences file of CriWareBuildPostprocessor. (" + prefsFilePath + ")");

		Selection.activeObject = scobj;
	}

	private static CriWareBuildPostprocessor LoadExistingAsset()
	{
		string postProcessorPath = "";
		string[] searchResult = AssetDatabase.FindAssets("t:CriWareBuildPostprocessor");
		if (searchResult.Length == 0) {
			return null;
		}
		postProcessorPath = AssetDatabase.GUIDToAssetPath(searchResult[0]);
		var instance = (CriWareBuildPostprocessor)AssetDatabase.LoadAssetAtPath(postProcessorPath, typeof(CriWareBuildPostprocessor));

		return instance;
	}

	[PostProcessScene]
	public static void OnPostProcessScene() {
		CheckGraphicsApiProcess ();
	}


	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget build_target, string path)
	{
		CriWareBuildPostprocessor instance = LoadExistingAsset();
		if (instance == null) {
            instance = ScriptableObject.CreateInstance<CriWareBuildPostprocessor>();
			Debug.Log(
				"[CRIWARE] Run CriWareBuildPostprocessor.OnPostprocessBuild with default preferences.\n"
				+ "If you want to change the preferences, please create the preferences file by 'GameObject/CRIWARE/Create CriWareBuildPostprocessorPrefs.asset' menu."
				);
		} else {
			Debug.Log(
				"[CRIWARE] Run CriWareBuildPostprocessor.OnPostprocessBuild with default preferences.\n"
				+ "If you want to change the preferences, please edit the preferences file (" + prefsFilePath + ")"
				);
		}

		instance.IosAddDependencyFrameworksProcess(build_target, path,
		                                           instance.iosAddDependencyFrameworks,
		                                           instance.iosReorderLibraryLinkingsForVp9);
		switch (build_target) {
			case BuildTarget.WebGL:
				instance.PostprocessBuildWebGL(path);
				break;
		}
	}

	private string GetAssetPath(string assetName, string substring)
	{
		string[] pathStock = AssetDatabase.FindAssets(assetName);
		if (pathStock == null) {
			return null;
		}
		foreach (string path in pathStock) {
			string strPath = AssetDatabase.GUIDToAssetPath(path);
			if (strPath.Contains(substring)) {
				return strPath;
			}
		}
		return null;
	}

	private void IosAddDependencyFrameworksProcess(BuildTarget build_target, string path, bool add_frameworks, bool reorder_linkings)
	{
#if UNITY_IOS || UNITY_TVOS
#if UNITY_5 || UNITY_2017_1_OR_NEWER
		if ((build_target != BuildTarget.iOS && build_target != BuildTarget.tvOS) ||
		    (!add_frameworks && !reorder_linkings)) {
			return;
		}
		string project_path = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
		PBXProject project = new PBXProject();
		project.ReadFromString(File.ReadAllText(project_path));
#if UNITY_2019_3_OR_NEWER
		string target = project.GetUnityFrameworkTargetGuid();
#else
		string target = project.TargetGuidByName("Unity-iPhone");
#endif
		if (add_frameworks) {
			Debug.Log("[CRIWARE][iOS] Add dependency frameworks (VideoToolbox/OpenGLES/Metal)");
			project.AddFrameworkToProject(target, "VideoToolbox.framework", false);
			project.AddFrameworkToProject(target, "OpenGLES.framework", false);
			project.AddFrameworkToProject(target, "Metal.framework", false);
		}
		if (reorder_linkings) {
			bool isVp9Enabled = false;
			string libPartialPath = null;
			if (build_target == BuildTarget.iOS) {
				libPartialPath = "iOS/libcri_mana_vpx.a";
			} else { /* tvOS */
				libPartialPath = "tvOS/libcri_mana_vpx.a";
			}
			string unityLibRelPath = GetAssetPath("libcri_mana_vpx", libPartialPath);
			if (unityLibRelPath == null) {
				isVp9Enabled = false;
			} else {
				if (!(unityLibRelPath.StartsWith("Assets") || unityLibRelPath.StartsWith("Package"))) {
					Debug.LogAssertion("[CRIWARE] Internal: Not supported directory root name : " + unityLibRelPath
						+ "libcri_mana_vpx should be placed in the directory below Assets/ and Package/.");
				}
				unityLibRelPath = unityLibRelPath.Replace("\\", "/");
				string unityRootDirName = unityLibRelPath.Split('/')[0];
				string xcodeProjectRelPath = unityLibRelPath.Replace(unityRootDirName, "Libraries");
				isVp9Enabled = project.ContainsFileByRealPath(xcodeProjectRelPath);
			}
            if (isVp9Enabled) {
				string guid = project.FindFileGuidByRealPath("Libraries/libiPhone-lib.a", PBXSourceTree.Source);
				if (!string.IsNullOrEmpty(guid)) {
					project.RemoveFileFromBuild(target, guid);
					project.AddFileToBuild(target, guid);
					Debug.Log("[CRIWARE][iOS] Reordered linking of libiPhone-lib.a for VP9 support.");
				} else {
					Debug.LogWarning("[CRIWARE][iOS] Failed to reorder linkings of libraries for VP9 support.");
				}
			}
		}

		File.WriteAllText(project_path, project.WriteToString());
#else
		if (build_target != BuildTarget.iPhone) {
			return;
		}
		Debug.LogWarning("[CRIWARE][iOS] Please add dependency frameworks (VideoToolbox.framework, Metal.framework) on Xcode.");
#endif
#endif
	}


	private static bool CheckGraphicsApiProcess() {
		return true;
	}


	private void PostprocessBuildWebGL(string path)
	{
		string SourceDir = GetAssetPath("sofdec2.worker", "WebGL/sofdec2.worker.bin");
		if (SourceDir == null) {
			SourceDir = GetAssetPath("sofdec2.worker", "WebGL2/sofdec2.worker.bin");
			Debug.Assert((SourceDir != null), "[CRIWARE] Internal: Failed to copy WebGL library files.");
		}
		SourceDir = Directory.GetParent(SourceDir).ToString();
		string DestnationDir = path + "/StreamingAssets/";

		bool ret = true;
		try {
			Directory.CreateDirectory(DestnationDir);
			File.Copy(Path.Combine(SourceDir, "sofdec2.worker.bin"),      DestnationDir + "sofdec2.worker.js", true);
			File.Copy(Path.Combine(SourceDir, "sofdec2.worker.wajs.bin"), DestnationDir + "sofdec2wasm.worker.js", true);
			File.Copy(Path.Combine(SourceDir, "sofdec2.worker.wasm.bin"), DestnationDir + "sofdec2.worker.wasm", true);
		}
		catch (System.Exception) {
			Debug.LogError("[CRIWARE][WebGL] Copying sofdec2.worker.js failed.");
			ret = false;
		}
		if (ret) {
			Debug.Log("[CRIWARE][WebGL] Copying sofdec2.worker.js succeeded.");
		}
	}
}

} //namespace CriWare