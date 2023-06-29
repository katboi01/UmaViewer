using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Aim %IK solver component.
	/// </summary>
	[HelpURL("https://www.youtube.com/watch?v=wT8fViZpLmQ&index=3&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Aim IK")]
	public class AimIK : IK {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page1.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_aim_i_k.html");
		}
		
		// Open a video tutorial about setting up the component
		[ContextMenu("TUTORIAL VIDEO")]
		void OpenSetupTutorial() {
			Application.OpenURL("https://www.youtube.com/watch?v=wT8fViZpLmQ");
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
		/// The Aim %IK solver.
		/// </summary>
		public IKSolverAim solver = new IKSolverAim();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}
	}
}

