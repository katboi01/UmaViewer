public class SQDressData
{
    public static bool isPlus = false;

    /// <summary>
    /// ドレスデータNo
    /// 0-10 共通衣装
    /// 100 SSR
    /// 7000000- ショップ
    /// </summary>
    public int dressID;

    /// <summary>
    /// ドレス名
    /// </summary>
    public string dressName;

    /// <summary>
    /// カードのアイコンID
    /// </summary>
    public int cardID;

    /// <summary>
    /// カードのアイコンID
    /// 特訓後のほう
    /// </summary>
    public int cardIDPlus;

    /// <summary>
    /// キャラID
    /// </summary>
    public int charaID;

    /// <summary>
    /// カード名
    /// </summary>
    public string cardName;

    /// <summary>
    /// タイプ
    /// </summary>
    public int type;

    /// <summary>
    /// SSRドレスモデルのファイルID
    /// 共通/ショップ衣装はdressIDから
    /// </summary>
    public int openDressID;

    /// <summary>
    /// ソロライブが存在するか
    /// </summary>
    public int soloLive;

    private const string _commonshop = "dressselect_{0:D3}.unity3d";
    private const string _ssrcardicon = "card_{0}_m.unity3d";

    /// <summary>
    /// ユニークなドレスID
    /// 共通/ショップ衣装はdressID
    /// SSR衣装はドレスIDを返す
    /// </summary>
    public int activeDressID
    {
        get
        {
            if (dressID == 100) return openDressID; //SSRのとき
            if (dressID > 7000000) return dressID + charaID; //ショップ衣装の時
            return dressID; //共通衣装の時
        }
    }

    /// <summary>
    /// ユニークなドレス名
    /// 共通/ショップ衣装はdress名
    /// SSR衣装はカード名を返す
    /// </summary>
    public string dressKeyName
    {
        get
        {
            if (dressID == 100)
            {
                return cardName;
            }
            else
            {
                return dressName;
            }

        }
    }
    /// <summary>
    /// アイコンファイルのIDを返す
    /// </summary>
    public int dressIconKey
    {
        get
        {
            if (dressID == 100)
            {
                if (isPlus)
                {
                    return cardIDPlus;
                }
                else
                {
                    return cardID;
                }
            }
            else
            {
                return dressID;
            }

        }
    }

    public static string getIconfilename(int key)
    {
        if (key < 100)
        {
            return string.Format(_commonshop, key);
        }
        else if (key > 7000000)
        {
            return string.Format(_commonshop, key);
        }
        else
        {
            return string.Format(_ssrcardicon, key);
        }
    }
}
