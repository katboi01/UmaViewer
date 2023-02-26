using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// URLを管理する
/// </summary>
public class URLData
{
    public string OS;

    private const string rootAdd = @"https://asset-starlight-stage.akamaized.net/dl/resources/";
    private const string _assetbaseURL = @"AssetBundles/";
    private const string _soundbaseURL = @"Sound/";
    private const string _sqldbbaseURL = @"Generic/";

    private string assetbaseURL
    {
        get
        {
            return rootAdd + _assetbaseURL;
        }
    }
    private string soundbaseURL
    {
        get
        {
            return rootAdd + _soundbaseURL;
        }
    }
    private string sqldbbaseURL
    {
        get
        {
            return rootAdd + _sqldbbaseURL;
        }
    }

    public string _name;

    public string hash;

    public string dir
    {
        get
        {
            if (Path.GetDirectoryName(_name) == "") { return ""; }
            else { return Path.GetDirectoryName(_name); }
        }
    }

    /// <summary>
    /// ファイルDL時に使用するURL
    /// </summary>
    public string url
    {
        get
        {
            string ret = "";
            //ファイル名でURLを振り分け
            switch (Path.GetExtension(_name))
            {
                case ".mdb":
                case ".bdb":
                    ret = sqldbbaseURL;
                    break;
                case ".unity3d":
                    ret = assetbaseURL;
                    break;
                case ".acb":
                    ret = soundbaseURL;
                    break;
                default:
                    //拡張子がおかしい
                    return "";
            }
            if (hash.Length < 2)
            {
                //ハッシュがない
                return "";
            }

            //hashの先頭2文字を切り出し
            string hashdir = hash.Substring(0, 2);
            ret = ret + hashdir + @"/" + hash;
            
            return ret;
        }
    }

    public string name
    {
        get { return Path.GetFileName(_name); }
    }

    /// <summary>
    /// LZ4拡張子付き
    /// </summary>
    public string filename
    {
        get
        {
            string ret = "";
            switch (Path.GetExtension(_name))
            {
                case ".mdb":
                case ".bdb":
                case ".unity3d":
                    ret = Path.GetFileName(_name) + ".lz4";
                    break;
                case ".acb":
                    ret = Path.GetFileName(_name);
                    break;
                default:
                    break;
            }
            return ret;
        }
    }

    public URLData()
    {
        OS = OSConfig.osName;
    }
};