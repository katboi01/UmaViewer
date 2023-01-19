using System.Collections.Generic;
using UnityEngine;

namespace Cutt
{
    /// <summary>
    /// キャラクターモーションの設定
    /// </summary>
    public class LiveTimelineMotionSequence
    {
        /// <summary>
        /// キャラクタのモーションのパラメータ
        /// </summary>
        public struct Context
        {
            /// <summary>
            /// モーション対象のターゲット
            /// </summary>
            public ILiveTimelineMSQTarget target;

            /// <summary>
            /// モーションのキーフレーム
            /// </summary>
            public LiveTimelineKeyCharaMotionSeqDataList motionKeys;

            /// <summary>
            /// 
            /// </summary>
            public LiveTimelineKeyCharaOverrideMotionSeqDataList overwriteMotionKeys;

            public LiveTimelineControl timelineControl;

            /// <summary>
            /// オブジェクトの高さを変更するモーションのキー
            /// </summary>
            public LiveTimelineKeyCharaHeightMotionSeqDataList heightMotionKeys;
        }

        private const int HeightBlendNum = 3;

        /// <summary>
        /// キーブレンドの最小値
        /// </summary>
        private const float MinBlendRate = 0.0001f;

        /// <summary>
        /// モーション対象
        /// </summary>
        private ILiveTimelineMSQTarget _target;

        /// <summary>
        /// 対象のオブジェクトが行うアニメーション
        /// </summary>
        private Animation _animation;

        /// <summary>
        /// モーションの動作を指定するキー
        /// </summary>
        private LiveTimelineKeyCharaMotionSeqDataList _keys;

        /// <summary>
        /// 前キーを上書きしてモーションを変更するキー
        /// </summary>
        private LiveTimelineKeyCharaOverrideMotionSeqDataList _overwriteKeys;

        /// <summary>
        /// 指定モーション（A-B間）のB値
        /// </summary>
        private int _indexOfPlayingKeyB = -1;

        /// <summary>
        /// タイムラインコントローラ
        /// </summary>
        private LiveTimelineControl _timelineControl;

        /// <summary>
        /// 指定モーション（A-B間）のA値
        /// </summary>
        private int _indexOfPlayingKeyA = -1;

        /// <summary>
        /// 高さのモーションが存在するか
        /// </summary>
        private bool _isHeightMotionTimeline;

        /// <summary>
        /// 高さモーションのキー
        /// </summary>
        private LiveTimelineKeyCharaHeightMotionData _heightMotionKey;

        /// <summary>
        /// 高さモーションのブレンド値
        /// </summary>
        private float[] _heightBlendRate = new float[3];

        /// <summary>
        /// オーバーライトモーションが存在するか
        /// </summary>
        private bool _isOverwriteMotionTimeline;

        private float _currentTime;

        private Dictionary<string, bool> _beforeMotionStateName = new Dictionary<string, bool>();

        public float currentTime => _currentTime;

        private void StopHeightMotionKey(LiveTimelineKeyCharaHeightMotionData key)
        {
            if (key.clip != null)
            {
                AnimationState animationState = _animation[key.clip.name];
                if (animationState != null)
                {
                    animationState.enabled = false;
                }
            }
            if (key.partsMotion == null)
            {
                return;
            }
            LiveTimelineKeyCharaHeightMotionData.PartsMotion[] partsMotion = key.partsMotion;
            for (int i = 0; i < partsMotion.Length; i++)
            {
                LiveTimelineKeyCharaHeightMotionData.PartsMotion partsMotion2 = partsMotion[i];
                AnimationState animationState2 = _animation[partsMotion2.baseMotion.name];
                if (animationState2 != null)
                {
                    animationState2.enabled = false;
                }
                animationState2 = _animation[partsMotion2.blendMotion.name];
                if (animationState2 != null)
                {
                    animationState2.enabled = false;
                }
            }
        }

