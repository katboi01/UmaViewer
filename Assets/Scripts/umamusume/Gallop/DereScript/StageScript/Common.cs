using System.Collections.Generic;
using UnityEngine;

namespace Live3D
{
    public class Common
    {
        private static readonly float sFramePerSecond = 60f;

        private static readonly float sSecondPerFrame = 1f / Common.sFramePerSecond;

        public static float GetFrame2Second(float frame)
        {
            return frame * Common.sSecondPerFrame;
        }

        public static float GetSceond2Frame(float second)
        {
            return second * Common.sFramePerSecond;
        }

        public static void GetArrayComponentFromArrayObject<T>(out T[] components, GameObject[] gameobjects)
        {
            components = null;
            if (gameobjects == null)
            {
                return;
            }
            if (gameobjects.Length == 0)
            {
                return;
            }
            List<T> list = new List<T>();
            for (int i = 0; i < gameobjects.Length; i++)
            {
                T component = gameobjects[i].GetComponent<T>();
                if (component != null)
                {
                    if (!(component.ToString() == "null"))
                    {
                        list.Add(component);
                    }
                }
            }
            if (list.Count == 0)
            {
                return;
            }
            components = list.ToArray();
        }

        public static void GetRendererFromMaterial(out Renderer[] arrRenderers, ref Material baseMaterial, string baseMaterialName, GameObject rootObject)
        {
            arrRenderers = null;
            if (rootObject == null)
            {
                return;
            }
            if (baseMaterial == null)
            {
                return;
            }
            List<Renderer> list = new List<Renderer>();
            Renderer[] componentsInChildren = rootObject.GetComponentsInChildren<Renderer>(true);
            string b;
            if (baseMaterialName == null)
            {
                b = baseMaterial.name;
            }
            else
            {
                b = baseMaterialName;
            }
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                if (!(componentsInChildren[i] == null))
                {
                    if (!(componentsInChildren[i].sharedMaterial == null))
                    {
                        if (componentsInChildren[i].sharedMaterial.name == b)
                        {
                            list.Add(componentsInChildren[i]);
                        }
                    }
                }
            }
            if (list.Count == 0)
            {
                return;
            }
            arrRenderers = list.ToArray();
            baseMaterial = UnityEngine.Object.Instantiate<Material>(baseMaterial);
            for (int i = 0; i < arrRenderers.Length; i++)
            {
                arrRenderers[i].sharedMaterial = baseMaterial;
            }
        }

        public static Color GetGrayScaleColor(Color color)
        {
            float num = Mathf.Min(color.r, color.g);
            num = Mathf.Min(num, color.b);
            float num2 = Mathf.Max(color.r, color.g);
            num2 = Mathf.Max(num2, color.b);
            float num3 = (num2 + num) * 0.5f;
            color.r = num3;
            color.g = num3;
            color.b = num3;
            return color;
        }
    }
}
