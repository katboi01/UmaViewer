#if !UNITY_ANDROID || UNITY_EDITOR
using SFB;
#endif

using System.Collections;
using System.Diagnostics;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsOther : MonoBehaviour
{
    public Button UpdateDBButton;
    public TMP_Dropdown RegionDropdown;
    public TMP_Dropdown WorkModeDropdown;
    public TMP_Dropdown LanguageDropdown;
    private IEnumerator _updateResVerCoroutine;

    public void ApplySettings()
    {
        WorkModeDropdown.SetValueWithoutNotify((int)Config.Instance.WorkMode);
        RegionDropdown.SetValueWithoutNotify((int)Config.Instance.Region);
        LanguageDropdown.SetValueWithoutNotify((int)Config.Instance.Language);
        UpdateDBButton.interactable = (Config.Instance.WorkMode == WorkMode.Standalone);
    }

    public void ChangeLanguage(int lang)
    {
        if ((int)Config.Instance.Language != lang)
        {
            Config.Instance.Language = (Language)lang;
            Config.Instance.UpdateConfig(true);
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
        UmaDatabaseController.Instance.CloseAllConnection();
        ManifestDB dB = new ManifestDB();
        _updateResVerCoroutine = dB.UpdateResourceVersion(delegate (string msg, UIMessageType type) { UmaViewerUI.Instance.ShowMessage(msg, type); });
        StartCoroutine(_updateResVerCoroutine);
    }

    public void ChangeDataPath()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        var path = StandaloneFileBrowser.OpenFolderPanel("Select Folder", Config.Instance.MainPath, false);
        if (path != null && path.Length > 0 && !string.IsNullOrEmpty(path[0]))
        {
            if (path[0] != Config.Instance.MainPath)
            {
                Config.Instance.MainPath = path[0];
                Config.Instance.UpdateConfig(true);
            }
        }
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
#endif
    }

    public void UnloadAllBundle() => UmaAssetManager.UnloadAllBundle(true);
}
