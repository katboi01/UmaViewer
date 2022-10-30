using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UmaViewerDownload : MonoBehaviour
{
    public static IEnumerator DownloadText(string url, System.Action<string> callback)
    {
        if (PlayerPrefs.GetString(url, "")=="")
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                PlayerPrefs.SetString(url, www.downloadHandler.text);
                callback(www.downloadHandler.text);
            }
        }
        else
        {
            callback(PlayerPrefs.GetString(url, ""));
        }
        
    }
}
