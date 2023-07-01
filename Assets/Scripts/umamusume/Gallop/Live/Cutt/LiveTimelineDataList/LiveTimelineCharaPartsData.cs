using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gallop.Cutt.Util;
using Cutt;

namespace Gallop.Live.Cutt
{
	[System.Serializable]
	public class LiveTimelineKeyCharaParts : LiveTimelineKeyWithInterpolate
	{
		public TimelineCharaPartsData[] CharaPartsDataArray;
	}

	[System.Serializable]
	public class LiveTimelineKeyCharaPartsDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCharaParts>
	{

	}

	[System.Serializable]
    public class LiveTimelineCharaPartsData
    {
        private const int DATA_LIST_SIZE = 20;
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList centerKeys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList left1Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList right1Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList left2Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList right2Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place06Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place07Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place08Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place09Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place10Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place11Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place12Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place13Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place14Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place15Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place16Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place17Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place18Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place19Keys; 
        [SerializeField] 
        private LiveTimelineKeyCharaPartsDataList place20Keys; 
        private readonly ILiveTimelineKeyDataList[] _cacheDataList; 
    }
}