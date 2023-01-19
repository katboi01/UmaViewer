namespace Cutt
{
    internal static class eEffectOwnerUtil
    {
        public static int ToCharaPosIndex(eEffectOwner owner)
        {
            if (eEffectOwner.World == owner)
            {
                return 0;
            }
            if (owner < eEffectOwner.World)
            {
                return (int)owner;
            }
            return (int)(owner - 1);
        }

        public static eEffectOwner FromLiveCharaPosition(LiveCharaPosition charaPos)
        {
            if (charaPos <= LiveCharaPosition.Right2)
            {
                return (eEffectOwner)charaPos;
            }
            return (eEffectOwner)(charaPos + 1);
        }
    }
}
