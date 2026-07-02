#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// 清理重复翻译 Key - 删除带数字后缀的重复项
/// </summary>
public class CleanDuplicateKeys
{
    [MenuItem("Tools/清理重复翻译 Key")]
    static void Clean()
    {
        var zhPath = "Assets/Resources/Localization/zh_cn.json";
        if (!File.Exists(zhPath))
        {
            EditorUtility.DisplayDialog("错误", "找不到翻译文件: " + zhPath, "确定");
            return;
        }
        
        var json = File.ReadAllText(zhPath);
        var obj = JObject.Parse(json);
        int originalCount = obj.Count;

        // 收集 base key → value
        var clean = new JObject();
        var seen = new Dictionary<string, string>();

        foreach (var prop in obj.Properties().OrderBy(p => p.Name))
        {
            // 去掉末尾的 _数字 后缀
            var baseName = Regex.Replace(prop.Name, @"_\d+$", "");

            if (!seen.ContainsKey(baseName))
            {
                seen[baseName] = prop.Value.ToString();
                clean[prop.Name] = prop.Value;
            }
            // 否则跳过重复项
        }

        File.WriteAllText(zhPath, clean.ToString(Formatting.Indented));
        AssetDatabase.Refresh();
        
        int cleanedCount = clean.Count;
        Debug.Log($"[CleanDuplicateKeys] 清理完成，原始 {originalCount} 条 → 清理后 {cleanedCount} 条");
        EditorUtility.DisplayDialog("完成", $"清理完成！\n原始: {originalCount} 条\n清理后: {cleanedCount} 条", "确定");
    }
}
#endif
