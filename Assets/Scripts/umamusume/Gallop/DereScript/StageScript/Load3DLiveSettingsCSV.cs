using Cute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Load3DLiveSettingsCSV
{
    public List<Master3dLive.Live3dData> _infos = new List<Master3dLive.Live3dData>();

    public void Load(string csvPath)
    {
        TextAsset textAsset = ResourcesManager.instance.LoadObject(csvPath) as TextAsset;
        Parse(textAsset.ToString());
    }

    public void Parse(string text)
    {
        ArrayList arrayList = Utility.ConvertCSV(text);
        _infos = new List<Master3dLive.Live3dData>();
        for (int i = 0; i < arrayList.Count; i++)
        {
            ArrayList arrayList2 = (ArrayList)arrayList[i];
            string[] columns = arrayList2.ToArray(typeof(string)) as string[];
            Master3dLive.Live3dData item = new Master3dLive.Live3dData(columns);
            _infos.Add(item);
        }
    }
}