        public void SetHeightMotionKey(LiveTimelineKeyCharaHeightMotionData key)
        {
            if (_heightMotionKey != key && _animation != null)
            {
                if (key != null)
                {
                    StopHeightMotionKey(key);
                }
                if (_heightMotionKey != null)
                {
                    StopHeightMotionKey(_heightMotionKey);
                }
            }
            _heightMotionKey = key;
        }

        public void SetHeightMotionBlend(int index, float rate)
        {
            _heightBlendRate[index] = rate;
        }

        public void Initialize(ref Context context)
        {
            if (context.target == null || context.target.liveMSQAnimation == null || context.motionKeys == null || context.timelineControl == null)
            {
                return;
            }
            _keys = context.motionKeys;
            _overwriteKeys = context.overwriteMotionKeys;
            _target = context.target;
            _animation = context.target.liveMSQAnimation;
            _timelineControl = context.timelineControl;
            _indexOfPlayingKeyB = -1;
            _indexOfPlayingKeyA = -1;
            _isHeightMotionTimeline = false;
            _isOverwriteMotionTimeline = false;
            _target.liveMSQControlled = true;
            _animation.Stop();
            for (int i = 0; i < context.motionKeys.Count; i++)
            {
                LiveTimelineKeyCharaMotionData liveTimelineKeyCharaMotionData = _keys.At(i) as LiveTimelineKeyCharaMotionData;
                if (!string.IsNullOrEmpty(liveTimelineKeyCharaMotionData.motionName))
                {
                    SetupAnimationClip(liveTimelineKeyCharaMotionData);
                }
            }
            if (context.overwriteMotionKeys != null)
            {
                for (int j = 0; j < context.overwriteMotionKeys.Count; j++)
                {
                    LiveTimelineKeyCharaMotionData liveTimelineKeyCharaMotionData2 = _overwriteKeys.At(j) as LiveTimelineKeyCharaMotionData;
                    if (!string.IsNullOrEmpty(liveTimelineKeyCharaMotionData2.motionName))
                    {
                        SetupAnimationClip(liveTimelineKeyCharaMotionData2);
                    }
                }
            }
            if (context.heightMotionKeys != null)
            {
                LiveTimelineKeyCharaHeightMotionSeqDataList heightMotionKeys = context.heightMotionKeys;
                for (int k = 0; k < heightMotionKeys.Count; k++)
                {
                    LiveTimelineKeyCharaHeightMotionData key = heightMotionKeys.At(k) as LiveTimelineKeyCharaHeightMotionData;
                    SetupAnimationClip(key);
                }
            }
        }

        public void ChangeTarget(ILiveTimelineMSQTarget target)
        {
            if (target != null)
            {
                _target = target;
                _animation = target.liveMSQAnimation;
                _indexOfPlayingKeyB = -1;
                _indexOfPlayingKeyA = -1;
                _isHeightMotionTimeline = false;
                _isOverwriteMotionTimeline = false;
                _target.liveMSQControlled = true;
                _animation.Stop();
            }
        }

        private bool SetupAnimationClip(LiveTimelineKeyCharaHeightMotionData key)
        {
            if (key.partsMotion != null)
            {
                LiveTimelineKeyCharaHeightMotionData.PartsMotion[] partsMotion = key.partsMotion;
                for (int i = 0; i < partsMotion.Length; i++)
                {
                    LiveTimelineKeyCharaHeightMotionData.PartsMotion partsMotion2 = partsMotion[i];
                    if (!(partsMotion2.baseMotion == null) && !(partsMotion2.blendMotion == null))
                    {
                        _animation.AddClip(partsMotion2.baseMotion, partsMotion2.baseMotion.name);
                        _animation.AddClip(partsMotion2.blendMotion, partsMotion2.blendMotion.name);
                    }
                }
            }
            if (string.IsNullOrEmpty(key.motionName))
            {
                return false;
            }
            AnimationClip animationClip = _timelineControl.LoadAnimationClip(key);
            if (animationClip == null)
            {
                return false;
            }
            _animation.AddClip(animationClip, animationClip.name);
            return true;
        }

