using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPhotoStudioChara : UIIdolSelect
{
    int chara = -1;
    int charaIcon = -1;
    int dress = -1;
    int dressIcon = -1;

    public GameObject CharaText;

    public GameObject DressText;

    // Use this for initialization
    void Start()
    {
        base.name = base.name.Replace("(Clone)", "");

        //CharaViewの呼び出しに使う番号を付与
        for (int i = 9; i < 10; i++)
        {
            Button button = charas[i].transform.GetComponentInChildren<Button>();
            AddCharaViewEvent(button, i);
        }
        //DressViewの呼び出しに使う番号を付与
        for (int i = 9; i < 10; i++)
        {
            Button button = dresses[i].transform.GetComponentInChildren<Button>();
            AddDressViewEvent(button, i);
        }

        UIManager = GameObject.Find("UIManager");

        LoadingSavedata();

    }

    /// <summary>
    /// セーブデータを読み込み
    /// </summary>
    private void LoadingSavedata()
    {
        chara = SaveManager.GetChara(9);
        charaIcon = SaveManager.GetCharaIcon(9);
        dress = SaveManager.GetDress(9);
        dressIcon = SaveManager.GetDressIcon(9);

        //アイコンファイルを列挙
        List<string> filelist = new List<string>(4);
        if (chara != 0)
        {
            filelist.Add(string.Format("chara_icon_{0:000}_m.unity3d", charaIcon));
        }
        //ドレスアイコン
        if (dress != 0)
        {
            filelist.Add(SQDressData.getIconfilename(dressIcon));
        }
        //アイコンファイルをDL
        StartCoroutine(AssetManager.instance.DownloadFromFilenamesAsync(filelist));

        //アイコンに設定
        if (dress != 0)
        {
            StartCoroutine(CharaIconSetter(chara, charaIcon, 9));
            StartCoroutine(LoadCharaText(chara));
        }
        if (dress != 0)
        {
            StartCoroutine(DressIconSetter(dress, dressIcon, 9));
            StartCoroutine(LoadDressText(dress));
        }
    }

    /// <summary>
    /// 開いたときに読み込んだセーブで復元する
    /// </summary>
    private void RollBackSave()
    {
        SaveManager.SetChara(9, chara);
        SaveManager.SetCharaIcon(9, charaIcon);
        SaveManager.SetDress(9, dress);
        SaveManager.SetDressIcon(9, dressIcon);
        SaveManager.Save();
    }

    /// <summary>
    /// セーブを削除して消す
    /// </summary>
    private void ResetSave()
    {
        SaveManager.SetChara(9, 0);
        SaveManager.SetCharaIcon(9, 0);
        SaveManager.SetDress(9, 0);
        SaveManager.SetDressIcon(9, 0);
        SaveManager.Save();

        chara = 0;
        charaIcon = 0;
        dress = 0;
        dressIcon = 0;

        StartCoroutine(CharaIconSetter(-1, -1, 9));
        StartCoroutine(DressIconSetter(-1, -1, 9));

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void ReLoadIcons()
    {
        dress = SaveManager.GetDress(9);
        dressIcon = SaveManager.GetDressIcon(9);

        //アイコンファイルを列挙
        List<string> filelist = new List<string>(4);
        if (dress != 0)
        {
            filelist.Add(SQDressData.getIconfilename(dressIcon));
        }
        //アイコンファイルをDL
        StartCoroutine(AssetManager.instance.DownloadFromFilenamesAsync(filelist));

        StartCoroutine(DressIconSetter(dress, dressIcon, 9));
        StartCoroutine(LoadDressText(dress));
    }

    public override IEnumerator CharaIconSetter(int charaID, int charaIcon, int charaPlace)
    {
        StartCoroutine(LoadCharaText(charaID));

        //キャラクタが変更になった場合、ドレスアイコンを変更
        int currentChara = SaveManager.GetChara(charaPlace);
        if (currentChara != charaID)
        {
            //dressIcon=1 ※初期ドレスに設定
            int setDress = 1;
            switch (charaID)
            {
                case 20:
                    setDress = 900001;
                    break;
                case 48:
                    setDress = 900009;
                    break;
                case 49:
                    setDress = 900011;
                    break;
                case 50:
                    setDress = 900013;
                    break;
                case 83:
                    setDress = 900019;
                    break;
            }
            StartCoroutine(LoadDressText(setDress));
        }

        StartCoroutine(base.CharaIconSetter(charaID, charaIcon, charaPlace));
        yield return null;
    }

    public override IEnumerator DressIconSetter(int dressID, int dressIcon, int dressPlace)
    {
        StartCoroutine(LoadDressText(dressID));

        StartCoroutine(base.DressIconSetter(dressID, dressIcon, dressPlace));
        yield return null;
    }

    public IEnumerator LoadCharaText(int charaId)
    {
        if (charaId != 0)
        {
            UIManagerChara mc = UIManager.GetComponent<UIManagerChara>();
            while (true)
            {
                if (mc.isLoad)
                {
                    break;
                }
                yield return null;
            }

            SQCharaData sQCharaData = mc.GetCharaData(charaId);
            CharaText.GetComponent<Text>().text = sQCharaData.name;
        }

    }

    public IEnumerator LoadDressText(int dressId)
    {
        if (dressId != 0)
        {
            UIManagerDress uIManagerDress = UIManager.GetComponent<UIManagerDress>();
            while (true)
            {
                if (uIManagerDress.isLoad)
                {
                    break;
                }
                yield return null;
            }
            SQDressData sqData = uIManagerDress.GetDressDataFromKeyID(dressId);
            DressText.GetComponent<Text>().text = sqData.dressKeyName;
        }

    }
}
