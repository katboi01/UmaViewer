// CuttMotionVmRun
using Stage;

public class CuttMotionVmRun : ConditionVmRun
{
    private Character3DBase.CharacterData[] _characterData;

    public CuttMotionVmRun(ConditionVmData vmdata, ref Character3DBase.CharacterData[] characterData)
        : base(vmdata)
    {
        _characterData = characterData;
    }

    public override int CheckPositionAndChrFlags(ConditionVmData.POSITION position, ConditionVmData.CHR_FLAGS chr_flags)
    {
        bool flag = false;
        int characterIdFromPosition = GetCharacterIdFromPosition(position);
        if (characterIdFromPosition >= 0 && chr_flags == ConditionVmData.CHR_FLAGS.LEFTY)
        {
            flag = (int)GetCharaMaster(characterIdFromPosition).hand == 3002;
        }
        if (!flag)
        {
            return 0;
        }
        return 1;
    }

    public override int CheckPositionAndCardFlags(ConditionVmData.POSITION position, ConditionVmData.CARD_FLAGS card_flags)
    {
        bool flag = false;
        int cardIdFromPosition = GetCardIdFromPosition(position);
        if (cardIdFromPosition >= 0 && card_flags == ConditionVmData.CARD_FLAGS.SSR)
        {
            flag = (int)GetCardMaster(cardIdFromPosition).rarity == 7;
        }
        if (!flag)
        {
            return 0;
        }
        return 1;
    }

    public override int CheckPositionAndChrId(ConditionVmData.POSITION position, int chara_id)
    {
        if (chara_id != GetCharacterIdFromPosition(position))
        {
            return 0;
        }
        return 1;
    }

    public override int GetPositionAndStatus(ConditionVmData.POSITION position, ConditionVmData.STATUS status)
    {
        int result = 0;
        switch (status)
        {
            case ConditionVmData.STATUS.DRESS:
                if ((int)position < _characterData.Length && _characterData[(int)position] != null)
                {
                    result = _characterData[(int)position].activeDressId;
                }
                break;
            case ConditionVmData.STATUS.HEIGHT:
                {
                    int characterIdFromPosition = GetCharacterIdFromPosition(position);
                    if (characterIdFromPosition >= 0)
                    {
                        result = GetCharaMaster(characterIdFromPosition).height;
                    }
                    break;
                }
            default:
                return base.GetPositionAndStatus(position, status);
        }
        return result;
    }

    public override int CheckExistsChrId(int chara_id)
    {
        bool flag = false;
        int num = _characterData.Length;
        for (int i = 0; i < num; i++)
        {
            if (_characterData[i] != null && _characterData[i].charaId == chara_id)
            {
                flag = true;
                break;
            }
        }
        if (!flag)
        {
            return 0;
        }
        return 1;
    }

    private int GetCharacterIdFromPosition(ConditionVmData.POSITION position, int unitIndex = 0)
    {
        int result = -1;
        if ((int)position < _characterData.Length && _characterData[(int)position] != null)
        {
            result = _characterData[(int)position].charaId;
        }
        return result;
    }

    private int GetCardIdFromPosition(ConditionVmData.POSITION position, int unitIndex = 0)
    {
        int result = -1;
        if ((int)position < _characterData.Length && _characterData[(int)position] != null)
        {
            result = _characterData[(int)position].cardId;
        }
        return result;
    }

    private MasterCharaData.CharaData GetCharaMaster(int chara_id)
    {
        return MasterDBManager.instance.masterCharaData.Get(chara_id);
    }

    private MasterCardData.CardData GetCardMaster(int chara_id)
    {
        return MasterDBManager.instance.masterCardData.Get(chara_id);
    }
}
