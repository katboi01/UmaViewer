using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UmaSceneController:MonoBehaviour
{
    public static UmaSceneController instance;

    private void Awake()
    {
        if (instance)
        {
            Destroy(this.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void LoadScene(string name, Action onNewSceneLoaded, Action onPrevSceneUnLoaded)
    {
        StartCoroutine(LoadLiveSceneAsync("LiveScene", onNewSceneLoaded, onPrevSceneUnLoaded));
    }

    IEnumerator LoadLiveSceneAsync(string sceneName, Action onNewSceneLoaded, Action onPrevSceneUnLoaded)
    {
        AsyncOperation asyncTransitionLoad = SceneManager.LoadSceneAsync("TransitionScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncTransitionLoad.isDone);

        Animation loadingCanvas = GameObject.Find("LoadingCanvas").GetComponent<Animation>();
        loadingCanvas.Play("SceneTransition_s");
        yield return new WaitUntil(() => !loadingCanvas.isPlaying);

        // Set the current Scene to be able to unload it later
        Scene currentScene = SceneManager.GetActiveScene();

        // The Application loads the Scene in the background at the same time as the current Scene.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Wait until the last operation fully loads to return anything
        yield return new WaitUntil(()=> asyncLoad.isDone);

        onNewSceneLoaded.Invoke();

        // Unload the previous Scene
        AsyncOperation asyncUnLoad = SceneManager.UnloadSceneAsync(currentScene);
        yield return new WaitUntil(() => asyncUnLoad.isDone);


        onPrevSceneUnLoaded.Invoke();

        loadingCanvas.Play("SceneTransition_e");
        yield return new WaitUntil(() => !loadingCanvas.isPlaying);

        // Unload the Transition Scene
        AsyncOperation asyncTransitionUnLoad = SceneManager.UnloadSceneAsync("TransitionScene");
        yield return new WaitUntil(() => asyncTransitionUnLoad.isDone);
    }
}

