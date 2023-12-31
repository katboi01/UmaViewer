using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// %IK system for standard biped characters that is designed to replicate and enhance the behaviour of the Unity's built-in character %IK setup.
	/// </summary>
	[HelpURL("http://www.root-motion.com/finalikdox/html/page4.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Biped IK")]
	public class BipedIK : SolverManager {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		private void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page4.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_biped_i_k.html");
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

		#region Main Interface

		/// <summary>
		/// References to character bones.
		/// </summary>
		public BipedReferences references = new BipedReferences();
		/// <summary>
		/// The %IK solvers.
		/// </summary>
		public BipedIKSolvers solvers = new BipedIKSolvers();
		
		/// <summary>
		/// Gets the %IK position weight.
		/// </summary>
		/// <param name='goal'>
		/// %IK Goal.
		/// </param>
		public float GetIKPositionWeight(AvatarIKGoal goal) {
			return GetGoalIK(goal).GetIKPositionWeight();
		}
		
		/// <summary>
		/// Gets the %IK rotation weight.
		/// </summary>
		/// <param name='goal'>
		/// IK Goal.
		/// </param>
		public float GetIKRotationWeight(AvatarIKGoal goal) {
			return GetGoalIK(goal).GetIKRotationWeight();
		}
		
		/// <summary>
		/// Sets the %IK position weight.
		/// </summary>
		/// <param name='goal'>
		/// %IK Goal.
		/// </param>
		/// <param name='weight'>
		/// Weight.
		/// </param>
		public void SetIKPositionWeight(AvatarIKGoal goal, float weight) {
			GetGoalIK(goal).SetIKPositionWeight(weight);
		}
		
		/// <summary>
		/// Sets the %IK rotation weight.
		/// </summary>
		/// <param name='goal'>
		/// %IK Goal.
		/// </param>
		/// <param name='weight'>
		/// Weight.
		/// </param>
		public void SetIKRotationWeight(AvatarIKGoal goal, float weight) {
			GetGoalIK(goal).SetIKRotationWeight(weight);
		}
		
		/// <summary>
		/// Sets the %IK position.
		/// </summary>
		/// <param name='goal'>
		/// %IK Goal.
		/// </param>
		/// <param name='IKPosition'>
		/// Position.
		/// </param>
		public void SetIKPosition(AvatarIKGoal goal, Vector3 IKPosition) {
			GetGoalIK(goal).SetIKPosition(IKPosition);
		}
		
		/// <summary>
		/// Sets the %IK rotation.
		/// </summary>
		/// <param name='goal'>
		/// %IK Goal.
		/// </param>
		/// <param name='IKRotation'>
		/// Rotation.
		/// </param>
		public void SetIKRotation(AvatarIKGoal goal, Quaternion IKRotation) {
			GetGoalIK(goal).SetIKRotation(IKRotation);
		}
		
		/// <summary>
		/// Gets the %IK position.
		/// </summary>
		/// <param name='goal'>
		/// %IK Goal.
		/// </param>
		public Vector3 GetIKPosition(AvatarIKGoal goal) {
			return GetGoalIK(goal).GetIKPosition();
		}
		
		/// <summary>
		/// Gets the %IK rotation.
		/// </summary>
		/// <param name='goal'>
		/// %IK Goal.
		/// </param>
		public Quaternion GetIKRotation(AvatarIKGoal goal) {
			return GetGoalIK(goal).GetIKRotation();
		}
		
		/// <summary>
		/// Sets the look at weight.
		/// </summary>
		/// <param name='weight'>
		/// Master Weight.
		/// </param>
		/// <param name='bodyWeight'>
		/// Body weight.
		/// </param>
		/// <param name='headWeight'>
		/// Head weight.
		/// </param>
		/// <param name='eyesWeight'>
		/// Eyes weight.
		/// </param>
		/// <param name='clampWeight'>
		/// Clamp weight for body and head.
		/// </param>
		/// <param name='clampWeightEyes'>
		/// Clamp weight for eyes.
		/// </param>
		public void SetLookAtWeight(float weight, float bodyWeight , float headWeight, float eyesWeight, float clampWeight, float clampWeightHead, float clampWeightEyes) {
			solvers.lookAt.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight, clampWeight, clampWeightHead, clampWeightEyes);
		}

		/// <summary>
		/// Sets the look at target.
		/// </summary>
		/// <param name='lookAtPosition'>
		/// Look at position.
		/// </param>
		public void SetLookAtPosition(Vector3 lookAtPosition) {
			solvers.lookAt.SetIKPosition(lookAtPosition);
		}
		
		/// <summary>
		/// Sets the spine %IK position.
		/// </summary>
		/// <param name='spinePosition'>
		/// Spine %IK position.
		/// </param>
		public void SetSpinePosition(Vector3 spinePosition) {
			solvers.spine.SetIKPosition(spinePosition);
		}
		
		/// <summary>
		/// Sets the spine weight.
		/// </summary>
		/// <param name='weight'>
		/// Weight.
		/// </param>
		public void SetSpineWeight(float weight) {
			solvers.spine.SetIKPositionWeight(weight);
		}
		
		/// <summary>
		/// Gets the limb solver for the %IK Goal.
		/// </summary>
		/// <returns>
		/// The solver.
		/// </returns>
		/// <param name='goal'>
		/// %IK Goal.
		/// </param>
		public IKSolverLimb GetGoalIK(AvatarIKGoal goal) {
			switch(goal) {
			case AvatarIKGoal.LeftFoot: return solvers.leftFoot;
			case AvatarIKGoal.RightFoot: return solvers.rightFoot;
			case AvatarIKGoal.LeftHand: return solvers.leftHand;
			case AvatarIKGoal.RightHand: return solvers.rightHand;
			}
			return null;
		}

		/// <summary>
		/// (Re)Initiates the biped IK solvers.
		/// </summary>
		public void InitiateBipedIK() {
			InitiateSolver();
		}

		/// <summary>
		/// Updating BipedIK
		/// </summary>
		public void UpdateBipedIK() {
			UpdateSolver();
		}

		/* 
		 * Set default solver values.
		 * */
		public void SetToDefaults() {
			// Limbs
			foreach (IKSolverLimb limb in solvers.limbs) {
				limb.SetIKPositionWeight(0f);
				limb.SetIKRotationWeight(0f);
				limb.bendModifier = IKSolverLimb.BendModifier.Animation;
				limb.bendModifierWeight = 1f;
			}
			
			solvers.leftHand.maintainRotationWeight = 0f;
			solvers.rightHand.maintainRotationWeight = 0f;
			
			// Spine
			solvers.spine.SetIKPositionWeight(0f);
			solvers.spine.tolerance = 0f;
			solvers.spine.maxIterations = 2;
			solvers.spine.useRotationLimits = false;
			
			// Aim
			solvers.aim.SetIKPositionWeight(0f);
			solvers.aim.tolerance = 0f;
			solvers.aim.maxIterations = 2;
			
			// LookAt
			SetLookAtWeight(0f, 0.5f, 1f, 1f, 0.5f, 0.7f, 0.5f);
		}

		#endregion Main Interface

		/*
		 * Fixes all the Transforms used by the solver to their default local states.
		 * */
		protected override void FixTransforms() {
			solvers.lookAt.FixTransforms();
			for (int i = 0; i < solvers.limbs.Length; i++) solvers.limbs[i].FixTransforms();
		}

		/*
		 * Initiates the %IK solver
		 * */
		protected override void InitiateSolver() {
			string message = "";
			if (BipedReferences.SetupError(references, ref message)) {
				Warning.Log(message, references.root, false);
				return;
			}
			solvers.AssignReferences(references);
			
			// Initiating solvers
			if (solvers.spine.bones.Length > 1) solvers.spine.Initiate(transform);
			solvers.lookAt.Initiate(transform);
			solvers.aim.Initiate(transform);
			foreach (IKSolverLimb limb in solvers.limbs) limb.Initiate(transform);
			
			// Initiating constraints
			solvers.pelvis.Initiate(references.pelvis);
		}

		/*
		 * Updates the solvers. If you need full control of the execution order of your IK solvers, disable this script and call UpdateSolver() instead.
		 * */
		protected override void UpdateSolver() {
			// Storing Limb bend and rotation before %IK
			for (int i = 0; i < solvers.limbs.Length; i++) {
				solvers.limbs[i].MaintainBend();
				solvers.limbs[i].MaintainRotation();
			}
			
			// Updating constraints
			solvers.pelvis.Update();
			
			// Updating %IK solvers
			if (solvers.spine.bones.Length > 1) solvers.spine.Update();
			solvers.aim.Update();
			solvers.lookAt.Update();
			for (int i = 0; i < solvers.limbs.Length; i++) solvers.limbs[i].Update();
		}

		/// <summary>
		/// Logs the warning if no other warning has beed logged in this session.
		/// </summary>
		public void LogWarning(string message) {
			Warning.Log(message, transform);
		}
	}
}
