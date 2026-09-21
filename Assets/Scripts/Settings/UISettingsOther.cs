#if UNITY_STANDALONE || UNITY_EDITOR
using SFB;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISettingsOther : MonoBehaviour
{
    public Button UpdateDBButton;
    public TMP_Dropdown RegionDropdown;
    public TMP_Dropdown WorkModeDropdown;
    public TMP_Dropdown LanguageDropdown;
    public TMP_Dropdown FrameRateDropdown;
    private GameObject _runtimeFrameRateRow;
    private IEnumerator _updateResVerCoroutine;

    public void ApplySettings()
    {
        EnsureLanguageOptions();
        EnsureFrameRateDropdown();
        WorkModeDropdown.SetValueWithoutNotify((int)Config.Instance.WorkMode);
        RegionDropdown.SetValueWithoutNotify((int)Config.Instance.Region);
        LanguageDropdown.SetValueWithoutNotify(LanguageToDropdownValue(Config.Instance.Language));
        FrameRateDropdown?.SetValueWithoutNotify(FrameRateToDropdownValue(Config.Instance.GetTargetFrameRate()));
        FrameRateDropdown?.RefreshShownValue();
        UmaViewerMain.ApplyFrameRateLimit();
        UpdateDBButton.interactable = (Config.Instance.WorkMode == WorkMode.Standalone);
    }

    private void EnsureLanguageOptions()
    {
        if (LanguageDropdown == null)
            return;

        var options = new List<string>
        {
            "简体中文",
            "English",
            "日本語"
        };

        bool optionsMatch = LanguageDropdown.options.Count == options.Count;
        if (optionsMatch)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (LanguageDropdown.options[i].text != options[i])
                {
                    optionsMatch = false;
                    break;
                }
            }
        }

        if (!optionsMatch)
        {
            LanguageDropdown.ClearOptions();
            LanguageDropdown.AddOptions(options);
        }
    }

    private int LanguageToDropdownValue(Language language)
    {
        switch (language)
        {
            case Language.Cn:
                return 0;
            case Language.En:
                return 1;
            case Language.Jp:
            default:
                return 2;
        }
    }

    private Language DropdownValueToLanguage(int value)
    {
        switch (value)
        {
            case 0:
                return Language.Cn;
            case 1:
                return Language.En;
            case 2:
            default:
                return Language.Jp;
        }
    }

    private void EnsureFrameRateDropdown()
    {
        if (FrameRateDropdown != null || LanguageDropdown == null)
        {
            return;
        }

        var sourceRow = LanguageDropdown.transform.parent;
        var parent = sourceRow.parent;
        _runtimeFrameRateRow = Instantiate(sourceRow.gameObject, parent);
        _runtimeFrameRateRow.name = "FrameRate";
        _runtimeFrameRateRow.transform.SetSiblingIndex(sourceRow.GetSiblingIndex() + 1);

        var label = _runtimeFrameRateRow.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.text = "Frame Rate";
        }

        FrameRateDropdown = _runtimeFrameRateRow.GetComponentInChildren<TMP_Dropdown>(true);
        FrameRateDropdown.name = "FrameRateDropdown";
        FrameRateDropdown.ClearOptions();
        FrameRateDropdown.AddOptions(new List<string> { "60 FPS", "30 FPS" });
        FrameRateDropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();
        FrameRateDropdown.onValueChanged.AddListener(ChangeFrameRate);

        var parentRect = parent as RectTransform;
        var sourceRect = sourceRow as RectTransform;
        if (parentRect != null && sourceRect != null)
        {
            parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentRect.rect.height + sourceRect.rect.height);
            LayoutRebuilder.MarkLayoutForRebuild(parentRect);
        }
    }

    private int FrameRateToDropdownValue(int frameRate)
    {
        return frameRate == 30 ? 1 : 0;
    }

    private int DropdownValueToFrameRate(int value)
    {
        return value == 1 ? 30 : 60;
    }

    public void ChangeFrameRate(int value)
    {
        var frameRate = DropdownValueToFrameRate(value);
        if (Config.Instance.TargetFrameRate != frameRate)
        {
            Config.Instance.TargetFrameRate = frameRate;
            Config.Instance.UpdateConfig(false);
        }
        UmaViewerMain.ApplyFrameRateLimit();
    }

    public void ChangeLanguage(int lang)
    {
        Language language = DropdownValueToLanguage(lang);
        if (Config.Instance.Language != language)
        {
            Config.Instance.Language = language;
            Config.Instance.UpdateConfig(false);

            // Rebuild all localized lists and reload English translations when
            // necessary without requiring the application process to restart.
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
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
        Popup.Create($"Automatic database update is no longer supported until all issues with new files are resolved. Please run the game to obtain required files.", -1, 200,
            "Ok", null, "Ok");
    }

    public void ChangeDataPath()
    {
#if UNITY_EDITOR
        // Editor 里用 Unity 自带的选文件夹（不会走 SFB 的原生 .bundle，因此不会 DllNotFoundException）
        var p = EditorUtility.OpenFolderPanel("Select Folder", Config.Instance.MainPath, "");
        if (!string.IsNullOrEmpty(p) && p != Config.Instance.MainPath)
        {
            Config.Instance.MainPath = p;
            Config.Instance.UpdateConfig(true);
        }

#elif UNITY_STANDALONE
        // Standalone 运行时仍走 SFB（如果以后要打包到 mac App，这里建议再换成更现代的方案）
        var path = StandaloneFileBrowser.OpenFolderPanel("Select Folder", Config.Instance.MainPath, false);
        if (path != null && path.Length > 0 && !string.IsNullOrEmpty(path[0]) && path[0] != Config.Instance.MainPath)
        {
            Config.Instance.MainPath = path[0];
            Config.Instance.UpdateConfig(true);
        }
#else
        if (WebFileMount.Active)
        {
            // WebGL: the browser picks the folder, remembers it, and reloads.
            WebFileMount.ChangeDataFolder();
        }
        else
        {
            UmaViewerUI.Instance.ShowMessage("Not supported on this platform", UIMessageType.Warning);
        }
#endif
    }

    public void OpenConfig()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (File.Exists(Config.configPath))
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Config.configPath,
                UseShellExecute = true
            });
        }
#else
        UmaViewerUI.Instance.ShowMessage("Not supported on this platform", UIMessageType.Warning);
#endif
    }

    public void UnloadAllBundle() => UmaAssetManager.UnloadAllBundle(true);
}
