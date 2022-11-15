using Gallop.Live.Cutt;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DynamicBone;

namespace Gallop
{
    public class FaceDrivenKeyTarget : ScriptableObject
    {
        public List<EyeTarget> _eyeTarget;
        public List<EyebrowTarget> _eyebrowTarget;
        public List<MouthTarget> _mouthTarget;
        public List<TargetInfomation> _earTarget;

        public List<Transform> Objs;
        private FacialMorph BaseLEarMorph, BaseREarMorph;
        private FacialMorph BaseLEyeBrowMorph, BaseREyeBrowMorph;
        private FacialMorph BaseLEyeMorph, BaseREyeMorph;
        private FacialMorph BaseMouthMorph;
        public FacialMorph LeftEyeXrange, LeftEyeYrange, RightEyeXrange, RightEyeYrange;

        public List<FacialMorph> EarMorphs = new List<FacialMorph>();
        public List<FacialMorph> EyeBrowMorphs = new List<FacialMorph>();
        public List<FacialMorph> EyeMorphs = new List<FacialMorph>();
        public List<FacialMorph> MouthMorphs = new List<FacialMorph>();

        public GameObject DrivenKeyLocator;
        public GameObject EyeballCtrl_L_Locator, EyeballCtrl_R_Locator, EyeballCtrl_All_Locator;

        public UmaContainer Container;

        Dictionary<Transform, Vector3> RotationRecorder = new Dictionary<Transform, Vector3>();
        Dictionary<Particle, Vector3> ParticleRecorder = new Dictionary<Particle, Vector3>();
        public void Initialize(List<Transform> objs)
        {
            Objs = objs;

            List<Particle> particles = new List<Particle>();
            foreach (CySpringDataContainer spring in Container.cySpringDataContainers)
            {
                foreach (DynamicBone dynamicBone in spring.DynamicBones)
                {
                    particles.AddRange(dynamicBone.Particles);
                }
            }

            for (int i = 0; i < _earTarget.Count; i++)
            {
                for (int j = 0; j < _earTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.target = this;
                    morph.direction = j > 0;
                    morph.name = "Ear_" + i + "_" + (morph.direction ? "L" : "R");
                    morph.tag = Enum.GetName(typeof(EarType), i);
                    morph.trsArray = _earTarget[i]._faceGroupInfo[j]._trsArray;
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        var physicsParticle = particles.Find(a => a.m_Transform.name.Equals(trs._path));
                        if (physicsParticle != null)
                        {
                            trs.isPhysics = true;
                            trs.physicsParticle = physicsParticle;
                        }
                        trs.transform = Objs.Find(ani => ani.name.Equals(trs._path));
                    }
                    if (i == 0)
                    {
                        if (morph.direction) BaseLEarMorph = morph;
                        else BaseREarMorph = morph;
                    }
                    else
                    {
                        EarMorphs.Add(morph);
                    }
                }
            }

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
                        if (morph.direction) BaseLEyeBrowMorph = morph;
                        else BaseREyeBrowMorph = morph;
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
                        if (morph.direction) BaseLEyeMorph = morph;
                        else BaseREyeMorph = morph;
                    }
                    else
                    {
                        if (i == 20)
                        {
                            if (morph.direction) LeftEyeXrange = morph;
                            else RightEyeXrange = morph;
                        }
                        else if (i == 21)
                        {
                            if (morph.direction) LeftEyeYrange = morph;
                            else RightEyeYrange = morph;
                        }

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

            ChangeMorphWeight(MouthMorphs[3], 1);
            SetupAnimator();
            if (UmaViewerUI.Instance)
            {
                UmaViewerUI.Instance.LoadFacialPanels(this);
            }
        }

        public void SetupAnimator()
        {
            DrivenKeyLocator = new GameObject("DrivenKeyLocator");
            DrivenKeyLocator.transform.SetParent(Container.transform);
            SetupLocator("Ear_L_Ctrl", "Ear_L__", EarMorphs.FindAll(a => a.direction == true));
            SetupLocator("Ear_R_Ctrl", "Ear_R__", EarMorphs.FindAll(a => a.direction == false));
            SetupLocator("Eye_L_Base_Ctrl", "Eye_L__", EyeMorphs.FindAll(a => a.direction == true));
            SetupLocator("Eye_R_Base_Ctrl", "Eye_R__", EyeMorphs.FindAll(a => a.direction == false));
            SetupLocator("Eyebrow_L_Base_Ctrl", "Eyebrow_L__", EyeBrowMorphs.FindAll(a => a.direction == true));
            SetupLocator("Eyebrow_R_Base_Ctrl", "Eyebrow_R__", EyeBrowMorphs.FindAll(a => a.direction == false));
            SetupLocator("Mouth_Base_Ctrl", "Mouth__", MouthMorphs.FindAll(a => a.direction == false));

            var root = new GameObject("Eyeball_L_Ctrl");
            root.transform.SetParent(DrivenKeyLocator.transform);
            EyeballCtrl_L_Locator = root;
                
            root = new GameObject("Eyeball_R_Ctrl");
            root.transform.SetParent(DrivenKeyLocator.transform);
            EyeballCtrl_R_Locator = root;
                
            root = new GameObject("Eyeball_all_Ctrl");
            root.transform.SetParent(DrivenKeyLocator.transform);
            EyeballCtrl_All_Locator = root;

            Container.UmaFaceAnimator = DrivenKeyLocator.AddComponent<Animator>();
            Container.UmaFaceAnimator.avatar = AvatarBuilder.BuildGenericAvatar(DrivenKeyLocator, "DrivenKeyLocator");
            Container.FaceOverrideController = Instantiate(UmaViewerBuilder.Instance.FaceOverrideController);
            Container.UmaFaceAnimator.runtimeAnimatorController = Container.FaceOverrideController;
        }

