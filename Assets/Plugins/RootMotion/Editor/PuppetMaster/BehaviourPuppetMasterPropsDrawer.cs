using UnityEditor;
using UnityEngine;

namespace RootMotion.Dynamics {

	// Custom drawer for BehaviourPuppet MasterProps.
	[CustomPropertyDrawer (typeof (BehaviourPuppet.MasterProps))]
	public class BehaviourPuppetMasterPropsDrawer : PropertyDrawer {

		private float lineHeight = 22f;

		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
			lineHeight = base.GetPropertyHeight(prop, label);

			int enumValueIndex = prop.FindPropertyRelative("normalMode").enumValueIndex;
			
			switch(enumValueIndex) {
			case 1: return lineHeight * 2;
			case 2: return lineHeight * 3;
			default: return lineHeight;
			}
		}

		private void NextLine(ref Rect pos) {
			pos = EditorGUI.IndentedRect(pos);
			pos.width -= pos.x - pos.x;
			pos.x = pos.x;
			pos.y += lineHeight;
			pos.height = lineHeight;

		}

		public override void OnGUI (Rect pos, SerializedProperty prop, GUIContent label) {
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUI.BeginChangeCheck();

			EditorGUI.PropertyField (new Rect(pos.x, pos.y, pos.width, lineHeight), prop.FindPropertyRelative ("normalMode"), new GUIContent("Normal Mode", "Active mode keeps the puppet and it's mapping active at all times. " +
			                                                                                                                                 "Unmapped mode blends in mapping only if the puppet becomes in contact with something and blends out again later to maintain 100% animation accuracy while not in contact. " +
	                                                                                                                                 "If Kinematic, will set the PuppetMaster.mode to Active when any of the muscles collides with something (static colliders are ignored if 'Activate On Static Collisions' is false) or is hit. PuppetMaster.mode will be set to Kinematic when all muscles are pinned and the puppet is in normal Puppet state. This will increase performance and enable you to have full animation accuracy when PuppetMaster is not needed. Note that collision events are only broadcasted if one of the Rigidbodies is not kinematic, so if you need 2 characters to wake up each other, one has to be active."));

			int enumValueIndex = prop.FindPropertyRelative("normalMode").enumValueIndex;

			switch(enumValueIndex) {
			case 1:
				NextLine(ref pos);
				EditorGUI.PropertyField (pos, prop.FindPropertyRelative ("mappingBlendSpeed"), new GUIContent("Mapping Blend Speed", "The speed of blending in mapping in case of contact."));
				break;
			case 2:
				NextLine(ref pos);
				EditorGUI.PropertyField (pos, prop.FindPropertyRelative ("activateOnStaticCollisions"), new GUIContent("Activate On Static Collisions", "The speed of blending in mapping in case of contact."));

				NextLine(ref pos);
				EditorGUI.PropertyField (pos, prop.FindPropertyRelative ("activateOnImpulse"), new GUIContent("Activate On Impulse", "Minimum collision impulse for activating the puppet."));
				break;
			default: break;
			}

			EditorGUI.EndChangeCheck();
			EditorGUI.indentLevel = indent;
		}
	}
}