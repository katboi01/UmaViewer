using System;
using UnityEngine;

namespace ShaderParam
{
    [Serializable]
    public class ShaderParamFloat : ShaderParamBase<float>
    {
        private ShaderParamFloat()
        {
            setGlobalParamFromID = Shader.SetGlobalFloat;
            setGlobalParamFromName = Shader.SetGlobalFloat;
        }

        public ShaderParamFloat(string name)
        {
            Name = name;
            setGlobalParamFromID = Shader.SetGlobalFloat;
            setGlobalParamFromName = Shader.SetGlobalFloat;
        }
    }

}
