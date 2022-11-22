using Gallop.Live.Cutt;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DynamicBone;

namespace Gallop
{
    public class FaceDrivenKeyTarget : ScriptableObject
    {
        public UmaContainer Container;

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
        public List<FacialOtherMorph> OtherMorphs = new List<FacialOtherMorph>();

        public Transform DrivenKeyLocator;
        public Transform EyeballCtrl_L_Locator, EyeballCtrl_R_Locator, EyeballCtrl_All_Locator;

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

            InitializeExtra();
            SetupAnimator();

            if (UmaViewerUI.Instance)
                UmaViewerUI.Instance.LoadFacialPanels(this);
        }

        public void InitializeExtra()
        {
            if (Container.CheekTex_0)
            {
                var CheekMorph = new FacialOtherMorph
                {
                    name = "Cheek_Ctrl",
                    locator = DrivenKeyLocator.Find("Cheek_Ctrl"),
                    BindGameObject = Container.Head.transform.Find("M_Cheek").gameObject
                };
                var bindMaterial = CheekMorph.BindGameObject.GetComponent<SkinnedMeshRenderer>().material;

                BindProperty cheekProperty = new BindProperty()
                {
                    BindMaterial = bindMaterial,
                    Part = BindProperty.LocatorPart.ScaX,
                    Type = BindProperty.BindType.Texture,
                    BindTexture = Container.CheekTex_0,
                    PropertyName = "_CheekRate"
                };
                CheekMorph.BindProperties.Add(cheekProperty);

                if (Container.CheekTex_1)
                {
                    BindProperty cheekProperty1 = new BindProperty()
                    {
                        BindMaterial = bindMaterial,
                        Part = BindProperty.LocatorPart.ScaY,
                        Type = BindProperty.BindType.Texture,
                        BindTexture = Container.CheekTex_1,
                        PropertyName = "_CheekRate"
                    };
                    CheekMorph.BindProperties.Add(cheekProperty1);
                }

                if (Container.CheekTex_2)
                {
                    BindProperty cheekProperty2 = new BindProperty()
                    {
                        BindMaterial = bindMaterial,
                        Part = BindProperty.LocatorPart.ScaZ,
                        Type = BindProperty.BindType.Texture,
                        BindTexture = Container.CheekTex_2,
                        PropertyName = "_CheekRate"
                    };
                    CheekMorph.BindProperties.Add(cheekProperty2);
                }
                OtherMorphs.Add(CheekMorph);
            }

            Container.FaceMaterial = Container.Head.transform.Find("M_Face").GetComponent<SkinnedMeshRenderer>().material;
            if (Container.FaceMaterial)
            {
                FacialOtherMorph FaceShadowMorph = new FacialOtherMorph()
                {
                    name = "Shade_Ctrl",
                    locator = DrivenKeyLocator.Find("Shade_Ctrl")
                };
                FaceShadowMorph.BindProperties.Add(
                    new BindProperty()
                    {
                        BindMaterial = Container.FaceMaterial,
                        Part = BindProperty.LocatorPart.ScaX,
                        Type = BindProperty.BindType.Shader,
                        PropertyName = "_faceShadowAlpha"
                    });
                OtherMorphs.Add(FaceShadowMorph);
            }

            FacialOtherMorph FaceMangaMorph = new FacialOtherMorph()
            {
                name = "Manga_Ctrl",
                locator = DrivenKeyLocator.Find("Manga_Ctrl")
            };

            var mangaObjects = new List<GameObject>();
            mangaObjects.AddRange(Container.LeftMangaObject);
            mangaObjects.AddRange(Container.RightMangaObject);
            FaceMangaMorph.BindProperties.Add(new BindProperty()
            {
                Part = BindProperty.LocatorPart.ScaY,
                Type = BindProperty.BindType.EyeSelect,
                BindPrefab = mangaObjects
            });
            FaceMangaMorph.BindProperties.Add(new BindProperty()
            {
                Part = BindProperty.LocatorPart.ScaX,
                Type = BindProperty.BindType.Enable,
                BindPrefab = mangaObjects
            });
            OtherMorphs.Add(FaceMangaMorph);

            foreach(var tear in Container.TearControllers)
            {
                var index = Container.TearControllers.IndexOf(tear);
                FacialOtherMorph FaceTearMorph = new FacialOtherMorph()
                {
                    name = $"Tear{index}_Ctrl",
                    locator = DrivenKeyLocator.Find($"Tear{index}_Ctrl")
                };
                FaceTearMorph.BindProperties.Add(new BindProperty()
                {
                    Part = BindProperty.LocatorPart.ScaX,
                    Type = BindProperty.BindType.TearWeight,
                    BindTearController = tear,
                    Value = tear.Weight
                });
                FaceTearMorph.BindProperties.Add(new BindProperty()
                {
                    Part = BindProperty.LocatorPart.PosY,
                    Type = BindProperty.BindType.TearSide,
                    BindTearController = tear,
                    Value = tear.CurrentDir
                });
                FaceTearMorph.BindProperties.Add(new BindProperty()
                {
                    Part = BindProperty.LocatorPart.ScaY,
                    Type = BindProperty.BindType.TearSelect,
                    BindTearController = tear,
                    Value = tear.CurrenObject
                });
                FaceTearMorph.BindProperties.Add(new BindProperty()
                {
                    Part = BindProperty.LocatorPart.ScaZ,
                    Type = BindProperty.BindType.TearSpeed,
                    BindTearController = tear,
                    Value = tear.Speed
                });
                OtherMorphs.Add(FaceTearMorph);
            }
        }

