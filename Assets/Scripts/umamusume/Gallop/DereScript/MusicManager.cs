using UnityEngine;
using System.Collections;
using System.IO;

public class MusicManager : MonoBehaviour {

    [SerializeField]
    public static int MusicID;

    private AudioSource audiosource;

    private bool isLoadLive = false;
    private bool isLoadMusic = false;

    public bool isLoad
    {
        get
        {
            if(isLoadLive && isLoadMusic)
            {
                return true;
            }
            return false;
        }
    }
    
    private float totalLiveLength = 0f;
    
    public bool isStart = false;
    public bool isPause = false;

    private float currenttimer;

    /// <summary>
    /// ライブの再生時間
    /// </summary>
    public float totalLength
    {
        get
        {
            return totalLiveLength;
        }
    }

    private void Update()
    {
        if (!isLoad) { return; }

        //ボリューム
        if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus))
        {
            audiosource.volume += 0.01f;
        }
        else if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            audiosource.volume -= 0.01f;
        }
       
        if (isStart)
        {
            //再生が停止している＝ポーズを掛けたor再生時間超過
            if (!audiosource.isPlaying)
            {

                if (currenttimer > audiosource.clip.length)
                {
                    //再生時間超過
                    audiosource.time = audiosource.clip.length;
                    //audiosource.Play();
                    audiosource.Pause();
                }
                else
                {
                    if (!isPause)
                    {
                        audiosource.time = currenttimer;
                        audiosource.Play();
                    }
                }
            }
            else
            {
                //音楽が再生中
                if(Mathf.Abs(currenttimer - audiosource.time) > 0.1f)
                {
                    currenttimer = audiosource.time;
                }
            }
            //currenttimer = audiosource.time;

            if (!isPause)
            {
                currenttimer += Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// 現在の再生時間
    /// </summary>
    public float currentTime
    {
        get
        {
            return currenttimer;
        }

        set
        {
            currenttimer = value;
            float f = currenttimer;
            if (f > audiosource.clip.length)
            {
                f = audiosource.clip.length;
            }
            audiosource.time = f;
        }
    }

    /// <summary>
    /// 音楽を再生する
    /// </summary>
    public void PlaySong()
    {
        if (!isLoad)
        {
            return;
        }
        if (audiosource == null)
        {
            print("audio null");
            return;
        }
        audiosource.Play();
        currenttimer = 0f;
        isStart = true;
    }

    /// <summary>
    /// 音楽の再生を停める
    /// 使用しない
    /// </summary>
    public void StopSong()
    {
        audiosource.Stop();
    }

    /// <summary>
    /// 再生中の曲を一時停止する
    /// </summary>
    public bool PauseSong()
    {
        if (!isPause)
        {
            if (audiosource.isPlaying)
            {
                audiosource.Pause();
            }
            isPause = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 一時停止していた曲を再開する
    /// </summary>
    public bool ResumeSong()
    {
        if (isPause)
        {
            if (!audiosource.isPlaying)
            {
                audiosource.Play();
                isPause = false;
                return true;
            }
        }
        return false;
    }

    void Start()
    {
        var bgmObject = base.gameObject;
        audiosource = bgmObject.GetComponent<AudioSource>();
        StartCoroutine(loadMusic());
        StartCoroutine(Initialize());
    }

    /// <summary>
    /// 音楽をロードする
    /// </summary>
    private IEnumerator loadMusic()
    {        
        /*
        while (true)
        {
            if (ResourcesManager.instance.CheckAudioClip(ViewLauncher.instance.musicDirector.musicFile))
            {
                break;
            }
            yield return null;
        }
        audiosource.clip = ResourcesManager.instance.GetAudioClip(ViewLauncher.instance.musicDirector.musicFile);
        if(audiosource.clip == null)
        {
            print("音楽ファイルがロードできませんでした" + ViewLauncher.instance.musicDirector.musicFile);
            yield break;
        }        
        */
        //キャッシュファイル作成およびパスの取得
        string musicfile = AssetManager.instance.GetCachefromName(ViewLauncher.instance.liveDirector.musicFile);
        while (!File.Exists(musicfile))
        {
            yield return null;
        }
        musicfile = @"file:///" + musicfile;
        //print(musicfile);

        using (WWW www = new WWW(musicfile))
        {
            // ロード完了まで待機
            yield return www;

            audiosource.clip = www.GetAudioClip();

            // ロード開始まで待機
            while (audiosource.clip.loadState == AudioDataLoadState.Unloaded)
            {
                yield return null;
            }
        }
        isLoadMusic = true;
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private IEnumerator Initialize()
    {
        while(true)
        {
            if (ViewLauncher.instance.liveDirector.musicLength != 0f)
            {
                if (ViewLauncher.instance.liveDirector.musicLength > 0)
                {
                    totalLiveLength = ViewLauncher.instance.liveDirector.musicLength;
                    break;
                }
                else
                {
                    if (audiosource != null && audiosource.clip != null)
                    {
                        totalLiveLength = audiosource.clip.length;
                        break;
                    }
                }
            }
            yield return null;
        }

        isLoadLive = true;
    }
}
