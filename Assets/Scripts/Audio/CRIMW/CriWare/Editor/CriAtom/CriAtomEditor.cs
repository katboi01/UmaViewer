/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using UnityEditor;
using System;

namespace CriWare {

[CustomEditor(typeof(CriAtom))]
public class CriAtomEditor : UnityEditor.Editor
{
	#region Variables
	private CriAtom atom = null;
	#endregion

	#region GUI Functions
	private void OnEnable()
	{
		atom = (CriAtom)base.target;
	}

	public override void OnInspectorGUI()
	{
		if (atom == null) {
			return;
		}

		Undo.RecordObject(target, null);

		GUI.changed = false;
		{
			atom.acfFile       = EditorGUILayout.TextField("ACF File", atom.acfFile);
			atom.dspBusSetting = EditorGUILayout.TextField("DSP Bus Setting", atom.dspBusSetting);

			for (int i = 0; i < atom.cueSheets.Length; i++) {
				var cueSheet = atom.cueSheets[i];
				EditorGUILayout.BeginVertical("HelpBox");
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Cue Sheet");
				if (GUILayout.Button("Remove")) {
					atom.RemoveCueSheetInternal(cueSheet.name);
					break;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel++;
				cueSheet.name = EditorGUILayout.TextField("Name", cueSheet.name);
				cueSheet.acbFile = EditorGUILayout.TextField("ACB File", cueSheet.acbFile);
				cueSheet.awbFile = EditorGUILayout.TextField("AWB File", cueSheet.awbFile);
				EditorGUI.indentLevel--;
				EditorGUILayout.EndVertical();
			}
			if (GUILayout.Button("Add CueSheet")) {
				atom.AddCueSheetInternal("", "", "", null);
			}

			atom.dontRemoveExistsCueSheet = EditorGUILayout.Toggle("Dont Remove Exists CueSheet", atom.dontRemoveExistsCueSheet);
			atom.dontDestroyOnLoad        = EditorGUILayout.Toggle("Dont Destroy On Load", atom.dontDestroyOnLoad);
		}
		if (GUI.changed) {
			EditorUtility.SetDirty(atom);
		}
	}
	#endregion

	#region Editor Utilities
	/**
	 * \deprecated
	 * This is a deprecated API that will be removed.
	 * Please consider using CriAtomEditorUtilities.InitializeLibrary() instead.
	*/
	[Obsolete("Consider using CriAtomEditorUtilities.InitializeLibrary() instead.")]
	public static void InitializePluginForEditor() {
		CriWare.Editor.CriAtomEditorUtilities.InitializeLibrary();
	}
	
	/**
	 * \deprecated
	 * This is a deprecated API that will be removed.
	 * Please consider using CriAtomEditorUtilities.PreviewPlayer instead.
	*/
	[Obsolete("Consider using CriAtomEditorUtilities.PreviewPlayer instead.")]
	public class PreviewPlayer : CriWare.Editor.CriAtomEditorUtilities.PreviewPlayer { }
	#endregion
} // end of class

} //namespace CriWare

/* end of file */