        private bool SetupAnimationClip(LiveTimelineKeyCharaMotionData key)
        {
            AnimationClip animationClip = _timelineControl.LoadAnimationClip(key);
            if (animationClip == null)
            {
                return false;
            }
            _animation.AddClip(animationClip, animationClip.name);
            return true;
        }

        public void SetOverwriteKeys(LiveTimelineKeyCharaOverrideMotionSeqDataList overwriteKeys)
        {
            _overwriteKeys = overwriteKeys;
        }

        private void FindPlayingMotion<T>(out LiveTimelineKeyCharaMotionData a, out LiveTimelineKeyCharaMotionData b, out int indexA, out int indexB, float t, float spf, LiveTimelineKeyDataListTemplate<T> keys, LiveTimelineKeyDataType keyType) where T : LiveTimelineKey
        {
            a = (b = null);
            indexA = (indexB = -1);
            int i = 0;
            for (int count = keys.Count; i < count; i++)
            {
                LiveTimelineKeyCharaMotionData liveTimelineKeyCharaMotionData = keys.thisList[i] as LiveTimelineKeyCharaMotionData;
                float num = (float)liveTimelineKeyCharaMotionData.frame * spf;
                if (t >= num && t <= num + liveTimelineKeyCharaMotionData.playLength)
                {
                    if (a != null)
                    {
                        if (keyType == LiveTimelineKeyDataType.CharaMotionOverwrite || keyType == LiveTimelineKeyDataType.CharaHeightMotion)
                        {
                            a = liveTimelineKeyCharaMotionData;
                            indexA = i;
                        }
                        else if ((float)a.frame * spf + a.playLength >= num)
                        {
                            b = liveTimelineKeyCharaMotionData;
                            indexB = i;
                        }
                        break;
                    }
                    a = liveTimelineKeyCharaMotionData;
                    indexA = i;
                }
                else if (num > t)
                {
                    break;
                }
            }
        }

        private LiveTimelineKeyDataType FindPlayingMot(out LiveTimelineKeyCharaMotionData a, out LiveTimelineKeyCharaMotionData b, out LiveTimelineKeyCharaMotionData lastKey, out int indexA, out int indexB, float t, float spf)
        {
            if (_overwriteKeys != null)
            {
                FindPlayingMotion(out a, out b, out indexA, out indexB, t, spf, _overwriteKeys, LiveTimelineKeyDataType.CharaMotionOverwrite);
                if (indexA >= 0)
                {
                    lastKey = null;
                    return LiveTimelineKeyDataType.CharaMotionOverwrite;
                }
            }
            FindPlayingMotion(out a, out b, out indexA, out indexB, t, spf, _keys, LiveTimelineKeyDataType.CharaMotionSequecne);
            if (_keys.Count > 0)
            {
                lastKey = _keys.At(_keys.Count - 1) as LiveTimelineKeyCharaMotionData;
            }
            else
            {
                lastKey = null;
            }
            return LiveTimelineKeyDataType.CharaMotionSequecne;
        }

        private void OnAnimationBlend(AnimationState motionState, string motionName, float blendRate, WrapMode wrapMode, float setTime)
        {
            motionState.speed = 1f;
            motionState.enabled = true;
            if (blendRate >= 0.0001f)
            {
                _animation.Blend(motionName, blendRate, 0f);
            }
            else
            {
                motionState.enabled = false;
            }
            motionState.wrapMode = wrapMode;
            motionState.time = setTime;
            motionState.speed = 0f;
        }

        private void OnMotionBlend(AnimationClip clip, float blendRate, WrapMode wrapMode, float setTime)
        {
            AnimationState animationState = GetAnimationState(clip);
            if (!(animationState == null))
            {
                OnAnimationBlend(animationState, clip.name, blendRate, wrapMode, setTime);
            }
        }

        private void OnMotionBlend(LiveTimelineKeyCharaMotionData clip, float blendRate, WrapMode wrapMode, float setTime)
        {
            AnimationState animationState = GetAnimationState(clip);
            if (!(animationState == null))
            {
                OnAnimationBlend(animationState, clip.motionName, blendRate, wrapMode, setTime);
            }
        }

