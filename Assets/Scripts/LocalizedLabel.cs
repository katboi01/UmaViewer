using UnityEngine;
using TMPro;

/// <summary>
/// 本地化标签组件 - 用于场景中的静态UI文字翻译
/// 通过 key 绑定本地化键，订阅 OnLanguageChanged 事件自动刷新
/// </summary>
public class LocalizedLabel : MonoBehaviour
{
    [Tooltip("本地化键名，如 tab_model、tab_character")]
    public string key;
    
    private UnityEngine.UI.Text uiText;
    private TextMeshProUGUI tmpText;
    
    /// <summary>
    /// Awake 中缓存组件引用，比 Start 更早执行
    /// </summary>
    void Awake()
    {
        uiText = GetComponent<UnityEngine.UI.Text>();
        tmpText = GetComponent<TextMeshProUGUI>();
    }
    
    void Start()
    {
        // 订阅语言切换事件
        LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        
        // 延迟刷新，确保LocalizationManager已加载翻译
        // 增加延迟时间到0.5秒，确保LocalizationManager完全初始化
        Invoke("DelayedRefresh", 0.5f);
    }
    
    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
    }
    
    /// <summary>
    /// 延迟刷新 - 等待LocalizationManager初始化完成
    /// </summary>
    void DelayedRefresh()
    {
        Refresh();
        
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning($"[LocalizedLabel] {gameObject.name} 的 key 为空，请在 Inspector 中设置");
        }
        else
        {
            Debug.Log($"[LocalizedLabel] {gameObject.name} 延迟刷新完成，key: {key}，翻译: {LocalizationManager.Get(key)}");
        }
    }
    
    /// <summary>
    /// 语言切换事件回调
    /// </summary>
    void OnLanguageChanged()
    {
        Refresh();
    }
    
    /// <summary>
    /// 刷新文字显示
    /// </summary>
    public void Refresh()
    {
        if (string.IsNullOrEmpty(key)) return;
        
        string translatedText = LocalizationManager.Get(key);
        
        if (uiText != null)
        {
            uiText.text = translatedText;
        }
        
        if (tmpText != null)
        {
            tmpText.text = translatedText;
        }
    }
    
    /// <summary>
    /// 强制刷新 - 供外部兜底调用
    /// 不依赖事件触发，重新获取组件引用后刷新
    /// </summary>
    public void ForceRefresh()
    {
        // 重新获取引用（防止组件在 Awake 之后才挂载）
        if (uiText == null) uiText = GetComponent<UnityEngine.UI.Text>();
        if (tmpText == null) tmpText = GetComponent<TextMeshProUGUI>();
        Refresh();
    }
    
    /// <summary>
    /// 设置新的 key 并刷新
    /// </summary>
    public void SetKey(string newKey)
    {
        key = newKey;
        Refresh();
    }
}