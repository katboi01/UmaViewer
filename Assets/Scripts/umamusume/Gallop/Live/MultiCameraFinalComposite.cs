using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live
{
    public class MultiCameraFinalComposite : MonoBehaviour
    {
        public enum FadeType
        {
            Normal = 0,
            Single = 1,
            CaptureFrame = 2
        }

        private enum MaterialType
        {
            None = 0,
            Normal = 1,
            Single = 2
        }

        [SerializeField]
        private RenderTexture _finalCompositeTexture;
        [SerializeField]
        private RenderTexture _compositeTexture;
        [SerializeField]
        private Material[] _material;
        private Material _clearMaterial;
        [SerializeField]
        private float _fadeValue;
        [SerializeField]
        private RenderTexture _monitorTexture;
        private MultiCameraFinalComposite.FadeType _fadeType;
        //private GallopImageEffect _imageEffect;
    }
}

