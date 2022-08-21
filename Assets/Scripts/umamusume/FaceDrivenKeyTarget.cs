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
        private List<Vector3> OriTrans = new List<Vector3>();
        private List<Quaternion> OriRots = new List<Quaternion>();
        private List<Vector3> OriScal = new List<Vector3>();

        public List<FacialMorph> EyeBrowMorphs = new List<FacialMorph>();
        public List<FacialMorph> EyeMorphs = new List<FacialMorph>();
        public List<FacialMorph> MouthMorphs = new List<FacialMorph>();

        public FaceLoadCallBack callBack;
        private void Start()
        {

            Objs.AddRange(GetComponentsInChildren<Transform>());
            foreach (Transform g in Objs)
            {
                OriTrans.Add(g.localPosition);
                OriRots.Add(g.localRotation);
                OriScal.Add(g.localScale);
            }

            for (int i = 0; i < _eyebrowTarget.Count; i++)
            {
                for (int j = 0; j < _eyebrowTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.SetFace(this);
                    morph.name = "EyeBrow_" + i + "_" + (j > 0 ? "R" : "L");
                    morph.trsArray = _eyebrowTarget[i]._faceGroupInfo[j]._trsArray;
                    foreach(TrsArray trs in morph.trsArray)
                    {
                        trs.transform = Objs.Find(ani => ani.name.Equals(trs._path));
                    }
                    EyeBrowMorphs.Add(morph);
                }
            }

            for (int i = 0; i < _eyeTarget.Count; i++)
            {
                for (int j = 0; j < _eyeTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.SetFace(this);
                    morph.name = "Eye_" + i + "_" + (j > 0 ? "R" : "L");
                    morph.trsArray = _eyeTarget[i]._faceGroupInfo[j]._trsArray;
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        trs.transform = Objs.Find(ani => ani.name.Equals(trs._path));
                    }
                    EyeMorphs.Add(morph);
                }
            }

            for (int i = 0; i < _mouthTarget.Count; i++)
            {
                for (int j = 0; j < _mouthTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.SetFace(this);
                    morph.name = "Mouth_" + i + "_" + j;
                    morph.trsArray = _mouthTarget[i]._faceGroupInfo[j]._trsArray;
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        trs.transform = Objs.Find(ani => ani.name.Equals(trs._path));
                    }
                    MouthMorphs.Add(morph);
                }
            }

            MouthMorphs[4].Weight = 1;
            if (callBack != null) { callBack.CallBack(this); }
        }

       
        private void FixedUpdate()
        {
            bool changed = false;
            for (int i = 0; i < EyeBrowMorphs.Count; i++)
            {
                if(EyeBrowMorphs[i].LastWeight!= EyeBrowMorphs[i].Weight)
                {
                    changed = true;
                }
            }
            for (int i = 0; i < EyeMorphs.Count; i++)
            {
                if (EyeMorphs[i].LastWeight != EyeMorphs[i].Weight)
                {
                    changed = true;
                }
            }
            for (int i = 0; i < MouthMorphs.Count; i++)
            {
                if (MouthMorphs[i].LastWeight != MouthMorphs[i].Weight)
                {
                    changed = true;
                }
            }

            if (changed)ChangeMorph();
        }

        public void ChangeMorph()
        {
            FacialReset();
            foreach (FacialMorph morph in EyeBrowMorphs)
            {
                if(morph.LastWeight!=morph.Weight || morph.Weight > 0)
                foreach(TrsArray trs in morph.trsArray)
                {
                   if (trs.transform)
                   {
                        if (!trs.IsOverrideTarget)
                        {
                            var tmp = trs.transform.localRotation.eulerAngles;
                            tmp += (trs._rotation * morph.Weight);
                            trs.transform.localRotation = Quaternion.Euler(trs.transform.localRotation.eulerAngles + (trs._rotation * morph.Weight));
                            trs.transform.localPosition += trs._position * morph.Weight;
                            trs.transform.localScale += trs._scale * morph.Weight;
                        }
                   };
                }
            }

            foreach (FacialMorph morph in EyeMorphs)
            {
                if(morph.LastWeight!=morph.Weight || morph.Weight > 0)
                foreach (TrsArray trs in morph.trsArray)
                {
                    if (trs.transform)
                    {
                        if (!trs.IsOverrideTarget)
                        {
                            var tmp = trs.transform.localRotation.eulerAngles;
                            tmp += (trs._rotation * morph.Weight);
                            trs.transform.localRotation = Quaternion.Euler(trs.transform.localRotation.eulerAngles + (trs._rotation * morph.Weight));
                            trs.transform.localPosition += trs._position * morph.Weight;
                            trs.transform.localScale += trs._scale * morph.Weight;
                        }
                    };
                }
            }

            foreach (FacialMorph morph in MouthMorphs)
            {
                if(morph.LastWeight!=morph.Weight || morph.Weight > 0)
                foreach (TrsArray trs in morph.trsArray)
                {
                    if (trs.transform)
                    {
                        if (!trs.IsOverrideTarget)
                        {
                            var tmp = trs.transform.localRotation.eulerAngles;
                            tmp += (trs._rotation * morph.Weight);
                            trs.transform.localRotation = Quaternion.Euler(trs.transform.localRotation.eulerAngles + (trs._rotation * morph.Weight));
                            trs.transform.localPosition += trs._position * morph.Weight;
                            trs.transform.localScale += trs._scale * morph.Weight;
                        }
                    };
                }
            }
            

            foreach (FacialMorph morph in EyeBrowMorphs)//处理覆盖表情
            {
                if (morph.LastWeight != morph.Weight || morph.Weight > 0)
                {
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        if (trs.transform)
                        {
                            if (trs.IsOverrideTarget)
                            {
                                trs.transform.localRotation = Quaternion.Euler(Vector3.Lerp(trs.transform.localRotation.eulerAngles, trs._rotation, morph.Weight));
                                trs.transform.localPosition = Vector3.Lerp(trs.transform.localPosition, trs._position, morph.Weight);
                                trs.transform.localScale = Vector3.Lerp(trs.transform.localScale, trs._scale, morph.Weight);
                            }
                        };
                    }
                    morph.LastWeight = morph.Weight;
                }
            }

            foreach (FacialMorph morph in EyeMorphs)
            {
                if (morph.LastWeight != morph.Weight || morph.Weight > 0)
                {
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        if (trs.transform)
                        {
                            if (trs.IsOverrideTarget)
                            {
                                trs.transform.localRotation = Quaternion.Euler(Vector3.Lerp(trs.transform.localRotation.eulerAngles, trs._rotation, morph.Weight));
                                trs.transform.localPosition = Vector3.Lerp(trs.transform.localPosition, trs._position, morph.Weight);
                                trs.transform.localScale = Vector3.Lerp(trs.transform.localScale, trs._scale, morph.Weight);
                            }
                        };
                    }
                    morph.LastWeight = morph.Weight;
                }
            }

            foreach (FacialMorph morph in MouthMorphs)
            {
                if (morph.LastWeight != morph.Weight || morph.Weight > 0)
                {
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        if (trs.transform)
                        {
                            if (trs.IsOverrideTarget)
                            {
                                trs.transform.localRotation = Quaternion.Euler(Vector3.Lerp(trs.transform.localRotation.eulerAngles, trs._rotation, morph.Weight));
                                trs.transform.localPosition = Vector3.Lerp(trs.transform.localPosition, trs._position, morph.Weight);
                                trs.transform.localScale = Vector3.Lerp(trs.transform.localScale, trs._scale, morph.Weight);
                            }
                        };
                    }
                    morph.LastWeight = morph.Weight;
                }
            }
        }

        public void FacialReset()
        {
            for (int i = 0; i < Objs.Count; i++)
            {
                Objs[i].localPosition = OriTrans[i];
                Objs[i].localRotation = OriRots[i];
                Objs[i].localScale = OriScal[i];
            }
        }

        
    }

   

}



