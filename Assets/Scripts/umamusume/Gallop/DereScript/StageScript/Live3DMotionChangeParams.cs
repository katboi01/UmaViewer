using System;
using UnityEngine;

namespace Stage
{

    [CreateAssetMenu(menuName = "Live3DMotionChangeParams")]
    public class Live3DMotionChangeParams : ScriptableObject
    {
        [Serializable]
        public struct ChangeParams
        {
            public string motionName;

            public ChangeList[] changeList;
        }

        [Serializable]
        public struct ChangeList
        {
            public string conditionName;

            public string motionSuffix;
        }

        [SerializeField]
        public ChangeParams[] changeParams;
    }
}
