using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop
{
    [System.Serializable]
    public class FaceTypeSet // TypeDefIndex: 24416
    {
        public int[] faceType; // 0x10
        public int count; // 0x18
    }

    [System.Serializable]
    public class EarTypeSet // TypeDefIndex: 24417
    {
        public EarType[] faceType; // 0x10
        public int count; // 0x18
    }

    [System.Serializable]
    public class DrivenKeyAnimation : ScriptableObject
    {
        public const int FaceNumGroupDiff = 2;
        public const int FaceNum = 5;
        public const int EyeNum = 2;
        public const int EyebrowNum = 2;
        public const int MouthNum = 1;
        public const int EarNum = 2;
        public AnimationClip clip; // 0x18
        public FaceTypeSet[] animationFaceType; // 0x20
        public EarTypeSet[] animationEarType; // 0x28
        [SerializeField] // RVA: 0x1EC0 Offset: 0x1EC0 VA: 0x7FFD44411EC0
        private bool _isCheckEyeRange; // 0x30
    }
}
