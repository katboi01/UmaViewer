using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class UmaUtility
{
    public static Dictionary<string, Transform> ConvertListToDictionary(List<Transform> transformList)
    {
        Dictionary<string, Transform> dict = new Dictionary<string, Transform>();

        foreach (Transform transform in transformList)
        {
            if (!dict.ContainsKey(transform.name))
            {
                dict.Add(transform.name, transform);
            }
        }
        return dict;
    }

    public static Dictionary<string, Transform> ConvertArrayToDictionary(Transform[] transformArray)
    {
        Dictionary<string, Transform> dict = new Dictionary<string, Transform>();

        for (int i = 0; i < transformArray.Length; i++)
        {
            Transform transform = transformArray[i];
            if (!dict.ContainsKey(transform.name))
            {
                dict.Add(transform.name, transform);
            }
        }
        return dict;
    }

    public static Dictionary<string, T> MergeDictionaries<T>(Dictionary<string, T> dict1, Dictionary<string, T> dict2)
    {
        Dictionary<string, T> mergedDict = new Dictionary<string, T>(dict1);
        foreach (var kvp in dict2)
        {
            if (!mergedDict.ContainsKey(kvp.Key))
            {
                mergedDict.Add(kvp.Key, kvp.Value);
            }
        }
        return mergedDict;
    }

    public static void CopyValues<T>(T from, T to)
    {
        var json = JsonUtility.ToJson(from);
        JsonUtility.FromJsonOverwrite(json, to);
    }

    public static string GetCurrentLyrics(float time, List<UmaLyricsData> lyricsDatas)
    {
        for (int i = lyricsDatas.Count - 1; i >= 0; i--)
        {
            if (lyricsDatas[i].time < time)
            {
                var lyric = lyricsDatas[i].text;
                lyric = lyric.Replace("[COMMA]", ",");
                lyric = lyric.Replace("\\n", "\n");
                return lyric;
            }
        }
        return "";
    }
}

