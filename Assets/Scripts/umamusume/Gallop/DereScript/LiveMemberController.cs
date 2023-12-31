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

    /// <summary>
    /// オリジナルメンバーの曲がない場合は押せないようにする
    /// </summary>
    private void setActive(bool active)
    {
        OriginalMember.GetComponent<Button>().interactable = active;
    }
}