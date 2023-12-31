using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {
	
	/// <summary>
	/// Rotates a hierarchy of bones to face a target.
	/// </summary>
	[System.Serializable]
	public class IKSolverLookAt : IKSolver {
		
		#region Main Interface

		/// <summary>
		/// The target Transform.
		/// </summary>
		public Transform target;
		/// <summary>
		/// The spine hierarchy.
		/// </summary>
		public LookAtBone[] spine = new LookAtBone[0];
		/// <summary>
		/// The head bone.
		/// </summary>
		public LookAtBone head = new LookAtBone();
		/// <summary>
		/// The eye bones.
		/// </summary>
		public LookAtBone[] eyes = new LookAtBone[0];
		/// <summary>
		/// The body weight.
		/// </summary>
		[Range(0f, 1f)]
		public float bodyWeight = 0.5f;
		/// <summary>
		/// The head weight.
		/// </summary>
		[Range(0f, 1f)]
		public float headWeight = 0.5f;
		/// <summary>
		/// The eyes weight.
		/// </summary>
		[Range(0f, 1f)]
		public float eyesWeight = 1f;
		/// <summary>
		/// Clamp weight for the body.
		/// </summary>
		[Range(0f, 1f)]
		public float clampWeight = 0.5f;
		/// <summary>
		/// Clamp weight for the head.
		/// </summary>
		[Range(0f, 1f)]
		public float clampWeightHead = 0.5f;
		/// <summary>
		/// Clamp weight for the eyes.
		/// </summary>
		[Range(0f, 1f)]
		public float clampWeightEyes = 0.5f;
		/// <summary>
		/// Number of sine smoothing iterations applied on clamping to make the clamping point smoother.
		/// </summary>
		[Range(0, 2)]
		public int clampSmoothing = 2;
		/// <summary>
		/// Weight distribution between the spine bones.
		/// </summary>
		public AnimationCurve spineWeightCurve = new AnimationCurve(new Keyframe[2] { new Keyframe(0f, 0.3f), new Keyframe(1f, 1f) });

		/// <summary>
		/// Offset for the spine target in world space..
		/// </summary>
		public Vector3 spineTargetOffset;
		
		/// <summary>
		/// Sets the look at weight. NOTE: You are welcome edit the weights directly, this method is here only to match the Unity's built in %IK API.
		/// </summary>
		public void SetLookAtWeight(float weight) {
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		}
		
		/// <summary>
		/// Sets the look at weight. NOTE: You are welcome to edit the weights directly, this method is here only to match the Unity's built in %IK API.
		/// </summary>
		public void SetLookAtWeight(float weight, float bodyWeight) {
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
			this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
		}
		
		/// <summary>
		/// Sets the look at weight. NOTE: You are welcome to edit the weights directly, this method is here only to match the Unity's built in %IK API.
		/// </summary>
		public void SetLookAtWeight(float weight, float bodyWeight, float headWeight) {
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
			this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
			this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
		}
		
		/// <summary>
		/// Sets the look at weight. NOTE: You are welcome to edit the weights directly, this method is here only to match the Unity's built in %IK API.
		/// </summary>
		public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight) {
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
			this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
			this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
			this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
		}
		
		/// <summary>
		/// Sets the look at weight. NOTE: You are welcome to edit the weights directly, this method is here only to match the Unity's built in %IK API. 
		/// </summary>
		public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight, float clampWeight) {
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
			this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
			this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
			this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
			this.clampWeight = Mathf.Clamp(clampWeight, 0f, 1f);
			this.clampWeightHead = this.clampWeight;
			this.clampWeightEyes = this.clampWeight;
		}
		
		/// <summary>
		/// Sets the look at weight. NOTE: You are welcome to edit the weights directly, this method is here only to match the Unity's built in %IK API.
		/// </summary>
		public void SetLookAtWeight(float weight, float bodyWeight = 0f, float headWeight = 1f, float eyesWeight = 0.5f, float clampWeight = 0.5f, float clampWeightHead = 0.5f, float clampWeightEyes = 0.3f) {
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
			this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
			this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
			this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
			this.clampWeight = Mathf.Clamp(clampWeight, 0f, 1f);
			this.clampWeightHead = Mathf.Clamp(clampWeightHead, 0f, 1f);
			this.clampWeightEyes = Mathf.Clamp(clampWeightEyes, 0f, 1f);
		}

		public override void StoreDefaultLocalState() {
			for (int i = 0; i < spine.Length; i++) spine[i].StoreDefaultLocalState();
			for (int i = 0; i < eyes.Length; i++) eyes[i].StoreDefaultLocalState();
			if (head != null && head.transform != null) head.StoreDefaultLocalState();
		}
		
		public override void FixTransforms() {
			if (!initiated) return;
			if (IKPositionWeight <= 0f) return;

			for (int i = 0; i < spine.Length; i++) spine[i].FixTransform();
			for (int i = 0; i < eyes.Length; i++) eyes[i].FixTransform();
			if (head != null && head.transform != null) head.FixTransform();
		}
		
		public override bool IsValid (ref string message) {
			if (!spineIsValid) {
				message = "IKSolverLookAt spine setup is invalid. Can't initiate solver.";
				return false;
			}
			if (!headIsValid) {
				message = "IKSolverLookAt head transform is null. Can't initiate solver.";
				return false;
			}
			if (!eyesIsValid) {
				message = "IKSolverLookAt eyes setup is invalid. Can't initiate solver.";
				return false;
			}

			if (spineIsEmpty && headIsEmpty && eyesIsEmpty) {
				message = "IKSolverLookAt eyes setup is invalid. Can't initiate solver.";
				return false;
			}

			Transform spineDuplicate = ContainsDuplicateBone(spine);
			if (spineDuplicate != null) {
				message = spineDuplicate.name + " is represented multiple times in a single IK chain. Can't initiate solver.";
				return false;
			}
			Transform eyeDuplicate = ContainsDuplicateBone(eyes);
			if (eyeDuplicate != null) {
				message = eyeDuplicate.name + " is represented multiple times in a single IK chain. Can't initiate solver.";
				return false;
			}
			return true;
		}
		
		public override IKSolver.Point[] GetPoints() {
			IKSolver.Point[] allPoints = new IKSolver.Point[spine.Length + eyes.Length + (head.transform != null? 1: 0)];
			for (int i = 0; i < spine.Length; i++) allPoints[i] = spine[i] as IKSolver.Point;

            int eye = 0;
            for (int i = spine.Length; i < spine.Length + eyes.Length; i++)
            {
                allPoints[i] = eyes[eye] as IKSolver.Point;
                eye++;
            }
			
			if (head.transform != null) allPoints[allPoints.Length - 1] = head as IKSolver.Point;
			return allPoints;
		}
		
		public override IKSolver.Point GetPoint(Transform transform) {
			foreach (IKSolverLookAt.LookAtBone b in spine) if (b.transform == transform) return b as IKSolver.Point;
			foreach (IKSolverLookAt.LookAtBone b in eyes) if (b.transform == transform) return b as IKSolver.Point;
			if (head.transform == transform) return head as IKSolver.Point;
			return null;
		}
		
		/// <summary>
		/// Look At bone class.
		/// </summary>
		[System.Serializable]
		public class LookAtBone: IKSolver.Bone {

            #region Public methods

            public Vector3 baseForwardOffsetEuler;

			public LookAtBone() {}

			/*
			 * Custom constructor
			 * */
			public LookAtBone(Transform transform) {
				this.transform = transform;
			}
			
			/*
			 * Initiates the bone, precalculates values.
			 * */
			public void Initiate(Transform root) {
				if (transform == null) return;
				
				axis = Quaternion.Inverse(transform.rotation) * root.forward;
			}
			
			/*
			 * Rotates the bone to look at a world direction.
			 * */
			public void LookAt(Vector3 direction, float weight) {
				Quaternion fromTo = Quaternion.FromToRotation(forward, direction);
				Quaternion r = transform.rotation;
				transform.rotation = Quaternion.Lerp(r, fromTo * r, weight);
			}
			
			/*
			 * Gets the local axis to goal in world space.
			 * */
			public Vector3 forward {
				get {
					return transform.rotation * axis;
				}
			}

			#endregion Public methods
		}
		
		/// <summary>
		/// Reinitiate the solver with new bone Transforms.
		/// </summary>
		/// <returns>
		/// Returns true if the new chain is valid.
		/// </returns>
		public bool SetChain(Transform[] spine, Transform head, Transform[] eyes, Transform root) {
			// Spine
			SetBones(spine, ref this.spine);

			// Head
			this.head = new LookAtBone(head);

			// Eyes
			SetBones(eyes, ref this.eyes);
			
			Initiate(root);
			return initiated;
		}

		#endregion Main Interface

		protected Vector3[] spineForwards = new Vector3[0];
        protected Vector3[] headForwards = new Vector3[1];
        protected Vector3[] eyeForward = new Vector3[1];
		
		protected override void OnInitiate() {
			// Set IKPosition to default value
			if (firstInitiation || !Application.isPlaying) {
				if (spine.Length > 0) IKPosition = spine[spine.Length - 1].transform.position + root.forward * 3f;
				else if (head.transform != null) IKPosition = head.transform.position + root.forward * 3f;
				else if (eyes.Length > 0 && eyes[0].transform != null) IKPosition = eyes[0].transform.position + root.forward * 3f;
			}
			
			// Initiating the bones
			foreach (LookAtBone s in spine) s.Initiate(root);
			if (head != null) head.Initiate(root);
			foreach (LookAtBone eye in eyes) eye.Initiate(root);
			
			if (spineForwards == null || spineForwards.Length != spine.Length) spineForwards = new Vector3[spine.Length];
			if (headForwards == null) headForwards = new Vector3[1];
			if (eyeForward == null) eyeForward = new Vector3[1];
		}
		
		protected override void OnUpdate() {
			if (IKPositionWeight <= 0) return;
			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);

			if (target != null) IKPosition = target.position;

			// Solving the hierarchies
			SolveSpine();
			SolveHead();
			SolveEyes();
		}

        protected bool spineIsValid {
			get {
				if (spine == null) return false;
				if (spine.Length == 0) return true;

				for (int i = 0; i < spine.Length; i++) if (spine[i] == null || spine[i].transform == null) return false;
				return true;
			}
		}

        protected bool spineIsEmpty { get { return spine.Length == 0; }}

        // Solving the spine hierarchy
        protected void SolveSpine() {
			if (bodyWeight <= 0) return;
			if (spineIsEmpty) return;
			
			// Get the look at vectors for each bone
			//Vector3 targetForward = Vector3.Lerp(spine[0].forward, (IKPosition - spine[spine.Length - 1].transform.position).normalized, bodyWeight * IKPositionWeight).normalized;
			Vector3 targetForward = (IKPosition + spineTargetOffset - spine[spine.Length - 1].transform.position).normalized;

			GetForwards(ref spineForwards, spine[0].forward, targetForward, spine.Length, clampWeight);
			
			// Rotate each bone to face their look at vectors
			for (int i = 0; i < spine.Length; i++) {
				spine[i].LookAt(spineForwards[i], bodyWeight * IKPositionWeight);
			}
		}

        protected bool headIsValid {
			get {
				if (head == null) return false;
				return true;
			}
		}

        protected bool headIsEmpty { get { return head.transform == null; }}

        // Solving the head rotation
        protected void SolveHead() {
			if (headWeight <= 0) return;
			if (headIsEmpty) return;
			
			// Get the look at vector for the head
			Vector3 baseForward = spine.Length > 0 && spine[spine.Length - 1].transform != null? spine[spine.Length - 1].forward: head.forward;

			Vector3 targetForward = Vector3.Lerp(baseForward, (IKPosition - head.transform.position).normalized, headWeight * IKPositionWeight).normalized;
			GetForwards(ref headForwards, baseForward, targetForward, 1, clampWeightHead);
			
			// Rotate the head to face its look at vector
			head.LookAt(headForwards[0], headWeight * IKPositionWeight);
		}

        protected bool eyesIsValid {
			get {
				if (eyes == null) return false;
				if (eyes.Length == 0) return true;

				for (int i = 0; i < eyes.Length; i++) if (eyes[i] == null || eyes[i].transform == null) return false;
				return true;
			}
		}

        protected bool eyesIsEmpty { get { return eyes.Length == 0; }}

        // Solving the eye rotations
        protected void SolveEyes() {
			if (eyesWeight <= 0) return;
			if (eyesIsEmpty) return;
			
			for (int i = 0; i < eyes.Length; i++) {
                // Get the look at vector for the eye
                Quaternion baseRotation = head.transform != null ? head.transform.rotation : spine.Length > 0? spine[spine.Length - 1].transform.rotation: root.rotation;
                Vector3 baseAxis = head.transform != null ? head.axis : spine.Length > 0 ? spine[spine.Length - 1].axis : root.forward;

                if (eyes[i].baseForwardOffsetEuler != Vector3.zero) baseRotation *= Quaternion.Euler(eyes[i].baseForwardOffsetEuler);

                Vector3 baseForward = baseRotation * baseAxis;

                GetForwards(ref eyeForward, baseForward, (IKPosition - eyes[i].transform.position).normalized, 1, clampWeightEyes);
				
				// Rotate the eye to face its look at vector
				eyes[i].LookAt(eyeForward[0], eyesWeight * IKPositionWeight);
			}
		}

        /*
		 * Returns forwards for a number of bones rotating from baseForward to targetForward.
		 * NB! Make sure baseForward and targetForward are normalized.
		 * */
        protected Vector3[] GetForwards(ref Vector3[] forwards, Vector3 baseForward, Vector3 targetForward, int bones, float clamp) {
			// If clamp >= 1 make all the forwards match the base
			if (clamp >= 1 || IKPositionWeight <= 0) {
				for (int i = 0; i < forwards.Length; i++) forwards[i] = baseForward;
				return forwards;
			}
			
			// Get normalized dot product. 
			float angle = Vector3.Angle(baseForward, targetForward);
			float dot = 1f - (angle / 180f);
			
			// Clamping the targetForward so it doesn't exceed clamp
			float targetClampMlp = clamp > 0? Mathf.Clamp(1f - ((clamp - dot) / (1f - dot)), 0f, 1f): 1f;
			
			// Calculating the clamp multiplier
			float clampMlp = clamp > 0? Mathf.Clamp(dot / clamp, 0f, 1f): 1f;
			
			for (int i = 0; i < clampSmoothing; i++) {
				float sinF = clampMlp * Mathf.PI * 0.5f;
				clampMlp = Mathf.Sin(sinF);
			}
			
			// Rotation amount for 1 bone
			if (forwards.Length == 1) {
				forwards[0] = Vector3.Slerp(baseForward, targetForward, clampMlp * targetClampMlp);
			} else {
				float step = 1f / (float)(forwards.Length - 1);
				
				// Calculate the forward for each bone
				for (int i = 0; i < forwards.Length; i++) {
					forwards[i] = Vector3.Slerp(baseForward, targetForward, spineWeightCurve.Evaluate(step * i) * clampMlp * targetClampMlp);
				}
			}
			
			return forwards;
		}

        /* 
		 * Build LookAtBone[] array of a Transform array
		 * */
        protected void SetBones(Transform[] array, ref LookAtBone[] bones) {
			if (array == null) {
				bones = new LookAtBone[0];
				return;
			}
			
			if (bones.Length != array.Length) bones = new LookAtBone[array.Length];
			
			for (int i = 0; i < array.Length; i++) {
				if (bones[i] == null) bones[i] = new LookAtBone(array[i]);
				else bones[i].transform = array[i];
			}
		}
	}
}
