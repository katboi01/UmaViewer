using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public void Initialize(Transform target, int targetIndex, int seqDataIndex, LiveTimelineControl timelineControl)
        {
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
            if (_currentKey != null && _tempAnim != null)
            {
                /*
                var seqs = _currentKey.thisList;

                for (int i = seqs.Count - 1; i >= 0; i--)
                {
                    Debug.Log(currentTime);
                    if (currentTime >= (double)seqs[i].frame / 60)
                    {
                        Debug.Log("Ê±¼ä:" + ((double)seqs[i].frame / 60).ToString());

                        AnimationClip anim = seqs[i].clip;
                        double start = (double)seqs[i].motionHeadFrame / 60;
                        double interval = currentTime - (double)seqs[i].frame / 60;

                        Debug.Log("time:" + interval.ToString());

                        //anim.SampleAnimation(container.gameObject, (float)(start + interval));
                        _tempAnim[anim.name].time = (float)(start + interval);
                        _tempAnim.Play(anim.name);

                        break;
                    }
                }
                */

                var timelineKey = GetKey(currentTime);

                //if ( _prevKey != timelineKey ) Play animation all the time not affect performance maybe?
                if ( true )
                {
                    AnimationClip anim = timelineKey.clip;
                    double start = (double)timelineKey.motionHeadFrame / 60;
                    double interval = currentTime - (double)timelineKey.frame / 60;

                    _tempAnim[anim.name].time = (float)(start + interval);
                    _tempAnim.Play(anim.name);

                    _prevKey = timelineKey;
                }
            }
        }

        public LiveTimelineKeyCharaMotionData GetKey(float time)
        {
            var seqs = _currentKey.thisList;

            for (int i = seqs.Count - 1; i >= 0; i--)
            {
                if (time >= (double)seqs[i].frame / 60)
                {
                    return seqs[i];
                }
            }

            return seqs[0];
        }
    }
}
