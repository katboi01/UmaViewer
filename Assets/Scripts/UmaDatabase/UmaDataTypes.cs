using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//DO NOT SERIALIZE OR FPS DIES
public class UmaDatabaseEntry
{
    public UmaFileType Type;
    public string Name;
    public string Url;
    public string Checksum;
    public string Prerequisites;
}

public class UmaCharaData
{
    public string id { get; set; }
    public string tail_model_id { get; set; }
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
}
