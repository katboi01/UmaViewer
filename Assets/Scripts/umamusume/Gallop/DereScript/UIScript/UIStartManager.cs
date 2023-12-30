using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// MenuからStartを押した際にViewに渡す変数を取りまとめる
/// 最終的にViewLancherに引き渡してSceneを呼ぶ
/// </summary>
public class UIStartManager : MonoBehaviour
{
    protected enum ViewType
    {
        LiveView = 0,
        PhotoStudioView = 1
    }

    private bool isHighQuality = true;

    private Button StartButton = null;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(firstDownload());
    }

    // Update is called once per frame
    void Update()
    {
        if (StartButton == null)
        {
            GameObject startButton = GameObject.Find("B_OK");
            if (startButton == null) { return; }
            Button button = startButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    this.OnClickStart();
                    this.StartButton = null;
                });
                StartButton = button;
            }
        }
    }

    /// <summary>
    /// 必須ファイルの再DL
    /// </summary>
    public void ResetData()
    {
        StartButton = null;
        StartCoroutine(firstDownload());
    }

    /// <summary>
    /// Startボタンを押した時に呼ばれる
    /// </summary>
    public void OnClickStart()
    {
        //現在の状態をセーブ
        SaveManager.Save();

        //HQモデル
        //int save = SaveManager.GetInt("RichModel");
        int save = 1; //リッチモデルで固定
        if (save > 0)
        {
            isHighQuality = true;
            ViewLauncher.instance.isRich = true;
        }
        else
        {
            isHighQuality = false;
            ViewLauncher.instance.isRich = false;
        }

        ViewType viewType;
        int liveSelect = SaveManager.GetInt("ViewSwitch", 1);
        if (liveSelect == 1)
        {
            viewType = ViewType.LiveView;
        }
        else
        {
            viewType = ViewType.PhotoStudioView;
        }
        switch (viewType)
        {
            case ViewType.LiveView:
                StartCoroutine(StartLiveView());
                break;
            case ViewType.PhotoStudioView:
                break;

        }
    }

    /// <summary>
    /// LiveViewを開始する
    /// </summary>
    private IEnumerator StartLiveView()
    {
        List<string> filenames = new List<string>();

        UIManagerChara uIManagerChara = base.GetComponent<UIManagerChara>();
        UIManagerDress uIManagerDress = base.GetComponent<UIManagerDress>();

        //楽曲データ取得
        UIManagerMusic mm = base.GetComponent<UIManagerMusic>();
        SQMusicData musicdata = mm.GetCurrentMusic();
        if (musicdata == null)
        {
            print("楽曲が選択されていません");
            yield break;
        }

        //メインキャラ取得
        SQCharaData[] mainCharaDatas = uIManagerChara.GetCurrentCharas();
        SQDressData[] mainDressDatas = uIManagerDress.GetCurrentDresses();

        //アナザーキャラ取得
        SQCharaData[] additionCharaDatas = uIManagerChara.GetAnotherCharas();
        SQDressData[] additionDressDatas = uIManagerDress.GetAnotherDresses();

        SQCharaData[] charaList = new SQCharaData[15];
        SQDressData[] dressList = new SQDressData[15];

        //並び順を変更する
        //int[] parse = new int[15] { 2, 1, 3, 0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
        charaList[0] = mainCharaDatas[2];
        charaList[1] = mainCharaDatas[1];
        charaList[2] = mainCharaDatas[3];
        charaList[3] = mainCharaDatas[0];
        charaList[4] = mainCharaDatas[4];
        charaList[5] = additionCharaDatas[0];
        charaList[6] = additionCharaDatas[1];
        charaList[7] = additionCharaDatas[2];
        charaList[8] = additionCharaDatas[3];

        dressList[0] = mainDressDatas[2];
        dressList[1] = mainDressDatas[1];
        dressList[2] = mainDressDatas[3];
        dressList[3] = mainDressDatas[0];
        dressList[4] = mainDressDatas[4];
        dressList[5] = additionDressDatas[0];
        dressList[6] = additionDressDatas[1];
        dressList[7] = additionDressDatas[2];
        dressList[8] = additionDressDatas[3];

        //メンバー数だけ使用
        int membercount = musicdata.member_number;
        SQCharaData[] finalizeChara = new SQCharaData[membercount];
        SQDressData[] finalizeDress = new SQDressData[membercount];
        for (int i = 0; i < membercount; i++)
        {
            finalizeChara[i] = charaList[i];
            if (finalizeChara[i] == null)
            {
                finalizeChara[i] = uIManagerChara.GetRandomCharaDataWithoutAprilfool();
            }

            finalizeDress[i] = dressList[i];
            if (finalizeDress[i] == null)
            {
                List<SQDressData> sQDressDatas;
                int save = SaveManager.GetInt("RandomMemberSet");
                switch (save)
                {
                    case 0: //キャラクタのみ＝衣装はデフォルト
                        sQDressDatas = uIManagerDress.GetDressDataFromCharaID(finalizeChara[i].charaID);
                        if (sQDressDatas != null && sQDressDatas.Count > 0)
                        {
                            finalizeDress[i] = sQDressDatas[0]; //先頭の衣装
                        }
                        else
                        {
                            print("衣装が選択されていません");
                            yield break;
                        }
                        break;

                    case 1: //全衣装
                        sQDressDatas = uIManagerDress.GetDressDataFromCharaID(finalizeChara[i].charaID);
                        if (sQDressDatas != null && sQDressDatas.Count > 0)
                        {
                            finalizeDress[i] = sQDressDatas[UnityEngine.Random.Range(0, sQDressDatas.Count - 1)];
                        }
                        else
                        {
                            print("衣装が選択されていません");
                            yield break;
                        }
                        break;
                    case 2: //SSRのみ
                        sQDressDatas = uIManagerDress.GetSSRDressesFromCharaID(finalizeChara[i].charaID);
                        if (sQDressDatas != null && sQDressDatas.Count > 0)
                        {
                            finalizeDress[i] = sQDressDatas[UnityEngine.Random.Range(0, sQDressDatas.Count - 1)];
                        }
                        else
                        {
                            //SSRが見つからなかった場合は初期衣装を検索
                            sQDressDatas = uIManagerDress.GetDressDataFromCharaID(finalizeChara[i].charaID);
                            if (sQDressDatas != null && sQDressDatas.Count > 0)
                            {
                                finalizeDress[i] = sQDressDatas[UnityEngine.Random.Range(0, sQDressDatas.Count - 1)];
                            }
                            else
                            {
                                print("衣装が選択されていません");
                                yield break;
                            }
                        }
                        break;
                }


            }

            if (musicdata.music_data_id == 2023) //O-Ku-Ri-Mo-No Sunday!
            {
                if (i == 2)
                {
                    finalizeChara[i] = charaList[0];
                    finalizeDress[i] = dressList[0];
                }
                if (i == 3)
                {
                    finalizeChara[i] = charaList[1];
                    finalizeDress[i] = dressList[1];
                }
            }
        }

        filenames.AddRange(initLive(musicdata, finalizeChara[0]));
        filenames.AddRange(ViewLauncher.instance.shaderFiles);

        //Cabinetを読み込み(ライブ終了毎に消えるため毎回読み込む必要がある)
        yield return MasterDBManager.instance.LoadCabinet();

        for (int i = 0; i < membercount; i++)
        {
            filenames.AddRange(initChara(finalizeChara, finalizeDress, musicdata));
        }

        //ファイルＤＬ
        yield return AssetManager.instance.DownloadFromFilenames(filenames);

        //DL完了待ち
        yield return AssetManager.instance.WaitDownloadProgress();

        //ファイルをすべてロード
        yield return ResourcesManager.instance.LoadAssetGroup(ViewLauncher.instance.GetCommonAssetList());

        //曲のデコード待ち
        yield return AssetManager.instance.WaitACBDecodeProgress();

        //デバッグの設定を読み込み
        PostEffectSetting.LoadSave();

        //ステージ系のリソースを読み込み(パーティクルがうまく読み込まれないためここへ戻す)
        yield return ResourcesManager.instance.LoadAssetGroup(ViewLauncher.instance.GetStageAssetList());

        //DL終了
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LiveView");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        yield return null;
    }

    private string[] initChara(SQCharaData[] charas, SQDressData[] dresses, SQMusicData live)
    {
        CharaDirector[] charadirector = new CharaDirector[charas.Length];
        List<string> filenames = new List<string>();

        for (int i = 0; i < charas.Length; i++)
        {
            bool isAtt = false;
            SQCharaData charaData = null;
            SQDressData dressData = null;

            //ちひろ、最上、ジュリアが共通衣装を着ていた場合はアペンド扱いとなる
            bool special = false;
            if (charas[i].charaID == 20 || charas[i].charaID == 313 || charas[i].charaID == 314)
            {
                if (dresses[i].dressID < 100)
                {
                    special = true;
                }
            }

            //キャラと衣装が異なる
            if ((dresses[i].charaID > 0 && charas[i].charaID != dresses[i].charaID) || special)
            {
                charaData = GameObject.Find("UIManager").GetComponent<UIManagerChara>().GetCharaData(special ? charas[i].charaID : dresses[i].charaID);
                //キャラが着れる最初のドレス（基本はスターリースカイブライト。それを着れない特殊キャラ用）
                var cdress = GameObject.Find("UIManager").GetComponent<UIManagerDress>().GetDressDataFromCharaID(charas[i].charaID);
                dressData = cdress[0];
                isAtt = true;
            }

            if (isAtt)
            {
                charadirector[i] = new CharaDirector(charas[i], dressData, charaData, dresses[i], live, i);
                Character3DBase.GetAssetBundleList(filenames, charas[i].charaID, dressData.activeDressID, dressData.activeDressID);
                Character3DBase.GetAssetBundleList(filenames, charaData.charaID, dresses[i].activeDressID, dresses[i].activeDressID);
            }
            else
            {
                charadirector[i] = new CharaDirector(charas[i], dresses[i], live, i);
                Character3DBase.GetAssetBundleList(filenames, charas[i].charaID, dresses[i].activeDressID, dresses[i].activeDressID);
            }
        }

        ViewLauncher.instance.SetValue(charadirector);

        return filenames.ToArray();
    }

    /// <summary>
    /// 楽曲データから必要なアセットデータファイルを取得する
    /// </summary>
    private string[] initLive(SQMusicData live, SQCharaData chara)
    {
        LiveDirector liveDirector = null;
        List<string> filenames = new List<string>();

        if (isHighQuality)
        {
            LiveDirectorHQ md = new LiveDirectorHQ(live);

            if (AssetManager.instance.CheckExistFileInManifest(md.stageFile))
            {
                liveDirector = md;
            }
            else
            {
                liveDirector = new LiveDirectorNormal(live);
            }
        }
        else
        {
            liveDirector = new LiveDirectorNormal(live);
        }

        //縦モード判定
        int smart = SaveManager.GetInt("SmartMode");
        if (smart == 1)
        {
            liveDirector.SetIsSmartMode(true);
            bool isexist = AssetManager.instance.CheckExistFileInManifest(liveDirector.cuttFile);
            //縦モードファイルが無ければオフ
            if (!isexist)
            {
                liveDirector.SetIsSmartMode(false);
            }
        }

        //アナザー楽曲判定
        int another = SaveManager.GetInt("AnotherSong");
        if (another == 1)
        {
            liveDirector.SetAnotherMode(true);
            bool isexist = AssetManager.instance.CheckExistFileInManifest(liveDirector.musicFile);
            //アナザーソングファイルが無ければオフ
            if (!isexist)
            {
                liveDirector.SetAnotherMode(false);
            }
        }

        //ソロ楽曲判定
        int solo = SaveManager.GetInt("SoloSong");
        if (solo == 1)
        {
            liveDirector.SetSoloCharaID(chara.charaID);
            bool isexist = AssetManager.instance.CheckExistFileInManifest(liveDirector.musicFile);
            //ソロソングファイルが無ければオフ
            if (!isexist)
            {
                liveDirector.SetSoloCharaID(-1);
            }
        }
        else
        {
            liveDirector.SetSoloCharaID(-1);
        }

        ViewLauncher.instance.liveDirector = liveDirector;
        filenames.AddRange(liveDirector.GetAssetFiles());

        return filenames.ToArray();
    }

    /// <summary>
    /// キャラクターがHQモデルに対応しているか確認
    /// 現状だとエイプリルフール曲だけ非対応？
    /// </summary>
    private bool CheckCharaHQ(SQCharaData charas, SQDressData dresses, SQMusicData music)
    {/*
        CharaDirectorHQ cd = new CharaDirectorHQ(charas, dresses, music, 0);

        return AssetManager.instance.CheckExistFileInManifest(cd.BodyModel);
        */
        return true;
    }

    public IEnumerator firstDownload()
    {
        while (!AssetManager.instance.isManifestLoad)
        {
            if (MasterDBManager.instance.isError || ManifestManager.instance.isError)
            {
                yield break;
            }
            yield return null;
        }

        List<string> filenames = new List<string>();
        filenames.AddRange(ViewLauncher.instance.assetFiles);

        StartCoroutine(AssetManager.instance.DownloadFromFilenamesAsync(filenames));
        //DL完了待ち
        yield return AssetManager.instance.WaitDownloadProgress();

    }
}
