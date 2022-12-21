using Mono.Data.Sqlite;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static BSVReader;
using static ManifestCategory;

public class ManifestDB
{
    public SqliteConnection MetaDB;
    public static string DBPath;
    public const string DefaultDBPath = "umamusume/meta";
    public Action<string> callback;
    public ManifestDB(string DBPath = DefaultDBPath)
    {
        ManifestDB.DBPath = DBPath;
        if (File.Exists(DBPath))
        {
            MetaDB = new SqliteConnection($@"Data Source={DBPath}");
            MetaDB.Open();
        }
        else
        {
            SqliteConnection.CreateFile(DBPath);
            MetaDB = new SqliteConnection($@"Data Source={DBPath}");
            MetaDB.Open();
            SqliteCommand cmd = new SqliteCommand(
                "CREATE TABLE `a` ( `i` INTEGER PRIMARY KEY,`n` TEXT NOT NULL,`d` TEXT,`g` INTEGER(4) NOT NULL,`l` INTEGER(8) NOT NULL,`c` INTEGER(8) NOT NULL,`h` TEXT NOT NULL,`m` TEXT NOT NULL,`k` INTEGER(1) NOT NULL,`s` INTEGER(1) NOT NULL,`p` INTEGER(4) NOT NULL DEFAULT 0)");
            cmd.Connection = MetaDB;
            cmd.ExecuteNonQuery();
            cmd.CommandText = "CREATE INDEX `a0` ON `a`(`n`, `s`)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "CREATE INDEX `a2` ON `a`(`m`, `s`)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "CREATE INDEX `a3` ON `a`(`g`, 's')";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "CREATE INDEX `a4` ON `a`(`p`)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "CREATE INDEX `a1` ON `a`(`s`)";
            cmd.ExecuteNonQuery();
        }
    }

    public IEnumerator UpdateResourceVersion(Action<string> callback)
    {
        this.callback = callback;
        callback?.Invoke("Checking Resource Version");
        yield return UpdateMetaDB();
        yield return UpdateMasterDB();
        callback?.Invoke($"Done");
    }

    //umamusume uses the manifest file to generate meta database.
    //it needs to get the root manifest file first
    //then read the root manifest to get other manifest file information
    //recursively read the manifest file to obtain all resource files information
    //finally, insert all data into the meta database
    public IEnumerator UpdateMetaDB()
    {
        string rootHash = null;
        callback?.Invoke("Getting resource version");
        yield return UmaViewerDownload.DownloadText("https://www.tracenacademy.com/api/MetaC/root", txt =>
        {
            if (string.IsNullOrEmpty(txt)) return;
            var item = JObject.Parse(txt);
            if ((string)item["n"] == "//root")
            {
                rootHash = (string)item["h"];
            }
        });

        if (rootHash == null)
        {
            Debug.LogError("Get RootHash Error");
            yield break;
        }

        ManifestEntry rootEntry = new ManifestEntry(Encoding.UTF8.GetBytes("root"), null, 0, 0, 0, 0, "", 3);
        rootEntry.hname = rootHash;
        yield return UpdateMetaDB(rootEntry);
    }

    public IEnumerator UpdateMetaDB(ulong size, ulong checksum)
    {
        ManifestEntry rootEntry = new ManifestEntry(Encoding.UTF8.GetBytes("root"), null, 0, 0, size, checksum, "", 3);
        rootEntry.hname = rootEntry.CalHameString();
        yield return UpdateMetaDB(rootEntry);
    }

    public IEnumerator UpdateMetaDB(ManifestEntry rootEntry)
    {
        if (MetaDB == null || MetaDB.State == System.Data.ConnectionState.Closed)
        {
            Debug.LogError("Open Meta Database Error");
            yield break;
        }

        SqliteCommand command = new SqliteCommand(MetaDB);

        //Insert root
        int index = 1;
        UpdateManifestEntry(index, "manifest3", rootEntry, ref command);

        //Insert platform
        callback?.Invoke("Reading Platform Manifest");
        ManifestEntry[] platformEntrys = null;
        yield return GetManifest(rootEntry.hname, Kind.PlatformManifest, entrys => { platformEntrys = entrys; });
        if (platformEntrys == null)
        {
            Debug.LogError("Read PlatformManifest Error");
            MetaDB.Close();
            yield break;
        }
        ManifestEntry platformEntry = GetLocalPlatformEntry(platformEntrys);
        index++;
        UpdateManifestEntry(index, "manifest2", platformEntry, ref command);

        //Insert AssetManifest
        ManifestEntry[] assetEntrys = null;
        yield return GetManifest(platformEntry.hname, Kind.AssetManifest, entrys => { assetEntrys = entrys; });
        if (assetEntrys == null)
        {
            Debug.LogError("Read AssetManifest Error");
            MetaDB.Close();
            yield break;
        }
        using (SqliteTransaction tran = MetaDB.BeginTransaction())
        {
            for (int i = 0; i < assetEntrys.Length; i++)
            {
                index++;
                callback?.Invoke($"Reading Assets Manifest : {assetEntrys[i].tname} ({i + 1}/{assetEntrys.Length})");
                UpdateManifestEntry(index, "manifest", assetEntrys[i], ref command);
            }
            tran.Commit();
        }

        //Insert Asset
        foreach (var assetEntry in assetEntrys)
        {
            ManifestEntry[] assets = null;
            yield return GetManifest(assetEntry.hname, Kind.Default, entrys => { assets = entrys; });
            if (assets == null)
            {
                Debug.LogError("Read AssetManifest Error");
                continue;
            }
            using SqliteTransaction tran = MetaDB.BeginTransaction();
            for (int i = 0; i < assets.Length; i++)
            {
                index++;
                callback?.Invoke($"Reading AssetBundles Manifest : {assetEntry.tname} ({i + 1}/{assets.Length})");
                UpdateManifestEntry(index, assetEntry.tname, assets[i], ref command);
            }
            tran.Commit();
        }
    }

