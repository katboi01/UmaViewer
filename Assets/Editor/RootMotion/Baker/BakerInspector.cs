using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.Playables;
using System;

namespace RootMotion
{
    public class BakerInspector : Editor
    {

        protected void DrawKeyframeSettings(Baker script)
        {
            if (script.isBaking) return;
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("frameRate"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keyReductionError"));
        }

        protected void DrawModeSettings(Baker script)
        {
            if (script.isBaking) return;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"));

            switch (script.mode)
            {
                case Baker.Mode.AnimationClips:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("animationClips"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("appendName"));
                    break;
                case Baker.Mode.AnimationStates:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("loop"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("animationStates"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("appendName"));
                    break;
                case Baker.Mode.PlayableDirector:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("loop"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("saveName"));
                    break;
                default:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("saveName"));
                    break;
            }

            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Save To Folder"));

            if (EditorGUILayout.DropdownButton(new GUIContent(serializedObject.FindProperty("saveToFolder").stringValue, "The folder to save the baked AnimationClips to."), FocusType.Passive, GUILayout.MaxWidth(400)))
            {
                serializedObject.FindProperty("saveToFolder").stringValue = SaveClipFolderPanel.Apply(serializedObject.FindProperty("saveToFolder").stringValue);
            }
            //EditorGUILayout.EndHorizontal();
        }

        private void TryBake(Baker script)
        {
            switch (script.mode)
            {
                case Baker.Mode.AnimationClips:
                    if (script.animator == null)
                    {
                        EditorGUILayout.LabelField("No Animator found on Baker GameObject", EditorStyles.helpBox);
                        return;
                    }

                    if (script.animator.isHuman && script.animator.runtimeAnimatorController == null)
                    {
                        EditorGUILayout.LabelField("Humanoid Animator needs to have a valid Controller assigned for clip baking (Unity crash bug)", EditorStyles.helpBox);
                        return;
                    }

                    if (script.animationClips.Length == 0)
                    {
                        EditorGUILayout.LabelField("Please add AnimationClips to bake", EditorStyles.helpBox);
                        return;
                    }

                    foreach (AnimationClip clip in script.animationClips)
                    {
                        if (clip == null)
                        {
                            EditorGUILayout.LabelField("One of the AnimationClips is null, can not bake.", EditorStyles.helpBox);
                            return;
                        }
                    }

                    if (GUILayout.Button("Bake Animation Clips")) script.BakeClip();
                    return;
                case Baker.Mode.AnimationStates:
                    if (script.animator == null)
                    {
                        EditorGUILayout.LabelField("No Animator found on Baker GameObject", EditorStyles.helpBox);
                        return;
                    }

                    if (script.animationStates.Length == 0)
                    {
                        EditorGUILayout.LabelField("Please add Animation State names to bake. The Animator must contain AnimationStates with matching names. If AnimationState names match with clip names used in them, you can have the Baker fill the names in automatically by right-clicking on the component header and selecting 'Find Animation States'.", EditorStyles.helpBox);
                        return;
                    }

                    for (int i = 0; i < script.animationStates.Length; i++)
                    {
                        if (script.animationStates[i] == string.Empty || script.animationStates[i] == "")
                        {
                            EditorGUILayout.LabelField("One of the animation state names in 'Animation States' is empty, can not bake.", EditorStyles.helpBox);
                            return;
                        }
                    }

                    if (GUILayout.Button("Bake Animation States")) script.BakeClip();
                    return;
                case Baker.Mode.PlayableDirector:
                    if (script.director == null)
                    {
                        EditorGUILayout.LabelField("No PlayableDirector found on Baker GameObject", EditorStyles.helpBox);
                        return;
                    }

                    if (GUILayout.Button("Bake Timeline")) script.BakeClip();
                    break;
                case Baker.Mode.Realtime:
                    if (GUILayout.Button("Start Baking")) script.StartBaking();
                    return;
            }
        }

        protected void DrawButtons(Baker script)
        {
            if (!script.enabled) return;

            if (script.animator == null)
            {
                serializedObject.FindProperty("animator").objectReferenceValue = script.GetComponent<Animator>();
            }

            if (script.director == null)
            {
                serializedObject.FindProperty("director").objectReferenceValue = script.GetComponent<PlayableDirector>();
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("Enter Play Mode to bake.", EditorStyles.helpBox);
                return;
            }

            if (!script.isBaking)
            {
                TryBake(script);
            }
            else
            {
                GUI.color = Color.red;

                switch (script.mode)
                {
                    case Baker.Mode.Realtime:
                        if (GUILayout.Button("Stop Baking")) script.StopBaking();
                        break;
                    default:
                        GUILayout.Label("Baking Progress: " + System.Math.Round(script.bakingProgress, 2));
                        break;
                }

                GUI.color = Color.white;
                EditorUtility.SetDirty(script);
            }
        }
    }
}