        private void OnMotionBUpdate(LiveTimelineKeyCharaMotionData playingKeyB, bool isHeightMotionTimeline, float blendRate, float currentTime, float spf, ref Dictionary<string, bool> controlClips)
        {
            float num = (float)playingKeyB.frame * spf;
            float num2 = (currentTime - num - playingKeyB.motionHeadTime) * playingKeyB.playSpeed;
            WrapMode wrapMode = (playingKeyB.loop ? WrapMode.Loop : WrapMode.ClampForever);
            if (!playingKeyB.loop && num2 < 0f)
            {
                wrapMode = WrapMode.Loop;
            }
            OnMotionBlend(playingKeyB, blendRate, wrapMode, num2);
            if (!isHeightMotionTimeline)
            {
                return;
            }
            LiveTimelineKeyCharaHeightMotionData heightMotionKey = _heightMotionKey;
            if (heightMotionKey.partsMotion != null)
            {
                int num3 = 1;
                LiveTimelineKeyCharaHeightMotionData.PartsMotion[] partsMotion = heightMotionKey.partsMotion;
                for (int i = 0; i < partsMotion.Length; i++)
                {
                    LiveTimelineKeyCharaHeightMotionData.PartsMotion partsMotion2 = partsMotion[i];
                    blendRate = _heightBlendRate[num3];
                    OnMotionBlend(partsMotion2.baseMotion, 1f - blendRate, wrapMode, num2);
                    OnMotionBlend(partsMotion2.blendMotion, blendRate, wrapMode, num2);
                    controlClips[partsMotion2.baseMotion.name] = true;
                    controlClips[partsMotion2.blendMotion.name] = true;
                    num3++;
                }
            }
        }

        private void DisablePrevMotion(LiveTimelineKeyCharaMotionData motionA, LiveTimelineKeyCharaMotionData prevMotionA, LiveTimelineKeyCharaMotionData motionB, LiveTimelineKeyCharaMotionData prevMotionB, ref bool isChangeMotion)
        {
            if (motionA != prevMotionA && prevMotionA != null && motionA != null)
            {
                if (motionA.motionNameHash != prevMotionA.motionNameHash)
                {
                    AnimationState animationState = _animation[prevMotionA.motionName];
                    if (animationState != null)
                    {
                        animationState.enabled = false;
                    }
                }
                isChangeMotion = true;
            }
            if (prevMotionB != motionB && prevMotionB != null && prevMotionB != motionA)
            {
                AnimationState animationState2 = _animation[prevMotionB.motionName];
                if (animationState2 != null)
                {
                    animationState2.enabled = false;
                }
                isChangeMotion = true;
            }
        }

