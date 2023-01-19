using System;
using UnityEngine;

namespace ShaderParam
{
    [Serializable]
    public class ShaderParamBase<T> : ShaderParamCommon<T>
    {
        public T Param;

        [HideInInspector]
        public int PropertyID;

        [HideInInspector]
        public T OldParam;

        public override void Start()
        {
            if (StartOnly)
            {
                setGlobalParamFromName(Name, Param);
                return;
            }
            PropertyID = Shader.PropertyToID(Name);
            setGlobalParamFromID(PropertyID, Param);
            OldParam = Param;
        }

        public override void Update()
        {
            if (!StartOnly && !OldParam.Equals(Param))
            {
                setGlobalParamFromID(PropertyID, Param);
                OldParam = Param;
            }
        }
    }
}