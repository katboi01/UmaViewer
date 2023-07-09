using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UmaSceneController:MonoBehaviour
{
    public static UmaSceneController instance;
    public GameObject CavansPrefab;
    public GameObject CavansInstance;
    private void Awake()
    {
        if (instance)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void LoadScene(string name, Action OnSceneloaded, Action OnLastSceneUnloaded)
    {
        StartCoroutine(LoadLiveSceneAsync(name, OnSceneloaded, OnLastSceneUnloaded));
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

        OnSceneloaded();

        // Unload the previous Scene
        AsyncOperation asyncUnLoad = SceneManager.UnloadSceneAsync(currentScene);
        yield return new WaitUntil(() => asyncUnLoad.isDone);

        OnLastSceneUnloaded();

        animation.Play("SceneTransition_e");
        yield return new WaitUntil(() => !animation.isPlaying);
        Destroy(CavansInstance);
    }
}

