using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyDressChangeData : LiveTimelineKey
    {
        public enum eTarget
        {
            Center = 0,
            Left1 = 1,
            Right1 = 2,
            Left2 = 3,
            Right2 = 4,
            Left3 = 5,
            Right3 = 6,
            Left4 = 7,
            Right4 = 8,
            Left5 = 9,
            Right5 = 10,
            Left6 = 11,
            Right6 = 12,
            Left7 = 13,
            Right7 = 14,
            MAX = 0xF,
            INVALID = 0xF
        }

        public enum eTargetFlag
        {
            Center = 1,
            Left1 = 2,
            Right1 = 4,
            Left2 = 8,
            Right2 = 0x10,
            Left3 = 0x20,
            Right3 = 0x40,
            Left4 = 0x80,
            Right4 = 0x100,
            Left5 = 0x200,
            Right5 = 0x400,
            Left6 = 0x800,
            Right6 = 0x1000,
            Left7 = 0x2000,
            Right7 = 0x4000,
            MainUnit = 0x1F,
            ExtUnit = 32736,
            AllUnit = 0x7FFF
        }

        public enum eDressType
        {
            DressA,
            DressB,
            DressC
        }

        [SerializeField]
        private int _targetFlags;

        [SerializeField]
        private eDressType[] _dressType = new eDressType[15];

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.DressChange;

        public eTargetFlag targetFlags => (eTargetFlag)_targetFlags;

        public eDressType[] dressType => _dressType;

        protected int GetTargetFlag(eTarget target)
        {
            return 1 << (int)target;
        }

        public bool CheckTargetFlag(eTarget target)
        {
            int targetFlag = GetTargetFlag(target);
            return (_targetFlags & targetFlag) == targetFlag;
        }

        public void SetTargetFlag(eTarget target, bool sw)
        {
            int targetFlag = GetTargetFlag(target);
            _targetFlags |= targetFlag;
            if (!sw)
            {
                _targetFlags ^= targetFlag;
            }
        }

        public eDressType GetDressType(eTarget target)
        {
            return _dressType[(int)target];
        }

        public eDressType[] GetCloneDressTypeArray()
        {
            eDressType[] array = new eDressType[15];
            for (int i = 0; i < _dressType.Length; i++)
            {
                array[i] = _dressType[i];
            }
            return array;
        }
    }
}
