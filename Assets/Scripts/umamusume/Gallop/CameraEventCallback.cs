using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop
{
    public class CameraEventCallback : MonoBehaviour
    {
        private Camera _targetCamera; // 0x18
        public Action<Camera> OnPreCullCallback; // 0x20
        public Action<Camera> OnPreRenderCallback; // 0x28
        public Action<Camera> OnPostRenderCallback; // 0x30
    }
}
