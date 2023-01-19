using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using MiniJSON; //Dictionaryが扱えるMiniJsonを使用

public class SaveManager
{
    private static SaveDataBase savedatabase = null;

    private static SaveDataBase saveDataBase
    {
        get
        {
            if (savedatabase == null)
            {
                string path = Path.Combine(Application.persistentDataPath, "savedata.json");
                savedatabase = new SaveDataBase(path);
                DefaultSaveSetting();
            }
            return savedatabase;
        }
    }

    /// <summary>
    /// デフォルト値を事前に設定する
    /// </summary>
    private static void DefaultSaveSetting()
    {
        DefaultSetInt("SettingSwitch", 1);
        DefaultSetInt("Confetti", 1);
        DefaultSetInt("InitTitle", 1);
        DefaultSetInt("Lyrics", 1);
        DefaultSetInt("Mob", 1);
        DefaultSetInt("Outline", 1);
        DefaultSetInt("CySpring", 0);
        DefaultSetInt("ReturnTitleEndStage", 1);
        DefaultSetInt("SelectOriginalMember", 2);
        DefaultSetInt("RandomMemberSet", 2);
        DefaultSetInt("SSRIcon", 1);
        DefaultSetInt("ResolutionRatio", 1);
        DefaultSetInt("ScreenShot", 2);

        DefaultSetInt("ViewSwitch", 1);
        DefaultSetInt("IdolSelectSwitch", 1);

        DefaultSetInt("CharaSortOrder", 0);
        DefaultSetInt("MusicSortOrder", 0);
        DefaultSetInt("CharaRefineSelect", 1);
        DefaultSetInt("MusicRefineSelect", 1);

        DefaultSetInt("PostEffectDebug", 1);
        DefaultSetInt("PostEffectDebugPanel", 0);
        DefaultSetInt("config_PostEffect", 1);
        DefaultSetInt("config_GlobalFog", 1);
        DefaultSetInt("config_SunShafts", 1);
        DefaultSetInt("config_TiltShift", 1);
        DefaultSetInt("config_IndirectLightShafts", 1);
        DefaultSetInt("config_A2U", 1);
        DefaultSetInt("config_ColorCorrection", 1);
        DefaultSetInt("config_Bloom", 1);
        DefaultSetInt("config_Dof", 1);
        DefaultSetInt("config_Diffusion", 0);
        DefaultSetInt("config_ScreenOverlay", 1);
        DefaultSetInt("config_AntiAliasing", 1);
    }

    private static void DefaultSetInt(string key, int value)
    {
        var tmp = GetInt(key, -1);
        if (tmp == -1)
        {
            SetInt(key, value);
        }
    }

    /// <summary>
    /// 指定されたキーに関連付けられている値を取得します。
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="value">値</param>
    public static void SetString(string key, string value)
    {
        saveDataBase.SetString(key, value);
    }

    /// <summary>
    /// 指定されたキーに関連付けられているString型の値を取得します。
    /// 値がない場合、_defaultの値を返します。省略した場合、空の文字列を返します。
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="_default">デフォルトの値</param>
    /// <exception cref="System.ArgumentException"></exception>
    /// <returns></returns>
    public static string GetString(string key, string _default = "")
    {
        return saveDataBase.GetString(key, _default);
    }

    /// <summary>
    /// 指定されたキーに関連付けられているInt型の値を取得します。
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="value">デフォルトの値</param>
    /// <exception cref="System.ArgumentException"></exception>
    public static void SetInt(string key, int value)
    {
        saveDataBase.SetInt(key, value);
    }

    /// <summary>
    /// 指定されたキーに関連付けられているInt型の値を取得します。
    /// 値がない場合、_defaultの値を返します。省略した場合、0を返します。
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="_default">デフォルトの値</param>
    /// <exception cref="System.ArgumentException"></exception>
    /// <returns></returns>
    public static int GetInt(string key, int _default = 0)
    {
        return saveDataBase.GetInt(key, _default);
    }

