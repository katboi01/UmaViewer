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
        public bool direction;
        public FaceDrivenKeyTarget target;
        private float _weight;
        public float Weight
        {
            get
            {
                return _weight;
            }
            set
            {
                _weight = value;
                target.ChangeMorph();
            }
        }
        public List<TrsArray> trsArray;
    }
}
