using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	public class RotateShoulderToTarget : MonoBehaviour {

		public Transform shoulder;
		public Vector3 euler;

		void OnPuppetMasterRead() {
			shoulder.rotation = Quaternion.Euler(euler) * shoulder.rotation;
		}
	}
}
