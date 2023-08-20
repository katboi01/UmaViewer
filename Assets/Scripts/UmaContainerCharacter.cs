using Gallop;
using Gallop.Live.Cutt;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class UmaContainerCharacter : UmaContainer
{
    public void LoadTextures(UmaDatabaseEntry entry)
    {
        foreach(Texture2D tex2D in entry.GetAll<Texture2D>())
        {
            if (entry.Name.Contains("/mini/head"))
            {
                MiniHeadTextures.Add(tex2D);
            }
            else if (entry.Name.Contains("/tail/"))
            {
                TailTextures.Add(tex2D);
            }
            else if (entry.Name.Contains("bdy0"))
            {
                GenericBodyTextures.Add(tex2D);
            }
            else if (entry.Name.Contains("_face") || entry.Name.Contains("_hair"))
            {
                if (IsMob)
                    MobHeadTextures.Add(tex2D);
            }
        }
    }

    public void LoadBody(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        Body = Instantiate(go, transform);
        UmaAnimator = Body.GetComponent<Animator>();

        if (IsMini)
        {
            UpBodyBone = Body.transform.Find("Position/Hip").gameObject;
        }
        else
        {
            UpBodyBone = Body.GetComponent<AssetHolder>()._assetTable["upbody_ctrl"] as GameObject;
        }

        if (IsGeneric)
        {
            List<Texture2D> textures = GenericBodyTextures;
            string costumeIdShort = VarCostumeIdShort,
                   costumeIdLong = VarCostumeIdLong,
                   height = VarHeight,
                   skin = VarSkin,
                   socks = VarSocks,
                   bust = VarBust;

            foreach (Renderer r in Body.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    string mainTex = "", toonMap = "", tripleMap = "", optionMap = "", zekkenNumberTex = "";

                    if (IsMini)
                    {
                        m.SetTexture("_MainTex", textures[0]);
                    }
                    else
                    {

                        if (m.shader.name.Contains("Noline") && m.shader.name.Contains("TSER"))
                        {
                            var s = Builder.ShaderList.Find(a => a.name == m.shader.name.Replace("Noline", "")); //Generic costume shader need to change manually.
                            if (s)
                            {
                                m.shader = s;
                            }
                        }

                        //BodyAlapha's shader need to change manually.
                        if (m.name.Contains("bdy"))
                        {
                            if (m.name.Contains("Alpha"))
                            {
                                m.shader = UmaAssetManager.BodyAlphaShader;
                            }
                            else
                            {
                                //some costume use area texture
                                var areaTex = UmaViewerMain.Instance.AbChara.FirstOrDefault(a => a.Name.StartsWith(UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/textures") && a.Name.EndsWith("area"));
                                if (areaTex != null)
                                {
                                    LoadTextures(areaTex);
                                    m.SetTexture("_MaskColorTex", textures.FirstOrDefault(t => t.name.Contains(costumeIdShort) && t.name.EndsWith("area")));
                                    SetMaskColor(m, IsMob ? MobDressColor : CharaData, IsMob);
                                }
                            }
                        }

                        switch (costumeIdShort.Split('_')[0]) //costume ID
                        {
                            case "0001":
                                switch (r.sharedMaterials.ToList().IndexOf(m))
                                {
                                    case 0:
                                        mainTex = $"tex_bdy{costumeIdShort}_00_waku0_diff";
                                        toonMap = $"tex_bdy{costumeIdShort}_00_waku0_shad_c";
                                        tripleMap = $"tex_bdy{costumeIdShort}_00_waku0_base";
                                        optionMap = $"tex_bdy{costumeIdShort}_00_waku0_ctrl";
                                        break;
                                    case 1:
                                        mainTex = $"tex_bdy{costumeIdShort}_00_{skin}_{bust}_{socks.PadLeft(2, '0')}_diff";
                                        toonMap = $"tex_bdy{costumeIdShort}_00_{skin}_{bust}_{socks.PadLeft(2, '0')}_shad_c";
                                        tripleMap = $"tex_bdy{costumeIdShort}_00_0_{bust}_00_base";
                                        optionMap = $"tex_bdy{costumeIdShort}_00_0_{bust}_00_ctrl";
                                        break;
                                    case 2:
                                        int color = UnityEngine.Random.Range(0, 4);
                                        mainTex = $"tex_bdy0001_00_zekken{color}_{bust}_diff";
                                        toonMap = $"tex_bdy0001_00_zekken{color}_{bust}_shad_c";
                                        tripleMap = $"tex_bdy0001_00_zekken0_{bust}_base";
                                        optionMap = $"tex_bdy0001_00_zekken0_{bust}_ctrl";
                                        break;
                                }

                                zekkenNumberTex = $"tex_bdy0001_00_num{UnityEngine.Random.Range(1, 18):d2}";
                                break;
                            case "0003":
                                mainTex = $"tex_bdy{costumeIdShort}_00_{skin}_{bust}_diff";
                                toonMap = $"tex_bdy{costumeIdShort}_00_{skin}_{bust}_shad_c";
                                tripleMap = $"tex_bdy{costumeIdShort}_00_0_{bust}_base";
                                optionMap = $"tex_bdy{costumeIdShort}_00_0_{bust}_ctrl";
                                break;
                            case "0006":
                                mainTex = $"tex_bdy{costumeIdLong}_{skin}_{bust}_{"00"}_diff";
                                toonMap = $"tex_bdy{costumeIdLong}_{skin}_{bust}_{"00"}_shad_c";
                                tripleMap = $"tex_bdy{costumeIdLong}_0_{bust}_00_base";
                                optionMap = $"tex_bdy{costumeIdLong}_0_{bust}_00_ctrl";
                                break;
                            case "0009":
                                mainTex = $"tex_bdy{costumeIdLong}_{skin}_{bust}_{"00"}_diff";
                                toonMap = $"tex_bdy{costumeIdLong}_{skin}_{bust}_{"00"}_shad_c";
                                tripleMap = $"tex_bdy{costumeIdLong}_0_{bust}_00_base";
                                optionMap = $"tex_bdy{costumeIdLong}_0_{bust}_00_ctrl";
                                break;
                            default:
                                mainTex = $"tex_bdy{costumeIdLong}_{skin}_{bust}_diff";
                                toonMap = $"tex_bdy{costumeIdLong}_{skin}_{bust}_shad_c";
                                tripleMap = $"tex_bdy{costumeIdLong}_0_{bust}_base";
                                optionMap = $"tex_bdy{costumeIdLong}_0_{bust}_ctrl";
                                break;
                        }
                        Debug.Log("Looking for texture " + mainTex);
                        m.SetTexture("_MainTex", textures.FirstOrDefault(t => t.name == mainTex));
                        m.SetTexture("_ToonMap", textures.FirstOrDefault(t => t.name == toonMap));
                        m.SetTexture("_TripleMaskMap", textures.FirstOrDefault(t => t.name == tripleMap));
                        m.SetTexture("_OptionMaskMap", textures.FirstOrDefault(t => t.name == optionMap));

                        if (!string.IsNullOrEmpty(zekkenNumberTex))
                            m.SetTexture("_ZekkenNumberTex", textures.FirstOrDefault(t => t.name == zekkenNumberTex));
                    }
                }
            }
        }
        else
        {
            foreach (Renderer r in Body.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    //BodyAlapha's shader need to change manually.
                    if (m.name.Contains("bdy") && m.name.Contains("Alpha"))
                    {
                        m.shader = UmaAssetManager.BodyAlphaShader;
                    }
                }
            }
        }
    }

    public void LoadHead(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        var textures = MobHeadTextures;
        GameObject head = Instantiate(go, transform);
        Head = head;

        //Some setting for Head
        EnableEyeTracking = UI.EnableEyeTracking;

        foreach (Renderer r in head.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                if (head.name.Contains("mchr"))
                {
                    if (r.name.Contains("Hair"))
                    {
                        Tail = head;
                    }
                    if (r.name == "M_Face")
                    {
                        m.SetTexture("_MainTex", MiniHeadTextures.First(t => t.name.Contains("face") && t.name.Contains("diff")));
                    }
                    if (r.name == "M_Cheek")
                    {
                        m.CopyPropertiesFromMaterial(Builder.TransMaterialCharas);
                        m.SetTexture("_MainTex", MiniHeadTextures.First(t => t.name.Contains("cheek")));
                    }
                    if (r.name == "M_Mouth")
                    {
                        m.SetTexture("_MainTex", MiniHeadTextures.First(t => t.name.Contains("mouth")));
                    }
                    if (r.name == "M_Eye")
                    {
                        m.SetTexture("_MainTex", MiniHeadTextures.First(t => t.name.Contains("eye")));
                    }
                    if (r.name.StartsWith("M_Mayu_"))
                    {
                        m.SetTexture("_MainTex", MiniHeadTextures.First(t => t.name.Contains("mayu")));
                    }
                }
                else
                {
                    if (IsMob)
                    {
                        if (m.name.EndsWith("eye"))
                        {
                            m.SetTexture("_MainTex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("eye0")));
                            m.SetTexture("_High0Tex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("hi00")));
                            m.SetTexture("_High1Tex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("hi01")));
                            m.SetTexture("_High2Tex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("hi02")));
                            m.SetTexture("_MaskColorTex", textures.LastOrDefault(t => t.name.Contains("_eye") && t.name.EndsWith("area")));
                            SetMaskColor(m, MobHeadColor, "eye", false);
                        }
                        if (m.name.EndsWith("face"))
                        {
                            m.SetTexture("_MainTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("diff")));
                            m.SetTexture("_ToonMap", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("shad_c")));
                            m.SetTexture("_TripleMaskMap", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("base")));
                            m.SetTexture("_OptionMaskMap", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("ctrl")));
                            m.SetTexture("_MaskColorTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("area") && !t.name.Contains("_eye")));
                            SetMaskColor(m, MobHeadColor, "mayu", true);
                        }
                        if (m.name.EndsWith("mayu"))
                        {
                            m.SetTexture("_MainTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("diff")));
                            m.SetTexture("_MaskColorTex", textures.LastOrDefault(t => t.name.Contains("_face") && t.name.EndsWith("area") && !t.name.Contains("_eye")));
                            SetMaskColor(m, MobHeadColor, "mayu", true);
                        }
                    }

                    //Glasses's shader need to change manually.
                    if (r.name.Contains("Hair") && r.name.Contains("Alpha"))
                    {
                        m.shader = UmaAssetManager.AlphaShader;
                    }

                    //Blush Setting
                    if (r.name.Contains("Cheek"))
                    {
                        r.gameObject.SetActive(false);
                        if (IsMob)
                        {
                            CheekTex_0 = MobHeadTextures.FindLast(a => a.name.Contains("cheek0"));
                            CheekTex_1 = MobHeadTextures.FindLast(a => a.name.Contains("cheek1"));
                        }
                        else
                        {
                            var table = Head.GetComponent<AssetHolder>()._assetTable;
                            CheekTex_0 = table["cheek0"] as Texture;
                            CheekTex_1 = table["cheek1"] as Texture;
                        }
                    }
                    switch (m.shader.name)
                    {
                        case "Gallop/3D/Chara/MultiplyCheek":
                            m.shader = UmaAssetManager.CheekShader; ;
                            break;
                        case "Gallop/3D/Chara/ToonFace/TSER":
                            m.shader = UmaAssetManager.FaceShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            m.SetColor("_RimColor", new Color(0, 0, 0, 0));
                            break;
                        case "Gallop/3D/Chara/ToonEye/T":
                            m.shader = UmaAssetManager.EyeShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            break;
                        case "Gallop/3D/Chara/ToonHair/TSER":
                            m.shader = UmaAssetManager.HairShader;
                            m.SetFloat("_CylinderBlend", 0.25f);
                            break;
                        case "Gallop/3D/Chara/ToonMayu":
                            m.shader = UmaAssetManager.EyebrowShader;
                            break;
                        default:
                            Debug.Log(m.shader.name);
                            // m.shader = Shader.Find("Nars/UmaMusume/Body");
                            break;
                    }
                }
            }
        }

        //shader effect
        var assetholder = head.GetComponent<AssetHolder>();
        if (assetholder)
        {
            ShaderEffectData = assetholder._assetTable["chara_shader_effect"] as CharaShaderEffectData;
            if (ShaderEffectData)
            {
                ShaderEffectData.Initialize();
            }
        }
    }

    public void LoadHair(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        GameObject hair = Instantiate(go, transform);
        Hair = hair;
        var textures = MobHeadTextures;
        foreach (Renderer r in hair.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {

                //Glasses's shader need to change manually.
                if (r.name.Contains("Hair") && r.name.Contains("Alpha"))
                {
                    m.shader = UmaAssetManager.AlphaShader;
                }

                if (m.name.EndsWith("_hair"))
                {
                    m.SetTexture("_MainTex", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("diff")));
                    m.SetTexture("_ToonMap", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("shad_c")));
                    m.SetTexture("_TripleMaskMap", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("base")));
                    m.SetTexture("_OptionMaskMap", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("ctrl")));
                    m.SetTexture("_MaskColorTex", textures.FirstOrDefault(t => t.name.Contains("_hair") && t.name.EndsWith("area")));
                    SetMaskColor(m, MobHeadColor, "hair", true);
                }

                switch (m.shader.name)
                {
                    case "Gallop/3D/Chara/ToonHair/TSER":
                        m.shader = UmaAssetManager.HairShader;
                        m.SetFloat("_CylinderBlend", 0.25f);
                        break;
                    default:
                        Debug.Log(m.shader.name);
                        // m.shader = Shader.Find("Nars/UmaMusume/Body");
                        break;
                }
            }
        }
    }

    public void LoadTail(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        Tail = Instantiate(go, transform);
        var textures = TailTextures;
        foreach (Renderer r in Tail.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                m.SetTexture("_MainTex", textures.FirstOrDefault(t => t.name.EndsWith("diff")));
                m.SetTexture("_ToonMap", textures.FirstOrDefault(t => t.name.Contains("shad")));
                m.SetTexture("_TripleMaskMap", textures.FirstOrDefault(t => t.name.Contains("base")));
                m.SetTexture("_OptionMaskMap", textures.FirstOrDefault(t => t.name.Contains("ctrl")));
                if (IsMob)
                {
                    SetMaskColor(m, MobHeadColor, "tail", true);
                }
            }
        }
    }

    public void LoadTear(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        if (go.name.EndsWith("000"))
        {
            TearPrefab_0 = go;
        }
        else if (go.name.EndsWith("001"))
        {
            TearPrefab_1 = go;
        }
    }

    private void SetMaskColor(Material mat, DataRow colordata, bool IsMob)
    {
        mat.EnableKeyword("USE_MASK_COLOR");
        Color c1, c2, c3, c4, c5, c6, t1, t2, t3, t4, t5, t6;
        if (IsMob)
        {
            ColorUtility.TryParseHtmlString(colordata["color_r1"].ToString(), out c1);
            ColorUtility.TryParseHtmlString(colordata["color_r2"].ToString(), out c2);
            ColorUtility.TryParseHtmlString(colordata["color_g1"].ToString(), out c3);
            ColorUtility.TryParseHtmlString(colordata["color_g2"].ToString(), out c4);
            ColorUtility.TryParseHtmlString(colordata["color_b1"].ToString(), out c5);
            ColorUtility.TryParseHtmlString(colordata["color_b2"].ToString(), out c6);
            ColorUtility.TryParseHtmlString(colordata["toon_color_r1"].ToString(), out t1);
            ColorUtility.TryParseHtmlString(colordata["toon_color_r2"].ToString(), out t2);
            ColorUtility.TryParseHtmlString(colordata["toon_color_g1"].ToString(), out t3);
            ColorUtility.TryParseHtmlString(colordata["toon_color_g2"].ToString(), out t4);
            ColorUtility.TryParseHtmlString(colordata["toon_color_b1"].ToString(), out t5);
            ColorUtility.TryParseHtmlString(colordata["toon_color_b2"].ToString(), out t6);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#" + colordata["image_color_main"].ToString(), out c1);
            ColorUtility.TryParseHtmlString("#" + colordata["image_color_sub"].ToString(), out c2);
            ColorUtility.TryParseHtmlString("#" + colordata["ui_color_sub"].ToString(), out c3);
            ColorUtility.TryParseHtmlString("#" + colordata["ui_color_sub"].ToString(), out c4);
            ColorUtility.TryParseHtmlString("#" + colordata["ui_training_color_1"].ToString(), out c5);
            ColorUtility.TryParseHtmlString("#" + colordata["ui_training_color_2"].ToString(), out c6);
            float toonstrength = 0.8f;
            t1 = c1 * toonstrength;
            t2 = c2 * toonstrength;
            t3 = c3 * toonstrength;
            t4 = c4 * toonstrength;
            t5 = c5 * toonstrength;
            t6 = c6 * toonstrength;
        }

        mat.SetColor("_MaskColorR1", c1);
        mat.SetColor("_MaskColorR2", c2);
        mat.SetColor("_MaskColorG1", c3);
        mat.SetColor("_MaskColorG2", c4);
        mat.SetColor("_MaskColorB1", c5);
        mat.SetColor("_MaskColorB2", c6);
        mat.SetColor("_MaskToonColorR1", t1);
        mat.SetColor("_MaskToonColorR2", t2);
        mat.SetColor("_MaskToonColorG1", t3);
        mat.SetColor("_MaskToonColorG2", t4);
        mat.SetColor("_MaskToonColorB1", t5);
        mat.SetColor("_MaskToonColorB2", t6);
    }

    private void SetMaskColor(Material mat, DataRow colordata, string prefix, bool hastoon)
    {
        mat.EnableKeyword("USE_MASK_COLOR");
        Color c1, c2, c3, c4, c5, c6, t1, t2, t3, t4, t5, t6;
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_r1"].ToString(), out c1);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_r2"].ToString(), out c2);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_g1"].ToString(), out c3);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_g2"].ToString(), out c4);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_b1"].ToString(), out c5);
        ColorUtility.TryParseHtmlString(colordata[$"{prefix}_color_b2"].ToString(), out c6);
        mat.SetColor("_MaskColorR1", c1);
        mat.SetColor("_MaskColorR2", c2);
        mat.SetColor("_MaskColorG1", c3);
        mat.SetColor("_MaskColorG2", c4);
        mat.SetColor("_MaskColorB1", c5);
        mat.SetColor("_MaskColorB2", c6);
        if (hastoon)
        {
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_r1"].ToString(), out t1);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_r2"].ToString(), out t2);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_g1"].ToString(), out t3);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_g2"].ToString(), out t4);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_b1"].ToString(), out t5);
            ColorUtility.TryParseHtmlString(colordata[$"{prefix}_toon_color_b2"].ToString(), out t6);
            mat.SetColor("_MaskToonColorR1", t1);
            mat.SetColor("_MaskToonColorR2", t2);
            mat.SetColor("_MaskToonColorG1", t3);
            mat.SetColor("_MaskToonColorG2", t4);
            mat.SetColor("_MaskToonColorB1", t5);
            mat.SetColor("_MaskToonColorB2", t6);
        }
    }

    public void LoadPhysics(UmaDatabaseEntry entry)
    {
        GameObject go = entry.Get<GameObject>();
        if (!PhysicsContainer)
        {
            PhysicsContainer = new GameObject("PhysicsController");
            PhysicsContainer.transform.SetParent(transform);
        }
        Instantiate(go, PhysicsContainer.transform);
    }

    public void LoadAnimation(UmaDatabaseEntry entry)
    {
        if (UI.LiveTime)
        {
            return;
        }

        var aClip = entry.Get<AnimationClip>();

        if (UmaAnimator)
        {
            Debug.Log("LiveTime" + UI.LiveTime.ToString());
            aClip.name = entry.Name; // Need a complete path to find dependencies
            LoadAnimation(aClip);
            return;
        }

        if (aClip.name.Contains("tear"))
        {
            return;
        }
    }

    private void LoadAnimation(AnimationClip clip)
    {
        if (clip.name.EndsWith("_s"))
        {
            OverrideController["clip_s"] = clip;
        }
        else if (clip.name.EndsWith("_e"))
        {
            OverrideController["clip_e"] = clip;
        }
        else if (clip.name.Contains("tail"))
        {
            if (IsMini) return;
            UpBodyReset();
            OverrideController["clip_t"] = clip;
            UmaAnimator.Play("motion_t", 1, 0);
        }
        else if (clip.name.EndsWith("_face"))
        {
            if (IsMini) return;
            LoadFaceAnimation(clip);
        }
        else if (clip.name.EndsWith("_ear"))
        {
            if (IsMini) return;
            LoadEarAnimation(clip);
        }
        else if (clip.name.EndsWith("_pos"))
        {
            if (IsMini) return;
            OverrideController["clip_p"] = clip;
            UmaAnimator.Play("motion_1", 2, 0);
        }
        else if (clip.name.EndsWith("_cam"))
        {
            Builder.SetPreviewCamera(clip);
        }
        else if (clip.name.Contains("_loop"))
        {
            UpBodyReset();
            if (isAnimatorControl && FaceDrivenKeyTarget)
            {
                FaceDrivenKeyTarget.ResetLocator();
                isAnimatorControl = false;
            }

            if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/facial")}_face", out UmaDatabaseEntry entry))
            {
                LoadAnimation(entry);
            }

            if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/facial")}_ear", out entry))
            {
                LoadAnimation(entry);
            }

            UmaDatabaseEntry motion_e = null, motion_s = null;
            if (Main.AbList.TryGetValue(clip.name.Replace("_loop", "_s"), out motion_s))
            {
                LoadAnimation(motion_s);
            }

            if (OverrideController["clip_2"].name.Contains("_loop"))
            {
                if (!OverrideController["clip_2"].name.Contains("hom_"))//home end animation not for interpolation
                {
                    if (Main.AbList.TryGetValue(OverrideController["clip_2"].name.Replace("_loop", "_e"), out motion_e))
                    {
                        LoadAnimation(motion_e);
                    }
                }
            }

            Builder.SetPreviewCamera(null);
            OverrideController["clip_1"] = OverrideController["clip_2"];
            OverrideController["clip_2"] = clip;
            UmaAnimator.Play("motion_1", -1, 0);
            UmaAnimator.SetTrigger((motion_s != null && motion_e != null) ? "next_e" : ((motion_s != null) ? "next_s" : "next"));
        }
        else
        {
            if (FaceDrivenKeyTarget)
            {
                FaceDrivenKeyTarget.ResetLocator();
                isAnimatorControl = false;
            }
            UpBodyReset();
            UmaAnimator.Rebind();
            OverrideController["clip_2"] = clip;
            // If Cut-in, play immediately without state interpolation
            if (clip.name.Contains("crd") || clip.name.Contains("res_chr"))
            {
                if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/facial")}_face", out UmaDatabaseEntry facialMotion))
                {
                    LoadAnimation(facialMotion);
                }

                if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/facial")}_ear", out UmaDatabaseEntry earMotion))
                {
                    LoadAnimation(earMotion);
                }

                if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/camera")}_cam", out UmaDatabaseEntry cameraMotion))
                {
                    LoadAnimation(cameraMotion);
                }

                if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/position")}_pos", out UmaDatabaseEntry posMotion))
                {
                    LoadAnimation(posMotion);
                }

                if (IsMini)
                {
                    Builder.SetPreviewCamera(null);
                }

                if (clip.name.Contains("_cti_crd"))
                {
                    var dir = Path.GetDirectoryName(clip.name).Replace("\\", "/");
                    string[] param = Path.GetFileName(clip.name).Split('_');
                    if (param.Length > 4)
                    {
                        int index = int.Parse(param[4]);
                        if (index == 1)
                        {
                            var cur = index + 1;
                            while (true)
                            {
                                var nextSearch = $"{dir}/{param[0]}_{param[1]}_{param[2]}_{param[3]}_{cur.ToString().PadLeft(2, '0')}";
                                if (Main.AbList.TryGetValue(nextSearch, out UmaDatabaseEntry result))
                                {
                                    UmaAssetManager.LoadAssetBundle(result);
                                    cur++;
                                }
                                else break;
                            }
                        }

                        index++;
                        var next = $"{dir}/{param[0]}_{param[1]}_{param[2]}_{param[3]}_{index.ToString().PadLeft(2, '0')}";
                        if (Main.AbList.TryGetValue(next, out UmaDatabaseEntry nextMotion))
                        {
                            var aevent = new AnimationEvent
                            {
                                time = clip.length * 0.99f,
                                stringParameter = (nextMotion != null ? nextMotion.Name : null),
                                functionName = (nextMotion != null ? "SetNextAnimationCut" : "SetEndAnimationCut")
                            };
                            clip.AddEvent(aevent);
                        }
                    }
                }

                UmaAnimator.Play("motion_2", 0, 0);
            }
            else
            {
                Builder.SetPreviewCamera(null);

                //Some animations have facial animation
                if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/facial")}_face", out UmaDatabaseEntry facialMotion))
                {
                    LoadAnimation(facialMotion);
                }

                if (Main.AbList.TryGetValue($"{clip.name.Replace("/body", "/facial")}_ear", out UmaDatabaseEntry earMotion))
                {
                    LoadAnimation(earMotion);
                }

                UmaAnimator.Play("motion_2", 0, 0);
            }
        }
    }

    private void LoadFaceAnimation(AnimationClip clip)
    {
        if (clip.name.Contains("_s"))
        {
            FaceOverrideController["clip_s"] = clip;
        }
        else if (clip.name.Contains("_e"))
        {
            FaceOverrideController["clip_e"] = clip;
        }
        else if (clip.name.Contains("_loop"))
        {
            isAnimatorControl = true;
            FaceDrivenKeyTarget.ResetLocator();
            UmaDatabaseEntry motion_e = null;
            UmaDatabaseEntry motion_s = null;
            if (Main.AbList.TryGetValue(clip.name.Replace("_loop", "_s"), out motion_s))
            {
                LoadAnimation(motion_s);
            }

            if (FaceOverrideController["clip_2"].name.Contains("_loop"))
            {
                if (!FaceOverrideController["clip_2"].name.Contains("hom_"))//home end animation not for interpolation
                {
                    if (Main.AbList.TryGetValue(FaceOverrideController["clip_2"].name.Replace("_loop", "_e"), out motion_e))
                    {
                        LoadAnimation(motion_e);
                    }
                }
            }

            FaceOverrideController["clip_1"] = FaceOverrideController["clip_2"];
            FaceOverrideController["clip_2"] = clip;
            UmaFaceAnimator.Play("motion_1", 0, 0);
            UmaFaceAnimator.SetTrigger((motion_s != null && motion_e != null) ? "next_e" : ((motion_s != null) ? "next_s" : "next"));
        }
        else
        {
            isAnimatorControl = true;
            FaceDrivenKeyTarget.ResetLocator();
            FaceOverrideController["clip_2"] = clip;
            UmaFaceAnimator.Play("motion_2", 0, 0);
        }
    }

    private void LoadEarAnimation(AnimationClip clip)
    {
        if (clip.name.Contains("_s"))
        {
            FaceOverrideController["clip_s_ear"] = clip;
        }
        else if (clip.name.Contains("_e"))
        {
            FaceOverrideController["clip_e_ear"] = clip;
        }
        else if (clip.name.Contains("_loop"))
        {
            UmaDatabaseEntry motion_e = null;
            UmaDatabaseEntry motion_s = null;
            if (Main.AbList.TryGetValue(clip.name.Replace("_loop", "_s"), out motion_s))
            {
                LoadAnimation(motion_s);
            }

            if (FaceOverrideController["clip_2_ear"].name.Contains("_loop"))
            {
                if (!FaceOverrideController["clip_2_ear"].name.Contains("hom_"))//home end animation not for interpolation
                {
                    if (Main.AbList.TryGetValue(FaceOverrideController["clip_2_ear"].name.Replace("_loop", "_e"), out motion_e))
                    {
                        LoadAnimation(motion_e);
                    }
                }
            }

            FaceOverrideController["clip_1_ear"] = FaceOverrideController["clip_2_ear"];
            FaceOverrideController["clip_2_ear"] = clip;
            UmaFaceAnimator.Play("motion_1", 1, 0);
            UmaFaceAnimator.SetTrigger((motion_s != null && motion_e != null) ? "next_e_ear" : ((motion_s != null) ? "next_s_ear" : "next_ear"));
        }
        else
        {
            if (FaceOverrideController["clip_2"].name == "clip_2")
            {
                isAnimatorControl = true;
                FaceDrivenKeyTarget.ResetLocator();
            }
            FaceOverrideController["clip_2_ear"] = clip;
            UmaFaceAnimator.Play("motion_2", 1, 0);
        }
    }

    public void SetNextAnimationCut(string cutName)
    {
        UmaViewerMain.Instance.AbList.TryGetValue(cutName, out UmaDatabaseEntry asset);
        LoadAnimation(asset);
    }

    public void SetEndAnimationCut()
    {
        UmaViewerUI.Instance.AnimationPause();
    }
}
