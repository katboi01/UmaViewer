using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sqlite3Plugin;

public class MasterLiveData
{
    /// <summary>
    /// 全ての要素をSelectするSQL文
    /// </summary>
    private static string selectallsql = "SELECT `id`,`music_data_id`,`sort`,`difficulty_1`,`difficulty_2`,`difficulty_3`,`difficulty_4`,`circle_type`,`live_bg`,`cyalume`,`chara_all_flag`,`chara_id`,`type`,`sp_type`,`jacket_id`,`prp_flag`,`release_type`,`start_date`,`end_date`,`event_type`,`difficulty_5`,`member_number`,`difficulty_101`,`v_mv`,`difficulty_11`,`difficulty_12`,`difficulty_21`,`difficulty_22`,`difficulty_6` FROM `live_data`;";

    public class LiveData
    {
        protected int _id;

        protected int _musicDataId;

        protected int _sort;

        protected int _difficulty1;

        protected int _difficulty2;

        protected int _difficulty3;

        protected int _difficulty4;

        protected int _circleType;

        protected int _liveBg;

        protected int _cyalume;

        protected int _soundType;

        protected string _charaIdText;

        protected int _type;

        protected int _spType;

        protected int _jacketId;

        protected int _prpFlag;

        protected int _releaseType;

        protected string _startDate;

        protected string _endDate;

        protected int _eventType;

        protected int _difficulty5;

        protected int _memberNumber;

        protected int _difficulty101;

        protected byte _supportVariableMv;

        protected int _difficulty11;

        protected int _difficulty12;

        protected int _difficulty21;

        protected int _difficulty22;

        protected int _difficulty6;

        private int[] _difficulty;

        private int[] _verticalDifficulty;

        private int[] _grandDifficulty;

        public int id => _id;

        public int musicDataId => _musicDataId;

        public int sort => _sort;

        public int difficulty1 => _difficulty1;

        public int difficulty2 => _difficulty2;

        public int difficulty3 => _difficulty3;

        public int difficulty4 => _difficulty4;

        public int circleType => _circleType;

        public int liveBg => _liveBg;

        public int cyalume => _cyalume;

        public int soundType => _soundType;

        public string charaIdText => _charaIdText;

        public int type => _type;

        public int spType => _spType;

        public int jacketId => _jacketId;

        public int prpFlag => _prpFlag;

        public int releaseType => _releaseType;

        public string startDate => _startDate;

        public string endDate => _endDate;

        public int eventType => _eventType;

        public int difficulty5 => _difficulty5;

        public int memberNumber => _memberNumber;

        public int difficulty101 => _difficulty101;

        public byte supportVariableMv => _supportVariableMv;

        public int difficulty11 => _difficulty11;

        public int difficulty12 => _difficulty12;

        public int difficulty21 => _difficulty21;

        public int difficulty22 => _difficulty22;

        public int difficulty6 => _difficulty6;

        public int[] charaId
        {
            get
            {
                string[] array = ((string)_charaIdText).Split(':');
                int num = array.Length;
                int[] array2 = new int[num];
                for (int i = 0; i < num; i++)
                {
                    array2[i] = int.Parse(array[i]);
                }
                return array2;
            }
        }

        public LiveData(string[] record)
        {
            _id = int.Parse(record[0]);
            _musicDataId = int.Parse(record[1]);
            _sort = int.Parse(record[2]);
            _difficulty1 = int.Parse(record[3]);
            _difficulty2 = int.Parse(record[4]);
            _difficulty3 = int.Parse(record[5]);
            _difficulty4 = int.Parse(record[6]);
            _circleType = int.Parse(record[7]);
            _liveBg = int.Parse(record[8]);
            _cyalume = int.Parse(record[9]);
            _soundType = int.Parse(record[10]);
            _charaIdText = record[11];
            _type = int.Parse(record[12]);
            _spType = int.Parse(record[13]);
            _jacketId = int.Parse(record[14]);
            _prpFlag = int.Parse(record[15]);
            _releaseType = int.Parse(record[16]);
            _startDate = record[17];
            _endDate = record[18];
            _eventType = int.Parse(record[19]);
            _difficulty5 = int.Parse(record[20]);
            _memberNumber = int.Parse(record[21]);
            _difficulty101 = int.Parse(record[22]);
            _supportVariableMv = byte.Parse(record[23]);
            _difficulty11 = int.Parse(record[24]);
            _difficulty12 = int.Parse(record[25]);
            _difficulty21 = int.Parse(record[26]);
            _difficulty22 = int.Parse(record[27]);
            _difficulty6 = int.Parse(record[28]);
        }

