using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Gallop.Live.Cutt;
using System;

namespace Gallop
{
    public class FaceDrivenKeyTarget : ScriptableObject
    {
        public List<EyeTarget> _eyeTarget;
        public List<EyebrowTarget> _eyebrowTarget;
        public List<MouthTarget> _mouthTarget;

        public List<Transform> Objs;
        private FacialMorph BaseLEyeBrowMorph, BaseREyeBrowMorph;
        private FacialMorph BaseLEyeMorph, BaseREyeMorph;
        private FacialMorph BaseMouthMorph;

        public List<FacialMorph> EyeBrowMorphs = new List<FacialMorph>();
        public List<FacialMorph> EyeMorphs = new List<FacialMorph>();
        public List<FacialMorph> MouthMorphs = new List<FacialMorph>();

        public bool needUpdate = false;
        public bool needAllUpdate = false;
        public UmaContainer Container;

        Dictionary<Transform, Vector3> RotationRecorder = new Dictionary<Transform, Vector3>();
        public void Initialize(List<Transform> objs)
        {
            Objs = objs;

            for (int i = 0; i < _eyebrowTarget.Count; i++)
            {
                for (int j = 0; j < _eyebrowTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.target = this;
                    morph.direction = j > 0;
                    morph.name = "EyeBrow_" + i + "_" + (morph.direction ? "L" : "R");
                    morph.tag = Enum.GetName(typeof(LiveTimelineDefine.FacialEyebrowId), i);
                    morph.trsArray = _eyebrowTarget[i]._faceGroupInfo[j]._trsArray;
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        trs.transform = Objs.Find(ani => ani.name.Equals(trs._path));
                    }
                    if (i == 0)
                    {
                        if (morph.direction) BaseREyeBrowMorph = morph;
                        else BaseLEyeBrowMorph = morph;
                    }
                    else
                    {
                        EyeBrowMorphs.Add(morph);
                    }
                }
            }

            for (int i = 0; i < _eyeTarget.Count; i++)
            {
                for (int j = 0; j < _eyeTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.target = this;
                    morph.direction = j > 0;
                    morph.name = "Eye_" + i + "_" + (morph.direction ? "L" : "R");
                    morph.tag = Enum.GetName(typeof(LiveTimelineDefine.FacialEyeId), i);
                    morph.trsArray = _eyeTarget[i]._faceGroupInfo[j]._trsArray;
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        trs.transform = Objs.Find(ani => ani.name.Equals(trs._path));
                    }
                    if (i == 0)
                    {
                        if (morph.direction) BaseREyeMorph = morph;
                        else BaseLEyeMorph = morph;
                    }
                    else
                    {
                        EyeMorphs.Add(morph);
                    }
                }
            }

            for (int i = 0; i < _mouthTarget.Count; i++)
            {
                for (int j = 0; j < _mouthTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.target = this;
                    morph.direction = j > 0;
                    morph.name = "Mouth_" + i + "_" + j;
                    morph.tag = Enum.GetName(typeof(LiveTimelineDefine.FacialMouthId), i);
                    morph.trsArray = _mouthTarget[i]._faceGroupInfo[j]._trsArray;
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        trs.transform = Objs.Find(ani => ani.name.Equals(trs._path));
                    }
                    if (i == 0)
                    {
                        BaseMouthMorph = morph;
                    }
                    else
                    {
                        MouthMorphs.Add(morph);
                    }
                }
            }
            FacialResetAll();
            ChangeMorphWeight(MouthMorphs[3], 1);

            if (UmaViewerUI.Instance)
            {
                UmaViewerUI.Instance.LoadFacialPanels(this);
            }
        }

        public void ChangeMorph()
        {
            FacialResetAll();

            foreach (FacialMorph morph in EyeBrowMorphs)
            {
                ProcessMorph(morph);
            }

            foreach (FacialMorph morph in EyeMorphs)
            {
                ProcessMorph(morph);
            }

            foreach (FacialMorph morph in MouthMorphs)
            {
                ProcessMorph(morph);
            }

            ApplyRotation();
        }

        public void ClearMorph()
        {
            foreach (FacialMorph morph in EyeBrowMorphs)
            {
                morph.weight = 0;
            }

            foreach (FacialMorph morph in EyeMorphs)
            {
                morph.weight = 0;
            }

            foreach (FacialMorph morph in MouthMorphs)
            {
                morph.weight = 0;
            }
        }

        private void ProcessMorph(FacialMorph morph)
        {
            foreach (TrsArray trs in morph.trsArray)
            {
                if (trs.transform)
                {
                    trs.transform.localScale += trs._scale * morph.weight;
                    trs.transform.localPosition += trs._position * morph.weight;
                    RotationRecorder[trs.transform] += trs._rotation * morph.weight;
                };
            }
        }

        public void FacialResetAll()
        {
            FacialReset(BaseLEyeBrowMorph.trsArray);
            FacialReset(BaseREyeBrowMorph.trsArray);
            FacialReset(BaseLEyeMorph.trsArray);
            FacialReset(BaseREyeMorph.trsArray);
            FacialReset(BaseMouthMorph.trsArray);
            ApplyRotation();
        }

        public void FacialReset(List<TrsArray> trsArrays)
        {
            foreach (TrsArray trs in trsArrays)
            {
                if (trs.transform)
                {
                    trs.transform.localPosition = trs._position;
                    trs.transform.localScale = trs._scale;
                    RotationRecorder[trs.transform] = trs._rotation;
                };
            }
        }

        public void ApplyRotation()
        {
            foreach (var trs in RotationRecorder)
            {
                trs.Key.localRotation = RotationConvert.fromMaya(trs.Value);
            }
        }

        public void ChangeMorphWeight(FacialMorph morph,float val)
        {
            morph.weight = val;
            ChangeMorph();
        }
        
    }
}



