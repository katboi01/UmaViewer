using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Stage;
using Cute;

/*musicScore用*/
using Sqlite3Plugin;

public class LiveDirectorHQ : LiveDirector
{
    public const string StageMobCommon = "3d_stage_mob_common.unity3d";

    public const string StageCommon = "3d_stage_common_hq.unity3d";
    public const string lightShuft1 = "3d_lightshuft_0001.unity3d";
    public const string lightShuft2 = "3d_lightshuft_0002.unity3d";
    public const string lightShuft3 = "3d_lightshuft_0003.unity3d";

    public Master3dLive master3dLive = null;

    private SQMusicData liveData;

    private float _musicLength = 0f;

    private ArrayList _musicScoreCyalumeArray;
    private ArrayList _musicScoreLyricsArray;

    private bool isSmartMode = false;

    private bool anotherMode = false;
    
    private int soloCharaID = -1;

    public bool isHQ
    {
        get
        {
            return true;
        }
    }

    public bool isAnother
    {
        get
        {
            if(liveData.chara_all_flag == 2)
            {
                return true;
            }
            return false;
        }
    }

    public bool isVertical
    {
        get
        {
            return isSmartMode;
        }
    }

    public ArrayList MusicScoreCyalumeArray
    {
        get
        {
            return _musicScoreCyalumeArray;
        }
    }
    public ArrayList MusicScoreLyricsArray
    {
        get
        {
            return _musicScoreLyricsArray;
        }
    }

    public float musicLength
    {
        get
        {
            return _musicLength;
        }
    }

    public LiveDirectorHQ(SQMusicData data)
    {
        liveData = data;
        if(master3dLive == null)
        {
            master3dLive = MasterDBManager.instance.master3dLive;
        }
    }

    /// <summary>
    /// 曲ID
    /// 音楽ファイル等のIDとして利用
    /// </summary>
    public int MusicID
    {
        get
        {
            return liveData.music_data_id;
        }
    }

    /// <summary>
    /// ライブID
    /// cyalumeのIDとして利用
    /// </summary>
    public int LiveID
    {
        get
        {
            return liveData.id;
        }
    }

    /// <summary>
    /// ステージID
    /// CuttファイルのID
    /// </summary>
    public int StageID
    {
        get
        {
            if (live3DData != null)
            {
                return live3DData.bg;
            }
            return 0;
        }
    }

    public int CyalumeType
    {
        get
        {
            return liveData.cyalume;
        }
    }

    public Master3dLive.Live3dData live3DData
    {
        get
        {
            if (master3dLive != null)
            {
                Master3dLive.Live3dData data = master3dLive.dictionary[liveData.id];
                return data;
            }
            else
            {
                return null;
            }
        }
    }

    public string[] GetAssetFiles()
    {
        List<string> filenames = new List<string>();
        filenames.Add(StageCommon);
        filenames.Add(StageMobCommon);
        filenames.Add(lightShuft1);
        filenames.Add(lightShuft2);
        filenames.Add(lightShuft3);
        if (musicScores != null) filenames.Add(musicScores);
        if (cyalumeFile != null) filenames.Add(cyalumeFile);
        if (cuttFile != null) filenames.Add(cuttFile);
        if (stageFile != null) filenames.Add(stageFile);
        if (cameraFile != null) filenames.Add(cameraFile);
        if (musicFile != null) filenames.Add(musicFile);
        if (autoLipFile != null) filenames.Add(autoLipFile);
        if (uvMovieFiles != null) filenames.AddRange(uvMovieFiles);
        if (mirrorScanFiles != null) filenames.AddRange(mirrorScanFiles);

        return filenames.ToArray();
    }

    public string musicScores
    {
        get
        {
            string filename = "";
            if (live3DData != null)
            {
                filename = string.Format("musicscores_m{0:d3}.bdb", live3DData.id);
                filename = filename.ToLower();
            }
            return filename;
        }
    }

    public string cyalumeFile
    {
        get
        {
            string filename = "";
            if (live3DData != null)
            {
                filename = string.Format("3d_cyalume_m{0:D03}_hq.unity3d", live3DData.id);
                filename = filename.ToLower();
            }
            return filename;
        }
    }

