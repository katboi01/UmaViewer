using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class UmaDatabaseEntry
{
    public UmaFileType Type;
    public string Name;
    public string Url;
    public string Checksum;
    public string Prerequisites;
    public AssetBundle LoadedBundle;
    public List<Object> Assets = new List<Object>();
    public Dictionary<Object, string> AssetNames = new Dictionary<Object, string>();

    public bool IsLoaded()
    {
        return UmaAssetManager.Exist(this);
    }

    public Object Get(string name)
    {
        return Assets.FirstOrDefault(a => a.name == name);
    }

    public T Get<T>()
    {
        return (T)System.Convert.ChangeType(Assets.FirstOrDefault(a => a.GetType() == typeof(T)), typeof(T));
    }

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

    public UmaDatabaseEntry Load(bool withtDependencies = true)
    {
        UmaAssetManager.LoadAssetBundle(this, isRecursive: withtDependencies);
        return this;
    }

    public void Unload(bool unloadAllObjects)
    {
        UmaAssetManager.UnloadBundle(this, unloadAllObjects);
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