using Gallop.Live.Cutt;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DynamicBone;
using static Gallop.DrivenKeyComponent;
using static Gallop.FaceDrivenKeyTarget;

namespace Gallop
{
    public class FaceDrivenKeyTarget : ScriptableObject
    {
        public UmaContainer Container;

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

        public void ChangeMorphEye()
        {
            FacialResetEye();
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
                    property.Value = morph.GetLocatorValue(property.Part);
                });
            });
        }

        private void ProcessMorph(FacialMorph morph)
        {
            if (morph.weight == 0)
            {
                return;
            }
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
                        property.BindPrefab.ForEach(a => a.SetActive((int)property.Value == property.BindPrefab.IndexOf(a)));
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
                            property.BindPrefab.ForEach(a => a.SetActive(false));
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
            if (Container.FaceOverrideData && Container.FaceOverrideData.Enable)
            {
                OverrideFace(Container.FaceOverrideData);
            }
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
            if (Container.FaceOverrideData && Container.FaceOverrideData.Enable)
            {
                OverrideFace(Container.FaceOverrideData);
            }
            FacialReset(BaseLEyeMorph.trsArray);
            FacialReset(BaseREyeMorph.trsArray);
        }

        public void FacialResetMouth()
        {
            if (Container.FaceOverrideData && Container.FaceOverrideData.Enable)
            {
                OverrideFace(Container.FaceOverrideData);
            }
            FacialReset(BaseMouthMorph.trsArray);
        }

        public void FacialResetEyebrow()
        {
            if (Container.FaceOverrideData && Container.FaceOverrideData.Enable)
            {
                OverrideFace(Container.FaceOverrideData);
            }
            FacialReset(BaseLEyeBrowMorph.trsArray);
            FacialReset(BaseREyeBrowMorph.trsArray);
        }

        public void FacialResetEar()
        {
            if (Container.FaceOverrideData && Container.FaceOverrideData.Enable)
            {
                OverrideFace(Container.FaceOverrideData);
            }
            FacialReset(BaseLEarMorph.trsArray);
            FacialReset(BaseREarMorph.trsArray);
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

        public void ChangeMorphWeight(BindProperty property, float val, FacialMorph morph)
        {
            Container.isAnimatorControl = false;
            property.Value = val < 0 ? 0 : val;
            ChangeMorph();

            if (property.Type == BindProperty.BindType.Enable && morph.name.Contains("Manga"))
            {
                ChangeMorphWeight(EyeMorphs[42], val > 0 ? 1 : 0);
                ChangeMorphWeight(EyeMorphs[43], val > 0 ? 1 : 0);
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
                foreach (var data in datalist)
                {
                    if (data.IsOnlyBaseReplace)
                    {
                        var replacemorph = GetMorphByType(morphs, data.BaseReplaceFaceType);
                        var morph = morphs.Find(m => (m.weight > 0 && m != replacemorph));
                        if (morph == null)
                        {
                            replacemorph.weight = 1;
                            break;
                        }
                        replacemorph.weight = 0;
                    }

                    foreach (var arr in data.DataArray)
                    {
                        List<Action> targetMorph = new List<Action>();
                        bool changed = false;
                        foreach (var src in arr.SrcArray)
                        {
                            if (src == LiveTimelineDefine.FacialEyeId.Base) continue;
                            var srcmorph = GetMorphByType(morphs, src);
                            if (srcmorph.weight > 0 && !changed)
                            {
                                foreach (var dst in arr.DstArray)
                                {
                                    var dm = GetMorphByType(morphs, dst.Index);
                                    var weight = (dst.Weight == 1 ? 1 : Mathf.Lerp(0, dst.Weight, srcmorph.weight));
                                    targetMorph.Add(delegate () { dm.weight = weight; });
                                }
                                changed = true;
                            }

                            if (new List<FaceOverrideElement>(arr.DstArray).Find(f => f.Index == src) == null)
                            {
                                srcmorph.weight = 0;
                            }
                        }
                        targetMorph.ForEach(a => a());
                    }
                }
            };

            if (overrideData.IsBothEyesSetting)
            {
                action.Invoke(leftEyeMorph, overrideData.FaceOverrideArray);
                action.Invoke(rightEyeMorph, overrideData.FaceOverrideArray);
            }
        }

        public FacialMorph GetMorphByType(List<FacialMorph> morphs, LiveTimelineDefine.FacialEyeId Type)
        {
            return morphs.Find(m => m.index == (int)Type);
        }

        public void AlterUpdateAutoLip(LiveTimelineKeyLipSyncData _prevLipKeyData, LiveTimelineKeyLipSyncData keyData_, float liveTime_, float _switch)
        {
            //|| lockMouth
            
            
            if (_switch == 0 || lockMouth)
            {
                return;
            }
            
            

            float weightRatio = CalcMorphWeight(liveTime_, keyData_.time, keyData_.interpolateType, keyData_, null);

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


                float weightRatio = CalcMorphWeight(liveTime_, updateInfo_.mouthCur.time, updateInfo_.mouthCur.interpolateType, updateInfo_.mouthCur, updateInfo_.mouthNext);

                MouthMorphs.ForEach(morph => morph.weight = 0);

                if (updateInfo_.mouthPrev != null)
                {
                    if (weightRatio != 1)
                    {
                        foreach (var part in updateInfo_.mouthPrev.facialPartsDataArray)
                        {
                            if (part.FacialPartsId == 0) { continue; }
                            MouthMorphs[part.FacialPartsId - 1].weight = ((float)part.WeightPer / 100 * (1 - weightRatio)) * ((float)updateInfo_.mouthPrev.weight / 100);
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
                    MouthMorphs[part.FacialPartsId - 1].weight += ((float)part.WeightPer / 100 * weightRatio) * ((float)updateInfo_.mouthCur.weight / 100);
                }

                ChangeMorphMouth();

            }
            if (updateInfo_.eyeCur != null)
            {

                float weightRatio = CalcMorphWeight(liveTime_, updateInfo_.eyeCur.time, updateInfo_.eyeCur.interpolateType, updateInfo_.eyeCur, updateInfo_.eyeNext);

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

                ChangeMorphEye();

            }

            if (updateInfo_.eyebrowCur != null)
            {

                float weightRatio = CalcMorphWeight(liveTime_, updateInfo_.eyebrowCur.time, updateInfo_.eyebrowCur.interpolateType, updateInfo_.eyebrowCur, updateInfo_.eyebrowNext);

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

                float weightRatio = CalcMorphWeight(liveTime_, updateInfo_.earCur.time, updateInfo_.earCur.interpolateType, updateInfo_.earCur, updateInfo_.earNext);

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
        }

        public float CalcMorphWeight(float time, int interFrame, InterpolateType interType, LiveTimelineKey curKey, LiveTimelineKey nextKey)
        {

            float keyTime = (float)curKey.frame / 60;


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