    #region Idolset
    /// <summary>
    /// 指定された配置のキャラクター値を設定します
    /// </summary>
    /// <param name="place">箇所</param>
    /// <param name="value">キャラクターID</param>
    public static void SetChara(int place, int value)
    {
        saveDataBase.SetChara(place, value);
    }

    /// <summary>
    /// 指定された配置のキャラクターアイコン値を設定します
    /// </summary>
    /// <param name="place">箇所</param>
    /// <param name="value">キャラアイコンID</param>
    public static void SetCharaIcon(int place, int value)
    {
        saveDataBase.SetCharaIcon(place, value);
    }

    /// <summary>
    /// 指定された配置の衣装値を設定します
    /// </summary>
    /// <param name="place">箇所</param>
    /// <param name="value">衣装ID</param>
    public static void SetDress(int place, int value)
    {
        saveDataBase.SetDress(place, value);
    }

    /// <summary>
    /// 指定された配置の衣装アイコン値を設定します
    /// </summary>
    /// <param name="place">箇所</param>
    /// <param name="value">衣装アイコンID</param>
    public static void SetDressIcon(int place, int value)
    {
        saveDataBase.SetDressIcon(place, value);
    }

    /// <summary>
    /// 指定された配置のキャラクタIDを取得します
    /// </summary>
    /// <param name="place">箇所</param>
    /// <param name="_default">デフォルトの値</param>
    public static int GetChara(int place, int _default = 0)
    {
        return saveDataBase.GetChara(place, _default);
    }

    /// <summary>
    /// 指定された配置のキャラクタアイコンIDを取得します
    /// </summary>
    /// <param name="place">箇所</param>
    /// <param name="_default">デフォルトの値</param>
    public static int GetCharaIcon(int place, int _default = 0)
    {
        return saveDataBase.GetCharaIcon(place, _default);
    }

    /// <summary>
    /// 指定された配置の衣装IDを取得します
    /// </summary>
    /// <param name="place">箇所</param>
    /// <param name="_default">デフォルトの値</param>
    public static int GetDress(int place, int _default = 0)
    {
        return saveDataBase.GetDress(place, _default);
    }

    /// <summary>
    /// 指定された配置の衣装アイコンIDを取得します
    /// </summary>
    /// <param name="place">箇所</param>
    /// <param name="_default">デフォルトの値</param>
    public static int GetDressIcon(int place, int _default = 0)
    {
        return saveDataBase.GetDressIcon(place, _default);
    }

    /// <summary>
    /// 保存された楽曲IDを取得します
    /// </summary>
    /// <returns>MusicID</returns>
    public static int GetMusicID()
    {
        return saveDataBase.GetInt("music", 0);
    }

    /// <summary>
    /// Unitリストを取得する
    /// </summary>
    /// <returns></returns>
    public static List<IdolSet> GetUnitList()
    {
        return saveDataBase.UnitList;
    }

    public static void SetUnitList(List<IdolSet> list)
    {
        saveDataBase.UnitList = list;
    }

    /// <summary>
    /// 現在のユニットを取得
    /// </summary>
    /// <returns></returns>
    public static IdolSet GetCurrentUnit()
    {
        return saveDataBase.CurrentUnit;
    }

    public static void AddUnit(IdolSet idolSet)
    {
        saveDataBase.AddUnit(idolSet);
        saveDataBase.Save();
    }

    public static void SetIdolUnit(IdolSet idolSet)
    {
        saveDataBase.SetIdolUnit(idolSet);
    }

    #endregion

    public static void Delete()
    {
        saveDataBase.Delete();
    }

    /// <summary>
    /// 明示的にファイルに書き込みます。
    /// </summary>
    public static void Save()
    {
        saveDataBase.Save();
    }

    private class SaveDataBase
    {
        private string filepath = "";

        private Dictionary<string, object> saveDictionary = null;

        private IdolSet idolset = default(IdolSet);

