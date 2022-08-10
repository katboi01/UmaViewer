using UnityEngine;
using UnityEditor;

namespace RootMotion.Dynamics {

	/// <summary>
	/// Converting 3D joints to ConfigurableJoints
	/// </summary>
	public class ConvertToConfigurableJoints: Editor {

		/// <summary>
		/// Converts any 3D joints on the selected Transform and it's children to ConfigurableJoints
		/// </summary>
		//[MenuItem ("Component/Physics/Convert to ConfigurableJoints")]
		[MenuItem ("GameObject/Convert to ConfigurableJoints")]
		public static void ConvertSelected() {
			if (Selection.activeGameObject == null) {
				Debug.Log("Please select the root of the ragdoll that you wish to convert to use ConfigurableJoints."); 
				return;
			}

			// @todo undo

			JointConverter.ToConfigurable(Selection.activeGameObject);
		}
	}
}