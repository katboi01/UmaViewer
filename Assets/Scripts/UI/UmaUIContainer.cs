using UnityEngine;
using UnityEngine.UI;

public class UmaUIContainer : MonoBehaviour
{
    enum TextType
    {
        Text,
        TextMesh,
    }

    [SerializeField]TextType TextComponentType = TextType.Text;
    public string Name
    {
        get 
        {
            switch (TextComponentType)
            {
                case TextType.Text: return Text.text;
                case TextType.TextMesh: return TextMesh.text;
                default: return TextMesh.text;
            }
        }
        set 
        {
            switch (TextComponentType)
            {
                case TextType.Text: 
                    Text.text = value;
                    break;
                case TextType.TextMesh:
                    TextMesh.text = value; 
                    break;
            }
        }
    }

    public float FontSize
    {
        get
        {
            switch (TextComponentType)
            {
                case TextType.Text: return Text.fontSize;
                case TextType.TextMesh: return TextMesh.fontSize;
                default: return TextMesh.fontSize;
            }
        }
        set
        {
            switch (TextComponentType)
            {
                case TextType.Text:
                    Text.fontSize = (int)value;
                    break;
                case TextType.TextMesh:
                    TextMesh.fontSize = value;
                    break;
            }
        }
    }
    public TMPro.TextMeshProUGUI TextMesh;
    public Text Text;
    public Button Button;
    public Slider Slider;
    public Toggle Toggle;
    public Image Image;
    public Image ToggleImage;
    public string Id;
}

