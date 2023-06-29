using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

    /// <summary>
    /// Arm %IK solver component.
    /// </summary>
    [HelpURL("http://www.root-motion.com/finalikdox/html/page2.html")]
    [AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Arm IK")]
	public class ArmIK : IK {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page2.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_arm_i_k.html");
		}
		
		// Link to the Final IK Google Group
		[ContextMenu("Support Group")]
		void SupportGroup() {
			Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
		}
		
		// Link to the Final IK Asset Store thread in the Unity Community
		[ContextMenu("Asset Store Thread")]
		void ASThread() {
			Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
		}

		/// <summary>
		/// The Arm %IK solver.
		/// </summary>
		public IKSolverArm solver = new IKSolverArm();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}
	}
}
