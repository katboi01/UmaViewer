using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMonitorControlData : LiveTimelineKeyWithInterpolate
    {
        public enum eInputSource
        {
            Default,
            MonitorCamera,
            MultiCamera
        }

        [Flags]
        public enum Attr
        {
            MultiFlag = 0x10000,
            ReversePlayFlag = 0x20000
        }

        public Vector2 position = Vector2.zero;

        public Vector2 size = Vector2.one;

        public int dispID;

        public int subDispID;

        public float speed = 1f;

        public string outputTextureLabel = "";

        public int playStartOffsetFrame;

        public float blendFactor = 1f;

        public float colorPower = 2f;

        public eInputSource inputSource;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MonitorControl;

        private bool IsMultiFlag()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)Attr.MultiFlag);
        }

        private bool IsReversePlayFlag()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)Attr.ReversePlayFlag);
        }
    }
}
