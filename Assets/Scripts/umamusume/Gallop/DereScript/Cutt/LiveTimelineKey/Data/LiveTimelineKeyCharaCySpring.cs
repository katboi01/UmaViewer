using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCharaCySpring : LiveTimelineKey
    {
        public enum ParameterType
        {
            Reset,
            Soft,
            Usually,
            Hard,
            SuperHard,
            Suppression
        }

        private const float DefaultAccSitffness = 0.03f;

        public LiveCharaPositionFlag _resetCySpringFlag;

        public LiveCharaPositionFlag _overrideParameterCySpringFlag;

        public ParameterType[] _overrideParameterType;

        public bool[] _overrideAllBone;

        public bool[] _overrideAccBone;

        public float[] _overrideAccStiffness;

        public bool[] _overrideFurisodeBone;

        public float[] _overrideFurisodeStiffness;

        [NonSerialized]
        public bool _isExecute;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CharaCySpring;

        public void AddResetCharaPositionFlag(LiveCharaPositionFlag flag)
        {
            _resetCySpringFlag |= flag;
        }

        public void RemoveResetCharaPositionFlag(LiveCharaPositionFlag flag)
        {
            _resetCySpringFlag &= ~flag;
        }

        public bool IsResetCharaPositionFlag(LiveCharaPositionFlag flag)
        {
            return (_resetCySpringFlag & flag) != 0;
        }

        public bool IsResetCharaFlag(LiveCharaPosition flag)
        {
            LiveCharaPositionFlag flag2 = (LiveCharaPositionFlag)(1 << (int)flag);
            return IsResetCharaPositionFlag(flag2);
        }

        public void SetResetCharaPositionFlag(LiveCharaPositionFlag flag, bool enable)
        {
            if (enable)
            {
                AddResetCharaPositionFlag(flag);
            }
            else
            {
                RemoveResetCharaPositionFlag(flag);
            }
        }

        public void AddOverrideParameterCharaPositionFlag(LiveCharaPositionFlag flag)
        {
            _overrideParameterCySpringFlag |= flag;
        }

        public void RemoveOverrideParameterCharaPositionFlag(LiveCharaPositionFlag flag)
        {
            _overrideParameterCySpringFlag &= ~flag;
        }

        public bool IsOverrideParameterCharaPositionFlag(LiveCharaPositionFlag flag)
        {
            return (_overrideParameterCySpringFlag & flag) != 0;
        }

        public bool IsOverrideParameterCharaFlag(LiveCharaPosition flag)
        {
            LiveCharaPositionFlag flag2 = (LiveCharaPositionFlag)(1 << (int)flag);
            return IsOverrideParameterCharaPositionFlag(flag2);
        }

        public void SetOverrideParameterCharaPositionFlag(LiveCharaPositionFlag flag, bool enable)
        {
            if (enable)
            {
                AddOverrideParameterCharaPositionFlag(flag);
            }
            else
            {
                RemoveOverrideParameterCharaPositionFlag(flag);
            }
        }

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
            if (_overrideParameterType == null || _overrideParameterType.Length < 15)
            {
                _overrideParameterType = new ParameterType[15];
            }
            if (_overrideAllBone == null || _overrideAllBone.Length < 15)
            {
                _overrideAllBone = new bool[15];
            }
            if (_overrideAccBone == null || _overrideAccBone.Length < 15)
            {
                _overrideAccBone = new bool[15];
            }
            if (_overrideAccStiffness == null || _overrideAccStiffness.Length < 15)
            {
                _overrideAccStiffness = new float[15];
                for (int i = 0; i < _overrideAccStiffness.Length; i++)
                {
                    _overrideAccStiffness[i] = 0.03f;
                }
            }
            if (_overrideFurisodeBone == null || _overrideFurisodeBone.Length < 15)
            {
                _overrideFurisodeBone = new bool[15];
            }
            if (_overrideFurisodeStiffness == null || _overrideFurisodeStiffness.Length < 15)
            {
                _overrideFurisodeStiffness = new float[15];
                for (int j = 0; j < _overrideFurisodeStiffness.Length; j++)
                {
                    _overrideFurisodeStiffness[j] = 0.03f;
                }
            }
        }
    }
}
