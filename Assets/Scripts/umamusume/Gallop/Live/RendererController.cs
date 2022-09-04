using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live
{
    public class RendererController : MonoBehaviour
    {
		[SerializeField] 
		private int _lightmapIndex; 
		[SerializeField] 
		private Vector4 _lightmapScaleOffset; 
		[SerializeField]
		private Vector4 _realtimeLightmapScaleOffset;
		[SerializeField]
		private int _sortingOrder; 
	}
}

