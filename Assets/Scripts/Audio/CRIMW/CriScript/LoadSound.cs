using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadSound : ScriptableObject
{
    UmaViewerMain Main => UmaViewerMain.Instance;
    public struct UmaSoundInfo
    {
        public string acbPath;
        public string awbPath;
    }

    
    public UmaSoundInfo getSoundPath(string name) {
        UmaSoundInfo info;
        info.acbPath = UmaDatabaseController.GetABPath(Main.AbList.FirstOrDefault(a => a.Name.Contains(name) && a.Name.EndsWith("acb")));
        info.awbPath = UmaDatabaseController.GetABPath(Main.AbList.FirstOrDefault(a => a.Name.Contains(name) && a.Name.EndsWith("awb")));
        return info;
    }


}
