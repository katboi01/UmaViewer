#if !UNITY_ANDROID || UNITY_EDITOR
using SFB;
#endif
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                System.Diagnostics.Process.Start("explorer.exe", "/select," + filePath);
            }
        };
        LoadedAssetEntries.Add(entry.Name, assetentry);
        LoadedAssetPageCtrl.AddEntries(assetentry);
    }

    public void LoadedAssetsRemove(UmaDatabaseEntry entry)
    {
        if (!LoadedAssetPageCtrl) return;
        if (!LoadedAssetEntries.ContainsKey(entry.Name)) return;
        var pageEntry = LoadedAssetEntries[entry.Name];
        LoadedAssetEntries.Remove(entry.Name);
        LoadedAssetPageCtrl.RemoveEntry(pageEntry);
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

    public void CopyAllLoadedAssets()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        var path = StandaloneFileBrowser.OpenFolderPanel("Select Folder", Config.Instance.MainPath, false);
        if (path != null && path.Length > 0 && !string.IsNullOrEmpty(path[0]))
        {
            foreach (var entry in LoadedAssetEntries.Keys)
            {
                if (!UmaViewerMain.Instance.AbList.TryGetValue(entry, out var asset)) continue;

                var outName = Path.Combine(path[0], asset.Name);
                if (File.Exists(asset.FilePath) && !File.Exists(outName))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outName));

                    if (asset.IsEncrypted)
                    {
                        using var assetStream = new UmaAssetBundleStream(asset.FilePath, asset.FKey);
                        using var outputStream = new FileStream(outName, FileMode.Create);
                        assetStream.CopyTo(outputStream);
                    }
                    else
                    {
                        File.Copy(asset.FilePath, outName);
                    }
                }
            }
            UmaViewerUI.Instance.ShowMessage($"{LoadedAssetEntries.Count} files copied", UIMessageType.Success);
        }
#else
        UmaViewerUI.Instance.ShowMessage("Not supported on this platform", UIMessageType.Warning);
#endif
    }

    public void CopyCurrentUmaAssets()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        if(UmaViewerBuilder.Instance.CurrentUMAContainer == null)
        {
            UmaViewerUI.Instance.ShowMessage("No Umamusume loaded", UIMessageType.Warning);
            return;
        }

        var assets = UmaViewerBuilder.Instance.CurrentUMAContainer.GetAssetsList();

        var path = StandaloneFileBrowser.OpenFolderPanel("Select Folder", Config.Instance.MainPath, false);
        if (path != null && path.Length > 0 && !string.IsNullOrEmpty(path[0]))
        {
            foreach (var asset in assets)
            {
                var outName = Path.Combine(path[0], asset.Name);
                if (File.Exists(asset.FilePath) && !File.Exists(outName))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outName));

                    if (asset.IsEncrypted)
                    {
                        using var assetStream = new UmaAssetBundleStream(asset.FilePath, asset.FKey);
                        using var outputStream = new FileStream(outName, FileMode.Create);
                        assetStream.CopyTo(outputStream);
                    }
                    else
                    {
                        File.Copy(asset.FilePath, outName);
                    }
                }
            }
            UmaViewerUI.Instance.ShowMessage($"{assets.Count} files copied", UIMessageType.Success);
        }
#else
        UmaViewerUI.Instance.ShowMessage("Not supported on this platform", UIMessageType.Warning);
#endif
    }
}
