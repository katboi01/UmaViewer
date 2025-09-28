using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Config
{
    public static string configPath = Application.dataPath + "/../Config.json";
    public static Config Instance;
    public string Version = "";

    public string DBBaseKeyTip = "Base key to read game database files";
    public string DBBaseKeyText;

    public string DBKeyTip = "Key to read game database files";
    public string DBKeyText;

    public string ABKeyTip = "Key to read asset bundles";
    public string ABKeyText;

    public string LanguageTip = "Affects Uma names on the list. Language options: 0 - En, 1 - Jp";
    public Language Language = Language.En;

    public string RegionTip = "Game region. Region options: 0 - Global, 1 - Japan";
    public Region Region = Region.Jp;

    public string DownloadMissingResourcesTip = "true/false. automatically download missing files, which may require a VPN.";
    public bool DownloadMissingResources = true;

    public string MainPathTip = "Path to game folder, eg. D:/Backup/Cygames/umamusume";
    public string MainPath = "";

    public string WorkModeTip = "Affects how application work, options: 0 - work with game client, 1 - work without game client(slow), Database needs to be updated manually";
    public WorkMode WorkMode = WorkMode.Default;

    public string VmdKeyReductionLevelTip = "Affects the recording quality: 1 = record every frame, 2 = record every two frames, and so on.";
    public int VmdKeyReductionLevel = 2;

    public string AntiAliasingTip = "Display, screenshot antialiasing level. 0 - no AA, 1 - 2x MSAA, 2 - 4x MSAA, 3 - 8x MSAA";
    public int AntiAliasing = 2;

    public bool RegionDetectionPassed = false;

    public string VmdMorphConvertSettingTip = "The mapping of MMD mprphs to UMA mprphs during VMD recording, multiple UMA expression weights will be combined (not exceeding 1)";
    public List<MorphConvertConfig> VmdMorphConvertSetting = new List<MorphConvertConfig>
    {
        new MorphConvertConfig("にこり右", new string[] { "EyeBrow_1_R" }),
        new MorphConvertConfig("にこり左", new string[] { "EyeBrow_1_L" }),
        new MorphConvertConfig("真面目右", new string[] { "EyeBrow_17_R" }),
        new MorphConvertConfig("真面目左", new string[] { "EyeBrow_17_L" }),
        new MorphConvertConfig("困る右", new string[] { "EyeBrow_6_R", "EyeBrow_12_R" }),
        new MorphConvertConfig("困る左", new string[] { "EyeBrow_6_L" }),
        new MorphConvertConfig("困る２", new string[] { "EyeBrow_13_L", "EyeBrow_20_L", "EyeBrow_10_L" }),
        new MorphConvertConfig("怒り右", new string[] { "EyeBrow_5_R", "EyeBrow_15_R", "EyeBrow_16_R", "EyeBrow_19_R", "EyeBrow_7_R" }),
        new MorphConvertConfig("怒り左", new string[] { "EyeBrow_5_L", "EyeBrow_15_L", "EyeBrow_16_L", "EyeBrow_19_L", "EyeBrow_7_L" }),
        new MorphConvertConfig("怒り２", new string[] { "EyeBrow_18_L" }),
        new MorphConvertConfig("下右", new string[] { "EyeBrow_14_R", "EyeBrow_22_R", "EyeBrow_11_R" }),
        new MorphConvertConfig("下左", new string[] { "EyeBrow_14_L", "EyeBrow_22_L", "EyeBrow_11_L" }),
        new MorphConvertConfig("上右", new string[] { "EyeBrow_21_R" }),
        new MorphConvertConfig("上左", new string[] { "EyeBrow_21_L" }),
        new MorphConvertConfig("眉左", new string[] { "EyeBrow_23_L", "EyeBrow_23_R" }),
        new MorphConvertConfig("眉右", new string[] { "EyeBrow_24_L", "EyeBrow_24_R" }),
        new MorphConvertConfig("ｳｨﾝｸ２右", new string[] { "Eye_2_R" }),
        new MorphConvertConfig("ウィンク２", new string[] { "Eye_2_L" }),
        new MorphConvertConfig("ウィンク右", new string[] { "Eye_5_R", "Eye_8_R" }),
        new MorphConvertConfig("ウィンク", new string[] { "Eye_5_L", "Eye_8_L" }),
        new MorphConvertConfig("ｷﾘｯ", new string[] { "Eye_9_L", "Eye_10_L", "Eye_11_L", "Eye_18_L" }),
        new MorphConvertConfig("なごみ", new string[] { "Eye_16_L" }),
        new MorphConvertConfig("びっくり", new string[] { "Eye_12_L" }),
        new MorphConvertConfig("じと目", new string[] { "Eye_15_L" }),
        new MorphConvertConfig("瞳小", new string[] { "Eye_14_L" }),
        new MorphConvertConfig("笑い目", new string[] { "Eye_19_L" }),
        new MorphConvertConfig("笑い目２", new string[] { "Eye_23_L" }),
        new MorphConvertConfig("ぷく～_左", new string[] { "Mouth_2_0" }),
        new MorphConvertConfig("ぷく～_右", new string[] { "Mouth_3_0" }),
        new MorphConvertConfig("ん", new string[] { "Mouth_9_0" }),
        new MorphConvertConfig("~~", new string[] { "Mouth_44_0", "Mouth_11_0" }),
        new MorphConvertConfig("にっこり", new string[] { "Mouth_12_0" }),
        new MorphConvertConfig("口角上げ", new string[] { "Mouth_13_0" }),
        new MorphConvertConfig("□", new string[] { "Mouth_15_0" }),
        new MorphConvertConfig("▲", new string[] { "Mouth_16_0" }),
        new MorphConvertConfig("ω□", new string[] { "Mouth_17_0" }),
        new MorphConvertConfig("にやり２", new string[] { "Mouth_18_0" }),
        new MorphConvertConfig("にやり3", new string[] { "Mouth_19_0" }),
        new MorphConvertConfig("にやり", new string[] { "Mouth_1_0", "Mouth_4_0", "Mouth_22_0" }),
        new MorphConvertConfig("にぃー", new string[] { "Mouth_26_0" }),
        new MorphConvertConfig("あ", new string[] { "Mouth_5_0", "Mouth_6_0", "Mouth_7_0", "Mouth_23_0" }),
        new MorphConvertConfig("い", new string[] { "Mouth_25_0", "Mouth_35_0", "Mouth_36_0", "Mouth_8_0" }),
        new MorphConvertConfig("う", new string[] { "Mouth_28_0", "Mouth_27_0" }),
        new MorphConvertConfig("お", new string[] { "Mouth_31_0", "Mouth_10_0", "Mouth_14_0" }),
        new MorphConvertConfig("え", new string[] { "Mouth_29_0", "Mouth_30_0", "Mouth_37_0", "Mouth_33_0" }),
        new MorphConvertConfig("口角下げ", new string[] { "Mouth_41_0" }),
        new MorphConvertConfig("ぺろっ", new string[] { "Mouth_45_0" }),
        new MorphConvertConfig("てへぺろ", new string[] { "Mouth_46_0" }),
        new MorphConvertConfig("ぺろっ2", new string[] { "Mouth_47_0" }),
        new MorphConvertConfig("てへぺろ2", new string[] { "Mouth_48_0" }),
        new MorphConvertConfig("ぺろっ3", new string[] { "Mouth_49_0" }),
        new MorphConvertConfig("口上", new string[] { "Mouth_50_0" }),
        new MorphConvertConfig("口下", new string[] { "Mouth_51_0" }),
        new MorphConvertConfig("口左", new string[] { "Mouth_52_0" }),
        new MorphConvertConfig("口右", new string[] { "Mouth_53_0" }),
        new MorphConvertConfig("口横広げ", new string[] { "Mouth_54_0" }),
        new MorphConvertConfig("口横狭い", new string[] { "Mouth_55_0" })
    };

    [NonSerialized]
    public byte[] DBBaseKey = new byte[]
    {
        0xF1, 0x70, 0xCE, 0xA4, 0xDF, 0xCE, 0xA3, 0xE1,
        0xA5, 0xD8, 0xC7, 0x0B, 0xD1, 0x00, 0x00, 0x00
    };

    [NonSerialized]
    public byte[] DBKey = new byte[]
    {
        0x6D, 0x5B, 0x65, 0x33, 0x63, 0x36,
        0x63, 0x25, 0x54, 0x71, 0x2D, 0x73,
        0x50, 0x53, 0x63, 0x38, 0x6D, 0x34,
        0x37, 0x7B, 0x35, 0x63, 0x70, 0x23,
        0x37, 0x34, 0x53, 0x29, 0x73, 0x43,
        0x36, 0x33
    };
    [NonSerialized]
    public byte[] ABKey = new byte[]
    {
        0x53, 0x2B, 0x46, 0x31, 0xE4, 0xA7, 0xB9, 0x47, 0x3E, 0x7C, 0xFB
    };


    public Config()
    {
        Version = Application.version;

        MainPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low"}\Cygames\umamusume";
        if (Application.isMobilePlatform)
        {
            WorkMode = WorkMode.Standalone;
            DownloadMissingResources = true;
            MainPath = Application.persistentDataPath;
            Instance = this;
            return;
        }

        if (!File.Exists(configPath))
        {
            DBBaseKeyText = ByteArrayToHex(DBBaseKey);
            DBKeyText = ByteArrayToHex(DBKey);
            ABKeyText = ByteArrayToHex(ABKey);
            File.WriteAllText(configPath, JsonUtility.ToJson(this, true));
        }
        else
        {
            try
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(configPath), this);

                if (string.IsNullOrEmpty(DBBaseKeyText))
                {
                    DBBaseKeyText = ByteArrayToHex(DBBaseKey);
                }
                else
                {
                    DBBaseKey = HexToByteArray(DBBaseKeyText);
                }
                if (string.IsNullOrEmpty(DBKeyText))
                {
                    DBKeyText = ByteArrayToHex(DBKey);
                }
                else
                {
                    DBKey = HexToByteArray(DBKeyText);
                }
                if (string.IsNullOrEmpty(ABKeyText))
                {
                    ABKeyText = ByteArrayToHex(ABKey);
                }
                else
                {
                    ABKey = HexToByteArray(ABKeyText);
                }
                File.WriteAllText(configPath, JsonUtility.ToJson(this, true));
            }
            catch (Exception ex)
            {
                UmaViewerUI.Instance.ShowMessage("Config load error. Using default. " + ex.Message, UIMessageType.Error);
                MainPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low"}\Cygames\umamusume";
            }
        }
        Instance = this;
    }

    public void UpdateConfig(bool requireRestart)
    {
        File.WriteAllText(configPath, JsonUtility.ToJson(this, true));
        if (requireRestart)
        {
            UmaViewerUI.Instance.ShowMessage("The configuration has changed. Please restart the application.", UIMessageType.Default);
        }
    }

    private string ByteArrayToHex(byte[] byteArray)
    {
        if (byteArray == null) return string.Empty;

        StringBuilder sb = new StringBuilder(byteArray.Length * 2);
        foreach (byte b in byteArray)
        {
            sb.AppendFormat("{0:X2}", b);
        }
        return sb.ToString();
    }

    private byte[] HexToByteArray(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
            return new byte[0];

        byte[] result = new byte[hex.Length / 2];
        for (int i = 0; i < result.Length; i++)
        {
            string byteString = hex.Substring(i * 2, 2);
            result[i] = byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber);
        }
        return result;
    }
}

public enum Language
{
    En,
    Jp
}

public enum Region
{
    Global,
    Jp
}

public enum WorkMode
{
    Default,
    Standalone
}

[Serializable]
public class MorphConvertConfig
{
    public string MMDMorph;
    public List<string> UMAMorph;

    public MorphConvertConfig(string mmdMorph, string[] umaMorphs)
    {
        UMAMorph = new List<string>(umaMorphs);
        MMDMorph = mmdMorph;
    }
}