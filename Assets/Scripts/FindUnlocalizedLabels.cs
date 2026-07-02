using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 查找所有未本地化的标签
/// </summary>
public class FindUnlocalizedLabels : MonoBehaviour
{
    void Start()
    {
        Invoke("FindLabels", 0.5f);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            FindLabels();
        }
    }
    
    void FindLabels()
    {
        Debug.Log("=== 查找所有未本地化的标签 ===");
        
        int totalText = 0;
        int totalTMP = 0;
        int localizedCount = 0;
        int unlocalizedCount = 0;
        
        // 查找所有Text组件
        Text[] allTexts = FindObjectsOfType<Text>(true);
        totalText = allTexts.Length;
        
        Debug.Log($"\n--- Text 组件 ({totalText} 个) ---");
        foreach (var text in allTexts)
        {
            if (string.IsNullOrEmpty(text.text)) continue;
            
            var label = text.GetComponent<LocalizedLabel>();
            if (label != null && !string.IsNullOrEmpty(label.key))
            {
                localizedCount++;
            }
            else
            {
                unlocalizedCount++;
                Debug.LogWarning($"[未本地化] {text.gameObject.name}: \"{text.text}\"");
            }
        }
        
        // 查找所有TextMeshProUGUI组件
        TextMeshProUGUI[] allTMP = FindObjectsOfType<TextMeshProUGUI>(true);
        totalTMP = allTMP.Length;
        
        Debug.Log($"\n--- TextMeshProUGUI 组件 ({totalTMP} 个) ---");
        foreach (var tmp in allTMP)
        {
            if (string.IsNullOrEmpty(tmp.text)) continue;
            
            var label = tmp.GetComponent<LocalizedLabel>();
            if (label != null && !string.IsNullOrEmpty(label.key))
            {
                localizedCount++;
            }
            else
            {
                unlocalizedCount++;
                Debug.LogWarning($"[未本地化] {tmp.gameObject.name}: \"{tmp.text}\"");
            }
        }
        
        Debug.Log($"\n=== 统计 ===");
        Debug.Log($"Text 组件: {totalText} 个");
        Debug.Log($"TextMeshProUGUI 组件: {totalTMP} 个");
        Debug.Log($"已本地化: {localizedCount} 个");
        Debug.Log($"未本地化: {unlocalizedCount} 个");
        Debug.Log($"按 F 键重新查找");
    }
}