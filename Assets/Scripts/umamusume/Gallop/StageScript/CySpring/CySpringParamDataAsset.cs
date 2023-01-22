using System;
using System.Collections.Generic;
using UnityEngine;

// public class CySpringParamDataAsset : ScriptableObject {
//   [SerializeField]
//   public List<CySpringParamDataElement> _elements;
// }

[Serializable]
public class CySpringParamDataAsset {
  public List<CySpringParamDataElement> _elements;
}