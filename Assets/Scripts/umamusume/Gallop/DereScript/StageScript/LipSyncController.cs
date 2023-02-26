using Cutt;
using Stage;
using UnityEngine;

public interface ICharacterFacial
{
    int GetCharaFaceType(int sourceType, int faceSlotIndex, out Master3DCharaData.CharaDataSpecialFace.eCheek cheek);

    int GetOverridedFacialId(int faceId);

    bool GetCharaCheekFlag();
}

public class LipSyncController : MonoBehaviour
{
    private enum AnimatorParam
    {
        FaceFlag,
        MouthFlag,
        MouthSizeFlag,
        MouthSpeedFlag,
        EyeRFlag,
        EyeLFlag,
        EyeRSpeedFlag,
        EyeLSpeedFlag,
        Max
    }

    private enum FaceType
    {
        Normal = 0,
        Special = 1,
        Special_2 = 2,
        Special_3 = 3,
        Special_8 = 8,
        Dere = 12
    }

    private static int[] _animationParamHash = new int[8]
    {
        Animator.StringToHash("FaceFlag"),
        Animator.StringToHash("MouthFlag"),
        Animator.StringToHash("MouthSizeFlag"),
        Animator.StringToHash("MouthSpeedFlag"),
        Animator.StringToHash("EyeRFlag"),
        Animator.StringToHash("EyeLFlag"),
        Animator.StringToHash("EyeRSpeedFlag"),
        Animator.StringToHash("EyeLSpeedFlag")
    };

    private int kPropertyID_CharaColor;

    private const float kAutoBlinkRepressionTime = 1f;

    private const float defaultCheekMaxTime = 0.15f;

    private const float defaultAutoBlinkCap = 0.3f;

    [SerializeField]
    private Animator _animator;

    private AnimatorClipInfo[] _animeInfos;

    private int _nowVowel;

    [SerializeField]
    private float _nowTime;

    [SerializeField]
    private bool _isBlink;

    [SerializeField]
    private float _blinkTime;

    [SerializeField]
    private float _cheekMaxTime = 0.15f;

    [SerializeField]
    private bool _isCheek;

    private float _cheekTime;

    public Renderer _cheekRenderer;

    public bool _isRender = true;

    private bool _isUseMusicDelta = true;

    private float _autoBlinkCap = 0.3f;

    private ICharacterFacial _characterFacial;

    private int _currentFaceFlag;

    private int _rightEyeFlag;

    private int _leftEyeFlag;

    private bool _isAutoBlink = true;

    private bool _isRepressAutoBlink;

    private bool _isLipSync = true;

    private bool _isCheekDraw;

    private int _eyeRLayerIndex;

    private int _eyeLLayerIndex;

    private int _blinkRno;

    private int _currentStartFrameOfEye = -1;

    public bool isUseMusicDelta
    {
        get
        {
            return _isUseMusicDelta;
        }
        set
        {
            _isUseMusicDelta = value;
        }
    }

    public float autoBlinkCap
    {
        set
        {
            _autoBlinkCap = value;
        }
    }

    public ICharacterFacial characterFacial
    {
        set
        {
            _characterFacial = value;
        }
    }

    public int currentFaceFlag => _currentFaceFlag;

    public int rightEyeFlag => _rightEyeFlag;

    public int leftEyeFlag => _leftEyeFlag;

    public bool IsCheekDraw => _isCheekDraw;

    private static int GetParamID(AnimatorParam param)
    {
        return _animationParamHash[(int)param];
    }

    private float CalcBlinkInterval()
    {
        return Random.Range(3f, 5f);
    }

    private void Awake()
    {
        kPropertyID_CharaColor = Shader.PropertyToID("_CharaColor");
        _blinkTime = CalcBlinkInterval();
    }

    private void Start()
    {
        Initialize(_animator);
    }

    public void Initialize(Animator animator)
    {
        _animator = animator;
        if (_animator != null)
        {
            _animeInfos = _animator.GetNextAnimatorClipInfo(0);
            _animator.SetInteger(GetParamID(AnimatorParam.MouthFlag), 0);
            _eyeRLayerIndex = _animator.GetLayerIndex("Eye R Layer");
            _eyeLLayerIndex = _animator.GetLayerIndex("Eye L Layer");
        }
    }

    private void LateUpdate()
    {
        UpdateAutoBlink();
        UpdateAutoCheek();
    }

    private void UpdateAutoBlink()
    {
        if (!_isAutoBlink || _animator == null || _animeInfos == null)
        {
            return;
        }
        if (_isRepressAutoBlink)
        {
            _nowTime = 0f;
        }
        else if (_isUseMusicDelta)
        {
            //_nowTime += SingletonMonoBehaviour<AudioManager>.instance.fMusicDelta;
            _nowTime += Time.deltaTime;
        }
        else
        {
            _nowTime += Time.deltaTime;
        }
        float num = (_isBlink ? _autoBlinkCap : _blinkTime);
        if (_nowTime > num)
        {
            if (_isRepressAutoBlink)
            {
                _isBlink = false;
            }
            else
            {
                _isBlink = !_isBlink;
            }
            _animator.SetInteger(GetParamID(AnimatorParam.EyeLFlag), _isBlink ? 10 : 0);
            _animator.SetInteger(GetParamID(AnimatorParam.EyeRFlag), _isBlink ? 10 : 0);
            if (!_isBlink)
            {
                _blinkTime = CalcBlinkInterval();
            }
            _nowTime = 0f;
        }
    }

