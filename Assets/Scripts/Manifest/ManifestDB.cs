using Mono.Data.Sqlite;
using System;
using UnityEngine;
using static BSVReader;
using static ManifestCategory;

public class ManifestDB
{
    public SqliteConnection metaDb;

    public static ManifestEntry[] GetManifest(string path, bool isCompressed, Kind kind)
    {
        IBSVReader reader = Init(path, ReadMode.Memory, isCompressed);
        ulong count = reader.GetRowCount();
        ManifestEntry[] manifests = new ManifestEntry[count];
        if (kind == Kind.AssetManifest)
        {
            for (ulong i = 0; i < count; i++)
            {
                manifests[i].kind = (int)kind;
                if (!reader.ReadLine(ManifestEntry.GetBsvParser(ManifestEntry.Format.Full, reader), ref manifests[i]))
                {
                    throw new Exception("ReadLineError");
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
                    throw new Exception("ReadLineError");
                }
            }
        }
        return manifests;
    }

    
}

