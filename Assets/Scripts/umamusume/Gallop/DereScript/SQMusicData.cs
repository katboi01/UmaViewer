using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SQMusicData
{
    public int id;
    public int music_data_id;
    public string name;
    public string name_kana;
    public int jacket_id;
    public int cyalume;
    public int member_number;
    public int live_bg;
    public int type;

    public int chara_all_flag;

    public string composer;
    public string lyricist;
    public string discription;

    public bool smartmode;

    private GameObject _iconObject;
    public GameObject iconObject
    {
        get
        {
            return _iconObject;
        }
        set
        {
            _iconObject = value;
        }
    }

    public int liveAttribute
    {
        get
        {
            //エイプリルフール曲
            if (type == 4 && music_data_id > 1900 && music_data_id < 2000)
            {
                return 5;
            }
            return type;
        }
    }
}
