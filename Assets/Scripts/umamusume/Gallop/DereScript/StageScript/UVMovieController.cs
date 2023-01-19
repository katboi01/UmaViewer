using Stage;
using System;
using System.Text;
using UnityEngine;
using LitJson;

/// <summary>
/// UV動画を管理する
/// 画像1枚からコマを切り出し動画にする
/// </summary>
public class UVMovieController
{
    private const short STRING_BUILDER_SIZE = 256;

    private Texture2D _mainTexture;

    private Vector2 _mainTextureOffset = default(Vector2);

    private Vector2 _mainTextureScale = default(Vector2);

    [HideInInspector]
    public string _ResouceName = string.Empty;

    //private bool _isLight;

    private UVMovieData _UVMovieData;

    private Texture2D[] _Textures;

    private bool _IsInitlize;

    private Vector2 _FrameInfoSize = default(Vector2);

    private Vector2 _FrameOffset = default(Vector2);

    private Vector2 _FrameScale = default(Vector2);

    private Vector2 _TexturePixelOffset = default(Vector2);

    private Vector2 _TexturePixelScale = default(Vector2);

    private float _MoviePlayFrameSec;

    private float _Time;

    private float _StartOffsetSec;

    private float _EndOffsetSec;

    private float _StartLoopSec;

    private float _EndLoopSec;

    private int _LoopCount;

    private float _Fps = 60f;

    private float _TotalLoopSec;

    private float _PlayStartSec;

    private bool _existMaskTex;
    
    private Texture2D[] _MaskTextures;
    
    private Texture2D _maskTexture;

    private int _charaId = 0;

    private Action<float, bool> SetupMaterial;

    public Texture2D mainTexture
    {
        get
        {
            return _mainTexture;
        }
        protected set
        {
            _mainTexture = value;
        }
    }

    public Vector2 mainTextureOffset
    {
        get
        {
            return _mainTextureOffset;
        }
        protected set
        {
            _mainTextureOffset = value;
        }
    }

    public Vector2 mainTextureScale
    {
        get
        {
            return _mainTextureScale;
        }
        protected set
        {
            _mainTextureScale = value;
        }
    }
    /*
    public bool isLight
    {
        get
        {
            return _isLight;
        }
        set
        {
            _isLight = value;
        }
    }
    */

    public bool existMaskTex => _existMaskTex;

    public Texture2D maskTexture => _maskTexture;

    public int charaId
    {
        set
        {
            _charaId = value;
        }
    }

    public void Start()
    {
        Init_Standard();
        SetupMaterial = SetupMaterial_Standard;

        if (SetupMaterial != null)
        {
            SetupMaterial(0f, false);
        }
    }

