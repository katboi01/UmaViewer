#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 修复 Dropdown 标签 - 删除 Dropdown Item Label 上的 LocalizedLabel 组件
/// </summary>
public class FixDropdownLabels : EditorWindow
{
    [MenuItem("Tools/修复 Dropdown 标签")]
    static void ShowWindow()
    {
        GetWindow<FixDropdownLabels>("修复 Dropdown 标签");
    }
    
    void OnGUI()
    {
        GUILayout.Label("修复 Dropdown 标签", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("此工具会删除所有 Dropdown Item Label 上的 LocalizedLabel 组件。");
        GUILayout.Label("因为 Dropdown 的选项文字不应该被翻译。");
        GUILayout.Space(10);
        
        if (GUILayout.Button("删除 Dropdown 标签的 LocalizedLabel", GUILayout.Height(30)))
        {
            FixDropdowns();
        }
        
        if (GUILayout.Button("检查 Dropdown 标签", GUILayout.Height(30)))
        {
            CheckDropdowns();
        }
    }
    
    void FixDropdowns()
    {
        int removedCount = 0;
        
        // 查找所有 TMP_Dropdown 组件
        var allDropdowns = FindObjectsOfType<TMP_Dropdown>(true);
        
        foreach (var dropdown in allDropdowns)
        {
            // 查找 Dropdown 的 Template
            if (dropdown.template != null)
            {
                // 查找 Template 中的 Item Label
                var itemLabels = dropdown.template.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var label in itemLabels)
                {
                    if (label.gameObject.name == "Item Label")
                    {
                        // 检查是否有 LocalizedLabel 组件
                        var localizedLabel = label.GetComponent<LocalizedLabel>();
                        if (localizedLabel != null)
                        {
                            DestroyImmediate(localizedLabel);
                            removedCount++;
                            Debug.Log($"[FixDropdownLabels] 删除了 {dropdown.name} 的 Item Label 上的 LocalizedLabel");
                        }
                    }
                }
            }
            
            // 也检查 Dropdown 的 Caption
            if (dropdown.captionText != null)
            {
                var localizedLabel = dropdown.captionText.GetComponent<LocalizedLabel>();
                if (localizedLabel != null)
                {
                    DestroyImmediate(localizedLabel);
                    removedCount++;
                    Debug.Log($"[FixDropdownLabels] 删除了 {dropdown.name} 的 Caption 上的 LocalizedLabel");
                }
            }
        }
        
        if (removedCount > 0)
        {
            EditorUtility.DisplayDialog("完成", $"删除了 {removedCount} 个 LocalizedLabel 组件", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "没有找到需要删除的 LocalizedLabel 组件", "确定");
        }
    }
    
    void CheckDropdowns()
    {
        int count = 0;
        
        // 查找所有 TMP_Dropdown 组件
        var allDropdowns = FindObjectsOfType<TMP_Dropdown>(true);
        
        foreach (var dropdown in allDropdowns)
        {
            // 查找 Dropdown 的 Template
            if (dropdown.template != null)
            {
                // 查找 Template 中的 Item Label
                var itemLabels = dropdown.template.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var label in itemLabels)
                {
                    if (label.gameObject.name == "Item Label")
                    {
                        // 检查是否有 LocalizedLabel 组件
                        var localizedLabel = label.GetComponent<LocalizedLabel>();
                        if (localizedLabel != null)
                        {
                            Debug.LogWarning($"[FixDropdownLabels] {dropdown.name} 的 Item Label 有 LocalizedLabel 组件，key: {localizedLabel.key}");
                            count++;
                        }
                    }
                }
            }
            
            // 也检查 Dropdown 的 Caption
            if (dropdown.captionText != null)
            {
                var localizedLabel = dropdown.captionText.GetComponent<LocalizedLabel>();
                if (localizedLabel != null)
                {
                    Debug.LogWarning($"[FixDropdownLabels] {dropdown.name} 的 Caption 有 LocalizedLabel 组件，key: {localizedLabel.key}");
                    count++;
                }
            }
        }
        
        if (count > 0)
        {
            Debug.LogWarning($"[FixDropdownLabels] 找到 {count} 个需要修复的 LocalizedLabel 组件");
            EditorUtility.DisplayDialog("检查结果", $"找到 {count} 个需要修复的 LocalizedLabel 组件\n请运行 '删除 Dropdown 标签的 LocalizedLabel' 来修复", "确定");
        }
        else
        {
            Debug.Log("[FixDropdownLabels] 没有找到需要修复的 LocalizedLabel 组件");
            EditorUtility.DisplayDialog("检查结果", "没有找到需要修复的 LocalizedLabel 组件", "确定");
        }
    }
}
#endif
