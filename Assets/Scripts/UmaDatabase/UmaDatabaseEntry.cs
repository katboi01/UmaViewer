using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;

public class UmaDatabaseEntry
{
    public UmaFileType Type;
    public string Name;
    public string Url;
    public string Checksum;
    public string Prerequisites;
    public long Key;
    private byte[] fKey;

    public bool IsEncrypted => Key != 0;

    [NonSerialized]
    public List<UmaDatabaseEntry> CachedPrerequisites = null;

    public byte[] FKey
    {
        get
        {
            if (fKey == null && IsEncrypted)
            {
                var baseKeys = Config.Instance.ABKey;
                int baseLen = baseKeys.Length;

                var keys = new byte[baseLen * 8];

                byte[] keyBytes = BitConverter.GetBytes(Key);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(keyBytes);
                }
                for (int i = 0; i < baseLen; ++i)
                {
                    byte b = baseKeys[i];
                    int baseOffset = i << 3; // i * 8
                    for (int j = 0; j < 8; ++j)
                    {
                        keys[baseOffset + j] = (byte)(b ^ keyBytes[j]);
                    }
                }

                fKey = keys;
            }
            return fKey;
        }
    }

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