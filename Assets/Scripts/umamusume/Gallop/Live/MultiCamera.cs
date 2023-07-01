using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live
{
    public class MultiCamera : MonoBehaviour
    {
        private RenderTexture _framebufferCompositeTexture; // 0x18
        private RenderTexture _compositeTexture; // 0x20
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private Material[] _compositeMaterial; // 0x28
    }
}
