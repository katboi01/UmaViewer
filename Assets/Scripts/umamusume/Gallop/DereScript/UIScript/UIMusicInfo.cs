using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMusicInfo : MonoBehaviour
{
    private Text title;
    private Text content;
    
    private GameObject eventToggle;
    private GameObject soloToggle;

    private GameObject musicFrame;

    private GameObject UIManager;

    private Sprite[] frameSprites = null;

    private GameObject liveMemberController;
    
    private void Awake()
    {
        LoadFrameSprite();

        UIManager = GameObject.Find("UIManager");

        title = base.transform.Find("Header/Title").GetComponent<Text>();
        content = base.transform.Find("ScrollView/Viewport/Content").GetComponent<Text>();

        musicFrame = base.transform.Find("MusicSelect/Frame").gameObject;

        eventToggle = base.transform.Find("Header/EventToggle").gameObject;
        soloToggle = base.transform.Find("Header/SoloToggle").gameObject;

        liveMemberController = GameObject.Find("Panel/LiveContent/IdolInfo").gameObject;        
    }

    // Use this for initialization
    void Start()
    {
        int save = SaveManager.GetInt("AnotherSong");
        eventToggle.GetComponent<Toggle>().isOn = save == 1;

        save = SaveManager.GetInt("SoloSong");
        soloToggle.GetComponent<Toggle>().isOn = save == 1;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void LoadFrameSprite()
    {
        frameSprites = new Sprite[5];
        frameSprites[0] = Resources.Load<Sprite>("MusicFrame_cute");
        frameSprites[1] = Resources.Load<Sprite>("MusicFrame_cool");
        frameSprites[2] = Resources.Load<Sprite>("MusicFrame_passion");
        frameSprites[3] = Resources.Load<Sprite>("MusicFrame_all");
        frameSprites[4] = Resources.Load<Sprite>("MusicFrame_other");
    }

    public void SetData(int musicID)
    {
        SQMusicData data = MasterDBManager.instance.GetMusicDataFromID(musicID);
        if (data.name != null)
        {
            SetData(data);
        }
    }

    public void SetData(SQMusicData data)
    {
        liveMemberController.GetComponent<LiveMemberController>().CheckUnitData(data.id);

        string name = data.name;
        name = name.Replace("\\n", "");
        title.text = name;

        string disc = data.discription;
        disc = disc.Replace("\\n", "\n");
        content.text = disc;

        if (frameSprites != null && frameSprites[data.liveAttribute - 1] != null)
        {
            var image = musicFrame.GetComponent<Image>();
            image.sprite = frameSprites[data.liveAttribute - 1];
            musicFrame.SetActive(true);
        }

        //エイプリルフール曲だけ
        if(data.chara_all_flag == 2 && data.music_data_id >1900 && data.music_data_id < 1910)
        {
            eventToggle.SetActive(true);
        }
        else
        {
            eventToggle.SetActive(false);
        }

        CheckSoloSong();
    }

    /// <summary>
    /// ソロ曲があればボタンを表示
    /// </summary>
    public void CheckSoloSong()
    {
        int centerChara = SaveManager.GetChara(2); //2 = center
        int music = SaveManager.GetInt("music");

        //int centerDress = SaveManager.GetInt("dress2");
        //var dress = UIManager.GetComponent<UIManagerDress>().GetDressDataFromKeyID(centerDress);

        //お願いシンデレラはLiveID=1、musicIDは1001
        if (music == 1)
        {
            string filename = string.Format("song_{0}_{1:D3}.acb", 1001, centerChara);
            bool isexist = AssetManager.instance.CheckExistFileInManifest(filename);
            if (isexist)
            {
                soloToggle.SetActive(true);
            }
            else
            {
                soloToggle.SetActive(false);
            }
        }
        else
        {
            soloToggle.SetActive(false);
        }
    }

    /*
    /// <summary>
    /// アナザー用にデータを取得
    /// </summary>
    public void SetAnotherMusic(SQMusicData data)
    {
        if (select != null && another != null)
        {
            if (select.id == data.id || another.id == data.id)
            {
                //前回セットされていた曲がselectもしくはanotherにセットされている
                return;
            }
            else
            {
                //全然違う曲が選択された
                eventToggle.SetActive(false);
                eventToggle.GetComponent<Toggle>().isOn = false;
                select = null;
                another = null;
            }
        }

        if (data.chara_all_flag == 2)
        {
            SQMusicData[] datas = MasterDBManager.instance.GetMusicDatasFromName(data.name);
            if (datas.Length > 0)
            {
                for (int i = 0; i < datas.Length; i++)
                {
                    if (datas[i].id != data.id)
                    {
                        select = data;
                        another = datas[i];
                        break;
                    }
                }
            }
            eventToggle.GetComponent<Toggle>().isOn = false;
            eventToggle.SetActive(true);
        }
    }
    */

    public void OnChangeEventToggle(bool value)
    {
        if (value)
        {
            SaveManager.SetInt("AnotherSong", 1);
        }
        else
        {
            SaveManager.SetInt("AnotherSong", 0);
        }
        SaveManager.Save();
    }

    public void OnChangeSoloToggle(bool value)
    {
        if (value)
        {
            SaveManager.SetInt("SoloSong", 1);
        }
        else
        {
            SaveManager.SetInt("SoloSong", 0);
        }
        SaveManager.Save();
    }
}