using System;
using UnityEngine;

namespace ShaderParam
{
    [Serializable]
    public class ShaderParamVector4 : ShaderParamBase<Vector4>
    {
        private ShaderParamVector4()
        {
            setGlobalParamFromID = Shader.SetGlobalVector;
            setGlobalParamFromName = Shader.SetGlobalVector;
        }

        public ShaderParamVector4(string name)
        {
            Name = name;
            setGlobalParamFromID = Shader.SetGlobalVector;
            setGlobalParamFromName = Shader.SetGlobalVector;
        }

        public override void Update()
        {
            if (!StartOnly && OldParam != Param)
            {
                setGlobalParamFromID(PropertyID, Param);
                OldParam = Param;
            }
        }
    }
}
