using UnityEngine;
using UnityEngine.UI;

public class LiveCharacterSelect : MonoBehaviour
{
    public int CharaId;
    public string CostumeId;
    public Image CharaImage;
    public Image BackGroundImage;
    public Image CostumeImage;
    public Text IndexText;

    public void SelectChara(UmaViewerUI ui) 
    {
        if (!ui) return;
        ui.CurrentSeletChara = this;
        if (!ui.SelectCharacterPannel.activeInHierarchy)
            ui.ToggleUIPanel(ui.SelectCharacterPannel);
    }

    public void SetValue(int charaId, string costumeId,Sprite charaSprite, Sprite costumeSprite)
    {
        CharaId = charaId;
        CostumeId = costumeId;
        CharaImage.sprite = charaSprite;
        BackGroundImage.enabled = true;
        BackGroundImage.sprite = charaSprite;
        CostumeImage.sprite = costumeSprite;
    }
}

