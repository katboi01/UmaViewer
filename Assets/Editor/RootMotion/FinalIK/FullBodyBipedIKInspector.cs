using UnityEditor;
using UnityEngine;
using System.Collections;

	namespace RootMotion.FinalIK {

	/*
	 * Custom inspector for FullBodyBipedIK.
	 * */
	[CustomEditor(typeof(FullBodyBipedIK))]
	public class FullBodyBipedIKInspector : IKInspector {

		private FullBodyBipedIK script { get { return target as FullBodyBipedIK; }}
		private int selectedEffector;
		private SerializedProperty references;
		private bool autodetected;

		private static Color color {
			get {
				return new Color(0f, 0.75f, 1f);
			}
		}

		protected override MonoBehaviour GetMonoBehaviour(out int executionOrder) {
			executionOrder = 9999;
			return script;
		}

		protected override void OnEnableVirtual() {
			references = serializedObject.FindProperty("references");

			// Autodetecting References
			if (script.references.IsEmpty(false) && script.enabled) {
				BipedReferences.AutoDetectReferences(ref script.references, script.transform, new BipedReferences.AutoDetectParams(true, false));

				script.solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(script.references);

				Initiate();

				if (Application.isPlaying) Warning.Log("Biped references were auto-detected on a FullBodyBipedIK component that was added in runtime. Note that this only happens in the Editor and if the GameObject is selected (for quick and convenient debugging). If you want to add FullBodyBipedIK dynamically in runtime via script, you will have to use BipedReferences.AutodetectReferences() for automatic biped detection.", script.transform);
				
				references.isExpanded = !script.references.isFilled;
			}
		}

		protected override void AddInspector() {
			// While in editor
			if (!Application.isPlaying) {
				// Editing References, if they have changed, reinitiate.
				if (BipedReferencesInspector.AddModifiedInspector(references)) {
					Initiate();
					return; // Don't draw immediatelly to avoid errors
				}
				// Root Node
				IKSolverFullBodyBipedInspector.AddReferences(true, solver);

				// Reinitiate if rootNode has changed
				if (serializedObject.ApplyModifiedProperties()) {
					Initiate();
					return; // Don't draw immediatelly to avoid errors
				}
			} else {
				// While in play mode

				// Draw the references and the root node for UMA
				BipedReferencesInspector.AddModifiedInspector(references);	
				IKSolverFullBodyBipedInspector.AddReferences(true, solver);
			}

			string errorMessage = string.Empty;
			if (script.ReferencesError(ref errorMessage) || !script.solver.IsValid(ref errorMessage)) {
				AddWarningBox(errorMessage);
				Warning.Log(errorMessage, script.transform, false);
			} else {
				// Draw the inspector for IKSolverFullBody
				IKSolverFullBodyBipedInspector.AddInspector(solver, false);
			}

			EditorGUILayout.Space();
		}

		private void Initiate() {
			Warning.logged = false;

			// Check for possible errors, if found, do not initiate
			string message = "";
			if (script.ReferencesError(ref message)) {
				Warning.Log(message, script.transform, false);
				return;
			}

			// Notify of possible problems, but still initiate
			if (script.ReferencesWarning(ref message)) Warning.Log(message, script.transform, false);

			// Initiate
			script.solver.SetToReferences(script.references, script.solver.rootNode);
		}

		// Draw the scene view handles
		void OnSceneGUI() {
			// Draw the scene veiw helpers
			if (!script.references.isFilled) return;

			IKSolverFullBodyBipedInspector.AddScene(target, script.solver, color, ref selectedEffector, script.transform);
		}
	}
}
