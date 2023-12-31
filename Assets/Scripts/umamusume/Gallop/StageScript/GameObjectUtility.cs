using System;
using UnityEngine;

public class GameObjectUtility
{
    public static void SetTransformParentInit(GameObject childObj, Transform parent)
    {
        childObj.transform.parent = parent;
        childObj.transform.localPosition = Vector3.zero;
        childObj.transform.localScale = Vector3.one;
        childObj.transform.localRotation = Quaternion.identity;
    }

    public static void SetTransformParent(GameObject childObj, Transform parent)
    {
        Vector3 localPosition = childObj.transform.localPosition;
        Vector3 localScale = childObj.transform.localScale;
        childObj.transform.parent = parent;
        childObj.transform.localRotation = Quaternion.identity;
        childObj.transform.localPosition = new Vector3(localPosition.x, localPosition.y, localPosition.z);
        childObj.transform.localScale = new Vector3(localScale.x, localScale.y, localScale.z);
    }

    public static void SetTransformParent(GameObject childObj, Transform parent, Vector3 position, Vector3 scale)
    {
        childObj.transform.parent = parent;
        childObj.transform.localPosition = new Vector3(position.x, position.y, position.z);
        childObj.transform.localScale = new Vector3(scale.x, scale.y, scale.z);
        childObj.transform.localRotation = Quaternion.identity;
    }

    public static Transform FindChild(string name, Transform target)
    {
        Transform transform = target.Find(name);
        if (transform == null)
        {
            for (int i = 0; i < target.childCount; i++)
            {
                transform = GameObjectUtility.FindChild(name, target.GetChild(i));
                if (transform != null)
                {
                    return transform;
                }
            }
        }
        return transform;
    }

    public static void SetLayer(int layer, Transform trans, System.Func<int, GameObject, bool> fnCondition)
    {
        if (trans == null)
        {
            return;
        }
        if (fnCondition(layer, trans.gameObject))
        {
            trans.gameObject.layer = layer;
        }
        for (int i = 0; i < trans.childCount; i++)
        {
            GameObjectUtility.SetLayer(layer, trans.GetChild(i), fnCondition);
        }
    }

    public static void SetLayer(int layer, Transform trans)
    {
        if (trans == null)
        {
            return;
        }
        trans.gameObject.layer = layer;
        for (int i = 0; i < trans.childCount; i++)
        {
            GameObjectUtility.SetLayer(layer, trans.GetChild(i));
        }
    }

    public static GameObject FindGameObjectOfParent(string name, GameObject parentObject)
    {
        GameObject gameObject = GameObject.Find(name);
        if (parentObject != null)
        {
            GameObject gameObject2 = gameObject;
            while (gameObject2 != null)
            {
                Transform transform = gameObject2.transform;
                if (transform.parent != null && transform.gameObject == parentObject)
                {
                    return gameObject;
                }
                if (transform.parent == null)
                {
                    break;
                }
                gameObject2 = transform.parent.gameObject;
            }
            return null;
        }
        return gameObject;
    }

    public static Transform FindChildTransform(string name, Transform target)
    {
        Transform transform = target.Find(name);
        if (transform == null)
        {
            for (int i = 0; i < target.childCount; i++)
            {
                transform = GameObjectUtility.FindChildTransform(name, target.GetChild(i));
                if (transform != null)
                {
                    return transform;
                }
            }
        }
        return transform;
    }

    public static void ResetTransformLocalParam(Transform transform)
    {
        if (transform == null)
        {
            return;
        }
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}
