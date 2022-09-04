using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyPropsAttachData : LiveTimelineKeyWithInterpolate
    {
        public string _attachJointName;
        public int _attachJointHash;
        public string _copyPositionJointName;
        public int _copyPositionJointHash;
        public int _settingFlags;
        public int _propsId;
        public Vector3 _offsetPosition;
        public Vector3 OffsetRotate;
        public Vector3 OffsetScale;
    }

    [System.Serializable]
    public class LiveTimelineKeyPropsAttachDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyPropsAttachData>
    {

    }

    [System.Serializable]
    public class LiveTimelinePropsAttachData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyPropsAttachDataList keys;
    }
}
