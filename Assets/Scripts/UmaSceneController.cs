using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UmaSceneController:MonoBehaviour
{
    public static UmaSceneController instance;
    public GameObject CavansPrefab;
    public GameObject CavansInstance;

    public GameObject LoadingProgressPanel;
    public Slider LoadingProgressSlider;
    public TextMeshProUGUI LoadingProgressText;

    private void Awake()
    {
        if (instance)
        {
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        UmaAssetManager.OnLoadProgressChange += LoadingProgressChange;
    }

    public static void LoadScene(string name, Action OnSceneloaded = null, Action OnLastSceneUnloaded = null)
    {
        instance.StartCoroutine(instance.LoadLiveSceneAsync(name, OnSceneloaded, OnLastSceneUnloaded));
    }

    IEnumerator LoadLiveSceneAsync(string sceneName, Action OnSceneloaded, Action OnLastSceneUnloaded)
    {

        if (CavansInstance)
        {
            //Destroy(CavansInstance);
        }
        CavansInstance = Instantiate(CavansPrefab, transform);
        var animation = CavansInstance.GetComponent<Animation>();
        animation.Play("SceneTransition_s");
        yield return new WaitUntil(() => !animation.isPlaying);

        // Set the current Scene to be able to unload it later
        Scene currentScene = SceneManager.GetActiveScene();

        // The Application loads the Scene in the background at the same time as the current Scene.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Wait until the last operation fully loads to return anything
        yield return new WaitUntil(()=> asyncLoad.isDone);

        OnSceneloaded?.Invoke();

        // Unload the previous Scene
        AsyncOperation asyncUnLoad = SceneManager.UnloadSceneAsync(currentScene);
        yield return new WaitUntil(() => asyncUnLoad.isDone);

        OnLastSceneUnloaded?.Invoke();

        animation.Play("SceneTransition_e");
        yield return new WaitUntil(() => !animation.isPlaying);
        Destroy(CavansInstance);
    }

    public void LoadingProgressChange(int curren, int target)
    {
        if(curren == -1)
        {
            LoadingProgressPanel.SetActive(false);
        }
        else
        {
            LoadingProgressPanel.SetActive(true);
            LoadingProgressText.text = $"Loading...({curren}/{target})";
            LoadingProgressSlider.value = (float)curren / target;
        }
    }
}

