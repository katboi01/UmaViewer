using UnityEngine;
using Stage;

public class CharacterFlareCollision
{
    private delegate void AddComonentDelegate(GameObject targetObject, CharacterFlareCollisionParameter.BoneCollisionParameter.CollisionParameter param);

    public static bool AddCollisionToObject(GameObject rootObject, CharacterFlareCollisionParameter param)
    {
        return _AddCollisionToObject(rootObject, param, delegate (GameObject targetObject, CharacterFlareCollisionParameter.BoneCollisionParameter.CollisionParameter collisionParam)
        {
            AddComponentFromCollisionParameter(targetObject, collisionParam);
        });
    }

    private static void AddComponentFromCollisionParameter(GameObject targetObject, CharacterFlareCollisionParameter.BoneCollisionParameter.CollisionParameter param)
    {
        switch (param._type)
        {
            case CharacterFlareCollisionParameter.BoneCollisionParameter.CollisionParameter.Type.Box:
                {
                    BoxCollider boxCollider = targetObject.AddComponent<BoxCollider>();
                    boxCollider.center = param._center;
                    boxCollider.size = param._size;
                    break;
                }
            case CharacterFlareCollisionParameter.BoneCollisionParameter.CollisionParameter.Type.Capsule:
                {
                    CapsuleCollider capsuleCollider = targetObject.AddComponent<CapsuleCollider>();
                    capsuleCollider.center = param._center;
                    capsuleCollider.radius = param._radius;
                    capsuleCollider.height = param._height;
                    capsuleCollider.direction = (int)param._direction;
                    break;
                }
            case CharacterFlareCollisionParameter.BoneCollisionParameter.CollisionParameter.Type.Sphere:
                {
                    SphereCollider sphereCollider = targetObject.AddComponent<SphereCollider>();
                    sphereCollider.center = param._center;
                    sphereCollider.radius = param._radius;
                    break;
                }
        }
    }

    private static bool _AddCollisionToObject(GameObject rootObject, CharacterFlareCollisionParameter param, AddComonentDelegate callback)
    {
        if (param == null)
        {
            return false;
        }
        if (param.boneCollisionParameters == null)
        {
            return false;
        }
        CharacterFlareCollisionParameter.BoneCollisionParameter[] boneCollisionParameters = param.boneCollisionParameters;
        Transform transform = rootObject.transform;
        foreach (CharacterFlareCollisionParameter.BoneCollisionParameter boneCollisionParameter in boneCollisionParameters)
        {
            if (boneCollisionParameter._collisions == null)
            {
                continue;
            }
            Transform transform2 = GameObjectUtility.FindChild(boneCollisionParameter._targetBoneName, transform);
            if (!(transform2 == null))
            {
                CharacterFlareCollisionParameter.BoneCollisionParameter.CollisionParameter[] collisions = boneCollisionParameter._collisions;
                GameObject gameObject = transform2.gameObject;
                for (int j = 0; j < collisions.Length; j++)
                {
                    callback(gameObject, collisions[j]);
                }
            }
        }
        return true;
    }
}
