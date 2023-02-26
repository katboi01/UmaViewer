using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineOther4FacialData : ILiveTimelineSetData
    {
        public LiveTimelineKeyFacialFaceDataList faceKeys = new LiveTimelineKeyFacialFaceDataList();

        public LiveTimelineKeyFacialFaceMouthList mouthKeys = new LiveTimelineKeyFacialFaceMouthList();

        public LiveTimelineKeyFacialEyeDataList eyeKeys = new LiveTimelineKeyFacialEyeDataList();

        public LiveTimelineKeyFacialEyeTrackDataList eyeTrackKeys = new LiveTimelineKeyFacialEyeTrackDataList();

        public ILiveTimelineKeyDataList GetKeyList(int index)
        {
            return GetKeyListArray()[index];
        }

        public ILiveTimelineKeyDataList[] GetKeyListArray()
        {
            return new ILiveTimelineKeyDataList[4]
            {
                faceKeys,
                mouthKeys,
                eyeKeys,
                eyeTrackKeys
            };
        }

        public LiveTimelineKeyDataType[] GetKeyTypeArray()
        {
            return new LiveTimelineKeyDataType[4]
            {
                LiveTimelineKeyDataType.FacialFace,
                LiveTimelineKeyDataType.FacialMouth,
                LiveTimelineKeyDataType.FacialEye,
                LiveTimelineKeyDataType.FacialEyeTrack
            };
        }
    }
}