        private List<IdolSet> idolSetList = null;
        
        public SaveDataBase(string fileName)
        {
            filepath = fileName;
            saveDictionary = new Dictionary<string, object>();
            Load();
        }


#region IdolSet

        public void SetChara(int place, int value)
        {
            if (place < 0 || place > 14) return;
            switch (place)
            {
                case 0: idolset.chara0 = value; break;
                case 1: idolset.chara1 = value; break;
                case 2: idolset.chara2 = value; break;
                case 3: idolset.chara3 = value; break;
                case 4: idolset.chara4 = value; break;
                case 5: idolset.chara5 = value; break;
                case 6: idolset.chara6 = value; break;
                case 7: idolset.chara7 = value; break;
                case 8: idolset.chara8 = value; break;
                case 9: idolset.chara9 = value; break;
                case 10: idolset.chara10 = value; break;
                case 11: idolset.chara11 = value; break;
                case 12: idolset.chara12 = value; break;
                case 13: idolset.chara13 = value; break;
                case 14: idolset.chara14 = value; break;
            }
        }

        public void SetCharaIcon(int place, int value)
        {
            if (place < 0 || place > 14) return;
            switch (place)
            {
                case 0: idolset.charaIcon0 = value; break;
                case 1: idolset.charaIcon1 = value; break;
                case 2: idolset.charaIcon2 = value; break;
                case 3: idolset.charaIcon3 = value; break;
                case 4: idolset.charaIcon4 = value; break;
                case 5: idolset.charaIcon5 = value; break;
                case 6: idolset.charaIcon6 = value; break;
                case 7: idolset.charaIcon7 = value; break;
                case 8: idolset.charaIcon8 = value; break;
                case 9: idolset.charaIcon9 = value; break;
                case 10: idolset.charaIcon10 = value; break;
                case 11: idolset.charaIcon11 = value; break;
                case 12: idolset.charaIcon12 = value; break;
                case 13: idolset.charaIcon13 = value; break;
                case 14: idolset.charaIcon14 = value; break;
            }
        }

        public void SetDress(int place, int value)
        {
            if (place < 0 || place > 14) return;
            switch (place)
            {
                case 0: idolset.dress0 = value; break;
                case 1: idolset.dress1 = value; break;
                case 2: idolset.dress2 = value; break;
                case 3: idolset.dress3 = value; break;
                case 4: idolset.dress4 = value; break;
                case 5: idolset.dress5 = value; break;
                case 6: idolset.dress6 = value; break;
                case 7: idolset.dress7 = value; break;
                case 8: idolset.dress8 = value; break;
                case 9: idolset.dress9 = value; break;
                case 10: idolset.dress10 = value; break;
                case 11: idolset.dress11 = value; break;
                case 12: idolset.dress12 = value; break;
                case 13: idolset.dress13 = value; break;
                case 14: idolset.dress14 = value; break;
            }
        }

        public void SetDressIcon(int place, int value)
        {
            if (place < 0 || place > 14) return;
            switch (place)
            {
                case 0: idolset.dressIcon0 = value; break;
                case 1: idolset.dressIcon1 = value; break;
                case 2: idolset.dressIcon2 = value; break;
                case 3: idolset.dressIcon3 = value; break;
                case 4: idolset.dressIcon4 = value; break;
                case 5: idolset.dressIcon5 = value; break;
                case 6: idolset.dressIcon6 = value; break;
                case 7: idolset.dressIcon7 = value; break;
                case 8: idolset.dressIcon8 = value; break;
                case 9: idolset.dressIcon9 = value; break;
                case 10: idolset.dressIcon10 = value; break;
                case 11: idolset.dressIcon11 = value; break;
                case 12: idolset.dressIcon12 = value; break;
                case 13: idolset.dressIcon13 = value; break;
                case 14: idolset.dressIcon14 = value; break;
            }
        }
        
