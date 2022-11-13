using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UmaViewerDownload : MonoBehaviour
{
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
}
