#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

/// <summary>
/// 查找缺失的翻译 - 扫描所有 LocalizedLabel 组件，找出没有翻译的 key
/// </summary>
public class FindMissingTranslations : EditorWindow
{
    private List<string> missingKeys = new List<string>();
    private Dictionary<string, string> existingTranslations = new Dictionary<string, string>();
    
    [MenuItem("Tools/查找缺失的翻译")]
    static void ShowWindow()
    {
        GetWindow<FindMissingTranslations>("查找缺失的翻译");
    }
    
    void OnGUI()
    {
        GUILayout.Label("查找缺失的翻译", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("开始扫描", GUILayout.Height(30)))
        {
            FindMissing();
        }
        
        if (GUILayout.Button("导出缺失翻译", GUILayout.Height(25)))
        {
            ExportMissing();
        }
        
        if (GUILayout.Button("添加缺失翻译到文件", GUILayout.Height(25)))
        {
            AddMissingToFile();
        }
        
        GUILayout.Space(10);
        GUILayout.Label($"找到 {missingKeys.Count} 个缺失的翻译:");
        GUILayout.Space(5);
        
        foreach (var key in missingKeys)
        {
            GUILayout.Label(key);
        }
    }
    
    void FindMissing()
    {
        missingKeys.Clear();
        existingTranslations.Clear();
        
        // 加载现有翻译
        LoadTranslations();
        
        // 扫描所有 LocalizedLabel 组件
        var allLabels = FindObjectsOfType<LocalizedLabel>(true);
        HashSet<string> checkedKeys = new HashSet<string>();
        
        foreach (var label in allLabels)
        {
            if (string.IsNullOrEmpty(label.key)) continue;
            if (checkedKeys.Contains(label.key)) continue;
            
            checkedKeys.Add(label.key);
            
            // 检查是否有翻译
            if (!existingTranslations.ContainsKey(label.key))
            {
                missingKeys.Add(label.key);
            }
        }
        
        Debug.Log($"[FindMissingTranslations] 找到 {missingKeys.Count} 个缺失的翻译");
    }
    
    void LoadTranslations()
    {
        existingTranslations.Clear();
        
        // 加载中文翻译
        var zhPath = "Assets/Resources/Localization/zh_cn.json";
        if (File.Exists(zhPath))
        {
            var json = File.ReadAllText(zhPath);
            var obj = JObject.Parse(json);
            foreach (var prop in obj.Properties())
            {
                existingTranslations[prop.Name] = prop.Value.ToString();
            }
        }
    }
    
    void ExportMissing()
    {
        if (missingKeys.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有缺失的翻译", "确定");
            return;
        }
        
        string path = EditorUtility.SaveFilePanel("保存缺失翻译", "", "missing_translations.txt", "txt");
        if (string.IsNullOrEmpty(path)) return;
        
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("# 缺失的翻译");
            writer.WriteLine("# 格式: key | 中文翻译");
            writer.WriteLine("");
            
            foreach (var key in missingKeys)
            {
                string zhTranslation = GenerateTranslation(key);
                writer.WriteLine($"{key} | {zhTranslation}");
            }
        }
        
