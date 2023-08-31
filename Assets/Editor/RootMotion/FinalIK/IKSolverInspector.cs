using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/*
	 * Contains helper methods for managing IKSolver's fields.
	 * */
	public class IKSolverInspector: Inspector {

		public static float GetHandleSize(Vector3 position) {
			float s = HandleUtility.GetHandleSize(position) * 0.1f;
			return Mathf.Lerp(s, 0.025f, 0.2f);
		}

	}
}
