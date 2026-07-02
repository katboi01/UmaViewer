#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// 查找缺失的中文字符 - 检查字体图集是否包含所有需要的中文字符
/// </summary>
public class FindMissingChars : EditorWindow
{
    private TMP_FontAsset targetFont;
    private List<char> missingChars = new List<char>();
    
    [MenuItem("Tools/查找缺失的中文字符")]
    static void ShowWindow()
    {
        GetWindow<FindMissingChars>("查找缺失的中文字符");
    }
    
    void OnGUI()
    {
        GUILayout.Label("查找缺失的中文字符", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("目标字体", targetFont, typeof(TMP_FontAsset), false);
        
        if (GUILayout.Button("检查缺失字符", GUILayout.Height(30)))
        {
            CheckMissingChars();
        }
        
        if (GUILayout.Button("导出缺失字符到文件", GUILayout.Height(25)))
        {
            ExportMissingChars();
        }
        
        if (GUILayout.Button("添加缺失字符到字体", GUILayout.Height(25)))
        {
            AddMissingCharsToFont();
        }
        
        GUILayout.Space(10);
        
        if (missingChars.Count > 0)
        {
            GUILayout.Label($"找到 {missingChars.Count} 个缺失的字符:", EditorStyles.boldLabel);
            GUILayout.Label(new string(missingChars.ToArray()));
        }
    }
    
    void CheckMissingChars()
    {
        if (targetFont == null)
        {
            // 尝试自动查找 MSYH SDF 字体
            var fontGuids = AssetDatabase.FindAssets("MSYH SDF t:TMP_FontAsset");
            if (fontGuids.Length > 0)
            {
                var fontPath = AssetDatabase.GUIDToAssetPath(fontGuids[0]);
                targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            }
        }
        
        if (targetFont == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择一个字体资源", "确定");
            return;
        }
        
        missingChars.Clear();
        
        // 加载 ChineseChars.txt
        var charsPath = "Assets/Fonts/ChineseChars.txt";
        if (!File.Exists(charsPath))
        {
            EditorUtility.DisplayDialog("错误", "找不到 ChineseChars.txt，请先运行 Tools > 提取中文字符集", "确定");
            return;
        }
        
        var allChars = File.ReadAllText(charsPath);
        
        // 检查每个字符是否在字体图集中
        foreach (char c in allChars)
        {
            // 检查字符是否在字体的字符表中
            if (targetFont.characterTable != null)
            {
                bool found = false;
                foreach (var character in targetFont.characterTable)
                {
                    if (character.unicode == (uint)c)
                    {
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    missingChars.Add(c);
                }
            }
        }
        
        Debug.Log($"[FindMissingChars] 检查完成：共 {allChars.Length} 个字符，缺失 {missingChars.Count} 个");
        
        if (missingChars.Count > 0)
        {
            Debug.Log($"[FindMissingChars] 缺失的字符: {new string(missingChars.ToArray())}");
        }
    }
    
    void ExportMissingChars()
    {
        if (missingChars.Count == 0)
        {
            CheckMissingChars();
        }
        
        if (missingChars.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有缺失的字符", "确定");
            return;
        }
        
        var outPath = "Assets/Fonts/MissingChars.txt";
        File.WriteAllText(outPath, new string(missingChars.ToArray()));
        AssetDatabase.Refresh();
        
        Debug.Log($"[FindMissingChars] 已导出 {missingChars.Count} 个缺失字符到 {outPath}");
        EditorUtility.DisplayDialog("完成", $"已导出 {missingChars.Count} 个缺失字符到:\n{outPath}", "确定");
    }
    
    void AddMissingCharsToFont()
    {
        if (targetFont == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择一个字体资源", "确定");
            return;
        }
        
        if (missingChars.Count == 0)
        {
            CheckMissingChars();
        }
        
        if (missingChars.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有缺失的字符", "确定");
            return;
        }
        
        // 这里只是提示用户手动操作
        EditorUtility.DisplayDialog("提示", 
            $"找到 {missingChars.Count} 个缺失的字符:\n{new string(missingChars.ToArray())}\n\n" +
            "请手动添加到字体图集:\n" +
            "1. 选中字体资源\n" +
            "2. Window > TextMeshPro > Font Asset Creator\n" +
            "3. Character Set > Custom Characters\n" +
            "4. 粘贴缺失的字符\n" +
            "5. Generate Font Atlas", "确定");
    }
}
#endif
