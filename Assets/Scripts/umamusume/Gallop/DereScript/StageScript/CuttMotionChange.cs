using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cutt;
using Stage;
using UnityEngine;

public class CuttMotionChange
{
    private const int ReferenceMotSeqSheet = 0;

    private List<LiveTimelineWorkSheet> _sheetList;

    private CuttConditionOption _conditionOption;

    private Character3DBase.CharacterData[] _characterData;

    private Dictionary<string, ConditionVmData.POSITION> _originalName2Clip;

    private Dictionary<string, string> _swapMotionDic;

    private Dictionary<int, int> _motSeqIndex2position;

    private Dictionary<int, int> _motOverrideIndex2position;

    private Dictionary<string, AnimationClip> _motionClip;

    private bool _isExtendMotionLoad;

    private string[] _motNeedFilenames;

    private string _motFilePath;

    private string _localPath;

    public bool IsExtendMotionLoad => _isExtendMotionLoad;

    public CuttMotionChange(CuttConditionOption conditionOption, string charaMotion, Character3DBase.CharacterData[] characterData, List<LiveTimelineWorkSheet> sheetList, int[] motionSequenceIndices, int[] motionOverwriteIndices)
    {
        _conditionOption = conditionOption;
        _sheetList = sheetList;
        _characterData = characterData;
        Match match = Regex.Match(charaMotion, "([^/]+/)$");
        if (match.Success)
        {
            _isExtendMotionLoad = true;
            _localPath = match.Groups[1].Value;
            _motFilePath = charaMotion;
        }
        GetPositionFromMotSeqListDic(motionSequenceIndices, motionOverwriteIndices);
        _originalName2Clip = GetOriginalMotionNameFromCutt();
        _swapMotionDic = CreateSwapMotionDicFromCondition(characterData);
        _motNeedFilenames = GetNeedFilenames();
    }

    public string[] GetAssetBundleDLList()
    {
        string[] array = null;
        if (_isExtendMotionLoad)
        {
            array = new string[_motNeedFilenames.Length];
            for (int i = 0; i < _motNeedFilenames.Length; i++)
            {
                string arg = _motNeedFilenames[i].Replace(_localPath, "");
                array[i] = $"3d_cutt_{arg}_legacy.unity3d";
            }
        }
        else
        {
            array = new string[0];
        }
        return array;
    }

    public void OverrideMotionClip()
    {
        MakeInstantiateAnimationClip();
        OverrideMotionClipExec();
    }

    public void WriteLogText()
    {
    }

    private void MakeInstantiateAnimationClip()
    {
        _motionClip = new Dictionary<string, AnimationClip>();
        ResourcesManager instance = SingletonMonoBehaviour<ResourcesManager>.instance;
        if (_isExtendMotionLoad)
        {
            foreach (string key in _originalName2Clip.Keys)
            {
                if (!_swapMotionDic.TryGetValue(key, out var value))
                {
                    value = key;
                }
                string objectName = $"{_motFilePath}{value}_legacy";
                AnimationClip animationClip = instance.LoadObject(objectName) as AnimationClip;
                if (animationClip != null)
                {
                    animationClip.frameRate = Application.targetFrameRate;
                    _motionClip[key] = animationClip;
                }
            }
            return;
        }
        foreach (string key2 in _swapMotionDic.Keys)
        {
            string arg = _swapMotionDic[key2];
            string objectName2 = $"{_motFilePath}{arg}_legacy";
            AnimationClip animationClip2 = instance.LoadObject(objectName2) as AnimationClip;
            if (animationClip2 != null)
            {
                animationClip2.frameRate = Application.targetFrameRate;
                _motionClip[key2] = animationClip2;
            }
        }
    }

    private void RegistResumeList(AnimationClip srcClip, string destName)
    {
    }

