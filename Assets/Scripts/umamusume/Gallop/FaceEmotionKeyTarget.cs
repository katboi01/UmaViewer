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

            InitializeEmotionTarget(ref emotion.mouthTarget,ref emotion.mouthTarget, FaceDrivenKeyTarget.MouthMorphs, emotion.mouth, false);
            InitializeEmotionTarget(ref emotion.eyeLTarget, ref emotion.eyeLTarget, FaceDrivenKeyTarget.EyeMorphs, emotion.eye_l, true);
            InitializeEmotionTarget(ref emotion.eyeRTarget, ref emotion.eyeRTarget, FaceDrivenKeyTarget.EyeMorphs, emotion.eye_r, false);
            InitializeEmotionTarget(ref emotion.eyebrowLTarget, ref emotion.eyebrowLTarget, FaceDrivenKeyTarget.EyeBrowMorphs, emotion.eyebrow_l, true);
            InitializeEmotionTarget(ref emotion.eyebrowRTarget, ref emotion.eyebrowRTarget, FaceDrivenKeyTarget.EyeBrowMorphs, emotion.eyebrow_r, false);
        }

        if (UmaViewerUI.Instance)
        { 
            UmaViewerUI.Instance.LoadEmotionPanels(this);
        }
    }

    private void InitializeEmotionTarget(ref List<EmotionKey> targets, ref List<EmotionKey> emotionKeys,List<FacialMorph> morphs,string tags,bool direction)
    {
        targets = new List<EmotionKey>();
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
                    newValue.morph = morphs.Where(a => a.tag == splitArray[0] && a.direction == direction).First();
                }
                newValue.weight = Convert.ToInt32(splitArray[1]);
                emotionKeys.Add(newValue);
                Debug.Log(newValue.weight);
            }
            else
            {
                if (morphName == "Base")
                {
                    continue;
                }
                else
                {
                    newValue.morph = morphs.Where(a => a.tag == morphName && a.direction == direction).First();
                }
                newValue.weight = 100;
                emotionKeys.Add(newValue);
                Debug.Log(morphName);
            }
        }
    }

    public void UpdateAllFacialKeyTargets()
    {
        if (FaceDrivenKeyTarget == null) return;
        FaceDrivenKeyTarget.ClearMorph();
        foreach (var emotion in FaceEmotionKey)
        {
            if (emotion.mouthTarget != null)
            {
                foreach (var key in emotion.mouthTarget)
                {
                    key.morph.Weight += key.weight / 100 * emotion.Weight;
                }
            }
            if (emotion.eyeLTarget != null)
            {
                foreach (var key in emotion.eyeLTarget)
                {
                    key.morph.Weight += key.weight / 100 * emotion.Weight;
                }
            }
            if (emotion.eyeRTarget != null)
            {
                foreach (var key in emotion.eyeRTarget)
                {
                    key.morph.Weight += key.weight / 100 * emotion.Weight;
                }
            }
            if (emotion.eyebrowLTarget != null)
            {
                foreach (var key in emotion.eyeRTarget)
                {
                    key.morph.Weight += key.weight / 100 * emotion.Weight;
                }
            }
            if (emotion.eyebrowRTarget != null)
            {
                foreach (var key in emotion.eyeRTarget)
                {
                    key.morph.Weight += key.weight / 100 * emotion.Weight;
                }
            }
        }
    }
}

