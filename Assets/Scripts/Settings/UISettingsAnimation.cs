using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsAnimation : MonoBehaviour
{
    static UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI ProgressText;
    public Slider ProgressSlider;
    public Button PlayButton;
    public TextMeshProUGUI SpeedText;
    public Slider SpeedSlider;
    public Button VMDButton;

    internal void UpdateAnimationInfo(UmaContainerCharacter umaContainer)
    {
        if (umaContainer.OverrideController["clip_2"].name != "clip_2")
        {
            bool isLoop = umaContainer.OverrideController["clip_2"].name.Contains("_loop");
            var AnimeState = umaContainer.UmaAnimator.GetCurrentAnimatorStateInfo(0);
            var AnimeClip = umaContainer.OverrideController["clip_2"];
            if (AnimeClip && umaContainer.UmaAnimator.speed != 0)
            {
                var normalizedTime = (isLoop) ? Mathf.Repeat(AnimeState.normalizedTime, 1) : Mathf.Min(AnimeState.normalizedTime, 1);
                TitleText.text = AnimeClip.name;
                ProgressText.text = string.Format("{0} / {1}", ToFrameFormat(normalizedTime * AnimeClip.length, AnimeClip.frameRate), ToFrameFormat(AnimeClip.length, AnimeClip.frameRate));
                ProgressSlider.SetValueWithoutNotify(normalizedTime);
            }
        }
    }

    public void Pause()
    {
        var container = Builder.CurrentUMAContainer;
        if (!container || !container.UmaAnimator) return;
        if (Builder.OverrideController.animationClips.Length == 0) return;

        var animator = container.UmaAnimator;
        var animator_face = container.UmaFaceAnimator;
        var animator_cam = Builder.AnimationCameraAnimator;
        var AnimeState = animator.GetCurrentAnimatorStateInfo(0);
        var state = animator.speed > 0f;
        if (state)
        {
            animator.speed = 0;
            if (animator_face)
                animator_face.speed = 0;
            animator_cam.speed = 0;
        }
        else if (AnimeState.normalizedTime < 1f)
        {
            animator.speed = SpeedSlider.value;
            animator_cam.speed = SpeedSlider.value;
            if (animator_face)
                animator_face.speed = SpeedSlider.value;
        }
        else
        {
            animator.speed = SpeedSlider.value;
            animator.Play(0, 0, 0);
            animator.Play(0, 2, 0);
            animator_cam.speed = SpeedSlider.value;
            animator_cam.Play(0, -1, 0);
            if (animator_face)
            {
                animator_face.speed = SpeedSlider.value;
                animator_face.Play(0, 0, 0);
                animator_face.Play(0, 1, 0);
            }
        }
    }

    public void ChangeProgress(float val)
    {
        var container = Builder.CurrentUMAContainer;
        if (!container) return;
        var animator = container.UmaAnimator;
        var animator_face = container.UmaFaceAnimator;
        var animator_cam = Builder.AnimationCameraAnimator;
        if (animator != null)
        {
            var AnimeClip = container.OverrideController["clip_2"];

            // Pause and Seek;
            animator.speed = 0;
            animator.Play(0, 0, val);
            animator.Play(0, 2, val);
            if (animator_cam.runtimeAnimatorController)
            {
                animator_cam.speed = 0;
                animator_cam.Play(0, -1, val);
            }
            if (animator_face)
            {
                animator_face.speed = 0;
                animator_face.Play(0, 0, val);
                animator_face.Play(0, 1, val);
            }

            ProgressText.text = string.Format("{0} / {1}", ToFrameFormat(val * AnimeClip.length, AnimeClip.frameRate), ToFrameFormat(AnimeClip.length, AnimeClip.frameRate));
        }
    }

    public void ChangeSpeed(float val)
    {
        var container = Builder.CurrentUMAContainer;
        SpeedText.text = string.Format("Speed: {0:F2}", val);

        if (!container || !container.UmaAnimator) return;

        container.UmaAnimator.speed = val;
        Builder.AnimationCameraAnimator.speed = val;
        if (container.UmaFaceAnimator)
        {
            container.UmaFaceAnimator.speed = val;
        }
    }

    public void EnableRootMotion(bool enable)
    {
        var container = Builder.CurrentUMAContainer;
        if (!container || !container.UmaAnimator) return;
        container.UmaAnimator.SetLayerWeight(2, enable ? 1 : 0);
    }

    public static string ToFrameFormat(float time, float frameRate)
    {
        int frames = Mathf.FloorToInt(time % 1 * frameRate);
        int seconds = (int)time;
        int minute = seconds % 3600 / 60;
        seconds = seconds % 3600 % 60;
        return string.Format("{0:D2}m:{1:D2}s:{2:D2}f", minute, seconds, frames);
    }
}
