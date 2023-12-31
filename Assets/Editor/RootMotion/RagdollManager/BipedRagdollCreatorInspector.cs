using UnityEngine;
using UnityEditor;
using System.Collections;
using RootMotion;

namespace RootMotion.Dynamics {

	[CustomEditor(typeof(BipedRagdollCreator))]
	public class BipedRagdollCreatorInspector : Editor {

		public BipedRagdollCreator script { get { return target as BipedRagdollCreator; }}

		private GUIStyle miniLabelStyle = new GUIStyle();

		void OnEnable() {
			if (script == null) return;
			if (Application.isPlaying) return;

			// Autodetection
			if (script.references.IsEmpty(false)) {
				Animator animator = script.gameObject.GetComponent<Animator>();
				
				if (animator == null && script.references.root != null) {
					animator = script.references.root.GetComponentInChildren<Animator>();
					if (animator == null) animator = GetAnimatorInParents(script.references.root);
				}
				
				if (animator != null) {
					script.references = BipedRagdollReferences.FromAvatar(animator);

				} else {
					BipedReferences r = new BipedReferences();
					BipedReferences.AutoDetectReferences(ref r, script.transform, BipedReferences.AutoDetectParams.Default);
					if (r.isFilled) script.references = BipedRagdollReferences.FromBipedReferences(r);
				}

				if (!OnRoot()) {
					Debug.LogWarning("BipedRagdollCreator must be added to the root of the character. Destroying the component.");
					DestroyImmediate(script);
					return;
				}

				string msg = string.Empty;
				if (script.references.IsValid(ref msg)) {
					script.options = BipedRagdollCreator.AutodetectOptions(script.references);
					//BipedRagdollCreator.Create(script.references, script.options);

					//if (animator != null) DestroyImmediate(animator);
					//if (script.GetComponent<Animation>() != null) DestroyImmediate(script.GetComponent<Animation>());
				}
			}
		}

		private bool OnRoot() {
			if (script.references.Contains(script.transform, true)) return false;
			if (script.references.root != null) {
				var bipedRagdollCreatorOnChild = script.references.root.GetComponentInChildren<BipedRagdollCreator>();
				if (bipedRagdollCreatorOnChild != null && bipedRagdollCreatorOnChild.transform != script.references.root) return false;
			}
			return true;
		}

		public override void OnInspectorGUI() {
			miniLabelStyle.wordWrap = true;
			miniLabelStyle.fontSize = 9;
			miniLabelStyle.normal.textColor = EditorStyles.miniLabel.normal.textColor;

			serializedObject.Update();

			if (Application.isPlaying) {
				GUILayout.BeginVertical("Box");
				GUILayout.Label("Can not edit ragdolls in play mode.");
				GUILayout.EndVertical();
				return;
			}

			GUI.changed = false;
			GUILayout.Space(10);
			EditorGUILayout.BeginVertical("Box");
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 1;

			EditorGUILayout.PropertyField(serializedObject.FindProperty("references"), new GUIContent("References", "Definition of the biped ragdoll."), true);

			if (GUI.changed) {
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(script);
			}

			string msg = string.Empty;
			bool referencesValid = script.references.IsValid(ref msg);
			if (serializedObject.FindProperty("references").isExpanded) GUILayout.Space(5);
			EditorGUILayout.EndVertical();

			if (referencesValid) {
				if (!script.canBuild) {
					GUILayout.Space(5);

					if (GUILayout.Button("Create a Ragdoll")) {
						script.canBuild = true;
					}
					GUILayout.Label("Clears all existing physics components, creates a new ragdoll and starts live-updating. NB! THIS CAN NOT BE UNDONE!", miniLabelStyle);
					GUILayout.Space(5);
				} 

				if (script.canBuild) {
					GUI.changed = false;
					//GUILayout.Space(5);
					EditorGUILayout.BeginVertical("Box");
					EditorGUILayout.PropertyField(serializedObject.FindProperty("options"), new GUIContent("Options", "Options for automatic ragdoll generation."), true);

					if (GUI.changed) {
						serializedObject.ApplyModifiedProperties();
						EditorUtility.SetDirty(script);
					}
					if (serializedObject.FindProperty("options").isExpanded) GUILayout.Space(5);
					EditorGUILayout.EndVertical();

					BipedRagdollCreator.Create(script.references, script.options);

					GUILayout.Space(10);

					if (GUILayout.Button("Done")) {
						EditorGUI.indentLevel = indent;
						DestroyImmediate(script);
						return;
					}

					GUILayout.Label("Removes this component.", miniLabelStyle);

					if (GUILayout.Button("Start Editing Manually")) {
						script.gameObject.AddComponent<RagdollEditor>();
						EditorGUI.indentLevel = indent;
						DestroyImmediate(script);
						return;
					}
						
					GUILayout.Label("Replaces this component with the RagdollEditor.", miniLabelStyle);

					GUILayout.Space(10);
				}
			} else {
				GUILayout.Space(10);
				GUILayout.BeginVertical("Box");
				GUILayout.Label(msg);
				GUILayout.EndVertical();
				GUILayout.Space(10);

				if (script.canBuild) RagdollCreator.ClearAll(script.transform);
			}

			EditorGUI.indentLevel = indent;
		}

		private static Animator GetAnimatorInParents(Transform transform) {
			if (transform.GetComponent<Animator>() != null) return transform.GetComponent<Animator>();
			if (transform.parent == null) return null;
			return GetAnimatorInParents(transform.parent);
		}

	}
}
