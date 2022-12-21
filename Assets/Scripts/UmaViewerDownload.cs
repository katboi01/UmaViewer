using System.Collections;
using System.Collections.Generic;
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
