namespace Cutt
{
    public class SweatLocatorUpdateInfo
    {
        public eSweatLocatorOwner owner;

        public float alpha;

        public LiveTimelineKeySweatLocatorData.LocatorInfo[] locatorInfo;

        public SweatLocatorUpdateInfo(int arrayNum)
        {
            owner = eSweatLocatorOwner.Center;
            locatorInfo = new LiveTimelineKeySweatLocatorData.LocatorInfo[arrayNum];
        }
    }
}