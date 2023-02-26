using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sqlite3Plugin;

public class MasterCardGalleryMotion
{
    /// <summary>
    /// 全ての要素をSelectするSQL文
    /// </summary>
    private static string selectallsql = "SELECT `id`,`motion_1`,`motion_2`,`motion_3`,`motion_4`,`motion_5`,`motion_6`,`motion_7`,`motion_8`,`motion_9`,`motion_10` FROM `card_gallery_motion`;";

    public class CardGalleryMotion
    {
        protected int _id;

        protected string _motion1;

        protected string _motion2;

        protected string _motion3;

        protected string _motion4;

        protected string _motion5;

        protected string _motion6;

        protected string _motion7;

        protected string _motion8;

        protected string _motion9;

        protected string _motion10;

        public int id => _id;

        public string motion1 => _motion1;

        public string motion2 => _motion2;

        public string motion3 => _motion3;

        public string motion4 => _motion4;

        public string motion5 => _motion5;

        public string motion6 => _motion6;

        public string motion7 => _motion7;

        public string motion8 => _motion8;

        public string motion9 => _motion9;

        public string motion10 => _motion10;

        public CardGalleryMotion(string[] record)
        {
            _id = int.Parse(record[0]);
            _motion1 = record[1];
            _motion2 = record[2];
            _motion3 = record[3];
            _motion4 = record[4];
            _motion5 = record[5];
            _motion6 = record[6];
            _motion7 = record[7];
            _motion8 = record[8];
            _motion9 = record[9];
            _motion10 = record[10];
        }

        public CardGalleryMotion(int id = 0, string motion1 = "", string motion2 = "", string motion3 = "", string motion4 = "", string motion5 = "", string motion6 = "", string motion7 = "", string motion8 = "", string motion9 = "", string motion10 = "")
        {
            _id = id;
            _motion1 = motion1;
            _motion2 = motion2;
            _motion3 = motion3;
            _motion4 = motion4;
            _motion5 = motion5;
            _motion6 = motion6;
            _motion7 = motion7;
            _motion8 = motion8;
            _motion9 = motion9;
            _motion10 = motion10;
        }
    }

    public Dictionary<int, CardGalleryMotion> dictionary = new Dictionary<int, CardGalleryMotion>();

    public void Load(ref DBProxy masterDB)
    {
        using (Query query = masterDB.Query(selectallsql))
        {
            while (query.Step())
            {
                int @int = query.GetInt(0);
                string text = query.GetText(1);
                string text2 = query.GetText(2);
                string text3 = query.GetText(3);
                string text4 = query.GetText(4);
                string text5 = query.GetText(5);
                string text6 = query.GetText(6);
                string text7 = query.GetText(7);
                string text8 = query.GetText(8);
                string text9 = query.GetText(9);
                string text10 = query.GetText(10);
                int key = @int;

                CardGalleryMotion value = new CardGalleryMotion(@int, text, text2, text3, text4, text5, text6, text7, text8, text9, text10);
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, value);
                }
            }
        }
    }

    public CardGalleryMotion Get(int id)
    {
        if (dictionary.TryGetValue(id, out var value))
        {
            return value;
        }
        else
        {
            return null;
        }
    }
}
