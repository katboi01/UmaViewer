using System.Collections;
using UnityEngine;

/// <summary>
/// 画面のサイズ変更に合わせて修正する
/// </summary>
public class AdjustScreen : MonoBehaviour
{
    private bool isLoadDirector = false;

    float ratio = 1f;

    private float screenWidth;

    private float screenHeight;

    private Director _director;

    private Director.RenderTarget renderTarget;

    private void Awake()
    {
        _director = GameObject.Find("Live/Director").GetComponent<Director>();

        int resRatio = SaveManager.GetInt("ResolutionRatio", 0);
        switch (resRatio)
        {
            case 0:
                ratio = 1.0f;
                break;
            case 1:
                ratio = 1.4142f;
                break;
            case 2:
                ratio = 1.7320f;
                break;
            case 3:
                ratio = 2.0f;
                break;
        }


        StartCoroutine(LoadRenderTarget());
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (screenWidth != Screen.width || screenHeight != Screen.height)
        {
            if (isLoadDirector)
            {
                ResizeTexture();
            }
        }
        UpdateScreenRect();
    }

    private void UpdateScreenRect()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }

    private IEnumerator LoadRenderTarget()
    {
        while (_director.renderTarget == null)
        {
            yield return null;
        }
        renderTarget = _director.renderTarget;
        isLoadDirector = true;
    }

    private void ResizeTexture()
    {
        int width = (int)(Screen.width * ratio);
        int height = (int)(Screen.height * ratio);

        _director.ResizeScreen(width, height);
    }
}
