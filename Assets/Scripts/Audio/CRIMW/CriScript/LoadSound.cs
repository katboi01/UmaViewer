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
        info.acbPath = Main.AbSounds.FirstOrDefault(a => a.Name.Contains(name) && a.Name.EndsWith("acb")).FilePath;
        info.awbPath = Main.AbSounds.FirstOrDefault(a => a.Name.Contains(name) && a.Name.EndsWith("awb")).FilePath;
        return info;
    }


}
