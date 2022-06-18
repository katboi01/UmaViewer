using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UmaViewerUI : MonoBehaviour
{
    public static UmaViewerUI Instance;

    public ScrollRect CharactersList;
    public ScrollRect CostumeList;
    public ScrollRect AnimationSetList;
    public ScrollRect AnimationList;

    public GameObject UmaContainerPrefab;

    public Color UIColor1, UIColor2;

    private void Awake()
    {
        Instance = this;
    }

    public void HighlightChildImage(Transform mainObject, Image child)
    {
        Debug.Log("Looking for " + child.name + " in " + mainObject.name);
        foreach(var t in mainObject.GetComponentsInChildren<Image>())
        {
            if (t.transform.parent != mainObject) continue;
            t.color = t == child ? UIColor2 : UIColor1;
        }
    }

    public void ToggleUI(GameObject go)
    {
        go.SetActive(!go.activeSelf);
    }
}
