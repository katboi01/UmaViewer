using UnityEngine;

namespace ShaderParam
{
    public class ShaderParamController : MonoBehaviour
    {
        public ShaderParamVector4[] m_ShaderParamVector;

        public ShaderParamFloat[] m_ShaderParamFloat;

        public ShaderParamInt[] m_ShaderParamInt;

        public ShaderParamFloatArray[] m_ShaderParamFloatArray;

        public ShaderParamIntArray[] m_ShaderParamIntArray;

        private void Start()
        {
            ShaderParamObject[] shaderParamVector = m_ShaderParamVector;
            ShaderParamObject.StartArray(shaderParamVector);
            shaderParamVector = m_ShaderParamFloat;
            ShaderParamObject.StartArray(shaderParamVector);
            shaderParamVector = m_ShaderParamInt;
            ShaderParamObject.StartArray(shaderParamVector);
            shaderParamVector = m_ShaderParamFloatArray;
            ShaderParamObject.StartArray(shaderParamVector);
            shaderParamVector = m_ShaderParamIntArray;
            ShaderParamObject.StartArray(shaderParamVector);
        }

        private void Update()
        {
            ShaderParamObject[] shaderParamVector = m_ShaderParamVector;
            ShaderParamObject.UpdateArray(shaderParamVector);
            shaderParamVector = m_ShaderParamFloat;
            ShaderParamObject.UpdateArray(shaderParamVector);
            shaderParamVector = m_ShaderParamInt;
            ShaderParamObject.UpdateArray(shaderParamVector);
            shaderParamVector = m_ShaderParamFloatArray;
            ShaderParamObject.UpdateArray(shaderParamVector);
            shaderParamVector = m_ShaderParamIntArray;
            ShaderParamObject.UpdateArray(shaderParamVector);
        }

        public void FindShaderParam(out ShaderParamVector4 param, string name)
        {
            param = null;
            if (m_ShaderParamVector == null)
            {
                return;
            }
            for (int i = 0; i < m_ShaderParamVector.Length; i++)
            {
                if (m_ShaderParamVector[i].Name == name)
                {
                    param = m_ShaderParamVector[i];
                }
            }
        }

        public void FindShaderParam(out ShaderParamFloat param, string name)
        {
            param = null;
            if (m_ShaderParamFloat == null)
            {
                return;
            }
            for (int i = 0; i < m_ShaderParamFloat.Length; i++)
            {
                if (m_ShaderParamFloat[i].Name == name)
                {
                    param = m_ShaderParamFloat[i];
                }
            }
        }

        public void AddShaderParam(ShaderParamFloat param)
        {
            if (param != null)
            {
                ShaderParamObject.AddToArray(ref m_ShaderParamFloat, param);
            }
        }

        public void AddShaderParam(ShaderParamVector4 param)
        {
            if (param != null)
            {
                ShaderParamObject.AddToArray(ref m_ShaderParamVector, param);
            }
        }
    }
}
