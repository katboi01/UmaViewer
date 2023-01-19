using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyPropsData : LiveTimelineKeyWithInterpolate
    {
        public const int kAttrDrawAfterImage = 1048576;

        public const int kAttrDrawLighting = 2097152;

        public const int kAttrUseLocalAxis = 4194304;

        public int settingFlags;

        public int propsID = -1;

        public bool rendererEnable = true;

        public Color color = Color.white;

        public Color rootColor = Color.white;

        public Color tipColor = Color.white;

        public float colorPower = 1f;

        public Vector3 lightPos = Vector3.zero;

        public float specularPower = 5f;

        public Color specularColor = Color.white;

        public Color luminousColor = new Color(1f, 1f, 1f, 0f);

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Props;

        public bool IsDrawAfterImage()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrDrawAfterImage);
        }

        public bool IsUseLocalAxis()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrUseLocalAxis);
        }
    }
}
