using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
	[System.Serializable]
	public class LiveTimelineKeyCharaWind : LiveTimelineKeyWithInterpolate
	{
		public bool IsEnableWind;
		public CySpringWindParam WindParam;
	}

	[System.Serializable]
	public class LiveTimelineKeyCharaWindDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCharaWind>
	{

	}

	[System.Serializable]
    public class LiveTimelineCharaWindData
    {
		private const int DATA_LIST_SIZE = 18;
		public LiveTimelineKeyCharaWindDataList centerKeys; 
		public LiveTimelineKeyCharaWindDataList left1Keys;
		public LiveTimelineKeyCharaWindDataList right1Keys; 
		public LiveTimelineKeyCharaWindDataList left2Keys;
		public LiveTimelineKeyCharaWindDataList right2Keys; 
		public LiveTimelineKeyCharaWindDataList place06Keys;
		public LiveTimelineKeyCharaWindDataList place07Keys;
		public LiveTimelineKeyCharaWindDataList place08Keys;
		public LiveTimelineKeyCharaWindDataList place09Keys;
		public LiveTimelineKeyCharaWindDataList place10Keys;
		public LiveTimelineKeyCharaWindDataList place11Keys;
		public LiveTimelineKeyCharaWindDataList place12Keys;
		public LiveTimelineKeyCharaWindDataList place13Keys;
		public LiveTimelineKeyCharaWindDataList place14Keys;
		public LiveTimelineKeyCharaWindDataList place15Keys;
		public LiveTimelineKeyCharaWindDataList place16Keys;
		public LiveTimelineKeyCharaWindDataList place17Keys;
		public LiveTimelineKeyCharaWindDataList place18Keys;
	}
}