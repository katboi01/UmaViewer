#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

/// <summary>
/// 测试本地化 - 检查翻译文件和字体问题
/// </summary>
public class TestLocalization : EditorWindow
{
    private Vector2 scrollPos;
    private List<string> missingKeys = new List<string>();
    private List<string> untranslatedKeys = new List<string>();
    
    [MenuItem("Tools/测试本地化")]
    static void ShowWindow()
    {
        GetWindow<TestLocalization>("测试本地化");
    }
    
    void OnGUI()
    {
        GUILayout.Label("测试本地化", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("检查翻译文件", GUILayout.Height(30)))
        {
            CheckTranslations();
        }
        
        if (GUILayout.Button("检查字体问题", GUILayout.Height(30)))
        {
            CheckFontIssues();
        }
        
        if (GUILayout.Button("添加缺失翻译", GUILayout.Height(30)))
        {
            AddMissingTranslations();
        }
        
        GUILayout.Space(10);
        
        if (missingKeys.Count > 0)
        {
            GUILayout.Label($"缺失的翻译 ({missingKeys.Count}):", EditorStyles.boldLabel);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            foreach (var key in missingKeys)
            {
                GUILayout.Label(key);
            }
            GUILayout.EndScrollView();
        }
        
        if (untranslatedKeys.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label($"未翻译的 key ({untranslatedKeys.Count}):", EditorStyles.boldLabel);
            foreach (var key in untranslatedKeys)
            {
                GUILayout.Label(key);
            }
        }
    }
    
    void CheckTranslations()
    {
        missingKeys.Clear();
        untranslatedKeys.Clear();
        
        var zhPath = "Assets/Resources/Localization/zh_cn.json";
        if (!File.Exists(zhPath))
        {
            Debug.LogError("[TestLocalization] 找不到翻译文件: " + zhPath);
            return;
        }
        
        var json = File.ReadAllText(zhPath);
        var obj = JObject.Parse(json);
        
        // 扫描所有 LocalizedLabel 组件
        var allLabels = FindObjectsOfType<LocalizedLabel>(true);
        HashSet<string> checkedKeys = new HashSet<string>();
        
        foreach (var label in allLabels)
        {
            if (string.IsNullOrEmpty(label.key)) continue;
            if (checkedKeys.Contains(label.key)) continue;
            
            checkedKeys.Add(label.key);
            
            // 检查是否有翻译
            if (!obj.ContainsKey(label.key))
            {
                missingKeys.Add(label.key);
            }
            else
            {
                // 检查翻译是否就是 key 本身（未翻译）
                string translation = obj[label.key].ToString();
                if (translation == label.key)
                {
                    untranslatedKeys.Add(label.key);
                }
            }
        }
        
        Debug.Log($"[TestLocalization] 检查完成：缺失 {missingKeys.Count} 个，未翻译 {untranslatedKeys.Count} 个");
    }
    
    void CheckFontIssues()
    {
        Debug.Log("[TestLocalization] 检查字体问题...");
        
        // 检查是否有中文字体
        var fontFiles = Directory.GetFiles("Assets/Fonts", "*.asset", SearchOption.AllDirectories);
        bool hasChineseFont = false;
        
        foreach (var fontFile in fontFiles)
        {
            if (fontFile.Contains("Chinese") || fontFile.Contains("Noto") || fontFile.Contains("SourceHan"))
            {
                hasChineseFont = true;
                Debug.Log($"[TestLocalization] 找到中文字体: {fontFile}");
            }
        }
        
        if (!hasChineseFont)
        {
            Debug.LogWarning("[TestLocalization] 未找到中文字体！需要生成中文字体图集。");
        }
        
        // 检查 ChineseChars.txt 是否存在
        if (!File.Exists("Assets/Fonts/ChineseChars.txt"))
        {
            Debug.LogWarning("[TestLocalization] ChineseChars.txt 不存在！请运行 Tools → 提取中文字符集");
        }
        else
        {
            var chars = File.ReadAllText("Assets/Fonts/ChineseChars.txt");
            Debug.Log($"[TestLocalization] ChineseChars.txt 包含 {chars.Length} 个字符");
        }
    }
    
