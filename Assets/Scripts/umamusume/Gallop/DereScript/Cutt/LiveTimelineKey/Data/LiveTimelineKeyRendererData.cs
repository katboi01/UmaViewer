using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyRendererData : LiveTimelineKeyWithInterpolate
    {
        public bool renderEnable = true;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Renderer;
    }
}
