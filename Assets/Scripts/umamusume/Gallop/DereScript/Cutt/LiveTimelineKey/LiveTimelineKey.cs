using System;

namespace Cutt
{
    [Serializable]
    public abstract class LiveTimelineKey
    {
        public int frame;

        public LiveTimelineKeyAttribute attribute;

        public abstract LiveTimelineKeyDataType dataType { get; }

        public virtual bool IsInterpolateKey()
        {
            return false;
        }

        public virtual void OnLoad(LiveTimelineControl timelineControl)
        {
        }
    }
}

