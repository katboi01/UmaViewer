using UnityEngine;

namespace Stage
{
    public class CuttConditionOption : MonoBehaviour
    {
        [Header("モーション変換用")]
        [SerializeField]
        public Live3DMotionChangeParams motionChangeParams;

        [SerializeField]
        public ConditionVmData changeMotion;

        [Header("シート分岐用の条件データ")]
        [SerializeField]
        public ConditionVmData changeSheet;

        [Header("キャラ衣装変更")]
        [SerializeField]
        public ConditionVmData charaDressCondition;

        [SerializeField]
        public ConditionVmData charaDressExecution;
    }
}
