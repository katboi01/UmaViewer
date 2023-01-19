using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIIdolUnitListView : MonoBehaviour
{
    //Prefab
    [SerializeField]
    private GameObject UnitIcon;

    private Transform content;

    private GameObject UIManager = null;

    private GameObject IdolInfo = null;

    private List<GameObject> unitBtnList = new List<GameObject>();

    private ScrollRect scrollRect;

    private void Awake()
    {
        base.name = base.name.Replace("(Clone)", "");

        content = base.transform.Find("Content/UnitList/Viewport/Content").GetComponent<RectTransform>();
        scrollRect = base.transform.Find("Content/UnitList").GetComponent<ScrollRect>();
    }

    private void OnDestroy()
    {
        Save();
    }

    private void WindowSizeSetter()
    {
        //ウィンドウのサイズを取得
        Vector2 windowSize = GameObject.Find("Canvas").GetComponent<RectTransform>().sizeDelta;

        //自分のサイズを取得
        Vector3 mylocalScale = GetComponent<RectTransform>().localScale;
        Vector2 mysize = GetComponent<RectTransform>().sizeDelta;

        //縦長の場合
        if (windowSize.y > windowSize.x)
        {
            //はみ出すため小さくする
            //mylocalScale.x = 0.7f;
            //mylocalScale.y = 0.8f;

            //縦長に変更
            if (windowSize.y > 1160f)
            {
                mysize.y = 1250f;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        UIManager = GameObject.Find("UIManager");
        //多分ないけど非表示の時のため
        var obj = GameObject.Find("Panel");
        IdolInfo =  obj.transform.Find("LiveContent/IdolInfo").gameObject;

        CreateUnitButtons();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CreateUnitButtons()
    {
        List<SaveManager.IdolSet> unitlist = SaveManager.GetUnitList();
        if (unitBtnList == null)
        {
            unitBtnList = new List<GameObject>();
        }
        else
        {
            foreach(var tmp in unitBtnList)
            {
                Destroy(tmp);
            }
            unitBtnList.Clear();
        }

        if (unitlist == null) return;

        //ボタンを生成
        foreach (var tmp in unitlist)
        {
            GameObject btn = Instantiate(UnitIcon);
            unitBtnList.Add(btn);
            StartCoroutine(MakeButton(tmp, btn));
        }
    }

    /// <summary>
    /// ボタン生成単体
    /// </summary>
    private IEnumerator MakeButton(SaveManager.IdolSet data, GameObject btn)
    {
        //Awakeが動くまで待ち
        yield return null;

        UIUnitPanel uIUnitPanel = btn.GetComponent<UIUnitPanel>();
        AddButtonEvent(uIUnitPanel.btSelect, uIUnitPanel);

        UIManagerChara uIManagerChara = UIManager.GetComponent<UIManagerChara>();

        uIUnitPanel.SetIdolUnit(data);

        //スプライトを割り込みで生成
        //無ければ作るし、あったら戻ってくる
        StartCoroutine(IconSetter(uIUnitPanel.chara0, data.charaIcon0));
        StartCoroutine(IconSetter(uIUnitPanel.chara1, data.charaIcon1));
        StartCoroutine(IconSetter(uIUnitPanel.chara2, data.charaIcon2));
        StartCoroutine(IconSetter(uIUnitPanel.chara3, data.charaIcon3));
        StartCoroutine(IconSetter(uIUnitPanel.chara4, data.charaIcon4));

        //親をセット
        btn.transform.SetParent(content, false);
    }

    private IEnumerator IconSetter(GameObject charaIconObj, int iconID)
    {
        UIManagerChara uIManagerChara = UIManager.GetComponent<UIManagerChara>();

        StartCoroutine(uIManagerChara.CreateSprite(iconID));

        Sprite sp = uIManagerChara.GetIconSprite(iconID);
        if (sp == null)
        {
            //nullが返ってくる場合はまだSpriteが作成中の時に呼んでしまった可能性がある

            while (true)
            {
                sp = uIManagerChara.GetIconSprite(iconID);
                if (sp != null)
                {
                    break;
                }
                yield return null;
            }
        }
        Image image = charaIconObj.GetComponent<Image>();
        image.sprite = sp;
    }

    // ボタンに機能を付与する
    void AddButtonEvent(Button button, UIUnitPanel uIUnitPanel)
    {
        button.onClick.AddListener(() => {
            this.OnClick(uIUnitPanel);
            base.gameObject.SetActive(false);
        });
    }

    //選択されたとき
    private void OnClick(UIUnitPanel uIUnitPanel)
    {
        SaveManager.SetIdolUnit(uIUnitPanel.data);
        IdolInfo.GetComponent<UIIdolSelect>().ReLoadIcons();
    }

    public void OnViewClose()
    {
        base.gameObject.SetActive(false);
    }

    public void OnClickAddUnit()
    {
        var unit = SaveManager.GetCurrentUnit();

        unit.unitname = "新規ユニット";

        GameObject btn = Instantiate(UnitIcon);
        unitBtnList.Add(btn);
        StartCoroutine(MakeButton(unit, btn));

        SaveManager.AddUnit(unit);

        //一番下に移動
        StartCoroutine(SetScrollRect(0f));
    }

    public void Save()
    {
        var list = content.GetComponentsInChildren<UIUnitPanel>();

        SaveManager.IdolSet[] datas = new SaveManager.IdolSet[list.Length];

        for(int i = 0; i < list.Length; i++)
        {
            int place = list[i].transform.GetSiblingIndex();
            datas[place] = list[i].data;
        }

        SaveManager.SetUnitList(new List<SaveManager.IdolSet>(datas));
        SaveManager.Save();
    }

    /// <summary>
    /// ScrollRectはすぐには変更されないため1フレーム待つ必要がある
    /// </summary>
    private IEnumerator SetScrollRect(float pos)
    {
        //2フレーム待つ
        yield return null;
        yield return null;

        scrollRect.verticalNormalizedPosition = pos;
    }
}
