using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static PageManager;

public class UISettingsAssets : MonoBehaviour
{
    private Dictionary<string, Entry> LoadedAssetEntries = new Dictionary<string, Entry>();
    public PageManager LoadedAssetPageCtrl;
    public ScrollRect LoadedAssetScrollRect;

    public void LoadedAssetsAdd(UmaDatabaseEntry entry)
    {
        if (!LoadedAssetPageCtrl) return;
        if (LoadedAssetEntries.ContainsKey(entry.Name)) return;
        var file_name = Path.GetFileName(entry.Name);
        string filePath = entry.FilePath.Replace("/", "\\");
        var assetentry = new Entry()
        {
            Name = file_name,
            FontSize = 18,
            OnClick = (container) =>
            {
                Process.Start("explorer.exe", "/select," + filePath);
            }
        };
        LoadedAssetEntries.Add(entry.Name, assetentry);
        LoadedAssetPageCtrl.AddEntries(assetentry);
    }

    public void LoadedAssetsClear()
    {
        LoadedAssetEntries.Clear();
        if (LoadedAssetPageCtrl)
        {
            LoadedAssetPageCtrl.ResetCtrl();
            LoadedAssetPageCtrl.Initialize(LoadedAssetEntries.Values.ToList(), LoadedAssetScrollRect);
        }
    }

    public void CopyAllLoadedAssetsPath()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var entry in LoadedAssetEntries.Keys)
        {
            if (UmaViewerMain.Instance.AbList.TryGetValue(entry, out var asset))
            {
                sb.AppendLine(asset.FilePath);
            }
        }
        GUIUtility.systemCopyBuffer = sb.ToString();
        UmaViewerUI.Instance.ShowMessage($"{LoadedAssetEntries.Count} Path copied", UIMessageType.Success);
    }
}
