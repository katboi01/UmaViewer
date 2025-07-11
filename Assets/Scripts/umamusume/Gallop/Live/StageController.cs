using Gallop.Live.Cutt;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Gallop.Live
{
    [Serializable]
    public class StageObjectUnit
    {
        public string UnitName;
        public GameObject[] ChildObjects;
        public string[] _childObjectNames;
    }

    public class StageController : MonoBehaviour
    {
        public List<GameObject> _stageObjects;
        public StageObjectUnit[] _stageObjectUnits;
        public Dictionary<string, StageObjectUnit> StageObjectUnitMap = new Dictionary<string, StageObjectUnit>();
        public Dictionary<string, GameObject> StageObjectMap = new Dictionary<string, GameObject>();
        public Dictionary<string, Transform> StageParentMap = new Dictionary<string, Transform>();

        private void Awake()
        {
            InitializeStage();
            if (Director.instance)
            {
                Director.instance._stageController = this;
                Director.instance._liveTimelineControl.OnUpdateTransform += UpdateTransform;
                Director.instance._liveTimelineControl.OnUpdateObject += UpdateObject;
            }
        }

        private void OnDestroy()
        {
            if (Director.instance)
            {
                Director.instance._liveTimelineControl.OnUpdateTransform -= UpdateTransform;
                Director.instance._liveTimelineControl.OnUpdateObject -= UpdateObject;
            }
        }

        public void InitializeStage()
        {
            foreach (GameObject stage_part in _stageObjects)
            {
                var instance = Instantiate(stage_part, transform);
                foreach (var child in instance.GetComponentsInChildren<Transform>(true))
                {
                    if (!StageObjectMap.ContainsKey(child.name))
                    {
                        if (child.name.Contains("light"))
                        {
                            // Since the parsing of light material keyframes has been implemented
                            // Directly displaying the light will block the view
                            // Therefore, the light's GameObject is hidden here first
                            // TODO: Remove this after implementing light parsing
                            child.gameObject.SetActive(false);
                            continue;
                        }
                        var tmp_name = child.name.Replace("(Clone)", "");
                        StageObjectMap.Add(tmp_name, child.gameObject);
                        StageParentMap.TryAdd(tmp_name, child.gameObject.transform.parent);
                    }
                }
            }

            foreach (var unit in _stageObjectUnits)
            {
                if (!StageObjectUnitMap.ContainsKey(unit.UnitName))
                {
                    StageObjectUnitMap.Add(unit.UnitName, unit);
                }
            }
        }

        public void UpdateObject(ref ObjectUpdateInfo updateInfo) {

            if (updateInfo.data == null)
            {
                return;
            }
            if (StageObjectMap.TryGetValue(updateInfo.data.name, out GameObject gameObject))
            {
                gameObject.SetActive(updateInfo.renderEnable);

                Transform attach_transform = null;
                switch (updateInfo.AttachTarget)
                {
                    case AttachType.None:
                        if(StageParentMap.TryGetValue(updateInfo.data.name, out Transform parentTransform))
                        {
                            attach_transform = parentTransform;
                        }
                        break;
                    case AttachType.Character:
                        var chara = Director.instance.CharaContainerScript[updateInfo.CharacterPosition];
                        if (chara)
                        {
                            attach_transform = chara.transform;
                        }
                        break;
                    case AttachType.Camera:
                        attach_transform = Director.instance.MainCameraTransform;
                        break;
                }
                if (gameObject.transform.parent != attach_transform)
                {
                    gameObject.transform.SetParent(attach_transform);
                }

                if (updateInfo.data.enablePosition)
                {
                    gameObject.transform.localPosition = updateInfo.updateData.position;
                }
                if (updateInfo.data.enableRotate)
                {
                    gameObject.transform.localRotation = updateInfo.updateData.rotation;
                }
                if (updateInfo.data.enableScale)
                {
                    gameObject.transform.localScale = updateInfo.updateData.scale;
                }
            }
        }

        public void UpdateTransform(ref TransformUpdateInfo updateInfo)
        {
            if (updateInfo.data == null)
            {
                return;
            }
            if (StageObjectUnitMap.TryGetValue(updateInfo.data.name, out StageObjectUnit objectUnit))
            {
                foreach(var child in objectUnit.ChildObjects)
                {
                    if (StageObjectMap.TryGetValue(child.name, out GameObject gameObject))
                    {
                        if (updateInfo.data.enablePosition)
                        {
                            gameObject.transform.localPosition = updateInfo.updateData.position;
                        }
                        if (updateInfo.data.enableRotate)
                        {
                            gameObject.transform.localRotation = updateInfo.updateData.rotation;
                        }
                        if (updateInfo.data.enableScale)
                        {
                            gameObject.transform.localScale = updateInfo.updateData.scale;
                        }
                    }
                }
            }
        }
    }
}