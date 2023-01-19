using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIMusicSelect : MonoBehaviour
{
    private GameObject UIManager = null;

    private UIMusicInfo uIMusicInfo = null;

    public int currentMusic;

    // Use this for initialization
    void Start()
    {
        uIMusicInfo = base.transform.parent.GetComponent<UIMusicInfo>();

        UIManager = GameObject.Find("UIManager");
        LoadingSavedata();
    }

    // Update is called once per frame
    void Update()
    {
        //読みに行く
        if (UIManager == null)
        {
            UIManager = GameObject.Find("UIManager");
        }
    }

    /// <summary>
    /// セーブデータからアイコンをセット
    /// </summary>
    private void LoadingSavedata()
    {
        int music = SaveManager.GetInt("music");
        int music_jacket = SaveManager.GetInt("music_jacket");
        if (music != 0 && music_jacket != 0)
        {
            currentMusic = music;

            string filename = string.Format("jacket_{0:0000}.unity3d", music_jacket);

            StartCoroutine(AssetManager.instance.DownloadFromFilenameAsync(filename));

            StartCoroutine(MusicIconSetter(music, music_jacket));
        }

    }

    /// <summary>
    /// ミュージック選択ボタンをクリックした
    /// </summary>
    public void OnClickMusicSelect()
    {
        GameObject obj = GameObject.Find("Canvas");
        Transform tf = obj.transform.Find("MusicView");
        if (tf != null)
        {
            tf.gameObject.SetActive(true);
        }
        else
        {
            UIManagerMusic mm = UIManager.GetComponent<UIManagerMusic>();
            if (mm.isLoad)
            {
                mm.CreateMusicView();
            }
        }
    }

    /// <summary>
    /// 曲を選択
    /// </summary>
    public IEnumerator MusicIconSetter(int music_data_id, int jacket_id)
    {
        if (music_data_id == 0 || jacket_id == 0)
        {
            Sprite sprite = Resources.Load<Sprite>("noneMusic");
            Image img = base.GetComponent<Image>();
            img.sprite = sprite;

            yield break;
        }

        UIManagerMusic uIManagerMusic = UIManager.GetComponent<UIManagerMusic>();

        //割り込みで読み込む
        StartCoroutine(uIManagerMusic.CreateSprite(jacket_id));

        currentMusic = music_data_id;
        SaveManager.SetInt("music", music_data_id);
        SaveManager.SetInt("music_jacket", jacket_id);
        SaveManager.Save();

        Sprite sp = uIManagerMusic.GetIconSprite(jacket_id);
        if (sp == null)
        {
            while (true)
            {
                sp = uIManagerMusic.GetIconSprite(jacket_id);
                if (sp != null)
                {
                    break;
                }
                yield return null;
            }
        }

        Image image = base.gameObject.GetComponent<Image>();
        image.sprite = sp;

        if (uIManagerMusic.isLoad)
        {
            //音楽データをセット
            yield return uIManagerMusic.SetMusicInfo(music_data_id);
        }
        else
        {
            //まだuIManagerMusicが読み込まれていない場合はDBから直接読み込み
            uIMusicInfo.SetData(music_data_id);
        }
    }
}
