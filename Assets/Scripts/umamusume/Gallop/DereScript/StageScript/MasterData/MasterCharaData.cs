using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sqlite3Plugin;

public class MasterCharaData
{
    /// <summary>
    /// 全ての要素をSelectするSQL文
    /// </summary>
    private static string selectallsql = "SELECT `chara_id`,`name`,`name_kana`,`age`,`home_town`,`height`,`weight`,`body_size_1`,`body_size_2`,`body_size_3`,`birth_month`,`birth_day`,`constellation`,`blood_type`,`hand`,`favorite`,`voice`,`model_height_id`,`model_weight_id`,`model_bust_id`,`model_skin_id`,`spine_size`,`personality`,`type`,`base_card_id`,`bus_vo_value`,`bus_da_value`,`bus_vi_value`,`special_type` FROM `chara_data`;";

    public class CharaData
    {
        protected int _charaId;

        protected string _name;

        protected string _nameKana;

        protected int _age;

        protected int _homeTown;

        protected int _height;

        protected int _weight;

        protected int _bodySize1;

        protected int _bodySize2;

        protected int _bodySize3;

        protected int _birthMonth;

        protected int _birthDay;

        protected int _constellation;

        protected int _bloodType;

        protected int _hand;

        protected string _favorite;

        protected string _voice;

        protected int _modelHeightId;

        protected int _modelWeightId;

        protected int _modelBustId;

        protected int _modelSkinId;

        protected int _spineSize;

        protected int _personality;

        protected int _attribute;

        protected int _baseCardId;

        protected int _busVoValue;

        protected int _busDaValue;

        protected int _busViValue;

        protected int _specialType;

        private const int PROFILE_TEXT_OFFSET = 5000;

        private const int PROFILE_CONSTELLATION_OFFSET = 1000;

        private const int PROFILE_BLOOD_OFFSET = 2000;

        private const int PROFILE_HAND_OFFSET = 3000;

        private string _boneType;

        public int charaId => _charaId;

        public string name => _name;

        public string nameKana => _nameKana;

        public int age => _age;

        public int homeTown => _homeTown;

        public int height => _height;

        public int weight => _weight;

        public int bodySize1 => _bodySize1;

        public int bodySize2 => _bodySize2;

        public int bodySize3 => _bodySize3;

        public int birthMonth => _birthMonth;

        public int birthDay => _birthDay;

        public int constellation => _constellation;

        public int bloodType => _bloodType;

        public int hand => _hand;

        public string favorite => _favorite;

        public string voice => _voice;

        public int modelHeightId => _modelHeightId;

        public int modelWeightId => _modelWeightId;

        public int modelBustId => _modelBustId;

        public int modelSkinId => _modelSkinId;

        public int spineSize => _spineSize;

        public int personality => _personality;

        public int attribute => _attribute;

        public int baseCardId => _baseCardId;

        public int busVoValue => _busVoValue;

        public int busDaValue => _busDaValue;

        public int busViValue => _busViValue;

        public int specialType => _specialType;

        public string boneType
        {
            get
            {
                if (_boneType == null)
                {
                    _boneType = _spineSize.ToString();
                }
                return _boneType;
            }
        }

        public CharaData(string[] record)
        {
            _charaId = int.Parse(record[0]);
            _name = record[1];
            _nameKana = record[2];
            _age = int.Parse(record[3]);
            _homeTown = int.Parse(record[4]);
            _height = int.Parse(record[5]);
            _weight = int.Parse(record[6]);
            _bodySize1 = int.Parse(record[7]);
            _bodySize2 = int.Parse(record[8]);
            _bodySize3 = int.Parse(record[9]);
            _birthMonth = int.Parse(record[10]);
            _birthDay = int.Parse(record[11]);
            _constellation = int.Parse(record[12]);
            _bloodType = int.Parse(record[13]);
            _hand = int.Parse(record[14]);
            _favorite = record[15];
            _voice = record[16];
            _modelHeightId = int.Parse(record[17]);
            _modelWeightId = int.Parse(record[18]);
            _modelBustId = int.Parse(record[19]);
            _modelSkinId = int.Parse(record[20]);
            _spineSize = int.Parse(record[21]);
            _personality = int.Parse(record[22]);
            _attribute = int.Parse(record[23]);
            _baseCardId = int.Parse(record[24]);
            _busVoValue = int.Parse(record[25]);
            _busDaValue = int.Parse(record[26]);
            _busViValue = int.Parse(record[27]);
            _specialType = int.Parse(record[28]);
        }

