using UnityEngine;
using UnityEditor;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	[CustomEditor(typeof(CharacterPuppet))]
	public class CharacterPuppetInspector : Editor {

		private CharacterPuppet script { get { return target as CharacterPuppet; }}

		private GameObject replace;

		private static Color pro = new Color(0.7f, 0.9f, 0.5f, 1f);
		private static Color free = new Color(0.4f, 0.5f, 0.3f, 1f);

		public override void OnInspectorGUI() {
			GUI.changed = false;

			if (!Application.isPlaying) {
				GUI.color = EditorGUIUtility.isProSkin? pro: free;
				EditorGUILayout.BeginHorizontal();

				replace = (GameObject)EditorGUILayout.ObjectField("Replace Character Model", replace, typeof(GameObject), true);

				if (replace != null) {
					if (GUILayout.Button("Replace")) {
						ReplacePuppetModel(script as CharacterThirdPerson, replace);
						replace = null;
					}
				}
				GUI.color = Color.white;
				EditorGUILayout.EndHorizontal();
			}

			DrawDefaultInspector();
			if (GUI.changed) EditorUtility.SetDirty(script);
		}

		public static void ReplacePuppetModel(CharacterThirdPerson script, GameObject replace) {
			// Drag your Humanoid character to the scene, parent it to the "Animation Controller" just where the Pilot is
			GameObject find = GameObject.Find(replace.name);
			bool isSceneObject = find != null && find == replace;

			GameObject instance = isSceneObject? replace: (GameObject)GameObject.Instantiate(replace, script.transform.position, script.transform.rotation);
			instance.transform.parent = script.characterAnimation.transform;
			instance.name = replace.name;
			instance.transform.position = script.transform.position;
			instance.transform.rotation = script.transform.rotation;

			Animator animator = instance.GetComponentInChildren<Animator>();
			bool isAnimator = animator != null;
			if (!isAnimator || !animator.isHuman) {
				Debug.LogWarning("The demo character controller requires a Humanoid character.");
				return;
			}

			SkinnedMeshRenderer[] skinnedMeshRenders = instance.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer s in skinnedMeshRenders) s.updateWhenOffscreen = true;

			// Unparent "Character Controller" from the "ThirdPersonPuppet"
			Transform oldRoot = script.transform.parent;
			PuppetMaster oldPM = script.transform.parent.GetComponentInChildren<PuppetMaster>();
			RigidbodyInterpolation oldInterpolation = oldPM.muscles[0].joint.GetComponent<Rigidbody>().interpolation;
			BehaviourBase[] behaviours = script.transform.parent.GetComponentsInChildren<BehaviourBase>();
			Camera camera = script.transform.parent.GetComponentInChildren<Camera>();
			CameraController cameraController = null;
			if (camera != null) cameraController = camera.gameObject.GetComponent<CameraController>();
			LayerSetup layerSetup = oldPM.GetComponent<LayerSetup>();

			script.transform.parent = null;

			// Delete the "Pilot"
			DestroyImmediate(script.characterAnimation.transform.GetChild(0).gameObject);

			// Replace the Avatar in the Animator component of the "Animation Controller" gameobject.
			Animator characterAnimator = script.characterAnimation.GetComponent<Animator>();
			characterAnimator.avatar = animator.avatar;

			// Add BipedRagdollCreator to the "Animation Controller", use it to create a ragdoll, delete the component when done
			Joint joint = instance.GetComponentInChildren<Joint>();
			bool isRagdoll = joint != null;
			bool isConfigurable = isRagdoll && joint is ConfigurableJoint;

			if (isRagdoll && !isConfigurable) {
				JointConverter.ToConfigurable(instance);
			}

			if (!isRagdoll) {
				BipedRagdollReferences r = BipedRagdollReferences.FromAvatar(animator);
				BipedRagdollCreator.Options options = BipedRagdollCreator.AutodetectOptions(r);
				
				// Edit options here if you need to
				
				BipedRagdollCreator.Create(r, options);
			}

			// Delete the Animator from your character root (not the one on "Animation Controller")
			DestroyImmediate(animator);

			// Add PuppetMaster to the "Character Controller", click on Set Up PuppetMaster.
			int cLayer = layerSetup != null? layerSetup.characterControllerLayer: script.gameObject.layer;
			int rLayer = layerSetup != null? layerSetup.ragdollLayer: oldPM.muscles[0].joint.gameObject.layer;

			PuppetMaster newPuppetMaster = PuppetMaster.SetUp(script.transform, cLayer, rLayer);
			foreach (Muscle m in newPuppetMaster.muscles) {
				m.joint.GetComponent<Rigidbody>().interpolation = oldInterpolation;
			}

			// Move the "Puppet" and "Fall" behaviours from the old rig to the new one (parent them to the "Behaviours").
			Transform behaviourRoot = newPuppetMaster.transform.parent.Find("Behaviours");
			Transform oldBehaviourRoot = behaviours.Length > 0? behaviours[0].transform.parent: FindChildNameContaining(oldRoot, "Behaviours");
			foreach (BehaviourBase behaviour in behaviours) behaviour.transform.parent = behaviourRoot;
			if (oldBehaviourRoot != null) DestroyImmediate(oldBehaviourRoot.gameObject);

			// 9. Move the "Character Camera" to the new rig, replace the "Target" of the CharacterController component
			if (camera != null) camera.transform.parent = newPuppetMaster.transform.parent;
			if (cameraController != null) cameraController.target = newPuppetMaster.muscles[0].joint.transform;

			// THIS IS NOT REQUIRED SINCE 0.3 Use the RagdollEditor (add it to the PuppetMaster) to multiply the mass of the ragdoll by 10

			// 10. Copy the LayerSetup component from the old PuppetMaster to the new one if you have not set up the layers in the Layer Collision Matrix.
			if (layerSetup != null) {
				LayerSetup newLayerSetup = newPuppetMaster.gameObject.AddComponent<LayerSetup>();
				if (newLayerSetup != null) {
					newLayerSetup.characterController = layerSetup.characterController;
					newLayerSetup.characterControllerLayer = layerSetup.characterControllerLayer;
					newLayerSetup.ragdollLayer = layerSetup.ragdollLayer;
					newLayerSetup.ignoreCollisionWithCharacterController = layerSetup.ignoreCollisionWithCharacterController;
				}
			}

			//	11. Make sure the new PuppetMaster settings match with the old one, delete the old rig
			newPuppetMaster.state = oldPM.state;
			newPuppetMaster.stateSettings = oldPM.stateSettings;
			newPuppetMaster.mode = oldPM.mode;
			newPuppetMaster.blendTime = oldPM.blendTime;
			newPuppetMaster.fixTargetTransforms = oldPM.fixTargetTransforms;
			newPuppetMaster.solverIterationCount = oldPM.solverIterationCount;
			newPuppetMaster.visualizeTargetPose = oldPM.visualizeTargetPose;
			newPuppetMaster.mappingWeight = oldPM.mappingWeight;
			newPuppetMaster.pinWeight = oldPM.pinWeight;
			newPuppetMaster.muscleWeight = oldPM.muscleWeight;
			newPuppetMaster.muscleSpring = oldPM.muscleSpring;
			newPuppetMaster.muscleDamper = oldPM.muscleDamper;
			newPuppetMaster.pinPow = oldPM.pinPow;
			newPuppetMaster.pinDistanceFalloff = oldPM.pinDistanceFalloff;
			newPuppetMaster.updateJointAnchors = oldPM.updateJointAnchors;
			newPuppetMaster.supportTranslationAnimation = oldPM.supportTranslationAnimation;
			newPuppetMaster.angularLimits = oldPM.angularLimits;
			newPuppetMaster.internalCollisions = oldPM.internalCollisions;

			// 12 Parent this CharacterPuppet to the new puppet root
			script.transform.parent = newPuppetMaster.transform.parent;

			// 13. Delete the old root
			script.transform.parent.name = oldRoot.name;

			for (int i = 0; i < oldRoot.childCount; i++) {
				var child = oldRoot.GetChild(i);
				if (child != oldPM.transform && child != behaviourRoot) {
					child.parent = script.transform.parent;
				}
			}
			DestroyImmediate(oldRoot.gameObject);
		}

		private static Transform FindChildNameContaining(Transform t, string name) {
			for (int i = 0; i < t.childCount; i++) {
				if (t.GetChild(i).name.Contains(name)) return t.GetChild(i);
			}
			return null;
		}
	}
}
