using UnityEngine;
using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;

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

    public IEnumerable<UmaDatabaseEntry> MetaEntries;
    public IEnumerable<DataRow> CharaData;
    public IEnumerable<FaceTypeData> FaceTypeData;
    public IEnumerable<DataRow> LiveData;

    /// <summary> Meta Database Connection </summary>
    private SqliteConnection metaDb;
    /// <summary> Master Database Connection </summary>
    private SqliteConnection masterDb;
    /// <summary> Loads the file database </summary>
    public UmaDatabaseController()
    {
        try
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                metaDb = new SqliteConnection($@"Data Source={GetGameRootPath()}/meta;");
                masterDb = new SqliteConnection($@"Data Source={GetGameRootPath()}/master/master.mdb;");
            }
            else
            {
                metaDb = new SqliteConnection($@"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low"}\Cygames\umamusume\meta;");
                masterDb = new SqliteConnection($@"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low"}\Cygames\umamusume\master\master.mdb;");
            }
            metaDb.Open();
            MetaEntries = ReadMeta(metaDb);

            masterDb.Open();
            CharaData = ReadMaster(masterDb);
            FaceTypeData = ReadFaceTypeData(masterDb);
            LiveData = ReadAllLiveData(masterDb);
        }
        catch
        {
            UmaViewerUI.Instance.LyricsText.text = $"Database not found: \n{GetGameRootPath()}/meta\n{GetGameRootPath()}/master/master.mdb";
            UmaViewerUI.Instance.LyricsText.color = Color.red;
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

    static IEnumerable<DataRow> ReadMaster(SqliteConnection conn)
    {
        SqliteCommand sqlite_cmd = conn.CreateCommand();
        sqlite_cmd.CommandText = "SELECT * FROM chara_data C,(SELECT D.'index' charaid,D.'text' charaname FROM text_data D WHERE id like 6) T WHERE C.id like T.charaid";
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
        SqliteCommand sqlite_cmd = conn.CreateCommand();
        sqlite_cmd.CommandText = 
        $"SELECT * FROM live_data L,(SELECT D.'index' songid,D.'text' songname FROM text_data D WHERE id like 16) T WHERE L.music_id like T.songid";
        SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
        var result = new DataTable();
        result.Load(sqlite_datareader);
        var temp = result.Rows.GetEnumerator();
        while (temp.MoveNext()) 
        {
            yield return (DataRow)temp.Current;
        }
    }

    public static DataRow ReadCharaData(int id)
    {
        SqliteCommand sqlite_cmd = instance.masterDb.CreateCommand();
        sqlite_cmd.CommandText = $"SELECT * FROM chara_data WHERE id LIKE {id}";
        SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
        var result = new DataTable();
        result.Load(sqlite_datareader);
        DataRow row = result.Rows[0];
        return row;
    }

    public static string GetABPath(UmaDatabaseEntry entry)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            return $"{GetGameRootPath()}/dat/{entry.Url.Substring(0, 2)}/{entry.Url}";
        }

        return $"{GetGameRootPath()}\\dat\\{entry.Url.Substring(0, 2)}\\{entry.Url}";
    }

    public static string GetGameRootPath()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            return Application.persistentDataPath;
        }
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low"}\\Cygames\\umamusume";
    }
}
