using UnityEngine;

public class SQCharaData
{
    public string name;
    public string name_kana;
    public int charaID;
    public int modelHeightId;
    public int modelWeightId;
    public int modelBustId;
    public int modelSkinId;
    public int type;
    public int baseCardId;
    public int height;

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

    //別付け(ちひろ用)
    public int iconID
    {
        get
        {
            if (charaID == 20)
            {
                return 2;
            }
            return charaID;
        }
    }
}