        public void AlterUpdate(float currentTime, float targetFps)
        {
            if (_keys == null || _keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
            {
                return;
            }
            _currentTime = currentTime;
            float num = 1f / targetFps;
            bool flag = false;
            LiveTimelineKeyCharaMotionData liveTimelineKeyCharaMotionData = null;
            bool flag2 = false;
            if (_heightMotionKey != null)
            {
                liveTimelineKeyCharaMotionData = _heightMotionKey;
                flag2 = true;
            }
            if (FindPlayingMot(out var a, out var b, out var _, out var indexA, out var indexB, currentTime, num) == LiveTimelineKeyDataType.CharaMotionOverwrite)
            {
                flag = true;
            }
            LiveTimelineKeyCharaMotionData prevMotionA;
            LiveTimelineKeyCharaMotionData prevMotionB;
            if (_isOverwriteMotionTimeline)
            {
                prevMotionB = ((_indexOfPlayingKeyB < 0) ? null : (_overwriteKeys.At(_indexOfPlayingKeyB) as LiveTimelineKeyCharaMotionData));
                prevMotionA = ((_indexOfPlayingKeyA < 0) ? null : (_overwriteKeys.At(_indexOfPlayingKeyA) as LiveTimelineKeyCharaMotionData));
            }
            else
            {
                prevMotionB = ((_indexOfPlayingKeyB < 0) ? null : (_keys.At(_indexOfPlayingKeyB) as LiveTimelineKeyCharaMotionData));
                prevMotionA = ((_indexOfPlayingKeyA < 0) ? null : (_keys.At(_indexOfPlayingKeyA) as LiveTimelineKeyCharaMotionData));
            }
            bool isChangeMotion = false;
            if (_isOverwriteMotionTimeline != flag || _isHeightMotionTimeline != flag2)
            {
                isChangeMotion = true;
                _isOverwriteMotionTimeline = flag;
                _isHeightMotionTimeline = flag2;
            }
            if (flag2)
            {
                b = liveTimelineKeyCharaMotionData;
                indexB = -1;
            }
            DisablePrevMotion(a, prevMotionA, b, prevMotionB, ref isChangeMotion);
            if (isChangeMotion)
            {
                _target.OnMotionChange();
            }
            prevMotionA = a;
            prevMotionB = b;
            _indexOfPlayingKeyB = indexB;
            _indexOfPlayingKeyA = indexA;
            if (prevMotionA == null)
            {
                _target.liveMSQControlled = false;
                return;
            }
            Dictionary<string, bool> controlClips = new Dictionary<string, bool>();
            _target.liveMSQControlled = true;
            float num2 = 1f;
            float num3 = (float)prevMotionA.frame * num;
            AnimationState animationState = GetAnimationState(prevMotionA);
            _target.liveMSQCurrentAnimState = animationState;
            _target.liveMSQCurrentAnimStartTime = num3;
            if (animationState == null)
            {
                return;
            }
            if (prevMotionB != null)
            {
                if (!flag2)
                {
                    float num4 = (float)prevMotionB.frame * num;
                    float num5 = num3 + prevMotionA.playLength - num4;
                    if (!(num5 < 0f))
                    {
                        num2 = 1f - Mathf.Clamp01((currentTime - num4) / num5);
                    }
                }
                else
                {
                    num2 = 1f - _heightBlendRate[0];
                }
            }
            controlClips[animationState.name] = true;
            animationState.enabled = true;
            animationState.speed = 1f;
            if (num2 >= 0.0001f)
            {
                _animation.Blend(prevMotionA.motionName, num2, 0f);
            }
            else
            {
                animationState.enabled = false;
            }
            animationState.wrapMode = (prevMotionA.loop ? WrapMode.Loop : WrapMode.ClampForever);
            float num6 = currentTime - num3 - prevMotionA.motionHeadTime;
            animationState.time = num6 * prevMotionA.playSpeed;
            if (!prevMotionA.loop && num6 < 0f)
            {
                animationState.wrapMode = WrapMode.Loop;
            }
            animationState.speed = 0f;
            if (prevMotionB != null)
            {
                float blendRate = 1f - num2;
                OnMotionBUpdate(prevMotionB, flag2, blendRate, currentTime, num, ref controlClips);
            }
            string[] subtractClipNames = GetSubtractClipNames(_beforeMotionStateName, controlClips);
            foreach (string motionName in subtractClipNames)
            {
                AnimationState animationState2 = GetAnimationState(motionName);
                if (animationState2 != null)
                {
                    animationState2.enabled = false;
                }
            }
            _beforeMotionStateName = controlClips;
        }

        private string[] GetSubtractClipNames(Dictionary<string, bool> before, Dictionary<string, bool> now)
        {
            List<string> list = new List<string>();
            foreach (string key in before.Keys)
            {
                if (!now.ContainsKey(key))
                {
                    list.Add(key);
                }
            }
            return list.ToArray();
        }

        private AnimationState GetAnimationState(LiveTimelineKeyCharaMotionData key)
        {
            return GetAnimationState(key.motionName);
        }

        private AnimationState GetAnimationState(AnimationClip clip)
        {
            if (clip == null)
            {
                return null;
            }
            return GetAnimationState(clip.name);
        }

        private AnimationState GetAnimationState(string motionName)
        {
            if (_animation == null)
            {
                return null;
            }
            AnimationState animationState = _animation[motionName];
            if (animationState != null)
            {
                return animationState;
            }
            return null;
        }
    }
}
