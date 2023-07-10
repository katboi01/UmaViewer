using UnityEngine;
using UnityEditor;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {
	
	[CustomEditor(typeof(CharacterMeleeDemo))]
	public class CharacterMeleeDemoInspector : Editor {
		
		private CharacterMeleeDemo script { get { return target as CharacterMeleeDemo; }}
		
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
                        Debug.Log("PropMuscle needs to be set up manually and assigned as 'Prop Muscle' in the CharacterMeleeDemo component on the character.");

                        /* TODO Update to PropMuscle
						PropRoot propRoot = script.propRoot;
						Vector3 localPosition = propRoot.transform.localPosition;
						Quaternion localRotation = propRoot.transform.localRotation;
						propRoot.transform.parent = null;

						CharacterPuppetInspector.ReplacePuppetModel(script as CharacterThirdPerson, replace);

						Animator animator = script.characterAnimation.GetComponent<Animator>();
						PuppetMaster puppetMaster = script.transform.parent.GetComponentInChildren<PuppetMaster>();

						propRoot.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
						propRoot.transform.localPosition = localPosition;
						propRoot.transform.localRotation = localRotation;
						propRoot.puppetMaster = puppetMaster;
						propRoot.connectTo = GetRigidbody(puppetMaster, animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
                        
						Debug.Log("You probably need to adjust the localPosition and localRotation of the Prop Root to match this character's hand.");
                        */
                        UserControlAI[] userControls = (UserControlAI[])GameObject.FindObjectsOfType<UserControlAI>();
						foreach (UserControlAI ai in userControls) {
							if (ai.moveTarget == null) {
								ai.moveTarget = script.transform.parent.GetComponentInChildren<PuppetMaster>().muscles[0].joint.transform;
							}
						}
					}
				}
				
				EditorGUILayout.EndHorizontal();
				GUI.color = Color.white;
			}
			
			DrawDefaultInspector();

			if (GUI.changed) EditorUtility.SetDirty(script);
		}

		private Rigidbody GetRigidbody(PuppetMaster puppetMaster, Transform target) {
			foreach (Muscle m in puppetMaster.muscles) {
				if (m.target == target) return m.joint.GetComponent<Rigidbody>();
			}
			return null;
		}
	}
}
