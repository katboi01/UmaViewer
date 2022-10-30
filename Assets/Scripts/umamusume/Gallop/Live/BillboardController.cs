using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live
{
    public class BillboardController : MonoBehaviour
    {
        public enum RotationType
        {
            Default = 0,
            FixHorizon = 1,
            AlwaysCamera = 2
        }

        [SerializeField]
        private Transform _targetCameraTransform;
        [SerializeField]
        private BillboardController.RotationType _rotationType;
    }
}

