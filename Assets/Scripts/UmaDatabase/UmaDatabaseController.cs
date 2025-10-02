using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using Debug = UnityEngine.Debug;

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
    /// <summary> 3d/chara/body/ </summary>
    public static string BodyPath = "3d/chara/body/";
    /// <summary> 3d/chara/mini/body/ </summary>
    public static string MiniBodyPath = "3d/chara/mini/body/";
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

    public Dictionary<string, UmaDatabaseEntry> MetaEntries;
    public List<DataRow> CharaData;
    public List<DataRow> MobCharaData;
    public List<FaceTypeData> FaceTypeData;
    public List<DataRow> LiveData;
    public List<DataRow> DressData;


    //修改(载入通用服装ColorSet相关)
    public List<DataRow> CharaDressColor;


    /// <summary> Meta Database Connection </summary>
    public SqliteConnection metaDb;
    /// <summary> Master Database Connection </summary>
    private SqliteConnection masterDb;
    /// <summary> Loads the file database </summary>
    public UmaDatabaseController()
    {
        try
        {
            if (Config.Instance.WorkMode == WorkMode.Standalone)
            {
                if (!File.Exists($@"{Config.Instance.MainPath}/meta_umaviewer")) throw new Exception();
                metaDb = new SqliteConnection($@"Data Source={Config.Instance.MainPath}/meta_umaviewer;");
                masterDb = new SqliteConnection($@"Data Source={Config.Instance.MainPath}/master/master_umaviewer.mdb;");
            }
            else
            {
                if (!File.Exists($@"{Config.Instance.MainPath}/meta")) throw new Exception();
                metaDb = new SqliteConnection($@"Data Source={Config.Instance.MainPath}/meta;");
                masterDb = new SqliteConnection($@"Data Source={Config.Instance.MainPath}/master/master.mdb;");
            }

            try
            {
                metaDb.Open();
                MetaEntries = ReadMeta(metaDb);
            }
            catch (Exception e)
            {
                try
                {
                    var dbPath = $@"{Config.Instance.MainPath}/meta";
                    var key = GenFinalKey(Config.Instance.DBKey);
                    MetaEntries = ReadMetaFromEncryptedDb(dbPath, key, 3);
                }
                catch (Exception)
                {
                    throw;
                }
            }


            masterDb.Open();
            CharaData = ReadCharaMaster(masterDb);
            MobCharaData = ReadMobCharaMaster(masterDb);
            FaceTypeData = ReadFaceTypeData(masterDb);
            LiveData = ReadAllLiveData(masterDb);
            DressData = ReadAllDressData(masterDb);


            //修改(载入通用服装ColorSet相关)
            CharaDressColor = ReadAllCharaDressColor(masterDb);


#if UNITY_STANDALONE_WIN
            if (!Config.Instance.RegionDetectionPassed)
            {
                SetDefaultGameRegion(masterDb);
            }
#endif
        }
        catch
        {
            CloseAllConnection();
            var msg = $"Database not found at: {Config.Instance.MainPath}";
            if (Config.Instance.WorkMode == WorkMode.Standalone)
            {
                if (Config.Instance.Region == Region.Global)
                {
                    msg = "Only `Default` mode is supported for Global version. Please change Work Mode to Default in `Other` settings.";
                }
                else
                {
                    msg += "\nPlease update the database in the settings panel";
                }
            }
            else
            {
                msg += "\nPlease install the dmm game client or check the key settings.";
            }
            UmaViewerUI.Instance.ShowMessage(msg, UIMessageType.Error);
        }
    }



    //修改(载入通用服装ColorSet相关)
    static List<DataRow> ReadAllCharaDressColor(SqliteConnection conn)
    {
        SqliteCommand sqlite_cmd = conn.CreateCommand();
        sqlite_cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='chara_dress_color_set'";
        SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
        bool HasTable = sqlite_datareader.HasRows;

        if (!HasTable)
        {
            return new List<DataRow>();
        }

        return ReadMaster(conn, "SELECT * FROM chara_dress_color_set");
    }



    static Dictionary<string, UmaDatabaseEntry> ReadMeta(SqliteConnection conn)
    {
        bool HasEncryption = false;
        SqliteCommand sqlite_cmd = conn.CreateCommand();
        sqlite_cmd.CommandText = "SELECT name FROM pragma_table_info('a') WHERE name = 'e'";
        SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
        HasEncryption = sqlite_datareader.HasRows;

        sqlite_cmd = conn.CreateCommand();
        if (HasEncryption)
        {
            sqlite_cmd.CommandText = "SELECT m,n,h,d,e FROM a WHERE d IS NOT NULL";
        }
        else
        {
            sqlite_cmd.CommandText = "SELECT m,n,h,d FROM a WHERE d IS NOT NULL"; //filter out manifest entries, whitch have null prerequisites
        }
        sqlite_datareader = sqlite_cmd.ExecuteReader();
        Dictionary<string, UmaDatabaseEntry> meta = new Dictionary<string, UmaDatabaseEntry>();
        while (sqlite_datareader.Read())
        {
            UmaDatabaseEntry entry = null;
            try
            {
                var typestr = sqlite_datareader.GetString(0);
                if (Enum.TryParse<UmaFileType>(typestr, false, out var type))
                {
                    var name = sqlite_datareader.GetString(1);
                    var url = sqlite_datareader.GetString(2);
                    var prerequisites = sqlite_datareader.GetString(3);
                    var key = 0L;

                    if (HasEncryption)
                    {
                        key = sqlite_datareader.GetInt64(4);
                    }

                    if (string.IsNullOrEmpty(name))
                    {
                        throw new Exception($"Invalid entry name {name} or URL {url}");
                    }

                    entry = new UmaDatabaseEntry
                    {
                        Type = type,
                        Name = name,
                        Url = url,
                        Prerequisites = prerequisites,
                        Key = key
                    };
                }
                else
                {
                    Debug.LogWarning($"Unrecognized EntryType Enum Value :{typestr}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error caught while reading Entry : \n {e}");
            }

            if (entry != null && !meta.ContainsKey(entry.Name))
            {

                meta.Add(entry.Name, entry);
            }
        }
        return meta;
    }

    public static Dictionary<string, UmaDatabaseEntry> ReadMetaFromEncryptedDb(string dbPath, byte[] keyBytes, int cipherIndex = -1)
    {
        var meta = new Dictionary<string, UmaDatabaseEntry>(StringComparer.Ordinal);
        IntPtr db = IntPtr.Zero;

        try
        {
            db = Sqlite3MC.Open(dbPath);

            if (cipherIndex >= 0)
            {
                try
                {
                    int cfgRc = Sqlite3MC.MC_Config(db, "cipher", cipherIndex);
                }
                catch (Exception e)
                {
                    Debug.LogError($"MC_Config thrown: {e}");
                }
            }

            int rcKey = Sqlite3MC.Key_SetBytes(db, keyBytes);
            if (rcKey != Sqlite3MC.SQLITE_OK)
            {
                string em = Sqlite3MC.GetErrMsg(db);
                throw new InvalidOperationException($"sqlite3_key returned rc={rcKey}, errmsg={em}");
            }

            if (!Sqlite3MC.ValidateReadable(db, out string validateErr))
            {
                Debug.LogError($"DB validation after key failed: {validateErr}");
                throw new InvalidOperationException("DB validation after key failed: " + validateErr);
            }

            string sql = "SELECT m,n,h,c,d,e FROM a";
            Sqlite3MC.ForEachRow(sql, db, (stmt) =>
            {
                try
                {
                    string m = Sqlite3MC.ColumnText(stmt, 0);
                    string n = Sqlite3MC.ColumnText(stmt, 1);
                    string h = Sqlite3MC.ColumnText(stmt, 2);
                    string c = Sqlite3MC.ColumnText(stmt, 3);
                    string d = Sqlite3MC.ColumnText(stmt, 4);
                    long e = Sqlite3MC.ColumnInt64(stmt, 5);

                    if (string.IsNullOrEmpty(m))
                    {
                        Debug.LogWarning("Skipping row: empty type string (m).");
                        return;
                    }

                    if (!Enum.TryParse<UmaFileType>(m, /*ignoreCase*/ false, out UmaFileType type))
                    {
                        Debug.LogWarning($"Unrecognized EntryType Enum Value :{m}");
                        return;
                    }

                    if (string.IsNullOrEmpty(n))
                    {
                        Debug.LogError($"Invalid entry name '{n}' or URL '{h}'. Skipping row.");
                        return;
                    }

                    var entry = new UmaDatabaseEntry
                    {
                        Type = type,
                        Name = n,
                        Url = h,
                        Checksum = c,
                        Prerequisites = d,
                        Key = e
                    };

                    if (!meta.ContainsKey(entry.Name))
                    {
                        meta.Add(entry.Name, entry);
                    }
                }
                catch (Exception exRow)
                {
                    Debug.LogError("Error caught while reading row: " + exRow);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogError("ReadMetaFromEncryptedDb failed: " + ex);
            throw;
        }
        finally
        {
            if (db != IntPtr.Zero)
            {
                try { Sqlite3MC.Close(db); }
                catch (Exception e) { Debug.LogError("Closing DB failed: " + e); }
            }
        }

        return meta;
    }

    static void SetDefaultGameRegion(SqliteConnection conn)
    {
        var str = ReadMaster(conn, "SELECT text FROM text_data LIMIT 1");
        //check if first character of the first text_data entry is in English
        bool isAscii = str[0][0].ToString()[0] < 128;
        Region detectedRegion = isAscii ? Region.Global : Region.Jp;

        if (Config.Instance.Region != detectedRegion)
        {
            if (detectedRegion == Region.Global)
                Popup.Create($"Different game region was detected. Current region is {Config.Instance.Region}.\nDo you want to change it? (It can be changed again in `Other` settings)", -1, 200,
                    "Set to Global (Steam)", () =>
                    {
                        Config.Instance.RegionDetectionPassed = true;
                        Config.Instance.Region = detectedRegion;
                        Config.Instance.UpdateConfig(false);
                        UmaViewerUI.Instance.OtherSettings.ApplySettings();
                        UmaViewerUI.Instance.StartCoroutine(UmaViewerUI.Instance.ApplyGraphicsSettings());
                    },
                    "Keep Japan (DMM)", () =>
                    {
                        Config.Instance.RegionDetectionPassed = true;
                        Config.Instance.UpdateConfig(false);
                    });
            else
            {
                Popup.Create($"Different game region was detected. Current region is {Config.Instance.Region}.\nDo you want to change it? (It can be changed again in `Other` settings)", -1, 200,
                    "Set to Japan (DMM)", () =>
                    {
                        Config.Instance.RegionDetectionPassed = true;
                        Config.Instance.Region = detectedRegion;
                        Config.Instance.UpdateConfig(false);
                        UmaViewerUI.Instance.OtherSettings.ApplySettings();
                        UmaViewerUI.Instance.StartCoroutine(UmaViewerUI.Instance.ApplyGraphicsSettings());
                    },
                    "Keep Global (Steam)", () =>
                    {
                        Config.Instance.RegionDetectionPassed = true;
                        Config.Instance.UpdateConfig(false);
                    });
            }
        }
    }

    static List<DataRow> ReadCharaMaster(SqliteConnection conn)
    {
        return ReadMaster(conn, "SELECT * FROM chara_data C,(SELECT D.'index' charaid,D.'text' charaname FROM text_data D WHERE id like 6) T WHERE C.id like T.charaid");
    }

    static List<DataRow> ReadMobCharaMaster(SqliteConnection conn)
    {
        return ReadMaster(conn, "SELECT * FROM mob_data M,(SELECT D.'index' charaid,D.'text' charaname FROM text_data D WHERE id like 59) T WHERE M.mob_id like T.charaid");
    }

    static List<DataRow> ReadMaster(SqliteConnection conn, string sql)
    {
        List<DataRow> dr = new List<DataRow>();

        using (var sqlite_cmd = conn.CreateCommand())
        {
            sqlite_cmd.CommandText = sql;
            using var sqlite_datareader = sqlite_cmd.ExecuteReader();
            var result = new DataTable();
            for (int i = 0; i < sqlite_datareader.FieldCount; i++)
            {
                result.Columns.Add(sqlite_datareader.GetName(i), sqlite_datareader.GetFieldType(i));
            }
            while (sqlite_datareader.Read())
            {
                DataRow row = result.NewRow();
                for (int i = 0; i < sqlite_datareader.FieldCount; i++)
                {
                    row[i] = sqlite_datareader[i];
                }
                dr.Add(row);
            }
        }
        return dr;
    }

    static List<FaceTypeData> ReadFaceTypeData(SqliteConnection conn)
    {
        List<FaceTypeData> data = new List<FaceTypeData>();
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
            data.Add(entry);
        }
        return data;
    }

    static List<DataRow> ReadAllLiveData(SqliteConnection conn)
    {
        return ReadMaster(conn, $"SELECT * FROM live_data L,(SELECT D.'index' songid,D.'text' songname FROM text_data D WHERE id like 16) T WHERE L.music_id like T.songid");
    }

    static List<DataRow> ReadAllDressData(SqliteConnection conn)
    {
        return ReadMaster(conn, $"SELECT * FROM dress_data C,(SELECT D.'index' dressid,D.'text' dressname FROM text_data D WHERE id like 14) T WHERE C.id like T.dressid");
    }

    public static DataRow ReadCharaData(CharaEntry chara)
    {
        if (chara.IsMob)
        {
            return instance.MobCharaData.Find(e => { return Convert.ToInt32(e["mob_id"]) == chara.Id; });
        }
        return instance.CharaData.Find(e => { return Convert.ToInt32(e["id"]) == chara.Id; });
    }

    public static DataRow ReadMobDressColor(string mobid)
    {
        var results = ReadMaster(instance.masterDb, $"SELECT * FROM mob_dress_color_set WHERE id LIKE {mobid}");

        return results.Count > 0 ? results[0] : null;
    }

    public static DataRow ReadMobHairColor(string colorid)
    {
        var results = ReadMaster(instance.masterDb, $"SELECT * FROM mob_hair_color_set WHERE id LIKE {colorid}");
        foreach (var data in results)
        {
            return data;
        }
        return null;
    }

    public void CloseAllConnection()
    {
        masterDb?.Close();
        metaDb?.Close();
        masterDb?.Dispose();
        metaDb?.Dispose();
        SqliteConnection.ClearAllPools();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    public byte[] GenFinalKey(byte[] key)
    {
        var baseKey = Config.Instance.DBBaseKey;
        if (baseKey.Length < 13)
            throw new Exception("Invalid Base Key length");
        for (int i = 0; i < key.Length; i++)
        {
            key[i] = (byte)(key[i] ^ baseKey[i % 13]);
        }
        return key;
    }
}
