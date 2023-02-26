using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCameraLayerData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 layerOffset = Vector3.zero;

        public bool extraCamera;

        public Vector3 extraCameraLayerOffset = Vector3.zero;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CameraLayer;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }
    }
}

