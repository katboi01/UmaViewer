using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live
{
    public class MovieMaterialController : MonoBehaviour
    {
        public enum MovieType
        {
            MainMonitor = 0,
            SubMonitor = 1,
            Parts = 2,
            Overlay = 3
        }
        [System.Serializable]
        public class MovieMaterialInfo
        {
            public Material _movieMaterial;
            public MovieMaterialController.MovieType _movieType;
            public int _renderOrder;
        }
    }
}


