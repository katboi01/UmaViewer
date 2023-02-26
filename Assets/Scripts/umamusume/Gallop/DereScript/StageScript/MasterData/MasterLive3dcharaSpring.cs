using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sqlite3Plugin;

public class MasterLive3dcharaSpring
{
    /// <summary>
    /// 全ての要素をSelectするSQL文
    /// </summary>
    private static string selectallsql = "SELECT `music_id`,`chara_id`,`dress_id`,`head_ratio`,`body_ratio` FROM `live_3dchara_spring`;";

    public class Live3dcharaSpring
    {
        protected int _musicId;

        protected int _charaId;

        protected int _dressId;

        protected int _headRatio;

        protected int _bodyRatio;

        public int musicId => _musicId;

        public int charaId => _charaId;

        public int dressId => _dressId;

        public int headRatio => _headRatio;

        public int bodyRatio => _bodyRatio;

        public Live3dcharaSpring(string[] record)
        {
            _musicId = int.Parse(record[0]);
            _charaId = int.Parse(record[1]);
            _dressId = int.Parse(record[2]);
            _headRatio = int.Parse(record[3]);
            _bodyRatio = int.Parse(record[4]);
        }

        public Live3dcharaSpring(int musicId = 0, int charaId = 0, int dressId = 0, int headRatio = 0, int bodyRatio = 0)
        {
            _musicId = musicId;
            _charaId = charaId;
            _dressId = dressId;
            _headRatio = headRatio;
            _bodyRatio = bodyRatio;
        }
    }

    private Dictionary<int, Live3dcharaSpring> live3dcharaSpringDic = new Dictionary<int, Live3dcharaSpring>();

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

                int key = @int;
                Live3dcharaSpring value = new Live3dcharaSpring(@int, int2, int3, int4, int5);
                if (!live3dcharaSpringDic.ContainsKey(key))
                {
                    live3dcharaSpringDic.Add(key, value);
                }
            }
        }
    }

    public List<Live3dcharaSpring> GetListWithMusicIdAndCharaIdOrderByDressIdAsc(int musicId, int charaId)
    {
        Dictionary<ulong, List<Live3dcharaSpring>> _dictionaryWithMusicIdAndCharaId = new Dictionary<ulong, List<Live3dcharaSpring>>();
        ulong key = (uint)musicId | ((ulong)(uint)charaId << 32);
        if (!_dictionaryWithMusicIdAndCharaId.ContainsKey(key))
        {
            List<Live3dcharaSpring> list = new List<Live3dcharaSpring>();
            foreach (KeyValuePair<int, Live3dcharaSpring> item in live3dcharaSpringDic)
            {
                if ((int)item.Value.musicId == musicId && (int)item.Value.charaId == charaId)
                {
                    list.Add(item.Value);
                }
            }
            list.Sort((Live3dcharaSpring x, Live3dcharaSpring y) => (int)x.dressId - (int)y.dressId);
            _dictionaryWithMusicIdAndCharaId.Add(key, list);
        }
        return _dictionaryWithMusicIdAndCharaId[key];
    }
}