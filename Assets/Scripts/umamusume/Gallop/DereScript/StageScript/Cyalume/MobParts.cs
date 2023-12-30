using Cutt;
using UnityEngine;

namespace Stage.Cyalume
{
    public class MobParts
    {
        public enum Part
        {
            M_Mob,
            Arm_L,
            Elbow_L,
            Wrist_L,
            Cyalume_L,
            Arm_R,
            Elbow_R,
            Wrist_R,
            Cyalume_R,
            Max
        }

        public class Context
        {
            public GameObject RootObj;

            public Transform[] AnimationTransform;

            public GameObject[] PartArray;
        }

        private const int ANIMATION_VARY_NUM = 4;

        public const int ANIMATION_DELAY_NUM = 8;

        private static readonly int ANIMATION_MATRIX_PROPERTY_ID = Shader.PropertyToID("_AnimationMatrix");

        private static readonly int CURRENT_ANIMATION_INDEX_PROPERTY_ID = Shader.PropertyToID("_CurrentAnimationIndex");

        private static readonly int LOOK_AT_POSITION_PROPERTY_ID = Shader.PropertyToID("_LookAtPosition");

        private static readonly int POSITION_PROPERTY_ID = Shader.PropertyToID("_position");

        private static readonly int COLOR_PALETTE_PROPERTY_ID = Shader.PropertyToID("_colorPalette");

        private static readonly int CAMERA_ARROW_PROPERTY_ID = Shader.PropertyToID("_cameraArrow");

        private static readonly int GRADIATION_PROPERTY_ID = Shader.PropertyToID("_gradiation");

        private static readonly int RIMLIGHT_PROPERTY_ID = Shader.PropertyToID("_rimlight");

        private static readonly int BLENDRANGE_PROPERTY_ID = Shader.PropertyToID("_blendrange");

        private static readonly int WAVE_BASE_POSITION_PROPERTY_ID = Shader.PropertyToID("_waveBasePosition");

        private static readonly int WAVE_WIDTH_PROPERTY_ID = Shader.PropertyToID("_waveWidth");

        private static readonly int WAVE_HEIGHT_PROPERTY_ID = Shader.PropertyToID("_waveHeight");

        private static readonly int WAVE_ROUGHNESS_PROPERTY_ID = Shader.PropertyToID("_waveRoughness");

        private static readonly int WAVE_PROGRESS_PROPERTY_ID = Shader.PropertyToID("_waveProgress");

        private static readonly int WAVE_COLOR_BASE_POWER_PROPERTY_ID = Shader.PropertyToID("_waveColorBasePower");

        private static readonly int WAVE_COLOR_GAIN_POWER_PROPERTY_ID = Shader.PropertyToID("_waveColorGainPower");

        private const string STATIC_LOOK_AT_ENABLE_KEYWORD = "STATIC_LOOK_AT_ENABLE";

        private const string FLOATING_LOOK_AT_ENABLE_KEYWORD = "FLOATING_LOOK_AT_ENABLE";

        public readonly string[] MODE_WAVE_SHADER_KEYWORD = new string[5] { "MODE_WAVE_NONE", "MODE_WAVE_STRAIGHT_X", "MODE_WAVE_STRAIGHT_Z", "MOVE_WAVE_RADIAL", "MOVE_WAVE_CIRCLE" };

        private Matrix4x4[] _animationMatrix = new Matrix4x4[16];

        private int _animationIndex;

        public Transform[] _animationTransform;

        private GameObject[] _partArray;

        private Renderer[] _partRendererArray;

        private bool _isInitialize;

        private float _gradiation = 0.05f;

        private float _rimlight = 6f;

        private float _blendRange = 20f;

        private LiveMobCyalume3DLookAtMode _currentLookAtMode;

        private LiveMobCyalume3DWaveMode _currentWaveMode;

        private MaterialPropertyBlock _materialPropertyBlock;

        public float gradiation
        {
            set
            {
                _gradiation = value;
            }
        }

        public float rimlight
        {
            set
            {
                _rimlight = value;
            }
        }

        public float blendRange
        {
            set
            {
                _blendRange = value;
            }
        }

        private void OnSetup(Context context)
        {
            _animationTransform = context.AnimationTransform;
            _materialPropertyBlock = new MaterialPropertyBlock();
            _partArray = context.PartArray;
            _partRendererArray = new Renderer[_partArray.Length];
            for (int i = 0; i < _partArray.Length; i++)
            {
                _partRendererArray[i] = _partArray[i].GetComponent<Renderer>();
                _partRendererArray[i].material.EnableKeyword("RIMLIGHT_ENABLE");
            }
            _isInitialize = true;
        }

        public void Setup(Context context)
        {
            _isInitialize = false;
            OnSetup(context);
        }

