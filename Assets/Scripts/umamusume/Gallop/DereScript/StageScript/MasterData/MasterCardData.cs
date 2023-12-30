using Sqlite3Plugin;
using System.Collections.Generic;

public class MasterCardData
{
    /// <summary>
    /// 全ての要素をSelectするSQL文
    /// </summary>
    private static string selectallsql = "SELECT `id`,`name`,`chara_id`,`rarity`,`attribute`,`title_flag`,`evolution_id`,`series_id`,`pose`,`place`,`evolution_type`,`album_id`,`open_story_id`,`open_dress_id`,`skill_id`,`leader_skill_id`,`hp_min`,`vocal_min`,`dance_min`,`visual_min`,`hp_max`,`vocal_max`,`dance_max`,`visual_max`,`bonus_hp`,`bonus_vocal`,`bonus_dance`,`bonus_visual`,`solo_live`,`star_lesson_type`,`disp_order`,`voice_flag` FROM `card_data`;";

    public class CardData
    {
        protected int _id;

        protected string _name;

        protected int _charaId;

        protected int _rarity;

        protected int _attribute;

        protected bool _titleFlag;

        protected int _evolutionId;

        protected int _seriesId;

        protected int _pose;

        protected int _place;

        protected int _evolutionType;

        protected int _albumId;

        protected int _openStoryId;

        protected int _openDressId;

        protected int _liveSkillId;

        protected int _leaderSkillId;

        protected int _hpMin;

        protected int _vocalMin;

        protected int _danceMin;

        protected int _visualMin;

        protected int _hpMax;

        protected int _vocalMax;

        protected int _danceMax;

        protected int _visualMax;

        protected int _bonusHp;

        protected int _bonusVocal;

        protected int _bonusDance;

        protected int _bonusVisual;

        protected int _soloValue;

        protected int _starLessonType;

        protected int _dispOrder;

        protected int _voiceFlag;

        public int id => _id;

        public string name => _name;

        public int charaId => _charaId;

        public int rarity => _rarity;

        public int attribute => _attribute;

        public bool titleFlag => _titleFlag;

        public int evolutionId => _evolutionId;

        public int seriesId => _seriesId;

        public int pose => _pose;

        public int place => _place;

        public int evolutionType => _evolutionType;

        public int albumId => _albumId;

        public int openStoryId => _openStoryId;

        public int openDressId => _openDressId;

        public int liveSkillId => _liveSkillId;

        public int leaderSkillId => _leaderSkillId;

        public int hpMin => _hpMin;

        public int vocalMin => _vocalMin;

        public int danceMin => _danceMin;

        public int visualMin => _visualMin;

        public int hpMax => _hpMax;

        public int vocalMax => _vocalMax;

        public int danceMax => _danceMax;

        public int visualMax => _visualMax;

        public int bonusHp => _bonusHp;

        public int bonusVocal => _bonusVocal;

        public int bonusDance => _bonusDance;

        public int bonusVisual => _bonusVisual;

        public int soloValue => _soloValue;

        public int starLessonType => _starLessonType;

        public int dispOrder => _dispOrder;

        public int voiceFlag => _voiceFlag;

        public CardData(string[] record)
        {
            _id = int.Parse(record[0]);
            _name = record[1];
            _charaId = int.Parse(record[2]);
            _rarity = int.Parse(record[3]);
            _attribute = int.Parse(record[4]);
            _titleFlag = int.Parse(record[5]) == 1;
            _evolutionId = int.Parse(record[6]);
            _seriesId = int.Parse(record[7]);
            _pose = int.Parse(record[8]);
            _place = int.Parse(record[9]);
            _evolutionType = int.Parse(record[10]);
            _albumId = int.Parse(record[11]);
            _openStoryId = int.Parse(record[12]);
            _openDressId = int.Parse(record[13]);
            _liveSkillId = int.Parse(record[14]);
            _leaderSkillId = int.Parse(record[15]);
            _hpMin = int.Parse(record[17]);
            _vocalMin = int.Parse(record[18]);
            _danceMin = int.Parse(record[19]);
            _visualMin = int.Parse(record[20]);
            _hpMax = int.Parse(record[21]);
            _vocalMax = int.Parse(record[22]);
            _danceMax = int.Parse(record[23]);
            _visualMax = int.Parse(record[24]);
            _bonusHp = int.Parse(record[25]);
            _bonusVocal = int.Parse(record[26]);
            _bonusDance = int.Parse(record[27]);
            _bonusVisual = int.Parse(record[28]);
            _soloValue = int.Parse(record[29]);
            _starLessonType = int.Parse(record[30]);
            _dispOrder = int.Parse(record[31]);
            _voiceFlag = int.Parse(record[32]);
        }

