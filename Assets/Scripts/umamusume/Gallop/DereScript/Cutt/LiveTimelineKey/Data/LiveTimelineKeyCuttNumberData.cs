using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCuttNumberData : LiveTimelineKey
    {
        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CuttNumber;
    }
}
