using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    public class ConditionVmRun
    {
        public const int VmFalse = 0;

        public const int VmTrue = 1;

        protected ConditionVmData _data;

        protected int[] _resultCache;

        private Dictionary<string, int> _conditionName2Index;

        private string[] _conditionNames;

        public string[] ConditionNames => _conditionNames;

        public ConditionVmRun(ConditionVmData vmdata)
        {
            SetScript(vmdata);
        }

        public int GetConditionIndex(string name)
        {
            if (_conditionName2Index.TryGetValue(name, out var value))
            {
                return value;
            }
            return -1;
        }

        public void SetScript(ConditionVmData data)
        {
            _data = data;
            int num = data.conditions.Length;
            _resultCache = new int[num];
            _conditionName2Index = new Dictionary<string, int>(num);
            _conditionNames = new string[num];
            for (int i = 0; i < num; i++)
            {
                _conditionName2Index[data.conditions[i].conditionName] = i;
                _conditionNames[i] = data.conditions[i].conditionName;
            }
        }

        public bool Result(int index, out int result)
        {
            if (_resultCache == null || _resultCache.Length <= index)
            {
                result = 0;
                return false;
            }
            result = _resultCache[index];
            return true;
        }

        public bool ResultUpdate()
        {
            if (_resultCache == null)
            {
                return false;
            }
            for (int i = 0; i < _resultCache.Length; i++)
            {
                int num = RunVmAndLastReslut(_data.conditions[i].vmCodeSet);
                _resultCache[i] = num;
            }
            return true;
        }

        public Dictionary<string, int> GetResultAll()
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            if (_resultCache == null)
            {
                return null;
            }
            for (int i = 0; i < _resultCache.Length; i++)
            {
                dictionary[_data.conditions[i].conditionName] = _resultCache[i];
            }
            return dictionary;
        }

        protected int RunVmAndLastReslut(ConditionVmData.VmCodeSet[] vmCodeSet)
        {
            int[] registors = new int[0];
            int result = 0;
            for (int i = 0; i < vmCodeSet.Length; i++)
            {
                result = RunVmInternal(vmCodeSet[i].vmCode, ref registors);
            }
            return result;
        }

        protected int[] RunVmAndAllResults(ConditionVmData.VmCodeSet[] vmCodeSet)
        {
            int[] registors = new int[0];
            int[] array = new int[vmCodeSet.Length];
            for (int i = 0; i < vmCodeSet.Length; i++)
            {
                array[i] = RunVmInternal(vmCodeSet[i].vmCode, ref registors);
            }
            return array;
        }

        protected int RunVmInternal(ConditionVmData.VmCode[] vmCode, ref int[] registors)
        {
            ConditionVmStack conditionVmStack = new ConditionVmStack();
            for (int i = 0; i < vmCode.Length; i++)
            {
                ConditionVmData.VmCode vmCode2 = vmCode[i];
                switch (vmCode2.operation)
                {
                    case ConditionVmData.OP.ADD:
                        {
                            int num11 = conditionVmStack.Pop();
                            int num12 = conditionVmStack.Pop();
                            conditionVmStack.Push(num12 + num11);
                            break;
                        }
                    case ConditionVmData.OP.SUB:
                        {
                            int num9 = conditionVmStack.Pop();
                            int num10 = conditionVmStack.Pop();
                            conditionVmStack.Push(num10 - num9);
                            break;
                        }
                    case ConditionVmData.OP.OR:
                        {
                            bool flag4 = ((conditionVmStack.Pop() != 0) ? true : false);
                            bool flag5 = ((conditionVmStack.Pop() != 0) ? true : false);
                            conditionVmStack.Push((flag5 || flag4) ? 1 : 0);
                            break;
                        }
                    case ConditionVmData.OP.AND:
                        {
                            bool flag = ((conditionVmStack.Pop() != 0) ? true : false);
                            bool flag2 = ((conditionVmStack.Pop() != 0) ? true : false);
                            conditionVmStack.Push((flag2 && flag) ? 1 : 0);
                            break;
                        }
                    case ConditionVmData.OP.EQ:
                        {
                            int num20 = conditionVmStack.Pop();
                            int num21 = conditionVmStack.Pop();
                            conditionVmStack.Push((num21 == num20) ? 1 : 0);
                            break;
                        }
                    case ConditionVmData.OP.GREAT:
                        {
                            int num22 = conditionVmStack.Pop();
                            int num23 = conditionVmStack.Pop();
                            conditionVmStack.Push((num23 > num22) ? 1 : 0);
                            break;
                        }
                    case ConditionVmData.OP.LESS:
                        {
                            int num4 = conditionVmStack.Pop();
                            int num5 = conditionVmStack.Pop();
                            conditionVmStack.Push((num5 < num4) ? 1 : 0);
                            break;
                        }
                    case ConditionVmData.OP.GREAT_EQ:
                        {
                            int num18 = conditionVmStack.Pop();
                            int num19 = conditionVmStack.Pop();
                            conditionVmStack.Push((num19 >= num18) ? 1 : 0);
                            break;
                        }
                    case ConditionVmData.OP.LESS_EQ:
                        {
                            int num13 = conditionVmStack.Pop();
                            int num14 = conditionVmStack.Pop();
                            conditionVmStack.Push((num14 <= num13) ? 1 : 0);
                            break;
                        }
                    case ConditionVmData.OP.NOT:
                        {
                            bool flag3 = ((conditionVmStack.Pop() != 0) ? true : false);
                            conditionVmStack.Push((!flag3) ? 1 : 0);
                            break;
                        }
                    case ConditionVmData.OP.expr2uminus:
                        {
                            int num17 = conditionVmStack.Pop();
                            conditionVmStack.Push(-num17);
                            break;
                        }
                    case ConditionVmData.OP.MULT:
                        {
                            int num15 = conditionVmStack.Pop();
                            int num16 = conditionVmStack.Pop();
                            conditionVmStack.Push(num16 * num15);
                            break;
                        }
                    case ConditionVmData.OP.DIV:
                        {
                            int num7 = conditionVmStack.Pop();
                            int num8 = conditionVmStack.Pop();
                            int item6 = ((num7 == 0) ? int.MaxValue : (num8 / num7));
                            conditionVmStack.Push(item6);
                            break;
                        }
                    case ConditionVmData.OP.expr2NUM:
                        {
                            int item5 = vmCode2.param[0];
                            conditionVmStack.Push(item5);
                            break;
                        }
                    case ConditionVmData.OP.expr2REGISTOR:
                        {
                            int num6 = vmCode2.param[0];
                            if (num6 < registors.Length)
                            {
                                conditionVmStack.Push(registors[num6]);
                            }
                            break;
                        }
                    case ConditionVmData.OP.LET:
                        {
                            int num2 = vmCode2.param[0];
                            if (num2 < registors.Length)
                            {
                                int num3 = conditionVmStack.Pop();
                                registors[num2] = num3;
                                conditionVmStack.Push(num3);
                            }
                            break;
                        }
                    case ConditionVmData.OP.RANDOM:
                        {
                            int num = conditionVmStack.Pop();
                            conditionVmStack.Push(Random.Range(0, num - 1));
                            break;
                        }
                    case ConditionVmData.OP.IS_chrflasgs:
                        {
                            ConditionVmData.POSITION position4 = (ConditionVmData.POSITION)vmCode2.param[0];
                            ConditionVmData.CHR_FLAGS chr_flags = (ConditionVmData.CHR_FLAGS)vmCode2.param[1];
                            int item4 = CheckPositionAndChrFlags(position4, chr_flags);
                            conditionVmStack.Push(item4);
                            break;
                        }
                    case ConditionVmData.OP.IS_cardflasgs:
                        {
                            ConditionVmData.POSITION position3 = (ConditionVmData.POSITION)vmCode2.param[0];
                            ConditionVmData.CARD_FLAGS card_flags = (ConditionVmData.CARD_FLAGS)vmCode2.param[1];
                            int item3 = CheckPositionAndCardFlags(position3, card_flags);
                            conditionVmStack.Push(item3);
                            break;
                        }
                    case ConditionVmData.OP.IS_chrid:
                        {
                            ConditionVmData.POSITION position2 = (ConditionVmData.POSITION)vmCode2.param[0];
                            int chara_id2 = vmCode2.param[1];
                            int item2 = CheckPositionAndChrId(position2, chara_id2);
                            conditionVmStack.Push(item2);
                            break;
                        }
                    case ConditionVmData.OP.STATUS:
                        {
                            ConditionVmData.STATUS status = (ConditionVmData.STATUS)vmCode2.param[0];
                            ConditionVmData.POSITION position = (ConditionVmData.POSITION)vmCode2.param[1];
                            int positionAndStatus = GetPositionAndStatus(position, status);
                            conditionVmStack.Push(positionAndStatus);
                            break;
                        }
                    case ConditionVmData.OP.ISEXISTS:
                        {
                            int chara_id = vmCode2.param[0];
                            int item = CheckExistsChrId(chara_id);
                            conditionVmStack.Push(item);
                            break;
                        }
                    case ConditionVmData.OP.SYSTEMVALUE:
                        {
                            ConditionVmData.SYSTEMVALUE systemvalue = (ConditionVmData.SYSTEMVALUE)vmCode2.param[0];
                            int systemValue = GetSystemValue(systemvalue);
                            conditionVmStack.Push(systemValue);
                            break;
                        }
                }
            }
            return conditionVmStack.Pop();
        }

        public virtual int CheckPositionAndChrFlags(ConditionVmData.POSITION position, ConditionVmData.CHR_FLAGS chr_flags)
        {
            return 0;
        }

        public virtual int CheckPositionAndCardFlags(ConditionVmData.POSITION position, ConditionVmData.CARD_FLAGS card_flags)
        {
            return 0;
        }

        public virtual int CheckPositionAndChrId(ConditionVmData.POSITION position, int chara_id)
        {
            return 0;
        }

        public virtual int GetPositionAndStatus(ConditionVmData.POSITION position, ConditionVmData.STATUS status)
        {
            return 0;
        }

        public virtual int CheckExistsChrId(int chara_id)
        {
            return 0;
        }

        public virtual int GetSystemValue(ConditionVmData.SYSTEMVALUE systemvalue)
        {
            return 0;
        }
    }
}
