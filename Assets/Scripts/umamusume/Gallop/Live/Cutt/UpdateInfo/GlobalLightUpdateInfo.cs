using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public struct GlobalLightUpdateInfo
    {
        // Fields
        public Quaternion lightRotation; // 0x10
        public float globalRimShadowRate; // 0x20
        public Color rimColor; // 0x24
        public float rimStep; // 0x34
        public float rimFeather; // 0x38
        public float rimSpecRate; // 0x3c
        public LiveCharaPositionFlag flags; // 0x40
        public float RimHorizonOffset; // 0x44
        public float RimVerticalOffset; // 0x48
        public float RimHorizonOffset2; // 0x4c
        public float RimVerticalOffset2; // 0x50
        public Color rimColor2; // 0x54
        public float rimStep2; // 0x64
        public float rimFeather2; // 0x68
        public float rimSpecRate2; // 0x6c
        public float globalRimShadowRate2; // 0x70
    }

    public delegate void GlobalLightUpdateInfoDelegate(ref GlobalLightUpdateInfo updateInfo);
}