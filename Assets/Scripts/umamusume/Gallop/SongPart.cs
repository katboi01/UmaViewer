using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop
{
    public interface ISongPartAccesor
    {

    }

    public class SongPart : ISongPartAccesor
    {
        public enum PartPosition
        {
            Center = 0,
            Left = 1,
            Right = 2,
            Left2 = 3,
            Right2 = 4,
            Left3 = 5,
            Right3 = 6,
            Max = 7,
            LLeft = 3,
            RRight = 4
        }

        public enum PartType
        {
            Off = 0,
            Label_0 = 1,
            Label_1 = 2,
            Label_2 = 3,
            Label_3 = 4,
            Label_4 = 5,
            Label_5 = 6,
            Max = 7
        }

        private enum CsvLabel
        {
            Time = 0,
            Sing_LLeft = 1,
            Sing_Left = 2,
            Sing_Center = 3,
            Sing_Right = 4,
            Sing_RRight = 5,
            Volume_LLeft = 6,
            Volume_Left = 7,
            Volume_Center = 8,
            Volume_Right = 9,
            Volume_RRight = 10,
            Pan_LLeft = 11,
            Pan_Left = 12,
            Pan_Center = 13,
            Pan_Right = 14,
            Pan_RRight = 15,
            Max = 16
        }

        private enum CsvLabel7
        {
            Time = 0,
            Sing_Left3 = 1,
            Sing_Left2 = 2,
            Sing_Left = 3,
            Sing_Center = 4,
            Sing_Right = 5,
            Sing_Right2 = 6,
            Sing_Right3 = 7,
            Volume_Left3 = 8,
            Volume_Left2 = 9,
            Volume_Left = 10,
            Volume_Center = 11,
            Volume_Right = 12,
            Volume_Right2 = 13,
            Volume_Right3 = 14,
            Pan_Left3 = 15,
            Pan_Left2 = 16,
            Pan_Left = 17,
            Pan_Center = 18,
            Pan_Right = 19,
            Pan_Right2 = 20,
            Pan_Right3 = 21,
            VolumeRate = 22,
            Max = 23
        }

        public class SongPartData
        {
            public long Time { get; set; }
            public SongPart.PartType[] PartArray { get; set; }
            public int[] PositionArray { get; set; }
            public int[] OrderArray { get; set; }
            public float[] PartVolumeArray { get; set; }
            public float[] PartPanArray { get; set; }
            public float PartVolumeRate { get; set; }
        }

        private List<SongPart.SongPartData> _songPartDataList; // 0x10
    }
}
