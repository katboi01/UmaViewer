using UnityEngine;
using UnityEditor;
using System.Collections;
using RootMotion;

namespace RootMotion.Dynamics {

	[CustomEditor(typeof(RagdollEditor))]
	public class RagdollEditorInspector : Editor {
		
		public RagdollEditor script { get { return target as RagdollEditor; }}

		private float massMlp = 1f;
		private Transform t;
		private bool isDragging;

		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			GUILayout.Space(10);

			EditorGUILayout.LabelField("Rigidbodies", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();
			massMlp = EditorGUILayout.FloatField("Mass Multiplier", massMlp);
			if (massMlp <= 0f) massMlp = 0.00001f;

			if (GUILayout.Button(new GUIContent("Multiply", "Multiplies the mass of all child Rigidbodies."))) {
				Rigidbody[] rigidbodies = GetRigidbodies();
				Undo.RecordObjects(rigidbodies, "Multiply Mass");

				float totalMass = 0f;
				foreach (Rigidbody r in rigidbodies) {
					r.mass *= massMlp;
					totalMass += r.mass;
				}
				Debug.Log("Character mass: " + totalMass);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Set All Kinematic", "Enables 'Is Kinematic' on all child Rigidbodies."))) {
				Rigidbody[] rigidbodies = GetRigidbodies();
				Undo.RecordObjects(rigidbodies, "Set All Kinematic");
				
				foreach (Rigidbody r in rigidbodies) {
					r.isKinematic = true;
				}
			}
			
			if (GUILayout.Button(new GUIContent("Set All Non-Kinematic", "Disables 'Is Kinematic' on all child Rigidbodies."))) {
				Rigidbody[] rigidbodies = GetRigidbodies();
				Undo.RecordObjects(rigidbodies, "Set All Non-Kinematic");
				
				foreach (Rigidbody r in rigidbodies) {
					r.isKinematic = false;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			EditorGUILayout.LabelField("Joints", EditorStyles.boldLabel);
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Disable Preprocessing", "Unchecks 'Enable Preprocessing' on all joints. This can make ragdolls more stable."))) {
				Joint[] joints = GetJoints();
				Undo.RecordObjects(joints, "Disable Preprocessing");
				
				foreach (Joint j in joints) {
					j.enablePreprocessing = false;
				}
			}
			
			if (GUILayout.Button(new GUIContent("Enable Preprocessing", "Checks 'Enable Preprocessing' on all joints."))) {
				Joint[] joints = GetJoints();
				Undo.RecordObjects(joints, "Enable Preprocessing");
				
				foreach (Joint j in joints) {
					j.enablePreprocessing = true;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10);


			GUILayout.Label("Use the Scene View gizmos to edit Colliders and Joint limits", EditorStyles.miniLabel);
		}

		private Rigidbody[] GetRigidbodies() {
			return (Rigidbody[])script.GetComponentsInChildren<Rigidbody>();
		}

		private Joint[] GetJoints() {
			return (Joint[])script.GetComponentsInChildren<Joint>();
		}

		void OnEnable() {
			t = script.transform;
		}

		void OnDisable() {
			if (script == null) return;

			Collider[] colliders = script.transform.GetComponentsInChildren<Collider>();
			foreach (Collider collider in colliders) collider.enabled = true;
		}

		void OnDestroy() {
			if (t == null) return;

			Collider[] colliders = t.GetComponentsInChildren<Collider>();
			foreach (Collider collider in colliders) collider.enabled = true;
		}

		void OnSceneGUI() {
			Rigidbody[] rigidbodies = script.transform.GetComponentsInChildren<Rigidbody>();
			if (rigidbodies.Length == 0) return;

			Collider[] colliders = script.transform.GetComponentsInChildren<Collider>();

			if (script.selectedRigidbody == null) script.selectedRigidbody = rigidbodies[0];
			if (script.selectedCollider == null && colliders.Length > 0) script.selectedCollider = colliders[0];

			Rigidbody symmetricRigidbody = script.symmetry && script.selectedRigidbody != null? SymmetryTools.GetSymmetric(script.selectedRigidbody, rigidbodies, script.transform): null;
			Collider symmetricCollider = script.symmetry && script.selectedCollider != null? SymmetryTools.GetSymmetric(script.selectedCollider, colliders, script.transform): null;

			Joint joint = script.selectedRigidbody != null? script.selectedRigidbody.GetComponent<Joint>(): null;
			Joint symmetricJoint = joint != null && symmetricRigidbody != null? symmetricRigidbody.GetComponent<Joint>(): null;

			// Selected
			Transform selected = script.mode == RootMotion.Dynamics.RagdollEditor.Mode.Colliders? (script.selectedCollider != null? script.selectedCollider.transform: null): (script.selectedRigidbody != null? script.selectedRigidbody.transform: null);

			if (selected != null) {
				Handles.BeginGUI();		
				GUILayout.BeginVertical(new GUIContent("Ragdoll Editor", string.Empty), "Window", GUILayout.Width(200), GUILayout.Height(20));
				GUILayout.BeginHorizontal();
				GUILayout.Label(selected.name);
				if (GUILayout.Button("Select GameObject", EditorStyles.miniButton)) {
					Selection.activeGameObject = selected.gameObject;
				}
				GUILayout.EndHorizontal();

				GUILayout.Space(10);

				//GUILayout.BeginVertical("Box");

				GUILayout.BeginHorizontal();
				GUILayout.Label("Edit Mode", GUILayout.Width(86));
				script.mode = (RagdollEditor.Mode)EditorGUILayout.EnumPopup(string.Empty, script.mode, GUILayout.Width(100));
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				GUILayout.Label("Symmetry", GUILayout.Width(86));
				script.symmetry = GUILayout.Toggle(script.symmetry, string.Empty);
				GUILayout.EndHorizontal();

				GUILayout.Space(10);

				// COLLIDERS
				if (script.mode == RagdollEditor.Mode.Colliders && selected != null) {

					if (script.selectedCollider is CapsuleCollider) {
						var capsule = script.selectedCollider as CapsuleCollider;
						var capsuleS = symmetricCollider != null? symmetricCollider.transform.GetComponent<CapsuleCollider>(): null;
							
						if (GUILayout.Button("Convert To Box Collider")) {
							ColliderTools.ConvertToBoxCollider(capsule);
							script.selectedCollider = selected.GetComponent<Collider>();
							if (capsuleS != null) ColliderTools.ConvertToBoxCollider(capsuleS);
							return;
						}

						if (GUILayout.Button("Convert To Sphere Collider")) {
							ColliderTools.ConvertToSphereCollider(capsule);
							script.selectedCollider = selected.GetComponent<Collider>();
							if (capsuleS != null) ColliderTools.ConvertToSphereCollider(capsuleS);
							return;
						}
							
						string capsuleDir = capsule.direction == 0? "X": (capsule.direction == 1? "Y": "Z");
							
						if (GUILayout.Button("Direction: " + capsuleDir)) {
							if (capsuleS == null) {
								Undo.RecordObject(capsule, "Change Capsule Direction");
							} else {
								Undo.RecordObjects(new Object[2] { capsule, capsuleS }, "Change Capsule Direction");
							}

							capsule.direction ++;
							if (capsule.direction > 2) capsule.direction = 0;
							if (capsuleS != null) capsuleS.direction = capsule.direction; 
						}
					} else if (script.selectedCollider is BoxCollider) {
						var box = script.selectedCollider as BoxCollider;
						var boxS = symmetricCollider != null? symmetricCollider.transform.GetComponent<BoxCollider>(): null;
							
						if (GUILayout.Button("Convert To Capsule Collider")) {
							ColliderTools.ConvertToCapsuleCollider(box);
							script.selectedCollider = selected.GetComponent<Collider>();
							if (boxS != null) ColliderTools.ConvertToCapsuleCollider(boxS);
							return;
						}

						if (GUILayout.Button("Convert To Sphere Collider")) {
							ColliderTools.ConvertToSphereCollider(box);
							script.selectedCollider = selected.GetComponent<Collider>();
							if (boxS != null) ColliderTools.ConvertToSphereCollider(boxS);
							return;
						}
							
						if (GUILayout.Button("Rotate Collider")) {
							if (boxS == null) {
								Undo.RecordObject(box, "Rotate Collider");
							} else {
								Undo.RecordObjects(new Object[2] { box, boxS }, "Rotate Collider");
							}

							box.size = new Vector3(box.size.y, box.size.z, box.size.x);
							if (boxS != null) boxS.size = box.size;
						}
					} else if (script.selectedCollider is SphereCollider) {
						var sphere = script.selectedCollider as SphereCollider;
						var sphereS = symmetricCollider != null? symmetricCollider.transform.GetComponent<SphereCollider>(): null;
					
						if (GUILayout.Button("Convert To Capsule Collider")) {
							ColliderTools.ConvertToCapsuleCollider(sphere);
							script.selectedCollider = selected.GetComponent<Collider>();
							if (sphereS != null) ColliderTools.ConvertToCapsuleCollider(sphereS);
							return;
						}

						if (GUILayout.Button("Convert To Box Collider")) {
							ColliderTools.ConvertToBoxCollider(sphere);
							script.selectedCollider = selected.GetComponent<Collider>();
							if (sphereS != null) ColliderTools.ConvertToBoxCollider(sphereS);
							return;
						}
					}
				}

				// JOINTS
				if (script.mode == RagdollEditor.Mode.Joints) {
					if (joint != null && (joint is CharacterJoint || joint is ConfigurableJoint)) {

						GUILayout.BeginHorizontal();
						GUILayout.Label("Connected Body");

						var lastConnectedBody = joint.connectedBody;
						var newConnectedBody = (Rigidbody)EditorGUILayout.ObjectField(joint.connectedBody, typeof(Rigidbody), true);
						if (newConnectedBody != lastConnectedBody) {
							Undo.RecordObject(joint, "Changing Joint ConnectedBody");
							joint.connectedBody = newConnectedBody;
						}

						GUILayout.EndHorizontal();

						if (joint is CharacterJoint) {
							var j = joint as CharacterJoint;
							var sJ = symmetricJoint != null && symmetricJoint is CharacterJoint? symmetricJoint as CharacterJoint: null;

							if (GUILayout.Button("Convert to Configurable")) {
								JointConverter.CharacterToConfigurable(j);

								if (sJ != null) {
									JointConverter.CharacterToConfigurable(sJ);
								}
							}

							if (GUILayout.Button("Switch Yellow/Green")) {
								JointTools.SwitchXY(ref j);
								if (sJ != null) JointTools.SwitchXY(ref sJ);
							}
							
							if (GUILayout.Button("Switch Yellow/Blue")) {
								JointTools.SwitchXZ(ref j);
								if (sJ != null) JointTools.SwitchXZ(ref sJ);
							}
							
							if (GUILayout.Button("Switch Green/Blue")) {
								JointTools.SwitchYZ(ref j);
								if (sJ != null) JointTools.SwitchYZ(ref sJ);
							}

							if (GUILayout.Button("Invert Yellow")) {
								JointTools.InvertAxis(ref joint);
								if (sJ != null) JointTools.InvertAxis(ref symmetricJoint);
							}
						}

						if (joint is ConfigurableJoint) {
							var j = joint as ConfigurableJoint;
							var sJ = symmetricJoint != null && symmetricJoint is ConfigurableJoint? symmetricJoint as ConfigurableJoint: null;

							if (GUILayout.Button("Switch Yellow/Green")) {
								JointTools.SwitchXY(ref j);
								if (sJ != null) JointTools.SwitchXY(ref sJ);
							}

							if (GUILayout.Button("Switch Yellow/Blue")) {
								JointTools.SwitchXZ(ref j);
								if (sJ != null) JointTools.SwitchXZ(ref sJ);
							}

							if (GUILayout.Button("Switch Green/Blue")) {
								JointTools.SwitchYZ(ref j);
								if (sJ != null) JointTools.SwitchYZ(ref sJ);
							}

							if (GUILayout.Button("Invert Yellow")) {
								JointTools.InvertAxis(ref joint);
								if (sJ != null) JointTools.InvertAxis(ref symmetricJoint);
							}
						}
					}
				}

				GUILayout.EndVertical();
				//GUILayout.EndArea();
				Handles.EndGUI();

				if (script.mode == RagdollEditor.Mode.Joints && joint != null) {
					if (joint is CharacterJoint) {
						var j = joint as CharacterJoint;

						SoftJointLimit lowTwistLimit = j.lowTwistLimit;
						SoftJointLimit highTwistLimit = j.highTwistLimit;
						SoftJointLimit swing1Limit = j.swing1Limit;
						SoftJointLimit swing2Limit = j.swing2Limit;

						CharacterJointInspector.DrawJoint(j);

						if (symmetricJoint != null && symmetricJoint is CharacterJoint) {
							var sJ = symmetricJoint as CharacterJoint;

							CharacterJointInspector.DrawJoint(sJ, false, 0.5f);

							// Low Twist
							if (lowTwistLimit.limit != j.lowTwistLimit.limit) {
								Undo.RecordObject(sJ, "Change Joint Limits");
								
								Vector3 lowXAxisWorld = JointTools.GetLowXAxisWorld(j);
								Vector3 lowXAxisWorldS = JointTools.GetLowXAxisWorld(sJ);
								Vector3 lowXAxisWorldMirror = SymmetryTools.Mirror(lowXAxisWorld, script.transform);
								bool low = Vector3.Dot(lowXAxisWorldMirror, lowXAxisWorldS) < 0f;
								
								JointAxis sJointAxis = JointTools.GetSymmetricJointAxis(j, JointAxis.Primary, sJ, script.transform);
								float delta = j.lowTwistLimit.limit - lowTwistLimit.limit;
								
								JointTools.ApplyXDeltaToJointLimit(ref sJ, delta, sJointAxis, low);
							}

							// High Twist
							if (highTwistLimit.limit != j.highTwistLimit.limit) {
								Undo.RecordObject(sJ, "Change Joint Limits");
								
								Vector3 highXAxisWorld = JointTools.GetHighXAxisWorld(j);
								Vector3 highXAxisWorldS = JointTools.GetHighXAxisWorld(sJ);
								Vector3 highXAxisWorldMirror = SymmetryTools.Mirror(highXAxisWorld, script.transform);
								bool low = Vector3.Dot(highXAxisWorldMirror, highXAxisWorldS) > 0f;
								
								JointAxis sJointAxis = JointTools.GetSymmetricJointAxis(j, JointAxis.Primary, sJ, script.transform);
								float delta = j.highTwistLimit.limit - highTwistLimit.limit;
								
								JointTools.ApplyXDeltaToJointLimit(ref sJ, -delta, sJointAxis, low);
							}

							// Swing 1
							if (swing1Limit.limit != j.swing1Limit.limit) {
								Undo.RecordObject(sJ, "Change Joint Limits");
								JointAxis sJointAxis = JointTools.GetSymmetricJointAxis(j, JointAxis.Secondary, sJ, script.transform);
								float delta = j.swing1Limit.limit - swing1Limit.limit;
								
								JointTools.ApplyDeltaToJointLimit(ref sJ, delta, sJointAxis);
							}

							// Swing 2
							if (swing2Limit.limit != j.swing2Limit.limit) {
								Undo.RecordObject(sJ, "Change Joint Limits");
								JointAxis sJointAxis = JointTools.GetSymmetricJointAxis(j, JointAxis.Tertiary, sJ, script.transform);
								float delta = j.swing2Limit.limit - swing2Limit.limit;
								
								JointTools.ApplyDeltaToJointLimit(ref sJ, delta, sJointAxis);
							}
						}

					} else if (joint is ConfigurableJoint) {

						var j = joint as ConfigurableJoint;

						SoftJointLimit lowAngularXLimit = j.lowAngularXLimit;
						SoftJointLimit highAngularXLimit = j.highAngularXLimit;
						SoftJointLimit angularYLimit = j.angularYLimit;
						SoftJointLimit angularZLimit = j.angularZLimit;

						ConfigurableJointInspector.DrawJoint(j);

						if (symmetricJoint != null && symmetricJoint is ConfigurableJoint) {
							var sJ = symmetricJoint as ConfigurableJoint;

							ConfigurableJointInspector.DrawJoint(sJ, false, 0.5f);
						
							// Low X
							if (lowAngularXLimit.limit != j.lowAngularXLimit.limit) {
								Undo.RecordObject(sJ, "Change Joint Limits");

								Vector3 lowXAxisWorld = JointTools.GetLowXAxisWorld(j);
								Vector3 lowXAxisWorldS = JointTools.GetLowXAxisWorld(sJ);
								Vector3 lowXAxisWorldMirror = SymmetryTools.Mirror(lowXAxisWorld, script.transform);
								bool low = Vector3.Dot(lowXAxisWorldMirror, lowXAxisWorldS) < 0f;

								JointAxis sJointAxis = JointTools.GetSymmetricJointAxis(j, JointAxis.Primary, sJ, script.transform);
								float delta = j.lowAngularXLimit.limit - lowAngularXLimit.limit;

								JointTools.ApplyXDeltaToJointLimit(ref sJ, delta, sJointAxis, low);
							}

							// High X
							if (highAngularXLimit.limit != j.highAngularXLimit.limit) {
								Undo.RecordObject(sJ, "Change Joint Limits");

								Vector3 highXAxisWorld = JointTools.GetHighXAxisWorld(j);
								Vector3 highXAxisWorldS = JointTools.GetHighXAxisWorld(sJ);
								Vector3 highXAxisWorldMirror = SymmetryTools.Mirror(highXAxisWorld, script.transform);
								bool low = Vector3.Dot(highXAxisWorldMirror, highXAxisWorldS) > 0f;

								JointAxis sJointAxis = JointTools.GetSymmetricJointAxis(j, JointAxis.Primary, sJ, script.transform);
								float delta = j.highAngularXLimit.limit - highAngularXLimit.limit;

								JointTools.ApplyXDeltaToJointLimit(ref sJ, -delta, sJointAxis, low);
							}

							// Y
							if (angularYLimit.limit != j.angularYLimit.limit) {
								Undo.RecordObject(sJ, "Change Joint Limits");
								JointAxis sJointAxis = JointTools.GetSymmetricJointAxis(j, JointAxis.Secondary, sJ, script.transform);
								float delta = j.angularYLimit.limit - angularYLimit.limit;

								JointTools.ApplyDeltaToJointLimit(ref sJ, delta, sJointAxis);
							}

							// Z
							if (angularZLimit.limit != j.angularZLimit.limit) {
								Undo.RecordObject(sJ, "Change Joint Limits");
								JointAxis sJointAxis = JointTools.GetSymmetricJointAxis(j, JointAxis.Tertiary, sJ, script.transform);
								float delta = j.angularZLimit.limit - angularZLimit.limit;

								JointTools.ApplyDeltaToJointLimit(ref sJ, delta, sJointAxis);
							}
						}
					}
				}
			}

			if (!Application.isPlaying) {
				foreach (Collider c in colliders) {
					c.enabled = script.mode == RagdollEditor.Mode.Colliders;
				}
			}

			// HANDLES
			Color color = new Color(0.2f, 0.9f, 0.5f);
			Handles.color = color;

			if (Event.current.type == EventType.MouseUp) isDragging = false;
			if (Event.current.type == EventType.MouseDown) isDragging = false;

			if (script.mode == RagdollEditor.Mode.Colliders) {

				// Select/move scale colliders
				for (int i = 0; i < colliders.Length; i++) {

					if (colliders[i] == script.selectedCollider) {

						// Moving and scaling selected colliders
						switch(Tools.current) {
						case Tool.Move:
							Vector3 oldPosition = ColliderTools.GetColliderCenterWorld(colliders[i]);
							Vector3 newPosition = Handles.PositionHandle(oldPosition, colliders[i].transform.rotation);
							if (newPosition != oldPosition) {
								if (!isDragging) {
									Undo.RecordObjects(SymmetryTools.GetColliderPair(colliders[i], symmetricCollider), "Move Colliders");
									
									isDragging = true;
								}
								
								ColliderTools.SetColliderCenterWorld(colliders[i], symmetricCollider, newPosition, script.transform);
							}
							break;
						case Tool.Scale:
							Vector3 position = ColliderTools.GetColliderCenterWorld(colliders[i]);
							Vector3 oldSize = ColliderTools.GetColliderSize(colliders[i]);
							Vector3 oldSizeS = ColliderTools.GetColliderSize(symmetricCollider);
							Vector3 newSize = Handles.ScaleHandle(oldSize, position, colliders[i].transform.rotation, HandleUtility.GetHandleSize(position));
							if (newSize != oldSize) {
								if (!isDragging) {
									Undo.RecordObjects(SymmetryTools.GetColliderPair(colliders[i], symmetricCollider), "Scale Colliders");
									
									isDragging = true;
								}
								
								ColliderTools.SetColliderSize(colliders[i], symmetricCollider, newSize, oldSize, oldSizeS, script.transform);
							}
							
							Handles.color = color;
							break;
						}

						// Dot on selected collider
						if (Tools.current != Tool.Scale) {
							Handles.color = new Color(0.8f, 0.8f, 0.8f);
							Vector3 center = ColliderTools.GetColliderCenterWorld(colliders[i]);
							float size = GetHandleSize(center);
							Inspector.CubeCap(0, center, colliders[i].transform.rotation, size);
							Handles.color = color;
						}

					} else {
						// Selecting colliders
						Vector3 center = ColliderTools.GetColliderCenterWorld(colliders[i]);
						float size = GetHandleSize(center);
						
						if (Inspector.DotButton(center, Quaternion.identity, size * 0.5f, size * 0.5f)) {
							Undo.RecordObject(script, "Change RagdollEditor Selection");
							
							script.selectedCollider = colliders[i];
						}
					}


				}
			} else {
				// Select joints
				for (int i = 0; i < rigidbodies.Length; i++) {
					
					if (rigidbodies[i] == script.selectedRigidbody) {
						// Dots on selected joints
						Handles.color = new Color(0.8f, 0.8f, 0.8f);
						Vector3 center = rigidbodies[i].transform.position;
						float size = GetHandleSize(center);
						Inspector.CubeCap(0, center, rigidbodies[i].transform.rotation, size);
						Handles.color = color;
					} else {
						// Selecting joints
						Vector3 center = rigidbodies[i].transform.position;
						float size = GetHandleSize(center);
						
						if (Inspector.DotButton(center, Quaternion.identity, size * 0.5f, size * 0.5f)) {
							Undo.RecordObject(script, "Change RagdollEditor Selection");
							
							script.selectedRigidbody = rigidbodies[i];
						}
					}
				}
			}

			Handles.color = Color.white;
		}

		private static float GetHandleSize(Vector3 position) {
			float s = HandleUtility.GetHandleSize(position) * 0.1f;
			return Mathf.Lerp(s, 0.025f, 0.2f);
		}
	}
}
