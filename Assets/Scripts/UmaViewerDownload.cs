using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class UmaViewerDownload : MonoBehaviour
{
    public static string MANIFEST_ROOT_URL = "https://prd-storage-app-umamusume.akamaized.net/dl/resources/Manifest";
    public static string GENERIC_BASE_URL = "https://prd-storage-umamusume.akamaized.net/dl/resources/Generic";
    public static string ASSET_BASE_URL = "https://prd-storage-umamusume.akamaized.net/dl/resources/Windows/assetbundles/";

    public static IEnumerator DownloadText(string url, System.Action<string> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = 3;
        yield return www.SendWebRequest();
        
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            callback("");
        }
        else
        {
            callback(www.downloadHandler.text);
        }
    }

    public static void DownloadAssetSync(UmaDatabaseEntry entry,string path, Action<string> callback = null)
    {
        string baseurl = GetAssetRequestUrl(entry.Url);
        if (!string.IsNullOrEmpty(Path.GetExtension(entry.Name)))
        {
            baseurl = GetGenericRequestUrl(entry.Url);
        }

        using UnityWebRequest www = UnityWebRequest.Get(baseurl);
        www.SendWebRequest();
        while (!www.isDone) { }
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
            callback?.Invoke($"Failed to download resources : {www.error}");
        }
        else
        {
            Debug.Log("saving " + entry.Url);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, www.downloadHandler.data);
        }
    }

    public static string GetManifestRequestUrl(string hash)
    {
        return $"{MANIFEST_ROOT_URL}/{hash.Substring(0, 2)}/{hash}";
    }

    public static string GetGenericRequestUrl(string hash)
    {
        return $"{GENERIC_BASE_URL}/{hash.Substring(0, 2)}/{hash}";
    }
    
    public static string GetAssetRequestUrl(string hash)
    {
        return $"{ASSET_BASE_URL}/{hash.Substring(0, 2)}/{hash}";
    }
}
