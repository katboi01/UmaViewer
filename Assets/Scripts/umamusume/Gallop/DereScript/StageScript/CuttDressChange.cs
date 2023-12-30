using Cutt;
using Stage;
using System.Collections.Generic;
using System.Linq;

public class CuttDressChange
{
    private const int ReferenceMotSeqSheet = 0;

    private List<LiveTimelineWorkSheet> _sheetList;

    private CuttConditionOption _conditionOption;

    private Character3DBase.CharacterData[] _characterData;

    private CuttDressVmRun _vmRunCondition;

    private CuttDressVmRun _vmRunExecution;

    public CuttDressChange(CuttConditionOption conditionOption)
    {
        _conditionOption = conditionOption;
    }

    public int[] CheckAndRun(Character3DBase.CharacterData data, int position_id)
    {
        int[] result = null;
        if (_conditionOption == null || _conditionOption.charaDressCondition == null || _conditionOption.charaDressExecution == null)
        {
            return result;
        }
        _vmRunCondition = new CuttDressVmRun(_conditionOption.charaDressCondition, ref data, position_id);
        _vmRunExecution = new CuttDressVmRun(_conditionOption.charaDressExecution, ref data, position_id);
        _ = _vmRunCondition.ConditionNames;
        _vmRunCondition.ResultUpdate();
        foreach (KeyValuePair<string, int> item in _vmRunCondition.GetResultAll())
        {
            if (item.Value != 0)
            {
                string key = item.Key;
                result = _vmRunExecution.RunDressChange(key);
                string.Join(", ", result.Select((int i) => i.ToString()));
                return result;
            }
        }
        return result;
    }

    public void WriteLogText()
    {
    }
}
