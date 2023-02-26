using UnityEngine;

/// <summary>
/// 水の落下を表現する
/// Sunshine See Mayより追加
/// </summary>
public class Waterfall : MonoBehaviour
{
    public const string NAME_SHADER = "Cygames/3DLive/Stage/StageWaterfall";

    public const int INVALID_ID = -1;

    public const float SCROLL_SPEED = 0.01f;

    private Renderer _renderer;

    private MaterialPropertyBlock _propBlock;

    //シェーダプロパティID

    private int _offsetId_0 = INVALID_ID;

    private int _offsetId_1 = INVALID_ID;

    private int _pendulumId_0 = INVALID_ID;

    private int _pendulumId_1 = INVALID_ID;

    private int _lerpAlphaId = INVALID_ID;

    /// <summary>
    /// diffuseテクスチャのスクロール位置
    /// </summary>
    private float _offset_0;

    /// <summary>
    /// Alphaテクスチャのスクロール位置
    /// </summary>
    private float _offset_1;

    [SerializeField]
    [Range(0.01f, 100f)]
    private float diffuseScroll = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    private float diffusePendulum;

    [SerializeField]
    [Range(0.01f, 100f)]
    private float alphaScroll = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    private float alphaPendulum;

    [SerializeField]
    [Range(0f, 1f)]
    private float lerpAlpha;

    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _propBlock = new MaterialPropertyBlock();
        if ((bool)_renderer)
        {
            _offsetId_0 = Shader.PropertyToID("_offset_0");
            _offsetId_1 = Shader.PropertyToID("_offset_1");
            _pendulumId_0 = Shader.PropertyToID("_pendulum_0");
            _pendulumId_1 = Shader.PropertyToID("_pendulum_1");
            _lerpAlphaId = Shader.PropertyToID("_lerpAlpha");
        }
    }
    
    private void Update()
    {
        if(_renderer != null)
        {
            if(_propBlock != null)
            {
                if(Director.instance != null && !Director.instance.IsPauseLive())
                {
                    //水が流れるのでScroll値を更新
                    float diffuseScrollValue = diffuseScroll * SCROLL_SPEED + _offset_0;
                    _offset_0 = diffuseScrollValue;
                    float alphaScrollValue = alphaScroll * SCROLL_SPEED + _offset_1;
                    _offset_1 = alphaScrollValue;

                    //値をループさせる
                    if(diffuseScrollValue >= 1.0f)
                    {
                        _offset_0 = 0f;
                    }
                    if (alphaScrollValue >= 1.0f)
                    {
                        _offset_1 = 0f;
                    }

                    //時間から値を算出
                    float sin = Mathf.Sin(Time.time);
                    var diff = sin * diffusePendulum;
                    var alph = sin * alphaPendulum;

                    _propBlock.SetFloat(_offsetId_0, _offset_0);
                    _propBlock.SetFloat(_offsetId_1, _offset_1);

                    _propBlock.SetFloat(_pendulumId_0, diff);
                    _propBlock.SetFloat(_pendulumId_1, alph);

                    _propBlock.SetFloat(_lerpAlphaId, lerpAlpha);

                    _renderer.SetPropertyBlock(_propBlock);
                }
            }
        }
    }
}
