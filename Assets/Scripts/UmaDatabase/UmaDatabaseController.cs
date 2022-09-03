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
        metaDb = new SqliteConnection($@"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low"}\Cygames\umamusume\meta;");
        metaDb.Open();
        MetaEntries = ReadMeta(metaDb);
        masterDb = new SqliteConnection($@"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low"}\Cygames\umamusume\master\master.mdb;");
        masterDb.Open();
        CharaData = ReadMaster(masterDb);
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
}
