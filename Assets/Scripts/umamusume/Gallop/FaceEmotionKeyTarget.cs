using Gallop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class FaceEmotionKeyTarget : ScriptableObject
{
    public List<FaceTypeData> FaceEmotionKey;
    public FaceDrivenKeyTarget FaceDrivenKeyTarget;

    public void Initialize()
    {
        foreach (var emotion in FaceEmotionKey)
        {
            if (emotion.label == "Base") continue;
            emotion.target = this;
            emotion.emotionKeys = new List<EmotionKey>();
            InitializeEmotionTarget(emotion,FaceDrivenKeyTarget.MouthMorphs, emotion.mouth, false);
            InitializeEmotionTarget(emotion, FaceDrivenKeyTarget.EyeMorphs, emotion.eye_l, true);
            InitializeEmotionTarget(emotion, FaceDrivenKeyTarget.EyeMorphs, emotion.eye_r, false);
            InitializeEmotionTarget(emotion, FaceDrivenKeyTarget.EyeBrowMorphs, emotion.eyebrow_l, true);
            InitializeEmotionTarget(emotion, FaceDrivenKeyTarget.EyeBrowMorphs, emotion.eyebrow_r, false);
        }

        if (UmaViewerUI.Instance)
        { 
            UmaViewerUI.Instance.LoadEmotionPanels(this);
        }
    }

    private void InitializeEmotionTarget(FaceTypeData faceTypeData, List<FacialMorph> morphs,string tags,bool direction)
    {
        foreach (var morphName in tags.Split('|'))
        {
            EmotionKey newValue = new EmotionKey();
            if (morphName.Contains("__"))
            {
                var splitArray = Regex.Split(morphName, "__", RegexOptions.IgnoreCase);
                if (splitArray[0] == "Base")
                {
                    continue;
                }
                else
                {
                    newValue.morph = morphs.FirstOrDefault(a => a.tag == splitArray[0] && a.direction == direction);
                }
                newValue.weight = Convert.ToInt32(splitArray[1]);
                faceTypeData.emotionKeys.Add(newValue);
                //Debug.Log(newValue.weight);
            }
            else
            {
                if (morphName == "Base")
                {
                    continue;
                }
                else
                {
                    newValue.morph = morphs.FirstOrDefault(a => a.tag == morphName && a.direction == direction);
                }
                newValue.weight = 100;
                faceTypeData.emotionKeys.Add(newValue);
                //Debug.Log(morphName);
            }
        }
    }

    public void ChangeEmotionWeight(FaceTypeData emotion, float val)
    {
        FaceDrivenKeyTarget.Container.isAnimatorControl = false;
        emotion.weight = val;
        FaceDrivenKeyTarget.ClearAllWeights();
        UpdateAllTargetWeight();
        FaceDrivenKeyTarget.ChangeMorph();
    }

    public void UpdateAllTargetWeight()
    {
        if (FaceDrivenKeyTarget == null) return;
        
        foreach (var emotion in FaceEmotionKey)
        {
            if (emotion.emotionKeys != null)
            {
                foreach (var key in emotion.emotionKeys)
                {
                    key.morph.weight += key.weight / 100 * emotion.weight;
                }
            }
        }
    }
}

