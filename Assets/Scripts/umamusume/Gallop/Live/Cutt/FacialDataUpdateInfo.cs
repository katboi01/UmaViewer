using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public struct FacialDataUpdateInfo // TypeDefIndex: 29410
    {
        // Fields
        public LiveTimelineKeyFacialFaceData facePrev; // 0x0
        public LiveTimelineKeyFacialFaceData faceCur; // 0x0
        public LiveTimelineKeyFacialFaceData faceNext; // 0x8
        public LiveTimelineKeyFacialMouthData mouthPrev; // 0x10
        public LiveTimelineKeyFacialMouthData mouthCur; // 0x10
        public LiveTimelineKeyFacialMouthData mouthNext; // 0x10
        public LiveTimelineKeyFacialEyeData eyePrev; // 0x18
        public LiveTimelineKeyFacialEyeData eyeCur; // 0x18
        public LiveTimelineKeyFacialEyeData eyeNext; // 0x20
        public LiveTimelineKeyFacialEyebrowData eyebrowPrev; // 0x28
        public LiveTimelineKeyFacialEyebrowData eyebrowCur; // 0x28
        public LiveTimelineKeyFacialEyebrowData eyebrowNext; // 0x28
        public LiveTimelineKeyFacialEyeTrackData eyeTrackPrev; // 0x30
        public LiveTimelineKeyFacialEyeTrackData eyeTrackCur; // 0x30
        public LiveTimelineKeyFacialEyeTrackData eyeTrackNext; // 0x30
        public LiveTimelineKeyFacialEarData earPrev; // 0x38
        public LiveTimelineKeyFacialEarData earCur; // 0x38
        public LiveTimelineKeyFacialEarData earNext; // 0x38
        public LiveTimelineKeyFacialEffectData effect; // 0x40
        public int faceKeyIndex; // 0x48
        public int mouthKeyIndex; // 0x4C
        public int eyeKeyIndex; // 0x50
        public int eyebrowKeyIndex; // 0x54
        public int eyeTrackKeyIndex; // 0x58
        public int earKeyIndex; // 0x5C
        public int effectKeyIndex; // 0x60
    }
}