    public string cuttFile
    {
        get
        {
            string filename = "";
            if (live3DData != null)
            {
                if (isSmartMode)
                {
                    filename = string.Format("3d_{0}_vertical.unity3d", live3DData.cutt);
                    filename = filename.ToLower();
                }
                else
                {
                    filename = string.Format("3d_{0}.unity3d", live3DData.cutt);
                    filename = filename.ToLower();
                }
            }
            return filename;
        }
    }
    public string cuttName
    {
        get
        {
            string filename = "";
            if (live3DData != null)
            {
                if (isSmartMode)
                {
                    string cuttName = live3DData.cutt;
                    filename = string.Format("Cutt/{0}_vertical/{1}_vertical", cuttName, cuttName);
                }
                else
                {
                    string cuttName = live3DData.cutt;
                    filename = string.Format("Cutt/{0}/{1}", cuttName, cuttName);
                }
            }
            return filename;
        }
    }

    public string stageFile
    {
        get
        {
            string filename = "";
            if (live3DData != null)
            {
                filename = string.Format("3d_stage_{0:0000}_hq.unity3d", live3DData.bg);
            }
            filename = filename.ToLower();
            return filename;
        }
    }
    public string stageName
    {
        get
        {
            string name = "";
            if (live3DData != null)
            {
                name = string.Format("3D/Stage/stg_{0:D3}/Prefab/Stage{0:D3}", live3DData.bg);
            }
            return name;
        }
    }

    public string cameraFile
    {
        get
        {
            string filename = "";
            if (live3DData != null)
            {
                if (live3DData.camera.Length > 0)
                {
                    filename = string.Format("3d_cutt_ac_{0}.unity3d", live3DData.camera);
                    filename = filename.ToLower();
                }
            }
            return filename;
        }
    }
    public string cameraName
    {
        get
        {
            string filename = "";
            if (live3DData != null)
            {
                if (live3DData.camera.Length > 0)
                {
                    filename = string.Format("3D/Camera/{0}/ac_{0}", live3DData.camera);
                }
            }
            return filename;
        }
    }

    public string musicFile
    {
        get
        {
            string filename = null;
            if (live3DData != null)
            {
                //int musicid = live3DData.debug_music_data_id;

                int musicid = MusicID;
                if (anotherMode)
                {
                    filename = string.Format("song_{0}_another.acb", musicid);
                }
                else if (soloCharaID > 0)
                {
                    filename = string.Format("song_{0}_{1:D3}.acb", musicid, soloCharaID);
                }
                else
                {
                    filename = string.Format("song_{0}.acb", musicid);
                }
            }
            return filename;
        }
    }

    public string autoLipFile
    {
        get
        {
            string filename = "";
            if (live3DData != null)
            {
                if (live3DData.autolip.Length > 0)
                {
                    filename = "3d_cutt_" + live3DData.autolip + ".unity3d";
                }
            }
            return filename;
        }
    }
    public string autoLipName
    {
        get
        {
            string filename = "";
            if (live3DData != null)
            {
                if (live3DData.autolip.Length > 0)
                {
                    filename = "3D/Lip/" + live3DData.autolip;
                }
            }
            return filename;
        }
    }

    public int stageMemberNumber
    {
        get
        {
            return liveData.member_number;
        } 
    }

    public string[] charaMotionFiles
    {
        get
        {
            List<string> filenames = new List<string>();
            if (live3DData != null)
            {
                string filename = string.Format("3d_cutt_{0}.unity3d", live3DData.charaMotion);
                filename = filename.Replace(" ", "_");
                filename = filename.ToLower();
                if (filename.Contains("{"))
                {
                    for (int i = 0; i < liveData.member_number; i++)
                    {
                        filenames.Add(string.Format(filename, i + 1));
                    }
                }
                else
                {
                    filenames.Add(filename);
                }

                if (isSmartMode)
                {

                    filename = string.Format("3d_cutt_{0}_vertical.unity3d", live3DData.charaMotion);
                    filename = filename.Replace(" ", "_");
                    filename = filename.ToLower();
                    if (filename.Contains("{"))
                    {
                        for (int i = 0; i < liveData.member_number; i++)
                        {
                            filenames.Add(string.Format(filename, i + 1));
                        }
                    }
                    else
                    {
                        filenames.Add(filename);
                    }
                }

            }

            List<string> filenames2 = new List<string>();

            foreach (var tmp in filenames)
            {
                if (AssetManager.instance.CheckExistFileInManifest(tmp))
                {
                    filenames2.Add(tmp);
                }
            }

            return filenames2.ToArray();
        }
    }

