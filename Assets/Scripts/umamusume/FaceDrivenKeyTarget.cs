using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop
{
    public class FaceDrivenKeyTarget : MonoBehaviour
    {
        public List<EyeTarget> _eyeTarget { get; set; }
        public List<EyebrowTarget> _eyebrowTarget { get; set; }
        public List<MouthTarget> _mouthTarget { get; set; }
    }
}
