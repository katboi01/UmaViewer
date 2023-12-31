using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyMultiCameraPositionData : LiveTimelineKeyCameraPositionData
    {
        public enum MaskType
        {
            Single = 0,
            All = 1,
            Up = 2,
            Down = 3,
            Left = 4,
            Right = 5,
            LeftUp = 6,
            RightUp = 7,
            LeftDown = 8,
            RightDown = 9
        }

        public bool enableMultiCamera;
        public float fadeTime;
        public float lineThickness;
        public MultiCameraComposite.DivideLineType LineType;
        public Color LineColor;
        public float LineAntialiasing;
        public LiveTimelineKeyMultiCameraPositionData.MaskType maskType;
        public bool updateMainCamera;
        public float roll;
        public float fov;
        public float maskRoll;
        public Vector2 maskOffset;


    }

    [System.Serializable]
    public class LiveTimelineKeyMultiCameraPositionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMultiCameraPositionData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMultiCameraPositionData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "MultiCameraPos";
        public LiveTimelineKeyMultiCameraPositionDataList keys;
    }
}