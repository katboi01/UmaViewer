#if !UNITY_ANDROID || UNITY_EDITOR
using SFB;
#endif

using System.Diagnostics;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class UISettingsOther : MonoBehaviour
{
    public Button UpdateDBButton;
    public TMP_Dropdown RegionDropdown;
    public TMP_Dropdown WorkModeDropdown;
    public TMP_Dropdown LanguageDropdown;
    public TMP_Dropdown CharacterNameLanguageDropdown;  // ← 在 Inspector 中拖拽赋值

    private System.Collections.IEnumerator _updateResVerCoroutine;

    void Start()
    {
        Debug.Log("[UISettingsOther] Start() 开始执行");
        
        SetupLanguageDropdown();
        SetupCharacterNameLanguageDropdown();
        ApplySettings();
        
        // 延迟绑定事件，确保所有组件已初始化
        Invoke("BindCharacterNameLanguageEvent", 0.1f);
        
        Debug.Log("[UISettingsOther] Start() 执行完成");
    }
    
    void BindCharacterNameLanguageEvent()
    {
        Debug.Log("[UISettingsOther] BindCharacterNameLanguageEvent() 开始执行");
        
        // 如果 Inspector 中没有绑定，尝试自动查找
        if (CharacterNameLanguageDropdown == null)
        {
            Debug.Log("[UISettingsOther] CharacterNameLanguageDropdown 为 null，尝试自动查找...");
            
            // 在整个场景中查找名为 "CharacterNameLanguage" 的对象
            var charNameLangObj = GameObject.Find("CharacterNameLanguage");
            if (charNameLangObj != null)
            {
                Debug.Log($"[UISettingsOther] 找到 CharacterNameLanguage 对象: {charNameLangObj.name}");
                
                // 先尝试在对象本身获取 TMP_Dropdown
                CharacterNameLanguageDropdown = charNameLangObj.GetComponent<TMP_Dropdown>();
                if (CharacterNameLanguageDropdown != null)
                {
                    Debug.Log("[UISettingsOther] ✓ 在 CharacterNameLanguage 对象上找到 TMP_Dropdown 组件");
                }
                else
                {
                    // 如果没有，尝试在子对象中查找
                    Debug.Log("[UISettingsOther] CharacterNameLanguage 对象上没有 TMP_Dropdown，尝试查找子对象...");
                    CharacterNameLanguageDropdown = charNameLangObj.GetComponentInChildren<TMP_Dropdown>();
                    if (CharacterNameLanguageDropdown != null)
                    {
                        Debug.Log($"[UISettingsOther] ✓ 在子对象 {CharacterNameLanguageDropdown.gameObject.name} 上找到 TMP_Dropdown 组件");
                    }
                }
            }
        }
        
        if (CharacterNameLanguageDropdown != null)
        {
            CharacterNameLanguageDropdown.onValueChanged.RemoveAllListeners();
            CharacterNameLanguageDropdown.onValueChanged.AddListener(ChangeCharacterNameLanguage);
            Debug.Log("[UISettingsOther] ✓ 已成功绑定 CharacterNameLanguageDropdown 事件到 ChangeCharacterNameLanguage");
            
            // 设置当前值（因为ApplySettings执行时Dropdown还是null）
            CharacterNameLanguageDropdown.SetValueWithoutNotify((int)Config.Instance.CharacterNameLanguage);
            Debug.Log($"[UISettingsOther] ✓ 已设置 CharacterNameLanguageDropdown 值为: {Config.Instance.CharacterNameLanguage} ({(int)Config.Instance.CharacterNameLanguage})");
        }
        else
        {
            Debug.LogError("[UISettingsOther] ✗ 无法找到 CharacterNameLanguageDropdown！");
            Debug.LogError("[UISettingsOther] 请确认场景中存在名为 'CharacterNameLanguage' 的对象，且其子对象中有 TMP_Dropdown 组件");
        }
    }

    /// <summary>
    /// 设置语言下拉菜单选项（只在选项不对时才重设）
    /// </summary>
    private void SetupLanguageDropdown()
    {
        if (LanguageDropdown == null) return;

        if (LanguageDropdown.options.Count != 2 ||
            LanguageDropdown.options[0].text != "English" ||
            LanguageDropdown.options[1].text != "中文")
        {
            LanguageDropdown.ClearOptions();
            LanguageDropdown.options.Add(new TMP_Dropdown.OptionData("English"));
            LanguageDropdown.options.Add(new TMP_Dropdown.OptionData("中文"));
        }
    }

    /// <summary>
    /// 设置角色名语言下拉菜单选项（只在选项不对时才重设）
    /// </summary>
    private void SetupCharacterNameLanguageDropdown()
    {
        if (CharacterNameLanguageDropdown == null) return;

        if (CharacterNameLanguageDropdown.options.Count != 3 ||
            CharacterNameLanguageDropdown.options[0].text != "日本語" ||
            CharacterNameLanguageDropdown.options[1].text != "English" ||
            CharacterNameLanguageDropdown.options[2].text != "中文")
        {
            CharacterNameLanguageDropdown.ClearOptions();
            CharacterNameLanguageDropdown.options.Add(new TMP_Dropdown.OptionData("日本語"));
            CharacterNameLanguageDropdown.options.Add(new TMP_Dropdown.OptionData("English"));
            CharacterNameLanguageDropdown.options.Add(new TMP_Dropdown.OptionData("中文"));
        }
    }

    public void ApplySettings()
    {
        if (WorkModeDropdown != null)
            WorkModeDropdown.SetValueWithoutNotify((int)Config.Instance.WorkMode);
        
        if (RegionDropdown != null)
            RegionDropdown.SetValueWithoutNotify((int)Config.Instance.Region);
        
        if (LanguageDropdown != null)
            LanguageDropdown.SetValueWithoutNotify((int)Config.Instance.Language);

        if (CharacterNameLanguageDropdown != null)
        {
            CharacterNameLanguageDropdown.SetValueWithoutNotify((int)Config.Instance.CharacterNameLanguage);
            Debug.Log($"[UISettingsOther] ApplySettings - Language: {Config.Instance.Language}, CharacterNameLanguage: {Config.Instance.CharacterNameLanguage}");
        }

        if (UpdateDBButton != null)
            UpdateDBButton.interactable = (Config.Instance.WorkMode == WorkMode.Standalone);
    }

    public void ChangeLanguage(int lang)
    {
        if ((int)Config.Instance.Language != lang)
        {
            Config.Instance.Language = (Language)lang;
            Config.Instance.UpdateConfig(true);
            LocalizationManager.Instance.SetLanguage((Language)lang);

            if (UmaViewerUI.Instance != null)
            {
                UmaViewerUI.Instance.UpdateUILocalization();
            }

            foreach (var label in FindObjectsOfType<LocalizedLabel>(true))
            {
                label.ForceRefresh();
            }

            Popup.Create(LocalizationManager.Get("msg.restart_required"), -1, 150,
                LocalizationManager.Get("popup.ok"), null);
        }
    }

    /// <summary>
    /// 切换角色名语言并刷新显示
    /// </summary>
    public void ChangeCharacterNameLanguage(int lang)
    {
        Debug.Log($"[UISettingsOther] >>> ChangeCharacterNameLanguage 被调用! 参数: {lang}, 当前: {(int)Config.Instance.CharacterNameLanguage}");
        
        if ((int)Config.Instance.CharacterNameLanguage != lang)
        {
            Config.Instance.CharacterNameLanguage = (CharacterNameLanguage)lang;
            Config.Instance.UpdateConfig(true);

            Debug.Log($"[UISettingsOther] 角色名语言已切换到: {Config.Instance.CharacterNameLanguage}");
            
            // 刷新角色名和服装名显示
            if (UmaViewerUI.Instance != null)
            {
                UmaViewerUI.Instance.RefreshCharacterNames();
                Debug.Log("[UISettingsOther] 已触发角色名刷新");
                
                // 刷新服装名显示
                UmaViewerUI.Instance.RefreshCostumeNames();
                Debug.Log("[UISettingsOther] 已触发服装名刷新");
            }
        }
        else
        {
            Debug.Log("[UISettingsOther] 角色名语言未改变，跳过");
        }
    }

    public void ChangeRegion(int region)
    {
        if ((int)Config.Instance.Region != region)
        {
            Config.Instance.Region = (Region)region;
            Config.Instance.UpdateConfig(false);
            StartCoroutine(UmaViewerUI.Instance.ApplyGraphicsSettings());
        }
    }

    public void ChangeWorkMode(int mode)
    {
        if ((int)Config.Instance.WorkMode != mode)
        {
            Config.Instance.WorkMode = (WorkMode)mode;
            Config.Instance.UpdateConfig(true);
        }
    }

    public void UpdateGameDB()
    {
        if (_updateResVerCoroutine != null && Config.Instance.WorkMode != WorkMode.Standalone) return;
        Popup.Create(LocalizationManager.Get("popup.db_update_notice"), -1, 200,
            LocalizationManager.Get("popup.ok"), null, LocalizationManager.Get("popup.ok"));
    }

    public void ChangeDataPath()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        var path = StandaloneFileBrowser.OpenFolderPanel(LocalizationManager.Get("msg.select_folder"), Config.Instance.MainPath, false);
        if (path != null && path.Length > 0 && !string.IsNullOrEmpty(path[0]))
        {
            if (path[0] != Config.Instance.MainPath)
            {
                Config.Instance.MainPath = path[0];
                Config.Instance.UpdateConfig(true);
            }
        }
#else
        UmaViewerUI.Instance.ShowMessage(LocalizationManager.Get("msg.not_supported"), UIMessageType.Warning);
#endif
    }

    public void OpenConfig()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        if (File.Exists(Config.configPath))
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Config.configPath,
                UseShellExecute = true
            });
        }
#else
        UmaViewerUI.Instance.ShowMessage(LocalizationManager.Get("msg.not_supported"), UIMessageType.Warning);
#endif
    }

    public void UnloadAllBundle() => UmaAssetManager.UnloadAllBundle(true);
}
