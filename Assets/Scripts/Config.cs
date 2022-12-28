using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Config
{
    public static Config Instance;
    public string Version = "";
    public string LanguageTip = "Affects Uma names on the list. Language options: 0 - En, 1 - Jp";
    public Language Language = Language.En;
    public string MainPathTip = "Path to game folder, eg. D:\\Backup\\Cygames\\umamusume or D:/Backup/Cygames/umamusume";
    public string MainPath = "";

    public Config()
    {
        //TODO: Updating config file
        Version = Application.version;
        string configPath = Application.dataPath + "/../Config.json";

        MainPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low"}\Cygames\umamusume";
        if (!File.Exists(configPath))
        {
            File.WriteAllText(configPath, JsonUtility.ToJson(this, true));
        }
        else
        {
            try
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(configPath), this);
            }
            catch (Exception ex)
            {
                UmaViewerUI.Instance.LyricsText.text = "Config load error. Using default path. " + ex.Message;
                MainPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low"}\Cygames\umamusume";
            }
        }

        Instance = this;
    }
}

public enum Language
{
    En,
    Jp
}