    public void OverrideMotionClipExec()
    {
        Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
        foreach (LiveTimelineWorkSheet sheet in _sheetList)
        {
            List<LiveTimelineCharaMotSeqData> charaMotSeqList = sheet.charaMotSeqList;
            for (int i = 0; i < charaMotSeqList.Count; i++)
            {
                LiveTimelineCharaMotSeqData liveTimelineCharaMotSeqData = charaMotSeqList[i];
                for (int j = 0; j < liveTimelineCharaMotSeqData.keys.Count; j++)
                {
                    LiveTimelineKeyCharaMotionData liveTimelineKeyCharaMotionData = (LiveTimelineKeyCharaMotionData)liveTimelineCharaMotSeqData.keys[j];
                    string text = liveTimelineKeyCharaMotionData.motionName.Replace("_legacy", "");
                    if (_motionClip.TryGetValue(text, out var value))
                    {
                        RegistResumeList(liveTimelineKeyCharaMotionData.clip, value.name);
                        liveTimelineKeyCharaMotionData.clip = value;
                        liveTimelineKeyCharaMotionData.motionName = value.name;
                        liveTimelineKeyCharaMotionData.motionNameHash = FNVHash.Generate(liveTimelineKeyCharaMotionData.motionName);
                    }
                    else if (!string.IsNullOrEmpty(text))
                    {
                        dictionary[text] = true;
                    }
                }
            }
            List<LiveTimelineCharaOverrideMotSeqData> charaMotOverwriteList = sheet.charaMotOverwriteList;
            for (int k = 0; k < charaMotOverwriteList.Count; k++)
            {
                LiveTimelineCharaOverrideMotSeqData liveTimelineCharaOverrideMotSeqData = charaMotOverwriteList[k];
                for (int l = 0; l < liveTimelineCharaOverrideMotSeqData.keys.Count; l++)
                {
                    LiveTimelineKeyCharaMotionData liveTimelineKeyCharaMotionData2 = (LiveTimelineKeyCharaMotionData)liveTimelineCharaOverrideMotSeqData.keys[l];
                    string text2 = liveTimelineKeyCharaMotionData2.motionName.Replace("_legacy", "");
                    if (_motionClip.TryGetValue(text2, out var value2))
                    {
                        RegistResumeList(liveTimelineKeyCharaMotionData2.clip, value2.name);
                        liveTimelineKeyCharaMotionData2.clip = value2;
                        liveTimelineKeyCharaMotionData2.motionName = value2.name;
                    }
                    else if (!string.IsNullOrEmpty(text2))
                    {
                        dictionary[text2] = true;
                    }
                }
            }
            List<LiveTimelineCharaHeightMotSeqData> charaHeightMotList = sheet.charaHeightMotList;
            for (int m = 0; m < charaHeightMotList.Count; m++)
            {
                LiveTimelineCharaHeightMotSeqData liveTimelineCharaHeightMotSeqData = charaHeightMotList[m];
                for (int n = 0; n < liveTimelineCharaHeightMotSeqData.keys.Count; n++)
                {
                    LiveTimelineKeyCharaHeightMotionData liveTimelineKeyCharaHeightMotionData = (LiveTimelineKeyCharaHeightMotionData)liveTimelineCharaHeightMotSeqData.keys[n];
                    string text3 = liveTimelineKeyCharaHeightMotionData.motionName.Replace("_legacy", "");
                    if (_motionClip.TryGetValue(text3, out var value3))
                    {
                        RegistResumeList(liveTimelineKeyCharaHeightMotionData.clip, value3.name);
                        liveTimelineKeyCharaHeightMotionData.clip = value3;
                        liveTimelineKeyCharaHeightMotionData.motionName = value3.name;
                    }
                    else if (!string.IsNullOrEmpty(text3))
                    {
                        dictionary[text3] = true;
                    }
                }
            }
        }
    }

