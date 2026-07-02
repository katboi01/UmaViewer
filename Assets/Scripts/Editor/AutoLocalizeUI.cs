#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// 自动本地化 UI 工具 - 扫描场景中所有 Text/TMP 组件，自动添加 LocalizedLabel
/// </summary>
public class AutoLocalizeUI : EditorWindow
{
    private Dictionary<string, string> zhTranslations = new Dictionary<string, string>();
    private Dictionary<string, string> enTranslations = new Dictionary<string, string>();
    private Dictionary<string, string> textToKeyCache = new Dictionary<string, string>(); // 新增：文字内容 → key 的反向映射
    private int addedCount = 0;
    private int skippedCount = 0;
    private int translationAddedCount = 0;
    
    [MenuItem("Tools/自动本地化 UI")]
    static void ShowWindow()
    {
        GetWindow<AutoLocalizeUI>("自动本地化 UI");
    }
    
    void OnGUI()
    {
        GUILayout.Label("自动本地化 UI 工具", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("此工具将：");
        GUILayout.Label("1. 扫描场景中所有 Text 和 TextMeshProUGUI 组件");
        GUILayout.Label("2. 为没有 LocalizedLabel 的组件自动添加");
        GUILayout.Label("3. 根据文字内容生成 key 并添加到翻译文件");
        GUILayout.Space(10);
        
        if (GUILayout.Button("开始自动本地化", GUILayout.Height(30)))
        {
            AutoLocalize();
        }
        
        GUILayout.Space(10);
        GUILayout.Label($"已添加: {addedCount} 个 LocalizedLabel");
        GUILayout.Label($"已跳过: {skippedCount} 个（已有 LocalizedLabel）");
        GUILayout.Label($"新增翻译: {translationAddedCount} 条");
    }
    
    void AutoLocalize()
    {
        addedCount = 0;
        skippedCount = 0;
        translationAddedCount = 0;
        textToKeyCache.Clear();  // 初始化缓存
        
        // 加载现有翻译
        LoadTranslations();
        
        // 扫描所有 Text 组件
        var allTexts = FindObjectsOfType<Text>(true);
        foreach (var text in allTexts)
        {
            ProcessTextComponent(text.gameObject, text.text);
        }
        
        // 扫描所有 TextMeshProUGUI 组件
        var allTmpTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var tmp in allTmpTexts)
        {
            ProcessTextComponent(tmp.gameObject, tmp.text);
        }
        
        // 保存翻译文件
        SaveTranslations();
        
        Debug.Log($"[AutoLocalizeUI] 完成！已添加 {addedCount} 个 LocalizedLabel，跳过 {skippedCount} 个，新增 {translationAddedCount} 条翻译");
        EditorUtility.DisplayDialog("完成", $"自动本地化完成！\n\n已添加: {addedCount} 个 LocalizedLabel\n已跳过: {skippedCount} 个\n新增翻译: {translationAddedCount} 条", "确定");
    }
    
    void ProcessTextComponent(GameObject go, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (text.Length < 2) return;
        if (text.Contains("\n")) return;
        
        // 检查是否已有 LocalizedLabel
        var existingLabel = go.GetComponent<LocalizedLabel>();
        if (existingLabel != null)
        {
            skippedCount++;
            return;
        }
        
        // 检查是否已被翻译（文字在 zhTranslations 的 value 中）
        if (zhTranslations.ContainsValue(text))
        {
            skippedCount++;
            return;
        }
        
        // 生成 key
        string key = GenerateKey(text, go.name);
        
        // 添加 LocalizedLabel 组件
        var label = go.AddComponent<LocalizedLabel>();
        label.key = key;
        
        // 添加翻译
        if (!zhTranslations.ContainsKey(key))
        {
            string zhText = TranslateToChinese(text);
            zhTranslations[key] = zhText;
            enTranslations[key] = text;
            translationAddedCount++;
        }
        
        addedCount++;
        
        // 标记为已修改
        EditorUtility.SetDirty(go);
    }
    
    string GenerateKey(string text, string objectName)
    {
        // 先查缓存：相同文字复用同一个 key
        if (textToKeyCache.TryGetValue(text, out string existingKey))
        {
            return existingKey;
        }
        
        // 基于文字内容生成 key
        string baseKey = text.ToLower()
            .Replace(" ", "_")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace(":", "")
            .Replace(".", "_")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("+", "_plus")
            .Replace("-", "_");
        
        // 限制长度
        if (baseKey.Length > 30)
        {
            baseKey = baseKey.Substring(0, 30);
        }
        
        // 添加前缀
        string prefix = "ui.";
        
        // 确保 key 唯一
        string finalKey = prefix + baseKey;
        int counter = 1;
        while (zhTranslations.ContainsKey(finalKey))
        {
            finalKey = prefix + baseKey + "_" + counter;
            counter++;
        }
        
        // 写入缓存
        textToKeyCache[text] = finalKey;
        return finalKey;
    }
    