        public CardData(int id = 0, string name = "", int charaId = 0, int rarity = 0, int attribute = 0, bool titleFlag = false, int evolutionId = 0, int seriesId = 0, int pose = 0, int place = 0, int evolutionType = 0, int albumId = 0, int openStoryId = 0, int openDressId = 0, int liveSkillId = 0, int leaderSkillId = 0, int hpMin = 0, int vocalMin = 0, int danceMin = 0, int visualMin = 0, int hpMax = 0, int vocalMax = 0, int danceMax = 0, int visualMax = 0, int bonusHp = 0, int bonusVocal = 0, int bonusDance = 0, int bonusVisual = 0, int soloValue = 0, int starLessonType = 0, int dispOrder = 0, int voiceFlag = 0)
        {
            _id = id;
            _name = name;
            _charaId = charaId;
            _rarity = rarity;
            _attribute = attribute;
            _titleFlag = titleFlag;
            _evolutionId = evolutionId;
            _seriesId = seriesId;
            _pose = pose;
            _place = place;
            _evolutionType = evolutionType;
            _albumId = albumId;
            _openStoryId = openStoryId;
            _openDressId = openDressId;
            _liveSkillId = liveSkillId;
            _leaderSkillId = leaderSkillId;
            _hpMin = hpMin;
            _vocalMin = vocalMin;
            _danceMin = danceMin;
            _visualMin = visualMin;
            _hpMax = hpMax;
            _vocalMax = vocalMax;
            _danceMax = danceMax;
            _visualMax = visualMax;
            _bonusHp = bonusHp;
            _bonusVocal = bonusVocal;
            _bonusDance = bonusDance;
            _bonusVisual = bonusVisual;
            _soloValue = soloValue;
            _starLessonType = starLessonType;
            _dispOrder = dispOrder;
            _voiceFlag = voiceFlag;
        }
    }

    private Dictionary<int, CardData> cardDataDic = new Dictionary<int, CardData>();

    public void Load(ref DBProxy masterDB)
    {
        using (Query query = masterDB.Query(selectallsql))
        {
            while (query.Step())
            {
                int @int = query.GetInt(0);
                string text = query.GetText(1);
                int int2 = query.GetInt(2);
                int int3 = query.GetInt(3);
                int int4 = query.GetInt(4);
                bool titleFlag = query.GetInt(5) == 1;
                int int5 = query.GetInt(6);
                int int6 = query.GetInt(7);
                int int7 = query.GetInt(8);
                int int8 = query.GetInt(9);
                int int9 = query.GetInt(10);
                int int10 = query.GetInt(11);
                int int11 = query.GetInt(12);
                int int12 = query.GetInt(13);
                int int13 = query.GetInt(14);
                int int14 = query.GetInt(15);
                int int15 = query.GetInt(16);
                int int16 = query.GetInt(17);
                int int17 = query.GetInt(18);
                int int18 = query.GetInt(19);
                int int19 = query.GetInt(20);
                int int20 = query.GetInt(21);
                int int21 = query.GetInt(22);
                int int22 = query.GetInt(23);
                int int23 = query.GetInt(24);
                int int24 = query.GetInt(25);
                int int25 = query.GetInt(26);
                int int26 = query.GetInt(27);
                int int27 = query.GetInt(28);
                int int28 = query.GetInt(29);
                int int29 = query.GetInt(30);
                int int30 = query.GetInt(31);
                int key = @int;
                CardData value = new CardData(@int, text, int2, int3, int4, titleFlag, int5, int6, int7, int8, int9, int10, int11, int12, int13, int14, int15, int16, int17, int18, int19, int20, int21, int22, int23, int24, int25, int26, int27, int28, int29, int30);
                if (!cardDataDic.ContainsKey(key))
                {
                    cardDataDic.Add(key, value);
                }
            }
        }
    }

    public CardData Get(int id)
    {
        if (cardDataDic.TryGetValue(id, out var value))
        {
            return value;
        }
        else
        {
            return null;
        }
    }
    public List<CardData> GetListWithCharaIdOrderByIdAsc(int charaId)
    {
        Dictionary<int, List<CardData>> _dictionaryWithCharaId = new Dictionary<int, List<CardData>>();
        List<CardData> list = new List<CardData>();
        foreach (KeyValuePair<int, CardData> item in cardDataDic)
        {
            if (item.Value.charaId == charaId)
            {
                list.Add(item.Value);
            }
        }
        list.Sort((CardData x, CardData y) => x.id - y.id);
        _dictionaryWithCharaId.Add(charaId, list);
        return _dictionaryWithCharaId[charaId];
    }

    public List<CardData> GetRarityCardList(int charaId, int rality)
    {
        List<CardData> list = new List<CardData>();
        List<CardData> listWithCharaIdOrderByIdAsc = GetListWithCharaIdOrderByIdAsc(charaId);
        listWithCharaIdOrderByIdAsc.Sort((CardData a, CardData b) => a.albumId - b.albumId);
        for (int i = 0; i < listWithCharaIdOrderByIdAsc.Count; i++)
        {
            if (listWithCharaIdOrderByIdAsc[i].rarity == rality)
            {
                list.Add(listWithCharaIdOrderByIdAsc[i]);
            }
        }
        return list;
    }
}
