using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gallop;
using System.IO;

public class UmaDatabaseEntry
{
    public UmaFileType Type;
    public string Name;
    public string Url;
    public string Checksum;
    public string Prerequisites;
    public string FilePath 
    {
        get 
        {
            var path = $"{Config.Instance.MainPath}\\dat\\{Url.Substring(0, 2)}\\{Url}";
            if (!File.Exists(path) && Config.Instance.WorkMode == WorkMode.Standalone)
            {
                DownloadAsset(this);
            }
            return path;
        }
    } 

    //TODO Make it async
    public static void DownloadAsset(UmaDatabaseEntry entry)
    {
        var path = $"{Config.Instance.MainPath}\\dat\\{entry.Url.Substring(0, 2)}\\{entry.Url}";
        UmaViewerDownload.DownloadAssetSync(entry, path , delegate(string msg) 
        {
            UmaViewerUI.Instance.ShowMessage(msg);
        });
    }

}

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

    private float _weight;
    public float Weight
    {
        get
        {
            return _weight;
        }
        set
        {
            _weight = value;
            target.UpdateAllFacialKeyTargets();
        }
    }
}

[System.Serializable]
public class CharaEntry
{
    public string Name;
    public Sprite Icon;
    public int Id;
    public string ThemeColor;
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


[System.Serializable]
public class CostumeEntry
{
    public string Id;
    public int CharaId;
    public int BodyType;
    public int BodyTypeSub;
    public Sprite Icon;
}

public enum UmaFileType
{
    _3d_cutt,
    announce,
    atlas,
    bg,
    chara,
    font,
    gacha,
    gachaselect,
    guide,
    home,
    imageeffect,
    item,
    lipsync,
    live,
    loginbonus,
    manifest,
    manifest2,
    manifest3,
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
    collectevent
}