    private string[] GetNeedFilenames()
    {
        List<string> list = new List<string>(_originalName2Clip.Count);
        foreach (string key in _originalName2Clip.Keys)
        {
            if (_swapMotionDic.TryGetValue(key, out var value))
            {
                list.Add(value);
            }
            else
            {
                list.Add(key);
            }
        }
        return list.ToArray();
    }

    private Dictionary<string, ConditionVmData.POSITION> GetOriginalMotionNameFromCutt()
    {
        Dictionary<string, ConditionVmData.POSITION> dictionary = new Dictionary<string, ConditionVmData.POSITION>();
        foreach (LiveTimelineWorkSheet sheet in _sheetList)
        {
            List<LiveTimelineCharaMotSeqData> charaMotSeqList = sheet.charaMotSeqList;
            for (int i = 0; i < charaMotSeqList.Count; i++)
            {
                LiveTimelineCharaMotSeqData liveTimelineCharaMotSeqData = charaMotSeqList[i];
                for (int j = 0; j < liveTimelineCharaMotSeqData.keys.Count; j++)
                {
                    string text = ((LiveTimelineKeyCharaMotionData)liveTimelineCharaMotSeqData.keys[j]).motionName.Replace("_legacy", "");
                    if (!string.IsNullOrEmpty(text) && _motSeqIndex2position.TryGetValue(i, out var value))
                    {
                        dictionary[text] = (ConditionVmData.POSITION)value;
                    }
                }
            }
            List<LiveTimelineCharaOverrideMotSeqData> charaMotOverwriteList = sheet.charaMotOverwriteList;
            for (int k = 0; k < charaMotOverwriteList.Count; k++)
            {
                LiveTimelineCharaOverrideMotSeqData liveTimelineCharaOverrideMotSeqData = charaMotOverwriteList[k];
                for (int l = 0; l < liveTimelineCharaOverrideMotSeqData.keys.Count; l++)
                {
                    LiveTimelineKeyCharaOverrideMotionData liveTimelineKeyCharaOverrideMotionData = (LiveTimelineKeyCharaOverrideMotionData)liveTimelineCharaOverrideMotSeqData.keys[l];
                    int value2;
                    if (liveTimelineKeyCharaOverrideMotionData.enableRandomPlay)
                    {
                        for (int m = 0; m < liveTimelineKeyCharaOverrideMotionData.randomCount; m++)
                        {
                            var randomClips = liveTimelineKeyCharaOverrideMotionData.randomClips[m];
                            if (randomClips == null)
                            {
                                Debug.Log("コンパイルした場合、なぜかrandomClipsがNullになってしまう");
                                Debug.Log("Editorでのみ読み込みが可能な模様。要修正");
                            }
                            else
                            {
                                string text2 = randomClips.name.Replace("_legacy", "");
                                if (!string.IsNullOrEmpty(text2) && _motOverrideIndex2position.TryGetValue(k, out value2))
                                {
                                    dictionary[text2] = (ConditionVmData.POSITION)value2;
                                }
                            }
                        }
                    }
                    else
                    {
                        string text2 = liveTimelineKeyCharaOverrideMotionData.motionName.Replace("_legacy", "");
                        if (!string.IsNullOrEmpty(text2) && _motOverrideIndex2position.TryGetValue(k, out value2))
                        {
                            dictionary[text2] = (ConditionVmData.POSITION)value2;
                        }
                    }
                }
            }
            List<LiveTimelineCharaHeightMotSeqData> charaHeightMotList = sheet.charaHeightMotList;
            for (int m = 0; m < charaHeightMotList.Count; m++)
            {
                LiveTimelineCharaHeightMotSeqData liveTimelineCharaHeightMotSeqData = charaHeightMotList[m];
                for (int n = 0; n < liveTimelineCharaHeightMotSeqData.keys.Count; n++)
                {
                    string text3 = ((LiveTimelineKeyCharaHeightMotionData)liveTimelineCharaHeightMotSeqData.keys[n]).motionName.Replace("_legacy", "");
                    if (!string.IsNullOrEmpty(text3))
                    {
                        dictionary[text3] = (ConditionVmData.POSITION)liveTimelineCharaHeightMotSeqData.charaPosition;
                    }
                }
            }
        }
        return dictionary;
    }

