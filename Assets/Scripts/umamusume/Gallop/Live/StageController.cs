using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gallop.Live
{
    public class StageController : MonoBehaviour
    {
        [System.Serializable]
        public class MipmapBias
        {
            public float _stage;
        }
        public List<GameObject> _stageObjects;
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
        public List<GameObject>  _preLiveSkitPrefabs;
        public List<GameObject> _afterLiveSkitPrefabs;
        public List<AnimationClip> _audienceAnimations;
        public StageController.MipmapBias _mipmapBias;
        public List<GameObject> _blackBoardNamePrefabs;
        public GameObject _blackBoardNameAtlas;

        private void Awake()
        {
            InitializeStage();
        }

        public void InitializeStage()
        {
            foreach (GameObject stage_part in _stageObjects)
            {
                Instantiate(stage_part, transform.parent);
            }
        }
    }
}