using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	// Custom inspector and scene view tools for IKSolverFullBodyBiped
	public class IKSolverFullBodyBipedInspector : IKSolverInspector {
		
		#region Public methods
		
		public static void AddReferences(bool editHierarchy, SerializedProperty prop) {
			// RootNode
			if (editHierarchy) {
				EditorGUILayout.PropertyField(prop.FindPropertyRelative("rootNode"), new GUIContent("Root Node", "Select one of the bones in the (lower) spine."));
			}
		}
		
		// Draws the custom inspector for IKSolverFullBodybiped
		public static void AddInspector(SerializedProperty prop, bool editWeights) {
			IKSolverFullBodyInspector.AddInspector(prop, editWeights);
			
			EditorGUILayout.Space();
			EditorGUI.indentLevel = 0;

			AddSolver(prop);
		}


		// Draws the scene view helpers for IKSolverFullBodyBiped
		public static void AddScene(UnityEngine.Object target, IKSolverFullBodyBiped solver, Color color, ref int selectedEffector, Transform root) {
			if (Application.isPlaying && !solver.initiated) return;
			if (!Application.isPlaying && !solver.IsValid()) return;

			bool modifiable = solver.initiated;

			float heightF = Vector3.Distance(solver.chain[1].nodes[0].transform.position, solver.chain[1].nodes[1].transform.position) + 
				Vector3.Distance(solver.chain[3].nodes[0].transform.position, solver.chain[3].nodes[1].transform.position);

			float size = Mathf.Clamp(heightF * 0.075f, 0.001f, Mathf.Infinity);

			// Bend goals
			for (int i = 0; i < solver.chain.Length; i++) {
				if (solver.chain[i].nodes.Length == 3 && solver.chain[i].bendConstraint.bendGoal != null && solver.chain[i].bendConstraint.weight > 0f) {
					Color c = color;
					c.a = solver.chain[i].bendConstraint.weight;
					Handles.color = c;
					
					Handles.DrawLine(solver.chain[i].nodes[1].transform.position, solver.chain[i].bendConstraint.bendGoal.position);
					Inspector.SphereCap(0, solver.chain[i].nodes[1].transform.position, Quaternion.identity, size * 0.5f);
					Inspector.SphereCap(0, solver.chain[i].bendConstraint.bendGoal.position, Quaternion.identity, size * 0.5f);
					
					Handles.color = Color.white;
				}
			}

			// Chain
			if (!modifiable) {
				for (int i = 0; i < solver.chain.Length; i++) {
					IKSolverFullBodyInspector.AddChain(solver.chain, i, color, size);
				}

				Handles.DrawLine(solver.chain[1].nodes[0].transform.position, solver.chain[2].nodes[0].transform.position);
				Handles.DrawLine(solver.chain[3].nodes[0].transform.position, solver.chain[4].nodes[0].transform.position);

				AddLimbHelper(solver.chain[1], size);
				AddLimbHelper(solver.chain[2], size);
				AddLimbHelper(solver.chain[3], size, root);
				AddLimbHelper(solver.chain[4], size, root);
			}
			
			// Effectors
			IKSolverFullBodyInspector.AddScene(target, solver, color, modifiable, ref selectedEffector, size);
		}

		// Scene view handles to help with limb setup
		private static void AddLimbHelper(FBIKChain chain, float size, Transform root = null) {
			Vector3 cross = Vector3.Cross((chain.nodes[1].transform.position - chain.nodes[0].transform.position).normalized, (chain.nodes[2].transform.position - chain.nodes[0].transform.position).normalized);

			Vector3 bendDirection = -Vector3.Cross(cross.normalized, (chain.nodes[2].transform.position - chain.nodes[0].transform.position).normalized);

			if (bendDirection != Vector3.zero) {
				Color c = Handles.color;
				bool inverted = root != null && Vector3.Dot(root.forward, bendDirection.normalized) < 0f;

				// Inverted bend direction
				if (inverted) {
					GUI.color = new Color(1f, 0.75f, 0.75f);
					Handles.color = Color.yellow;

					if (Inspector.DotButton(chain.nodes[1].transform.position, Quaternion.identity, size * 0.5f, size)) {
						Warning.logged = false;
						Warning.Log("The bend direction of this limb appears to be inverted. Please rotate this bone so that the limb is bent in it's natural bending direction. If this limb is supposed to be bent in the direction pointed by the arrow, ignore this warning.", root, true);
					}
				}

				Inspector.ArrowCap(0, chain.nodes[1].transform.position, Quaternion.LookRotation(bendDirection), size * 2f);

				GUI.color = Color.white;
				Handles.color = c;
			} else {
				// The limb is completely stretched out
				Color c = Handles.color;
				Handles.color = Color.red;
				GUI.color = new Color(1f, 0.75f, 0.75f);

				if (Inspector.DotButton(chain.nodes[1].transform.position, Quaternion.identity, size * 0.5f, size)) {
					Warning.logged = false;
					Warning.Log("The limb is completely stretched out. Full Body Biped IK does not know which way the limb should be bent. Please rotate this bone slightly in it's bending direction.", root, true);
				}

				GUI.color = Color.white;
				Handles.color = c;
			}
		}
		
		#endregion Public methods
		
		private const string style = "Box";

		private static void AddProperty(SerializedProperty prop, GUIContent guiContent) {
			GUILayout.BeginHorizontal();
			GUILayout.Space(50);
			EditorGUILayout.PropertyField(prop, guiContent);
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}

		private static void AddSolver(SerializedProperty prop) {
			var chains = prop.FindPropertyRelative("chain");

			AddBody(prop, chains.GetArrayElementAtIndex(0), new GUIContent("Body", string.Empty));
			AddLimb(prop, chains.GetArrayElementAtIndex(1), FullBodyBipedChain.LeftArm, new GUIContent("Left Arm", string.Empty));
			AddLimb(prop, chains.GetArrayElementAtIndex(2), FullBodyBipedChain.RightArm, new GUIContent("Right Arm", string.Empty));
			AddLimb(prop, chains.GetArrayElementAtIndex(3), FullBodyBipedChain.LeftLeg, new GUIContent("Left Leg", string.Empty));
			AddLimb(prop, chains.GetArrayElementAtIndex(4), FullBodyBipedChain.RightLeg, new GUIContent("Right Leg", string.Empty));
		}

		private static void AddBody(SerializedProperty prop, SerializedProperty chain, GUIContent guiContent) {
			EditorGUILayout.PropertyField(chain, guiContent, false);
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.BeginVertical();

			if (chain.isExpanded) {
				var effectors = prop.FindPropertyRelative("effectors");
				var effector = effectors.GetArrayElementAtIndex(0);
				var spineMapping = prop.FindPropertyRelative("spineMapping");
				var headMapping = prop.FindPropertyRelative("boneMappings").GetArrayElementAtIndex(0);

				GUILayout.BeginVertical(style);
				
				DrawLabel("Body Effector", startEffectorIcon);

				AddProperty(effector.FindPropertyRelative("target"), new GUIContent("Target", "Target Transform (optional, you can also use bodyEffector.position and bodyEffector.rotation directly)."));
				AddProperty(effector.FindPropertyRelative("positionWeight"), new GUIContent("Position Weight", "The weight of pinning the effector bone to the effector position."));
				AddProperty(effector.FindPropertyRelative("effectChildNodes"), new GUIContent("Use Thighs", "If true, the effect of the body effector will be applied to also the thigh effectors (IKEffector.effectChildNodes)."));
				
				DrawLabel("Chain", null);
				
				// Spine stiffness
				AddProperty(prop.FindPropertyRelative("spineStiffness"), new GUIContent("Spine Stiffness", "The bend resistance of the spine."));
				
				// Pull Body
				AddProperty(prop.FindPropertyRelative("pullBodyVertical"), new GUIContent("Pull Body Vertical", "Weight of hand effectors pulling the body vertically."));
				AddProperty(prop.FindPropertyRelative("pullBodyHorizontal"), new GUIContent("Pull Body Horizontal", "Weight of hand effectors pulling the body horizontally."));
				
				DrawLabel("Mapping", null);
				
				AddProperty(spineMapping.FindPropertyRelative("iterations"), new GUIContent("Spine Iterations", "The number of FABRIK iterations for mapping the spine bones to the solver armature."));
				AddProperty(spineMapping.FindPropertyRelative("twistWeight"), new GUIContent("Spine Twist Weight", "The weight of spine twist."));
				AddProperty(headMapping.FindPropertyRelative("maintainRotationWeight"), new GUIContent("Maintain Head Rot", "The weight of maintaining the bone's animated rotation in world space."));
				
				GUILayout.Space(5);
				GUILayout.EndVertical();
			}

			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private static void AddLimb(SerializedProperty prop, SerializedProperty chain, FullBodyBipedChain chainType, GUIContent guiContent) {
			EditorGUILayout.PropertyField(chain, guiContent, false);
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.BeginVertical();
			
			if (chain.isExpanded) {
				var effectors = prop.FindPropertyRelative("effectors");
				var endEffector = effectors.GetArrayElementAtIndex(GetEndEffectorIndex(chainType));
				var startEffector = effectors.GetArrayElementAtIndex(GetStartEffectorIndex(chainType));
				var mapping = prop.FindPropertyRelative("limbMappings").GetArrayElementAtIndex(GetLimbMappingIndex(chainType));

				GUILayout.BeginVertical(style);
				
				DrawLabel(GetEndEffectorName(chainType), endEffectorIcon);

				AddProperty(endEffector.FindPropertyRelative("target"), new GUIContent("Target", "Target Transform (optional, you can also use IKEffector.position and IKEffector.rotation directly)."));
				AddProperty(endEffector.FindPropertyRelative("positionWeight"), new GUIContent("Position Weight", "The weight of pinning the effector bone to the effector position."));
				AddProperty(endEffector.FindPropertyRelative("rotationWeight"), new GUIContent("Rotation Weight", "The weight of pinning the effector bone to the effector rotation."));
				AddProperty(endEffector.FindPropertyRelative("maintainRelativePositionWeight"), new GUIContent("Maintain Relative Pos", "Maintains the position of the hand/foot fixed relative to the chest/hips while effector positionWeight is not weighed in."));
				
				DrawLabel(GetStartEffectorName(chainType), startEffectorIcon);

				AddProperty(startEffector.FindPropertyRelative("target"), new GUIContent("Target", "Target Transform (optional, you can also use IKEffector.position and IKEffector.rotation directly)."));
				AddProperty(startEffector.FindPropertyRelative("positionWeight"), new GUIContent("Position Weight", "The weight of pinning the effector bone to the effector position."));
				
				DrawLabel("Chain", null);
				
				AddProperty(chain.FindPropertyRelative("pull"), new GUIContent("Pull", "The weight of pulling other chains."));
				AddProperty(chain.FindPropertyRelative("reach"), new GUIContent("Reach", "Pulls the first node closer to the last node of the chain."));
				AddProperty(chain.FindPropertyRelative("push"), new GUIContent("Push", "The weight of the end-effector pushing the first node."));
				AddProperty(chain.FindPropertyRelative("pushParent"), new GUIContent("Push Parent", "The amount of push force transferred to the parent (from hand or foot to the body)."));
				AddProperty(chain.FindPropertyRelative("reachSmoothing"), new GUIContent("Reach Smoothing", "Smoothing the effect of the Reach with the expense of some accuracy."));
				AddProperty(chain.FindPropertyRelative("pushSmoothing"), new GUIContent("Push Smoothing", "Smoothing the effect of the Push."));
				AddProperty(chain.FindPropertyRelative("bendConstraint").FindPropertyRelative("bendGoal"), new GUIContent("Bend Goal", "The Transform to bend towards (optional, you can also use ik.leftArmChain.bendConstraint.direction)."));
				AddProperty(chain.FindPropertyRelative("bendConstraint").FindPropertyRelative("weight"), new GUIContent("Bend Goal Weight", "The weight of to bending towards the Bend Goal (optional, you can also use ik.leftArmChain.bendConstraint.weight)."));

				DrawLabel("Mapping", null);
				
				AddProperty(mapping.FindPropertyRelative("weight"), new GUIContent("Mapping Weight", "The weight of mapping the limb to it's IK pose. This can be useful if you want to disable the effect of IK for the limb."));
				AddProperty(mapping.FindPropertyRelative("maintainRotationWeight"), new GUIContent(GetEndBoneMappingName(chainType), "The weight of maintaining the bone's animated rotation in world space."));
				
				GUILayout.Space(5);
				GUILayout.EndVertical();
			}

			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private static void DrawLabel(string label, Texture2D texture) {
			GUILayout.Space(3);
			GUILayout.BeginHorizontal();
			
			if (texture != null) {
				Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(16), GUILayout.Height(16));
				GUI.DrawTexture(rect, texture);
			} else GUILayout.Space(21);
			
			EditorGUILayout.LabelField(new GUIContent(label, string.Empty));
			GUILayout.EndHorizontal();
			GUILayout.Space(3);
		}

		private static string GetEndEffectorName(FullBodyBipedChain chain) {
			switch(chain) {
			case FullBodyBipedChain.LeftArm: return "Left Hand Effector";
			case FullBodyBipedChain.RightArm: return "Right Hand Effector";
			case FullBodyBipedChain.LeftLeg: return "Left Foot Effector";
			case FullBodyBipedChain.RightLeg: return "Right Foot Effector";
			default: return string.Empty;
			}
		}
		
		private static string GetStartEffectorName(FullBodyBipedChain chain) {
			switch(chain) {
			case FullBodyBipedChain.LeftArm: return "Left Shoulder Effector";
			case FullBodyBipedChain.RightArm: return "Right Shoulder Effector";
			case FullBodyBipedChain.LeftLeg: return "Left Thigh Effector";
			case FullBodyBipedChain.RightLeg: return "Right Thigh Effector";
			default: return string.Empty;
			}
		}
		
		private static string GetEndBoneMappingName(FullBodyBipedChain chain) {
			switch(chain) {
			case FullBodyBipedChain.LeftArm: return "Maintain Hand Rot";
			case FullBodyBipedChain.RightArm: return "Maintain Hand Rot";
			case FullBodyBipedChain.LeftLeg: return "Maintain Foot Rot";
			case FullBodyBipedChain.RightLeg: return "Maintain Foot Rot";
			default: return string.Empty;
			}
		}

		private static int GetEndEffectorIndex(FullBodyBipedChain chainType) {
			switch(chainType) {
			case FullBodyBipedChain.LeftArm: return 5;
			case FullBodyBipedChain.RightArm: return 6;
			case FullBodyBipedChain.LeftLeg: return 7;
			case FullBodyBipedChain.RightLeg: return 8;
			}
			return 0;
		}
		
		private static int GetStartEffectorIndex(FullBodyBipedChain chainType) {
			switch(chainType) {
			case FullBodyBipedChain.LeftArm: return 1;
			case FullBodyBipedChain.RightArm: return 2;
			case FullBodyBipedChain.LeftLeg: return 3;
			case FullBodyBipedChain.RightLeg: return 4;
			}
			return 0;
		}
		
		private static int GetLimbMappingIndex(FullBodyBipedChain chainType) {
			switch(chainType) {
			case FullBodyBipedChain.LeftArm: return 0;
			case FullBodyBipedChain.RightArm: return 1;
			case FullBodyBipedChain.LeftLeg: return 2;
			case FullBodyBipedChain.RightLeg: return 3;
			}
			return 0;
		}

		private static Texture2D endEffectorIcon {
			get {
				if (_endEffectorIcon == null) _endEffectorIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RootMotion/FinalIK/Gizmos/EndEffector Icon.png", typeof(Texture2D));
				return _endEffectorIcon;
			}
		}
		private static Texture2D _endEffectorIcon;
		
		private static Texture2D startEffectorIcon {
			get {
				if (_startEffectorIcon == null) _startEffectorIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RootMotion/FinalIK/Gizmos/MidEffector Icon.png", typeof(Texture2D));
				return _startEffectorIcon;
			}
		}
		private static Texture2D _startEffectorIcon;
		
		private static Texture2D chainIcon {
			get {
				if (_chainIcon == null) _chainIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RootMotion/FinalIK/Gizmos/Chain Icon.png", typeof(Texture2D));
				return _chainIcon;
			}
		}
		private static Texture2D _chainIcon;
		
		private static Texture2D mappingIcon {
			get {
				if (_mappingIcon == null) _mappingIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/RootMotion/FinalIK/Gizmos/Mapping Icon.png", typeof(Texture2D));
				return _mappingIcon;
			}
		}
		private static Texture2D _mappingIcon;
	}
}
