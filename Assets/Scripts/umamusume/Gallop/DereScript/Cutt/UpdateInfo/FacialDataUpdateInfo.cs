using UnityEngine;

namespace Cutt
{
    public struct FacialDataUpdateInfo
    {
        public LiveTimelineKeyFacialFaceData face;

        public LiveTimelineKeyFacialMouthData mouth;

        public LiveTimelineKeyFacialEyeData eyeCur;

        public LiveTimelineKeyFacialEyeData eyeNext;

        public LiveTimelineKeyFacialEyeTrackData eyeTrack;

        public Vector3 eyeTrackOffset;

        public int faceKeyIndex;

        public int mouthKeyIndex;

        public int eyeKeyIndex;

        public int eyeTrackKeyIndex;
    }
}