        public int GetChara(int place, int _default)
        {
            int returnvalue = _default;
            if (place < 0 || place > 14) return returnvalue;

            switch (place)
            {
                case 0: returnvalue = idolset.chara0; break;
                case 1: returnvalue = idolset.chara1; break;
                case 2: returnvalue = idolset.chara2; break;
                case 3: returnvalue = idolset.chara3; break;
                case 4: returnvalue = idolset.chara4; break;
                case 5: returnvalue = idolset.chara5; break;
                case 6: returnvalue = idolset.chara6; break;
                case 7: returnvalue = idolset.chara7; break;
                case 8: returnvalue = idolset.chara8; break;
                case 9: returnvalue = idolset.chara9; break;
                case 10: returnvalue = idolset.chara10; break;
                case 11: returnvalue = idolset.chara11; break;
                case 12: returnvalue = idolset.chara12; break;
                case 13: returnvalue = idolset.chara13; break;
                case 14: returnvalue = idolset.chara14; break;
            }
            return returnvalue;
        }

        public int GetCharaIcon(int place, int _default)
        {
            int returnvalue = _default;
            if (place < 0 || place > 14) return returnvalue;

            switch (place)
            {
                case 0: returnvalue = idolset.charaIcon0; break;
                case 1: returnvalue = idolset.charaIcon1; break;
                case 2: returnvalue = idolset.charaIcon2; break;
                case 3: returnvalue = idolset.charaIcon3; break;
                case 4: returnvalue = idolset.charaIcon4; break;
                case 5: returnvalue = idolset.charaIcon5; break;
                case 6: returnvalue = idolset.charaIcon6; break;
                case 7: returnvalue = idolset.charaIcon7; break;
                case 8: returnvalue = idolset.charaIcon8; break;
                case 9: returnvalue = idolset.charaIcon9; break;
                case 10: returnvalue = idolset.charaIcon10; break;
                case 11: returnvalue = idolset.charaIcon11; break;
                case 12: returnvalue = idolset.charaIcon12; break;
                case 13: returnvalue = idolset.charaIcon13; break;
                case 14: returnvalue = idolset.charaIcon14; break;
            }
            return returnvalue;
        }

        public int GetDress(int place, int _default)
        {
            int returnvalue = _default;
            if (place < 0 || place > 14) return returnvalue;

            switch (place)
            {
                case 0: returnvalue = idolset.dress0; break;
                case 1: returnvalue = idolset.dress1; break;
                case 2: returnvalue = idolset.dress2; break;
                case 3: returnvalue = idolset.dress3; break;
                case 4: returnvalue = idolset.dress4; break;
                case 5: returnvalue = idolset.dress5; break;
                case 6: returnvalue = idolset.dress6; break;
                case 7: returnvalue = idolset.dress7; break;
                case 8: returnvalue = idolset.dress8; break;
                case 9: returnvalue = idolset.dress9; break;
                case 10: returnvalue = idolset.dress10; break;
                case 11: returnvalue = idolset.dress11; break;
                case 12: returnvalue = idolset.dress12; break;
                case 13: returnvalue = idolset.dress13; break;
                case 14: returnvalue = idolset.dress14; break;
            }
            return returnvalue;
        }

        public int GetDressIcon(int place, int _default)
        {
            int returnvalue = _default;
            if (place < 0 || place > 14) return returnvalue;

            switch (place)
            {
                case 0: returnvalue = idolset.dressIcon0; break;
                case 1: returnvalue = idolset.dressIcon1; break;
                case 2: returnvalue = idolset.dressIcon2; break;
                case 3: returnvalue = idolset.dressIcon3; break;
                case 4: returnvalue = idolset.dressIcon4; break;
                case 5: returnvalue = idolset.dressIcon5; break;
                case 6: returnvalue = idolset.dressIcon6; break;
                case 7: returnvalue = idolset.dressIcon7; break;
                case 8: returnvalue = idolset.dressIcon8; break;
                case 9: returnvalue = idolset.dressIcon9; break;
                case 10: returnvalue = idolset.dressIcon10; break;
                case 11: returnvalue = idolset.dressIcon11; break;
                case 12: returnvalue = idolset.dressIcon12; break;
                case 13: returnvalue = idolset.dressIcon13; break;
                case 14: returnvalue = idolset.dressIcon14; break;
            }
            return returnvalue;
        }
        
