using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using static UmaViewerUI;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

public class UmaViewerDownload : MonoBehaviour
{
    public static string MANIFEST_ROOT_URL = "https://prd-storage-app-umamusume.akamaized.net/dl/resources/Manifest";
    public static string GENERIC_BASE_URL = "https://prd-storage-game-umamusume.akamaized.net/dl/resources/Generic";
#if UNITY_ANDROID && !UNITY_EDITOR
    public static string ASSET_BASE_URL = "https://prd-storage-game-umamusume.akamaized.net/dl/resources/Android/assetbundles/";
#else
    public static string ASSET_BASE_URL = "https://prd-storage-game-umamusume.akamaized.net/dl/resources/Windows/assetbundles/";
#endif

    private const int maxConcurrentDownloads = 30;
    private static SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrentDownloads);
    private static List<Coroutine> downloadCoroutines = new List<Coroutine>();
    private static int CurrentCoroutinesCount = 0;
    private static WaitUntil downloadWaitUntil = new WaitUntil(() => CurrentCoroutinesCount < maxConcurrentDownloads);
    private static WaitUntil downloadWaitUntilComplete = new WaitUntil(() => CurrentCoroutinesCount == 0);
    private static List<Task> downloadTasks = new List<Task>();

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

    public static void DownloadAssetSync(UmaDatabaseEntry entry, Action<string , UIMessageType> callback = null)
    {
        string baseurl = entry.IsAssetBundle ? GetAssetRequestUrl(entry.Url) : GetGenericRequestUrl(entry.Url);

        using UnityWebRequest www = UnityWebRequest.Get(baseurl);
        www.SendWebRequest();
        while (!www.isDone) { }
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
            callback?.Invoke($"Failed to download resources : {www.error}", UIMessageType.Error);
        }
        else
        {
            Debug.Log("saving " + entry.Url);
            Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
            File.WriteAllBytes(entry.Path, www.downloadHandler.data);
        }
    }
  
    public static IEnumerator DownloadAssets(List<UmaDatabaseEntry> entries, Action<int, int, string> callback = null)
    {
        entries = entries.Where(e => !File.Exists(e.Path)).ToList();
        if (entries.Count == 0) yield break;
        callback?.Invoke(0, entries.Count, "DownLoading");
        var percent_num = (float)entries.Count / 100;
        for(int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            yield return downloadWaitUntil;
            if(i % percent_num == 0)
            {
                callback?.Invoke(i, entries.Count, "DownLoading");
            }
            CurrentCoroutinesCount++;
            semaphore.WaitAsync();
            downloadCoroutines.Add(Instance.StartCoroutine(DownloadTask(entry)));
        }

        yield return downloadWaitUntilComplete;
    }

    public static IEnumerator DownloadTask(UmaDatabaseEntry entry)
    {
        string baseurl = (string.IsNullOrEmpty(Path.GetExtension(entry.Name)) ? GetAssetRequestUrl(entry.Url) : GetGenericRequestUrl(entry.Url));
        using (UnityWebRequest www = UnityWebRequest.Get(baseurl))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                if (Instance)
                {
                    Instance.ShowMessage($"Failed to download resources : {www.error}", UIMessageType.Error);
                }
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
                File.WriteAllBytes(entry.Path, www.downloadHandler.data);
            }
        }
        CurrentCoroutinesCount--;
    }

    public static async void DownloadAssets(IEnumerable<UmaDatabaseEntry> entrys)
    {
        downloadTasks.Clear();
        foreach (var entry in entrys)
        {
            await semaphore.WaitAsync();
            downloadTasks.Add(DownloadTask(entry, semaphore));
        }
        await Task.WhenAll(downloadTasks);
    }

    public static async Task DownloadTask(UmaDatabaseEntry entry, SemaphoreSlim semaphore)
    {
        if (!File.Exists(entry.Path))
        {
            string baseurl = (string.IsNullOrEmpty(Path.GetExtension(entry.Name)) ? GetAssetRequestUrl(entry.Url) : GetGenericRequestUrl(entry.Url));
            using UnityWebRequest www = UnityWebRequest.Get(baseurl);
            www.SendWebRequest();
            await Task.Run(() => { while (!www.isDone) { } });
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("saving " + entry.Url);
                Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
                File.WriteAllBytes(entry.Path, www.downloadHandler.data);
            }
        }   
        semaphore.Release();
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
