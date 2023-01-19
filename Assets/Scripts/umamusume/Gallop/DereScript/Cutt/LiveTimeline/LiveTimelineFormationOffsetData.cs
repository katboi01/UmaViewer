using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineFormationOffsetData : ILiveTimelineSetData
    {
        private const int DATA_LIST_SIZE = 5;

        public LiveTimelineKeyFormationOffsetDataList centerKeys = new LiveTimelineKeyFormationOffsetDataList();

        public LiveTimelineKeyFormationOffsetDataList left1Keys = new LiveTimelineKeyFormationOffsetDataList();

        public LiveTimelineKeyFormationOffsetDataList right1Keys = new LiveTimelineKeyFormationOffsetDataList();

        public LiveTimelineKeyFormationOffsetDataList left2Keys = new LiveTimelineKeyFormationOffsetDataList();

        public LiveTimelineKeyFormationOffsetDataList right2Keys = new LiveTimelineKeyFormationOffsetDataList();

        private ILiveTimelineKeyDataList[] _cacheDataList = new ILiveTimelineKeyDataList[DATA_LIST_SIZE];

        public ILiveTimelineKeyDataList GetKeyList(int index)
        {
            return GetKeyListArray()[index];
        }

        public ILiveTimelineKeyDataList[] GetKeyListArray()
        {
            _cacheDataList[0] = centerKeys;
            _cacheDataList[1] = left1Keys;
            _cacheDataList[2] = right1Keys;
            _cacheDataList[3] = left2Keys;
            _cacheDataList[4] = right2Keys;
            return _cacheDataList;
        }

        public LiveTimelineKeyDataType[] GetKeyTypeArray()
        {
            return new LiveTimelineKeyDataType[5]
            {
                LiveTimelineKeyDataType.FormationOffset,
                LiveTimelineKeyDataType.FormationOffset,
                LiveTimelineKeyDataType.FormationOffset,
                LiveTimelineKeyDataType.FormationOffset,
                LiveTimelineKeyDataType.FormationOffset
            };
        }
    }
}
