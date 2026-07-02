#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 扫描所有英文文字 - 列出场景中所有英文内容
/// </summary>
public class ScanAllEnglishText : EditorWindow
{
    private List<string> englishTexts = new List<string>();
    private Vector2 scrollPos;
    
    [MenuItem("Tools/扫描所有英文文字")]
    static void ShowWindow()
    {
        GetWindow<ScanAllEnglishText>("扫描所有英文文字");
    }
    
    void OnGUI()
    {
        GUILayout.Label("扫描所有英文文字", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("开始扫描", GUILayout.Height(30)))
        {
            ScanAllTexts();
        }
        
        if (GUILayout.Button("导出到文件", GUILayout.Height(25)))
        {
            ExportToFile();
        }
        
        GUILayout.Space(10);
        GUILayout.Label($"找到 {englishTexts.Count} 个英文文字:");
        GUILayout.Space(5);
        
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        foreach (var text in englishTexts)
        {
            GUILayout.Label(text);
        }
        GUILayout.EndScrollView();
    }
    
    void ScanAllTexts()
    {
        englishTexts.Clear();
        HashSet<string> uniqueTexts = new HashSet<string>();
        
        // 扫描所有 Text 组件
        var allTexts = FindObjectsOfType<Text>(true);
        foreach (var text in allTexts)
        {
            if (!string.IsNullOrEmpty(text.text) && IsEnglish(text.text))
            {
                if (uniqueTexts.Add(text.text))
                {
                    englishTexts.Add(text.text);
                }
            }
        }
        
        // 扫描所有 TextMeshProUGUI 组件
        var allTmpTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var tmp in allTmpTexts)
        {
            if (!string.IsNullOrEmpty(tmp.text) && IsEnglish(tmp.text))
            {
                if (uniqueTexts.Add(tmp.text))
                {
                    englishTexts.Add(tmp.text);
                }
            }
        }
        
        // 排序
        englishTexts.Sort();
        
        Debug.Log($"[ScanAllEnglishText] 找到 {englishTexts.Count} 个英文文字");
    }
    
    bool IsEnglish(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        if (text.Length < 2) return false;
        if (text.Contains("\n")) return false;
        
        // 检查是否包含英文字母
        foreach (char c in text)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                return true;
            }
        }
        return false;
    }
    
    void ExportToFile()
    {
        if (englishTexts.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "请先扫描", "确定");
            return;
        }
        
        string path = EditorUtility.SaveFilePanel("保存翻译列表", "", "english_texts.txt", "txt");
        if (string.IsNullOrEmpty(path)) return;
        
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("# 英文文字列表");
            writer.WriteLine("# 格式: 英文 | 中文翻译");
            writer.WriteLine("");
            
            foreach (var text in englishTexts)
            {
                writer.WriteLine($"{text} | ");
            }
        }
        
        EditorUtility.DisplayDialog("完成", $"已导出 {englishTexts.Count} 个英文文字到:\n{path}", "确定");
        Debug.Log($"[ScanAllEnglishText] 已导出到 {path}");
    }
}
#endif
