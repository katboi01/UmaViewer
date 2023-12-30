using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.ShaderParam
{
    public class ShaderParamObject
    {

    }

    public class ShaderParamCommon<T> : ShaderParamObject
    {
        public bool StartOnly;
        public string Name;
        public string Comment;
    }

    public class ShaderParamBase<T> : ShaderParamCommon<T>
    {
        public T Param;
        public int PropertyID;
        public T OldParam;
    }

    public class ShaderParamArrayBase<T> : ShaderParamCommon<T>
    {
        public T[] Params;
        public int[] PropertyIDs;
        public T[] OldParams;
    }
    [System.Serializable]
    public class ShaderParamVector4 : ShaderParamBase<Vector4>
    {

    }
    [System.Serializable]
    public class ShaderParamFloat : ShaderParamBase<float>
    {

    }
    [System.Serializable]
    public class ShaderParamInt : ShaderParamBase<int>
    {

    }
    [System.Serializable]
    public class ShaderParamFloatArray : ShaderParamArrayBase<float>
    {

    }
    [System.Serializable]
    public class ShaderParamIntArray : ShaderParamArrayBase<int>
    {

    }


    public class ShaderParamController : MonoBehaviour
    {
        public List<ShaderParamVector4> m_ShaderParamVector; // 0x18
        public List<ShaderParamFloat> m_ShaderParamFloat; // 0x20
        public List<ShaderParamInt> m_ShaderParamInt; // 0x28
        public List<ShaderParamFloatArray> m_ShaderParamFloatArray; // 0x30
        public List<ShaderParamIntArray> m_ShaderParamIntArray; // 0x38
    }
}

