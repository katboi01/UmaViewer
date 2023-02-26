using Cutt;
using UnityEngine;

public class AnimationObjectController : MonoBehaviour
{
    private const int INVALID_ANIMATION_ID = -1;

    private const float INVALID_ANIMATION_TIME = -1f;

    private Animation _animation;

    private AnimationState[] _animationStates;

    private string[] _cachedClipName;

    private int _playAnimationNo;

    private float _playAnimationTime;

    private void Start()
    {
        _animation = GetComponent<Animation>();
        if (_animation != null)
        {
            int clipCount = _animation.GetClipCount();
            if (clipCount > 0)
            {
                _animationStates = new AnimationState[clipCount];
                _cachedClipName = new string[clipCount];
                int num = 0;
                foreach (AnimationState item in _animation)
                {
                    _animationStates[num] = item;
                    _cachedClipName[num] = item.clip.name;
                    num++;
                }
            }
        }
        _animation.clip = null;
        _animation.Stop();
        _playAnimationNo = INVALID_ANIMATION_ID;
        _playAnimationTime = INVALID_ANIMATION_TIME;
    }

    public void UpdateStatus(ref AnimationUpdateInfo updateInfo)
    {
        if (_animationStates == null || updateInfo.animationID < 0 || _animationStates.Length <= updateInfo.animationID)
        {
            return;
        }
        bool flag = false;
        AnimationState animationState;
        if (_playAnimationNo != updateInfo.animationID)
        {
            if (_playAnimationNo != INVALID_ANIMATION_ID)
            {
                animationState = _animationStates[_playAnimationNo];
                if (_animation.IsPlaying(_cachedClipName[_playAnimationNo]))
                {
                    _animation.Stop(_cachedClipName[_playAnimationNo]);
                }
            }
            _playAnimationTime = INVALID_ANIMATION_TIME;
            _playAnimationNo = updateInfo.animationID;
            flag = true;
        }
        animationState = _animationStates[_playAnimationNo];
        float num = updateInfo.offsetTime + updateInfo.progressTime * updateInfo.speed;
        if (_playAnimationTime != num || flag)
        {
            animationState.time = (_playAnimationTime = num);
            _animation.Play(_cachedClipName[_playAnimationNo]);
            animationState.enabled = true;
            _animation.wrapMode = updateInfo.wrapMode;
            _animation.Sample();
            animationState.enabled = false;
        }
    }
}
