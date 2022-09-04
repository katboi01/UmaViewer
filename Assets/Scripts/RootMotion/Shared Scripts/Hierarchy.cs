using UnityEngine;
using System.Collections;
using System;

namespace RootMotion {

	/// <summary>
	/// Contains tools for working on Transform hierarchies.
	/// </summary>
	public class Hierarchy {
		
		/// <summary>
		/// Make sure the bones are in valid %Hierarchy
		/// </summary>
		public static bool HierarchyIsValid(Transform[] bones) {
			for (int i = 1; i < bones.Length; i++) {
				// If parent bone is not an ancestor of bone, the hierarchy is invalid
				if (!IsAncestor(bones[i], bones[i - 1])) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Checks if an array of objects contains any duplicates.
		/// </summary>
		public static UnityEngine.Object ContainsDuplicate(UnityEngine.Object[] objects) {
			for (int i = 0; i < objects.Length; i++) {
				for (int i2 = 0; i2 < objects.Length; i2++) {
					if (i != i2 && objects[i] == objects[i2]) return objects[i];
				}
			}
			return null;
		}
		
		/// <summary>
		/// Determines whether the second Transform is an ancestor to the first Transform.
		/// </summary>
		public static bool IsAncestor(Transform transform, Transform ancestor) {
			if (transform == null) return true;
			if (ancestor == null) return true;
			if (transform.parent == null) return false;
			if (transform.parent == ancestor) return true;
			return IsAncestor(transform.parent, ancestor);
		}

		/// <summary>
		/// Returns true if the transforms contains the child
		/// </summary>
		public static bool ContainsChild(Transform transform, Transform child) {
			if (transform == child) return true;
			
			Transform[] children = transform.GetComponentsInChildren<Transform>() as Transform[];
			foreach (Transform c in children) if (c == child) return true;
			return false;
		}
		
		/// <summary>
		/// Adds all Transforms until the blocker to the array
		/// </summary>
		public static void AddAncestors(Transform transform, Transform blocker, ref Transform[] array) {		
			if (transform.parent != null && transform.parent != blocker) {
				if (transform.parent.position != transform.position && transform.parent.position != blocker.position) {
					Array.Resize(ref array, array.Length + 1);
					array[array.Length - 1] = transform.parent;
				}
				AddAncestors(transform.parent, blocker, ref array);
			}
		}
		
		/// <summary>
		/// Gets the last ancestor that has more than minChildCount number of child Transforms 
		/// </summary>
		public static Transform GetAncestor(Transform transform, int minChildCount) {
			if (transform == null) return null;

			if (transform.parent != null) {
				if (transform.parent.childCount >= minChildCount) return transform.parent;
				return GetAncestor(transform.parent, minChildCount);
			}
			return null;
		}
		
		/// <summary>
		/// Gets the first common ancestor up the hierarchy
		/// </summary>
		public static Transform GetFirstCommonAncestor(Transform t1, Transform t2) {
			if (t1 == null) return null;
			if (t2 == null) return null;
			if (t1.parent == null) return null;
			if (t2.parent == null) return null;

			if (IsAncestor(t2, t1.parent)) return t1.parent;
			return GetFirstCommonAncestor(t1.parent, t2);
		}

		/// <summary>
		/// Gets the first common ancestor of the specified transforms.
		/// </summary>
		public static Transform GetFirstCommonAncestor(Transform[] transforms) {
			if (transforms == null) {
				Debug.LogWarning("Transforms is null.");
				return null;
			}
			if (transforms.Length == 0) {
				Debug.LogWarning("Transforms.Length is 0.");
				return null;
			}

			for (int i = 0; i < transforms.Length; i++) {
				if (transforms[i] == null) return null;

				if (IsCommonAncestor(transforms[i], transforms)) return transforms[i];
			}

			return GetFirstCommonAncestorRecursive(transforms[0], transforms);
		}

		/// <summary>
		/// Gets the first common ancestor recursively.
		/// </summary>
		public static Transform GetFirstCommonAncestorRecursive(Transform transform, Transform[] transforms) {
			if (transform == null) {
				Debug.LogWarning("Transform is null.");
				return null;
			}

			if (transforms == null) {
				Debug.LogWarning("Transforms is null.");
				return null;
			}
			if (transforms.Length == 0) {
				Debug.LogWarning("Transforms.Length is 0.");
				return null;
			}

			if (IsCommonAncestor(transform, transforms)) return transform;
			if (transform.parent == null) return null;
			return GetFirstCommonAncestorRecursive(transform.parent, transforms);
		}

		/// <summary>
		/// Determines whether the first parameter is the common ancestor of all the other specified transforms.
		/// </summary>
		public static bool IsCommonAncestor(Transform transform, Transform[] transforms) {
			if (transform == null) {
				Debug.LogWarning("Transform is null.");
				return false;
			}

			for (int i = 0; i < transforms.Length; i++) {
				if (transforms[i] == null) {
					Debug.Log("Transforms[" + i + "] is null.");
					return false;
				}
				if (!IsAncestor(transforms[i], transform) && transforms[i] != transform) return false;
			}
			return true;
		}
	}
}
