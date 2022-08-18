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
        private FaceDrivenKeyTarget face;
        [Range(0, 1)]
        public float Weight;
        [HideInInspector]public float LastWeight;
        [HideInInspector] public List<TrsArray> trsArray;
        public void SetFace(FaceDrivenKeyTarget faceTarget)
        {
            face = faceTarget;
        }
    }
}
