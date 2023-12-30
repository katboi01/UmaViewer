using Sqlite3Plugin;
using System.Collections.Generic;

public class MasterDressColorData
{
    /// <summary>
    /// 全ての要素をSelectするSQL文
    /// </summary>
    private static string selectallsql = "SELECT `chara_id`,`dress_id`,`model_type`,`color_id` FROM `dress_color_data`;";

    public class DressColorData
    {
        protected int _charaId;

        protected int _dressId;

        protected byte _modelType;

        protected int _colorId;

        public int charaId => _charaId;

        public int dressId => _dressId;

        public byte modelType => _modelType;

        public int colorId => _colorId;

        public DressColorData(string[] record)
        {
            _charaId = int.Parse(record[0]);
            _dressId = int.Parse(record[1]);
            _modelType = byte.Parse(record[2]);
            _colorId = int.Parse(record[3]);
        }

        public DressColorData(int charaId = 0, int dressId = 0, byte modelType = 0, int colorId = 0)
        {
            _charaId = charaId;
            _dressId = dressId;
            _modelType = modelType;
            _colorId = colorId;
        }
    }

    private Dictionary<int, DressColorData> dressColorDataDic = new Dictionary<int, DressColorData>();

    public void Load(ref DBProxy masterDB)
    {
        using (Query query = masterDB.Query(selectallsql))
        {
            while (query.Step())
            {
                int @int = query.GetInt(0);
                int int2 = query.GetInt(1);
                byte @byte = (byte)query.GetInt(2);
                int int4 = query.GetInt(3);

                int key = @int;
                DressColorData value = new DressColorData(@int, int2, @byte, int4);
                if (!dressColorDataDic.ContainsKey(key))
                {
                    dressColorDataDic.Add(key, value);
                }
            }
        }
    }


    public List<DressColorData> GetListWithCharaIdAndModelTypeOrderByDressIdAsc(int charaId, byte modelType)
    {
        Dictionary<ulong, List<DressColorData>> _dictionaryWithCharaIdAndModelType = new Dictionary<ulong, List<DressColorData>>();

        ulong key = (uint)charaId | ((ulong)modelType << 32);
        if (!_dictionaryWithCharaIdAndModelType.ContainsKey(key))
        {
            List<DressColorData> list = new List<DressColorData>();
            foreach (KeyValuePair<int, DressColorData> item in dressColorDataDic)
            {
                if (item.Value.charaId == charaId && item.Value.modelType == modelType)
                {
                    list.Add(item.Value);
                }
            }
            list.Sort((DressColorData x, DressColorData y) => x.dressId - y.dressId);
            _dictionaryWithCharaIdAndModelType.Add(key, list);
        }
        return _dictionaryWithCharaIdAndModelType[key];
    }


    public int GetDressColor(int charaID, int dressID)
    {
        foreach (KeyValuePair<int, DressColorData> item in dressColorDataDic)
        {
            if (item.Value.charaId == charaID && (byte)item.Value.dressId == dressID)
            {
                return item.Value.colorId;
            }
        }
        return 0;
    }
}