        public void SetupAnimator()
        {
            if (!DrivenKeyLocator) return;

            SetupLocator("Ear_L_Ctrl", EarMorphs.FindAll(a => a.direction == true));
            SetupLocator("Ear_R_Ctrl", EarMorphs.FindAll(a => a.direction == false));
            SetupLocator("Eye_L_Base_Ctrl", EyeMorphs.FindAll(a => a.direction == true));
            SetupLocator("Eye_R_Base_Ctrl", EyeMorphs.FindAll(a => a.direction == false));
            SetupLocator("Eyebrow_L_Base_Ctrl", EyeBrowMorphs.FindAll(a => a.direction == true));
            SetupLocator("Eyebrow_R_Base_Ctrl", EyeBrowMorphs.FindAll(a => a.direction == false));
            SetupLocator("Mouth_Base_Ctrl", MouthMorphs.FindAll(a => a.direction == false));

            EyeballCtrl_L_Locator = DrivenKeyLocator.transform.Find("Eyeball_L_Ctrl");
            EyeballCtrl_R_Locator = DrivenKeyLocator.transform.Find("Eyeball_R_Ctrl");
            EyeballCtrl_All_Locator = DrivenKeyLocator.transform.Find("Eyeball_all_Ctrl");

            Container.UmaFaceAnimator = DrivenKeyLocator.GetComponent<Animator>();
            Container.UmaFaceAnimator.avatar = AvatarBuilder.BuildGenericAvatar(DrivenKeyLocator.gameObject, "DrivenKeyLocator");
            Container.FaceOverrideController = Instantiate(UmaViewerBuilder.Instance.FaceOverrideController);
            Container.UmaFaceAnimator.runtimeAnimatorController = Container.FaceOverrideController;
        }

        public void SetupLocator(string rootName, List<FacialMorph> morphs)
        {

            var root = DrivenKeyLocator.transform.Find(rootName);
            var locators = new List<Transform>(root.GetComponentsInChildren<Transform>());
            morphs.ForEach(morph =>
            {
                var locator = locators.Find(a => a.name.EndsWith(morph.tag));
                if (locator)
                {
                    morph.locator = locator;
                }
            });
        }

        public void ChangeMorph()
        {
            FacialResetAll();
            EarMorphs.ForEach(morph => ProcessMorph(morph));
            EyeBrowMorphs.ForEach(morph => ProcessMorph(morph));
            EyeMorphs.ForEach(morph => ProcessMorph(morph));
            MouthMorphs.ForEach(morph => ProcessMorph(morph));
            OtherMorphs.ForEach(morph => ProcessExtraMorph(morph));
            ApplyRotation();
        }

