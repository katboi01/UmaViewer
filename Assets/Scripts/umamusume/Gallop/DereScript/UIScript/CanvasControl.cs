using UnityEngine;
using UnityEngine.UI;

public class CanvasControl : MonoBehaviour
{
    private Canvas canvas;

    private CanvasScaler canvasScaler;

    private Vector2 resolution;

    private GameObject Panel;

    private GameObject VerticalMenu;
    private GameObject HorizontalMenu;

    /// <summary>
    /// View系のprefabを表示するGameObject
    /// </summary>
    private GameObject ViewSetter;

    public bool isVertical
    {
        get
        {
            if (resolution.y > 0 && resolution.x > 0)
            {
                //縦長である場合(同じサイズの場合は横長扱い)
                if (resolution.y > resolution.x)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();
        resolution.x = Screen.width;
        resolution.y = Screen.height;

        Panel = GameObject.Find("Canvas/Panel");

        VerticalMenu = GameObject.Find("Panel/VerticalMenu");
        HorizontalMenu = GameObject.Find("Panel/HorizontalMenu");

        if (isVertical)
        {
            //縦表示

            canvasScaler.referenceResolution = new Vector2(800f, 0f);
            canvasScaler.matchWidthOrHeight = 0f; //横にフィット
            VerticalMenu.SetActive(true);
            HorizontalMenu.SetActive(false);

            var _transform = Panel.transform;
            _transform.localPosition = new Vector3(0f, 0f, 0f);
        }
        else
        {
            //横表示

            canvasScaler.referenceResolution = new Vector2(0f, 750f);
            canvasScaler.matchWidthOrHeight = 1f; //縦にフィット
            VerticalMenu.SetActive(false);
            HorizontalMenu.SetActive(true);

            var _transform = Panel.transform;
            _transform.localPosition = new Vector3(-90f, 0f, 0f);
        }
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}