using System;
using UnityEngine;

namespace ShaderParam
{
    [Serializable]
    public class ShaderParamIntArray : ShaderParamArrayBase<int>
    {
        private ShaderParamIntArray()
        {
            setGlobalParamFromID = Shader.SetGlobalInt;
            setGlobalParamFromName = Shader.SetGlobalInt;
        }

        public ShaderParamIntArray(string name)
        {
            Name = name;
            setGlobalParamFromID = Shader.SetGlobalInt;
            setGlobalParamFromName = Shader.SetGlobalInt;
        }
    }
}
