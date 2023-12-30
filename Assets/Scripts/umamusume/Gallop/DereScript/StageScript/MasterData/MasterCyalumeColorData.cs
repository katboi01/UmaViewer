using Cute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    public class MasterCyalumeColorData : AbstractMasterData
    {
        private enum CsvLabel
        {
            ColorName,
            InColorR,
            InColorG,
            InColorB,
            OutColorR,
            OutColorG,
            OutColorB
        }

        public struct CyalumeColorData
        {
            public Color _inColor;

            public Color _outColor;

            public CyalumeColorData(string[] record)
            {
                this._inColor.r = int.Parse(record[1]) / 255f;
                this._inColor.g = int.Parse(record[2]) / 255f;
                this._inColor.b = int.Parse(record[3]) / 255f;
                this._inColor.a = 1f;
                this._outColor.r = int.Parse(record[4]) / 255f;
                this._outColor.g = int.Parse(record[5]) / 255f;
                this._outColor.b = int.Parse(record[6]) / 255f;
                this._outColor.a = 1f;
            }

            public CyalumeColorData(string name, Color incolor, Color outcolor)
            {
                this._inColor = incolor;
                this._outColor = outcolor;
            }
        }

        public static MasterCyalumeColorData.CyalumeColorData _defaultColor = new MasterCyalumeColorData.CyalumeColorData("Default Color", Color.white, Color.white);

        private Dictionary<int, MasterCyalumeColorData.CyalumeColorData> _cyalumeColorDictionary = new Dictionary<int, MasterCyalumeColorData.CyalumeColorData>();

        public Dictionary<int, MasterCyalumeColorData.CyalumeColorData> dictionary
        {
            get
            {
                return this._cyalumeColorDictionary;
            }
        }
        /*
        public MasterCyalumeColorData(MasterLiveDatabase db) : base(db)
        {
            throw new Exception("Schemaless tables cannot be constructed with DB. Use CreateFromSchemalessTable factory method instead");
        }
        */

        public MasterCyalumeColorData(ArrayList list) : base(list)
        {
            if (list.Count == 0)
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                ArrayList arrayList = list[i] as ArrayList;
                string[] array = arrayList.ToArray(typeof(string)) as string[];
                MasterCyalumeColorData.CyalumeColorData value = new MasterCyalumeColorData.CyalumeColorData(array);
                int hashCode = array[0].GetHashCode();
                if (!this._cyalumeColorDictionary.ContainsKey(hashCode))
                {
                    this._cyalumeColorDictionary.Add(hashCode, value);
                }
            }
        }

        public static MasterCyalumeColorData CreateFromSchemalessTable(string csvString)
        {
            MasterCyalumeColorData result = null;

            /*
            using (Query query = db.Query("SELECT `raw` FROM `cyalume_color`"))
            {
                if (query.Step())
                {
                    string text = query.GetText(0);
                    ArrayList list = Utility.ConvertCSV(text, true);
                    result = new MasterCyalumeColorData(list);
                }
            }
            */
            ArrayList list = Utility.ConvertCSV(csvString, true);
            result = new MasterCyalumeColorData(list);

            return result;
        }

        public override void Unload()
        {
        }
    }
}
