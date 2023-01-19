using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyFacialFaceData : LiveTimelineKey
    {
        private enum eCheekControl
        {
            ControlEnable = 1,
            CheekEnable
        }

        private const int kAttrCheek = 65536;

        private const int kAttrEyeNop = 131072;

        private const int CHR_ID_FIRST_INDEX = 100;

        public int faceFlag;

        public int faceSlotIdx;

        [SerializeField]
        private int _cheekControl;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.FacialFace;

        public bool IsCheek()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrCheek);
        }

        public bool IsEyeNop()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrEyeNop);
        }

        public static bool isChrSpecialFace(int flg)
        {
            return CHR_ID_FIRST_INDEX < flg;
        }

        private void SetCheekControlFlag(eCheekControl flag, bool enable)
        {
            if (enable)
            {
                _cheekControl |= (int)flag;
            }
            else
            {
                _cheekControl &= (int)(~flag);
            }
        }

        private bool CheckCheekControlFlag(eCheekControl flag)
        {
            return ((uint)_cheekControl & (uint)flag) != 0;
        }

        public bool IsCheekControlEnable()
        {
            return CheckCheekControlFlag(eCheekControl.ControlEnable);
        }

        public bool IsCheekEnable()
        {
            return CheckCheekControlFlag(eCheekControl.CheekEnable);
        }
    }
}
