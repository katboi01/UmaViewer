using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.Dynamics {
	
	// Code for setting up Puppets.
	public partial class PuppetMaster: MonoBehaviour {

		/// <summary>
		/// Sets up a Puppet from the specified ragdoll and target characters.
		/// </summary>
		public static PuppetMaster SetUp(Transform target, Transform ragdoll, int characterControllerLayer, int ragdollLayer) {
			if (ragdoll != target) {
				PuppetMaster puppetMaster = ragdoll.gameObject.AddComponent<PuppetMaster>();
				puppetMaster.SetUpTo(target, characterControllerLayer, ragdollLayer);
				return puppetMaster;
			} else {
				return SetUp(ragdoll, characterControllerLayer, ragdollLayer);
			}
		}

		/// <summary>
		/// Sets up a Puppet using a single ragdoll character. This will duplicate the ragdoll character, remove the ragdoll components from the original and use it as the animated target.
		/// </summary>
		public static PuppetMaster SetUp(Transform target, int characterControllerLayer, int ragdollLayer) {
			Transform ragdoll = ((GameObject)GameObject.Instantiate(target.gameObject, target.position, target.rotation)).transform;

			PuppetMaster puppetMaster = ragdoll.gameObject.AddComponent<PuppetMaster>();
			puppetMaster.SetUpTo(target, characterControllerLayer, ragdollLayer);

			// Clean up the target from all the ragdoll components
			RemoveRagdollComponents(target, characterControllerLayer);

			return puppetMaster;
		}

		/// <summary>
		/// Sets the ragdoll up as a Puppet. Assigns the specified layers to the animated target and the ragdoll. If setUpTo is the same Transform as the PuppetMaster's, the character will be duplicated and the duplicate will be used as the animated target.
		/// </summary>
		public void SetUpTo(Transform setUpTo, int characterControllerLayer, int ragdollLayer) {
			if (setUpTo == null) {
				Debug.LogWarning("SetUpTo is null. Can not set the PuppetMaster up to a null Transform.",transform);
				return;
			}

			// Setting up the ragdoll to itself
			if (setUpTo == transform) {
				setUpTo = ((GameObject)GameObject.Instantiate(setUpTo.gameObject, setUpTo.position, setUpTo.rotation)).transform;
				setUpTo.name = name;

				// Clean up the target from all the ragdoll components
				RemoveRagdollComponents(setUpTo, characterControllerLayer);
			}
			
			RemoveUnnecessaryBones();

			/*
			Animator[] animators = GetComponentsInChildren<Animator>();
			for (int i = 0; i < animators.Length; i++) {
				DestroyImmediate(animators[i]);
			}

			Animation[] animations = GetComponentsInChildren<Animation>();
			for (int i = 0; i < animations.Length; i++) {
				DestroyImmediate(animations[i]);
			}
			*/

			Component[] components = GetComponentsInChildren<Component>();
			for (int i = 0; i < components.Length; i++) {
				if (components[i] is PuppetMaster
				    || components[i] is Transform
				    || components[i] is Rigidbody
				    || components[i] is BoxCollider
				    || components[i] is CapsuleCollider
				    || components[i] is SphereCollider
				    || components[i] is MeshCollider
				    || components[i] is Joint
				    || components[i] is Animator) {
				} else DestroyImmediate(components[i]);
			}

			// Destroy Animators last, in case some components require it (ThirdPersonCharacter)
			Animator[] animators = GetComponentsInChildren<Animator>();
			for (int i = 0; i < animators.Length; i++) {
				DestroyImmediate(animators[i]);
			}

			// Remove everything except PuppetMaster and Transform from this component
			components = transform.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++) {
				if (components[i] is PuppetMaster || components[i] is Transform) {
				} else DestroyImmediate(components[i]);
			}

			//JointConverter.ToConfigurable(gameObject);
			
			// Add ConfigurableJoints to Rigidbodies that don't have a Joint (Pelvis and other free muscles)
			Rigidbody[] rigidbodies = transform.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody r in rigidbodies) {
				if (r.transform != transform && r.GetComponent<ConfigurableJoint>() == null) {
					r.gameObject.AddComponent<ConfigurableJoint>();
				}
			}
			
			targetRoot = setUpTo;
			
			// Auto-detect targets
			SetUpMuscles(setUpTo);
			
			name = "PuppetMaster";
			
			bool newParent = setUpTo.parent == null || setUpTo.parent != transform.parent || setUpTo.parent.name != setUpTo.name + " Root";
			Transform root = newParent? new GameObject(setUpTo.name + " Root").transform: setUpTo.parent;
			root.parent = transform.parent;
			
			Transform behaviourRoot = new GameObject("Behaviours").transform;
			Comments comments = behaviourRoot.gameObject.GetComponent<Comments>();
			if (comments == null) comments = behaviourRoot.gameObject.AddComponent<Comments>();
			comments.text = "All Puppet Behaviours should be parented to this GameObject, the PuppetMaster will automatically find them from here. All Puppet Behaviours have been designed so that they could be simply copied from one character to another without changing any references. It is important because they contain a lot of parameters and would be otherwise tedious to set up and tweak.";
			
			root.position = setUpTo.position;
			root.rotation = setUpTo.rotation;
			behaviourRoot.position = setUpTo.position;
			behaviourRoot.rotation = setUpTo.rotation;
			transform.position = setUpTo.position;
			transform.rotation = setUpTo.rotation;
			
			behaviourRoot.parent = root;
			transform.parent = root;
			setUpTo.parent = root;
			
			// Layers
			targetRoot.gameObject.layer = characterControllerLayer;
			
			gameObject.layer = ragdollLayer;
			foreach (Muscle m in muscles) m.joint.gameObject.layer = ragdollLayer;
			
			Physics.IgnoreLayerCollision(characterControllerLayer, ragdollLayer);
		}

		/// <summary>
		/// Removes all the ragdoll components (except Cloth colliders), including compound colliders that are not used by Cloth. Components on the 'target' GameObject will not be touched for they are probably not ragdoll components, but required by a character controller.
		/// </summary>
		/// <param name="target">Target.</param>
		public static void RemoveRagdollComponents(Transform target, int characterControllerLayer) {
			if (target == null) return;
			
			// Get rid of all the ragdoll components on the target
			Rigidbody[] rigidbodies = target.GetComponentsInChildren<Rigidbody>();
			Cloth[] cloths = target.GetComponentsInChildren<Cloth>();
			
			for (int i = 0; i < rigidbodies.Length; i++) {
				if (rigidbodies[i].gameObject != target.gameObject) {
					var joint = rigidbodies[i].GetComponent<Joint>();
					var collider = rigidbodies[i].GetComponent<Collider>();
					
					if (joint != null) DestroyImmediate(joint);
					if (collider != null) {
						if (!IsClothCollider(collider, cloths)) DestroyImmediate(collider);
						else collider.gameObject.layer = characterControllerLayer;
					}
					DestroyImmediate(rigidbodies[i]);
				}
			}
			
			// Get rid of (compound) colliders that are not used by Cloth
			Collider[] colliders = target.GetComponentsInChildren<Collider>();
			
			for (int i = 0; i < colliders.Length; i++) {
				if (colliders[i].transform != target && !IsClothCollider(colliders[i], cloths)) {
					DestroyImmediate(colliders[i]);
				}
			}
			
			// Get rid of the PuppetMaster
			var puppetMaster = target.GetComponent<PuppetMaster>();
			if (puppetMaster != null) DestroyImmediate(puppetMaster);
		}

		// Builds muscles
		private void SetUpMuscles(Transform setUpTo) {
			// Auto-detect targets
			ConfigurableJoint[] joints = transform.GetComponentsInChildren<ConfigurableJoint>();
			
			if (joints.Length == 0) {
				Debug.LogWarning("No ConfigurableJoints found, can not build PuppetMaster. Please create ConfigurableJoints to connect the ragdoll bones together.", transform);
				return;
			}

			var animator = targetRoot.GetComponentInChildren<Animator>();
			Transform[] children = setUpTo.GetComponentsInChildren<Transform>();

			muscles = new Muscle[joints.Length];
			int hipIndex = -1;
			
			for (int i = 0; i < joints.Length; i++) {
				muscles[i] = new Muscle();
				muscles[i].joint = joints[i];
				muscles[i].name = joints[i].name;
				muscles[i].props = new Muscle.Props(1f, 1f, 1f, 1f, muscles[i].joint.connectedBody == null);
				if (muscles[i].joint.connectedBody == null && hipIndex == -1) hipIndex = i;

				foreach (Transform c in children) {
					if (c.name == joints[i].name) {
						muscles[i].target = c;
						if (animator != null) {
							muscles[i].props.group = FindGroup(animator, muscles[i].target);

							if (muscles[i].props.group == Muscle.Group.Hips || muscles[i].props.group == Muscle.Group.Leg || muscles[i].props.group == Muscle.Group.Foot) {
								muscles[i].props.mapPosition = true;
							}
						}
						break;
					}
				}
			}

			// The hip muscle is not first in hierarchy
			if (hipIndex != 0) {
				var firstMuscle = muscles[0];
				var hipMuscle = muscles[hipIndex];
				muscles[hipIndex] = firstMuscle;
				muscles[0] = hipMuscle;
			}
			
			bool allSameGroup = true;
			
			foreach (Muscle m in muscles) {
				if (m.target == null) {
					Debug.LogWarning("No target Transform found for PuppetMaster muscle " + m.joint.name + ". Please assign manually.", transform);
				}
				
				if (m.props.group != muscles[0].props.group) allSameGroup = false;
			}
			
			if (allSameGroup) {
				Debug.LogWarning("Muscle groups need to be assigned in the PuppetMaster!", transform);
			}
		}
		
		// Returns the Muscle.Group of the specified bone Transform (only if using the Humanoid rig)
		private static Muscle.Group FindGroup(Animator animator, Transform t) {
			if (!animator.isHuman) return Muscle.Group.Hips;
			if (t == animator.GetBoneTransform(HumanBodyBones.Chest)) return Muscle.Group.Spine;
			if (t == animator.GetBoneTransform(HumanBodyBones.Head)) return Muscle.Group.Head;
			if (t == animator.GetBoneTransform(HumanBodyBones.Hips)) return Muscle.Group.Hips;
			if (t == animator.GetBoneTransform(HumanBodyBones.LeftFoot)) return Muscle.Group.Foot;
			if (t == animator.GetBoneTransform(HumanBodyBones.LeftHand)) return Muscle.Group.Hand;
			if (t == animator.GetBoneTransform(HumanBodyBones.LeftLowerArm)) return Muscle.Group.Arm;
			if (t == animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg)) return Muscle.Group.Leg;
			if (t == animator.GetBoneTransform(HumanBodyBones.LeftUpperArm)) return Muscle.Group.Arm;
			if (t == animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg)) return Muscle.Group.Leg;
			if (t == animator.GetBoneTransform(HumanBodyBones.RightFoot)) return Muscle.Group.Foot;
			if (t == animator.GetBoneTransform(HumanBodyBones.RightHand)) return Muscle.Group.Hand;
			if (t == animator.GetBoneTransform(HumanBodyBones.RightLowerArm)) return Muscle.Group.Arm;
			if (t == animator.GetBoneTransform(HumanBodyBones.RightLowerLeg)) return Muscle.Group.Leg;
			if (t == animator.GetBoneTransform(HumanBodyBones.RightUpperArm)) return Muscle.Group.Arm;
			if (t == animator.GetBoneTransform(HumanBodyBones.RightUpperLeg)) return Muscle.Group.Leg;
			return Muscle.Group.Spine;
		}
		
		// Removes all bones that don't have a Rigidbody or a Collider attached because they are not part of the simulation
		private void RemoveUnnecessaryBones() {
			Transform[] children = GetComponentsInChildren<Transform>();
			for (int i = 1; i < children.Length; i++) {
				bool keep = false;

				if (children[i].GetComponent<Rigidbody>() != null || children[i].GetComponent<ConfigurableJoint>() != null) keep = true; // Ragdoll bone
				if (children[i].GetComponent<Collider>() != null && children[i].GetComponent<Rigidbody>() == null) keep = true; // Compound collider
				if (children[i].GetComponent<CharacterController>() != null) keep = false; // Character controller

				if (!keep) {
					Transform[] save = new Transform[children[i].childCount];
					for (int c = 0; c < save.Length; c++) {
						save[c] = children[i].GetChild(c);
					}
					
					for (int c = 0; c < save.Length; c++) {
						save[c].parent = children[i].parent;
					}
					
					DestroyImmediate(children[i].gameObject);
				}
			}
		}

		// Returns true if the collider is used by the Cloth.
		private static bool IsClothCollider(Collider collider, Cloth[] cloths) {
			if (cloths == null) return false;
			
			foreach (Cloth cloth in cloths) {
				if (cloth == null) return false;
				foreach (CapsuleCollider c in cloth.capsuleColliders) {
					if (c != null && c.gameObject == collider.gameObject) return true;
				}
				foreach (ClothSphereColliderPair s in cloth.sphereColliders) {
					if (s.first != null && s.first.gameObject == collider.gameObject) return true;
					if (s.second != null && s.second.gameObject == collider.gameObject) return true;
				}
			}
			
			return false;
		}

	}
}