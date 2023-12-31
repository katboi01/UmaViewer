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

        public Vector2 MaskOffset;
        public float MaskRoll;

        //From DereViewer

        private const float NearClipOffset = 0.01f;

        private Camera _camera;

        private int _maskIndex = -1;

        private Transform _myTransform;

        private Transform _maskTransform;

        private Renderer[] _maskRenderer;


        public Vector3 maskPosition { get; set; }

        public Quaternion maskRotation { get; set; }

        public Vector3 maskScale { get; set; }

        public int maskIndex => _maskIndex;

        public Camera GetCamera()
        {
            return _camera;
        }

        public Transform GetMaskTransform()
        {
            return _maskTransform;
        }

        public void DetachMask()
        {
            _maskTransform = null;
            _maskRenderer = null;
            _maskIndex = -1;
        }

        public void Initialize()
        {
            if (!(_camera != null))
            {
                _myTransform = base.transform;
                _camera = base.gameObject.AddComponent<Camera>();
                _camera.enabled = false;
                _camera.depth = -3;
            }
        }

        public void Setup(int cameraDepth, RenderTexture colorBuffer, RenderTexture depthBuffer)
        {
            if (!(_camera == null))
            {
                _camera.depth = cameraDepth;
                //_camera.cullingMask = StageUtil.Background3dAllLayers() | StageUtil.CharaAllLayers();
                _camera.clearFlags = CameraClearFlags.Depth;
                _camera.allowHDR = false;
                _camera.SetTargetBuffers(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
            }
        }

        private void Release()
        {
            DetachMask();
            _myTransform = null;
            _camera = null;
        }

        private void OnDestroy()
        {
            Release();
        }

        private void UpdateTransform()
        {
            Vector3 forward = _myTransform.forward;
            Vector3 vector = _myTransform.localRotation * maskPosition + forward * (_camera.nearClipPlane + 0.01f);
            _maskTransform.localPosition = _myTransform.localPosition + vector;
            _maskTransform.localRotation = Quaternion.LookRotation(forward) * maskRotation;
            _maskTransform.localScale = maskScale;
        }

        private void OnPreCull()
        {
            if (_maskIndex != -1)
            {
                UpdateTransform();
                Renderer[] maskRenderer = _maskRenderer;
                for (int i = 0; i < maskRenderer.Length; i++)
                {
                    maskRenderer[i].enabled = true;
                }
            }
        }

        private void OnPostRender()
        {
            if (_maskIndex != -1)
            {
                Renderer[] maskRenderer = _maskRenderer;
                for (int i = 0; i < maskRenderer.Length; i++)
                {
                    maskRenderer[i].enabled = false;
                }
            }
        }
    }
}
