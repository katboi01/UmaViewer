using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {

	public class GroundingInspector : Editor {

		public static void Visualize(Grounding grounding, Transform root, Transform foot) {
			Inspector.SphereCap (0, foot.position + root.forward * grounding.footCenterOffset, root.rotation, grounding.footRadius * 2f);
		}
	}
}
