using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RootMotion {

	// Just making sure the camera controller updates last
	[CustomEditor(typeof(CameraController))]
	public class CameraControllerInspector : Editor {

		private CameraController script { get { return target as CameraController; }}
		private MonoScript monoScript;

		void OnEnable() {
			if (!Application.isPlaying) {
				monoScript = MonoScript.FromMonoBehaviour(script);
				int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
				if (currentExecutionOrder != 10200) MonoImporter.SetExecutionOrder(monoScript, 10200);
			}
		}
	}
}
