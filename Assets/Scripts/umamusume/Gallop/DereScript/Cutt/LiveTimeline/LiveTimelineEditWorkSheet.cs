using System.Collections.Generic;
using UnityEngine;

namespace Cutt
{
    public class LiveTimelineEditWorkSheet : ScriptableObject
    {
        public const string EditAssetDataName = "EditData";

        public string version = LiveTimelineWorkSheet.sVersionList[0];

        public LiveTimelineKeyCuttNumberDataList cuttNumberKeys = new LiveTimelineKeyCuttNumberDataList();

        public List<EditTimelineEditorColor> formationOffsetColor = new List<EditTimelineEditorColor>();

        public List<EditTimelineEditorColor> objectColor = new List<EditTimelineEditorColor>();

        public List<EditTimelineEditorName> motionSeqLineName = new List<EditTimelineEditorName>();

        public List<EditTimelineEditorName> overrideMotionSeqLineName = new List<EditTimelineEditorName>();

        private ILiveTimelineKeyDataList[] _keysArray;

        public ILiveTimelineKeyDataList[] keysArray
        {
            get
            {
                if (_keysArray == null)
                {
                    List<ILiveTimelineKeyDataList> list = new List<ILiveTimelineKeyDataList> { cuttNumberKeys };
                    _keysArray = list.ToArray();
                }
                return _keysArray;
            }
        }

        public void OnLoad(LiveTimelineControl timelineControl)
        {
        }
    }
}