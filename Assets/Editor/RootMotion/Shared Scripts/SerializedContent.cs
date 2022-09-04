using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion {

	/*
	 * Serialized content is used for caching serialized properties and their respective GUIContent and enforcing unified GUI layout.
	 * */
	public struct SerializedContent {
		public SerializedProperty prop;
		public GUIContent guiContent;
		
		public SerializedContent(SerializedProperty prop, GUIContent guiContent) {
			this.prop = prop;
			this.guiContent = guiContent;
		}
	}
}
