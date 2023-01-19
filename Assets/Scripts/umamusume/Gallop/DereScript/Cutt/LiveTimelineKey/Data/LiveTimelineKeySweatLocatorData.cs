using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeySweatLocatorData : LiveTimelineKeyWithInterpolate
    {
        public struct LocatorInfo
        {
            public bool isVisible;

            public Vector3 offset;

            public Vector3 offsetAngle;
        }

        public eSweatLocatorOwner owner;

        public float alpha = 1f;

        public int randomVisibleCount;

        public bool locator0_isVisible;

        public Vector3 locator0_offset;

        public Vector3 locator0_offsetAngle;

        public bool locator1_isVisible;

        public Vector3 locator1_offset;

        public Vector3 locator1_offsetAngle;

        public bool locator2_isVisible;

        public Vector3 locator2_offset;

        public Vector3 locator2_offsetAngle;

        public bool locator3_isVisible;

        public Vector3 locator3_offset;

        public Vector3 locator3_offsetAngle;

        public bool locator4_isVisible;

        public Vector3 locator4_offset;

        public Vector3 locator4_offsetAngle;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.SweatLocator;
    }
}
