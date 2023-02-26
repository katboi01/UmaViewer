using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyFormationOffsetData : LiveTimelineKeyWithInterpolate
    {
        private const int kAttrResetCloth = 65536;

        private const int kAttrEffectClear = 131072;

        public Vector2 posXZ = Vector2.zero;

        public float posY;

        public float rotY;

        public Vector3 charaAxisRotate = Vector3.zero;

        public bool visible = true;

        public bool useLateRotate;

        public Vector3 lateCharaRotate = Vector3.zero;

        public bool isWorldSpace;

        public Vector3 worldSpaceOrigin = Vector3.zero;

        public float worldRotationY;

        public bool isLookAtWorldOrigin;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.FormationOffset;

        public bool IsResetCloth()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrResetCloth);
        }

        public bool IsEffectClear()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrEffectClear);
        }
    }
}

