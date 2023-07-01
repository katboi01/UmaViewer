using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum LiveFlashPlayerActionType
    {
        PlayIn = 0,
        None = 1,
        Max = 2,
    }

    [Serializable]
    public class LiveTimelineKeyFlashPlayerData : LiveTimelineKey
    {
        public LiveFlashPlayerActionType _actionType;
        public string CueSheetName;
        public string CueName;
    }

    [Serializable]
    public class LiveTimelineKeyFlashPlayerDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFlashPlayerData>
    {

    }
}

