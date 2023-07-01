using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop
{
    [System.Serializable]
    public abstract class ModelControllerBehaviour : MonoBehaviour
    {

    }

    [System.Serializable]
    public class ModelController : ModelControllerBehaviour
    {
        public enum OutlineColorBlend
        {
            Blend = 0,
            Multiply = 1,
            Max = 2
        }

        public struct ModelDepthInfo
        {
            public int index; // 0x0
            public float depthOrder; // 0x4
        }
    }
}
