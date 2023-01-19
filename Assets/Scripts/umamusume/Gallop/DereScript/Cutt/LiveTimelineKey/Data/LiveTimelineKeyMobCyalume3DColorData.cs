using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMobCyalume3DColorData : LiveTimelineKeyWithInterpolate
    {
        public List<Color> colorList = new List<Color> { Color.white };

        public LiveMobCyalume3DColorPreset preset;

        public bool isEditDetails = true;

        public bool isDevideByGroup;

        public int[] colorIndex_1_0 = new int[1] { -1 };

        public int[] colorIndex_3_0 = new int[3] { -1, -1, -1 };

        public int[] colorIndex_1_1 = new int[2] { -1, -1 };

        public int[] colorIndex_3_3 = new int[6] { -1, -1, -1, -1, -1, -1 };

        public bool isAvoidDuplicateColor;

        public bool isSameCombinationColor;

        private static string _resultString = string.Empty;

        private static string _resultDetailString = string.Empty;

        private const int COLOR_COUNT_MIN = 1;

        private const int COLOR_COUNT_MAX = 10;

        private const int SECTION_INTERVAL = 8;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MobCyalume3DColor;
    }
}
