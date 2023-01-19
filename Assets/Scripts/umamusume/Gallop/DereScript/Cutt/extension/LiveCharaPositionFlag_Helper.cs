namespace Cutt
{
    public static class LiveCharaPositionFlag_Helper
    {
        public static LiveCharaPositionFlag Default5 => LiveCharaPositionFlag.Center | LiveCharaPositionFlag.Left1 | LiveCharaPositionFlag.Right1 | LiveCharaPositionFlag.Left2 | LiveCharaPositionFlag.Right2;

        public static LiveCharaPositionFlag Everyone => LiveCharaPositionFlag.All;

        public static bool hasFlag(this LiveCharaPositionFlag This, LiveCharaPosition pos)
        {
            return This.hasFlag((LiveCharaPositionFlag)(1 << (int)pos));
        }

        public static bool hasFlag(this LiveCharaPositionFlag This, int bit)
        {
            return This.hasFlag((LiveCharaPositionFlag)bit);
        }

        public static bool hasFlag(this LiveCharaPositionFlag This, LiveCharaPositionFlag bit)
        {
            return (This & bit) != 0;
        }
    }

}
