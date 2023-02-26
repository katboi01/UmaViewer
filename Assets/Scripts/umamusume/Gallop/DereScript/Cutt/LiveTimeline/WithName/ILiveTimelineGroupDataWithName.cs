namespace Cutt
{
    public class ILiveTimelineGroupDataWithName : ILiveTimelineGroupData
    {
        public string name;

        private int _nameHash;

        public int nameHash => _nameHash;

        public override void UpdateStatus()
        {
            base.UpdateStatus();
            _nameHash = FNVHash.Generate(name);
        }

        public ILiveTimelineGroupDataWithName()
        {
            name = "";
            _nameHash = 0;
        }

        public ILiveTimelineGroupDataWithName(string default_name)
        {
            name = default_name;
            _nameHash = FNVHash.Generate(name);
        }
    }
}
