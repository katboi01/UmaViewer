using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop
{
    public class CySpringDataContainer:MonoBehaviour
    {
		public List<CySpringCollisionData> collisionParam;
		public List<CySpringParamDataElement> springParam;
		public List<ConnectedBoneData> ConnectedBoneList;
		public bool enableVerticalWind;
		public bool enableHorizontalWind;
		public float centerWindAngleSlow;
		public float centerWindAngleFast;
		public float verticalCycleSlow;
		public float horizontalCycleSlow;
		public float verticalAngleWidthSlow;
		public float horizontalAngleWidthSlow;
		public float verticalCycleFast;
		public float horizontalCycleFast;
		public float verticalAngleWidthFast;
		public float horizontalAngleWidthFast;
		public bool IsEnableHipMoveParam;
		public float HipMoveInfluenceDistance;
		public float HipMoveInfluenceMaxDistance;

        private void Start()
        {
			SetPhysics();
		}
        public void SetPhysics()
        {
			List<Transform> gameObjects = new List<Transform>();
			gameObjects.AddRange(transform.parent.GetComponentsInChildren<Transform>());

			foreach (CySpringParamDataElement spring in springParam)
            {
				var bone = gameObjects.Find(a => { return a.name == spring._boneName; });
                if (bone)
                {
					var dynamic = bone.gameObject.AddComponent<DynamicBone>();
					dynamic.m_Root = bone;
				}
            }
        }
	}
}
