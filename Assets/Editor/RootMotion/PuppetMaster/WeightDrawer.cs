using UnityEngine;
using UnityEditor;

namespace RootMotion.Dynamics {

	[CustomPropertyDrawer (typeof (Weight))]
	public class WeightDrawer : PropertyDrawer {

		const int valueWidth = 30;
		const int gap = 20;
		const float min = 0;
		const float max = 1;

		public override void OnGUI (Rect pos, SerializedProperty prop, GUIContent label) {

			SerializedProperty mode = prop.FindPropertyRelative("mode");
			SerializedProperty floatValue = prop.FindPropertyRelative ("floatValue");
			SerializedProperty curve = prop.FindPropertyRelative ("curve");
			SerializedProperty tooltip = prop.FindPropertyRelative("tooltip");
			label.tooltip = tooltip.stringValue;

			Rect left = new Rect(pos.x, pos.y, pos.width - valueWidth - gap, pos.height);
			Rect right = new Rect (pos.width - valueWidth, pos.y, valueWidth, pos.height);

			Weight.Mode m = (Weight.Mode)mode.enumValueIndex;

			if (m == Weight.Mode.Float) {
				floatValue.floatValue = EditorGUI.FloatField(left, label, floatValue.floatValue);
			} else {
				// Draw curve
				int indent = EditorGUI.indentLevel;

				EditorGUI.PropertyField (left, curve, label);
				EditorGUI.indentLevel = indent;
			}

			int i = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			m = (Weight.Mode)EditorGUI.EnumPopup(right, GUIContent.none, ( Weight.Mode)mode.enumValueIndex, EditorStyles.miniBoldLabel);
			mode.enumValueIndex = (int)m;

			EditorGUI.indentLevel = i;
		}
	}
}