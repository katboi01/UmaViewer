using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class AssetUtil
{
    static public bool _BitTest(int valueToTest, int testBit)
    {
        return ((valueToTest & (1 << testBit)) == (1 << testBit));
    }

    public enum PlatformID
    {
        iOS = 4,
        PC = 5,
        Android = 13
    };

    /// <summary>
    /// プラットフォームをAndroid(13)からPC(5)へ変更する
    /// </summary>
    public static bool RewriteAssetPlatform(ref byte[] bundle, string unityVersion)
    {
        if (unityVersion == "")
        {
            return false;
        }
        byte[] sArray = Encoding.ASCII.GetBytes(unityVersion);

        //後ろにプラットフォームを付加
        Array.Resize(ref sArray, sArray.Length + 2);
        sArray[sArray.Length - 1] = (byte)PlatformID.Android;

        //UnityVersionの位置を検索
        int keypos = ByteIndexOfOne(ref bundle, sArray);
        if (keypos > 0)
        {
            bundle[keypos + sArray.Length - 1] = (byte)PlatformID.PC; //platformをPC(05)に変更する
            return true;
        }
        else
        {
            Console.WriteLine("unityVersion:" + unityVersion + " Notfound!");
            return false;
        }
    }

    /// <summary>
    /// AssetBundleファイルをの中の画像ファイルのRewriteReadableを書き換えてEnableに変更する
    /// </summary>
    /// <param name="binArray">AssetBundleバイナリ</param>
    public static void RewriteReadable(ref byte[] binArray)
    {
        //Bundleから画像一覧を取得
        List<string> texList = new List<string>();
        AssetBundle bundle = AssetBundle.LoadFromMemory(binArray);
        foreach (var item in bundle.LoadAllAssets<Texture2D>())
        {
            texList.Add(item.name);
        }
        bundle.Unload(true);//読んだTextureは破棄

        //binaryを書き換え
        foreach (var item in texList)
        {
            RewriteTextureReadableFlag(ref binArray, item);
        }
    }

    /// <summary>
    /// AssetBundleバイナリからRewriteReadableフラグを書き換える
    /// </summary>
    /// <param name="bundle">Bundleバイナリ</param>
    /// <param name="texname">テクスチャ名</param>
    private static void RewriteTextureReadableFlag(ref byte[] bundle, string texname)
    {
        int readableOffset = 0x1c;

        //テクスチャ名をバイナリに変換
        byte[] tmparray = Encoding.ASCII.GetBytes(texname);
        //頭にバイナリ長を加えることで誤検索を防止
        byte[] searcharray = new byte[tmparray.Length + 4];
        Array.Copy(tmparray, 0, searcharray, 4, tmparray.Length);

        searcharray[0] = (byte)tmparray.Length; //テキストファイル名長なので0xff(256)バイトは超えないはず…

        //binaryを検索
        int keypos = ByteIndexOfOne(ref bundle, searcharray);

        if (keypos < 0) { return; }

        //4byte区切りのデータ列のため
        int offset = searcharray.Length;
        if ((offset % 4) > 0)
        {
            offset += 4 - (offset % 4);
        }

        if (bundle[keypos + offset + readableOffset] == 0x00)
        {
            bundle[keypos + offset + readableOffset] = 0x01; //readableをTrueに変更する
        }
        if (bundle[keypos + offset + readableOffset + 1] == 0x00)
        {
            bundle[keypos + offset + readableOffset + 1] = 0x01; //ReadAllowedをTrueに変更する
        }
    }

    /// <summary>
    /// バイナリ列からバイナリ列の検索を行い、最初に見つかったポジションを返す
    /// </summary>
    /// <param name="target">検索元バイナリ</param>
    /// <param name="pattern">検索パターン</param>
    /// <returns>結果位置</returns>
    private static int ByteIndexOfOne(ref byte[] target, byte[] pattern)
    {
        byte key = 0;
        int binLen = target.Length;

        for (int i = 0; i < binLen; i++)
        {
            key = target[i];
            if (key == pattern[0])
            {
                int cnt = 1;
                while (true)
                {
                    if ((i + cnt) == target.Length)
                    {
                        //target長を超えた
                        break;
                    }
                    key = target[i + cnt];
                    if (key != pattern[cnt])
                    {
                        //差異あり
                        break;
                    }
                    cnt++;
                    if (cnt == pattern.Length)
                    {
                        //パターン長合致
                        return i;
                    }
                }
            }
        }
        return -1;
    }

    /// <summary>
    /// バイナリ列からバイナリ列の検索を行い全ての結果を返す
    /// </summary>
    /// <param name="target">検索元バイナリ</param>
    /// <param name="pattern">検索パターン</param>
    /// <returns>結果位置</returns>
    private static int[] ByteIndexOfMultiple(ref byte[] target, byte[] pattern)
    {
        byte key = 0;
        int binLen = target.Length;
        List<int> hitdatas = new List<int>();

        for (int i = 0; i < binLen; i++)
        {
            key = target[i];
            if (key == pattern[0])
            {
                int cnt = 1;
                while (true)
                {
                    if ((i + cnt) == target.Length)
                    {
                        //target長を超えた
                        break;
                    }
                    key = target[i + cnt];
                    if (key != pattern[cnt])
                    {
                        //差異あり
                        break;
                    }
                    cnt++;
                    if (cnt == pattern.Length)
                    {
                        //パターン長合致
                        hitdatas.Add(i);
                        break;
                    }
                }
            }
        }

        //あったら配列で返す
        if (hitdatas.Count > 0)
        {
            return hitdatas.ToArray();
        }
        else
        {
            return null;
        }
    }


    /// <summary>
    /// バイナリ列を検索して最初に見つかった検索パターンを置換する
    /// </summary>
    /// <param name="target">検索元バイナリ</param>
    /// <param name="pattern">検索パターン</param>
    /// <param name="replace">置換パターン</param>
    /// <returns>位置</returns>
    private static int ReplaceIndexOfOne(ref byte[] target, byte[] pattern, byte[] replace)
    {
        if (pattern.Length != replace.Length)
        {
            return -2;
        }

        int pos = ByteIndexOfOne(ref target, pattern);
        if (pos < 0) { return pos; } //見つからなかった

        int length = pattern.Length;
        for (int i = 0; i < length; i++)
        {
            if (target[pos + i] == pattern[i])
            {
                target[pos + i] = replace[i];
            }
        }
        return pos;
    }

    /// <summary>
    /// バイナリ列を検索して検索パターンにヒットした箇所を全て置換する
    /// </summary>
    /// <param name="target">検索元バイナリ</param>
    /// <param name="pattern">検索パターン</param>
    /// <param name="replace">置換パターン</param>
    /// <returns>位置配列</returns>
    private static int[] ReplaceIndexOfAll(ref byte[] target, byte[] pattern, byte[] replace)
    {
        if (pattern.Length != replace.Length)
        {
            return null;
        }

        int[] pos = ByteIndexOfMultiple(ref target, pattern);
        if (pos == null) { return null; } //見つからなかった

        int length = pattern.Length;
        for (int i = 0; i < pos.Length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                if (target[pos[i] + j] == pattern[j])
                {
                    target[pos[i] + j] = replace[j];
                }
            }
        }
        return pos; //置換回数を返す
    }

    private static uint readIntBE(byte[] m_array)
    {
        if (m_array.Length == 4)
        {
            return (uint)(m_array[3] | (m_array[2] << 8) | (m_array[1] << 16) | (m_array[0] << 24));
        }
        return 0;
    }
}