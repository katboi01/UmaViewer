using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineIndirectLightShuftSettings
    {
        public enum ShaftType
        {
            Default,
            Vr03
        }

        public int id = 1;

        public ShaftType shaftType;

        public int shaftTextureNameLength;

        public string[] shaftTextureNames = new string[0];
    }
}
