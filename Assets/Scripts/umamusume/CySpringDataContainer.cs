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
			Dictionary<string, DynamicBoneColliderBase> DynamicBoneColliders = new Dictionary<string, DynamicBoneColliderBase>();
		    gameObjects.AddRange(transform.parent.GetComponentsInChildren<Transform>());
			foreach (CySpringCollisionData collider in collisionParam)
			{
				if (collider._isInner) continue;

				var bone = gameObjects.Find(a => { return a.name == collider._targetObjectName; });
				if (bone)
				{

					var child = new GameObject(collider._collisionName);
					child.transform.SetParent(bone);
					child.transform.localPosition = Vector3.zero;
					child.transform.localRotation = Quaternion.identity;
					child.transform.localScale = Vector3.one;

					switch (collider._type){
						case CySpringCollisionData.CollisionType.Capsule:
						case CySpringCollisionData.CollisionType.Sphere:
							var dynamic = child.AddComponent<DynamicBoneCollider>();
							dynamic.ColliderName = collider._collisionName;
							dynamic.m_Center = collider._offset;
							dynamic.m_Radius = collider._radius;
							dynamic.m_Height = collider._distance;
							dynamic.m_Bound = collider._isInner ? DynamicBoneColliderBase.Bound.Inside : DynamicBoneColliderBase.Bound.Outside;
							DynamicBoneColliders.Add(collider._collisionName,dynamic);
							break;
						case CySpringCollisionData.CollisionType.Plane:
							var planedynamic = child.AddComponent<DynamicBonePlaneCollider>();
							planedynamic.ColliderName = collider._collisionName;
							planedynamic.m_Center = collider._offset*0.1f;
							planedynamic.m_Bound = collider._isInner ? DynamicBoneColliderBase.Bound.Inside : DynamicBoneColliderBase.Bound.Outside;
							DynamicBoneColliders.Add(collider._collisionName, planedynamic);
							break;
						case CySpringCollisionData.CollisionType.None:
							break;
					}
				}
			}


			foreach (CySpringParamDataElement spring in springParam)
            {
				var bone = gameObjects.Find(a => { return a.name == spring._boneName; });
                if (bone)
                {
					var dynamic = bone.gameObject.AddComponent<DynamicBone>();
					dynamic.m_Root = bone;
					dynamic.m_Gravity = new Vector3(0, -spring._gravity/70000, 0);
					dynamic.m_Stiffness = spring._stiffnessForce/7000;
					dynamic.m_Elasticity = spring._dragForce/9000;
					dynamic.m_Radius = spring._collisionRadius;
					dynamic.m_Colliders = new List<DynamicBoneColliderBase>();
					foreach (string collisionName in spring._collisionNameList)
                    {
						if(DynamicBoneColliders.TryGetValue(collisionName,out DynamicBoneColliderBase val))
                        {
							if (!dynamic.m_Colliders.Contains(val))
								dynamic.m_Colliders.Add(val);
						}
                    }
					foreach (CySpringParamDataChildElement Childcollision in spring._childElements)
					{
                        foreach (string collisionName in Childcollision._collisionNameList)
                        {
							if (DynamicBoneColliders.TryGetValue(collisionName, out DynamicBoneColliderBase val))
							{
								if(!dynamic.m_Colliders.Contains(val))
									dynamic.m_Colliders.Add(val);
							}
						}
					}
				}
            }
        }
	}
}
