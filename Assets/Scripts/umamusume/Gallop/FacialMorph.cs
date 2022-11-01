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
        public FaceDrivenKeyTarget target;
        public float weight;
        public List<TrsArray> trsArray;
    }
}
