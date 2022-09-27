/****************************************************************************
 *
 * Copyright (c) 2015 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using UnityEditor;

namespace CriWare {

[CustomEditor(typeof(CriManaMovieMaterial))]
public class CriManaMovieMaterialEditor : UnityEditor.Editor
{
	private CriManaMovieMaterial source = null;

	private void OnEnable()
	{
		source = (CriManaMovieMaterial)base.target;
	}

	public override void OnInspectorGUI()
	{
		if (this.source == null) {
			return;
		}

		Undo.RecordObject(target, null);

		GUI.changed = false;
		{
			EditorGUILayout.PrefixLabel("Startup Parameters");
			++EditorGUI.indentLevel;
			{
				EditorGUI.BeginChangeCheck();
				string moviePath = EditorGUILayout.TextField(new GUIContent("Movie Path", "The path to the movie file."), source.moviePath);
				if (EditorGUI.EndChangeCheck()) {
					source.moviePath = moviePath;
				}
				source.playOnStart = EditorGUILayout.Toggle(new GUIContent("Play On Start", "Immediatly play movie after Start of the component."), source.playOnStart);
				EditorGUI.BeginChangeCheck();
				bool loop = EditorGUILayout.Toggle(new GUIContent("Loop", "Movie is played in continuous loop."), source.loop);
				if (EditorGUI.EndChangeCheck()) {
					source.loop = loop;
				}
				source.restartOnEnable = EditorGUILayout.Toggle(new GUIContent("Restart On Enable", "Restart playback after disabling and then enabling the component."), source.restartOnEnable);
			}
			--EditorGUI.indentLevel;
			EditorGUILayout.PrefixLabel("Render Parameters");
			++EditorGUI.indentLevel;
			{
				EditorGUI.BeginChangeCheck();
				bool additiveMode = EditorGUILayout.Toggle(new GUIContent("Additive Mode", "Movie is rendered in additive blend mode."), source.additiveMode);
				if (EditorGUI.EndChangeCheck()) {
					source.additiveMode = additiveMode;
				}
				source.material = (Material)EditorGUILayout.ObjectField(new GUIContent("Material",
					"The material to render movie.\n" +
					"If 'none' use an internal default material."), source.material, typeof(Material), true);
				source.renderMode = (CriManaMovieMaterial.RenderMode)EditorGUILayout.EnumPopup(new GUIContent("Render Mode",
					"- Always: Render movie at each frame.\n" +
					"- OnVisibility: Render movie only when owner GameObject is visible. Optimization when movie is not visible on screen.\n" +
					"- Never: Never render movie to the material. You need to call 'RenderMovie()' to control rendering."), source.renderMode);
				source.maxFrameDrop = (CriManaMovieMaterial.MaxFrameDrop)EditorGUILayout.EnumPopup(new GUIContent("Max Frame Drop",
					"- Disabled: Disable frame dropping.\n" +
					"- One~Ten: Drops successively one or more frames at maximum if not in-sync.\n" +
					"- Infinite: Drops all frames until playback is in-sync.\n" +
					"Default is 'Disabled'."), source.maxFrameDrop);
            }
			--EditorGUI.indentLevel;
		}
		if (GUI.changed) {
			EditorUtility.SetDirty(this.source);
		}
	}
}

} //namespace CriWare