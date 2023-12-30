using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManagerPhotoStudioStage : MonoBehaviour
{

    /*Prefab*/
    [SerializeField]
    private GameObject PhotoStudioStageView;

    private List<SQPhotoStudioStageData> stageList;
    private Dictionary<int, SQPhotoStudioStageData> stageDic;
    private Dictionary<int, Sprite> iconDic;


    public bool isLoad
    {
        get
        {
            if (stageDic != null)
            {
                if (stageDic.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }


    // Use this for initialization
    void Start()
    {
        StartCoroutine(CreateSpriteDic());
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// フォトスタジオステージ関連のデータをすべて破棄して読み込み直す
    /// </summary>
    public void ResetData()
    {
        if (stageList != null)
        {
            stageList.Clear();
            stageList = null;
        }
        if (stageDic != null)
        {
            stageDic.Clear();
            stageDic = null;
        }
        if (iconDic != null)
        {
            iconDic.Clear();
            iconDic = null;
        }

        StartCoroutine(CreateSpriteDic());
    }

    /// <summary>
    /// MusicIcon用のSprite辞書を生成する
    /// </summary>
    /// <returns></returns>
    private IEnumerator CreateSpriteDic()
    {
        if (iconDic == null)
        {
            iconDic = new Dictionary<int, Sprite>();
        }
        if (stageDic == null)
        {
            stageDic = new Dictionary<int, SQPhotoStudioStageData>();
        }
        if (stageList == null)
        {
            stageList = new List<SQPhotoStudioStageData>();
        }

        while (true)
        {
            if (MasterDBManager.instance.isLoadDB && AssetManager.instance.isManifestLoad)
            {
                break;
            }
            if (MasterDBManager.instance.isError || ManifestManager.instance.isError)
            {
                yield break;
            }
            yield return null;
        }

        while (true)
        {
            if (MasterDBManager.instance != null && MasterDBManager.instance.master3dLive != null)
            {
                break;
            }
            yield return null;
        }

        stageList.AddRange(MasterDBManager.instance.GetPhotoStudioStageData());
        List<string> bgnames = new List<string>();
        foreach (var tmp in stageList)
        {
            //はーつっかえ
            //int musicID = ViewLauncher.instance.master3dLive.GetMusicIDFromBGID(tmp.bg_id);
            int musicID = 0;
            var data = MasterDBManager.instance.GetMusicDataFromID(musicID);
            tmp.bg_fileid = data.live_bg;
            if (tmp.bg_fileid != 0)
            {
                bgnames.Add(string.Format("live_bg2d_bg_live_{0:00}.unity3d", tmp.bg_fileid));
                if (!stageDic.ContainsKey(tmp.id))
                {
                    stageDic.Add(tmp.id, tmp);
                }
            }
        }
        //コルーチンでDLを投げる
        StartCoroutine(AssetManager.instance.DownloadFromFilenamesAsync(bgnames));

        //アイコン作成
        foreach (var tmp in stageList)
        {
            if (tmp.bg_fileid != 0)
            {
                //並列で実行
                StartCoroutine(CreateSprite(tmp.bg_fileid));
            }
        }
    }

    /// <summary>
    /// Spriteを生成し辞書に登録する
    /// </summary>
    public IEnumerator CreateSprite(int iconID)
    {
        //辞書が存在しない
        if (iconDic == null)
        {
            iconDic = new Dictionary<int, Sprite>();
        }
        //既に登録されている
        if (iconDic.ContainsKey(iconID))
        {
            yield break;
        }
        else
        {
            //先に仮登録
            iconDic.Add(iconID, null);
        }

        string name = string.Format("live_bg2d_bg_live_{0:00}.unity3d", iconID);
        //Localのファイル確認
        while (!AssetManager.instance.CheckFileFromFilename(name))
        {
            yield return null;
        }
        //読み込み
        AssetBundle bundleTex = AssetManager.instance.LoadAssetFromName(name);
        if (bundleTex == null)
        {
            yield break;
        }
        Sprite tmpSp = null;
        Texture2D tex2D = null;
        foreach (var obj in bundleTex.LoadAllAssets())
        {
            if (obj is Texture2D)
            {
                tex2D = obj as Texture2D;
                break;
            }
        }
        Rect rect = new Rect(0, 0, tex2D.width, tex2D.height);
        tmpSp = Sprite.Create(tex2D, rect, Vector2.zero);

        bundleTex.Unload(false);

        //既に登録されている
        if (iconDic.ContainsKey(iconID))
        {
            if (iconDic[iconID] == null)
            {
                iconDic[iconID] = tmpSp;
            }
        }
    }

    /// <summary>
    /// Spriteを取得する
    /// </summary>
    public Sprite GetIconSprite(int iconID)
    {
        if (iconDic == null)
        {
            return null;
        }
        if (iconDic.ContainsKey(iconID))
        {
            return iconDic[iconID];
        }
        return null;
    }

    /// <summary>
    /// StageViewを生成し表示
    /// </summary>
    public void CreateStageView()
    {
        GameObject obj = Instantiate(PhotoStudioStageView);
        GameObject baseObj = GameObject.Find("Canvas");
        obj.transform.SetParent(baseObj.transform, false);

        UIPhotoStudioStageView stageView = obj.GetComponent<UIPhotoStudioStageView>();
        stageView.SetStageList(stageList);
    }


    public SQPhotoStudioStageData GetPSStage()
    {
        int stagedata = SaveManager.GetInt("stage", -1);
        SQPhotoStudioStageData data = null;

        if (stageDic.TryGetValue(stagedata, out data))
        {
            return data;
        }
        else
        {
            return MasterDBManager.instance.GetPhotoStudioStageDataFromID(stagedata);
        }
    }
}
