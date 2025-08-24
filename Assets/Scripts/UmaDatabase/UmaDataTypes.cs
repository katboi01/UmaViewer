using System.Collections.Generic;
using UnityEngine;
using Gallop;
using System;

public class UmaCharaData
{
    public int id;
    public string tail_model_id;
}

public class UmaHeadData
{
    public int id;
    public string costumeId;
    public int tailId;
    public CharaEntry chara;
}

public class UmaLyricsData
{
    public float time;
    public string text;
}

[System.Serializable]
public class EmotionKey
{
    public FacialMorph morph;
    public float weight;
}

[System.Serializable]
public class FaceTypeData
{
    public string label, eyebrow_l, eyebrow_r, eye_l, eye_r, mouth, inverce_face_type;
    public int mouth_shape_type, set_face_group;
    public FaceEmotionKeyTarget target;
    public List<EmotionKey> emotionKeys;
    public float weight;
}

[System.Serializable]
public class CharaEntry
{
    public string Name;
    public string EnName;
    public Sprite Icon;
    public int Id;
    public string ThemeColor;
    public bool IsMob;
    public string GetName()
    {
        return string.IsNullOrEmpty(EnName) ? Name : EnName;
    }
}

[System.Serializable]
public class LiveEntry
{
    public int MusicId;
    public string SongName;
    public int MemberCount;
    public int DefaultDress;
    public string BackGroundId;
    public Sprite Icon;
    public List<string[]> LiveSettings = new List<string[]>();

    public LiveEntry(string data)
    {
        string[] lines = data.Split("\n"[0]);
        for (int i = 1; i < lines.Length; i++)
        {
            LiveSettings.Add(lines[i].Split(','));
        }
        BackGroundId = LiveSettings[1][2];
    }
}

public class PartEntry
{
    public Dictionary<string, List<float>> PartSettings = new Dictionary<string, List<float>>();
    public int SingerCount = 0;
    public PartEntry(string data)
    {
        string[] lines = data.Split('\n');

        string[] names = lines[0].Split(',');

        foreach (var name in names)
        {
            var temp = name;
            temp.Replace("lleft", "left2");
            temp.Replace("rright", "right2");
            temp.Replace("llleft", "left3");
            temp.Replace("rrright", "right3");
            PartSettings[temp] = new List<float>();
        }

        for (int i = 1; i < lines.Length - 1; i++)
        {
            var values = lines[i].Split(',');
            for (int j = 0; j < names.Length; j++)
            {
                PartSettings[names[j]].Add((float)Convert.ToDouble(values[j]));
            }
        }

        foreach(var part in PartSettings)
        {
            if (part.Key != "time" && !part.Key.Contains("_"))
            {
                if (part.Value.FindAll(v => v > 0).Count > 0) 
                {
                    SingerCount += 1;
                }
            }
        } 
    }
}


[System.Serializable]
public class CostumeEntry
{
    public string Id;
    public int CharaId;
    public string DressName;
    public int BodyType;
    public int BodyTypeSub;
    public Sprite Icon;
}

public enum UIMessageType
{
    Default,
    Warning,
    Error,
    Success,
    Close
}
public enum UmaFileType
{
    _3d_cutt,
    announce,
    atlas,
    bg,
    chara,
    font,
    fontresources,
    gacha,
    gachaselect,
    guide,
    heroes,
    home,
    imageeffect,
    item,
    lipsync,
    live,
    loginbonus,
    manifest,
    manifest2,
    manifest3,
    mapevent,
    master,
    minigame,
    mob,
    movie,
    outgame,
    paddock,
    race,
    shader,
    single,
    sound,
    story,
    storyevent,
    supportcard,
    uianimation,
    transferevent,
    teambuilding,
    challengematch,
    collectevent,
    ratingrace,
    jobs,
    manualdownloadatlas,
}

public enum TranslationTables
{
    Costumes = 5,
    UmaNames = 6,
    MobNames = 59,
}