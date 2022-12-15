using Mono.Data.Sqlite;
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
    public string DBPath;
    public static string MANIFEST_ROOT_URL = "https://prd-storage-app-umamusume.akamaized.net/dl/resources/Manifest";
    public ManifestDB(string DBPath)
    {
        this.DBPath = DBPath;
        if (File.Exists(DBPath))
        {
            MetaDB = new SqliteConnection($@"Data Source={DBPath}");
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


    //umamusume uses the manifest file to generate meta database.
    //it needs to get the root manifest file first
    //then read the root manifest to get other manifest file information
    //recursively read the manifest file to obtain all resource files information
    //finally, insert all data into the meta database
    public IEnumerator GenerateMetaDB(ulong rootSize, ulong rootCheckSum)
    {
        if (MetaDB == null || MetaDB.State == System.Data.ConnectionState.Closed) {
            Debug.LogError("Open Meta Database Error");
            yield break;
        }

        //Insert root
        ManifestEntry rootEntry = new ManifestEntry(Encoding.UTF8.GetBytes("root"), null, 0, 0, rootSize, rootCheckSum, "", 3);
        rootEntry.hname = rootEntry.CalHameString();

        SqliteCommand command = new SqliteCommand(MetaDB);
        command.CommandText = "DELETE FROM a";
        command.ExecuteNonQuery();

        int index = 1;
        InsertManifestEntry(index, "manifest3", rootEntry, ref command);

        //Insert platform
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
        InsertManifestEntry(index, "manifest2", platformEntry, ref command);

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
            foreach (var entry in assetEntrys)
            {
                index++;
                InsertManifestEntry(index, "manifest", entry, ref command);
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
            using (SqliteTransaction tran = MetaDB.BeginTransaction())
            {
                foreach (var entry in assets)
                {
                    index++;
                    InsertManifestEntry(index, assetEntry.tname, entry, ref command);
                }
                tran.Commit();
            }
        }
        MetaDB.Close();
    }

    private void InsertManifestEntry(int index, string type, ManifestEntry entry, ref SqliteCommand command)
    {
        command.CommandText = "INSERT INTO a (i,n,d,g,l,c,h,m,k,s,p) VALUES (@id,@name,@deps,@group,@length,@check,@hash,@m,@k,@s,@p)";
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
            yield return DownloadManifest(hash);
        }
        if (File.Exists(path))
        {
            callback.Invoke(GetManifest(path, true, kind));
        }
    }

    public IEnumerator DownloadManifest(string hash)
    {
        UnityWebRequest www = UnityWebRequest.Get(GetManifestRequestUrl(hash));
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
        if (createPath && !Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path + $"/{hash}";
    }

    public static string GetManifestRequestUrl(string hash)
    {
        return $"{MANIFEST_ROOT_URL}/{hash.Substring(0, 2)}/{hash}";
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