    private void Init_Standard()
    {
        if (ResourcesManager.instance == null)
        {
            return;
        }
        Director instance = Director.instance;
        StringBuilder stringBuilder = new StringBuilder(STRING_BUILDER_SIZE);
        stringBuilder.AppendFormat("3D/UVMovie/{0}/{1}", _ResouceName, _ResouceName);
        string text = stringBuilder.ToString();
        ResourcesManager instance2 = ResourcesManager.instance;
        TextAsset textAsset = instance2.LoadObject<TextAsset>(text);
        if (textAsset == null)
        {
            return;
        }
        _UVMovieData = JsonMapper.ToObject<UVMovieData>(textAsset.text);
        if (_UVMovieData == null || _UVMovieData.ImageCount == 0)
        {
            return;
        }
        _Textures = new Texture2D[_UVMovieData.ImageCount];
        _existMaskTex = _UVMovieData.ExistMaskTex;
        if (_existMaskTex)
        {
            _MaskTextures = new Texture2D[_UVMovieData.ImageCount];
        }
        for (int i = 0; i < _UVMovieData.ImageCount; i++)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            stringBuilder.AppendFormat("{0}_tex_{1:D2}_chr{2:D3}", text, i, _charaId);
            string objectName = stringBuilder.ToString();
            if (instance2.CheckLoadObject(objectName))
            {
                _Textures[i] = instance2.LoadObject<Texture2D>(objectName);
            }
            if (_Textures[i] == null)
            {
                stringBuilder.Remove(0, stringBuilder.Length);
                stringBuilder.AppendFormat("{0}_tex_{1:D2}", text, i);
                objectName = stringBuilder.ToString();
                _Textures[i] = instance2.LoadObject<Texture2D>(objectName);
            }
            if (_existMaskTex)
            {
                stringBuilder.Append("_mask");
                objectName = stringBuilder.ToString();
                _MaskTextures[i] = instance2.LoadObject<Texture2D>(objectName);
            }
        }
        if (_Textures[0].filterMode != 0)
        {
            _TexturePixelOffset.x = 1f / ((float)_Textures[0].width * 2f);
            _TexturePixelOffset.y = 1f / ((float)_Textures[0].height * 2f);
            _TexturePixelScale = _TexturePixelOffset * 2f;
        }
        else
        {
            _TexturePixelOffset.Set(0f, 0f);
            _TexturePixelScale.Set(0f, 0f);
        }
        _FrameScale.Set((float)_UVMovieData.FrameInfo.Size.x, (float)_UVMovieData.FrameInfo.Size.y);
        _FrameInfoSize.x = (float)_UVMovieData.FrameInfo.Size.x;
        _FrameInfoSize.y = (float)_UVMovieData.FrameInfo.Size.y;
        _StartOffsetSec = (float)_UVMovieData.FrameInfo.StartOffsetSec;
        _EndOffsetSec = (float)_UVMovieData.FrameInfo.EndOffsetSec;
        _MoviePlayFrameSec = (float)((double)_UVMovieData.FrameInfo.Count / _UVMovieData.FrameInfo.Fps);
        _StartLoopSec = (float)_UVMovieData.FrameInfo.StartLoopSec;
        _EndLoopSec = (float)_UVMovieData.FrameInfo.EndLoopSec;
        if (_EndLoopSec == 0f)
        {
            _EndLoopSec = _MoviePlayFrameSec;
        }
        _LoopCount = _UVMovieData.FrameInfo.LoopCount;
        _Fps = (float)_UVMovieData.FrameInfo.Fps;
        _TotalLoopSec = (float)_LoopCount * (_EndLoopSec - _StartLoopSec);
        _Time = 0f;
        _IsInitlize = true;
    }

    public bool IsPlaying(float time)
    {
        if (time < 0f)
        {
            return false;
        }
        float num;
        if (_UVMovieData.FrameInfo.IsLoop)
        {
            if (_LoopCount == 0)
            {
                return true;
            }
            num = _StartOffsetSec + _EndOffsetSec + _TotalLoopSec;
        }
        else
        {
            num = _StartOffsetSec + _EndOffsetSec + _MoviePlayFrameSec;
        }
        return time <= num;
    }

    private void SetupMaterial_Standard(float settime, bool isReversePlay)
    {
        if (_IsInitlize)
        {
            _Time = settime;
            if (_PlayStartSec > 0f)
            {
                _Time += _PlayStartSec;
            }
            if (_Time < _StartOffsetSec)
            {
                _Time = 0f;
            }
            else
            {
                _Time -= _StartOffsetSec;
            }
            float num = (isReversePlay ? (_MoviePlayFrameSec - _EndLoopSec) : _StartLoopSec);
            float num2 = (isReversePlay ? (_MoviePlayFrameSec - _StartLoopSec) : _EndLoopSec);
            if (_UVMovieData.FrameInfo.IsLoop)
            {
                if (_LoopCount > 0)
                {
                    if (_Time > num2)
                    {
                        _Time -= num2;
                        if (_Time > _TotalLoopSec)
                        {
                            _Time -= _TotalLoopSec;
                        }
                        else
                        {
                            _Time %= num2 - num;
                        }
                        _Time += num;
                    }
                }
                else if (_Time > num2)
                {
                    _Time -= num2;
                    _Time %= num2 - num;
                    _Time += num;
                }
            }
            else if (_Time > _MoviePlayFrameSec)
            {
                _Time = _MoviePlayFrameSec;
            }
            int num3 = (int)((isReversePlay ? (_MoviePlayFrameSec - _Time) : _Time) * _Fps);
            if (num3 >= _UVMovieData.FrameInfo.Count)
            {
                num3 = _UVMovieData.FrameInfo.Count - 1;
            }
            int num4 = num3 / _UVMovieData.FremePerImage;
            num3 -= num4 * _UVMovieData.FremePerImage;
            int num5 = num3 % _UVMovieData.FremePerWidth;
            int num6 = num3 / _UVMovieData.FremePerWidth;
            _FrameOffset.x = (float)num5 * _FrameInfoSize.x;
            _FrameOffset.y = (float)num6 * _FrameInfoSize.y;
            _mainTexture = _Textures[num4];
            _mainTextureOffset = _FrameOffset + _TexturePixelOffset;
            _mainTextureScale = _FrameScale - _TexturePixelScale;
            if (_existMaskTex)
            {
                _maskTexture = _MaskTextures[num4];
            }
        }
    }

    public void UpdateAddTime(float addtime, bool isReversePlay)
    {
        if (SetupMaterial != null)
        {
            SetupMaterial(_Time + addtime, isReversePlay);
        }
    }

    public void UpdateSetTime(float settime, bool isReversePlay)
    {
        if (SetupMaterial != null)
        {
            SetupMaterial(settime, isReversePlay);
        }
    }

    /// <summary>
    /// アニメーションの開始時間をセット
    /// </summary>
    public void SetPlayStartSec(int startOffsetFrame)
    {
        if (_UVMovieData != null && _UVMovieData.FrameInfo != null)
        {
            _PlayStartSec = (float)((double)startOffsetFrame / _UVMovieData.FrameInfo.Fps);
        }
    }
}
