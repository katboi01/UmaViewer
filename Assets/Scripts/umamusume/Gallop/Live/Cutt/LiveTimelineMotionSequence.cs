using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RootMotion.Demos.CharacterMeleeDemo.Action;

namespace Gallop.Live.Cutt
{
    public class LiveTimelineMotionSequence
    {
        private ILiveTimelineMSQTarget _target; // 0x10
        private Animation[] _animationArray; // 0x18
        private int _targetIndex; // 0x20
        private LiveTimelineKeyCharaMotionSeqDataList[] _keyArray; // 0x28
        private LiveTimelineKeyCharaMotionSeqDataList _currentKey; // 0x30
        private int _indexOfPlayingKey; // 0x38
        private LiveTimelineControl _timelineControl; // 0x40
        private float _heightRate; // 0x48
        private LiveTimelineDefine.SheetIndex _currentSheetIndex; // 0x4C
        //private LivePlayableAnimator[] _playableAnimatorArray; // 0x50
        private int _prevSysTextId; // 0x58
        private const float OVERRIDE_ANIMATOR_FACIAL_WAIT = 1;
        private int _prevFrame; // 0x5C
        private float _prevFrameAnimationTime; // 0x60

        //Testing
        private Transform _tempTarget;
        private Animation _tempAnim;

        private bool _needChange;
        private LiveTimelineKeyCharaMotionData _prevKey;

        private AnimationClip _targetAnim = null;

        private bool _motionSetup = false;

        public void Initialize(Transform target, int targetIndex, int seqDataIndex, LiveTimelineControl timelineControl, List<AnimationClip> animclips = null)
        {
            if(animclips != null)
            {
                if (targetIndex < Director.instance.charaAnims.Count)
                {
                    _targetAnim = animclips[targetIndex];

                    _tempAnim = Director.instance.charaAnims[targetIndex];
                    _tempAnim.AddClip(_targetAnim, _targetAnim.name);
                }
                return;
            }

            _tempTarget = target;
            _targetIndex = targetIndex;

            _keyArray = timelineControl._keyArray;

            if (seqDataIndex < _keyArray.Length)
            {
                _currentKey = _keyArray[seqDataIndex];
            }

            if (targetIndex < Director.instance.charaAnims.Count)
            {
                _tempAnim = Director.instance.charaAnims[targetIndex];
            }

            if(_currentKey != null && _tempAnim != null)
            {
                foreach (var key in _currentKey.thisList)
                {
                    _tempAnim.AddClip(key.clip, key.clip.name);
                }
            }
        }

        public void AlterUpdate(float currentTime)
        {
            if(Director.instance.liveMode == 0)
            {
                
                if(_targetAnim != null)
                {
                    //_tempAnim[_targetAnim.name].time = currentTime;
                    if (Director.instance.sliderControl.is_Touched)
                    {
                        _tempAnim[_targetAnim.name].time = currentTime;
                        _tempAnim.Play(_targetAnim.name);
                        _motionSetup = false;
                    }
                    else if (!_motionSetup)
                    {
                        _tempAnim[_targetAnim.name].time = currentTime;
                        _tempAnim.Play(_targetAnim.name);
                        _motionSetup = true;
                    }
                    
                }
                return;
            }

            if (_currentKey != null && _tempAnim != null)
            {
                LiveTimelineKeyIndex curKey = LiveTimelineControl.AlterUpdate_Key(_currentKey, currentTime);

                if (curKey != null && curKey.index != -1)
                {
                    LiveTimelineKeyCharaMotionData arg = curKey.key as LiveTimelineKeyCharaMotionData;

                    AnimationClip anim = arg.clip;
                    double start = (double)arg.motionHeadFrame / 60;
                    double interval = currentTime - (double)arg.frame / 60;

                    _tempAnim[anim.name].time = (float)(start + interval);
                    _tempAnim.Play(anim.name);
                }
            }
        }
    }
}
