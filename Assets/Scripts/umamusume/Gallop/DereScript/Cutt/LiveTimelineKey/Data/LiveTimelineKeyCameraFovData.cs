using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCameraFovData : LiveTimelineKeyWithInterpolate
    {
        public LiveCameraFovType fovType;

        public float fov = 30f;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CameraFov;
    }
}

