using UnityEngine;

namespace Cutt
{
    public struct PropsUpdateInfo
    {
        public float progressTime;

        public float currentTime;

        public LiveTimelinePropsData data;

        public int settingFlags;

        public int propsID;

        public bool renderEnable;

        public Color color;

        public float colorPower;

        public bool drawAfterImage;

        public Color rootColor;

        public Color tipColor;

        public Vector3 lightPosition;

        public bool useLocalAxis;

        public float specularPower;

        public Color specularColor;

        public Color luminousColor;
    }
}
