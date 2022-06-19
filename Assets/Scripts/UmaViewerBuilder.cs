using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class UmaViewerBuilder : MonoBehaviour
{
    public static UmaViewerBuilder Instance;
    UmaViewerMain Main => UmaViewerMain.Instance;
    UmaViewerUI UI => UmaViewerUI.Instance;

    public List<AssetBundle> Loaded;
    public List<Shader> ShaderList = new List<Shader>();
    public Material InvisibleMaterial;
    private UmaContainer CurrentContainer;

    public AnimatorOverrideController OverrideController;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator LoadUma(int id, string costumeId, bool mini)
    {
        if(CurrentContainer != null)
        {
            Destroy(CurrentContainer);
        }
        CurrentContainer = new GameObject($"{id}_{costumeId}").AddComponent<UmaContainer>();

        foreach (var bundle in Main.LoadedBundles)
        {
            bundle.Value.Unload(true);
        }
        Main.LoadedBundles.Clear();
        UI.LoadedAssetsClear();

        yield return UmaViewerDownload.DownloadText($"https://www.tracenacademy.com/api/CharaData/{id}", txt =>
        {
            Debug.Log(txt);
            CurrentContainer.CharaData = JObject.Parse(txt);
            if (mini)
            {
                LoadMiniUma(id, costumeId);
            }
            else
            {
                LoadNormalUma(id, costumeId);
            }
        });
    }

    private void LoadNormalUma(int id, string costumeId)
    {
        JObject charaData = CurrentContainer.CharaData;
        bool genericCostume = CurrentContainer.IsGeneric = costumeId.Length >= 4;
        string skin = (string)charaData["skin"],
               height = (string)charaData["height"],
               socks = (string)charaData["socks"],
               bust = (string)charaData["bust"],
               costumeIdShort = "";

        UmaDatabaseEntry asset = null;
        if (genericCostume)
        {
            costumeIdShort = costumeId.Remove(costumeId.LastIndexOf('_'));
            CurrentContainer.VarCostumeIdShort = costumeIdShort;
            CurrentContainer.VarCostumeIdLong = costumeId;
            CurrentContainer.VarBust = bust;
            CurrentContainer.VarSkin = skin;
            CurrentContainer.VarSocks = socks;
            CurrentContainer.VarHeight = height;
            string body = UmaDatabaseController.BodyPath + $"bdy{costumeIdShort}/pfb_bdy{costumeId}_{height}_{0}_{bust}";
            Debug.Log("Looking for " + body);
            asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == body);
        }
        else asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == UmaDatabaseController.BodyPath + $"bdy{id}_{costumeId}/pfb_bdy{id}_{costumeId}");

        if (asset == null)
        {
            Debug.LogError("No body, can't load!");
            return;
        }
        else if (genericCostume)
        {
            string texPattern1 = "", texPattern2 = "";
            switch (costumeId.Split('_')[0])
            {
                case "0001":
                    texPattern1 = $"tex_bdy{costumeIdShort}_00_{skin}_{socks}_0{bust}";
                    texPattern2 = $"tex_bdy{costumeIdShort}_00_0_{socks}_00_";
                    break;
                case "0003":
                    texPattern1 = $"tex_bdy{costumeIdShort}_00_{skin}_{bust}";
                    texPattern2 = $"tex_bdy{costumeIdShort}_00_0_{bust}";
                    break;
                case "0006": //last var is color?
                    texPattern1 = $"tex_bdy{costumeId}_{skin}_{bust}_0{0}";
                    texPattern2 = $"tex_bdy{costumeId}_0_{bust}_00_";
                    break;
                default:
                    texPattern1 = $"tex_bdy{costumeId}_{skin}_{bust}";
                    texPattern2 = $"tex_bdy{costumeId}_0_{bust}";
                    break;
            }
            Debug.Log(texPattern1 + " " + texPattern2);
            //Load Body Textures
            foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith(UmaDatabaseController.BodyPath) && (a.Name.Contains(texPattern1) || a.Name.Contains(texPattern2))))
            {
                RecursiveLoadAsset(asset1);
            }
            //Load Body
            RecursiveLoadAsset(asset);
        }
        else 
            RecursiveLoadAsset(asset);

        string head = UmaDatabaseController.HeadPath + $"chr{id}_{costumeId}/pfb_chr{id}_{costumeId}";
        asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == head);

        //Some costumes don't have custom heads
        if (costumeId != "00" && asset == null)
        {
            asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == UmaDatabaseController.HeadPath + $"chr{id}_00/pfb_chr{id}_00");
        }
        if (asset != null)
        {
            //Load Hair Textures
            foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith($"{UmaDatabaseController.HeadPath}chr{id}_{costumeId}/textures")))
            {
                RecursiveLoadAsset(asset1);
            }
            //Load Head
            RecursiveLoadAsset(asset);
        }

        int tailId = (int)charaData["tailModelId"];
        if (tailId != 0)
        {
            string tailName = $"tail{tailId.ToString().PadLeft(4, '0')}_00";
            string tailPath = $"3d/chara/tail/{tailName}/";
            string tailPfb = tailPath + $"pfb_{tailName}";
            asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == tailPfb);
            if (asset != null)
            {
                foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith($"{tailPath}textures/tex_{tailName}_{id}")))
                {
                    RecursiveLoadAsset(asset1);
                }
                RecursiveLoadAsset(asset);
            }
            else
            {
                Debug.Log("no tail");
            }
        }
    }

    private void LoadMiniUma(int id, string costumeId)
    {
        JObject charaData = CurrentContainer.CharaData;
        CurrentContainer.IsMini = true;
        bool isGeneric = CurrentContainer.IsGeneric = costumeId.Length >= 4;
        string skin = (string)charaData["skin"],
               height = (string)charaData["height"],
               socks = (string)charaData["socks"],
               bust = (string)charaData["bust"],
               costumeIdShort = "";
        bool customHead = true;

        UmaDatabaseEntry asset = null;
        if (isGeneric)
        {
            costumeIdShort = costumeId.Remove(costumeId.LastIndexOf('_'));
            string body = $"3d/chara/mini/body/mbdy{costumeIdShort}/pfb_mbdy{costumeId}_0";
            asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == body);
        }
        else asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == $"3d/chara/mini/body/mbdy{id}_{costumeId}/pfb_mbdy{id}_{costumeId}");
        if (asset == null)
        {
            Debug.LogError("No body, can't load!");
            return;
        }
        else if (isGeneric)
        {
            string texPattern1 = "";
            switch (costumeId.Split('_')[0])
            {
                case "0003":
                    texPattern1 = $"tex_mbdy{costumeIdShort}_00_{skin}_{0}";
                    break;
                default:
                    texPattern1 = $"tex_mbdy{costumeId}_{skin}_{0}";
                    break;
            }
            //Load Body Textures
            foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith("3d/chara/mini/body/") && a.Name.Contains(texPattern1)))
            {
                RecursiveLoadAsset(asset1);
            }
            //Load Body
            RecursiveLoadAsset(asset);
        }
        else
            RecursiveLoadAsset(asset);

        string hair = $"3d/chara/mini/head/mchr{id}_{costumeId}/pfb_mchr{id}_{costumeId}_hair";
        asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == hair);
        if (costumeId != "00" && asset == null)
        {
            customHead = false;
            asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == $"3d/chara/mini/head/mchr{id}_00/pfb_mchr{id}_00_hair");
        }
        if (asset != null)
        {
            //Load Hair Textures
            if (customHead)
            {
                foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith($"3d/chara/mini/head/mchr{id}_{costumeId}/textures")))
                {
                    RecursiveLoadAsset(asset1);
                }
            }
            else
            {
                foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith($"3d/chara/mini/head/mchr{id}_00/textures")))
                {
                    RecursiveLoadAsset(asset1);
                }
            }

            //Load Hair
            RecursiveLoadAsset(asset);
        }

        string head = $"3d/chara/mini/head/mchr0001_00/pfb_mchr0001_00_face0";
        asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == head);
        if (asset != null)
        {
            //Load Head Textures
            foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith($"3d/chara/mini/head/mchr0001_00/textures/tex_mchr0001_00_face0_{skin}")))
            {
                RecursiveLoadAsset(asset1);
            }
            //Load Head
            RecursiveLoadAsset(asset);
        }
    }

    private void RecursiveLoadAsset(UmaDatabaseEntry entry)
    {
        if (!string.IsNullOrEmpty(entry.Prerequisites))
        {
            foreach (string prerequisite in entry.Prerequisites.Split(';'))
            {
                RecursiveLoadAsset(Main.AbList.FirstOrDefault(ab => ab.Name == prerequisite));
            }
        }
        LoadAsset(entry);
    }


    public void LoadAsset(UmaDatabaseEntry entry)
    {
        Debug.Log("Loading " + entry.Name);
        if (Main.LoadedBundles.ContainsKey(entry.Name)) return;

        string filePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low"}\\Cygames\\umamusume\\dat\\{entry.Url.Substring(0, 2)}\\{entry.Url}";
        if (File.Exists(filePath))
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
            if (bundle == null)
            {
                Debug.Log(filePath + " exists and doesn't work");
                return;
            }
            Main.LoadedBundles.Add(entry.Name, bundle);
            UI.LoadedAssetsAdd(Path.GetFileName(entry.Name) + " : " + Path.GetFileName(entry.Url));
            LoadBundle(bundle);
        }
    }

    private void LoadBundle(AssetBundle bundle)
    {
        if (bundle.name == "shader.a")
        {
            if(Main.ShadersLoaded) return;
            else Main.ShadersLoaded = true;
        }
                
        foreach (string name in bundle.GetAllAssetNames())
        {
            object asset = bundle.LoadAsset(name);
            if (asset == null) { continue; }
            if (asset.GetType() == typeof(AnimationClip))
            {
                if (CurrentContainer.Body)
                {
                    AnimationClip bbb = asset as AnimationClip;
                    bbb.wrapMode = WrapMode.Loop;
                    CurrentContainer.OverrideController["clip"] = bbb;
                    CurrentContainer.UmaAnimator.Play("clip");
                    UnloadBundle(bundle, false);
                }
                else
                {
                    UnloadBundle(bundle, true);
                }
            }
            else if (asset.GetType() == typeof(GameObject))
            {
                if (bundle.name.Contains("/head/"))
                {
                    LoadHead(asset as GameObject);
                }
                else if (bundle.name.Contains("/body/"))
                {
                    LoadBody(asset as GameObject);
                }
                else if (bundle.name.Contains("/tail/"))
                {
                    LoadTail(asset as GameObject);
                }
                else
                {
                    UnloadBundle(bundle, true);
                }
                //else if (aaa.GetType() == typeof(FaceDrivenKeyTarget))
                //{
                //    Debug.LogError(name);
                //}
            }
            else if (asset.GetType() == typeof(Shader))
            {
                ShaderList.Add(asset as Shader);
            }
            else if (asset.GetType() == typeof(Texture2D))
            {
                if (bundle.name.Contains("/mini/head"))
                {
                    CurrentContainer.MiniHeadTextures.Add(asset as Texture2D);
                }
                else if (bundle.name.Contains("/tail/"))
                {
                    CurrentContainer.TailTextures.Add(asset as Texture2D);
                }
                else if (bundle.name.Contains("bdy0"))
                {
                    CurrentContainer.GenericBodyTextures.Add(asset as Texture2D);
                }
            }
        }
    }

    private void LoadBody(GameObject go)
    {
        CurrentContainer.Body = Instantiate(go, CurrentContainer.transform);
        CurrentContainer.UmaAnimator = CurrentContainer.Body.GetComponent<Animator>();
        CurrentContainer.UmaAnimator.runtimeAnimatorController = CurrentContainer.OverrideController = Instantiate(OverrideController);

        if (CurrentContainer.IsGeneric)
        {
            List<Texture2D> textures = CurrentContainer.GenericBodyTextures;
            string costumeIdShort = CurrentContainer.VarCostumeIdShort,
                   costumeIdLong = CurrentContainer.VarCostumeIdLong,
                   height = CurrentContainer.VarHeight,
                   skin = CurrentContainer.VarSkin,
                   socks = CurrentContainer.VarSocks,
                   bust = CurrentContainer.VarBust;

            foreach (Renderer r in CurrentContainer.Body.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    string mainTex = "", toonMap = "", tripleMap = "", optionMap = "";
                    if (CurrentContainer.IsMini)
                    {
                        m.SetTexture("_MainTex", textures[0]);
                    }
                    else
                    {
                        m.shader = Shader.Find("Nars/UmaMusume/Body");
                        switch (costumeIdShort.Split('_')[0]) //costume ID
                        {
                            case "0001":
                                mainTex = $"tex_bdy{costumeIdShort}_00_{skin}_{socks}_{bust.PadLeft(2, '0')}_diff";
                                toonMap = $"tex_bdy{costumeIdShort}_00_{skin}_{socks}_{bust.PadLeft(2, '0')}_shad_c";
                                tripleMap = $"tex_bdy{costumeIdShort}_00_0_{socks}_00_base";
                                optionMap = $"tex_bdy{costumeIdShort}_00_0_{socks}_00_ctrl";
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
                    }
                }
            }
        }
        else
        {

            foreach (Renderer r in CurrentContainer.Body.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (!m.shader.name.Contains("Mini"))
                    {
                        m.shader = Shader.Find("Nars/UmaMusume/Body");
                    }
                }
            }
        }
    }

    private void LoadHead(GameObject go)
    {
        GameObject head = Instantiate(go, CurrentContainer.transform);
        CurrentContainer.Heads.Add(head);
        CurrentContainer.HeadNeckBones.Add(UmaContainer.FindBoneInChildren(head.transform, "Neck"));
        CurrentContainer.HeadHeadBones.Add(UmaContainer.FindBoneInChildren(head.transform, "Head"));

        foreach (Renderer r in head.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                if (head.name.Contains("mchr"))
                {
                    if(r.name == "M_Face")
                    {
                        m.SetTexture("_MainTex", CurrentContainer.MiniHeadTextures.First(t => t.name.Contains("face") && t.name.Contains("diff")));
                    }
                    if (r.name == "M_Cheek")
                    {
                        m.CopyPropertiesFromMaterial(InvisibleMaterial);
                        m.SetTexture("_MainTex", CurrentContainer.MiniHeadTextures.First(t => t.name.Contains("cheek")));
                    }
                    if (r.name == "M_Mouth")
                    {
                        m.SetTexture("_MainTex", CurrentContainer.MiniHeadTextures.First(t => t.name.Contains("mouth")));
                    }
                    if (r.name == "M_Eye")
                    {
                        m.SetTexture("_MainTex", CurrentContainer.MiniHeadTextures.First(t => t.name.Contains("eye")));
                    }
                    if (r.name.StartsWith("M_Mayu_"))
                    {
                        m.SetTexture("_MainTex", CurrentContainer.MiniHeadTextures.First(t => t.name.Contains("mayu")));
                    }
                }
                else
                {
                    switch (m.shader.name)
                    {
                        case "Gallop/3D/Chara/MultiplyCheek":
                            m.CopyPropertiesFromMaterial(InvisibleMaterial);
                            break;
                        case "Gallop/3D/Chara/ToonEye/T":
                        case "Nars/UmaMusume/Eyes":
                            m.shader = Shader.Find("Nars/UmaMusume/Eyes");
                            foreach(var tex in m.GetTexturePropertyNames())
                            {
                                if(m.GetTexture(tex) != null)
                                {
                                    m.GetTexture(tex).wrapMode = TextureWrapMode.Repeat;
                                }
                            }
                            break;
                        case "Gallop/3D/Chara/ToonFace/TSER":
                        case "Nars/UmaMusume/Face":
                            m.shader = Shader.Find("Nars/UmaMusume/Face");
                            break;
                        case "Gallop/3D/Chara/ToonHair/TSER":
                            m.shader = Shader.Find("Nars/UmaMusume/Body");
                            break;
                        default:
                            Debug.LogError(m.shader.name);
                            // m.shader = Shader.Find("Nars/UmaMusume/Body");
                            break;
                    }
                }
            }
        }
    }

    private void LoadTail(GameObject gameObject)
    {
        Transform hHip = UmaContainer.FindBoneInChildren(CurrentContainer.Body.transform, "Hip");
        if(hHip == null) return;
        CurrentContainer.Tail = Instantiate(gameObject, hHip);
        var textures = CurrentContainer.TailTextures;
        foreach (Renderer r in CurrentContainer.Tail.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                m.shader = Shader.Find("Nars/UmaMusume/Body");
                m.SetTexture("_MainTex", textures.FirstOrDefault(t => t.name.EndsWith("diff")));
                m.SetTexture("_ToonMap", textures.FirstOrDefault(t => t.name.Contains("shad")));
                m.SetTexture("_TripleMaskMap", textures.FirstOrDefault(t => t.name.Contains("base")));
                m.SetTexture("_OptionMaskMap", textures.FirstOrDefault(t => t.name.Contains("ctrl")));
            }
        }
    }

    void UnloadBundle(AssetBundle bundle, bool unloadAllObjects)
    {
        var entry = Main.LoadedBundles.FirstOrDefault(b=>b.Value == bundle);
        if (entry.Key != null)
        {
            Main.LoadedBundles.Remove(entry.Key);
        }
        bundle.Unload(unloadAllObjects);
    }
}
