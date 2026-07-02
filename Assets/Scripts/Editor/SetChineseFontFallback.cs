#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// 设置中文字体 Fallback - 自动为所有 TMP 字体添加中文字体作为 Fallback
/// </summary>
public class SetChineseFontFallback : EditorWindow
{
    [MenuItem("Tools/设置中文字体 Fallback")]
    static void ShowWindow()
    {
        GetWindow<SetChineseFontFallback>("设置中文字体 Fallback");
    }
    
    void OnGUI()
    {
        GUILayout.Label("设置中文字体 Fallback", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("此工具会自动为所有 TMP 字体资源添加中文字体作为 Fallback。");
        GUILayout.Label("这样原有文字不受影响，遇到中文字符时自动回退到中文字体。");
        GUILayout.Space(10);
        
        if (GUILayout.Button("自动设置 Fallback", GUILayout.Height(30)))
        {
            SetFallback();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("检查字体状态", GUILayout.Height(30)))
        {
            CheckFonts();
        }
    }
    
    void SetFallback()
    {
        // 查找中文字体
        var chineseFontPath = "Assets/Scenes/Fonts/MSYH SDF.asset";
        if (!File.Exists(chineseFontPath))
        {
            chineseFontPath = "Assets/Scenes/Fonts/ChineseFont.asset";
        }
        
        if (!File.Exists(chineseFontPath))
        {
            EditorUtility.DisplayDialog("错误", "找不到中文字体文件！\n请确认 Assets/Scenes/Fonts/ 目录下有中文字体。", "确定");
            return;
        }
        
        var chineseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(chineseFontPath);
        if (chineseFont == null)
        {
            EditorUtility.DisplayDialog("错误", "无法加载中文字体: " + chineseFontPath, "确定");
            return;
        }
        
        Debug.Log($"[SetChineseFontFallback] 找到中文字体: {chineseFontPath}");
        
        // 查找所有 TMP 字体资源
        var fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        int updatedCount = 0;
        
        foreach (var guid in fontGuids)
        {
            var fontPath = AssetDatabase.GUIDToAssetPath(guid);
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            
            if (fontAsset == null) continue;
            if (fontAsset == chineseFont) continue; // 跳过中文字体本身
            
            // 检查是否已经有 Fallback
            bool hasFallback = false;
            
            // 使用 fallbackFontAssetTable (TextMeshPro 3.x+)
            if (fontAsset.fallbackFontAssetTable != null)
            {
                foreach (var fallback in fontAsset.fallbackFontAssetTable)
                {
                    if (fallback == chineseFont)
                    {
                        hasFallback = true;
                        break;
                    }
                }
            }
            
            if (!hasFallback)
            {
                // 添加 Fallback
                if (fontAsset.fallbackFontAssetTable == null)
                {
                    fontAsset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                }
                fontAsset.fallbackFontAssetTable.Add(chineseFont);
                
                EditorUtility.SetDirty(fontAsset);
                updatedCount++;
                
                Debug.Log($"[SetChineseFontFallback] 为 {fontPath} 添加了中文字体 Fallback");
            }
        }
        
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[SetChineseFontFallback] 完成！更新了 {updatedCount} 个字体资源");
        EditorUtility.DisplayDialog("完成", $"已为 {updatedCount} 个字体资源添加中文字体 Fallback", "确定");
    }
    
    void CheckFonts()
    {
        var fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        int totalFonts = 0;
        int fontsWitchFallback = 0;
        
        foreach (var guid in fontGuids)
        {
            var fontPath = AssetDatabase.GUIDToAssetPath(guid);
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            
            if (fontAsset == null) continue;
            
            totalFonts++;
            
            if (fontAsset.fallbackFontAssetTable != null && fontAsset.fallbackFontAssetTable.Count > 0)
            {
                fontsWitchFallback++;
                Debug.Log($"[SetChineseFontFallback] {fontPath} 有 {fontAsset.fallbackFontAssetTable.Count} 个 Fallback");
            }
            else
            {
                Debug.LogWarning($"[SetChineseFontFallback] {fontPath} 没有 Fallback");
            }
        }
        
        Debug.Log($"[SetChineseFontFallback] 检查完成：共 {totalFonts} 个字体，{fontsWitchFallback} 个有 Fallback");
        EditorUtility.DisplayDialog("检查结果", $"共 {totalFonts} 个字体资源\n{fontsWitchFallback} 个有 Fallback\n{totalFonts - fontsWitchFallback} 个没有 Fallback", "确定");
    }
}
#endif
