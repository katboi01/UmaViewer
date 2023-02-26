using System;
using UnityEngine;

/// <summary>
/// OSを設定する
/// </summary>
public class OSConfig
{
    /// <summary>
    /// OSの設定
    /// </summary>
    public static RuntimePlatform os = RuntimePlatform.WindowsPlayer;

    public static string osName
    {
        get
        {
            switch (os)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WindowsPlayer:
                    return "Standalone";
                default:
                    return "";
            }
        }
    }

}
