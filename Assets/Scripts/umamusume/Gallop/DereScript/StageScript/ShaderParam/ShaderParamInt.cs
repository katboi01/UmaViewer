using System;
using UnityEngine;

namespace ShaderParam
{
    [Serializable]
    public class ShaderParamInt : ShaderParamBase<int>
    {
        private ShaderParamInt()
        {
            setGlobalParamFromID = Shader.SetGlobalInt;
            setGlobalParamFromName = Shader.SetGlobalInt;
        }

        public ShaderParamInt(string name)
        {
            Name = name;
            setGlobalParamFromID = Shader.SetGlobalInt;
            setGlobalParamFromName = Shader.SetGlobalInt;
        }
    }
}
