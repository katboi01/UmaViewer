using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineCharaIKData : ILiveTimelineSetData
    {
        [NonSerialized]
        private static readonly LiveTimelineKeyDataType[] DataTypeArray = new LiveTimelineKeyDataType[DATA_LIST_SIZE]
        {
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK,
            LiveTimelineKeyDataType.CharaIK
        };

        private const int DATA_LIST_SIZE = 15;

        public LiveTimelineKeyCharaIKDataList centerKeys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList left1Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList right1Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList left2Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList right2Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList left3Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList right3Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList left4Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList right4Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList left5Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList right5Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList left6Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList right6Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList left7Keys = new LiveTimelineKeyCharaIKDataList();

        public LiveTimelineKeyCharaIKDataList right7Keys = new LiveTimelineKeyCharaIKDataList();

        private LiveTimelineKeyCharaIKDataList[] _cacheDataList = new LiveTimelineKeyCharaIKDataList[DATA_LIST_SIZE];

        public ILiveTimelineKeyDataList GetKeyList(int index)
        {
            return GetKeyListArray()[index];
        }

        public LiveTimelineKeyCharaIKDataList[] GetIKKeyListArray()
        {
            return GetKeyListArray() as LiveTimelineKeyCharaIKDataList[];
        }

        public ILiveTimelineKeyDataList[] GetKeyListArray()
        {
            _cacheDataList[0] = centerKeys;
            _cacheDataList[1] = left1Keys;
            _cacheDataList[2] = right1Keys;
            _cacheDataList[3] = left2Keys;
            _cacheDataList[4] = right2Keys;
            _cacheDataList[5] = left3Keys;
            _cacheDataList[6] = right3Keys;
            _cacheDataList[7] = left4Keys;
            _cacheDataList[8] = right4Keys;
            _cacheDataList[9] = left5Keys;
            _cacheDataList[10] = right5Keys;
            _cacheDataList[11] = left6Keys;
            _cacheDataList[12] = right6Keys;
            _cacheDataList[13] = left7Keys;
            _cacheDataList[14] = right7Keys;

            return _cacheDataList;
        }

        public LiveTimelineKeyDataType[] GetKeyTypeArray()
        {
            return DataTypeArray;
        }
    }
}

