using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop
{
    [Serializable]
    public struct FaceParts
    {
        public int _faceParts; // 0x0
        public float _weight; // 0x4
        public float _originalWeight; // 0x8
    }

    [Serializable]
    public class FacePartsSet
    {
        private static FacePartsSet _baseFacePartsSet; // 0x0
        public FaceParts[] _eyeR; // 0x10
        public FaceParts[] _eyeL; // 0x18
        public FaceParts[] _eyebrowR; // 0x20
        public FaceParts[] _eyebrowL; // 0x28
        public FaceParts[] _mouth; // 0x30
    }
}
