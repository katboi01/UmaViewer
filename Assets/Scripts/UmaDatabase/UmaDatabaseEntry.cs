using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class UmaDatabaseEntry
{
    public UmaFileType Type;
    public string Name;
    public string Url;
    public string Checksum;
    public string Prerequisites;

    public string FilePath
    {
        get
        {
            var path = $"{Config.Instance.MainPath}\\dat\\{Url.Substring(0, 2)}\\{Url}";
            if (!File.Exists(path) && Config.Instance.WorkMode == WorkMode.Standalone)
            {
                DownloadAsset(this);
            }
            return path;
        }
    }

    public T Get<T>(bool withDependencies = true)
    {
        UmaAssetManager.LoadAssetBundle(this, isRecursive: withDependencies);
        Object asset = UmaAssetManager.Get(this).LoadAllAssets().FirstOrDefault(a=>a.GetType() == typeof(T));
        return (T)System.Convert.ChangeType(asset, typeof(T));
    }

    public IEnumerable<T> GetAll<T>(bool withDependencies = true)
    {
        UmaAssetManager.LoadAssetBundle(this, isRecursive: withDependencies);
        IEnumerable<Object> assets = UmaAssetManager.Get(this).LoadAllAssets().Where(a => a.GetType() == typeof(T));
        return assets.Select(asset => (T)System.Convert.ChangeType(asset, typeof(T)));
    }

    //TODO Make it async
    public static void DownloadAsset(UmaDatabaseEntry entry)
    {
        var path = $"{Config.Instance.MainPath}\\dat\\{entry.Url.Substring(0, 2)}\\{entry.Url}";
        UmaViewerDownload.DownloadAssetSync(entry, path, delegate (string msg, UIMessageType type)
        {
            UmaViewerUI.Instance.ShowMessage(msg, type);
        });
    }

}