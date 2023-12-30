using Sqlite3Plugin;
using System.Collections.Generic;

public class MasterDressData
{
    /// <summary>
    /// 全ての要素をSelectするSQL文
    /// </summary>
    private static string selectallsql = "SELECT `id`,`name`,`description`,`open_type`,`dress_type` FROM `dress_data`;";

    public class DressData
    {
        protected int _id;

        protected string _name;

        protected string _description;

        protected int _openType;

        protected int _dressType;

        public int id => _id;

        public string name => _name;

        public string description => _description;

        public int openType => _openType;

        public int dressType => _dressType;

        public DressData(string[] record)
        {
            _id = int.Parse(record[0]);
            _name = record[1];
            _description = record[2];
            _openType = int.Parse(record[3]);
            _dressType = int.Parse(record[4]);
        }

        public DressData(int id = 0, string name = "", string description = "", int openType = 0, int dressType = 0)
        {
            _id = id;
            _name = name;
            _description = description;
            _openType = openType;
            _dressType = dressType;
        }
    }


    private Dictionary<int, DressData> dressDataDic = new Dictionary<int, DressData>();

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
                int key = @int;
                DressData value = new DressData(@int, text, text2, int2, int3);
                if (!dressDataDic.ContainsKey(key))
                {
                    dressDataDic.Add(key, value);
                }
            }
        }
    }

    public DressData Get(int id)
    {
        if (!dressDataDic.TryGetValue(id, out var value))
        {
            return value;
        }
        return null;
    }
}