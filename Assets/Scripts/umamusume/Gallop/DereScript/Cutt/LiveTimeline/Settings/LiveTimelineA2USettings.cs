using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineA2USettings
    {
        [Serializable]
        public class Prefab
        {
            public string name = "";

            public string path = "";
        }

        public int flickRandomSeed = 123456789;

        public uint flickCount = 40u;

        public float flickStepSec = 0.06f;

        public uint flickMin = 50u;

        public uint flickMax = 53u;

        public Prefab[] prefabs = new Prefab[0];

        public string[] sprites = new string[0];

        public int assetNo = 1;
    }
}
