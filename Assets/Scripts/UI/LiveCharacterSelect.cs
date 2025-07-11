using UnityEngine;
using UnityEngine.UI;

public class LiveCharacterSelect : MonoBehaviour
{
    public CharaEntry CharaEntry;
    public string CostumeId;
    public string HeadCostumeId;
    public Image CharaImage;
    public Image CostumeImage;
    public Text IndexText;

    public void SelectChara(UmaViewerUI ui) 
    {
        if (!ui) return;
        ui.CurrentSeletChara = this;
        if (!ui.SelectCharacterPannel.activeInHierarchy)
            ui.ToggleUIPanel(ui.SelectCharacterPannel);
    }

    public void SetValue(CharaEntry charaentry, string costumeId, Sprite costumeSprite, string headCostumeId)
    {
        CharaEntry = charaentry;
        CostumeId = costumeId;
        HeadCostumeId = headCostumeId;
        CharaImage.enabled = true;
        CharaImage.sprite = charaentry.Icon;
        CostumeImage.sprite = costumeSprite;
    }
}

