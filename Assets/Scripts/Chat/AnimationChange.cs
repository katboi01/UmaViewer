using System.Linq;
using UnityEngine;
using System.IO;

public class AnimationChange : MonoBehaviour
{
    UmaViewerMain Main => UmaViewerMain.Instance;
    private UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

    void Start()
    {
        //LoadAnimationForCurrentCharacter("anm_eve_type00_hurry01_loop");
    }

    public void LoadAnimationForCurrentCharacter(string animationName)
    {
        if (Builder.CurrentUMAContainer == null)
        {
            Debug.LogError("No character is currently loaded.");
            return;
        }

        // 查找包含所需动画的资源包
        UmaDatabaseEntry animationEntry = Main.AbMotions.FirstOrDefault(entry => Path.GetFileName(entry.Name) == animationName);

        if (animationEntry == null)
        {
            Debug.LogError($"Animation '{animationName}' not found in the available assets.");
            return;
        }

        // 在资源包中加载动画剪辑
        AssetBundle bundle;

        // 检查资源包是否已经加载，如果没有加载，则加载资源包
        if (!Main.LoadedBundles.TryGetValue(animationEntry.Name, out bundle))
        {
            // 加载包含动画剪辑的资源包
            Builder.LoadAsset(animationEntry);

            if (Main.LoadedBundles.TryGetValue(animationEntry.Name, out bundle))
            {
                LoadAnimationClipFromBundle(animationName, animationEntry, bundle);
            }
            //else
            //{
            //    Debug.LogError($"Failed to find loaded AssetBundle: {animationEntry.Name}");
            //}
        }
        else
        {
            LoadAnimationClipFromBundle(animationName, animationEntry, bundle);
        }
    }

    private void LoadAnimationClipFromBundle(string animationName, UmaDatabaseEntry animationEntry, AssetBundle bundle)
    {
        AnimationClip animationClip = bundle.LoadAsset<AnimationClip>(animationEntry.Name);
        if (animationClip != null)
        {
            // 使用现有的LoadAnimation()方法加载动画剪辑
            Builder.LoadAnimation(animationClip);
        }
        else
        {
            Debug.LogError($"Failed to load AnimationClip from the AssetBundle: {animationEntry.Name}");
        }
    }
}