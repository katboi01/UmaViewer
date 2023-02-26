using System;
using UnityEngine;

public class PropertyID<T>
{
    private int[] _id;

    public int this[int idx]
    {
        get
        {
            return this._id[idx];
        }
        set
        {
            this._id[idx] = value;
        }
    }

    public PropertyID()
    {
        Type typeFromHandle = typeof(T);
        if (!typeFromHandle.IsEnum)
        {
            throw new ArgumentException("Not enum type");
        }
        string[] names = Enum.GetNames(typeFromHandle);
        this._id = new int[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            this._id[i] = Shader.PropertyToID(names[i]);
        }
    }
}