    /*
    public string[] charaMotionNames
    {
        get
        {
            List<string> filenames = new List<string>();
            if (live3DData != null)
            {
                string filename = string.Format("3D/Chara/Legacy/{0}", live3DData.charaMotion);
                filename = filename.Replace(" ", "_");
                filename = filename.ToLower();
                if (filename.Contains("{"))
                {
                    for (int i = 0; i < musicData.member_number; i++)
                    {
                        filenames.Add(string.Format(filename, i + 1));
                    }
                }
                else
                {
                    filenames.Add(filename);
                }
            }
            return filenames.ToArray();
        }
    }
    */

    public string[] uvMovieFiles
    {
        get
        {
            List<string> filenames = new List<string>();

            if (live3DData != null)
            {
                int count = live3DData.uvMovies.Length;
                for (int i = 0; i < count; i++)
                {
                    if (live3DData.uvMovies[i] != null)
                    {
                        if (live3DData.uvMovies[i].Length != 0)
                        {
                            string filename = string.Format("3d_uvm_{0}.unity3d", live3DData.uvMovies[i]);
                            filenames.Add(filename);
                        }
                    }
                }
            }
            return filenames.ToArray();
        }
    }
    public string[] uvMovieNames
    {
        get
        {
            List<string> filenames = new List<string>();

            if (live3DData != null)
            {
                int count = live3DData.uvMovies.Length;
                for (int i = 0; i < count; i++)
                {
                    if (live3DData.uvMovies[i] != null)
                    {
                        if (live3DData.uvMovies[i].Length != 0)
                        {
                            string filename = live3DData.uvMovies[i];
                            filenames.Add(filename);
                        }
                    }
                }
            }
            return filenames.ToArray();
        }
    }

    public string[] imgResourcesNames
    {
        get
        {
            List<string> filenames = new List<string>();

            if (live3DData != null)
            {
                int count = live3DData.imgResources.Length;
                for (int i = 0; i < count; i++)
                {
                    if (live3DData.imgResources[i] != null)
                    {
                        if (live3DData.imgResources[i].Length != 0)
                        {
                            string filename = "3D/UVMovie/Texture/" + live3DData.imgResources[i];
                            filenames.Add(filename);
                        }
                    }
                }
            }
            return filenames.ToArray();
        }
    }

    public string[] mirrorScanFiles
    {
        get
        {
            List<string> filenames = new List<string>();

            if (live3DData != null)
            {

                int count = live3DData.mirrorScanMatNames.Length;
                for (int i = 0; i < count; i++)
                {
                    if (live3DData.mirrorScanMatNames[i] != null)
                    {
                        if (live3DData.mirrorScanMatNames[i].Length != 0)
                        {
                            string filename = string.Format("3d_{0}.unity3d", live3DData.mirrorScanMatNames[i]);
                            filenames.Add(filename);
                        }
                    }
                }
            }
            return filenames.ToArray();
        }
    }
    public string[] mirrorScanMaterialNames
    {
        get
        {
            List<string> filenames = new List<string>();

            if (live3DData != null)
            {
                if (live3DData.mirrorScanMatIDs != null && live3DData.mirrorScanMatIDs.Length > 0)
                {
                    int count = live3DData.mirrorScanMatIDs.Length;
                    for (int i = 0; i < count; i++)
                    {
                        string filename = string.Format("3D/Stage/stg_{0:0000}/Materials/mt_stg_{0:0000}_MirrorScanLight_{1:000}", live3DData.bg, live3DData.mirrorScanMatIDs[i]);
                        filenames.Add(filename);
                    }
                }
                else if (live3DData.mirrorScanMatNames != null && live3DData.mirrorScanMatNames.Length > 0)
                {
                    int count = live3DData.mirrorScanMatNames.Length;
                    for (int i = 0; i < count; i++)
                    {
                        string filename = "3D/MirrorScanLight/Pattern/" + live3DData.mirrorScanMatNames[i] + "/mt_" + live3DData.mirrorScanMatNames[i];
                        filenames.Add(filename);
                    }
                }
            }
            return filenames.ToArray();
        }
    }

