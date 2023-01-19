using System;
using System.Runtime.Serialization;
using System.Net;
using UnityEngine;
using MiniJSON;
using System.Collections;
using System.Collections.Generic;

public class VersionCheckerKiraraAPI
{
    private const string url = @"https://starlight.kirara.ca/api/v1/info";

    public int version = 0;

    public IEnumerator Check()
    {
        using (WWW www = new WWW(url))
        {
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError("www Error:" + www.error);
                yield break;
            }
            Debug.Log(www.text);

            Dictionary<string, object> dic;
            dic = Json.Deserialize(www.text) as Dictionary<string, object>;

            if (dic.ContainsKey("truth_version"))
            {
                string ver = dic["truth_version"] as string;
                version = int.Parse(ver);
            }

            Debug.Log("ResVer:" + version);
        }

    }
}