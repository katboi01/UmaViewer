using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiveMemberController : MonoBehaviour
{
    public bool isLoad
    {
        get
        {
            //辞書にデータがあれば読み込み完了している
            if (posDic.Count > 0)
            {
                return true;
            }
            return false;
        }
    }
    public bool isError = false;
    private List<SQLiveDataPosition> positions = new List<SQLiveDataPosition>();
    private Dictionary<int, SQLiveDataPosition> posDic = new Dictionary<int, SQLiveDataPosition>();

    private GameObject OriginalMember = null;

    private GameObject UIManager = null;

    private void Awake()
    {
        OriginalMember = base.transform.Find("OriginalMember").gameObject;
    }

    // Use this for initialization
    void Start()
    {
        UIManager = GameObject.Find("UIManager");
        StartCoroutine(LoadData());
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
    /// データの読み込み
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadData()
    {
        while (true)
        {
            //読み込み完了まで待つ
            if (MasterDBManager.instance.isLoadDB)
            {
                break;
            }
            //エラー発生時
            if (MasterDBManager.instance.isError)
            {
                yield break;
            }
            yield return null;
        }

        positions = MasterDBManager.instance.GetLiveDataPosition();
        if (positions == null)
        {
            isError = true;
            yield break;
        }
        foreach (var tmp in positions)
        {
            posDic.Add(tmp.liveDataID, tmp);
        }

        //エイプリルフール曲
        SQLiveDataPosition data;
        data = SQLiveDataPosition.GetChihiro201604();
        posDic.Add(data.liveDataID, data);
        data = SQLiveDataPosition.GetPutiderera201704();
        posDic.Add(data.liveDataID, data);
        data = SQLiveDataPosition.GetKirarinLobo201804();
        posDic.Add(data.liveDataID, data);
        data = SQLiveDataPosition.GetNono201904();
        posDic.Add(data.liveDataID, data);

    }

    public void OnClickButton()
    {
        UIManagerMusic musicManager = UIManager.GetComponent<UIManagerMusic>();
        UIManagerDress dressManager = UIManager.GetComponent<UIManagerDress>();

        GameObject baseObj = GameObject.Find("Canvas");
        if (baseObj == null) { return; }

        UIIdolSelect idolSelect = base.GetComponent<UIIdolSelect>();

        SQMusicData data = musicManager.GetCurrentMusic();
        if (data == null)
        {
            return;
        }

        int save = SaveManager.GetInt("SelectOriginalMember");
        bool commdress = false;

        SQLiveDataPosition posdata = null;
        if (posDic.TryGetValue(data.id, out posdata))
        {
            int[] charas = posdata.CharaPositions;
            int[] dresses = posdata.DressPositions;

            for (int i = 0; i < 5; i++)
            {
                //キャラアイコンをセット
                if (charas[i] > 0)
                {
                    StartCoroutine(idolSelect.CharaIconSetter(charas[i], charas[i], i));
                    StartCoroutine(dressManager.DownloadAndCreateDressIcons(charas[i]));
                }

                //ドレスアイコンをセット
                if (dresses[i] > 0)
                {
                    int dress = dresses[i];
                    int dressIcon = dress;
                    if (dress > 7000000)
                    {
                        //ショップドレス
                        dress += charas[i];
                    }
                    else if (dress > 100000 && dress < 500000)
                    {
                        //SSR衣装(現状エイプリルフールのみ)
                        var sqData = dressManager.GetDressDataFromKeyID(dress);
                        if (sqData.charaID == charas[i])
                        {
                            dressIcon = sqData.dressIconKey;
                        }
                    }
                    else if (dress < 100)
                    {
                        commdress = true;
                    }
                    StartCoroutine(idolSelect.DressIconSetter(dress, dressIcon, i));
                }
                else
                {
                    if (charas[i] > 0)
                    {
                        //ドレスが指定されていない場合の処理
                        switch (save)
                        {
                            case 0:
                                StartCoroutine(idolSelect.DressIconSetter(1, 1, i));
                                break;
                            case 1:
                                {
                                    List<SQDressData> dressList = dressManager.GetDressDataFromCharaID(charas[i]);
                                    if (dressList != null)
                                    {
                                        int iResult2 = Random.Range(0, dressList.Count);

                                        StartCoroutine(idolSelect.DressIconSetter(dressList[iResult2].activeDressID, dressList[iResult2].dressIconKey, i));
                                    }
                                }
                                break;
                            case 2:
                                {
                                    List<SQDressData> dressList = dressManager.GetSSRDressesFromCharaID(charas[i]);
                                    if (dressList != null && dressList.Count != 0)
                                    {
                                        int iResult2 = Random.Range(0, dressList.Count);

                                        StartCoroutine(idolSelect.DressIconSetter(dressList[iResult2].activeDressID, dressList[iResult2].dressIconKey, i));
                                    }
                                    else
                                    {
                                        goto case 1;
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            if (commdress)
            {
                int dress = dresses[0];
                for (int i = 5; i < 9; i++)
                {
                    StartCoroutine(idolSelect.DressIconSetter(dress, dress, i));
                }
            }
        }
    }

    /// <summary>
    /// オリジナルユニットのある曲はボタンを有効にする
    /// </summary>
    public bool CheckUnitData(int MusicID)
    {
        if (posDic != null)
        {
            //辞書から曲を検索
            if (posDic.ContainsKey(MusicID))
            {
                setActive(true);
                return true;
            }
        }
        setActive(false);
        return false;
    }

    /// <summary>
    /// オリジナルメンバーの曲がない場合は押せないようにする
    /// </summary>
    private void setActive(bool active)
    {
        OriginalMember.GetComponent<Button>().interactable = active;
    }
}