using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live
{
    public class NeonMaterialController : MonoBehaviour
    {
        [System.Serializable]
        public class NeonMaterialInfo
        {
            public Material _mainMaterial;
            public Material _backMaterial;
        }
    }

}
