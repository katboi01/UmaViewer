using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineStageObjectsSettings
    {
        [Serializable]
        public class Object
        {
            public int replaceQuality;

            public int objectId;

            public string objectName = string.Empty;
        }

        public int objectCount;

        public Object[] objectData = new Object[0];
    }
}
