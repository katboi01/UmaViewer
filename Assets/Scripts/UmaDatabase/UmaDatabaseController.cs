using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using System.IO;

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
    /// <summary>
    /// Android/assetbundles/
    /// </summary>
    public static string AssetUrl = "Windows/assetbundles/";
    /// <summary>
    /// 3d/chara/body/
    /// </summary>
    public static string BodyPath = "3d/chara/body/";
    /// <summary>
    /// 3d/chara/head/
    /// </summary>
    public static string HeadPath = "3d/chara/head/";
    /// <summary>
    /// 3d/motion/
    /// </summary>
    public static string MotionPath = "3d/motion/";
    public IEnumerable<UmaDatabaseEntry> MetaEntries;
    public IEnumerable<UmaCharaData> CharaData;

    /// <summary>
    /// Meta Database Connection
    /// </summary>
    private SqliteConnection metaDb;
    /// <summary>
    /// Master Database Connection
    /// </summary>
    private SqliteConnection masterDb;
    /// <summary>
    /// Loads the file database
    /// </summary>
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
        //conn.Close();
    }

    static IEnumerable<UmaCharaData> ReadMaster(SqliteConnection conn)
    {
        SqliteCommand sqlite_cmd = conn.CreateCommand();
        sqlite_cmd.CommandText = "SELECT id,tail_model_id FROM chara_data";
        SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
        while (sqlite_datareader.Read())
        {
            UmaCharaData entry = new UmaCharaData()
            {
                id = sqlite_datareader.GetInt32(0),
                tail_model_id = sqlite_datareader.GetInt32(1).ToString(),
            };
            yield return entry;
        }
        //conn.Close();
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
