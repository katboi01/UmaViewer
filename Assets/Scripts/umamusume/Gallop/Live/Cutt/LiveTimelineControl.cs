using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public class LiveTimelineControl : MonoBehaviour
    {
        public LiveTimelineData data;
        public Transform cameraPositionLocatorsRoot;
        public Transform cameraLookAtLocatorsRoot;
        [SerializeField]
        private Transform[] characterStandPosLocators;

        public void CopyValues<T>(T from, T to)
        {
            var json = JsonUtility.ToJson(from);
            JsonUtility.FromJsonOverwrite(json, to);
        }

        private void Awake()
        {
            InitializeTimeLineData();
        }

        public void InitializeTimeLineData()
        {
            var LoadData = gameObject.AddComponent<LiveTimelineData>();
            CopyValues(data, LoadData);
            foreach(LiveTimelineWorkSheet worksheet in LoadData.worksheetList)
            {
                var LoadSheet = gameObject.AddComponent<LiveTimelineWorkSheet>();
                CopyValues(worksheet, LoadSheet);
            }
        }
    }

    
}

