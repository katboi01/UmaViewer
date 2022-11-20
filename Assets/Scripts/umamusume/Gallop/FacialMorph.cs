using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gallop
{
    [System.Serializable]
    public class FacialMorph
    {
        public string name;
        public string tag = "";
        public bool direction;
        public float weight;
        public List<TrsArray> trsArray;
        public Transform locator;
    }

    [System.Serializable]
    public class FacialExtraMorph : FacialMorph
    {
        public GameObject BindGameObject;
        public List<BindProperty> BindProperties = new List<BindProperty>();
        public float GetLocatorValue(BindProperty.LocatorPart part)
        {
            switch (part)
            {
                case BindProperty.LocatorPart.PosX: return locator.localPosition.x;
                case BindProperty.LocatorPart.PosY: return locator.localPosition.y;
                case BindProperty.LocatorPart.PosZ: return locator.localPosition.z;
                case BindProperty.LocatorPart.ScaX: return locator.localScale.x;
                case BindProperty.LocatorPart.ScaY: return locator.localScale.y;
                case BindProperty.LocatorPart.ScaZ: return locator.localScale.z;
                default: return locator.position.x;
            }
        }
    }

    [System.Serializable]
    public class BindProperty
    {
        public enum LocatorPart
        {
            PosX,
            PosY,
            PosZ,
            ScaX,
            ScaY,
            ScaZ,
        }

        public enum BindType
        {
            Shader,
            Select,
            EyeSelect,
            Texture,
            Enable,
            TearSide,
            TearWeight,
            TearSelect,
            TearSpeed
        }
        public Material BindMaterial;
        public Texture BindTexture;
        public TearController BindTearController;
        public List<GameObject> BindPrefab = new List<GameObject>();
        public string PropertyName;
        public LocatorPart Part;
        public BindType Type;
        public float Value;
    }
}
