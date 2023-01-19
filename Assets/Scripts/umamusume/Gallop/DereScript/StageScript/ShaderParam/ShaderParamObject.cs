using System;
using System.Collections.Generic;

namespace ShaderParam
{
    [Serializable]
    public class ShaderParamObject
    {
        public virtual void Start()
        {
        }

        public virtual void Update()
        {
        }

        public static void StartArray(ShaderParamObject[] _shaderParams)
        {
            if (_shaderParams != null)
            {
                for (int i = 0; i < _shaderParams.Length; i++)
                {
                    _shaderParams[i].Start();
                }
            }
        }

        public static void UpdateArray(ShaderParamObject[] _shaderParams)
        {
            if (_shaderParams != null)
            {
                for (int i = 0; i < _shaderParams.Length; i++)
                {
                    _shaderParams[i].Update();
                }
            }
        }

        public static void AddToArray<T>(ref T[] arrayData, T addData)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < arrayData.Length; i++)
            {
                list.Add(arrayData[i]);
            }
            list.Add(addData);
            arrayData = list.ToArray();
        }
    }
}
