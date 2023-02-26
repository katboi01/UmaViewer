using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineLensFlareSetting
    {
        [Serializable]
        public class FlareDataGroup
        {
            public int objectId;

            public bool isHqOnly;
        }

        public FlareDataGroup[] flareDataGroup = new FlareDataGroup[0];

        public int flareDataGroupCount;

        public float lightStandard = 3f;

        public float underLimit;
    }
}
