using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live
{
    public class Master3dLive
    {
        public class LiveSettings
        {
            public enum DataType
            {
                Cutt = 0,
                Bg = 1,
                Delay = 2,
                Camera = 3,
                UVMovie = 4,
                MirrorScanLightMaterial = 5,
                OP = 6,
                ED = 7,
                OptionOP = 8,
                OptionED = 9,
                AudienceAnimation = 10,
                ProjectorTexture = 11,
                DressChangePosition = 12,
                CharacterExpansion = 13,
                ExtraResource = 14,
                MusicResource = 15
            }

            private const int COLUM_MIN = 2;
            public const int PARAM_MAX = 20;

            public int Id { get; set; }
            public int TypeValue { get; set; }
            public Master3dLive.LiveSettings.DataType Type { get; set; }
            public string[] ParamStringArray { get; set; }
            public int[] ParamValueArray { get; set; }
        }

        public class Live3dData
        {
            public enum ExtraResource
            {
                None = 0,
                ChampionsMeeting = 1
            }

            public class UvMovieData
            {
                public enum LoadCondition
                {
                    Never = -1,
                    Always = 0,
                    CharaDress = 1
                }

                public enum PlayCondition
                {
                    PlayNoLight = 0,
                    PlayLight = 1
                }

                public string ResourceName { get; set; }
                public int LoadConditionValue { get; set; }
                public int PlayConditionValue { get; set; }
            }

            public int MusicId { get; set; }
            public int MusicResourceId { get; set; }
            public string CuttName { get; set; }
            public int BgId { get; set; }
            public int BgVar { get; set; }
            public float DelayTime { get; set; }
            public string CameraName { get; set; }
            public Master3dLive.Live3dData.UvMovieData[] UvMovieDataArray { get; set; }
            public int[] MirrorScanLightIdArray { get; set; }
            public string[] MirrorScanLightNameArray { get; set; }
            public bool IsOP { get; set; }
            public bool IsED { get; set; }
            public bool IsLyricsOP { get; set; }
            public bool IsLyricsED { get; set; }
            public bool IsCyalumeOP { get; set; }
            public bool IsCyalumeED { get; set; }
            public string[] AudienceAnimationArray { get; set; }
            public Dictionary<int, string> ProjectorTextureDictionary { get; set; }
            public int ExtraResourceId { get; set; }
        }

        public Master3dLive.Live3dData LiveData;
    }
}
