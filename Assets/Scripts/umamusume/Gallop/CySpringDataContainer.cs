using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop
{
    public class CySpringDataContainer : MonoBehaviour
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

        public List<DynamicBone> DynamicBones = new List<DynamicBone>();

        public Dictionary<string, Transform> InitiallizeCollider(Dictionary<string, Transform> bones)
        {
            var colliders = new Dictionary<string, Transform>();
            foreach (CySpringCollisionData collider in collisionParam)
            {
                if (collider._isInner) continue;
                if (bones.TryGetValue(collider._targetObjectName, out Transform bone))
                {

                    var child = new GameObject(collider._collisionName);
                    child.transform.SetParent(bone);
                    child.transform.localPosition = Vector3.zero;
                    child.transform.localRotation = Quaternion.identity;
                    child.transform.localScale = Vector3.one;
                    colliders.Add(child.name, child.transform);

                    switch (collider._type)
                    {
                        case CySpringCollisionData.CollisionType.Capsule:
                            var dynamic = child.AddComponent<DynamicBoneCollider>();
                            dynamic.ColliderName = collider._collisionName;
                            child.transform.localPosition = (collider._offset + collider._offset2) / 2;
                            child.transform.LookAt(child.transform.TransformPoint(collider._offset2));
                            dynamic.m_Direction = DynamicBoneColliderBase.Direction.Z;
                            dynamic.m_Height = (collider._offset - collider._offset2).magnitude + collider._radius;
                            dynamic.m_Radius = collider._radius;
                            dynamic.m_Bound = collider._isInner ? DynamicBoneColliderBase.Bound.Inside : DynamicBoneColliderBase.Bound.Outside;
                            break;
                        case CySpringCollisionData.CollisionType.Sphere:
                            var Spheredynamic = child.AddComponent<DynamicBoneCollider>();
                            Spheredynamic.ColliderName = collider._collisionName;
                            child.transform.localPosition = collider._offset;
                            Spheredynamic.m_Radius = collider._radius;
                            Spheredynamic.m_Height = collider._distance;
                            Spheredynamic.m_Bound = collider._isInner ? DynamicBoneColliderBase.Bound.Inside : DynamicBoneColliderBase.Bound.Outside;
                            break;
                        case CySpringCollisionData.CollisionType.Plane:
                            var planedynamic = child.AddComponent<DynamicBonePlaneCollider>();
                            planedynamic.ColliderName = collider._collisionName;
                            child.transform.localPosition = collider._offset;
                            planedynamic.m_Bound = collider._isInner ? DynamicBoneColliderBase.Bound.Inside : DynamicBoneColliderBase.Bound.Outside;
                            break;
                        case CySpringCollisionData.CollisionType.None:
                            break;
                    }
                }
            }
            return colliders;
        }

        public void InitializePhysics(Dictionary<string, Transform> bones, Dictionary<string, Transform> colliders)
        {
            DynamicBones.Clear();
            foreach (CySpringParamDataElement spring in springParam)
            {
                if (bones.TryGetValue(spring._boneName,out Transform bone))
                {
                    bool isTail = gameObject.name.Contains("tail");//The tail needs less traction
                    var dynamic = bone.gameObject.AddComponent<DynamicBone>();
                    dynamic.m_Root = bone;
                    dynamic.m_Gravity = new Vector3(0, Mathf.Clamp01(-30 / spring._gravity), 0);
                    dynamic.m_LimitAngel_Min = spring._limitAngleMin;
                    dynamic.m_LimitAngel_Max = spring._limitAngleMax;
                    if (isTail)
                    {
                        dynamic.m_Damping = 0.1f;
                        dynamic.m_Stiffness = Mathf.Clamp01(10 / spring._stiffnessForce);
                        dynamic.m_Elasticity = Mathf.Clamp01(10 / spring._dragForce);
                    }
                    else
                    {
                        dynamic.m_Damping = 0.2f;
                        dynamic.m_Stiffness = Mathf.Clamp01(45 / spring._stiffnessForce);
                        dynamic.m_Elasticity = Mathf.Clamp01(45 / spring._dragForce);

                    }
                    dynamic.m_Radius = spring._collisionRadius;
                    dynamic.m_Inert = spring.MoveSpringApplyRate / 3;
                    dynamic.SetupParticles();
                    DynamicBones.Add(dynamic);

                    foreach (string collisionName in spring._collisionNameList)
                    {
                        if (colliders.TryGetValue(collisionName,out Transform tmp))
                        {
                            dynamic.Particles[0].m_Colliders.Add(tmp.GetComponent<DynamicBoneColliderBase>());
                        }
                    }

                    foreach (CySpringParamDataChildElement Childcollision in spring._childElements)
                    {
                        var tempParticle = dynamic.Particles.Find(a => { return a.m_Transform.gameObject.name == Childcollision._boneName; });
                        if (tempParticle != null)
                        {
                            if (isTail)
                            {
                                tempParticle.m_Damping = 0.1f;
                                tempParticle.m_Stiffness = Mathf.Clamp01(10 / Childcollision._stiffnessForce);
                                tempParticle.m_Elasticity = Mathf.Clamp01(10 / Childcollision._dragForce);
                            }
                            else
                            {
                                tempParticle.m_Damping = 0.2f;
                                tempParticle.m_Stiffness = Mathf.Clamp01(45 / Childcollision._stiffnessForce);
                                tempParticle.m_Elasticity = Mathf.Clamp01(45 / Childcollision._dragForce);
                            }
                            tempParticle.m_Inert = Childcollision.MoveSpringApplyRate / 3;
                            tempParticle.m_Radius = Childcollision._collisionRadius;
                            tempParticle.m_LimitAngel_Min = Childcollision._limitAngleMin;
                            tempParticle.m_LimitAngel_Max = Childcollision._limitAngleMax;
                            foreach (string collisionName in Childcollision._collisionNameList)
                            {
                                if (colliders.TryGetValue(collisionName,out Transform tmp))
                                {
                                    tempParticle.m_Colliders.Add(tmp.GetComponent<DynamicBoneColliderBase>());
                                }
                            }
                        }
                    }
                }
            }
        }

        public void EnablePhysics(bool isOn)
        {
            foreach(DynamicBone dynamic in DynamicBones)
            {
                dynamic.enabled = isOn;
            }
        }

        public void ResetPhysics()
        {
            foreach (DynamicBone dynamic in DynamicBones)
            {
                dynamic.ResetParticlesPosition();
            }
        }
    }
}
