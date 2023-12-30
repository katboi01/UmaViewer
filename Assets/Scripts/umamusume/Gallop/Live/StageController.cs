using System;
using System.Collections.Generic;
using UnityEngine;


namespace Gallop.Live
{
    public class StageController : MonoBehaviour
    {
        [Serializable]
        public class StageObjectUnit
        {
            public string UnitName; // 0x10
            public GameObject[] ChildObjects; // 0x18

            public bool IsEnabled { get; set; }
            public int UnitNameHash { get; set; }
            public int[] ChildObjectNameHashArray { get; set; }
        }

        [SerializeField]
        public class StageObjectSelect
        {
            public enum Condition
            {
                Random = 0,
                Direct = 1
            }

            [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
            private string _prefabName; // 0x10
            [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
            private StageController.StageObjectSelect.Condition _condition; // 0x18
            [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
            private int _selectIndex; // 0x1C
            [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
            private GameObject[] _stageObjectArray; // 0x20
            private bool _isEnabled; // 0x28
        }

        [Serializable]
        public class MipmapBias
        {
            public float _stage;
        }
        public List<GameObject> _stageObjects;
        public StageController.StageObjectUnit[] _stageObjectUnits;
        public StageController.StageObjectSelect[] _stageObjectSelects;
        public List<GameObject> _audienceObjects;
        public Vector3 _cyalumeOffsetPosition;
        public List<Material> _washLightMaterials;
        public List<Material> _uvScrollLightMaterials;
        public List<GameObject> _laserObjects;
        public List<Material> _laserMaterials;
        public List<Material> _mirrorScanLightMaterials;
        public List<Material> _footLightMaterials;
        public List<NeonMaterialController.NeonMaterialInfo> _neonMaterialInfos;
        public List<MovieMaterialController.MovieMaterialInfo> _movieMaterialInfos;
        public List<GameObject> _animationObjects;
        public string _borderLightObjectName;
        public string _sunObjectName;
        public List<GameObject> _preLiveSkitPrefabs;
        public List<GameObject> _afterLiveSkitPrefabs;
        public List<AnimationClip> _audienceAnimations;
        public StageController.MipmapBias _mipmapBias;
        public List<GameObject> _blackBoardNamePrefabs;
        public GameObject _blackBoardNameAtlas;

        private void Awake()
        {
            InitializeStage();
            if (Director.instance)
            {
                Director.instance._stageController = this;
            }
        }

        public void InitializeStage()
        {
            foreach (GameObject stage_part in _stageObjects)
            {
                Instantiate(stage_part, transform);
            }
        }
    }
}