    private void UpdateAutoCheek()
    {
        if (_cheekRenderer == null)
        {
            return;
        }
        if (_isCheek && _isRender)
        {
            _cheekTime += Time.deltaTime;
            if (_cheekTime >= _cheekMaxTime)
            {
                _cheekTime = _cheekMaxTime;
            }
            _isCheekDraw = true;
        }
        else if (!_isRender)
        {
            _cheekTime = 0f;
            _isCheekDraw = false;
        }
        else
        {
            _cheekTime -= Time.deltaTime;
            if (_cheekTime <= 0f)
            {
                _cheekTime = 0f;
                _isCheekDraw = false;
            }
        }
    }

    public MaterialPropertyBlock GetCheekMaterialPropertyBlock(MaterialPropertyBlock materialPropertyBlock)
    {
        if (_cheekRenderer == null)
        {
            return materialPropertyBlock;
        }
        Color color = materialPropertyBlock.GetVector(kPropertyID_CharaColor);
        if (_isCheekDraw)
        {
            if (_cheekMaxTime <= 0f)
            {
                _cheekMaxTime = 0.15f;
            }
            color.a = _cheekTime / _cheekMaxTime;
        }
        else
        {
            color.a = 0f;
        }
        materialPropertyBlock.SetVector(kPropertyID_CharaColor, color);
        return materialPropertyBlock;
    }

    public void AlterUpdateAutoLip(float currentTime, LiveTimelineKeyRipSyncData lipSyncData, LiveCharaPosition chara)
    {
        if (_isLipSync && _animator != null)
        {
            int nowVowel = 0;
            if (lipSyncData.character.hasFlag(chara))
            {
                nowVowel = (int)lipSyncData.vowel;
            }
            _nowVowel = nowVowel;
            _animator.SetInteger(GetParamID(AnimatorParam.MouthFlag), _nowVowel);
            _animator.SetInteger(GetParamID(AnimatorParam.MouthSizeFlag), (!lipSyncData.IsSmallMouth()) ? 1 : 0);
            _animator.SetInteger(GetParamID(AnimatorParam.MouthSpeedFlag), lipSyncData.mouthSpeed);
        }
    }

