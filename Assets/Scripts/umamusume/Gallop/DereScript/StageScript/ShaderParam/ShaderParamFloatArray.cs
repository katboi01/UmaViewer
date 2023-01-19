using System;
using UnityEngine;

namespace ShaderParam
{
    [Serializable]
    public class ShaderParamFloatArray : ShaderParamArrayBase<float>
    {
        private ShaderParamFloatArray()
        {
            setGlobalParamFromID = Shader.SetGlobalFloat;
            setGlobalParamFromName = Shader.SetGlobalFloat;
        }

        public ShaderParamFloatArray(string name)
        {
            Name = name;
            setGlobalParamFromID = Shader.SetGlobalFloat;
            setGlobalParamFromName = Shader.SetGlobalFloat;
        }
    }
}
