using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// 本地化管理器 - 加载 JSON 翻译文件，提供翻译查询
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }
    
    /// <summary>
    /// 语言切换事件，所有 LocalizedLabel 订阅此事件
    /// </summary>
    public static event Action OnLanguageChanged;
    
    private Dictionary<string, string> currentTranslations = new Dictionary<string, string>();
    private Dictionary<string, string> characterNames = new Dictionary<string, string>();
    private Language currentLanguage = Language.En;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Start()
    {
        // 初始化时加载当前语言
        if (Config.Instance != null)
        {
            SetLanguage(Config.Instance.Language);
        }
    }
    
    /// <summary>
    /// 设置语言并触发刷新
    /// </summary>
    public void SetLanguage(Language language)
    {
        currentLanguage = language;
        LoadTranslations(language);
        
        // 始终加载角色名映射（不受全局语言影响，由CharacterNameLanguage控制）
        if (characterNames.Count == 0)
        {
            LoadCharacterNames();
        }
        
        // 触发事件，通知所有 LocalizedLabel 刷新
        OnLanguageChanged?.Invoke();
        
        Debug.Log($"[Localization] 语言已设置为: {language}，加载了 {currentTranslations.Count} 条翻译");
    }
    
    /// <summary>
    /// 从 Resources/Localization/ 目录加载 JSON 翻译文件
    /// </summary>
    void LoadTranslations(Language language)
    {
        currentTranslations.Clear();
        
        string fileName = language switch
        {
            Language.ZhCn => "Localization/zh_cn",
            Language.En => "Localization/en",
            _ => "Localization/en"
        };
        
        TextAsset jsonAsset = Resources.Load<TextAsset>(fileName);
        if (jsonAsset == null)
        {
            Debug.LogError($"[Localization] 无法加载翻译文件: Resources/{fileName}.json");
            return;
        }
        
        try
        {
            var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonAsset.text);
            if (parsed != null)
            {
                foreach (var kvp in parsed)
                {
                    currentTranslations[kvp.Key] = kvp.Value;
                }
            }
            Debug.Log($"[Localization] 从 {fileName}.json 加载了 {currentTranslations.Count} 条翻译");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Localization] 解析翻译文件失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 加载角色中文名映射
    /// </summary>
    void LoadCharacterNames()
    {
        characterNames.Clear();
        
        TextAsset jsonAsset = Resources.Load<TextAsset>("Localization/character_names_zh");
        if (jsonAsset == null)
        {
            Debug.LogWarning("[Localization] 角色名映射文件不存在: Localization/character_names_zh.json");
            return;
        }
        
        try
        {
            var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonAsset.text);
            if (parsed != null)
            {
                foreach (var kvp in parsed)
                {
                    characterNames[kvp.Key] = kvp.Value;
                }
            }
            Debug.Log($"[Localization] 加载了 {characterNames.Count} 个角色名映射");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Localization] 解析角色名映射失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 获取翻译文本（静态便捷方法）
    /// </summary>
    public static string Get(string key)
    {
        if (Instance == null)
        {
            // 尝试查找或创建实例
            Instance = FindObjectOfType<LocalizationManager>();
            if (Instance == null)
            {
                var go = new GameObject("LocalizationManager");
                Instance = go.AddComponent<LocalizationManager>();
                DontDestroyOnLoad(go);
            }
        }
        
        if (Instance.currentTranslations.TryGetValue(key, out string value))
        {
            return value;
        }
        
        // Debug.LogWarning($"[Localization] 未找到翻译: {key}");
        return key;
    }
    
    /// <summary>
    /// 获取翻译文本（带格式化参数）
    /// </summary>
    public static string Get(string key, params object[] args)
    {
        string template = Get(key);
        try
        {
            return string.Format(template, args);
        }
        catch (FormatException)
        {
            Debug.LogWarning($"[Localization] 格式化失败: {key}");
            return template;
        }
    }
    
    /// <summary>
    /// 获取角色名（根据角色名语言设置）
    /// </summary>
    /// <param name="charaId">角色ID</param>
    /// <param name="defaultName">默认名称（通常是日文）</param>
    /// <param name="enName">英文名称（可选）</param>
    /// <returns>根据设置返回对应语言的角色名</returns>
    public static string GetCharacterName(string charaId, string defaultName, string enName = "")
    {
        if (Instance == null) return defaultName;
        
        // 获取角色名语言设置
        CharacterNameLanguage nameLang = CharacterNameLanguage.Japanese;
        if (Config.Instance != null)
        {
            nameLang = Config.Instance.CharacterNameLanguage;
        }
        
        switch (nameLang)
        {
            case CharacterNameLanguage.Chinese:
                // 返回中文名
                if (Instance.characterNames.TryGetValue(charaId, out string zhName))
                {
                    return zhName;
                }
                break;
                
            case CharacterNameLanguage.English:
                // 返回英文名
                if (!string.IsNullOrEmpty(enName))
                {
                    return enName;
                }
                break;
                
            case CharacterNameLanguage.Japanese:
            default:
                // 返回日文名（defaultName）
                break;
        }
        
        return defaultName;
    }
}