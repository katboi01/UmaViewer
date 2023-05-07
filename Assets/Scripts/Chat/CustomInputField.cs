using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System;
using UnityEngine.Audio;
using System.IO;
using System.Linq;

public class CustomInputField : MonoBehaviour
{
    public TMP_InputField inputField;
    public AudioSource audioSource;
    private string speaker_id;
    private const float typingSpeed = 0.2f;
    private const string serverUrl = "http://127.0.0.1:8000/chat/";
    private string user_id;
    UmaViewerMain Main => UmaViewerMain.Instance;
    private UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

    void Start()
    {
        if (inputField != null)
        {
            inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            inputField.caretWidth = 0; // 将 Caret Width 设置为 0
        }

        // 生成随机的八位字符串作为 user_id
        user_id = Guid.NewGuid().ToString("N").Substring(0, 8);

        // 在场景中查找所有 GameObject
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        // 遍历所有的 GameObject
        foreach (GameObject go in allGameObjects)
        {
            // 判断 GameObject 的名字是否以 "Chara_" 开头
            if (go.name.StartsWith("Chara_"))
            {
                speaker_id = go.name;
            }
        }
    }

    void Update()
    {
        if (inputField == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                inputField.text += System.Environment.NewLine;
            }
            else
            {
                string user_question = inputField.text;
                StartCoroutine(SubmitForm(user_question));
            }
        }
    }

    IEnumerator SubmitForm(string user_question)
    {
        inputField.text = "……";

        // 创建请求
        UnityWebRequest www = new UnityWebRequest(serverUrl, "POST");
        www.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");

        InputData inputData = new InputData { speaker_id = speaker_id, user_question = user_question, user_id = user_id };
        string jsonData = JsonUtility.ToJson(inputData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string response = www.downloadHandler.text;
            // 解析返回的文本和音频数据
            var jsonResponse = JsonUtility.FromJson<ResponseData>(response);
            string text = jsonResponse.answer;
            string audioData = jsonResponse.audio_base64;
            byte[] audioBytes = Convert.FromBase64String(audioData);
            string action = jsonResponse.action;
            string expression = jsonResponse.expression;
            float expression_weight = jsonResponse.expression_weight;

            inputField.text = "";

            bool textFinished = false;
            bool audioFinished = false;

            // 开始播放第一个动画
            LoadAnimationForCurrentCharacter(action);

            StartCoroutine(TypeText(text, () => {
                textFinished = true;
                if (audioFinished)
                {
                    StartCoroutine(DelayedClearText(1.0f));
                    //// 开始播放第二个动画
                    //LoadAnimationForCurrentCharacter("SecondAnimationName");
                }
            }));

            StartCoroutine(PlayAudio(audioBytes, () => {
                audioFinished = true;
                if (textFinished)
                {
                    StartCoroutine(DelayedClearText(1.0f));
                    //// 开始播放第二个动画
                    //LoadAnimationForCurrentCharacter("SecondAnimationName");
                }
            }));
        }
    }

    [Serializable]
    public class InputData
    {
        public string speaker_id;
        public string user_question;
        public string user_id; // 添加 user_id
    }

    IEnumerator TypeText(string text, Action onFinished)
    {
        inputField.text = "";
        foreach (char c in text)
        {
            inputField.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        onFinished?.Invoke();
    }

    IEnumerator PlayAudio(byte[] audioBytes, Action onFinished)
    {
        // 将字节数组转换为 float[]
        float[] floatArray = new float[audioBytes.Length / 4];
        Buffer.BlockCopy(audioBytes, 0, floatArray, 0, audioBytes.Length);

        // 将 float[] 转换为 AudioClip
        AudioClip audioClip = AudioClip.Create("responseAudio", floatArray.Length, 1, 22050, false);
        audioClip.SetData(floatArray, 0);

        // 播放音频
        audioSource.clip = audioClip;
        audioSource.Play();

        // 等待音频播放结束
        yield return new WaitForSeconds(audioClip.length);

        onFinished?.Invoke();
    }

    IEnumerator DelayedClearText(float delay)
    {
        yield return new WaitForSeconds(delay);
        inputField.text = "";
    }

    public void LoadAnimationForCurrentCharacter(string animationName)
    {
        if (Builder.CurrentUMAContainer == null)
        {
            Debug.LogError("No character is currently loaded.");
            return;
        }

        // 查找包含所需动画的资源包
        UmaDatabaseEntry animationEntry = Main.AbMotions.FirstOrDefault(entry => Path.GetFileName(entry.Name) == animationName);

        if (animationEntry == null)
        {
            Debug.LogError($"Animation '{animationName}' not found in the available assets.");
            return;
        }

        // 在资源包中加载动画剪辑
        AssetBundle bundle;

        // 检查资源包是否已经加载，如果没有加载，则加载资源包
        if (!Main.LoadedBundles.TryGetValue(animationEntry.Name, out bundle))
        {
            // 加载包含动画剪辑的资源包
            Builder.LoadAsset(animationEntry);

            if (Main.LoadedBundles.TryGetValue(animationEntry.Name, out bundle))
            {
                LoadAnimationClipFromBundle(animationName, animationEntry, bundle);
            }
            //else
            //{
            //    Debug.LogError($"Failed to find loaded AssetBundle: {animationEntry.Name}");
            //}
        }
        else
        {
            LoadAnimationClipFromBundle(animationName, animationEntry, bundle);
        }
    }

    private void LoadAnimationClipFromBundle(string animationName, UmaDatabaseEntry animationEntry, AssetBundle bundle)
    {
        AnimationClip animationClip = bundle.LoadAsset<AnimationClip>(animationEntry.Name);
        if (animationClip != null)
        {
            // 使用现有的LoadAnimation()方法加载动画剪辑
            Builder.LoadAnimation(animationClip);
        }
        else
        {
            Debug.LogError($"Failed to load AnimationClip from the AssetBundle: {animationEntry.Name}");
        }
    }
}

[Serializable]
public class ResponseData
{
    public string answer;
    public string audio_base64;
    public string action;
    public string expression;
    public int expression_weight;
}