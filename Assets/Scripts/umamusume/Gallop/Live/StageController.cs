using System;
using System.Collections.Generic;
using UnityEngine;


namespace Gallop.Live
{
    public class StageController : MonoBehaviour
    {
        public List<GameObject> _stageObjects;

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