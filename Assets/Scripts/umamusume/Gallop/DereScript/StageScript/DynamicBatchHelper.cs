using UnityEngine;

namespace Stage
{
    /// <summary>
    /// お願いシンデレラ(Grand Ver)で使用
    /// </summary>
    public class DynamicBatchHelper : MonoBehaviour
    {
        [SerializeField]
        private int _renderQueue = 2000;

        [SerializeField]
        private string _materialName = "";

        public int renderQueue => _renderQueue;

        public string materialName => _materialName;

        public Material FindMaterial()
        {
            MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
            string value = _materialName;
            string value2 = _materialName + "(Clone)";
            MeshRenderer[] array = componentsInChildren;
            foreach (MeshRenderer meshRenderer in array)
            {
                string text = meshRenderer.sharedMaterial.name;
                if (text.Equals(value2) || text.Equals(value))
                {
                    return meshRenderer.sharedMaterial;
                }
            }
            return null;
        }

        public void Initialize(Material material)
        {
            MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
            string value = _materialName;
            string value2 = _materialName + "(Clone)";
            MeshRenderer[] array = componentsInChildren;
            foreach (MeshRenderer meshRenderer in array)
            {
                string text = meshRenderer.sharedMaterial.name;
                if (text.Equals(value2) || text.Equals(value))
                {
                    meshRenderer.sharedMaterial = material;
                }
            }
        }
    }
}
