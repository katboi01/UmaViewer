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
                    MouthMorphs.Add(morph);
                }
            }
        }

        private void Update()
        {
            for (int i = 0; i < EyeBrowMorphs.Count; i++)
            {
                if(EyeBrowMorphs[i].LastWeight!= EyeBrowMorphs[i].Weight)
                {
                    ChangeMorph();
                    EyeBrowMorphs[i].LastWeight = EyeBrowMorphs[i].Weight;
                }
            }
            for (int i = 0; i < EyeMorphs.Count; i++)
            {
                if (EyeMorphs[i].LastWeight != EyeMorphs[i].Weight)
                {
                    ChangeMorph();
                    EyeMorphs[i].LastWeight = EyeMorphs[i].Weight;
                }
            }
            for (int i = 0; i < MouthMorphs.Count; i++)
            {
                if (MouthMorphs[i].LastWeight != MouthMorphs[i].Weight)
                {
                    ChangeMorph();
                    MouthMorphs[i].LastWeight = MouthMorphs[i].Weight;
                }
            }
        }

        public void ChangeMorph()
        {
            FacialReset();
            foreach (FacialMorph morph in EyeBrowMorphs)
            {
                foreach(TrsArray trs in morph.trsArray)
                {
                    var bone = Objs.Find(ani => ani.name.Equals(trs._path));
                    if (bone)
                    {
                        if (trs.IsOverrideTarget==1)
                        {
                            bone.localRotation = Quaternion.Euler(Vector3.Lerp(OriRots[Objs.IndexOf(bone)].eulerAngles, trs._rotation, morph.Weight));
                            bone.localPosition = Vector3.Lerp(OriTrans[Objs.IndexOf(bone)],trs._position,morph.Weight);
                            bone.localScale = Vector3.Lerp(OriScal[Objs.IndexOf(bone)],trs._scale, morph.Weight);
                        }
                        else
                        {
                            bone.localRotation = Quaternion.Euler(bone.localRotation.eulerAngles+ (trs._rotation* morph.Weight));
                            bone.localPosition += trs._position*morph.Weight;
                            bone.localScale += trs._scale* morph.Weight;
                        }
                    };
                }
            }

            foreach (FacialMorph morph in EyeMorphs)
            {
                foreach (TrsArray trs in morph.trsArray)
                {
                    var bone = Objs.Find(ani => ani.name.Equals(trs._path));
                    if (bone)
                    {
                        if (trs.IsOverrideTarget == 1)
                        {
                            bone.localRotation = Quaternion.Euler(Vector3.Lerp(OriRots[Objs.IndexOf(bone)].eulerAngles, trs._rotation, morph.Weight));
                            bone.localPosition = Vector3.Lerp(OriTrans[Objs.IndexOf(bone)], trs._position, morph.Weight);
                            bone.localScale = Vector3.Lerp(OriScal[Objs.IndexOf(bone)], trs._scale, morph.Weight);
                        }
                        else
                        {
                            bone.localRotation = Quaternion.Euler(bone.localRotation.eulerAngles + (trs._rotation * morph.Weight));
                            bone.localPosition += trs._position * morph.Weight;
                            bone.localScale += trs._scale * morph.Weight;
                        }
                    };
                }
            }

            foreach (FacialMorph morph in MouthMorphs)
            {
                foreach (TrsArray trs in morph.trsArray)
                {
                    var bone = Objs.Find(ani => ani.name.Equals(trs._path));
                    if (bone)
                    {
                        if (trs.IsOverrideTarget == 1)
                        {
                            bone.localRotation = Quaternion.Euler(Vector3.Lerp(OriRots[Objs.IndexOf(bone)].eulerAngles, trs._rotation, morph.Weight));
                            bone.localPosition = Vector3.Lerp(OriTrans[Objs.IndexOf(bone)], trs._position, morph.Weight);
                            bone.localScale = Vector3.Lerp(OriScal[Objs.IndexOf(bone)], trs._scale, morph.Weight);
                        }
                        else
                        {
                            bone.localRotation = Quaternion.Euler(bone.localRotation.eulerAngles + (trs._rotation * morph.Weight));
                            bone.localPosition += trs._position * morph.Weight;
                            bone.localScale += trs._scale * morph.Weight;
                        }
                    };
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