        public void AlterLateUpdate(Vector4[] lookAtPosition, Vector4[] position, Vector2 colorPalette, Vector3 cameraArrow, bool isUpdateMotionMatrix)
        {
            if (!_isInitialize)
            {
                return;
            }
            int num = 0;
            if (isUpdateMotionMatrix)
            {
                for (int i = 0; i < 2; i++)
                {
                    _animationMatrix[i * 8 + _animationIndex] = Matrix4x4.TRS(_animationTransform[i].localPosition, _animationTransform[i].localRotation, _animationTransform[i].localScale);
                }
                num = _animationIndex;
                _animationIndex = (_animationIndex + 1) % 8;
            }
            else
            {
                _animationIndex = 1;
            }
            _materialPropertyBlock.SetFloat(CURRENT_ANIMATION_INDEX_PROPERTY_ID, num);
            _materialPropertyBlock.SetMatrixArray(ANIMATION_MATRIX_PROPERTY_ID, _animationMatrix);
            _materialPropertyBlock.SetVectorArray(LOOK_AT_POSITION_PROPERTY_ID, lookAtPosition);
            _materialPropertyBlock.SetVectorArray(POSITION_PROPERTY_ID, position);
            _materialPropertyBlock.SetVector(COLOR_PALETTE_PROPERTY_ID, colorPalette);
            _materialPropertyBlock.SetVector(CAMERA_ARROW_PROPERTY_ID, cameraArrow);
            _materialPropertyBlock.SetFloat(GRADIATION_PROPERTY_ID, _gradiation);
            _materialPropertyBlock.SetFloat(RIMLIGHT_PROPERTY_ID, _rimlight);
            _materialPropertyBlock.SetFloat(BLENDRANGE_PROPERTY_ID, 1f / _blendRange);
            Renderer[] partRendererArray = _partRendererArray;
            for (int j = 0; j < partRendererArray.Length; j++)
            {
                partRendererArray[j].SetPropertyBlock(_materialPropertyBlock, 0);
            }
        }

        public void SetMotionSample(int index)
        {
            for (int i = 0; i < 2; i++)
            {
                _animationMatrix[i * 8 + (8 - index) % 8] = Matrix4x4.TRS(_animationTransform[i].localPosition, _animationTransform[i].localRotation, _animationTransform[i].localScale);
            }
        }

        public void SetMaterialLookAtMode(LiveMobCyalume3DLookAtMode lookAtMode)
        {
            if (lookAtMode == _currentLookAtMode)
            {
                return;
            }
            _currentLookAtMode = lookAtMode;
            for (int i = 0; i < _partRendererArray.Length; i++)
            {
                Renderer renderer = _partRendererArray[i];
                switch (lookAtMode)
                {
                    case LiveMobCyalume3DLookAtMode.Default:
                        renderer.material.DisableKeyword(STATIC_LOOK_AT_ENABLE_KEYWORD);
                        renderer.material.DisableKeyword(FLOATING_LOOK_AT_ENABLE_KEYWORD);
                        break;
                    case LiveMobCyalume3DLookAtMode.Locator:
                        renderer.material.EnableKeyword(STATIC_LOOK_AT_ENABLE_KEYWORD);
                        renderer.material.DisableKeyword(FLOATING_LOOK_AT_ENABLE_KEYWORD);
                        break;
                    case LiveMobCyalume3DLookAtMode.LookAtPosition:
                        renderer.material.DisableKeyword(STATIC_LOOK_AT_ENABLE_KEYWORD);
                        renderer.material.EnableKeyword(FLOATING_LOOK_AT_ENABLE_KEYWORD);
                        break;
                }
            }
        }

        public void SetMaterialWaveModeParam(LiveMobCyalume3DWaveMode waveMode, Vector3 waveBasePosition, float waveWidth, float waveHeight, float waveRoughness, float waveProgress, float waveColorBasePower, float waveColorGainPower)
        {
            if (_currentWaveMode != waveMode)
            {
                _currentWaveMode = waveMode;
                for (int i = 0; i < _partRendererArray.Length; i++)
                {
                    Renderer renderer = _partRendererArray[i];
                    SetShaderKeyword((int)_currentWaveMode, MODE_WAVE_SHADER_KEYWORD, renderer.material);
                }
            }
            _materialPropertyBlock.SetVector(WAVE_BASE_POSITION_PROPERTY_ID, waveBasePosition);
            _materialPropertyBlock.SetFloat(WAVE_WIDTH_PROPERTY_ID, waveWidth);
            _materialPropertyBlock.SetFloat(WAVE_HEIGHT_PROPERTY_ID, waveHeight);
            _materialPropertyBlock.SetFloat(WAVE_ROUGHNESS_PROPERTY_ID, waveRoughness);
            _materialPropertyBlock.SetFloat(WAVE_PROGRESS_PROPERTY_ID, waveProgress);
            _materialPropertyBlock.SetFloat(WAVE_COLOR_BASE_POWER_PROPERTY_ID, waveColorBasePower);
            _materialPropertyBlock.SetFloat(WAVE_COLOR_GAIN_POWER_PROPERTY_ID, waveColorGainPower);
        }

        public void SetVisible(bool isVisible)
        {
            for (int i = 0; i < _partArray.Length; i++)
            {
                _partArray[i].SetActive(isVisible);
            }
        }

        private void SetShaderKeyword(int index, string[] keywords, Material material)
        {
            for (int i = 0; i < keywords.Length; i++)
            {
                if (index != i)
                {
                    material.DisableKeyword(keywords[i]);
                }
            }
            if (0 < index && index < keywords.Length)
            {
                material.EnableKeyword(keywords[index]);
            }
        }
    }
}
