// StageMob3DImitationController
using Cutt;
using System.Collections.Generic;
using UnityEngine;

public class StageMob3DImitationController : MonoBehaviour
{
    private static readonly int CAMERA_ARROW_PROPERTY_ID = Shader.PropertyToID("_cameraArrow");

    private static readonly int GRADIATION_PROPERTY_ID = Shader.PropertyToID("_gradiation");

    private static readonly int RIMLIGHT_PROPERTY_ID = Shader.PropertyToID("_rimlight");

    private static readonly int BLENDRANGE_PROPERTY_ID = Shader.PropertyToID("_blendrange");

    private static readonly int COLOR_PALETTE_PROPERTY_ID = Shader.PropertyToID("_colorPalette");

    private const string test = "_Test";

    private static readonly int TestPropertyID = Shader.PropertyToID("_Test");

    private const string MobShaderName = "Cygames/3DLive/Stage/StageMob3DImitation";

    private const string CyalumeShaderName = "Cygames/3DLive/Stage/StageCyalume3DImitation";

    private List<Material> _materialList;

    private Director _director;

    private List<float> _cyalumeColorIndexList;

    public void Setup()
    {
        Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
        _materialList = new List<Material>();
        _cyalumeColorIndexList = new List<float>();
        int num = 0;
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            Material material = componentsInChildren[i].material;
            //_ = componentsInChildren[i].GetComponent<MeshFilter>().mesh.vertexCount;
            string text = material.shader.name;
            if (text == MobShaderName || text == CyalumeShaderName)
            {
                Material material2 = Object.Instantiate(material);
                componentsInChildren[i].material = material2;
                _materialList.Add(material2);
                if (text == CyalumeShaderName)
                {
                    _cyalumeColorIndexList.Add(num++);
                }
                else
                {
                    _cyalumeColorIndexList.Add(-1f);
                }
            }
        }
        _director = Director.instance;
    }

    public void UpdateParams(ref MobCyalume3DUpdateInfo updateInfo)
    {
        float num = (updateInfo.paletteScrollSection + 0.5f) * 60f / 1024f;
        float x = num - (int)num;
        float num2 = ((int)num * 256 + 0.5f) / 1024f;
        for (int i = 0; i < _materialList.Count; i++)
        {
            Material material = _materialList[i];
            material.SetVector(CAMERA_ARROW_PROPERTY_ID, _director.mainCamera.transform.forward);
            material.SetFloat(GRADIATION_PROPERTY_ID, updateInfo.gradiation);
            material.SetFloat(RIMLIGHT_PROPERTY_ID, updateInfo.rimlight);
            material.SetFloat(BLENDRANGE_PROPERTY_ID, 1f / updateInfo.blendRange);
            material.SetVector(COLOR_PALETTE_PROPERTY_ID, new Vector2(x, num2 + _cyalumeColorIndexList[i] / 1024f));
        }
    }
}
