using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion {

	/*
	 * Custom inspector for BipedReferences
	 * */
	public class BipedReferencesInspector: Inspector {

		/*
		 * Draws the default property, returns true if modified
		 * */
		public static bool AddModifiedInspector(SerializedProperty prop) {
			EditorGUILayout.PropertyField(prop, true);
			
			if (prop.isExpanded) EditorGUILayout.Space();
			
			// If references have changed reinitiate the bipedIK.
			if (prop.serializedObject.ApplyModifiedProperties()) return true;
			
			return false;
		}
	}
}
