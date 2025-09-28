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
        public bool UseCorrectScaleCalc;

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


                    //修改(动骨与碰撞相关)
                    if (collider._collisionName == "Col_B_Hip_Tail")
                    {
                        collider._radius *= 0.96f;
                    }
                    else if (collider._collisionName == "Col_B_Chest_Tail")
                    {
                        collider._radius *= 1.14f;
                    }
                    else if (collider._collisionName.Contains("Col_B_Hip_Skirt"))
                    {
                        collider._radius *= 0.8f;
                    }
                    else if (collider._collisionName.Contains("Col_B_Hip_Jacket"))
                    {
                        collider._radius *= 1f;
                    }
                    else if (collider._collisionName == "Col_Elbow_R_Hair")
                    {
                        collider._radius *= 0.3f;
                    }
                    else if (collider._collisionName == "Col_Elbow_L_Hair")
                    {
                        collider._radius *= 0.3f;
                    }
                    else
                    {
                        collider._radius *= 0.9f;
                    }



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



        //修改(动骨与碰撞相关)
        public void InitializePhysics(Dictionary<string, Transform> bones, Dictionary<string, Transform> colliders)
        {
            DynamicBones.Clear();
        
            // 获取对象名称并判断类型
            string nameLower = gameObject.name.ToLower();
            bool isTailObject = nameLower.Contains("tail"); // 判断是否为尾巴
            bool isClothObject = !isTailObject && nameLower.Contains("chr"); // 判断是否为头发，排除尾巴避免重复匹配
        
            foreach (CySpringParamDataElement spring in springParam)
            {
                if (!bones.TryGetValue(spring._boneName, out Transform bone)) continue;
        
                var dynamic = bone.gameObject.AddComponent<DynamicBone>();
                dynamic.m_Root = bone;
        
                // 设置重力
                dynamic.m_Gravity = new Vector3(0, Mathf.Clamp01(-30f / spring._gravity), 0);
                dynamic.m_LimitAngel_Min = spring._limitAngleMin;
                dynamic.m_LimitAngel_Max = spring._limitAngleMax;
        
                // 主参数设置
                if (isTailObject)
                {
                    // 尾巴主骨骼
                    dynamic.m_Damping = 0.05f;
                    dynamic.m_Stiffness = Mathf.Clamp01(20f / spring._stiffnessForce);
                    dynamic.m_Elasticity = Mathf.Clamp01(20f / spring._dragForce);
                    dynamic.m_Radius = spring._collisionRadius * 0.9f;
                    dynamic.m_Inert = spring.MoveSpringApplyRate / 4f;
                }
                else if (isClothObject)
                {
                    // 头发主骨骼
                    dynamic.m_Damping = 0.1f;
                    dynamic.m_Stiffness = Mathf.Clamp01(15f / spring._stiffnessForce);
                    dynamic.m_Elasticity = Mathf.Clamp01(20f / spring._dragForce);
                    dynamic.m_Radius = spring._collisionRadius * 0.9f;
                    dynamic.m_Inert = spring.MoveSpringApplyRate / 2.5f;
                }
                else
                {
                    // 衣服主骨骼
                    dynamic.m_Damping = 0.1f;
                    dynamic.m_Stiffness = Mathf.Clamp01(20f / spring._stiffnessForce);
                    dynamic.m_Elasticity = Mathf.Clamp01(30f / spring._dragForce);
                    dynamic.m_Radius = spring._collisionRadius * 0.8f;
                    dynamic.m_Inert = spring.MoveSpringApplyRate / 3f;
                }
        
                dynamic.SetupParticles();
                DynamicBones.Add(dynamic);
        
                // 主碰撞器添加
                foreach (string collisionName in spring._collisionNameList)
                {
                    if (colliders.TryGetValue(collisionName, out Transform tmp))
                    {
                        dynamic.Particles[0].m_Colliders.Add(tmp.GetComponent<DynamicBoneColliderBase>());
                    }
                }
        
                // 子参数设置
                foreach (var child in spring._childElements)
                {
                    var tempParticle = dynamic.Particles.Find(p => p.m_Transform.gameObject.name == child._boneName);
                    if (tempParticle == null) continue;
        
                    if (isTailObject)
                    {
                        // 尾巴子粒子
                        tempParticle.m_Damping = 0.05f;
                        tempParticle.m_Stiffness = Mathf.Clamp01(20f / child._stiffnessForce);
                        tempParticle.m_Elasticity = Mathf.Clamp01(20f / child._dragForce);
                        tempParticle.m_Radius = child._collisionRadius * 1f;
                        tempParticle.m_Inert = child.MoveSpringApplyRate / 4f;
                    }
                    else if (isClothObject)
                    {
                        // 头发子粒子
                        tempParticle.m_Damping = 0.1f;
                        tempParticle.m_Stiffness = Mathf.Clamp01(15f / child._stiffnessForce);
                        tempParticle.m_Elasticity = Mathf.Clamp01(20f / child._dragForce);
                        tempParticle.m_Radius = child._collisionRadius * 1f;
                        tempParticle.m_Inert = child.MoveSpringApplyRate / 2.5f;
                    }
                    else
                    {
                        // 衣服子粒子
                        tempParticle.m_Damping = 0.1f;
                        tempParticle.m_Stiffness = Mathf.Clamp01(20f / child._stiffnessForce);
                        tempParticle.m_Elasticity = Mathf.Clamp01(30f / child._dragForce);
                        tempParticle.m_Radius = child._collisionRadius * 0.8f;
                        tempParticle.m_Inert = child.MoveSpringApplyRate / 2f;
                    }
        
                    tempParticle.m_LimitAngel_Min = child._limitAngleMin;
                    tempParticle.m_LimitAngel_Max = child._limitAngleMax;
        
                    // 子碰撞器添加
                    foreach (string collisionName in child._collisionNameList)
                    {
                        if (colliders.TryGetValue(collisionName, out Transform tmp))
                        {
                            var collider = tmp.GetComponent<DynamicBoneColliderBase>();
                            if (collider != null)
                            {
                                tempParticle.m_Colliders.Add(collider);
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
