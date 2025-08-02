using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsScreenshot : MonoBehaviour
{
    public TMP_InputField _width;
    public TMP_InputField _height;
    public Toggle _transparent;
    public TMP_InputField _gifWidth;
    public TMP_InputField _gifHeight;
    public Toggle _gifTransparent;
    public Button GifButton;
    public Button SequenceButton;
    public TMP_Dropdown SequenceFPSDropdown;

    public int Width => int.Parse(_width.text);
    public int Height => int.Parse(_height.text);
    public bool Transparent => _transparent.isOn;

    public int GifWidth => int.Parse(_gifWidth.text);
    public int GifHeight => int.Parse(_gifHeight.text);
    public bool GifTransparent => _gifTransparent.isOn;
    public int GifFPS {
        get
        {
            switch (SequenceFPSDropdown.value)
            {
                case 1: return 30;
                case 2: return 60;
                case 3: return 120;
                default: return -1;
            }
        }
    }
}
