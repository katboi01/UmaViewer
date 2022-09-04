using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyTextureAnimationData : LiveTimelineKeyWithInterpolate
    {
        public string textureName;
        public bool textureNameEmpty;
        public Vector2 offset;
        public Vector2 tiling;
        public Vector2 scrollSpeed;
        public float scrollInterval;
        public int textureID;
    }


    [System.Serializable]
    public class LiveTimelineKeyTextureAnimationDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyTextureAnimationData>
    {

    }

    [System.Serializable]
    public class LiveTimelineTextureAnimationData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyTextureAnimationDataList keys;
    }
}
