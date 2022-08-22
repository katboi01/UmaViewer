using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gallop
{
    public class FaceDrivenKeyTarget : MonoBehaviour
    {
        public List<EyeTarget> _eyeTarget;
        public List<EyebrowTarget> _eyebrowTarget;
        public List<MouthTarget> _mouthTarget;

        public List<Transform> Objs = new List<Transform>();
        private FacialMorph BaseLEyeBrowMorph, BaseREyeBrowMorph;
        private FacialMorph BaseLEyeMorph, BaseREyeMorph;
        private FacialMorph BaseMouthMorph;

        public List<FacialMorph> EyeBrowMorphs = new List<FacialMorph>();
        public List<FacialMorph> EyeMorphs = new List<FacialMorph>();
        public List<FacialMorph> MouthMorphs = new List<FacialMorph>();

        public FaceLoadCallBack callBack;
        private void Start()
        {

            Objs.AddRange(GetComponentsInChildren<Transform>());

            for (int i = 0; i < _eyebrowTarget.Count; i++)
            {
                for (int j = 0; j < _eyebrowTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.target = this;
                    morph.direction = j > 0;
                    morph.name = "EyeBrow_" + i + "_" + (morph.direction ? "R" : "L");
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
                    morph.name = "Eye_" + i + "_" + (morph.direction ? "R" : "L");
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
            FacialReset();
            MouthMorphs[3].Weight = 1;
            if (callBack != null) { callBack.CallBack(this); }
        }
        
        public void ChangeMorph()
        {
            FacialReset();

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
        }
        private void ProcessMorph(FacialMorph morph)
        {
            foreach (TrsArray trs in morph.trsArray)
            {
                if (trs.transform)
                {
                    trs.transform.localRotation = Quaternion.Euler(trs.transform.localRotation.eulerAngles + (trs._rotation * morph.Weight));
                    trs.transform.localPosition += trs._position * morph.Weight;
                    trs.transform.localScale += trs._scale * morph.Weight;
                };
            }
        }

        public void FacialReset()
        {
            foreach (TrsArray trs in BaseLEyeBrowMorph.trsArray)
            {
                if (trs.transform)
                {
                    trs.transform.localRotation = Quaternion.Euler(trs._rotation);
                    trs.transform.localPosition = trs._position;
                    trs.transform.localScale = trs._scale;
                };
            }

            foreach (TrsArray trs in BaseREyeBrowMorph.trsArray)
            {
                if (trs.transform)
                {
                    trs.transform.localRotation = Quaternion.Euler(trs._rotation);
                    trs.transform.localPosition = trs._position;
                    trs.transform.localScale = trs._scale;
                };
            }

            foreach (TrsArray trs in BaseLEyeMorph.trsArray)
            {
                if (trs.transform)
                {
                    trs.transform.localRotation = Quaternion.Euler(trs._rotation);
                    trs.transform.localPosition = trs._position;
                    trs.transform.localScale = trs._scale;
                };
            }

            foreach (TrsArray trs in BaseREyeMorph.trsArray)
            {
                if (trs.transform)
                {
                    trs.transform.localRotation = Quaternion.Euler(trs._rotation);
                    trs.transform.localPosition = trs._position;
                    trs.transform.localScale = trs._scale;
                };
            }

            foreach (TrsArray trs in BaseMouthMorph.trsArray)
            {
                if (trs.transform)
                {
                    trs.transform.localRotation = Quaternion.Euler(trs._rotation);
                    trs.transform.localPosition = trs._position;
                    trs.transform.localScale = trs._scale;
                };
            }
        }


    }



}