    string TranslateToChinese(string englishText)
    {
        // 常见翻译映射（已去重）
        var translations = new Dictionary<string, string>
        {
            // UI 元素
            {"Animation", "动画"},
            {"Sound", "声音"},
            {"Model", "模型"},
            {"Camera", "相机"},
            {"Settings", "设置"},
            {"Environment", "环境"},
            {"Screenshot", "截图"},
            {"Loaded Assets", "已加载资源"},
            {"Assets", "资源"},
            {"Other", "其他"},
            {"ColorSet", "配色"},
            {"Characters", "角色"},
            {"Props", "道具"},
            {"Scenes", "场景"},
            {"About", "关于"},
            {"Facial Morphs", "表情变形"},
            {"Mob", "NPC"},
            {"Mini", "Q版"},
            {"Live", "演出"},
            {"Pose", "姿势"},
            {"Ear", "耳朵"},
            {"Eyebrow", "眉毛"},
            {"Eye", "眼睛"},
            {"Mouth", "嘴"},
            {"Expression", "表情"},
            {"Action", "动作"},
            {"Costume", "服装"},
            {"Upgraded", "突破"},
            {"Race", "比赛"},
            {"School", "校服"},
            {"Tracksuit", "运动服"},
            {"Swimsuit", "泳装"},
            {"Towel", "毛巾"},
            {"Default", "默认"},
            {"Generic", "通用"},
            {"Tail", "尾巴"},
            {"General", "常规"},
            
            // 属性
            {"Speed", "速度"},
            {"Stamina", "耐力"},
            {"Power", "力量"},
            {"Guts", "毅力"},
            {"Wisdom", "智慧"},
            {"Wit", "智力"},
            {"Skills", "技能"},
            {"Level", "等级"},
            {"Rarity", "稀有度"},
            
            // 操作
            {"Play", "播放"},
            {"Pause", "暂停"},
            {"Stop", "停止"},
            {"Record", "录制"},
            {"Save", "保存"},
            {"Load", "加载"},
            {"Export", "导出"},
            {"Import", "导入"},
            {"Browse", "浏览"},
            {"Open", "打开"},
            {"Close", "关闭"},
            {"Cancel", "取消"},
            {"OK", "确定"},
            {"Apply", "应用"},
            {"Reset", "重置"},
            {"Clear", "清除"},
            {"Search", "搜索"},
            {"Filter", "筛选"},
            {"Sort", "排序"},
            {"Refresh", "刷新"},
            {"Update", "更新"},
            {"Download", "下载"},
            {"Upload", "上传"},
            {"Delete", "删除"},
            {"Remove", "移除"},
            {"Add", "添加"},
            {"Edit", "编辑"},
            {"View", "查看"},
            {"Show", "显示"},
            {"Hide", "隐藏"},
            {"Enable", "启用"},
            {"Disable", "禁用"},
            
            // 状态
            {"On", "开"},
            {"Off", "关"},
            {"Yes", "是"},
            {"No", "否"},
            {"Error", "错误"},
            {"Warning", "警告"},
            {"Info", "信息"},
            {"Success", "成功"},
            {"Failed", "失败"},
            {"Loading", "加载中"},
            {"Done", "完成"},
            {"Complete", "完成"},
            {"Ready", "就绪"},
            {"Start", "开始"},
            {"End", "结束"},
            {"Next", "下一个"},
            {"Previous", "上一个"},
            {"All", "全部"},
            {"None", "无"},
            {"Select", "选择"},
            {"Active", "激活"},
            {"Inactive", "未激活"},
            {"Visible", "可见"},
            {"Hidden", "隐藏"},
            
            // UI 组件
            {"Text", "文字"},
            {"Label", "标签"},
            {"Button", "按钮"},
            {"Toggle", "开关"},
            {"Slider", "滑块"},
            {"Scrollbar", "滚动条"},
            {"Dropdown", "下拉框"},
            {"Input", "输入"},
            {"Panel", "面板"},
            {"Container", "容器"},
            {"Group", "组"},
            {"List", "列表"},
            {"Grid", "网格"},
            {"Table", "表格"},
            {"Header", "标题"},
            {"Footer", "页脚"},
            {"Sidebar", "侧边栏"},
            {"Toolbar", "工具栏"},
            {"Menu", "菜单"},
            {"Tooltip", "提示"},
            {"Help", "帮助"},
            
            // 通用
            {"Name", "名称"},
            {"Title", "标题"},
            {"Description", "描述"},
            {"Comment", "评论"},
            {"Note", "备注"},
            {"Message", "消息"},
            {"Notification", "通知"},
            {"Event", "事件"},
            {"Command", "命令"},
            {"Function", "函数"},
            {"Property", "属性"},
            {"Field", "字段"},
            {"Parameter", "参数"},
            {"Option", "选项"},
            {"Setting", "设置"},
            {"Configuration", "配置"},
            {"Profile", "配置文件"},
            {"Account", "账户"},
            {"User", "用户"},
            {"Player", "玩家"},
            {"Avatar", "头像"},
            {"Icon", "图标"},
            {"Image", "图片"},
            {"File", "文件"},
            {"Folder", "文件夹"},
            {"Path", "路径"},
            {"URL", "链接"},
            {"Address", "地址"},
            {"Location", "位置"},
            {"Map", "地图"},
            {"Area", "区域"},
            
            // 尺寸
            {"Width", "宽度"},
            {"Height", "高度"},
            {"Size", "大小"},
            {"Position", "位置"},
            {"Rotation", "旋转"},
            {"Scale", "缩放"},
            {"Color", "颜色"},
            {"Alpha", "透明度"},
            {"Font", "字体"},
            
            // 颜色
            {"Red", "红色"},
            {"Green", "绿色"},
            {"Blue", "蓝色"},
            {"Yellow", "黄色"},
            {"Orange", "橙色"},
            {"Purple", "紫色"},
            {"Pink", "粉色"},
            {"Brown", "棕色"},
            {"Black", "黑色"},
            {"White", "白色"},
            {"Gray", "灰色"},
            {"Silver", "银色"},
            {"Gold", "金色"},
            
            // 时间
            {"Time", "时间"},
            {"Date", "日期"},
            {"Duration", "时长"},
            {"Progress", "进度"},
            {"Status", "状态"},
            {"Mode", "模式"},
            {"Type", "类型"},
            {"Category", "类别"},
            {"Tag", "标签"}
        };
        
        // 尝试直接翻译
        if (translations.ContainsKey(englishText))
        {
            return translations[englishText];
        }
        
        // 尝试小写翻译
        string lowerText = englishText.ToLower();
        if (translations.ContainsKey(lowerText))
        {
            return translations[lowerText];
        }
        
        // 尝试单词翻译
        string[] words = englishText.Split(' ');
        if (words.Length == 1)
        {
            if (translations.ContainsKey(words[0]))
            {
                return translations[words[0]];
            }
        }
        
        // 如果无法翻译，返回原文
        return englishText;
    }
    

    
    void LoadTranslations()
    {
        zhTranslations.Clear();
        enTranslations.Clear();
        
        // 加载中文翻译
        var zhPath = "Assets/Resources/Localization/zh_cn.json";
        if (File.Exists(zhPath))
        {
            var json = File.ReadAllText(zhPath);
            var obj = JObject.Parse(json);
            foreach (var prop in obj.Properties())
            {
                zhTranslations[prop.Name] = prop.Value.ToString();
            }
        }
        
        // 加载英文翻译
        var enPath = "Assets/Resources/Localization/en.json";
        if (File.Exists(enPath))
        {
            var json = File.ReadAllText(enPath);
            var obj = JObject.Parse(json);
            foreach (var prop in obj.Properties())
            {
                enTranslations[prop.Name] = prop.Value.ToString();
            }
        }
    }
    
    void SaveTranslations()
    {
        // 保存中文翻译
        var zhPath = "Assets/Resources/Localization/zh_cn.json";
        var zhObj = new JObject();
        foreach (var kvp in zhTranslations)
        {
            zhObj[kvp.Key] = kvp.Value;
        }
        File.WriteAllText(zhPath, zhObj.ToString(Formatting.Indented));
        
        // 保存英文翻译
        var enPath = "Assets/Resources/Localization/en.json";
        var enObj = new JObject();
        foreach (var kvp in enTranslations)
        {
            enObj[kvp.Key] = kvp.Value;
        }
        File.WriteAllText(enPath, enObj.ToString(Formatting.Indented));
        
        AssetDatabase.Refresh();
    }
}
#endif