    private Dictionary<string, string> CreateSwapMotionDicFromCondition(Character3DBase.CharacterData[] characterDatas)
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        if (_conditionOption != null && _conditionOption.changeMotion != null && _conditionOption.motionChangeParams != null)
        {
            CuttMotionVmRun cuttMotionVmRun = new CuttMotionVmRun(_conditionOption.changeMotion, ref characterDatas);
            cuttMotionVmRun.ResultUpdate();
            int num = _conditionOption.motionChangeParams.changeParams.Length;
            for (int i = 0; i < num; i++)
            {
                Live3DMotionChangeParams.ChangeParams changeParams = _conditionOption.motionChangeParams.changeParams[i];
                string motionName = changeParams.motionName;
                int num2 = changeParams.changeList.Length;
                for (int j = 0; j < num2; j++)
                {
                    string conditionName = changeParams.changeList[j].conditionName;
                    int conditionIndex = cuttMotionVmRun.GetConditionIndex(conditionName);
                    if (cuttMotionVmRun.Result(conditionIndex, out var result) && result != 0)
                    {
                        if (_originalName2Clip.TryGetValue(motionName, out var value))
                        {
                            string motionSuffix = changeParams.changeList[j].motionSuffix;
                            string text = (dictionary[motionName] = GetProcessedMotionName(motionName, motionSuffix, (int)value));
                        }
                        break;
                    }
                }
            }
        }
        return dictionary;
    }

    private string GetProcessedMotionName(string baseName, string destName, int position)
    {
        int characterIdFromPosition = GetCharacterIdFromPosition(position);
        string text = destName;
        if (0 < characterIdFromPosition)
        {
            text = text.Replace("%CHARA_ID%", $"{characterIdFromPosition:000}");
        }
        return text.Replace("%BASE%", baseName);
    }

    private void GetPositionFromMotSeqListDic(int[] motionSequenceIndices, int[] motionOverwriteIndices)
    {
        _motSeqIndex2position = new Dictionary<int, int>();
        int count = _sheetList[0].charaMotSeqList.Count;
        for (int i = 0; i < motionSequenceIndices.Length; i++)
        {
            int num = motionSequenceIndices[i];
            if (0 <= num && num < count && !_motSeqIndex2position.ContainsKey(num))
            {
                _motSeqIndex2position[num] = i;
            }
        }
        _motOverrideIndex2position = new Dictionary<int, int>();
        int count2 = _sheetList[0].charaMotOverwriteList.Count;
        for (int j = 0; j < motionOverwriteIndices.Length; j++)
        {
            int num2 = motionOverwriteIndices[j];
            if (0 <= num2 && num2 < count2 && !_motOverrideIndex2position.ContainsKey(num2))
            {
                _motOverrideIndex2position[num2] = j;
            }
        }
    }

    private int GetPositionFromMotSeqList(int motSeqListindex)
    {
        if (_motSeqIndex2position.TryGetValue(motSeqListindex, out var value))
        {
            return value;
        }
        return -1;
    }

    private int GetPositionFromMotOverrideList(int motSeqListindex)
    {
        if (_motOverrideIndex2position.TryGetValue(motSeqListindex, out var value))
        {
            return value;
        }
        return -1;
    }

    private int GetCharacterIdFromPosition(int position, int unitIndex = 0)
    {
        int result = -1;
        if (position < _characterData.Length)
        {
            result = _characterData[position].charaId;
        }
        return result;
    }

    public bool IsSwapMotionName(string name)
    {
        string key = name.Replace("_legacy", "");
        if (_swapMotionDic != null && _swapMotionDic.ContainsKey(key))
        {
            return true;
        }
        return false;
    }
}
