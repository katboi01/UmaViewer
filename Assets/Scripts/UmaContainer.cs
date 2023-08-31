using Gallop;
using Gallop.Live.Cutt;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class UmaContainer : MonoBehaviour
{
    public Transform Position;

    [Header("Live")]
    public bool IsLive = false;
    public bool LiveVisible = true;
    public LiveTimelineCharaLocator LiveLocator;

    [Header("Other")]
    public CharaShaderEffectData ShaderEffectData;
    public List<MaterialHelper> Materials = new List<MaterialHelper>();

    protected UmaViewerBuilder Builder => UmaViewerBuilder.Instance;
    protected UmaViewerMain Main => UmaViewerMain.Instance;
    protected UmaViewerUI UI => UmaViewerUI.Instance;

    public class MaterialHelper
    {
        public Material Mat;
        public UmaUIContainer Toggle;
        public Dictionary<Renderer, List<int>> Renderers;
    
        public void ToggleMaterials(bool value)
        {
            foreach(var rend in Renderers)
            {
                Debug.Log(rend.Key.name);
                Material[] mat = new Material[rend.Key.materials.Length];
                for(int i = 0; i < mat.Length; i++)
                {
                    //if material slot is not in list, keep current material
                    //if material slot in list and toggle is on - assign original
                    //if material slot in list and toggle is off - assign invisible
                    mat[i] = rend.Value.Contains(i) ? (value? Mat : UmaViewerBuilder.Instance.TransMaterialCharas ): rend.Key.materials[i];
                }

                rend.Key.materials = mat;
            }
        }
    }
}
