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

    public List<AssetBundle> Loaded;
    public List<Shader> ShaderList = new List<Shader>();
    public Material InvisibleMaterial;
    private UmaContainer CurrentContainer;

    public AnimatorOverrideController OverrideController;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator LoadUma(int id, string costumeId)
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

        yield return UmaViewerDownload.DownloadText($"https://www.tracenacademy.com/api/CharaData/{id}", txt =>
        {
            Debug.Log(txt);

            string body = UmaDatabaseController.BodyPath + $"bdy{id}_{costumeId}/pfb_bdy{id}_{costumeId}";
            UmaDatabaseEntry asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == body);
            if (asset == null)
            {
                Debug.LogError("No body, can't load!");
                return;
            }
            RecursiveLoadAsset(asset);

            string head = UmaDatabaseController.HeadPath + $"chr{id}_{costumeId}/pfb_chr{id}_{costumeId}";
            asset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name == head);
            if (asset != null)
            {
                //Load Hair Textures
                foreach (var asset1 in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith($"{UmaDatabaseController.HeadPath}chr{id}_{costumeId}/textures")))
                {
                    RecursiveLoadAsset(asset1);
                }
                //Load Hair
                RecursiveLoadAsset(asset);
            }

            int tailId = (int)JObject.Parse(txt)["tailModelId"];
            if(tailId != 0)
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
        });
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
                    LoadBody(bundle.name, asset as GameObject);
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
                if (bundle.name.Contains("/tail/"))
                {
                    CurrentContainer.TailTextures.Add(asset as Texture2D);
                }
            }
        }
    }

    private void LoadBody(string bundleName, GameObject go)
    {
        string umaID = "";
        string[] nameSplit = bundleName.Split(new char[] { '/', '_' });
        foreach (string s in nameSplit)
        {
            if (s.Contains("bdy"))
            {
                umaID = s.Substring(s.Length - 4, 4);
                break;
            }
        }

        CurrentContainer.Body = Instantiate(go, CurrentContainer.transform);
        CurrentContainer.UmaAnimator = CurrentContainer.Body.GetComponent<Animator>();
        CurrentContainer.UmaAnimator.runtimeAnimatorController = CurrentContainer.OverrideController = Instantiate(OverrideController);

        foreach (Renderer r in CurrentContainer.Body.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                m.shader = Shader.Find("Nars/UmaMusume/Body");
            }
        }
    }

    private void LoadHead(GameObject go)
    {
        CurrentContainer.Head = Instantiate(go, CurrentContainer.transform);

        foreach (Renderer r in CurrentContainer.Head.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.sharedMaterials)
            {
                switch (m.shader.name)
                {
                    case "Gallop/3D/Chara/MultiplyCheek":
                        m.CopyPropertiesFromMaterial(InvisibleMaterial);
                        break;
                    case "Gallop/3D/Chara/ToonEye/T":
                    case "Nars/UmaMusume/Eyes":
                        m.shader = Shader.Find("Nars/UmaMusume/Eyes");
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
