namespace Cutt
{
    public static class LiveTimelineKeyAttribute_Ext
    {
        public static bool hasFlag(this LiveTimelineKeyAttribute This, LiveTimelineKeyAttribute bit)
        {
            return (This & bit) != 0;
        }

        public static LiveTimelineKeyAttribute addFlag(this LiveTimelineKeyAttribute This, LiveTimelineKeyAttribute bit, bool on)
        {
            if (on)
            {
                return This | bit;
            }
            return This & ~bit;
        }

        public static int GetHighWord(this LiveTimelineKeyAttribute This)
        {
            return (int)This >> 16;
        }

        public static LiveTimelineKeyAttribute SetHighWord(this LiveTimelineKeyAttribute This, int val)
        {
            LiveTimelineKeyAttribute liveTimelineKeyAttribute = (LiveTimelineKeyAttribute)(val << 16);
            return liveTimelineKeyAttribute | (This & LiveTimelineKeyAttribute.KeyCommonBitMask);
        }
    }
}
