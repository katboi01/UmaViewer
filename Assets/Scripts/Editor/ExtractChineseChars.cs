#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 提取中文字符集 - 用于生成 Font Asset Creator 所需的字符集文件
/// </summary>
public class ExtractChineseChars
{
    [MenuItem("Tools/提取中文字符集")]
    static void Extract()
    {
        var zhPath = "Assets/Resources/Localization/zh_cn.json";
        if (!File.Exists(zhPath))
        {
            EditorUtility.DisplayDialog("错误", "找不到翻译文件: " + zhPath, "确定");
            return;
        }
        
        var json = File.ReadAllText(zhPath);
        var obj = JObject.Parse(json);

        var chars = new HashSet<char>();
        foreach (var prop in obj.Properties())
        {
            foreach (char c in prop.Value.ToString())
            {
                if (c >= 0x4E00 && c <= 0x9FFF)  // CJK 基本区
                    chars.Add(c);
                else if (c >= 0x3400 && c <= 0x4DBF)  // CJK 扩展 A
                    chars.Add(c);
                else if (c >= 0xFF00 && c <= 0xFFEF)  // 全角字符
                    chars.Add(c);
                else if (c >= 0x3000 && c <= 0x303F)  // CJK 标点
                    chars.Add(c);
            }
        }

        var sb = new StringBuilder();
        foreach (var c in chars.OrderBy(c => c))
            sb.Append(c);

        var outPath = "Assets/Fonts/ChineseChars.txt";
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        
        Debug.Log($"[ExtractChineseChars] 提取了 {chars.Count} 个中文字符到 {outPath}");
        EditorUtility.DisplayDialog("完成", $"已提取 {chars.Count} 个中文字符到:\n{outPath}", "确定");
    }
}
#endif
