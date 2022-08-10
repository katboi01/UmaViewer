using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace RootMotion.Dynamics {

	[CustomEditor(typeof(PuppetMasterHumanoidConfig))]
	public class PuppetMasterHumanoidConfigInspector : Editor {
		
		private PuppetMasterHumanoidConfig script { get { return target as PuppetMasterHumanoidConfig; } }

		public override void OnInspectorGUI() {
			foreach (PuppetMasterHumanoidConfig.HumanoidMuscle m in script.muscles) {
				m.name = m.bone.ToString();
			}

			DrawDefaultInspector ();
		}
	}
}