using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsEnvironment : MonoBehaviour
{
    public ScrollRect BackGroundList;
    public PageManager BackGroundPageCtrl;
    public GameObject BG_Canvas;
    public GameObject BG_HSVPickerObj;
    public Image BG_Image;

    static UmaViewerMain Main => UmaViewerMain.Instance;
    static UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

    public void ChangeBackground(int index)
    {
        BackGroundPageCtrl.ResetCtrl();
        switch (index)
        {
            case 0:
                Camera.main.clearFlags = CameraClearFlags.Skybox;
                break;
            case 1:
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                break;
            case 2:
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                ListBackgrounds();
                break;
            default:
                Camera.main.clearFlags = CameraClearFlags.Skybox;
                break;
        }

        BG_HSVPickerObj.SetActive(index != 2);
        BG_Canvas.SetActive(index == 2);
        BackGroundPageCtrl.transform.parent.gameObject.SetActive(index == 2);
    }

    public void ChangeBackgroundColor(Color color)
    {
        Camera.main.backgroundColor = color;
    }

    void ListBackgrounds()
    {
        var pageentrys = new List<PageManager.Entry>();
        foreach (var entry in Main.AbList.Where(a => a.Key.StartsWith("bg/bg")))
        {
            var pageentry = new PageManager.Entry();
            pageentry.Name = Path.GetFileName(entry.Key);
            pageentry.Sprite = Builder.LoadSprite(entry.Value);
            if (pageentry.Sprite == null) continue;
            pageentry.OnClick = (container) =>
            {
                UmaViewerUI.Instance.HighlightChildImage(BackGroundList.content, container);
                BG_Image.sprite = pageentry.Sprite;
                BG_Image.SetVerticesDirty();
            };

            if (BG_Image.sprite == null)
            {
                BG_Image.sprite = pageentry.Sprite;
                BG_Image.SetVerticesDirty();
            }
            pageentrys.Add(pageentry);
        }
        BackGroundPageCtrl.Initialize(pageentrys, BackGroundList);
    }
}
