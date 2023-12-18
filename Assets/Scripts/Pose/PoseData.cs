using System.Collections.Generic;

[System.Serializable]
public class PoseData
{
    public string Name;
    public string Date;
    public string Character;
    public string Description;
    public string ViewerVersion;

    public SerializableBone Root;
    public List<SerializableBone> Bones = new List<SerializableBone>();
    public List<SerializableMorph> Morphs = new List<SerializableMorph>();
}