        public IdolSet CurrentUnit
        {
            get
            {
                return idolset;
            }

            set
            {
                idolset = value;
            }
        }

        public List<IdolSet> UnitList
        {
            get
            {
                return idolSetList;
            }
            set
            {
                idolSetList = value;
            }
        }

        public void AddUnit(IdolSet idolSet)
        {
            if (idolSetList == null) idolSetList = new List<IdolSet>();
            
            idolSetList.Add(idolSet);
        }

        public void SetIdolUnit(IdolSet idolSet)
        {
            idolset = idolSet;
        }
#endregion

        public void SetString(string key, string value)
        {
            keyCheck(key);
            saveDictionary[key] = value;
        }

        public string GetString(string key, string _default)
        {
            keyCheck(key);

            if (!saveDictionary.ContainsKey(key))
            {
                return _default;
            }
            return (string)saveDictionary[key];
        }

        public void SetInt(string key, int value)
        {
            keyCheck(key);
            saveDictionary[key] = value.ToString();
        }

        public int GetInt(string key, int _default)
        {
            keyCheck(key);
            if (!saveDictionary.ContainsKey(key))
            {
                return _default;
            }
            int ret;
            if (!int.TryParse((string)saveDictionary[key], out ret))
            {
                ret = 0;
            }
            return ret;
        }

        public void Delete()
        {
            Clear();
            File.Delete(filepath);
        }

        public void Clear()
        {
            saveDictionary.Clear();
        }

        public bool ContainsKey(string _key)
        {
            return saveDictionary.ContainsKey(_key);
        }

        public void Save()
        {
            using (StreamWriter writer = new StreamWriter(filepath, false, Encoding.GetEncoding("utf-8")))
            {
                Dictionary<string, object> dic = new Dictionary<string, object>(saveDictionary.Count);
                //ソート
                var vs1 = saveDictionary.OrderBy((x) => x.Key);
                foreach(var tmp in vs1)
                {
                    dic[tmp.Key] = tmp.Value;
                }

                //IdolSetをシリアライズ
                idolset.unitname = "";
                object setstr = SerializeIdolSet(idolset);
                dic["idolset"] = setstr;

                if (idolSetList != null && idolSetList.Count > 0)
                {
                    for (int i = 0; i < idolSetList.Count; i++)
                    {
                        string name = string.Format("unit{0:D2}", i);
                        object setstr1 = SerializeIdolSet(idolSetList[i]);
                        dic[name] = setstr1;
                    }
                }
                string str = Json.Serialize(dic);
                str = str.Replace("{", "{\n");
                str = str.Replace(",", ",\n");
                str = str.Replace("}", "\n}");

                writer.Write(str);
            }
        }

        private static object SerializeIdolSet(IdolSet idolSet)
        {
            Dictionary<string, object> tmpDic = new Dictionary<string, object>(61);

            tmpDic.Add("UnitName", idolSet.unitname);

            tmpDic.Add("chara0", idolSet.chara0.ToString());
            tmpDic.Add("chara1", idolSet.chara1.ToString());
            tmpDic.Add("chara2", idolSet.chara2.ToString());
            tmpDic.Add("chara3", idolSet.chara3.ToString());
            tmpDic.Add("chara4", idolSet.chara4.ToString());
            tmpDic.Add("chara5", idolSet.chara5.ToString());
            tmpDic.Add("chara6", idolSet.chara6.ToString());
            tmpDic.Add("chara7", idolSet.chara7.ToString());
            tmpDic.Add("chara8", idolSet.chara8.ToString());
            tmpDic.Add("chara9", idolSet.chara9.ToString());
            tmpDic.Add("chara10", idolSet.chara10.ToString());
            tmpDic.Add("chara11", idolSet.chara11.ToString());
            tmpDic.Add("chara12", idolSet.chara12.ToString());
            tmpDic.Add("chara13", idolSet.chara13.ToString());
            tmpDic.Add("chara14", idolSet.chara14.ToString());