    public void AlterUpdateFacialNew(int charaIndex, int dressId, float currentTime, int targetFps, ref FacialDataUpdateInfo updateInfo)
    {
        if (_animator == null)
        {
            return;
        }
        _isLipSync = true;
        _isAutoBlink = true;
        if (updateInfo.eyeCur != null && _currentStartFrameOfEye != updateInfo.eyeCur.frame)
        {
            _blinkRno = 0;
        }
        bool flag = false;
        Director instance = Director.instance;
        if (updateInfo.face != null)
        {
            int num = updateInfo.face.faceFlag;
            int faceSlotIdx = updateInfo.face.faceSlotIdx;
            Master3DCharaData.CharaDataSpecialFace.eCheek cheek = Master3DCharaData.CharaDataSpecialFace.eCheek.Default;
            if (LiveTimelineKeyFacialFaceData.isChrSpecialFace(num))
            {
                num = ((instance != null) ? instance.GetCharaFaceType(charaIndex, num, faceSlotIdx, out cheek) : ((_characterFacial == null) ? updateInfo.face.faceFlag : _characterFacial.GetCharaFaceType(num, faceSlotIdx, out cheek)));
                if (num != 0)
                {
                    flag = updateInfo.face.IsEyeNop();
                }
            }
            _currentFaceFlag = num;
            if (instance != null)
            {
                //_currentFaceFlag = instance.GetOverridedFacialId(charaIndex, num);
                _currentFaceFlag = num;
            }
            else if (_characterFacial != null)
            {
                _currentFaceFlag = _characterFacial.GetOverridedFacialId(num);
            }
            _animator.SetInteger(GetParamID(AnimatorParam.FaceFlag), _currentFaceFlag);
            if (updateInfo.face.IsCheekControlEnable())
            {
                _isCheek = updateInfo.face.IsCheekEnable();
            }
            else
            {
                bool flag2 = false;
                if (num >= 1 && num <= 8)
                {
                    if (instance != null)
                    {
                        flag2 = instance.GetCharaCheekFlag(charaIndex, dressId);
                    }
                    else if (_characterFacial != null)
                    {
                        flag2 = _characterFacial.GetCharaCheekFlag();
                    }
                }
                if (num == 12 || cheek == Master3DCharaData.CharaDataSpecialFace.eCheek.On || flag2)
                {
                    _isCheek = true;
                }
                else
                {
                    _isCheek = false;
                }
            }
        }
        if (updateInfo.mouth != null && !updateInfo.mouth.IsNop())
        {
            _isLipSync = false;
            _animator.SetInteger(GetParamID(AnimatorParam.MouthFlag), updateInfo.mouth.mouthFlag);
            _animator.SetInteger(GetParamID(AnimatorParam.MouthSizeFlag), updateInfo.mouth.mouthSizeFlag);
            _animator.SetInteger(GetParamID(AnimatorParam.MouthSpeedFlag), updateInfo.mouth.mouthSpeed);
        }
        if (updateInfo.eyeCur != null)
        {
            if (updateInfo.eyeCur.IsNop() || flag)
            {
                if (_currentStartFrameOfEye != updateInfo.eyeCur.frame || flag)
                {
                    _animator.SetInteger(GetParamID(AnimatorParam.EyeRFlag), 0);
                    _animator.SetInteger(GetParamID(AnimatorParam.EyeLFlag), 0);
                    _animator.SetInteger(GetParamID(AnimatorParam.EyeRSpeedFlag), 0);
                    _animator.SetInteger(GetParamID(AnimatorParam.EyeLSpeedFlag), 0);
                    _rightEyeFlag = 0;
                    _leftEyeFlag = 0;
                }
            }
            else
            {
                _isAutoBlink = false;
                if (updateInfo.eyeCur.IsBlink())
                {
                    if (_blinkRno == 0)
                    {
                        int blinkCloseEyeFlag = GetBlinkCloseEyeFlag(_animator.GetInteger(GetParamID(AnimatorParam.EyeRFlag)));
                        int blinkCloseEyeFlag2 = GetBlinkCloseEyeFlag(_animator.GetInteger(GetParamID(AnimatorParam.EyeLFlag)));
                        _animator.SetInteger(GetParamID(AnimatorParam.EyeRFlag), blinkCloseEyeFlag);
                        _animator.SetInteger(GetParamID(AnimatorParam.EyeLFlag), blinkCloseEyeFlag2);
                        _animator.SetInteger(GetParamID(AnimatorParam.EyeRSpeedFlag), updateInfo.eyeCur.eyeSpeed);
                        _animator.SetInteger(GetParamID(AnimatorParam.EyeLSpeedFlag), updateInfo.eyeCur.eyeSpeed);
                        if (_animator.GetCurrentAnimatorStateInfo(_eyeRLayerIndex).normalizedTime >= 1f && _animator.GetCurrentAnimatorStateInfo(_eyeLLayerIndex).normalizedTime >= 1f)
                        {
                            _blinkRno++;
                        }
                    }
                    else if (_blinkRno == 1)
                    {
                        _animator.SetInteger(GetParamID(AnimatorParam.EyeRFlag), updateInfo.eyeCur.eyeRFlag);
                        _animator.SetInteger(GetParamID(AnimatorParam.EyeLFlag), updateInfo.eyeCur.eyeLFlag);
                    }
                }
                else
                {
                    _blinkRno = 0;
                    _animator.SetInteger(GetParamID(AnimatorParam.EyeRFlag), updateInfo.eyeCur.eyeRFlag);
                    _animator.SetInteger(GetParamID(AnimatorParam.EyeLFlag), updateInfo.eyeCur.eyeLFlag);
                    _animator.SetInteger(GetParamID(AnimatorParam.EyeRSpeedFlag), updateInfo.eyeCur.eyeSpeed);
                    _animator.SetInteger(GetParamID(AnimatorParam.EyeLSpeedFlag), updateInfo.eyeCur.eyeSpeed);
                }
                _rightEyeFlag = updateInfo.eyeCur.eyeRFlag;
                _leftEyeFlag = updateInfo.eyeCur.eyeLFlag;
            }
        }
        _isRepressAutoBlink = false;
        if (updateInfo.eyeNext != null && !updateInfo.eyeNext.IsNop())
        {
            float num2 = (float)updateInfo.eyeNext.frame / (float)targetFps - currentTime;
            num2 = ((num2 < 0f) ? 0f : num2);
            if (num2 < 1f)
            {
                _isRepressAutoBlink = true;
            }
        }
        _currentStartFrameOfEye = ((updateInfo.eyeCur != null) ? updateInfo.eyeCur.frame : (-1));
    }

    private int GetBlinkCloseEyeFlag(int curEyeFlag)
    {
        return curEyeFlag switch
        {
            21 => 13,
            3 => 11,
            _ => 10,
        };
    }

    public void SetEyeSpeedFlag(int flag)
    {
        _animator.SetInteger(GetParamID(AnimatorParam.EyeRSpeedFlag), flag);
        _animator.SetInteger(GetParamID(AnimatorParam.EyeLSpeedFlag), flag);
    }

    public void SetMouthFlag(int flag)
    {
        _animator.SetInteger(GetParamID(AnimatorParam.MouthFlag), flag);
    }
}
