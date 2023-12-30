using Gallop;
using Gallop.Live.Cutt;
using System.Collections.Generic;
using UnityEngine;

public class UmaContainer : MonoBehaviour
{
    public Transform Position;

    [Header("Live")]
    public bool IsLive = false;
    public bool LiveVisible = true;
    public LiveTimelineCharaLocator LiveLocator;

    [Header("Other")]
    public CharaShaderEffectData HeadShaderEffectData;
    public CharaShaderEffectData BodyShaderEffectData;
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
            foreach (var rend in Renderers)
            {
                Debug.Log(rend.Key.name);
                Material[] mat = new Material[rend.Key.sharedMaterials.Length];
                for (int i = 0; i < mat.Length; i++)
                {
                    //if material slot is not in list, keep current material
                    //if material slot in list and toggle is on - assign original
                    //if material slot in list and toggle is off - assign invisible
                    mat[i] = rend.Value.Contains(i) ? (value ? Mat : UmaViewerBuilder.Instance.TransMaterialCharas) : rend.Key.sharedMaterials[i];
                }

                rend.Key.sharedMaterials = mat;
            }
        }
    }
}
