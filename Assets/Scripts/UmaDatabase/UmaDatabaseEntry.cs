using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

public class UmaDatabaseEntry
{
    public UmaFileType Type;
    public string Name;
    public string Url;
    public string Checksum;
    public string Prerequisites;
    public string Key;

    [NonSerialized]
    public List<UmaDatabaseEntry> CachedPrerequisites = null;

    public string FilePath
    {
        get
        {
            if (Config.Instance.DownloadMissingResources && !File.Exists(Path))
            {
                DownloadAsset(this);
            }
            return Path;
        }
    }

    public string Path
    {
        get
        {
            return $"{Config.Instance.MainPath.Replace("\\", "/")}/dat/{Url.Substring(0, 2)}/{Url}";
        }
    }

    public bool IsAssetBundle
    {
        get
        {
            return string.IsNullOrEmpty(System.IO.Path.GetExtension(Name));
        }
    }

    public T Get<T>(bool withDependencies = true)
    {
        Object asset = UmaAssetManager.LoadAssetBundle(this, isRecursive: withDependencies)
            .LoadAllAssets().FirstOrDefault(a=>a.GetType() == typeof(T));
        return (T)System.Convert.ChangeType(asset, typeof(T));
    }

    public IEnumerable<T> GetAll<T>(bool withDependencies = true)
    {
        IEnumerable<Object> assets = UmaAssetManager.LoadAssetBundle(this, isRecursive: withDependencies)
            .LoadAllAssets().Where(a => a.GetType() == typeof(T));
        return assets.Select(asset => (T)System.Convert.ChangeType(asset, typeof(T)));
    }

    //TODO Make it async
    public static void DownloadAsset(UmaDatabaseEntry entry)
    {
        UmaViewerDownload.DownloadAssetSync(entry, delegate (string msg, UIMessageType type)
        {
            UmaViewerUI.Instance.ShowMessage(msg, type);
        });
    }

}