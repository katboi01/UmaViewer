using Gallop.Live.Cutt;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DynamicBone;
using static Gallop.DrivenKeyComponent;
using static Gallop.Live.Cutt.LiveTimelineDefine;

namespace Gallop
{
    public class FaceDrivenKeyTarget : ScriptableObject
    {
        public UmaContainerCharacter Container;

        public List<EyeTarget> _eyeTarget;
        public List<EyebrowTarget> _eyebrowTarget;
        public List<MouthTarget> _mouthTarget;
        public List<TargetInfomation> _earTarget;

        public Dictionary<string, Transform> Objs;
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
        public List<FacialMorph> AllMorphs => EarMorphs.Concat(EyeBrowMorphs).Concat(EyeMorphs).Concat(MouthMorphs).Concat(OtherMorphs).ToList();

        public Transform DrivenKeyLocator;
        public Transform EyeballCtrl_L_Locator, EyeballCtrl_R_Locator, EyeballCtrl_All_Locator;

        Dictionary<Transform, Vector3> RotationRecorder = new Dictionary<Transform, Vector3>();
        Dictionary<Particle, Vector3> ParticleRecorder = new Dictionary<Particle, Vector3>();

        List<FacialMorph> leftEyeMorph = new List<FacialMorph>();
        List<FacialMorph> rightEyeMorph = new List<FacialMorph>();

        List<FacialMorph> leftEyebrowMorph = new List<FacialMorph>();
        List<FacialMorph> rightEyebrowMorph = new List<FacialMorph>();

        List<FacialMorph> leftEarMorph = new List<FacialMorph>();
        List<FacialMorph> rightEarMorph = new List<FacialMorph>();

        FacialOtherMorph CheekMorph;
        FacialOtherMorph ShadeMorph;
        FacialOtherMorph MangaMorph;
        FacialOtherMorph StaticTearMorph;
        List<FacialOtherMorph> TearMorphs = new List<FacialOtherMorph>();

        public void Initialize(Dictionary<string, Transform> objs)
        {
            Objs = objs;

            Dictionary<string, Particle> particles = new Dictionary<string, Particle>();
            foreach (CySpringDataContainer spring in Container.cySpringDataContainers)
            {
                foreach (DynamicBone dynamicbone in spring.DynamicBones)
                {
                    foreach(var particle in dynamicbone.Particles)
                    {
                        if (!particles.ContainsKey(particle.m_Transform.name)) 
                            particles.Add(particle.m_Transform.name,particle);
                    }
                }
            }

            for (int i = 0; i < _earTarget.Count; i++)
            {
                for (int j = 0; j < _earTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.index = i;
                    morph.direction = j > 0;
                    morph.name = "Ear_" + i + "_" + (morph.direction ? "L" : "R");
                    morph.tag = Enum.GetName(typeof(EarType), i);
                    morph.trsArray = _earTarget[i]._faceGroupInfo[j]._trsArray;
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        if (particles.TryGetValue(trs._path, out Particle physicsParticle))
                        {
                            trs.isPhysics = true;
                            trs.physicsParticle = physicsParticle;
                        }
                        Objs.TryGetValue(trs._path,out trs.transform);
                    }
                    if (i == 0)
                    {
                        if (morph.direction) BaseLEarMorph = morph;
                        else BaseREarMorph = morph;
                    }
                    else
                    {
                        if (morph.direction) leftEarMorph.Add(morph);
                        else rightEarMorph.Add(morph);

                        EarMorphs.Add(morph);
                    }
                }
            }

            for (int i = 0; i < _eyebrowTarget.Count; i++)
            {
                for (int j = 0; j < _eyebrowTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.index = i;
                    morph.direction = j > 0;
                    morph.name = "EyeBrow_" + i + "_" + (morph.direction ? "L" : "R");
                    morph.tag = Enum.GetName(typeof(LiveTimelineDefine.FacialEyebrowId), i);
                    morph.trsArray = _eyebrowTarget[i]._faceGroupInfo[j]._trsArray;
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        Objs.TryGetValue(trs._path, out trs.transform);
                    }
                    if (i == 0)
                    {
                        if (morph.direction) BaseLEyeBrowMorph = morph;
                        else BaseREyeBrowMorph = morph;
                    }
                    else
                    {
                        if (morph.direction) leftEyebrowMorph.Add(morph);
                        else rightEyebrowMorph.Add(morph);

                        EyeBrowMorphs.Add(morph);
                    }
                }
            }