    public SQMusicData sqMusicData
    {
        get
        {
            if (liveData != null)
            {
                return liveData;
            }
            return null;
        }
    }

    public void LoadMusicScoreCyalume()
    {
        if (!AssetManager.instance.isManifestLoad) return;
        if (live3DData == null) return;

        int id = liveData.id;
        string likeName = string.Format("MusicScores/m{0:d3}/m{0:d3}_cyalume%", id, id);
        string filepath = AssetManager.instance.GetCachefromName(musicScores);

        if (filepath == "") return;

        string csvtext = "";
        DBProxy musicDB = new DBProxy();
        try
        {
            if (!musicDB.Open(filepath)) { return; }
            string sql = string.Format("SELECT * FROM {0} where name like '{1}'", "blobs", likeName);
            using (Query query = musicDB.Query(sql))
            {
                query.Step(); //最初の１つだけでおｋ
                csvtext = query.GetText(1);
            }
            musicDB.CloseDB();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            musicDB.CloseDB();
        }
        if (csvtext == "" || csvtext == null) return;
        _musicScoreCyalumeArray = Utility.ConvertCSV(csvtext);

    }

    public void LoadMusicScoreLyrics()
    {
        if (!AssetManager.instance.isManifestLoad) return;
        if (live3DData == null) return;

        int id = liveData.id;
        string likeName = string.Format("MusicScores/m{0:d3}/m{0:d3}_lyrics%", id, id);
        string filepath = AssetManager.instance.GetCachefromName(musicScores);

        if (filepath == "") return;

        string csvtext = "";
        DBProxy musicDB = new DBProxy();
        try
        {
            if (!musicDB.Open(filepath)) { return; }
            string sql = string.Format("SELECT * FROM {0} where name like '{1}'", "blobs", likeName);
            using (Query query = musicDB.Query(sql))
            {
                query.Step(); //最初の１つだけでおｋ
                csvtext = query.GetText(1);
            }
            musicDB.CloseDB();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            musicDB.CloseDB();
        }
        if (csvtext == "" || csvtext == null) return;
        _musicScoreLyricsArray = Utility.ConvertCSV(csvtext);
    }

    public void LoadMusicScore(int level = 1)//1=Easy
    {
        if (!AssetManager.instance.isManifestLoad) return;
        if (live3DData == null) return;

        int id = liveData.id;
        string likeName = string.Format("MusicScores/m{0:d3}/{0}_{1}%", id, level);
        string filepath = AssetManager.instance.GetCachefromName(musicScores);

        if (filepath == "") return;

        string csvtext = "";
        DBProxy musicDB = new DBProxy();
        try
        {
            if (!musicDB.Open(filepath)) { return; }
            string sql = string.Format("SELECT * FROM {0} where name like '{1}'", "blobs", likeName);
            using (Query query = musicDB.Query(sql))
            {
                query.Step(); //最初の１つだけでおｋ
                csvtext = query.GetText(1);
            }
            musicDB.CloseDB();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            musicDB.CloseDB();
        }
        if (csvtext != null && csvtext != "")
        {

            MusicScore score = new MusicScore();
            score.LoadScore(id, level, csvtext);

            _musicLength = score.endTime;
        }
        else
        {
            _musicLength = -1f;
        }
    }

    public void SetIsSmartMode(bool isSmart)
    {
        isSmartMode = isSmart;
    }

    public void SetAnotherMode(bool isAnother)
    {
        anotherMode = isAnother;
    }

    public void SetSoloCharaID(int charaID)
    {
        soloCharaID = charaID;
    }
}

