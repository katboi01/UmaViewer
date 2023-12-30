using UnityEngine;
using UnityEngine.UI;

public class UIUnitPanel : MonoBehaviour
{
    private Text UnitName;

    public GameObject chara0;
    public GameObject chara1;
    public GameObject chara2;
    public GameObject chara3;
    public GameObject chara4;

    //public Button btUI;
    public Button btSelect;
    public Button btDelete;
    public Button btRename;
    public Button btUpper;
    public Button btDowner;

    public InputField inputField;

    private SaveManager.IdolSet _data;

    public SaveManager.IdolSet data
    {
        get
        {
            return _data;
        }
    }

    private void Awake()
    {
        base.name = base.name.Replace("(Clone)", "");

        UnitName = base.transform.Find("UI/Header/Title").GetComponent<Text>();

        chara0 = base.transform.Find("UI/Chara0").gameObject;
        chara1 = base.transform.Find("UI/Chara1").gameObject;
        chara2 = base.transform.Find("UI/Chara2").gameObject;
        chara3 = base.transform.Find("UI/Chara3").gameObject;
        chara4 = base.transform.Find("UI/Chara4").gameObject;

        btSelect = base.transform.Find("Select").GetComponent<Button>();
        btDelete = base.transform.Find("Delete").GetComponent<Button>();
        btUpper = base.transform.Find("UP").GetComponent<Button>();
        btDowner = base.transform.Find("DOWN").GetComponent<Button>();
        btRename = base.transform.Find("Rename").GetComponent<Button>();

        inputField = base.transform.Find("InputField").GetComponent<InputField>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickUpButton()
    {
        int place = base.transform.GetSiblingIndex();
        if (place > 0) place -= 1;
        base.transform.SetSiblingIndex(place);
    }

    public void OnClickDownButton()
    {
        int place = base.transform.GetSiblingIndex();
        base.transform.SetSiblingIndex(place + 1);
    }

    public void OnClickDeleteButton()
    {
        Destroy(base.gameObject);
    }

    private void SetUnitName(string name)
    {
        _data.unitname = name;
        UnitName.text = _data.unitname;
    }

    public void SetIdolUnit(SaveManager.IdolSet set)
    {
        _data = set;
        SetUnitName(_data.unitname);
    }

    public void OnClickRename()
    {
        inputField.text = UnitName.text;
        inputField.gameObject.SetActive(true);
        inputField.ActivateInputField();

        UnitName.gameObject.SetActive(false);
        btRename.gameObject.SetActive(false);
    }

    public void OnEndEditName()
    {
        string text = inputField.text;

        _data.unitname = text;
        UnitName.text = _data.unitname;

        inputField.gameObject.SetActive(false);
        UnitName.gameObject.SetActive(true);
        btRename.gameObject.SetActive(true);

    }
}