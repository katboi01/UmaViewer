/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2018_1_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace CriWare {

namespace CriTimeline.Mana
{
	[CustomEditor(typeof(CriManaClip)), CanEditMultipleObjects]
	public class CriManaTimelineComponentEditor : UnityEditor.Editor
	{
		private SerializedObject m_object;
		private SerializedProperty m_filePath;
		private SerializedProperty m_loopWithinClip;
		private SerializedProperty m_movieFrameRate;
		private SerializedProperty m_clipDuration;
		private SerializedProperty m_movieData;
		private SerializedProperty m_useOnMemoryPlayback;
		private SerializedProperty m_fadeinDuration;
		private SerializedProperty m_fadeinCurve;
		private SerializedProperty m_fadeoutDuration;
		private SerializedProperty m_fadeoutCurve;
		private SerializedProperty m_fadeAudio;

		public void OnEnable()
		{
			if (target != null) {
				m_object = new SerializedObject(target);
				m_filePath = m_object.FindProperty("m_moviePath");
				m_loopWithinClip = m_object.FindProperty("m_loopWithinClip");
				m_movieFrameRate = m_object.FindProperty("m_movieFrameRate");
				m_clipDuration = m_object.FindProperty("m_clipDuration");
				m_movieData = m_object.FindProperty("m_movieData");
				m_useOnMemoryPlayback = m_object.FindProperty("m_useOnMemoryPlayback");
				m_fadeinDuration = m_object.FindProperty("m_fadeinDuration");
				m_fadeoutDuration = m_object.FindProperty("m_fadeoutDuration");
				m_fadeAudio = m_object.FindProperty("m_fadeAudio");
				m_fadeinCurve = m_object.FindProperty("m_fadeinCurve");
				m_fadeoutCurve = m_object.FindProperty("m_fadeoutCurve");
			}
		}

		public override void OnInspectorGUI()
		{
			if (m_object == null ||
				m_filePath == null ||
				m_loopWithinClip == null) {
				return;
			}

			m_object.Update();
			EditorGUILayout.PropertyField(m_useOnMemoryPlayback);
			EditorGUILayout.PropertyField(m_movieData);
			if (m_useOnMemoryPlayback.boolValue) {
				GUI.enabled = false;
			}

			EditorGUILayout.PropertyField(m_filePath);

			Event evt = Event.current;
			Rect moviePathFieldRect = GUILayoutUtility.GetLastRect();
			int id = GUIUtility.GetControlID(FocusType.Passive);
			switch (evt.type) {
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if (!moviePathFieldRect.Contains(evt.mousePosition)) {
						break;
					}
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					DragAndDrop.activeControlID = id;
					if (evt.type == EventType.DragPerform) {
						DragAndDrop.AcceptDrag();
						foreach (var path in DragAndDrop.paths) {
							if (System.IO.Path.GetExtension(path).Equals(".usm")) {
								string[] splitPath = Regex.Split(path, "Assets/StreamingAssets/");
								if(splitPath.Length < 2) {
									Debug.LogWarning("[Warning] Not in StreamingAssets Folder [" + System.IO.Path.GetFileName(path) + "].");
								} else {
									m_filePath.stringValue = splitPath[1];
								}

							} else {
								Debug.LogWarning("[Warning] Not usm file [" + System.IO.Path.GetFileName(path) + "].");
							}
						}
						DragAndDrop.activeControlID = 0;
					}
					Event.current.Use();
					break;
			}

			EditorGUI.indentLevel++;
			GUI.enabled = false;
			EditorGUILayout.PropertyField(m_movieFrameRate);
			EditorGUILayout.PropertyField(m_clipDuration);
			GUI.enabled = true;
			EditorGUI.indentLevel--;
			EditorGUILayout.PropertyField(m_loopWithinClip);

			if ((target as CriManaClip).m_clip == null) return;
			var duration = (target as CriManaClip).m_clip.duration;
			EditorGUILayout.LabelField("Fade In", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EditorGUILayout.Slider(m_fadeinDuration, 0, (float)duration - m_fadeoutDuration.floatValue, new GUIContent("Duration [sec]"));
			EditorGUILayout.CurveField(m_fadeinCurve, Color.cyan, new Rect(0, 0, 1, 1), new GUIContent("Curve"));
			EditorGUI.indentLevel--;
			EditorGUILayout.LabelField("Fade Out", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EditorGUILayout.Slider(m_fadeoutDuration, 0, (float)duration-m_fadeinDuration.floatValue, new GUIContent("Duration [sec]"));
			EditorGUILayout.CurveField(m_fadeoutCurve, Color.cyan, new Rect(0, 0, 1, 1), new GUIContent("Curve"));
			EditorGUI.indentLevel--;
			EditorGUILayout.PropertyField(m_fadeAudio);

			m_object.ApplyModifiedProperties();
		}
	}

#if UNITY_2019_3_OR_NEWER

	[CustomTimelineEditor(typeof(CriManaClip))]
	public class CriManaClipEditor : ClipEditor
	{
		public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
		{
			var manaClip = clip.asset as CriManaClip;

			var fullRect = region.position;
			fullRect.x = Mathf.LerpUnclamped(region.position.xMax, region.position.xMin, (float)region.endTime / (float)(region.endTime - region.startTime));
			fullRect.width *= (float)clip.duration / (float)(region.endTime - region.startTime);

			var fadeinEnd = fullRect.xMin + fullRect.width * (float)(manaClip.m_fadeinDuration / clip.duration);
			var fadeoutStart = fullRect.xMax - fullRect.width * (float)(manaClip.m_fadeoutDuration / clip.duration);

			GL.Begin(GL.TRIANGLES);
			GL.Color(Color.black);
			GL.Vertex3(fullRect.xMin, fullRect.yMin, 0);
			GL.Vertex3(fadeinEnd, fullRect.yMin, 0);
			GL.Vertex3(fullRect.xMin, fullRect.yMax, 0);
			GL.Vertex3(fullRect.xMax, fullRect.yMin, 0);
			GL.Vertex3(fullRect.xMax, fullRect.yMax, 0);
			GL.Vertex3(fadeoutStart, fullRect.yMin, 0);
			GL.End();

			GL.Begin(GL.LINES);
			GL.Color(Color.gray);
			GL.Vertex3(fadeinEnd, fullRect.yMin, 0);
			GL.Vertex3(fadeinEnd, fullRect.yMax, 0);
			GL.Vertex3(fadeoutStart, fullRect.yMin, 0);
			GL.Vertex3(fadeoutStart, fullRect.yMax, 0);
			GL.End();
		}
	}

#endif

}

} //namespace CriWare

#endif