        public void ClearMorph()
        {
            EarMorphs.ForEach(morph => morph.weight = 0);
            EyeBrowMorphs.ForEach(morph => morph.weight = 0);
            EyeMorphs.ForEach(morph => morph.weight = 0);
            MouthMorphs.ForEach(morph => morph.weight = 0);

            OtherMorphs.ForEach(morph =>
            {
                morph.BindProperties.ForEach(property =>
                {
                    property.Value = morph.GetLocatorValue(property.Part);
                });
            });
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
                else if (trs.transform)
                {
                    trs.transform.localScale += trs._scale * morph.weight;
                    trs.transform.localPosition += trs._position * morph.weight;
                    RotationRecorder[trs.transform] += trs._rotation * morph.weight;
                }
            }
        }

        private void ProcessExtraMorph(FacialOtherMorph morph)
        {
            foreach (var property in morph.BindProperties)
            {
                switch (property.Type)
                {
                    case BindProperty.BindType.Texture:
                        if (property.Value > 0.01f)
                        {
                            morph.BindGameObject.SetActive(true);
                            property.BindMaterial.mainTexture = property.BindTexture;
                            property.BindMaterial.SetFloat(property.PropertyName, property.Value);
                            return;
                        }
                        else
                        {
                            if (morph.BindProperties.IndexOf(property) == morph.BindProperties.Count - 1)
                            {
                                morph.BindGameObject.SetActive(false);
                            }
                        }
                        break;
                    case BindProperty.BindType.Shader:
                        property.BindMaterial.SetFloat(property.PropertyName, property.Value);
                        break;
                    case BindProperty.BindType.Select:
                        property.BindPrefab.ForEach(a=>a.SetActive((int)property.Value == property.BindPrefab.IndexOf(a)));
                        break;
                    case BindProperty.BindType.EyeSelect:
                        var count = property.BindPrefab.Count / 2;
                        var val = (int)property.Value;
                        property.BindPrefab.ForEach(a =>
                        a.SetActive(val + count < property.BindPrefab.Count && (val == property.BindPrefab.IndexOf(a) || val + count == property.BindPrefab.IndexOf(a))));
                        break;
                    case BindProperty.BindType.Enable:
                        if (property.Value <= 0)
                        {
                            property.BindPrefab.ForEach(a =>a.SetActive(false));
                        }
                        break;
                    case BindProperty.BindType.TearSide:
                        property.BindTearController.SetDir((int)property.Value);
                        break;
                    case BindProperty.BindType.TearWeight:
                        property.BindTearController.SetWegiht(property.Value);
                        break;
                    case BindProperty.BindType.TearSelect:
                        property.BindTearController.SetObject((int)property.Value);
                        break;
                    case BindProperty.BindType.TearSpeed:
                        property.BindTearController.SetSpeed(property.Value);
                        break;
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

        public void ChangeMorphWeight(BindProperty property, float val)
        {
            Container.isAnimatorControl = false;
            property.Value = val < 0 ? 0 : val;
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
                if (morph.locator)
                    morph.weight = morph.locator.transform.localPosition.x * -100;
            });

            EarMorphs.ForEach(morph =>
            {
                if (morph.locator)
                    morph.weight = morph.locator.transform.localPosition.x * -100;
            });

            MouthMorphs.ForEach(morph =>
            {
                if (morph.locator)
                {
                    morph.weight = morph.locator.transform.localPosition.x * -100;
                    if (morph.weight < 0)
                        morph.weight = -morph.weight;
                }
            });

            EyeMorphs.ForEach(morph =>
            {
                if (!(morph == LeftEyeXrange || morph == LeftEyeYrange || morph == RightEyeXrange || morph == RightEyeYrange))
                {
                    if (morph.locator)
                        morph.weight = morph.locator.transform.localPosition.x * -100;
                }
            });

            OtherMorphs.ForEach(morph =>
            {
                morph.BindProperties.ForEach(property =>
                {
                    property.Value = morph.GetLocatorValue(property.Part);
                });
            });

            LeftEyeXrange.weight = -(EyeballCtrl_L_Locator.transform.localPosition.x * -100 + EyeballCtrl_All_Locator.transform.localPosition.x * -100);
            RightEyeXrange.weight = -(EyeballCtrl_R_Locator.transform.localPosition.x * -100 + EyeballCtrl_All_Locator.transform.localPosition.x * -100);

            LeftEyeYrange.weight = -(EyeballCtrl_L_Locator.transform.localPosition.y * -100 + EyeballCtrl_All_Locator.transform.localPosition.y * -100);
            RightEyeYrange.weight = (EyeballCtrl_R_Locator.transform.localPosition.y * -100 + EyeballCtrl_All_Locator.transform.localPosition.y * -100);

            ChangeMorph();
        }

        public void ResetLocator()
        {
            Container.UmaFaceAnimator.Rebind();
            ProcessLocator();
            ChangeMorph();
        }
    }
}



