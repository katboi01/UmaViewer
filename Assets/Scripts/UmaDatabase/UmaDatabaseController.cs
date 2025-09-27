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

    /// <summary> Meta Database Connection </summary>
    public SqliteConnection metaDb;
    /// <summary> Master Database Connection </summary>
    private SqliteConnection masterDb;
    /// <summary> Loads the file database </summary>
    public UmaDatabaseController()
    {
        try
        {
            if(Config.Instance.WorkMode == WorkMode.Standalone)
            {
                if(!File.Exists($@"{Config.Instance.MainPath}/meta_umaviewer")) throw new Exception();
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
                string dbPath = $@"{Config.Instance.MainPath}/meta";
                byte[] keyBytes = new byte[32] {
                    0x9C, 0x2B, 0xAB, 0x97, 0xBC, 0xF8, 0xC0, 0xC4,
                    0xF1, 0xA9, 0xEA, 0x78, 0x81, 0xA2, 0x13, 0xF6,
                    0xC9, 0xEB, 0xF9, 0xD8, 0xD4, 0xC6, 0xA8, 0xE4,
                    0x3C, 0xE5, 0xA2, 0x59, 0xBD, 0xE7, 0xE9, 0xFD
                };
                MetaEntries = ReadMetaFromEncryptedDb(dbPath, keyBytes, 3);
            }


            masterDb.Open();
            CharaData = ReadCharaMaster(masterDb);
            MobCharaData = ReadMobCharaMaster(masterDb);
            FaceTypeData = ReadFaceTypeData(masterDb);
            LiveData = ReadAllLiveData(masterDb);
            DressData = ReadAllDressData(masterDb);
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
            if(Config.Instance.WorkMode == WorkMode.Standalone)
            {
                if(Config.Instance.Region == Region.Global)
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
                msg += "\nPlease install the dmm game client";
            }
            UmaViewerUI.Instance.ShowMessage(msg, UIMessageType.Error);
        }
    }

    static Dictionary<string,UmaDatabaseEntry> ReadMeta(SqliteConnection conn)
    {
        SqliteCommand sqlite_cmd = conn.CreateCommand();
        //sqlite_cmd.CommandText = "SELECT m,n,h,d FROM a WHERE d IS NOT NULL"; Why?
        sqlite_cmd.CommandText = "SELECT m,n,h,d FROM a";
        SqliteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
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
                    
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new Exception($"Invalid entry name {name} or URL {url}");
                    }

                    entry = new UmaDatabaseEntry
                    {
                        Type = type,
                        Name = name,
                        Url = url,
                        Prerequisites = prerequisites
                    };
                }
                else
                {
                    Debug.LogWarning($"Unrecognized EntryType Enum Value :{typestr}");
                }
            }
            catch(Exception e) 
            { 
                Debug.LogError($"Error caught while reading Entry : \n {e}"); 
            }
            
            if (entry != null && !meta.ContainsKey(entry.Name)) {

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
            // 打开数据库
            db = Sqlite3MC.Open(dbPath);

            // 可选：设置 cipher index（如果你知道）
            if (cipherIndex >= 0)
            {
                try
                {
                    int cfgRc = Sqlite3MC.MC_Config(db, "cipher", cipherIndex);
                    // Debug.Log($"sqlite3mc_config(cipher,{cipherIndex}) returned {cfgRc}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"MC_Config thrown: {e}");
                }
            }

            // 设置 key（raw bytes）
            int rcKey = Sqlite3MC.Key_SetBytes(db, keyBytes);
            if (rcKey != Sqlite3MC.SQLITE_OK)
            {
                string em = Sqlite3MC.GetErrMsg(db);
                throw new InvalidOperationException($"sqlite3_key returned rc={rcKey}, errmsg={em}");
            }

            // 验证能否读取（必须做）
            if (!Sqlite3MC.ValidateReadable(db, out string validateErr))
            {
                Debug.LogError($"DB validation after key failed: {validateErr}");
                return meta; // 返回空字典（与你之前选择一致）
            }

            // 查询并逐行处理： m(0), n(1), h(2), c(3), d(4), e(5)
            string sql = "SELECT m,n,h,c,d,e FROM a";
            Sqlite3MC.ForEachRow(sql, db, (stmt) =>
            {
                try
                {
                    // 读取列（可能为 null）
                    string m = Sqlite3MC.ColumnText(stmt, 0);
                    string n = Sqlite3MC.ColumnText(stmt, 1);
                    string h = Sqlite3MC.ColumnText(stmt, 2);
                    string c = Sqlite3MC.ColumnText(stmt, 3);
                    string d = Sqlite3MC.ColumnText(stmt, 4);
                    string e = Sqlite3MC.ColumnText(stmt, 5);

                    // 解析 type 字符串（保持大小写敏感解析：TryParse(..., false, out type)）
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

                    // 校验 name（不能为空）
                    if (string.IsNullOrEmpty(n))
                    {
                        Debug.LogError($"Invalid entry name '{n}' or URL '{h}'. Skipping row.");
                        return;
                    }

                    // 构建 entry（对可能为 null 的列做容错：使用 empty string）
                    var entry = new UmaDatabaseEntry
                    {
                        Type = type,
                        Name = n,
                        Url = h,
                        Checksum = c,
                        Prerequisites = d,
                        Key = e
                    };

                    // 去重：只有在不存在相同 name 时添加
                    if (!meta.ContainsKey(entry.Name))
                    {
                        meta.Add(entry.Name, entry);
                    }
                    // 否则跳过（你原来的逻辑是这样）
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
        Region detectedRegion = isAscii? Region.Global : Region.Jp;

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
        var results= ReadMaster(instance.masterDb, $"SELECT * FROM mob_dress_color_set WHERE id LIKE {mobid}");

        return results.Count > 0 ? results[0] : null;
    }

    public static DataRow ReadMobHairColor(string colorid)
    {
        var results= ReadMaster(instance.masterDb, $"SELECT * FROM mob_hair_color_set WHERE id LIKE {colorid}");
        foreach(var data in results)
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

}