    void AddMissingTranslations()
    {
        if (missingKeys.Count == 0 && untranslatedKeys.Count == 0)
        {
            Debug.Log("[TestLocalization] 没有需要添加的翻译");
            return;
        }
        
        var zhPath = "Assets/Resources/Localization/zh_cn.json";
        var json = File.ReadAllText(zhPath);
        var obj = JObject.Parse(json);
        
        int addedCount = 0;
        
        // 添加缺失的翻译
        foreach (var key in missingKeys)
        {
            if (!obj.ContainsKey(key))
            {
                string translation = GenerateTranslation(key);
                obj[key] = translation;
                addedCount++;
            }
        }
        
        // 更新未翻译的 key
        foreach (var key in untranslatedKeys)
        {
            if (obj.ContainsKey(key))
            {
                string translation = GenerateTranslation(key);
                obj[key] = translation;
                addedCount++;
            }
        }
        
        File.WriteAllText(zhPath, obj.ToString(Newtonsoft.Json.Formatting.Indented));
        AssetDatabase.Refresh();
        
        Debug.Log($"[TestLocalization] 添加了 {addedCount} 个翻译");
        EditorUtility.DisplayDialog("完成", $"添加了 {addedCount} 个翻译", "确定");
    }
    
    string GenerateTranslation(string key)
    {
        // 移除 ui. 前缀
        string text = key;
        if (text.StartsWith("ui."))
        {
            text = text.Substring(3);
        }
        
        // 替换下划线为空格
        text = text.Replace("_", " ");
        
        // 常见翻译
        var translations = new Dictionary<string, string>
        {
            {"0 50", "0-50"},
            {"0 2", "0-2"},
            {"0 1", "0-1"},
            {"512", "512"},
            {"1", "1"},
            {"unload 1", "卸载"},
            {"unload 2", "卸载"},
            {"transparent 1", "透明"},
            {"enter text 1", "输入文字..."},
            {"enter text 2", "输入文字..."},
            {"enter text 3", "输入文字..."},
            {"enter text", "输入文字..."},
            {"no audio 1", "无音频"},
            {"no audio", "无音频"},
            {"option a 7", "选项 A"},
            {"option a", "选项 A"},
            {"record png sequence", "录制 PNG 序列"},
            {"record gif", "录制 GIF"},
            {"take screenshot", "截图"},
            {"export model", "导出模型"},
            {"open with tpose", "以 T 姿势打开"},
            {"look at camera", "看向相机"},
            {"lock character", "锁定角色"},
            {"enable physics", "启用物理"},
            {"face override", "脸部覆盖"},
            {"outline width", "轮廓宽度"},
            {"materials", "材质"},
            {"copy all", "全部复制"},
            {"copy spawned", "复制已生成"},
            {"export", "导出"},
            {"lyrics", "歌词"},
            {"original", "原始"},
            {"sequence fps", "序列帧率"},
            {"head", "头部"},
            {"normal", "普通"},
            {"unload", "卸载"},
            {"gacha", "扭蛋"},
            {"transparent", "透明"},
            {"msaa x4 default", "MSAA x4 (默认)"},
            {"hideui", "隐藏UI"},
            {"showswip", "显示滑动"},
            {"orbit around center", "环绕中心"},
            {"use animation camera", "使用动画相机"},
            {"camera distance", "相机距离"},
            {"target height", "目标高度"},
            {"zoom speed", "缩放速度"},
            {"camera speed", "相机速度"},
            {"fov", "视野"},
            {"camera height", "相机高度"},
            {"camera rotation", "相机旋转"}
        };
        
        // 尝试直接翻译
        if (translations.ContainsKey(text.ToLower()))
        {
            return translations[text.ToLower()];
        }
        
        // 尝试单词翻译
        string result = text;
        foreach (var kvp in translations)
        {
            result = result.Replace(kvp.Key, kvp.Value);
        }
        
        return result;
    }
}
#endif
