using UnityEngine;

namespace Gallop
{
    [System.Serializable]
    public class CySpringWindParam
    {
        [SerializeField]
        private bool _isEnableVertical;
        [SerializeField]
        private bool _isEnableHorizontal;
        [SerializeField]
        private float _verticalCycle;
        [SerializeField]
        private float _horizontalCycle;
        [SerializeField]
        private float _verticalAngleWidth;
        [SerializeField]
        private float _horizontalAngleWidth;
        [SerializeField]
        private int _partsMask;
        [SerializeField]
        private float[] _powerScaleArray;
        [SerializeField]
        private Vector3 _direction;
        [SerializeField]
        private Vector3 _right;
        [SerializeField]
        private bool _isLocalDirection;
    }
}
