using System;
using UnityEngine;

namespace Stage
{
    [CreateAssetMenu(menuName = "ConditionVmData")]
    public class ConditionVmData : ScriptableObject
    {
        public enum OP
        {
            NOP = 0,
            ADD = 1,
            SUB = 2,
            OR = 3,
            AND = 4,
            EQ = 5,
            GREAT = 6,
            LESS = 7,
            GREAT_EQ = 8,
            LESS_EQ = 9,
            NOT = 10,
            expr2uminus = 11,
            MULT = 12,
            DIV = 13,
            expr2NUM = 14,
            expr2REGISTOR = 0x40,
            LET = 65,
            RANDOM = 80,
            IS_chrflasgs = 0x80,
            IS_chrid = 129,
            STATUS = 130,
            ISEXISTS = 131,
            IS_cardflasgs = 132,
            SYSTEMVALUE = 133
        }

        public enum CHR_FLAGS
        {
            LEFTY,
            PASSION,
            COOL,
            CUTE
        }

        public enum CARD_FLAGS
        {
            SSR,
            SSR1,
            SSR2
        }

        public enum POSITION
        {
            CENTER,
            LEFT1,
            RIGHT1,
            LEFT2,
            RIGHT2,
            LEFT3,
            RIGHT3,
            LEFT4,
            RIGHT4,
            LEFT5,
            RIGHT5,
            LEFT6,
            RIGHT6,
            LEFT7,
            RIGHT7,
            LEFT8,
            RIGHT8,
            LEFT9,
            RIGHT9,
            LEFT10,
            RIGHT10
        }

        public enum STATUS
        {
            DRESS,
            HEIGHT,
            POSITION
        }

        public enum SYSTEMVALUE
        {
            chara_id,
            dress_id
        }

        [Serializable]
        public struct VmInfos
        {
            public string conditionName;

            public VmCodeSet[] vmCodeSet;

            public int registorCount;
        }

        [Serializable]
        public struct VmCodeSet
        {
            public VmCode[] vmCode;
        }

        [Serializable]
        public struct VmCode
        {
            public OP operation;

            public int[] param;
        }

        [SerializeField]
        public VmInfos[] conditions;
    }
}