        public LiveData(int id = 0, int musicDataId = 0, int sort = 0, int difficulty1 = 0, int difficulty2 = 0, int difficulty3 = 0, int difficulty4 = 0, int circleType = 0, int liveBg = 0, int cyalume = 0, int soundType = 0, string charaIdText = "", int type = 0, int spType = 0, int jacketId = 0, int prpFlag = 0, int releaseType = 0, string startDate = "", string endDate = "", int eventType = 0, int difficulty5 = 0, int memberNumber = 0, int difficulty101 = 0, byte supportVariableMv = 0, int difficulty11 = 0, int difficulty12 = 0, int difficulty21 = 0, int difficulty22 = 0, int difficulty6 = 0)
        {
            _id = id;
            _musicDataId = musicDataId;
            _sort = sort;
            _difficulty1 = difficulty1;
            _difficulty2 = difficulty2;
            _difficulty3 = difficulty3;
            _difficulty4 = difficulty4;
            _circleType = circleType;
            _liveBg = liveBg;
            _cyalume = cyalume;
            _soundType = soundType;
            _charaIdText = charaIdText;
            _type = type;
            _spType = spType;
            _jacketId = jacketId;
            _prpFlag = prpFlag;
            _releaseType = releaseType;
            _startDate = startDate;
            _endDate = endDate;
            _eventType = eventType;
            _difficulty5 = difficulty5;
            _memberNumber = memberNumber;
            _difficulty101 = difficulty101;
            _supportVariableMv = supportVariableMv;
            _difficulty11 = difficulty11;
            _difficulty12 = difficulty12;
            _difficulty21 = difficulty21;
            _difficulty22 = difficulty22;
            _difficulty6 = difficulty6;
        }

        public bool IsSupport2DRich()
        {
            return ((byte)_supportVariableMv & 2) == 2;
        }

        public bool IsSupportVerticalMV()
        {
            return ((byte)_supportVariableMv & 1) == 1;
        }

        public bool IsSupportOnly2D()
        {
            return ((byte)_supportVariableMv & 4) == 4;
        }

        public bool IsSupportMovie()
        {
            return ((byte)_supportVariableMv & 8) == 8;
        }
    }
    
    public enum eVariableMvType
    {
        Vertical = 1,
        Rich2D = 2,
        Only2D = 4,
        Movie = 8
    }

    private Dictionary<int, LiveData> liveDataDic = new Dictionary<int, LiveData>();

    public void Load(ref DBProxy masterDB)
    {
        using (Query query = masterDB.Query(selectallsql))
        {
            while (query.Step())
            {
                int @int = query.GetInt(0);
                int int2 = query.GetInt(1);
                int int3 = query.GetInt(2);
                int int4 = query.GetInt(3);
                int int5 = query.GetInt(4);
                int int6 = query.GetInt(5);
                int int7 = query.GetInt(6);
                int int8 = query.GetInt(7);
                int int9 = query.GetInt(8);
                int int10 = query.GetInt(9);
                int int11 = query.GetInt(10);
                string text = query.GetText(11);
                int int12 = query.GetInt(12);
                int int13 = query.GetInt(13);
                int int14 = query.GetInt(14);
                int int15 = query.GetInt(15);
                int int16 = query.GetInt(16);
                string text2 = query.GetText(17);
                string text3 = query.GetText(18);
                int int17 = query.GetInt(19);
                int int18 = query.GetInt(20);
                int int19 = query.GetInt(21);
                int int20 = query.GetInt(22);
                byte supportVariableMv = (byte)query.GetInt(23);
                int int21 = query.GetInt(24);
                int int22 = query.GetInt(25);
                int int23 = query.GetInt(26);
                int int24 = query.GetInt(27);
                int int25 = query.GetInt(28);
                int key = @int;
                LiveData value = new LiveData(@int, int2, int3, int4, int5, int6, int7, int8, int9, int10, int11, text, int12, int13, int14, int15, int16, text2, text3, int17, int18, int19, int20, supportVariableMv, int21, int22, int23, int24, int25);
                if (!liveDataDic.ContainsKey(key))
                {
                    liveDataDic.Add(key, value);
                }
            }
        }
    }
    
    public LiveData Get(int id)
    {
        if (liveDataDic.TryGetValue(id, out var value))
        {
            return value;
        }
        return null;
    }

    public LiveData GetFromMusicID(int music_id)
    {
        foreach(var tmp in liveDataDic)
        {
            if(tmp.Value.musicDataId == music_id)
            {
                return tmp.Value;
            }
        }
        return null;
    }
}