    public IEnumerator UpdateMasterDB()
    {
        if (MetaDB == null || MetaDB.State == System.Data.ConnectionState.Closed)
        {
            Debug.LogError("Open Meta Database Error");
            yield break;
        }

        callback?.Invoke($"Checking master.mdb");
        SqliteCommand command = new SqliteCommand("SELECT h FROM a WHERE n LIKE 'master.mdb.lz4'", MetaDB);
        var reader = command.ExecuteReader();
        
        if (reader.Read())
        {
            var hash = reader.GetString(0);
            var path = GetManifestPath(hash);
            if (!File.Exists(path))
            {
                callback?.Invoke($"Downloading master.mdb");
                UnityWebRequest www = UnityWebRequest.Get(UmaViewerDownload.GetGenericRequestUrl(hash));
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllBytes(path, www.downloadHandler.data);
                }
                else
                {
                    Debug.LogError("Download Master.mdb Failed :" + www.error);
                    yield break;
                }
            }

            callback?.Invoke($"Decompress master.mdb");
            var masterPath = $"{Path.GetDirectoryName(DBPath)}/master/master.mdb";
            Directory.CreateDirectory(Path.GetDirectoryName(masterPath));
            File.WriteAllBytes(masterPath, LZ4Util.DecompressFromFile(path));
        }
    }

    private void UpdateManifestEntry(int index, string type, ManifestEntry entry, ref SqliteCommand command)
    {
        command.CommandText = "INSERT OR REPLACE INTO a (i,n,d,g,l,c,h,m,k,s,p) VALUES (@id,@name,@deps,@group,@length,@check,@hash,@m,@k,@s,@p)";
        command.Parameters.Clear();
        command.Parameters.Add(new SqliteParameter("@id", index));
        command.Parameters.Add(new SqliteParameter("@name", (type.Contains("manifest") ? "//" : "") + entry.tname));
        command.Parameters.Add(new SqliteParameter("@deps", entry.tdeps));
        command.Parameters.Add(new SqliteParameter("@group", entry.group));
        command.Parameters.Add(new SqliteParameter("@length", (long)entry.size));
        command.Parameters.Add(new SqliteParameter("@check", (long)entry.checksum));
        command.Parameters.Add(new SqliteParameter("@hash", entry.hname));
        command.Parameters.Add(new SqliteParameter("@m", type));
        command.Parameters.Add(new SqliteParameter("@k", (byte)entry.kind));
        command.Parameters.Add(new SqliteParameter("@s", 1));
        command.Parameters.Add(new SqliteParameter("@p", entry.priority));
        command.ExecuteNonQuery();
    }

    public static ManifestEntry[] GetManifest(string path, bool isCompressed, Kind kind)
    {
        IBSVReader reader = Init(path, isCompressed);
        if (reader == null) return null;

        ulong count = reader.GetRowCount();
        ManifestEntry[] manifests = new ManifestEntry[count];
        if (kind == Kind.Default)
        {
            for (ulong i = 0; i < count; i++)
            {
                manifests[i].kind = (int)kind;
                if (!reader.ReadLine(ManifestEntry.GetBsvParser(ManifestEntry.Format.Full, reader), ref manifests[i]))
                {
                    Debug.LogError("ReadLineError");
                    return null;
                }
            }
        }
        else
        {
            for (ulong i = 0; i < count; i++)
            {
                manifests[i].kind = (int)kind;
                if (!reader.ReadLine(ManifestEntry.GetBsvParser(ManifestEntry.Format.Simplified, reader), ref manifests[i]))
                {
                    Debug.LogError("ReadLineError");
                    return null;
                }
            }
        }
        return manifests;

    }

    public IEnumerator GetManifest(string hash, Kind kind, Action<ManifestEntry[]> callback)
    {
        var path = GetManifestPath(hash);
        if (!File.Exists(path))
        {
            this.callback?.Invoke($"Downloading Manifest :{hash}");
            yield return DownloadManifest(hash);
        }
        if (File.Exists(path))
        {
            callback.Invoke(GetManifest(path, true, kind));
        }
    }

    public IEnumerator DownloadManifest(string hash)
    {
        UnityWebRequest www = UnityWebRequest.Get(UmaViewerDownload.GetManifestRequestUrl(hash));
        www.timeout = 15;
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(GetManifestPath(hash), www.downloadHandler.data);
        }
        else
        {
            Debug.LogError("Download Manifest Failed :" + www.error);
        }
    }

    public string GetManifestPath(string hash, bool createPath = true)
    {
        string path = $"{Path.GetDirectoryName(DBPath)}/dat/{hash.Substring(0, 2)}";
        if (createPath)
        {
            Directory.CreateDirectory(path);
        }
        return path + $"/{hash}";
    }

    public static ManifestEntry GetLocalPlatformEntry(ManifestEntry[] platformEntrys)
    {
        foreach (var entry in platformEntrys)
        {
            if (Encoding.UTF8.GetString(entry.name) == "Windows" && Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return entry;
            }
            else if (Encoding.UTF8.GetString(entry.name) == "IOS" && Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return entry;
            }
            else if (Encoding.UTF8.GetString(entry.name) == "Android" && Application.platform == RuntimePlatform.Android)
            {
                return entry;
            }
            else
            {
                return entry;
            }
        }
        return platformEntrys[0];
    }
}

