using Stage;

public class CuttDressVmRun : ConditionVmRun
{
    private Character3DBase.CharacterData _characterData;

    private int _position_id;

    public CuttDressVmRun(ConditionVmData vmdata, ref Character3DBase.CharacterData characterData, int position_id)
        : base(vmdata)
    {
        _characterData = characterData;
        _position_id = position_id;
    }

    public int[] RunDressChange(string name)
    {
        int conditionIndex = GetConditionIndex(name);
        if (0 <= conditionIndex)
        {
            ConditionVmData.VmCodeSet[] vmCodeSet = _data.conditions[conditionIndex].vmCodeSet;
            return RunVmAndAllResults(vmCodeSet);
        }
        return null;
    }

    public override int GetPositionAndStatus(ConditionVmData.POSITION position, ConditionVmData.STATUS status)
    {
        int num = 0;
        if (status == ConditionVmData.STATUS.POSITION)
        {
            return (_position_id == (int)position) ? 1 : 0;
        }
        return base.GetPositionAndStatus(position, status);
    }

    public override int GetSystemValue(ConditionVmData.SYSTEMVALUE systemvalue)
    {
        return systemvalue switch
        {
            ConditionVmData.SYSTEMVALUE.chara_id => _characterData.charaId,
            ConditionVmData.SYSTEMVALUE.dress_id => _characterData.changeDressId,
            _ => base.GetSystemValue(systemvalue),
        };
    }
}