        EditorUtility.DisplayDialog("完成", $"已导出 {missingKeys.Count} 个缺失的翻译到:\n{path}", "确定");
    }
    
    void AddMissingToFile()
    {
        if (missingKeys.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有缺失的翻译", "确定");
            return;
        }
        
        LoadTranslations();
        
        foreach (var key in missingKeys)
        {
            if (!existingTranslations.ContainsKey(key))
            {
                string zhTranslation = GenerateTranslation(key);
                existingTranslations[key] = zhTranslation;
            }
        }
        
        // 保存翻译文件
        SaveTranslations();
        
        EditorUtility.DisplayDialog("完成", $"已添加 {missingKeys.Count} 个翻译到 zh_cn.json", "确定");
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
        
        // 尝试翻译
        var translations = new Dictionary<string, string>
        {
            {"camera", "相机"},
            {"distance", "距离"},
            {"height", "高度"},
            {"rotation", "旋转"},
            {"speed", "速度"},
            {"fov", "视野"},
            {"target", "目标"},
            {"zoom", "缩放"},
            {"model", "模型"},
            {"animation", "动画"},
            {"sound", "声音"},
            {"screenshot", "截图"},
            {"environment", "环境"},
            {"characters", "角色"},
            {"props", "道具"},
            {"scenes", "场景"},
            {"other", "其他"},
            {"settings", "设置"},
            {"about", "关于"},
            {"colorset", "配色"},
            {"mob", "NPC"},
            {"mini", "Q版"},
            {"live", "演唱会"},
            {"loading", "加载中"},
            {"head", "头部"},
            {"unload", "卸载"},
            {"normal", "普通"},
            {"morph", "变形"},
            {"emotion", "表情"},
            {"clear", "清除"},
            {"gacha", "扭蛋"},
            {"export", "导出"},
            {"lyrics", "歌词"},
            {"no audio", "无音频"},
            {"physics", "物理"},
            {"enable physics", "启用物理"},
            {"fingers", "手指"},
            {"face bones", "脸部骨骼"},
            {"face override", "脸部覆盖"},
            {"materials", "材质"},
            {"rotation", "旋转"},
            {"position", "位置"},
            {"scale", "缩放"},
            {"outline width", "轮廓宽度"},
            {"transparent", "透明"},
            {"hsv", "HSV"},
            {"hex value", "十六进制值"},
            {"movement speed", "移动速度"},
            {"rotation speed", "旋转速度"},
            {"hideui", "隐藏UI"},
            {"showswip", "显示滑动"},
            {"orbit around center", "环绕中心"},
            {"use animation camera", "使用动画相机"},
            {"msaa x4 default", "MSAA x4 (默认)"},
            {"record png sequence", "录制 PNG 序列"},
            {"record gif", "录制 GIF"},
            {"take screenshot", "截图"},
            {"export model", "导出模型"},
            {"open with tpose", "以 T 姿势打开"},
            {"look at camera", "看向相机"},
            {"lock character", "锁定角色"},
            {"live character select", "演唱会角色选择"},
            {"credits", "制作人员"},
            {"control mode", "控制模式"},
            {"color set list", "配色列表"},
            {"export color code", "导出颜色代码"},
            {"copy all", "全部复制"},
            {"copy spawned", "复制已生成"},
            {"clear loaded bundle", "清除已加载资源包"},
            {"load stage", "加载舞台"},
            {"change datapath", "更改数据路径"},
            {"update database", "更新数据库"},
            {"open config", "打开配置"},
            {"unload prop", "卸载道具"},
            {"unload scene", "卸载场景"},
            {"full live mode wip", "完整演唱会模式(WIP)"},
            {"shows wip", "演出(WIP)"},
            {"redo", "重做"},
            {"undo", "撤销"},
            {"save", "保存"},
            {"default", "默认"},
            {"option a", "选项 A"},
            {"light", "光照"},
            {"lines", "线条"},
            {"mayu", "眉毛"},
            {"mouth", "嘴巴"},
            {"ear", "耳朵"},
            {"eye", "眼睛"},
            {"face", "脸部"},
            {"fingers", "手指"},
            {"rotation", "旋转"},
            {"position", "位置"},
            {"scale", "缩放"},
            {"hsv", "HSV"},
            {"hex value", "十六进制值"},
            {"movement speed", "移动速度"},
            {"rotation speed", "旋转速度"},
            {"speed 1 0", "速度: 1.0"},
            {"region", "区域"},
            {"work mode", "工作模式"},
            {"visibility", "可见性"},
            {"model viewers", "模型查看器"},
            {"global", "国际服"},
            {"gacha", "扭蛋"},
            {"load options", "加载选项"},
            {"language", "语言"},
            {"search", "搜索"},
            {"sequence fps", "序列帧率"},
            {"enter text", "输入文字"},
            {"original", "原始"},
            {"option a", "选项 A"}
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
    
    void SaveTranslations()
    {
        var zhPath = "Assets/Resources/Localization/zh_cn.json";
        var obj = new JObject();
        foreach (var kvp in existingTranslations)
        {
            obj[kvp.Key] = kvp.Value;
        }
        File.WriteAllText(zhPath, obj.ToString(Newtonsoft.Json.Formatting.Indented));
        AssetDatabase.Refresh();
    }
}
#endif