            for (int i = 0; i < _eyeTarget.Count; i++)
            {
                for (int j = 0; j < _eyeTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.index = i;
                    morph.direction = j > 0;
                    morph.name = "Eye_" + i + "_" + (morph.direction ? "L" : "R");
                    morph.tag = Enum.GetName(typeof(LiveTimelineDefine.FacialEyeId), i);
                    morph.trsArray = _eyeTarget[i]._faceGroupInfo[j]._trsArray;
                    foreach (TrsArray trs in morph.trsArray)
                    {
                         Objs.TryGetValue(trs._path,out trs.transform);
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

                        if (morph.direction) leftEyeMorph.Add(morph);
                        else rightEyeMorph.Add(morph);

                        EyeMorphs.Add(morph);
                    }
                }
            }

            for (int i = 0; i < _mouthTarget.Count; i++)
            {
                for (int j = 0; j < _mouthTarget[i]._faceGroupInfo.Count; j++)
                {
                    FacialMorph morph = new FacialMorph();
                    morph.index = i;
                    morph.direction = j > 0;
                    morph.name = "Mouth_" + i + "_" + j;
                    morph.tag = Enum.GetName(typeof(LiveTimelineDefine.FacialMouthId), i);
                    morph.trsArray = _mouthTarget[i]._faceGroupInfo[j]._trsArray;
                    foreach (TrsArray trs in morph.trsArray)
                    {
                        Objs.TryGetValue(trs._path, out trs.transform);
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
                this.CheekMorph = CheekMorph;
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
                ShadeMorph = FaceShadowMorph;
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
                BindPrefabs = mangaObjects
            });
            FaceMangaMorph.BindProperties.Add(new BindProperty()
            {
                Part = BindProperty.LocatorPart.ScaX,
                Type = BindProperty.BindType.Enable,
                BindPrefabs = mangaObjects
            });
            OtherMorphs.Add(FaceMangaMorph);
            MangaMorph = FaceMangaMorph;

            if(Container.StaticTear_L && Container.StaticTear_R)
            {
                FacialOtherMorph StaticTearMorph = new FacialOtherMorph()
                {
                    name = "StaticTear_Ctrl",
                    locator = DrivenKeyLocator.Find("Cheek_Ctrl")
                };

                StaticTearMorph.BindProperties.Add(new BindProperty()
                {
                    Part = BindProperty.LocatorPart.PosX,
                    Type = BindProperty.BindType.Enable,
                    BindPrefabs = new List<GameObject>() { Container.StaticTear_L }
                });

                StaticTearMorph.BindProperties.Add(new BindProperty()
                {
                    Part = BindProperty.LocatorPart.PosY,
                    Type = BindProperty.BindType.Enable,
                    BindPrefabs = new List<GameObject>() { Container.StaticTear_R }
                });

                this.StaticTearMorph = StaticTearMorph;
                OtherMorphs.Add(StaticTearMorph);
            }

            foreach (var tear in Container.TearControllers)
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
                    Value = tear.Weight,
                    DefaultValue = tear.Weight
                });
                FaceTearMorph.BindProperties.Add(new BindProperty()
                {
                    Part = BindProperty.LocatorPart.PosY,
                    Type = BindProperty.BindType.TearSide,
                    BindTearController = tear,
                    Value = tear.CurrentDir,
                    DefaultValue = tear.CurrentDir
                });
                FaceTearMorph.BindProperties.Add(new BindProperty()
                {
                    Part = BindProperty.LocatorPart.ScaY,
                    Type = BindProperty.BindType.TearSelect,
                    BindTearController = tear,
                    Value = tear.CurrenObject,
                    DefaultValue = tear.CurrenObject
                });
                FaceTearMorph.BindProperties.Add(new BindProperty()
                {
                    Part = BindProperty.LocatorPart.ScaZ,
                    Type = BindProperty.BindType.TearSpeed,
                    BindTearController = tear,
                    Value = tear.Speed,
                    DefaultValue = tear.Speed
                });
                OtherMorphs.Add(FaceTearMorph);
                TearMorphs.Add(FaceTearMorph);
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
            if (Container.FaceOverrideData && Container.FaceOverrideData.Enable)
            {
                OverrideFace(Container.FaceOverrideData);
            }
            EarMorphs.ForEach(morph => ProcessMorph(morph));
            EyeBrowMorphs.ForEach(morph => ProcessMorph(morph));
            EyeMorphs.ForEach(morph => ProcessMorph(morph));
            MouthMorphs.ForEach(morph => ProcessMorph(morph));
            OtherMorphs.ForEach(morph => ProcessExtraMorph(morph));
            ApplyRotation();
        }

        public void ChangeMorphEye()
        {
            FacialResetEye();
            if (Container.FaceOverrideData && Container.FaceOverrideData.Enable)
            {
                OverrideFace(Container.FaceOverrideData);
            }
            EyeMorphs.ForEach(morph => ProcessMorph(morph));
            ApplyRotationEye();
        }

        public void ChangeMorphMouth()
        {
            FacialResetMouth();
            MouthMorphs.ForEach(morph => ProcessMorph(morph));
            ApplyRotationMouth();
        }

        public void ChangeMorphEyebrow()
        {
            FacialResetEyebrow();
            EyeBrowMorphs.ForEach(morph => ProcessMorph(morph));
            ApplyRotationEyebrow();
        }

        public void ChangeMorphEar()
        {
            FacialResetEar();
            EarMorphs.ForEach(morph => ProcessMorph(morph));
            ApplyRotationEar();
        }

        public void ChangeMorphEffect()
        {
            OtherMorphs.ForEach(morph => ProcessExtraMorph(morph));
        }

        public void ClearAllWeights()
        {
            EarMorphs.ForEach(morph => morph.weight = 0);
            EyeBrowMorphs.ForEach(morph => morph.weight = 0);
            EyeMorphs.ForEach(morph => morph.weight = 0);
            MouthMorphs.ForEach(morph => morph.weight = 0);

            OtherMorphs.ForEach(morph =>
            {
                morph.BindProperties.ForEach(property =>
                {
                    property.Value = property.DefaultValue;
                });
            });
        }

        private void ProcessMorph(FacialMorph morph)
        {
            var isOverride = Container.FaceOverrideData && Container.FaceOverrideData.Enable;
            if (isOverride && morph.OverrideType == OverrideType.Ignore) return;
            if (morph.weight == 0 && (!isOverride || morph.OverrideType == OverrideType.None))return;

            var weight = (isOverride && morph.OverrideType == OverrideType.Override ? morph.overrideWeight : morph.weight);

            foreach (TrsArray trs in morph.trsArray)
            {
                if (trs.isPhysics && Container.EnablePhysics)
                {
                    trs.physicsParticle.m_InitLocalPosition += trs._position * weight;
                    ParticleRecorder[trs.physicsParticle] += trs._rotation * weight;
                }
                else if (trs.transform)
                {
                    trs.transform.localScale += trs._scale * weight;
                    trs.transform.localPosition += trs._position * weight;
                    RotationRecorder[trs.transform] += trs._rotation * weight;
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
                        }
                        else
                        {
                            if (morph.BindProperties.Find(p =>p.Value >0) == null)
                            {
                                morph.BindGameObject.SetActive(false);
                            }
                        }
                        break;
                    case BindProperty.BindType.Shader:
                        property.BindMaterial.SetFloat(property.PropertyName, property.Value);
                        break;
                    case BindProperty.BindType.Select:
                        property.BindPrefabs.ForEach(a => a.SetActive((int)property.Value == property.BindPrefabs.IndexOf(a)));
                        break;
                    case BindProperty.BindType.EyeSelect:
                        var count = property.BindPrefabs.Count / 2;
                        var val = (int)property.Value;
                        property.BindPrefabs.ForEach(a =>
                        a.SetActive(val + count < property.BindPrefabs.Count && (val == property.BindPrefabs.IndexOf(a) || val + count == property.BindPrefabs.IndexOf(a))));
                        break;
                    case BindProperty.BindType.Enable:
                        if(property.BindPrefabs.Count == 1)
                        {
                            property.BindPrefabs.ForEach(a => a.SetActive(property.Value > 0));
                        }
                        else if(property.Value <= 0)
                        {
                            property.BindPrefabs.ForEach(a => a.SetActive(false));
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

        public void FacialResetEye()
        {
            FacialReset(BaseLEyeMorph.trsArray);
            FacialReset(BaseREyeMorph.trsArray);
        }

        public void FacialResetMouth()
        {
            FacialReset(BaseMouthMorph.trsArray);
        }

        public void FacialResetEyebrow()
        {
            FacialReset(BaseLEyeBrowMorph.trsArray);
            FacialReset(BaseREyeBrowMorph.trsArray);
        }

        public void FacialResetEar()
        {
            FacialReset(BaseLEarMorph.trsArray);
            FacialReset(BaseREarMorph.trsArray);
        }

        public void FacialResetEffect()
        {
            foreach(var morphs in OtherMorphs)
            {
                foreach(var property in morphs.BindProperties)
                {
                    property.Value = property.DefaultValue;
                }
            }
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

        public void ApplyRotationEye()
        {
            Action<List<TrsArray>> applyRotation = delegate (List<TrsArray> trsArray)
            {
               foreach (TrsArray trs in trsArray)
               {
                   if(trs.transform)
                   trs.transform.localRotation = RotationConvert.fromMaya(RotationRecorder[trs.transform]);
               }
            };
            applyRotation.Invoke(BaseLEyeMorph.trsArray);
            applyRotation.Invoke(BaseREyeMorph.trsArray);
        }

        public void ApplyRotationMouth()
        {
            Action<List<TrsArray>> applyRotation = delegate (List<TrsArray> trsArray)
            {
                foreach (TrsArray trs in trsArray)
                {
                    if (trs.transform)
                        trs.transform.localRotation = RotationConvert.fromMaya(RotationRecorder[trs.transform]);
                }
            };
            applyRotation.Invoke(BaseMouthMorph.trsArray);
        }

        public void ApplyRotationEyebrow()
        {
            Action<List<TrsArray>> applyRotation = delegate (List<TrsArray> trsArray)
            {
                foreach (TrsArray trs in trsArray)
                {
                    if (trs.transform)
                        trs.transform.localRotation = RotationConvert.fromMaya(RotationRecorder[trs.transform]);
                }
            };
            applyRotation.Invoke(BaseLEyeBrowMorph.trsArray);
            applyRotation.Invoke(BaseREyeBrowMorph.trsArray);
        }

        public void ApplyRotationEar()
        {
            Action<List<TrsArray>> applyRotation = delegate (List<TrsArray> trsArray)
            {
                foreach (TrsArray trs in trsArray)
                {
                    if(trs.isPhysics && Container.EnablePhysics)
                    {
                        trs.physicsParticle.m_InitLocalRotation = RotationConvert.fromMaya(ParticleRecorder[trs.physicsParticle]);
                    }
                    else if (trs.transform)
                    {
                        if (RotationRecorder.ContainsKey(trs.transform))
                        {
                            //Debug.Log("Key Found:" + trs.transform.name);
                            trs.transform.localRotation = RotationConvert.fromMaya(RotationRecorder[trs.transform]);
                        }
                        else
                        {
                            //Debug.Log("No Key Found:" + trs.transform.name);
                        }
                    }   
                }
            };
            applyRotation.Invoke(BaseLEarMorph.trsArray);
            applyRotation.Invoke(BaseREarMorph.trsArray);
        }

        public void ChangeMorphWeight(FacialMorph morph, float val)
        {
            Container.isAnimatorControl = false;
            morph.weight = val;
            ChangeMorph();
        }

        public void ChangeMorphWeight(BindProperty property, float val, FacialMorph morph, bool autoHideEye = true)
        {
            Container.isAnimatorControl = false;
            property.Value = val < 0 ? 0 : val;
            ChangeMorphEffect();

            if (autoHideEye && property.Type == BindProperty.BindType.Enable && morph.name.Contains("Manga"))
            {
                EyeMorphs[42].weight = (val > 0 ? 1 : 0);
                EyeMorphs[43].weight = (val > 0 ? 1 : 0);
                ChangeMorphEye();
            }
        }

        public void SetEyeRange(float lx, float ly, float rx, float ry)
        {
            LeftEyeXrange.weight = lx;
            LeftEyeYrange.weight = ly;
            RightEyeXrange.weight = rx;
            RightEyeYrange.weight = ry;
            if (Container.FaceOverrideData && Container.FaceOverrideData.Enable)
            {
                ChangeMorph();
            }
            else
            {
                ChangeMorphEye();
            }
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

        public void OverrideFace(FaceOverrideData overrideData)
        {

            Action<List<FacialMorph>, FaceOverrideReplaceDataSet[]> action = delegate (List<FacialMorph> morphs, FaceOverrideReplaceDataSet[] datalist)
            {
                Dictionary<LiveTimelineDefine.FacialEyeId, FacialMorph> morphDic = new Dictionary<LiveTimelineDefine.FacialEyeId, FacialMorph>();
                morphs.ForEach(m => {
                    m.OverrideType = OverrideType.Ignore;
                    m.overrideWeight = 0; 
                    morphDic.Add((LiveTimelineDefine.FacialEyeId)m.index, m); 
                });

                foreach (var data in datalist)
                {
                    bool changed = false;
                    foreach (var arr in data.DataArray)
                    {
                        foreach (var src in arr.SrcArray)
                        {
                            if (src == LiveTimelineDefine.FacialEyeId.Base) continue;
                            if (morphDic[src].weight > 0 )
                            {
                                foreach (var dst in arr.DstArray)
                                {
                                    var dm = morphDic[dst.Index];
                                    var weight = (dst.Weight == 1 ? 1 : Mathf.Lerp(0, dst.Weight, morphDic[src].weight));
                                    dm.overrideWeight += weight;
                                    dm.OverrideType =  OverrideType.Override;
                                }
                                morphDic[src].OverrideType = OverrideType.Override;
                                changed = true;
                                break;
                            }
                        }
                    }

                    if (!changed && data.IsOnlyBaseReplace) 
                    {
                        var replacemorph = morphDic[data.BaseReplaceFaceType];
                        replacemorph.overrideWeight += 1;
                        replacemorph.OverrideType = OverrideType.Override;
                    }
                }
                morphs.ForEach(m=> { m.overrideWeight = Mathf.Clamp01(m.overrideWeight); });
            };

            if (overrideData.IsBothEyesSetting)
            {
                action.Invoke(leftEyeMorph, overrideData.FaceOverrideArray);
                action.Invoke(rightEyeMorph, overrideData.FaceOverrideArray);
            }
        }

        public void AlterUpdateAutoLip(LiveTimelineKeyLipSyncData _prevLipKeyData, LiveTimelineKeyLipSyncData keyData_, float liveTime_, float _switch)
        {
            //|| lockMouth
            
            
            if (_switch == 0 || lockMouth)
            {
                return;
            }
            
            

            float weightRatio = CalcMorphWeight(liveTime_, keyData_.speed, keyData_.time, keyData_.interpolateType, keyData_, null);

            MouthMorphs.ForEach(morph => morph.weight = 0);

            if (_prevLipKeyData != null)
            {
                if(weightRatio != 1)
                {
                    foreach (var part in _prevLipKeyData.facialPartsDataArray)
                    {
                        if (part.FacialPartsId == 0 ) { continue; }
                        MouthMorphs[part.FacialPartsId - 1].weight = ((float)part.WeightPer / 100 * (1 - weightRatio)) * ((float)_prevLipKeyData.weight / 100);
                    }
                }
                else
                {
                    foreach (var part in _prevLipKeyData.facialPartsDataArray)
                    {
                        if (part.FacialPartsId == 0) { continue; }
                        MouthMorphs[part.FacialPartsId - 1].weight = 0;
                    }
                }
            } 

            foreach (var part in keyData_.facialPartsDataArray)
            {
                if (part.FacialPartsId == 0) { continue; }
                MouthMorphs[part.FacialPartsId - 1].weight += ((float)part.WeightPer / 100 * weightRatio) * ((float)keyData_.weight / 100);
            }

            ChangeMorphMouth();
        }

        public bool lockMouth = false;

        public void AlterUpdateFacialNew(ref FacialDataUpdateInfo updateInfo_, float liveTime_)
        {
            if (updateInfo_.mouthCur != null)
            {
                if((int)updateInfo_.mouthCur.attribute == 131072 || (int)updateInfo_.mouthCur.attribute == 0)
                {
                    lockMouth = true;
                }
                else
                {
                    lockMouth = false;
                }


                float weightRatio = CalcMorphWeight(liveTime_, updateInfo_.mouthCur.speed, updateInfo_.mouthCur.time, updateInfo_.mouthCur.interpolateType, updateInfo_.mouthCur, updateInfo_.mouthNext);

                MouthMorphs.ForEach(morph => morph.weight = 0);

                if (updateInfo_.mouthPrev != null)
                {
                    if (weightRatio != 1)
                    {
                        foreach (var part in updateInfo_.mouthPrev.facialPartsDataArray)
                        {
                            if (part.FacialPartsId == 0) { continue; }
                            MouthMorphs[part.FacialPartsId - 1].weight = ((float)part.WeightPer / 100 * (1 - weightRatio));
                        }
                    }
                    else
                    {
                        foreach (var part in updateInfo_.mouthPrev.facialPartsDataArray)
                        {
                            if (part.FacialPartsId == 0) { continue; }
                            MouthMorphs[part.FacialPartsId - 1].weight = 0;
                        }
                    }
                }

                foreach (var part in updateInfo_.mouthCur.facialPartsDataArray)
                {
                    if (part.FacialPartsId == 0) { continue; }
                    MouthMorphs[part.FacialPartsId - 1].weight += ((float)part.WeightPer / 100 * weightRatio);
                }

                ChangeMorphMouth();

            }

            var HasManga = false;
            if (updateInfo_.effect != null)
            {
                var info = updateInfo_.effect;
                if (CheekMorph != null)
                {
                    var cheektype = info.cheekType;
                    for (int i = 0; i < CheekMorph.BindProperties.Count; i++)
                    {
                        CheekMorph.BindProperties[i].Value = (cheektype - 1 == i ? 1 : 0);
                    }
                }

                if (MangaMorph != null)
                {
                    var mangatype = info.mangameIndex;
                    if (mangatype == 0)
                    {
                        HasManga = false;
                        MangaMorph.BindProperties[1].Value = mangatype;
                    }
                    else
                    {
                        HasManga = true;
                        MangaMorph.BindProperties[0].Value = mangatype - 1;
                        MangaMorph.BindProperties[1].Value = 1;
                    }
                }

                if (StaticTearMorph != null)
                {
                    var tearfulEnable = ((int)info.attribute & LiveTimelineKeyFacialEffectData.kAttrTearful) > 0;
                    var tearfultype = tearfulEnable ? info.tearyType : 0;
                    StaticTearMorph.BindProperties.ForEach(p => p.Value = tearfultype);
                }

                if (ShadeMorph != null)
                {
                    var shaderEnable = ((int)info.attribute & LiveTimelineKeyFacialEffectData.kAttrFaceShadowVisible) > 0;
                    if (shaderEnable)
                    {
                        ShadeMorph.BindProperties[0].Value = Mathf.MoveTowards(ShadeMorph.BindProperties[0].Value, 1, Time.deltaTime * 2);
                    }
                    else
                    {
                        ShadeMorph.BindProperties[0].Value = 0;
                    }
                }
                ChangeMorphEffect();
            }

            if (updateInfo_.eyeCur != null)
            {

                float weightRatio = CalcMorphWeight(liveTime_, updateInfo_.eyeCur.speed, updateInfo_.eyeCur.time, updateInfo_.eyeCur.interpolateType, updateInfo_.eyeCur, updateInfo_.eyeNext);

                EyeMorphs.ForEach(morph => morph.weight = 0);

                if (updateInfo_.eyePrev != null)
                {
                    if (weightRatio != 1)
                    {
                        foreach (var part in updateInfo_.eyePrev.facialPartsDataArrayL)
                        {
                            if (part.FacialPartsId == 0) { continue; }
                            leftEyeMorph[part.FacialPartsId - 1].weight = ((float)part.WeightPer / 100 * (1 - weightRatio)) * ((float)updateInfo_.eyePrev.weight / 100);
                        }
                        foreach (var part in updateInfo_.eyePrev.facialPartsDataArrayR)
                        {
                            if (part.FacialPartsId == 0) { continue; }
                            rightEyeMorph[part.FacialPartsId - 1].weight = ((float)part.WeightPer / 100 * (1 - weightRatio)) * ((float)updateInfo_.eyePrev.weight / 100);
                        }
                    }
                    else
                    {
                        foreach (var part in updateInfo_.eyePrev.facialPartsDataArrayL)
                        {
                            if (part.FacialPartsId == 0) { continue; }
                            leftEyeMorph[part.FacialPartsId - 1].weight = 0;
                        }
                        foreach (var part in updateInfo_.eyePrev.facialPartsDataArrayR)
                        {
                            if (part.FacialPartsId == 0) { continue; }
                            rightEyeMorph[part.FacialPartsId - 1].weight = 0;
                        }
                    }
                }

                foreach (var part in updateInfo_.eyeCur.facialPartsDataArrayL)
                {
                    if (part.FacialPartsId == 0) { continue; }
                    leftEyeMorph[part.FacialPartsId - 1].weight += ((float)part.WeightPer / 100 * weightRatio) * ((float)updateInfo_.eyeCur.weight / 100);
                }
                foreach (var part in updateInfo_.eyeCur.facialPartsDataArrayR)
                {
                    if (part.FacialPartsId == 0) { continue; }
                    rightEyeMorph[part.FacialPartsId - 1].weight += ((float)part.WeightPer / 100 * weightRatio) * ((float)updateInfo_.eyeCur.weight / 100);
                }

                EyeMorphs[42].weight = (HasManga ? 1 : 0); //Hide eye when has Manga eye
                EyeMorphs[43].weight = (HasManga ? 1 : 0);
                ChangeMorphEye();
            }

            if (updateInfo_.eyebrowCur != null)
            {

                float weightRatio = CalcMorphWeight(liveTime_, updateInfo_.eyebrowCur.speed, updateInfo_.eyebrowCur.time, updateInfo_.eyebrowCur.interpolateType, updateInfo_.eyebrowCur, updateInfo_.eyebrowNext);

                EyeBrowMorphs.ForEach(morph => morph.weight = 0);

                if (updateInfo_.eyebrowPrev != null)
                {
                    if (weightRatio != 1)
                    {
                        foreach (var part in updateInfo_.eyebrowPrev.facialPartsDataArrayL)
                        {
                            if (part.FacialPartsId == 0) { continue; }
                            leftEyebrowMorph[part.FacialPartsId - 1].weight = ((float)part.WeightPer / 100 * (1 - weightRatio)) * ((float)updateInfo_.eyebrowPrev.weight / 100);
                        }
                        foreach (var part in updateInfo_.eyebrowPrev.facialPartsDataArrayR)
                        {
                            if (part.FacialPartsId == 0) { continue; }
                            rightEyebrowMorph[part.FacialPartsId - 1].weight = ((float)part.WeightPer / 100 * (1 - weightRatio)) * ((float)updateInfo_.eyebrowPrev.weight / 100);
                        }
                    }
                    else
                    {
                        foreach (var part in updateInfo_.eyebrowPrev.facialPartsDataArrayL)
                        {
                            if (part.FacialPartsId == 0) { continue; }
                            leftEyebrowMorph[part.FacialPartsId - 1].weight = 0;
                        }
                        foreach (var part in updateInfo_.eyebrowPrev.facialPartsDataArrayR)
                        {
                            if (part.FacialPartsId == 0) { continue; }
                            rightEyebrowMorph[part.FacialPartsId - 1].weight = 0;
                        }
                    }
                }

                foreach (var part in updateInfo_.eyebrowCur.facialPartsDataArrayL)
                {
                    if (part.FacialPartsId == 0) { continue; }
                    leftEyebrowMorph[part.FacialPartsId - 1].weight += ((float)part.WeightPer / 100 * weightRatio) * ((float)updateInfo_.eyebrowCur.weight / 100);
                }
                foreach (var part in updateInfo_.eyebrowCur.facialPartsDataArrayR)
                {
                    if (part.FacialPartsId == 0) { continue; }
                    rightEyebrowMorph[part.FacialPartsId - 1].weight += ((float)part.WeightPer / 100 * weightRatio) * ((float)updateInfo_.eyebrowCur.weight / 100);
                }

                ChangeMorphEyebrow();

            }

            if (updateInfo_.earCur != null)
            {

                float weightRatio = CalcMorphWeight(liveTime_, updateInfo_.earCur.speed, updateInfo_.earCur.time, updateInfo_.earCur.interpolateType, updateInfo_.earCur, updateInfo_.earNext);

                EarMorphs.ForEach(morph => morph.weight = 0);

                if (updateInfo_.earPrev != null)
                {
                    if (weightRatio != 1)
                    {
                        var partL = updateInfo_.earPrev.facialEarIdL;

                        if (partL != 0)
                        {
                            leftEarMorph[partL - 1].weight = (((1 - weightRatio)) * ((float)updateInfo_.earPrev.weight / 100));
                        }

                        var partR = updateInfo_.earPrev.facialEarIdR;

                        if (partR != 0)
                        {
                            rightEarMorph[partR - 1].weight = (((1 - weightRatio)) * ((float)updateInfo_.earPrev.weight / 100));
                        }
                    }
                    else
                    {
                        var partL = updateInfo_.earPrev.facialEarIdL;

                        if (partL != 0)
                        {
                            leftEarMorph[partL - 1].weight = 0;
                        }

                        var partR = updateInfo_.earPrev.facialEarIdR;

                        if (partR != 0)
                        {
                            rightEarMorph[partR - 1].weight = 0;
                        }
                    }
                }

                var curPartL = updateInfo_.earCur.facialEarIdL;

                if (curPartL != 0)
                {
                    leftEarMorph[curPartL - 1].weight += weightRatio * ((float)updateInfo_.earCur.weight / 100);
                }

                var curPartR = updateInfo_.earCur.facialEarIdR;

                if (curPartR != 0)
                {
                    rightEarMorph[curPartR - 1].weight += weightRatio * ((float)updateInfo_.earCur.weight / 100);
                }

                ChangeMorphEar();

            }

            if (updateInfo_.eyeTrackCur != null)
            {
                float weightRatio = CalcMorphWeight(liveTime_, updateInfo_.eyeTrackCur.speed, updateInfo_.eyeTrackCur.time, updateInfo_.eyeTrackCur.interpolateType, updateInfo_.eyeTrackCur, updateInfo_.eyeTrackNext);

                Vector2 finalRotation = new Vector2();

                if (updateInfo_.eyeTrackPrev != null)
                {
                    finalRotation = Vector2.Lerp(GetTimelineEyeTrackRotation(updateInfo_.eyeTrackPrev), GetTimelineEyeTrackRotation(updateInfo_.eyeTrackCur), weightRatio);
                }
                else
                {
                    finalRotation = GetTimelineEyeTrackRotation(updateInfo_.eyeTrackCur);
                }

                SetEyeTrack(finalRotation);
            }
        }

        public Vector2 GetTimelineEyeTrackRotation(LiveTimelineKeyFacialEyeTrackData key)
        {
            if(CalcTargetPosition(key, out Vector3 target))
            {
                return GetEyeTrackRotation(target) + new Vector2((float)key.horizontalRatePer / 100, (float)key.verticalRatePer / 100);
            }
            else
            {
                return new Vector2(0, 0) + new Vector2((float)key.horizontalRatePer / 100, (float)key.verticalRatePer / 100);
            }
        }

        public bool CalcTargetPosition(LiveTimelineKeyFacialEyeTrackData key, out Vector3 target)
        {

            switch (key.targetType)
            {
                case FacialEyeTrackTargetType.Arena:
                    target = Container.HeadBone.transform.position + new Vector3(0, 1, 10);
                    return true;
                case FacialEyeTrackTargetType.CharaForward:
                    target = Container.HeadBone.transform.position + Container.HeadBone.transform.forward;
                    return true;
                case FacialEyeTrackTargetType.DirectPosition:
                    target = key.DirectPosition;
                    return true;
                case FacialEyeTrackTargetType.Camera:
                    target = Live.Director.instance.MainCameraTransform.position;
                    return true;
                case FacialEyeTrackTargetType.MultiCamera1:
                    target = Live.Director.instance._liveTimelineControl.GetMultiCameraWorldPosition(0);
                    return true;
                case FacialEyeTrackTargetType.MultiCamera2:
                    target = Live.Director.instance._liveTimelineControl.GetMultiCameraWorldPosition(1);
                    return true;
                case FacialEyeTrackTargetType.MultiCamera3:
                    target = Live.Director.instance._liveTimelineControl.GetMultiCameraWorldPosition(2);
                    return true;
                case FacialEyeTrackTargetType.MultiCamera4:
                    target = Live.Director.instance._liveTimelineControl.GetMultiCameraWorldPosition(3);
                    return true;
            }
            target = Vector3.zero;
            return false;
        }

        public Vector2 GetEyeTrackRotation(Vector3 target)
        {
            var targetPosotion = target - Container.HeadBone.transform.up * Container.EyeHeight;
            var deltaPos = Container.HeadBone.transform.InverseTransformPoint(targetPosotion);
            var deltaRotation = Quaternion.LookRotation(deltaPos.normalized, Container.HeadBone.transform.up).eulerAngles;
            if (deltaRotation.x > 180) deltaRotation.x -= 360;
            if (deltaRotation.y > 180) deltaRotation.y -= 360;

            var finalRotation = new Vector2(Mathf.Clamp(deltaRotation.y / 35, -1, 1), Mathf.Clamp(-deltaRotation.x / 25, -1, 1));//Limited to the angle of view 

            return finalRotation;
            
        }

        public void SetEyeTrack(Vector2 rotation)
        {
            SetEyeRange(rotation.x, rotation.y, rotation.x, -rotation.y);
        }

        public static float CalcMorphWeight(float time, int speed, int interFrame, InterpolateType interType, LiveTimelineKey curKey, LiveTimelineKey nextKey)
        {

            float keyTime = (float)curKey.FrameSecond;

            switch (speed)
            {
                case 1:
                    interFrame = 3;
                    break;
                case 10:
                    interFrame = 2;
                    break;
                case 20:
                    interFrame = 6;
                    break;
                case 21:
                    interFrame = 9;
                    break;
                default:
                    break;
            }

            float duringTime;
            if (nextKey != null)
            {
                duringTime = nextKey.frame - curKey.frame;
            }
            else
            {
                duringTime = interFrame;
            }

            float easeTime;

            if (duringTime < interFrame)
            {
                easeTime = (float)duringTime / 60;
            }
            else
            {
                easeTime = (float)interFrame / 60;
            }


            float passTime = time - keyTime;

            float weightRatio = 0;

            if (passTime >= easeTime)
            {
                weightRatio = 1;
            }
            else
            {
                float ratio = passTime / easeTime;
                switch (interType)
                {
                    case InterpolateType.Linear:
                        weightRatio = ratio;
                        break;
                    case InterpolateType.EaseIn:
                        weightRatio = Easing.EaseIn(ratio);
                        break;
                    case InterpolateType.EaseOut:
                        weightRatio = Easing.EaseOut(ratio);
                        break;
                    case InterpolateType.EaseInOut:
                        weightRatio = Easing.EaseInOut(ratio);
                        break;
                    default:
                        weightRatio = ratio;
                        break;
                }
            }

            return weightRatio;
        }
    }
}