            tmpDic.Add("chara0_icon", idolSet.charaIcon0.ToString());
            tmpDic.Add("chara1_icon", idolSet.charaIcon1.ToString());
            tmpDic.Add("chara2_icon", idolSet.charaIcon2.ToString());
            tmpDic.Add("chara3_icon", idolSet.charaIcon3.ToString());
            tmpDic.Add("chara4_icon", idolSet.charaIcon4.ToString());
            tmpDic.Add("chara5_icon", idolSet.charaIcon5.ToString());
            tmpDic.Add("chara6_icon", idolSet.charaIcon6.ToString());
            tmpDic.Add("chara7_icon", idolSet.charaIcon7.ToString());
            tmpDic.Add("chara8_icon", idolSet.charaIcon8.ToString());
            tmpDic.Add("chara9_icon", idolSet.charaIcon9.ToString());
            tmpDic.Add("chara10_icon", idolSet.charaIcon10.ToString());
            tmpDic.Add("chara11_icon", idolSet.charaIcon11.ToString());
            tmpDic.Add("chara12_icon", idolSet.charaIcon12.ToString());
            tmpDic.Add("chara13_icon", idolSet.charaIcon13.ToString());
            tmpDic.Add("chara14_icon", idolSet.charaIcon14.ToString());

            tmpDic.Add("dress0", idolSet.dress0.ToString());
            tmpDic.Add("dress1", idolSet.dress1.ToString());
            tmpDic.Add("dress2", idolSet.dress2.ToString());
            tmpDic.Add("dress3", idolSet.dress3.ToString());
            tmpDic.Add("dress4", idolSet.dress4.ToString());
            tmpDic.Add("dress5", idolSet.dress5.ToString());
            tmpDic.Add("dress6", idolSet.dress6.ToString());
            tmpDic.Add("dress7", idolSet.dress7.ToString());
            tmpDic.Add("dress8", idolSet.dress8.ToString());
            tmpDic.Add("dress9", idolSet.dress9.ToString());
            tmpDic.Add("dress10", idolSet.dress10.ToString());
            tmpDic.Add("dress11", idolSet.dress11.ToString());
            tmpDic.Add("dress12", idolSet.dress12.ToString());
            tmpDic.Add("dress13", idolSet.dress13.ToString());
            tmpDic.Add("dress14", idolSet.dress14.ToString());

            tmpDic.Add("dress0_icon", idolSet.dressIcon0.ToString());
            tmpDic.Add("dress1_icon", idolSet.dressIcon1.ToString());
            tmpDic.Add("dress2_icon", idolSet.dressIcon2.ToString());
            tmpDic.Add("dress3_icon", idolSet.dressIcon3.ToString());
            tmpDic.Add("dress4_icon", idolSet.dressIcon4.ToString());
            tmpDic.Add("dress5_icon", idolSet.dressIcon5.ToString());
            tmpDic.Add("dress6_icon", idolSet.dressIcon6.ToString());
            tmpDic.Add("dress7_icon", idolSet.dressIcon7.ToString());
            tmpDic.Add("dress8_icon", idolSet.dressIcon8.ToString());
            tmpDic.Add("dress9_icon", idolSet.dressIcon9.ToString());
            tmpDic.Add("dress10_icon", idolSet.dressIcon10.ToString());
            tmpDic.Add("dress11_icon", idolSet.dressIcon11.ToString());
            tmpDic.Add("dress12_icon", idolSet.dressIcon12.ToString());
            tmpDic.Add("dress13_icon", idolSet.dressIcon13.ToString());
            tmpDic.Add("dress14_icon", idolSet.dressIcon14.ToString());

