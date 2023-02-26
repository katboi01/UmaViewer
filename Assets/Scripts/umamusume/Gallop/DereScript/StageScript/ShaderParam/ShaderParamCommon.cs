using System;

namespace ShaderParam
{
    [Serializable]
    public class ShaderParamCommon<T> : ShaderParamObject
    {
        public delegate void SetGlobalParamFromID(int id, T param);

        public delegate void SetGlobalParamFromName(string name, T param);

        public bool StartOnly;

        public string Name;

        public string Comment;

        public SetGlobalParamFromID setGlobalParamFromID;

        public SetGlobalParamFromName setGlobalParamFromName;
    }
}