        public void SetupLocator(string rootName, string prefix, List<FacialMorph> morphs)
        {
            var root = new GameObject(rootName);
            root.transform.SetParent(DrivenKeyLocator.transform);

            morphs.ForEach(morph =>
            {
                var locator = new GameObject(prefix + morph.tag);
                locator.transform.SetParent(root.transform);
                morph.locator = locator.transform;
            });
        }

        public void ChangeMorph()
        {
            FacialResetAll();

            foreach (FacialMorph morph in EarMorphs)
            {
                ProcessMorph(morph);
            }

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
            foreach (FacialMorph morph in EarMorphs)
            {
                morph.weight = 0;
            }

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
                if (trs.isPhysics && Container.EnablePhysics)
                {
                    trs.physicsParticle.m_InitLocalPosition += trs._position * morph.weight;
                    ParticleRecorder[trs.physicsParticle] += trs._rotation * morph.weight;
                }
                else if(trs.transform)
                {
                    trs.transform.localScale += trs._scale * morph.weight;
                    trs.transform.localPosition += trs._position * morph.weight;
                    RotationRecorder[trs.transform] += trs._rotation * morph.weight;
                }
            }
        }

        public void FacialResetAll()
        {
            FacialReset(BaseLEarMorph.trsArray);
            FacialReset(BaseREarMorph.trsArray);
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
                if (trs.isPhysics && Container.EnablePhysics)
                {
                    trs.physicsParticle.m_InitLocalPosition = trs._position;
                    ParticleRecorder[trs.physicsParticle] = trs._rotation;
                }
                else if (trs.transform)
                {
                    trs.transform.localPosition = trs._position;
                    trs.transform.localScale = trs._scale;
                    RotationRecorder[trs.transform] = trs._rotation;
                }
            }
        }

        public void ApplyRotation()
        {
            foreach (var trs in RotationRecorder)
            {
                trs.Key.transform.localRotation = RotationConvert.fromMaya(trs.Value);
            }
            if (Container.EnablePhysics)
            {
                foreach (var particle in ParticleRecorder)
                {
                    particle.Key.m_InitLocalRotation = RotationConvert.fromMaya(particle.Value);
                }
            }
        }

        public void ChangeMorphWeight(FacialMorph morph, float val)
        {
            Container.isAnimatorControl = false;
            morph.weight = val;
            ChangeMorph();
        }

        public void SetEyeRange(float lx, float ly, float rx, float ry)
        {
            LeftEyeXrange.weight = lx;
            LeftEyeYrange.weight = ly;
            RightEyeXrange.weight = rx;
            RightEyeYrange.weight = ry;
            ChangeMorph();
        }

        public void ProcessLocator()
        {
            EyeBrowMorphs.ForEach(morph =>
            {
                morph.weight = morph.locator.transform.localPosition.x * -100;
            });

            EarMorphs.ForEach(morph =>
            {
                morph.weight = morph.locator.transform.localPosition.x * -100;
            });

            MouthMorphs.ForEach(morph =>
            {
                morph.weight = morph.locator.transform.localPosition.x * -100;
                if (morph.weight < 0)
                    morph.weight = -morph.weight;  
            });

            EyeMorphs.ForEach(morph =>
            {
                if (!(morph == LeftEyeXrange || morph == LeftEyeYrange || morph == RightEyeXrange || morph == RightEyeYrange))
                {
                    morph.weight = morph.locator.transform.localPosition.x * -100;
                }
            });

            LeftEyeXrange.weight = -(EyeballCtrl_L_Locator.transform.localPosition.x * -100 + EyeballCtrl_All_Locator.transform.localPosition.x * -100);
            RightEyeXrange.weight = -(EyeballCtrl_R_Locator.transform.localPosition.x * -100 + EyeballCtrl_All_Locator.transform.localPosition.x * -100);

            LeftEyeYrange.weight = -(EyeballCtrl_L_Locator.transform.localPosition.y * -100 + EyeballCtrl_All_Locator.transform.localPosition.y * -100);
            RightEyeYrange.weight = (EyeballCtrl_R_Locator.transform.localPosition.y * -100 + EyeballCtrl_All_Locator.transform.localPosition.y * -100);

            ChangeMorph();
        }

        public void ResetLocator()
        {
            EarMorphs.ForEach(morph =>
            {
                morph.weight = 0;
                morph.locator.transform.localPosition = Vector3.zero;
            });

            EyeBrowMorphs.ForEach(morph =>
            {
                morph.weight = 0;
                morph.locator.transform.localPosition = Vector3.zero;
            });

            EyeMorphs.ForEach(morph =>
            {
                morph.weight = 0;
                morph.locator.transform.localPosition = Vector3.zero;
            });

            MouthMorphs.ForEach(morph =>
            {
                morph.weight = 0;
                morph.locator.transform.localPosition = Vector3.zero;
            });


            EyeballCtrl_L_Locator.transform.localPosition = Vector3.zero;
            EyeballCtrl_R_Locator.transform.localPosition = Vector3.zero;
            EyeballCtrl_All_Locator.transform.localPosition = Vector3.zero;
            ChangeMorph();
        }
    }
}