            return tmpDic;
        }

        public void Load()
        {
            //初期化
            if (idolSetList != null)
            {
                idolSetList.Clear();
                idolSetList = null;
            }

            if (File.Exists(filepath))
            {
                using (StreamReader sr = new StreamReader(filepath, Encoding.GetEncoding("utf-8")))
                {
                    if (saveDictionary != null)
                    {
                        var str = sr.ReadToEnd();
                        if (str != "")
                        {
                            saveDictionary = Json.Deserialize(str) as Dictionary<string, object>;

                            //idolset
                            object tmpobj = null;
                            if (saveDictionary.TryGetValue("idolset",out tmpobj))
                            {
                                idolset = DeSerializeIdolSet(tmpobj);
                            }

                            int count = 0;
                            while (true)
                            {
                                object tmpobj2 = null;
                                string name = string.Format("unit{0:D2}", count);
                                if (saveDictionary.TryGetValue(name, out tmpobj2))
                                {
                                    //初回
                                    if (idolSetList == null) idolSetList = new List<IdolSet>();

                                    IdolSet unit = DeSerializeIdolSet(tmpobj2);
                                    unit.unitNo = count;
                                    idolSetList.Add(unit);

                                    saveDictionary.Remove(name);
                                }
                                else
                                {
                                    //見つからなければ抜ける
                                    break;
                                }
                                count++;
                            }
                        }
                    }
                }
            }
            else
            {
                saveDictionary = new Dictionary<string, object>();
                Save(); //ファイルを作る
            }
        }

        private static IdolSet DeSerializeIdolSet(object idolSet)
        {
            Dictionary<string, object> tmpDic = idolSet as Dictionary<string, object>;
            IdolSet returnValue = default(IdolSet);

            if (tmpDic == null) return returnValue;

            object name;
            if (tmpDic.TryGetValue("UnitName", out name))
            {
                returnValue.unitname = (string)name;
            }

            int[] charas = new int[15];
            int[] charaIcons = new int[15];
            int[] dresses = new int[15];
            int[] dressIcons = new int[15];
            for (int i = 0; i < 15; i++)
            {
                object outobj = null;
                if(tmpDic.TryGetValue("chara" + i,out outobj))
                {
                    int.TryParse((string)outobj, out charas[i]);
                }
                if (tmpDic.TryGetValue("chara" + i + "_icon", out outobj))
                {
                    int.TryParse((string)outobj, out charaIcons[i]);
                }
                if (tmpDic.TryGetValue("dress" + i, out outobj))
                {
                    int.TryParse((string)outobj, out dresses[i]);
                }
                if (tmpDic.TryGetValue("dress" + i + "_icon", out outobj))
                {
                    int.TryParse((string)outobj, out dressIcons[i]);
                }
            }

            returnValue.chara0 = charas[0];
            returnValue.chara1 = charas[1];
            returnValue.chara2 = charas[2];
            returnValue.chara3 = charas[3];
            returnValue.chara4 = charas[4];
            returnValue.chara5 = charas[5];
            returnValue.chara6 = charas[6];
            returnValue.chara7 = charas[7];
            returnValue.chara8 = charas[8];
            returnValue.chara9 = charas[9];
            returnValue.chara10 = charas[10];
            returnValue.chara11 = charas[11];
            returnValue.chara12 = charas[12];
            returnValue.chara13 = charas[13];
            returnValue.chara14 = charas[14];

            returnValue.charaIcon0 = charaIcons[0];
            returnValue.charaIcon1 = charaIcons[1];
            returnValue.charaIcon2 = charaIcons[2];
            returnValue.charaIcon3 = charaIcons[3];
            returnValue.charaIcon4 = charaIcons[4];
            returnValue.charaIcon5 = charaIcons[5];
            returnValue.charaIcon6 = charaIcons[6];
            returnValue.charaIcon7 = charaIcons[7];
            returnValue.charaIcon8 = charaIcons[8];
            returnValue.charaIcon9 = charaIcons[9];
            returnValue.charaIcon10 = charaIcons[10];
            returnValue.charaIcon11 = charaIcons[11];
            returnValue.charaIcon12 = charaIcons[12];
            returnValue.charaIcon13 = charaIcons[13];
            returnValue.charaIcon14 = charaIcons[14];

            returnValue.dress0 = dresses[0];
            returnValue.dress1 = dresses[1];
            returnValue.dress2 = dresses[2];
            returnValue.dress3 = dresses[3];
            returnValue.dress4 = dresses[4];
            returnValue.dress5 = dresses[5];
            returnValue.dress6 = dresses[6];
            returnValue.dress7 = dresses[7];
            returnValue.dress8 = dresses[8];
            returnValue.dress9 = dresses[9];
            returnValue.dress10 = dresses[10];
            returnValue.dress11 = dresses[11];
            returnValue.dress12 = dresses[12];
            returnValue.dress13 = dresses[13];
            returnValue.dress14 = dresses[14];

            returnValue.dressIcon0 = dressIcons[0];
            returnValue.dressIcon1 = dressIcons[1];
            returnValue.dressIcon2 = dressIcons[2];
            returnValue.dressIcon3 = dressIcons[3];
            returnValue.dressIcon4 = dressIcons[4];
            returnValue.dressIcon5 = dressIcons[5];
            returnValue.dressIcon6 = dressIcons[6];
            returnValue.dressIcon7 = dressIcons[7];
            returnValue.dressIcon8 = dressIcons[8];
            returnValue.dressIcon9 = dressIcons[9];
            returnValue.dressIcon10 = dressIcons[10];
            returnValue.dressIcon11 = dressIcons[11];
            returnValue.dressIcon12 = dressIcons[12];
            returnValue.dressIcon13 = dressIcons[13];
            returnValue.dressIcon14 = dressIcons[14];

            return returnValue;
        }
        
