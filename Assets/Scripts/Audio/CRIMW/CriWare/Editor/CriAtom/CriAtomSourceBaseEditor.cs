using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CriWare {
	public abstract class CriAtomSourceBaseEditor : UnityEditor.Editor
	{
		#region Variables
		protected CriAtomSourceBase source = null;
		private bool showAndroidConfig;
		private GUIStyle style;

		private SerializedProperty m_followsOriginalSource;
		private SerializedProperty m_calculationType;
		private SerializedProperty m_calculationParameters;
		#endregion

		#region Functions
		protected virtual void OnEnable()
		{
			this.source = (CriAtomSourceBase)base.target;
			this.style = new GUIStyle();
			m_followsOriginalSource = serializedObject.FindProperty("randomize3dConfig.followsOriginalSource");
			m_calculationType = serializedObject.FindProperty("randomize3dConfig.calculationType");
			m_calculationParameters = serializedObject.FindProperty("randomize3dConfig.calculationParameters");
		}

		protected abstract void InspectorCueReferenceGUI();
		protected abstract void InspectorPreviewGUI();

		public override void OnInspectorGUI()
		{
			if (this.source == null)
			{
				return;
			}

			Undo.RecordObject(target, null);

			GUI.changed = false;
			{
				EditorGUI.indentLevel++;
				InspectorCueReferenceGUI();
				InspectorPreviewGUI();
				this.source.playOnStart = EditorGUILayout.Toggle("Play On Start", this.source.playOnStart);
				EditorGUILayout.Space();
				this.source.volume = EditorGUILayout.Slider("Volume", this.source.volume, 0.0f, 1.0f);
				this.source.pitch = EditorGUILayout.Slider("Pitch", this.source.pitch, -1200f, 1200);
				this.source.loop = EditorGUILayout.Toggle("Loop", this.source.loop);
				if (this.source.use3dPositioning = EditorGUILayout.Toggle("3D Positioning", this.source.use3dPositioning))
				{
					EditorGUI.indentLevel++;
					this.source.freezeOrientation = EditorGUILayout.Toggle("Freeze Orientation", this.source.freezeOrientation);
					this.source.regionOnStart = EditorGUILayout.ObjectField("Region On Start", this.source.regionOnStart, typeof(CriAtomRegion), true) as CriAtomRegion;
					this.source.listenerOnStart = EditorGUILayout.ObjectField("Listener On Start", this.source.listenerOnStart, typeof(CriAtomListener), true) as CriAtomListener;
					bool tempRandomizeSetting = EditorGUILayout.Toggle("3D Randomization", this.source.use3dRandomization);
					if (tempRandomizeSetting != this.source.use3dRandomization)
					{
						this.source.use3dRandomization = tempRandomizeSetting;
					}
					if (this.source.use3dRandomization)
					{
						EditorGUILayout.BeginVertical("HelpBox");
						EditorGUI.indentLevel++;
						EditorGUI.BeginChangeCheck();
						this.m_followsOriginalSource.boolValue = EditorGUILayout.Toggle("Follow origin", this.m_followsOriginalSource.boolValue);
						var calcType = (int)(CriAtomEx.Randomize3dCalcType)EditorGUILayout.EnumPopup("Type", (CriAtomEx.Randomize3dCalcType)this.m_calculationType.intValue);
						if (calcType != this.m_calculationType.intValue)
						{
							this.m_calculationType.intValue = calcType;
							/* set new parameter to 1 */
							for (int i = 0; i < CriAtomEx.Randomize3dConfig.NumOfCalcParams; ++i)
							{
								if (this.m_calculationParameters.GetArrayElementAtIndex(i).floatValue <= 0f)
								{
									this.m_calculationParameters.GetArrayElementAtIndex(i).floatValue = 1f;
								}
							}
						}
						for (int i = 0; i < CriAtomEx.Randomize3dConfig.NumOfCalcParams; ++i)
						{
							var paramType = CriAtomEx.randomize3dParamTable[(CriAtomEx.Randomize3dCalcType)m_calculationType.intValue][i];
							if (paramType == CriAtomEx.Randomize3dParamType.None)
							{
								this.m_calculationParameters.GetArrayElementAtIndex(i).floatValue = 0f;
								continue;
							}
							this.m_calculationParameters.GetArrayElementAtIndex(i).floatValue = EditorGUILayout.FloatField(paramType.ToString(), this.m_calculationParameters.GetArrayElementAtIndex(i).floatValue);
						}
						switch ((CriAtomEx.Randomize3dCalcType)m_calculationType.intValue)
						{
							case CriAtomEx.Randomize3dCalcType.List:
								var templength = (uint)Mathf.Abs(EditorGUILayout.IntField("Max length of list", (int)this.source.randomPositionListMaxLength));
								if (templength != this.source.randomPositionListMaxLength)
								{
									this.source.randomPositionListMaxLength = templength;
								}
								EditorGUILayout.HelpBox("Please set the list of positions by using CriAtomSource.SetRandomPositionList()", MessageType.Info);
								break;
							default:
								break;
						}
						if (EditorGUI.EndChangeCheck())
						{
							serializedObject.ApplyModifiedProperties();
							this.source.use3dRandomization = this.source.use3dRandomization; /* apply change */
						}
						EditorGUI.indentLevel--;
						EditorGUILayout.EndVertical();
					}
					EditorGUI.indentLevel--;
				}

				this.showAndroidConfig = EditorGUILayout.Foldout(this.showAndroidConfig, "Android Config");
				if (this.showAndroidConfig)
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.BeginHorizontal();
					style.stretchWidth = true;
					this.source.androidUseLowLatencyVoicePool = EditorGUILayout.Toggle("Low Latency Playback", this.source.androidUseLowLatencyVoicePool);
					EditorGUILayout.EndHorizontal();
					EditorGUI.indentLevel--;
				}
			}
			if (GUI.changed)
			{
				EditorUtility.SetDirty(this.source);
			}
		}
		#endregion
	} // end of class
} // namespace CriWare