        public CharaData(int charaId = 0, string name = "", string nameKana = "", int age = 0, int homeTown = 0, int height = 0, int weight = 0, int bodySize1 = 0, int bodySize2 = 0, int bodySize3 = 0, int birthMonth = 0, int birthDay = 0, int constellation = 0, int bloodType = 0, int hand = 0, string favorite = "", string voice = "", int modelHeightId = 0, int modelWeightId = 0, int modelBustId = 0, int modelSkinId = 0, int spineSize = 0, int personality = 0, int attribute = 0, int baseCardId = 0, int busVoValue = 0, int busDaValue = 0, int busViValue = 0, int specialType = 0)
        {
            _charaId = charaId;
            _name = name;
            _nameKana = nameKana;
            _age = age;
            _homeTown = homeTown;
            _height = height;
            _weight = weight;
            _bodySize1 = bodySize1;
            _bodySize2 = bodySize2;
            _bodySize3 = bodySize3;
            _birthMonth = birthMonth;
            _birthDay = birthDay;
            _constellation = constellation;
            _bloodType = bloodType;
            _hand = hand;
            _favorite = favorite;
            _voice = voice;
            _modelHeightId = modelHeightId;
            _modelWeightId = modelWeightId;
            _modelBustId = modelBustId;
            _modelSkinId = modelSkinId;
            _spineSize = spineSize;
            _personality = personality;
            _attribute = attribute;
            _baseCardId = baseCardId;
            _busVoValue = busVoValue;
            _busDaValue = busDaValue;
            _busViValue = busViValue;
            _specialType = specialType;
        }
    }

    private Dictionary<int, CharaData> charaDataDic = new Dictionary<int, CharaData>();

    public void Load(ref DBProxy masterDB)
    {
        using (Query query = masterDB.Query(selectallsql))
        {
            while (query.Step())
            {
                int @int = query.GetInt(0);
                string text = query.GetText(1);
                string text2 = query.GetText(2);
                int int2 = query.GetInt(3);
                int int3 = query.GetInt(4);
                int int4 = query.GetInt(5);
                int int5 = query.GetInt(6);
                int int6 = query.GetInt(7);
                int int7 = query.GetInt(8);
                int int8 = query.GetInt(9);
                int int9 = query.GetInt(10);
                int int10 = query.GetInt(11);
                int int11 = query.GetInt(12);
                int int12 = query.GetInt(13);
                int int13 = query.GetInt(14);
                string text3 = query.GetText(15);
                string text4 = query.GetText(16);
                int int14 = query.GetInt(17);
                int int15 = query.GetInt(18);
                int int16 = query.GetInt(19);
                int int17 = query.GetInt(20);
                int int18 = query.GetInt(21);
                int int19 = query.GetInt(22);
                int int20 = query.GetInt(23);
                int int21 = query.GetInt(24);
                int int22 = query.GetInt(25);
                int int23 = query.GetInt(26);
                int int24 = query.GetInt(27);
                int int25 = query.GetInt(28);
                int key = @int;

                //ちひろ用
                if(int20 == 4)
                {
                    int20 = UnityEngine.Random.Range(1, 3);
                }

                CharaData value = new CharaData(@int, text, text2, int2, int3, int4, int5, int6, int7, int8, int9, int10, int11, int12, int13, text3, text4, int14, int15, int16, int17, int18, int19, int20, int21, int22, int23, int24, int25);
                if (!charaDataDic.ContainsKey(key))
                {
                    charaDataDic.Add(key, value);
                }
            }
        }
    }


    public CharaData Get(int charaId)
    {
        if (charaDataDic.TryGetValue(charaId, out var value))
        {
            return value;
        }
        else
        {
            return null;
        }

    }

    public bool CheckExcludeChara(CharaData charaData = null, int charaId = 0)
    {
        CharaData charaData2 = null;
        charaData2 = ((charaData != null || charaId == 0) ? charaData : Get(charaId));
        if (charaData2 != null && (int)charaData2.specialType == 1)
        {
            return true;
        }
        return false;
    }
}