        /// <summary>
        /// キーに不正がないかチェックします。
        /// </summary>
        private void keyCheck(string _key)
        {
            if (string.IsNullOrEmpty(_key))
            {
                throw new ArgumentException("invalid key!!");
            }
        }
        
    }

    /// <summary>
    /// アイドルセットをパック化
    /// </summary>
    public struct IdolSet
    {
        //管理用
        public int unitNo;

        public string unitname;

        public int chara0;
        public int chara1;
        public int chara2;
        public int chara3;
        public int chara4;
        public int chara5;
        public int chara6;
        public int chara7;
        public int chara8;
        public int chara9;
        public int chara10;
        public int chara11;
        public int chara12;
        public int chara13;
        public int chara14;

        public int charaIcon0;
        public int charaIcon1;
        public int charaIcon2;
        public int charaIcon3;
        public int charaIcon4;
        public int charaIcon5;
        public int charaIcon6;
        public int charaIcon7;
        public int charaIcon8;
        public int charaIcon9;
        public int charaIcon10;
        public int charaIcon11;
        public int charaIcon12;
        public int charaIcon13;
        public int charaIcon14;

        public int dress0;
        public int dress1;
        public int dress2;
        public int dress3;
        public int dress4;
        public int dress5;
        public int dress6;
        public int dress7;
        public int dress8;
        public int dress9;
        public int dress10;
        public int dress11;
        public int dress12;
        public int dress13;
        public int dress14;

        public int dressIcon0;
        public int dressIcon1;
        public int dressIcon2;
        public int dressIcon3;
        public int dressIcon4;
        public int dressIcon5;
        public int dressIcon6;
        public int dressIcon7;
        public int dressIcon8;
        public int dressIcon9;
        public int dressIcon10;
        public int dressIcon11;
        public int dressIcon12;
        public int dressIcon13;
        public int dressIcon14;
    }
}
