using System;

[Serializable]
public class LiveTimelinePropsSettings
{
    public enum PropsConditionType
    {
        Default,
        CharaAttribute,
        CharaPosition,
        CharaID,
        CardID,
        DressType,
        DressID
    }

    public enum PropsAttribute
    {
        Cute,
        Cool,
        Passion
    }

    public enum AlterPropsMode
    {
        None,
        LeftHanded
    }

    [Serializable]
    public class PropsConditionData
    {
        public PropsConditionType Type;

        public int Value;
    }

    [Serializable]
    public class PropsConditionGroup
    {
        public PropsConditionData[] propsConditionData = new PropsConditionData[0];

        public int propsConditionCount;

        public string propsName = "";

        public bool random;

        public string[] randomNameArray;

        public bool satisfiesAllConditions;

        public float bodyScaleS = 1f;

        public float bodyScaleM = 1f;

        public float bodyScaleL = 1f;

        public float bodyScaleLL = 1f;

        public AlterPropsMode alterPropsMode;
    }

    [Serializable]
    public class PropsDataGroup
    {
        public PropsConditionGroup[] propsConditionGroup = new PropsConditionGroup[0];

        public int propsConditionGroupCount;
    }

    public PropsDataGroup[] propsDataGroup = new PropsDataGroup[0];

    public int propsDataGroupCount;
}
