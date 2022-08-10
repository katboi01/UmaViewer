using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	public class AnimationBlocker : MonoBehaviour {

        void LateUpdate()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
	}
}
