namespace Cutt
{
    public interface ILiveTimelineSetData
    {
        ILiveTimelineKeyDataList GetKeyList(int index);

        ILiveTimelineKeyDataList[] GetKeyListArray();

        LiveTimelineKeyDataType[] GetKeyTypeArray();
    }
}
