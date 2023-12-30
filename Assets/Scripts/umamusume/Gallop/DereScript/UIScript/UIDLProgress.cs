using UnityEngine;
using UnityEngine.UI;

public class UIDLProgress : MonoBehaviour
{
    private Text _text = null;

    private bool isVisible = false;

    // Use this for initialization
    void Start()
    {
        _text = base.transform.Find("DLProgress").gameObject.GetComponent<Text>();

        if (AssetManager.instance != null)
        {
            AssetManager.instance.DLProgress = base.gameObject;
        }

        base.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isVisible != gameObject.activeSelf)
        {
            gameObject.SetActive(isVisible);
        }
    }

    private void OnDestroy()
    {
        if (AssetManager.instance != null)
        {
            AssetManager.instance.DLProgress = null;
        }
    }

    public void SetText(string text)
    {
        if (_text != null)
        {
            if (_text.text != text)
            {
                _text.text = text;
                if (_text.text == "")
                {
                    isVisible = false;
                }
                else
                {
                    isVisible = true;
                }
                gameObject.SetActive(isVisible);
            }
        }
    }
}