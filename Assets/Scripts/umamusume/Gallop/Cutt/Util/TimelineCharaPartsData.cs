using UnityEngine;

namespace Gallop.Cutt.Util
{
    [System.Serializable]
    public class TimelineCharaPartsData
    {
        [SerializeField]
        private string _rendererName;
        [SerializeField]
        private int _rendererHash;
        public bool IsVisible;
    }
}
