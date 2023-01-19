using System;
using UnityEngine;

namespace ShaderParam
{
    [Serializable]
    public class ShaderParamArrayBase<T> : ShaderParamCommon<T>
    {
        public T[] Params;

        [HideInInspector]
        public int[] PropertyIDs;

        [HideInInspector]
        public T[] OldParams;

        public override void Start()
        {
            if (Params == null || Params.Length == 0)
            {
                return;
            }
            if (StartOnly)
            {
                for (int i = 0; i < Params.Length; i++)
                {
                    string name = Name + i;
                    setGlobalParamFromName(name, Params[i]);
                }
                return;
            }
            PropertyIDs = new int[Params.Length];
            OldParams = new T[Params.Length];
            for (int i = 0; i < Params.Length; i++)
            {
                string name = Name + i;
                PropertyIDs[i] = Shader.PropertyToID(name);
                setGlobalParamFromID(PropertyIDs[i], Params[i]);
                OldParams[i] = Params[i];
            }
        }

        public override void Update()
        {
            if (Params == null || Params.Length == 0 || StartOnly)
            {
                return;
            }
            for (int i = 0; i < Params.Length; i++)
            {
                if (!OldParams[i].Equals(Params[i]))
                {
                    setGlobalParamFromID(PropertyIDs[i], Params[i]);
                    OldParams[i] = Params[i];
                }
            }
        }
    }

}
