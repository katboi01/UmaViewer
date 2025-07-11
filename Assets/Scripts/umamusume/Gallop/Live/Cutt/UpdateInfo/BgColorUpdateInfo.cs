using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Gallop.Live.Cutt.LiveTimelineDefine;

namespace Gallop.Live.Cutt
{
    public struct BgColor1UpdateInfo
    {
        // Fields
        public int TimelineNameHash; // 0x10
        public int[] TargetCharaIdArray; // 0x18
        public int[] TargetDressIdArray; // 0x20
        public Color color; // 0x28
        public float colorPower; // 0x38
        public float scale; // 0x3c
        public float Saturation; // 0x40
        public int flags; // 0x44
        public Color toonDarkColor; // 0x48
        public Color toonBrightColor; // 0x58
        public float vertexColorToonPower; // 0x68
        public float outlineWidthPower; // 0x6c
        public Color outlineColor; // 0x70
        public OutlineColorBlend outlineColorBlend; // 0x80
        public LightBlendMode LightBlendMode; // 0x84
        public bool IsSilhouette; // 0x88
        public bool IsProjector; // 0x89
        public ColorType CurrentColorType; // 0x8c
        public Color CurrentColor; // 0x90
        public int CurrentFlags; // 0xa0
        public ColorType NextColorType; // 0xa4
        public Color NextColor; // 0xa8
        public int NextFlags; // 0xb8
        public float InterpolateRatio; // 0xbc
        public bool IsSyncBlinkLight; // 0xc0
        public bool IsSyncBlinkLightNext; // 0xc1
        public bool IsAdjustedBlinkLightColor; // 0xc2
        public int BlinkLightNameHash; // 0xc4
        public int BlinkLightContainerIndex; // 0xc8
        public float BlinkLightBrightnessPower; // 0xcc
    }

    public delegate void BgColor1UpdateInfoDelegate(ref BgColor1UpdateInfo updateInfo);

    public struct BgColor2UpdateInfo
    {
        // Fields
        public int TimelineNameHash; // 0x10
        public Color color1; // 0x14
        public Color color2; // 0x24
        public float value; // 0x34
        public int rndValueIdx; // 0x38
    }

    public delegate void BgColor2UpdateInfoDelegate(ref BgColor2UpdateInfo updateInfo);
}