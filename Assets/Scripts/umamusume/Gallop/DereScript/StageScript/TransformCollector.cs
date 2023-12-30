// TransformCollector
using Cutt;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// トランスフォームを一括で管理する
/// </summary>
public class TransformCollector
{
    private Transform _parentTransform;

    private Dictionary<int, Transform> _mapTransform = new Dictionary<int, Transform>();

    private Dictionary<Type, Transform[]> _catTransform = new Dictionary<Type, Transform[]>();

    public TransformCollector(Transform parent)
    {
        _parentTransform = parent;
    }

    public void Reset(Transform parent)
    {
        _parentTransform = parent;
        _mapTransform.Clear();
        _catTransform.Clear();
    }

    public bool CheckBuilded<T>()
    {
        return _catTransform.ContainsKey(typeof(T));
    }

    public void Collect()
    {
        Transform[] componentsInChildren = _parentTransform.GetComponentsInChildren<Transform>();
        foreach (Transform transform in componentsInChildren)
        {
            int key = FNVHash.Generate(transform.name.ToLower());
            if (!_mapTransform.ContainsKey(key))
            {
                _mapTransform.Add(key, transform);
            }
        }
    }

    public Transform[] Build<T>(bool allowNullPtr = true) where T : IConvertible
    {
        Type typeFromHandle = typeof(T);
        string[] names = Enum.GetNames(typeFromHandle);
        int num = names.Length;
        for (int i = 0; i < num; i++)
        {
            names[i] = names[i].ToLower();
        }
        Transform[] array = new Transform[num];
        for (int j = 0; j < num; j++)
        {
            Transform value = null;
            int key = FNVHash.Generate(names[j]);
            if (!_mapTransform.TryGetValue(key, out value))
            {
                value = FindChildTransformIgnoreCase(names[j], _parentTransform);
                _mapTransform.Add(key, value);
            }
            array[j] = value;
        }
        _catTransform.Add(typeFromHandle, array);
        return array;
    }

    public static Transform FindChildTransformIgnoreCase(string lowerName, Transform target)
    {
        Transform transform = null;
        int childCount = target.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = target.GetChild(i);
            if (string.Compare(child.name.ToLower(), lowerName) == 0)
            {
                transform = child;
                break;
            }
        }
        if (transform == null)
        {
            for (int j = 0; j < target.childCount; j++)
            {
                transform = FindChildTransformIgnoreCase(lowerName, target.GetChild(j));
                if (transform != null)
                {
                    return transform;
                }
            }
        }
        return transform;
    }

    public Transform Find(string strName)
    {
        string seed = strName.ToLower();
        return Find(FNVHash.Generate(seed));
    }

    public Transform Find(int hashKey)
    {
        Transform value = null;
        _mapTransform.TryGetValue(hashKey, out value);
        return value;
    }

    public Transform Find<T>(int idx) where T : IConvertible
    {
        Transform result = null;
        Transform[] value = null;
        if (_catTransform.TryGetValue(typeof(T), out value) && 0 <= idx && idx < value.Length)
        {
            result = value[idx];
        }
        return result;
    }

    public Transform[] GetTransformCategory<T>() where T : IConvertible
    {
        Transform[] value = null;
        _catTransform.TryGetValue(typeof(T), out value);
        return value;
    }

    public void SetTransform<T>(Transform transform, int idx, bool overlapped = true) where T : IConvertible
    {
        Transform[] value = null;
        if (_catTransform.TryGetValue(typeof(T), out value) && 0 <= idx && idx < value.Length)
        {
            if (overlapped)
            {
                value[idx] = transform;
            }
            else if (value[idx] == null)
            {
                value[idx] = transform;
            }
        }
    }
}
