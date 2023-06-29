using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RootMotion.FinalIK {

	// Custom Scene View handles for the FingerRig.
	[CustomEditor(typeof(FingerRig))]
	public class FingerRigInspector : Editor {

		private FingerRig script { get { return target as FingerRig; }}
		
		private int selected = -1;
		private MonoScript monoScript;

		void OnEnable() {
			if (serializedObject == null) return;
			
			// Changing the script execution order
			if (!Application.isPlaying) {
				monoScript = MonoScript.FromMonoBehaviour(script);
				int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
				if (currentExecutionOrder != 10000) MonoImporter.SetExecutionOrder(monoScript, 10000);
			}
		}

		void OnSceneGUI() {
			if (!script.enabled) return;
			string message = string.Empty;
			if (!script.IsValid(ref message)) return;
			if (Application.isPlaying && !script.initiated) return;

			Color color = Color.cyan;
			color.a = script.weight;

			Handles.color = color;
			GUI.color = color;

			// Display the bones
			if (!Application.isPlaying) {
				for (int i = 0; i < script.fingers.Length; i++) {
					Handles.DrawLine(script.fingers[i].bone1.position, script.fingers[i].bone2.position);
					Inspector.SphereCap(0, script.fingers[i].bone1.position, Quaternion.identity, IKSolverInspector.GetHandleSize(script.fingers[i].bone1.position) * 0.5f);
					Inspector.SphereCap(0, script.fingers[i].bone2.position, Quaternion.identity, IKSolverInspector.GetHandleSize(script.fingers[i].bone2.position) * 0.5f);

					if (script.fingers[i].bone3 != null) {
						Handles.DrawLine(script.fingers[i].bone2.position, script.fingers[i].bone3.position);
						Handles.DrawLine(script.fingers[i].bone3.position, script.fingers[i].tip.position);
						Inspector.SphereCap(0, script.fingers[i].bone3.position, Quaternion.identity, IKSolverInspector.GetHandleSize(script.fingers[i].bone3.position) * 0.5f);
					} else {
						Handles.DrawLine(script.fingers[i].bone2.position, script.fingers[i].tip.position);
					}

					Inspector.SphereCap(0, script.fingers[i].tip.position, Quaternion.identity, IKSolverInspector.GetHandleSize(script.fingers[i].tip.position) * 0.5f);
				}
			}

			// Selecting solvers
			if (Application.isPlaying) {
				if (selected >= 0 && selected < script.fingers.Length) {
					if (script.fingers[selected].weight > 0f) {
						color.a = script.weight * script.fingers[selected].weight;
						Handles.color = color;

						float size = IKSolverInspector.GetHandleSize(script.fingers[selected].IKPosition);

						Inspector.CubeCap(0, script.fingers[selected].IKPosition, script.fingers[selected].IKRotation, size);

						if (script.fingers[selected].target == null) {
							switch(Tools.current) {
							case Tool.Move:
								script.fingers[selected].IKPosition = Handles.PositionHandle(script.fingers[selected].IKPosition, Tools.pivotRotation == PivotRotation.Local? script.fingers[selected].IKRotation: Quaternion.identity);
								break;
							case Tool.Rotate:
								script.fingers[selected].IKRotation = Handles.RotationHandle(script.fingers[selected].IKRotation, script.fingers[selected].IKPosition);
								break;
							}
						}
					}
				}

				for (int i = 0; i < script.fingers.Length; i++) {
					color.a = script.weight * script.fingers[i].weight;
					Handles.color = color;
					Handles.DrawLine(script.fingers[i].tip.position, script.fingers[i].IKPosition);

					if (script.fingers[i].weight > 0 && selected != i && script.fingers[i].initiated) {
						float size = IKSolverInspector.GetHandleSize(script.fingers[i].IKPosition) * 0.5f;

						if (Inspector.DotButton(script.fingers[i].IKPosition, Quaternion.identity, size, size)) selected = i;
					}
				}
			}

			Handles.color = Color.white;
			GUI.color = Color.white;
		}
	}
}
