using UnityEngine;
using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;
using Assets.Scripts;

public class UmaDatabaseController
{
    private static UmaDatabaseController instance;

    /// <summary>
    /// Database Loader instance
    /// </summary>
    public static UmaDatabaseController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UmaDatabaseController();
            }
            return instance;
        }
    }
    /// <summary>
    /// https://prd-storage-umamusume.akamaized.net/dl/resources/
    /// </summary>
    public static string ServerUrl = "https://prd-storage-umamusume.akamaized.net/dl/resources/";
    /// <summary> Android/assetbundles/ </summary>
    public static string AssetUrl = "Windows/assetbundles/";
    /// <summary> 3d/chara/body/ </summary>
    public static string BodyPath = "3d/chara/body/";
    /// <summary> 3d/chara/head/ </summary>
    public static string HeadPath = "3d/chara/head/";
    /// <summary> 3d/motion/ </summary>
    public static string MotionPath = "3d/motion/";
    /// <summary> 3d/chara/ </summary>
    public static string CharaPath = "3d/chara/";
    /// <summary> 3d/effect/ </summary>
    public static string EffectPath = "3d/effect/";
    /// <summary> outgame/dress/ </summary>
    public static string CostumePath = "outgame/dress/";

    public IEnumerable<UmaDatabaseEntry> MetaEntries;
    public IEnumerable<DataRow> CharaData;
    public IEnumerable<DataRow> MobCharaData;
    public IEnumerable<FaceTypeData> FaceTypeData;
    public IEnumerable<DataRow> LiveData;
    public IEnumerable<DataRow> DressData;

    /// <summary> Meta Database Connection </summary>
    private SqliteConnection metaDb;
    /// <summary> Master Database Connection </summary>
    private SqliteConnection masterDb;
    /// <summary> Loads the file database </summary>
    public UmaDatabaseController()
    {
        try
        {
            if(Config.Instance.WorkMode == WorkMode.Standalone)
            {
                metaDb = new SqliteConnection($@"Data Source={Config.Instance.MainPath}\meta_umaviewer;");
                masterDb = new SqliteConnection($@"Data Source={Config.Instance.MainPath}\master\master_umaviewer.mdb;");
            }
            else
            {
                metaDb = new SqliteConnection($@"Data Source={Config.Instance.MainPath}\meta;");
                masterDb = new SqliteConnection($@"Data Source={Config.Instance.MainPath}\master\master.mdb;");
            }


            metaDb.Open();
            MetaEntries = ReadMeta(metaDb);

            masterDb.Open();
            CharaData = ReadCharaMaster(masterDb);
            MobCharaData = ReadMobCharaMaster(masterDb);
            FaceTypeData = ReadFaceTypeData(masterDb);
            LiveData = ReadAllLiveData(masterDb);
            DressData = ReadAllDressData(masterDb);
        }
        catch 
        {
            CloseAllConnection();
            var msg = $"Database not found at: {Config.Instance.MainPath}";
            if(Config.Instance.WorkMode == WorkMode.Standalone)
            {
                msg += "\nPlease update the database in the settings panel";
            }
            else
            {
                msg += "\nPlease install the dmm game client";
            }
            UmaViewerUI.Instance.ShowMessage(msg);
        }
    }

    static IEnumerable<UmaDatabaseEntry> ReadMeta(SqliteConnection conn)
    {
        List<UmaDatabaseEntry> entries = new List<UmaDatabaseEntry>();
        SqliteCommand sqlite_cmd = conn.CreateCommand();
        sqlite_cmd.CommandText = "SELECT m,n,h,c,d FROM a";
        SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
        while (sqlite_datareader.Read())
        {
            try
            {
                UmaDatabaseEntry entry = new UmaDatabaseEntry()
                {
                    Type = (UmaFileType)Enum.Parse(typeof(UmaFileType), sqlite_datareader["m"] as String),
                    Name = sqlite_datareader["n"] as String,
                    Url = sqlite_datareader["h"] as String,
                    Checksum = sqlite_datareader["c"].ToString(),
                    Prerequisites = sqlite_datareader["d"] as String
                };
                entries.Add(entry);
            }
            catch(Exception e) { Debug.LogError("Error caught: " + e); }
        }
        return entries;
    }

    static IEnumerable<DataRow> ReadCharaMaster(SqliteConnection conn)
    {
        return ReadMaster(conn, "SELECT * FROM chara_data C,(SELECT D.'index' charaid,D.'text' charaname FROM text_data D WHERE id like 6) T WHERE C.id like T.charaid");
    }

    static IEnumerable<DataRow> ReadMobCharaMaster(SqliteConnection conn)
    {
        return ReadMaster(conn, "SELECT * FROM mob_data");
    }

    static IEnumerable<DataRow> ReadMaster(SqliteConnection conn,string sql)
    {
        SqliteCommand sqlite_cmd = conn.CreateCommand();
        sqlite_cmd.CommandText = sql;
        SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
        var result = new DataTable();
        result.Load(sqlite_datareader);
        var temp = result.Rows.GetEnumerator();
        while (temp.MoveNext())
        {
            yield return (DataRow)temp.Current;
        }
    }

    static IEnumerable<FaceTypeData> ReadFaceTypeData(SqliteConnection conn)
    {
        SqliteCommand sqlite_cmd = conn.CreateCommand();
        sqlite_cmd.CommandText = "SELECT * FROM face_type_data";
        SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
        while (sqlite_datareader.Read())
        {
            FaceTypeData entry = new FaceTypeData()
            {
                label = sqlite_datareader.GetString(0),
                eyebrow_l = sqlite_datareader.GetString(1),
                eyebrow_r = sqlite_datareader.GetString(2),
                eye_l = sqlite_datareader.GetString(3),
                eye_r = sqlite_datareader.GetString(4),
                mouth = sqlite_datareader.GetString(5),
                mouth_shape_type = sqlite_datareader.GetInt32(6),
                inverce_face_type = sqlite_datareader.GetString(7),
                set_face_group = sqlite_datareader.GetInt32(8),
            };
            yield return entry;
        }
    }

    static IEnumerable<DataRow> ReadAllLiveData(SqliteConnection conn)
    {
        return ReadMaster(conn, $"SELECT * FROM live_data L,(SELECT D.'index' songid,D.'text' songname FROM text_data D WHERE id like 16) T WHERE L.music_id like T.songid");
    }

    static IEnumerable<DataRow> ReadAllDressData(SqliteConnection conn)
    {
        return ReadMaster(conn, $"SELECT * FROM dress_data C,(SELECT D.'index' dressid,D.'text' dressname FROM text_data D WHERE id like 14) T WHERE C.id like T.dressid");
    }

    public static DataRow ReadCharaData(int id)
    {
        foreach (var chara in instance.CharaData)
        {
            if (Convert.ToInt32(chara["id"]) == id)
            {
                return chara;
            }
        }
        return null;
    }

    public void CloseAllConnection()
    {
        masterDb.Close();
        metaDb.Close();
    }

}
