using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionCheckerMatsurihimeAPI
{
    private const string url = @"https://api.matsurihi.me/cgss/v1/version/latest";

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
            if (dic.ContainsKey("res"))
            {
                var res = dic["res"] as Dictionary<string, object>;
                if (res.ContainsKey("version"))
                {
                    Int64 ver = (Int64)res["version"];
                    version = (int)ver;
                }
            }
            Debug.Log("ResVer:" + version);
        }
    }
}