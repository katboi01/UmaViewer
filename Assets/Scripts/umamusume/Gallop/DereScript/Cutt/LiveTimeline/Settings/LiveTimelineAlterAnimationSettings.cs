using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineAlterAnimationSettings
    {
        [Serializable]
        public class AnimationNameList
        {
            [SerializeField]
            private string[] _list = new string[0];

            public string this[int index]
            {
                get
                {
                    return _list[index];
                }
                set
                {
                    _list[index] = value;
                }
            }

            public int Length => _list.Length;

            public AnimationNameList()
            {
            }

            public AnimationNameList(int n)
            {
                _list = new string[n];
            }
        }

        public AnimationNameList[] useAnimationNames = new AnimationNameList[0];

        public AnimationNameList[] useOverwriteAnimationNames = new AnimationNameList[0];

        public void Init()
        {
            useAnimationNames = new AnimationNameList[15];
            useOverwriteAnimationNames = new AnimationNameList[15];
            for (int i = 0; i < 15; i++)
            {
                useAnimationNames[i] = new AnimationNameList();
                useOverwriteAnimationNames[i] = new AnimationNameList();
            }
        }

        public List<int> GetMotionUseCharaPosition(string motionName)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < 15; i++)
            {
                AnimationNameList animationNameList = useAnimationNames[i];
                for (int j = 0; j < animationNameList.Length; j++)
                {
                    if (motionName == animationNameList[j] && !list.Contains(i))
                    {
                        list.Add(i);
                    }
                }
                animationNameList = useOverwriteAnimationNames[i];
                for (int k = 0; k < animationNameList.Length; k++)
                {
                    if (motionName == animationNameList[k] && !list.Contains(i))
                    {
                        list.Add(i);
                    }
                }
            }
            return list;
        }
    }
}