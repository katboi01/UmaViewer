/****************************************************************************
 *
 * Copyright (c) 2015 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using UnityEditor;

namespace CriWare {

[CustomEditor(typeof(CriManaMovieController))]
public class CriManaMovieControllerEditor : UnityEditor.Editor
{
	private CriManaMovieController source = null;

	private void OnEnable()
	{
		source = (CriManaMovieController)base.target;
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
				EditorGUI.BeginChangeCheck();
                bool advancedAudioMode = EditorGUILayout.BeginToggleGroup(new GUIContent("Advanced Audio Mode", "Enable advanced audio mode."), source.advancedAudio);
                if (EditorGUI.EndChangeCheck())
                {
                    source.advancedAudio = advancedAudioMode;
                }
                if (source.advancedAudio)
                {
                    EditorGUI.indentLevel += 2;
                    EditorGUI.BeginChangeCheck();
                    bool useAmbisonics = EditorGUILayout.Toggle(new GUIContent("Ambisonics", "Use Ambisonics."), source.ambisonics);
                    if (EditorGUI.EndChangeCheck())
                    {
                        source.ambisonics = useAmbisonics;
                    }
                    EditorGUI.indentLevel -= 2;
                }
                EditorGUILayout.EndToggleGroup();
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
				source.renderMode = (CriManaMovieMaterialBase.RenderMode)EditorGUILayout.EnumPopup(new GUIContent("Render Mode",
					"- Always: Render movie at each frame.\n" +
					"- OnVisibility: Render movie only when owner GameObject or Renderer Target is visible.\nOptimization when movie is not visible on screen.\n" +
					"- Never: Never render movie to the material. You need to call 'RenderMovie()' to control rendering."), source.renderMode);
				source.maxFrameDrop = (CriManaMovieMaterialBase.MaxFrameDrop)EditorGUILayout.EnumPopup(new GUIContent("Max Frame Drop",
					"- Disable: Disable frame dropping.\n" +
					"- One~Ten: Drops one or more maximum frames before each rendering if not in-sync.\n" +
					"- Infinite: Drops all frames until playback is in-sync.\n" +
					"Default is 'Two'."), source.maxFrameDrop);

				EditorGUILayout.PrefixLabel("Renderer Control");
				++EditorGUI.indentLevel;
				{
					source.target = (Renderer)EditorGUILayout.ObjectField(new GUIContent("Target", "Renderer object target to override its material with movie material.\nIf 'none', use owner GameObject Renderer if present."), source.target, typeof(Renderer), true);
					source.useOriginalMaterial = EditorGUILayout.Toggle(new GUIContent("Use Original Material", "If set to 'true', use the original Renderer Target material when movie is stopped."), source.useOriginalMaterial);
				}
				--EditorGUI.indentLevel;
			}
			--EditorGUI.indentLevel;
		}
		if (GUI.changed) {
			EditorUtility.SetDirty(this.source);
		}
	}
}

} //namespace CriWare