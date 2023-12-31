using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Generic FBIK solver. In each solver update, %IKSolverFullBody first reads the character's pose, then solves the %IK and writes the solved pose back to the character via IKMapping.
	/// </summary>
	[System.Serializable]
	public class IKSolverFullBody : IKSolver {
		
		#region Main Interface
		
		/// <summary>
		/// Number of solver iterations.
		/// </summary>
		[Range(0, 10)]
		public int iterations = 4;
		/// <summary>
		/// The root node chain.
		/// </summary>
		public FBIKChain[] chain = new FBIKChain[0];
		/// <summary>
		/// The effectors.
		/// </summary>
		public IKEffector[] effectors = new IKEffector[0];
		/// <summary>
		/// Mapping spine bones to the solver.
		/// </summary>
		public IKMappingSpine spineMapping = new IKMappingSpine();
		/// <summary>
		/// Mapping individual bones to the solver
		/// </summary>
		public IKMappingBone[] boneMappings = new IKMappingBone[0];
		/// <summary>
		/// Mapping 3 segment limbs to the solver
		/// </summary>
		public IKMappingLimb[] limbMappings = new IKMappingLimb[0];
		/// <summary>
		/// If false, will not solve a FABRIK pass and the arms/legs will not be able to pull the body.
		/// </summary>
		public bool FABRIKPass = true;
		
		/// <summary>
		/// Gets the effector of the specified Transform.
		/// </summary>
		public IKEffector GetEffector(Transform t) {
			for (int i = 0; i < effectors.Length; i++) if (effectors[i].bone == t) return effectors[i];
			return null;
		}
		
		/// <summary>
		/// Gets the chain that contains the specified Transform.
		/// </summary>
		public FBIKChain GetChain(Transform transform) {
			int index = GetChainIndex(transform);
			if (index == -1) return null;
			return chain[index];
		}

		/// <summary>
		/// Gets the index of the chain (in the IKSolverFullBody.chain array) that contains the specified Transform.
		/// </summary>
		public int GetChainIndex(Transform transform) {
			for (int i = 0; i < chain.Length; i++) {
				for (int n = 0; n < chain[i].nodes.Length; n++) if (chain[i].nodes[n].transform == transform) return i;
			}
			return -1; 
		}

		public IKSolver.Node GetNode(int chainIndex, int nodeIndex) {
			return chain[chainIndex].nodes[nodeIndex];
		}

		public void GetChainAndNodeIndexes(Transform transform, out int chainIndex, out int nodeIndex) {
			chainIndex = GetChainIndex(transform);
			if (chainIndex == -1) nodeIndex = -1;
			else nodeIndex = chain[chainIndex].GetNodeIndex(transform);
		}

		public override IKSolver.Point[] GetPoints() {
			int nodes = 0;
			for (int i = 0; i < chain.Length; i++) nodes += chain[i].nodes.Length;

			IKSolver.Point[] pointArray = new IKSolver.Point[nodes];

			int added = 0;
			for (int i = 0; i < chain.Length; i++) {
				for (int n = 0; n < chain[i].nodes.Length; n++) {
					pointArray[added] = chain[i].nodes[n] as IKSolver.Node;
                    added++;
				}
			}

			return pointArray;
		}
		
		public override IKSolver.Point GetPoint(Transform transform) {
			for (int i = 0; i < chain.Length; i++) {
				for (int n = 0; n < chain[i].nodes.Length; n++) if (chain[i].nodes[n].transform == transform) return chain[i].nodes[n] as IKSolver.Point;
			}
			return null;
		}
		
		public override bool IsValid(ref string message) {
			if (chain == null) {
				message = "FBIK chain is null, can't initiate solver.";
				return false;
			}

			if (chain.Length == 0) {
				message = "FBIK chain length is 0, can't initiate solver.";
				return false;
			}

			for (int i = 0; i < chain.Length; i++) {
				if (!chain[i].IsValid(ref message)) return false;
			}

			foreach (IKEffector e in effectors) if (!e.IsValid(this, ref message)) return false;

			if (!spineMapping.IsValid(this, ref message)) return false;
			foreach (IKMappingLimb l in limbMappings) if (!l.IsValid(this, ref message)) return false;
			foreach (IKMappingBone b in boneMappings) if (!b.IsValid(this, ref message)) return false;

			return true;
		}

		/// <summary>
		/// Called before reading the pose
		/// </summary>
		public UpdateDelegate OnPreRead;
		/// <summary>
		/// Called before solving.
		/// </summary>
		public UpdateDelegate OnPreSolve;
		/// <summary>
		/// Called before each iteration
		/// </summary>
		public IterationDelegate OnPreIteration;
		/// <summary>
		/// Called after each iteration
		/// </summary>
		public IterationDelegate OnPostIteration;
		/// <summary>
		/// Called before applying bend constraints.
		/// </summary>
		public UpdateDelegate OnPreBend;
		/// <summary>
		/// Called after updating the solver
		/// </summary>
		public UpdateDelegate OnPostSolve;
		/// <summary>
		/// Called when storing default local state (the state that FixTransforms will reset the hierarchy to).
		/// </summary>
		public UpdateDelegate OnStoreDefaultLocalState;
		/// <summary>
		/// Called when the bones used by the solver will reset to the default local state.
		/// </summary>
		public UpdateDelegate OnFixTransforms;
		
		#endregion Main Interface

		public override void StoreDefaultLocalState() {
			spineMapping.StoreDefaultLocalState();
			for (int i = 0; i < limbMappings.Length; i++) limbMappings[i].StoreDefaultLocalState();
			for (int i = 0; i < boneMappings.Length; i++) boneMappings[i].StoreDefaultLocalState();

			if (OnStoreDefaultLocalState != null) OnStoreDefaultLocalState();
		}
		
		public override void FixTransforms() {
			if (!initiated) return;
			if (IKPositionWeight <= 0f) return;

			spineMapping.FixTransforms();
			for (int i = 0; i < limbMappings.Length; i++) limbMappings[i].FixTransforms();
			for (int i = 0; i < boneMappings.Length; i++) boneMappings[i].FixTransforms();

			if (OnFixTransforms != null) OnFixTransforms();
		}

		protected override void OnInitiate() {
			// Initiate chain
			for (int i = 0; i < chain.Length; i++) {
				chain[i].Initiate(this);
			}

			// Initiate effectors
			foreach (IKEffector e in effectors) e.Initiate(this);
			
			// Initiate IK mapping
			spineMapping.Initiate(this);
			foreach (IKMappingBone boneMapping in boneMappings) boneMapping.Initiate(this);
			foreach (IKMappingLimb limbMapping in limbMappings) limbMapping.Initiate(this);
		}

		protected override void OnUpdate() {
			if (IKPositionWeight <= 0) {
				// clear effector positionOffsets so they would not accumulate
				for (int i = 0; i < effectors.Length; i++) effectors[i].positionOffset = Vector3.zero;

				return;
			}

			if (chain.Length == 0) return;

			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);

			if (OnPreRead != null) OnPreRead();

			// Phase 1: Read the pose of the biped
			ReadPose();

			if (OnPreSolve != null) OnPreSolve();

			// Phase 2: Solve IK
			Solve();

			if (OnPostSolve != null) OnPostSolve();

			// Phase 3: Map biped to it's solved state
			WritePose();

			// Reset effector position offsets to Vector3.zero
			for (int i = 0; i < effectors.Length; i++) effectors[i].OnPostWrite();
		}
		
		protected virtual void ReadPose() {
			// Making sure the limbs are not inverted
			for (int i = 0; i < chain.Length; i++) {
				if (chain[i].bendConstraint.initiated) chain[i].bendConstraint.LimitBend(IKPositionWeight, GetEffector(chain[i].nodes[2].transform).positionWeight);
			}

			// Presolve effectors, apply effector offset to the nodes
			for (int i = 0; i < effectors.Length; i++) effectors[i].ResetOffset(this);
			for (int i = 0; i < effectors.Length; i++) effectors[i].OnPreSolve(this);

			// Set solver positions to match the current bone positions of the biped
			for (int i = 0; i < chain.Length; i++) {
				chain[i].ReadPose(this, iterations > 0);
			}

			// IKMapping 
			if (iterations > 0) {
				spineMapping.ReadPose();
				for (int i = 0; i < boneMappings.Length; i++) boneMappings[i].ReadPose();
			}

			for (int i = 0; i < limbMappings.Length; i++) limbMappings[i].ReadPose();
		}

		protected virtual void Solve() {
			// Iterate solver
			if(iterations > 0) {
				for (int i = 0; i < (FABRIKPass? iterations: 1); i++) {
					if (OnPreIteration != null) OnPreIteration(i);
					
					// Apply end-effectors
					for (int e = 0; e < effectors.Length; e++) if (effectors[e].isEndEffector) effectors[e].Update(this);
				
					if (FABRIKPass) {
						// Reaching
						chain[0].Push(this);

						// Reaching
						if (FABRIKPass) chain[0].Reach(this);
					
						// Apply non end-effectors
						for (int e = 0; e < effectors.Length; e++) if (!effectors[e].isEndEffector) effectors[e].Update(this);
					}

					// Trigonometric pass to release push tension from the solver
					chain[0].SolveTrigonometric(this);

					if (FABRIKPass) {
						// Solving FABRIK forward
						chain[0].Stage1(this);

						// Apply non end-effectors again
						for (int e = 0; e < effectors.Length; e++) if (!effectors[e].isEndEffector) effectors[e].Update(this);

						// Solving FABRIK backwards
						chain[0].Stage2(this, chain[0].nodes[0].solverPosition);
					}

					if (OnPostIteration != null) OnPostIteration(i);
				}
			}

			// Before applying bend constraints (last chance to modify the bend direction)
			if (OnPreBend != null) OnPreBend();

			// Final end-effector pass
			for (int i = 0; i < effectors.Length; i++) if (effectors[i].isEndEffector) effectors[i].Update(this);

			ApplyBendConstraints();
		}

		protected virtual void ApplyBendConstraints() {
			// Solve bend constraints
			chain[0].SolveTrigonometric(this, true);
		}

		protected virtual void WritePose() {
			if (IKPositionWeight <= 0f) return;

			// Apply IK mapping
			if (iterations > 0) {
				spineMapping.WritePose(this);
				for (int i = 0; i < boneMappings.Length; i++) boneMappings[i].WritePose(IKPositionWeight);
			}

			for (int i = 0; i < limbMappings.Length; i++) limbMappings[i].WritePose(this, iterations > 0);
		}
	}
}
