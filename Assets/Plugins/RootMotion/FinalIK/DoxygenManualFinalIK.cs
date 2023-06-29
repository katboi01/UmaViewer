/*! \mainpage
 \image html FABRIK.png
 
 Welcome to Final %IK, the ultimate collection of inverse kinematics solutions for Unity.
 
\section contains Final IK Contains 

 - Full Body %IK system for biped characters
 - VRIK - High speed full body solver dedicated to Virtual Reality avatars.
 - Baker - tool for baking your IK procedures, animation adjustments, Timeline and Mecanim layers to Humanoid, Generic or Legacy animation clips
 - Biped %IK - alternate to Unity's built-in Avatar %IK system that provides more flexibility and works in Unity Free with the same API
 - CCD (Cyclic Coordinate Descent) %IK
 - Multi-effector %FABRIK (Forward and Backward Reaching %IK) 
 - LookAt %IK - look-at solver for bipeds
 - Aim %IK - for aiming weapons, looking at objects
 - Limb %IK - 3 joint solver for arms and legs
 - Arm IK - 4 joint solver for the arms
 - Leg IK - 4 joint solver for the legs
 - Rotation constraints - Angular, Polygonal (Reach Cone), Spline and Hinge rotation limits that work with CCD, %FABRIK and Aim solvers
 - Interaction System - simple tool for creating procedural IK interactions
 - Grounder - automatic vertical foot placement and alignment correction system
 - CCDIKJ, AimIKJ - Multithreaded solvers based on AnimationJobs

\section technicaloverview Technical Overview

 - Full source code included
 - Does NOT require Unity Pro
 - Does NOT require, but works with Mecanim
 - Written in C#, all scripts are namespaced under %RootMotion and %RootMotion.FinalIK to avoid any naming conflicts with Your existing assets
 - Tested on Standalone, Web Player, IOS, Android and VR
 - Custom undoable inspectors and scene view handles
 - Warning system to safeguard from null references and invalid setups (will not overflow your console with warnings)
 - Optimized for great performance
 - Modular, easily extendable. Compose your own custom character rigs
 - User manual, HTML documentation, fully documented code, YouTube video tutorials
 - Demo scenes and example scripts for all components
 - Tested on a wide range of characters
 
*/

/*! \page page1 Aim IK
 * AimIK solver is a modification of the CCD algorithm that rotates a hierarchy of bones to make a child Transform of that hierarchy aim at a target.
	  	It differs from the basic built-in Animator.SetLookAtPosition or the LookAtIK functionality, because it is able to accurately aim transforms that are not aligned to the main axis of the hierarchy.
	  	
	  	AimIK can produce very stabile and natural looking retargeting of character animation, it hence has great potential for use in weapon aiming systems. With AimIK we are able to offset a single forward aiming pose or animation to aim at targets even almost behind the character.
	  	It is only the Quaternion singularity point at 180 degrees offset, where the solver can not know which way to turn the spine. Just like LookAtIK, AimIK provides a clampWeight property to avoid problems with that singularity issue.
	  	
	  	AimIK also works with rotation limits, however it is more prone to get jammed than other constrained solvers, should the chain be heavily constrained.
	  	
	  	Aim provides high accuracy at a very good speed, still it is necessary to keep in mind to maintain the target position at a safe distance from the aiming Transform. 
	  	If distance to the target position is less than distance to the aiming Transform, the solver will try to roll in on itself and might be unable to produce a finite result.

\image html AimIK.png "The AimIK solver in action"

<b>Getting started:</b>

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/7__IafZGwvI" frameborder="0" allowfullscreen></iframe>\endhtmlonly
	- Set up your character's Animator/Animation to play an aiming forward animation/pose
	- Add the AimIK component to your character
	- Assign the spine bones to "Bones" in the component, one by one in descending order (parents first).
	- Assign the Aim Transform (the Transform that you want to aim at the target). It could be the gun, the hand bone or just an empty game object parented to the hand
	- Make sure Axis represents the local axis of the Aim Transform that you want to be aimed at the target. For example if the blue (z) axis of a gun is pointing towards it's barrel, you will need to set Axis to (0, 0, 1).
	- Set weight to 1, press Play
	- Move the target handle around in Scene View to see how AimIK behaves

<b>Component variables:</b>
	- <b>fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance

<b>Solver variables:</b>
	- <b>target</b> - the target Transform. If assigned, solver IKPosition will be automatically set to the position of the target.
	- <b>poleTarget</b> - if assigned, will automatically set polePosition to the position of this Transform. IKSolverAim.polePosition keeps another axis (poleAxis) of the Aim Transform oriented towards IKSolverAim.polePosition.
	- <b>transform</b> - (Aim Transform) the Transform that we want to aim at the IKPosition (usually the gun or the flashlight, represented as the pink cone on the image above).
	- <b>axis</b> - the local axis of the Aim Transform that you want to be aimed at the IKPosition. For example if the blue (z) axis of a gun is pointing towards it's barrel, you will need to set Axis to (0, 0, 1).
	- <b>poleAxis</b> - the local axis of the Aim Transform that you want to keep oriented towards IKSolverAim.polePosition
	- <b>weight</b> - the solver weight for smoothly blending out the effect of the IK
	- <b>poleWeight</b> - the weight of keeping the poleAxis of the AimTransform oriented towards polePosition.
	- <b>tolerance</b> - minimum offset from last reached angle. Will stop solving if difference from previous reached angle is less than tolerance. If tolerance is zero, will iterate until maxIterations.
	- <b>maxIterations</b> - max iterations per frame. If tolerance is 0, will always iterate until maxIterations
	- <b>clampWeight</b> - clamping rotation of the solver. 0 is free rotation, 1 is completely clamped to zero effect
	- <b>clampSmoothing</b> - the number of sine smoothing iterations applied to clamping to make it smoother.
	- <b>bones</b> - bones used by the solver to orient the Aim Transform to the target. All bones need to be direct ancestors of the Aim Transform and sorted in descending order.
	You can skip bones in the hierarchy and the Aim Transform itself can also be included. The bone hierarchy can not be branched, meaning you cant assing bones from both hands. Bone weight determines how strongly it is used in bending the hierarchy

\image html AimIKComponent.png

<b>Script References:</b>
	- <a href="http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_i_k_solver_aim.html">Solver </a> 
	- <a href="http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_aim_i_k.html">Component</a> 

<b>Changing the aiming target:</b>

\code
	public AimIK aimIK;
	
	void LateUpdate () {
		aimIK.solver.IKPosition = something;
	}
\endcode

<b>Using the Pole:</b>

The polePosition can be helpful when making weapon aiming setups. Let's say we have a gun that's local Z axis is facing towards it's barrel and local Y axis is facing up.
In this case we have to set AimIK "Axis" to (0, 0, 1) and "Pole Axis" to (0, 1, 0). If we now play the scene and set "Weight" and "Pole Weight" to 1, we will have 2 handles, 
one for the aiming target and the other for twisting the gun and the body of the character.

Adjusting the Pole by script:

\code
	public AimIK aimIK;
	
	void LateUpdate () {
		aimIK.solver.polePosition = something;
		aimIK.solver.poleWeight = 1f;
	}
\endcode

<b>Changing the Aim Transform:</b>

\code
	public AimIK aimIK;
	
	void LateUpdate () {
		aimIK.solver.transform = something;
		aimIK.solver.axis = localAxisOfTheTransformToAimAtTheTarget;
	}
\endcode
		

<b>Adding AimIK in runtime:</b>
		- Add the AimIK component via script
		- Call AimIK.solver.SetChain()

<b>Changing AimIK bone hierarchy in runtime:</b>
\code
public AimIK aimIK;

public Transform[] newBones;

void Change() {
    aimIK.solver.SetChain(newBones, aim.transform);
}
\endcode

<b>Using AimIK with Rotation Limits:</b>
<BR> It is sometimes necessary to limit the effect of AimIK on one of the bones in it's chain. 
Usually when you wish to use the elbow joint in the process of aiming a single handed weapon or when you wish to limit the rotation of a spine bone to twisting only.
If you just added the RotationLimit component to the bone, it would also interfere with the animation (keep the spine stiff), not just the IK.
You can make the RotationLimit only have an effect on the AimIK by defaulting it's rotation each frame before AimIK solves:

\code
public AimIK aimIK;

void LateUpdate() {
	// Set current animated localRotation as default local rotation for the rotation limits so they will not interfere with the animation, but only the IK
	for (int i = 0; i < aimIK.solver.bones.Length; i++) {
		if (aimIK.solver.bones[i].rotationLimit != null) {
			aimIK.solver.bones[i].rotationLimit.SetDefaultLocalRotation();
		}
	}
}
\endcode

Please note that each RotationLimit decreases the probability of the solver smoothly reaching it's target.
<BR> Since FinalIK 0.4 introduced the polePosition and poleWeight, using Rotation Limits on characters can in most cases be avoided by using the pole to keep the body upright.

<b>Bone weights</b>
<BR> Each bone in the "Bones" has a weight parameter. It determines how much proportionally is a bone used in the solving process. 
Fox example if you do not wish a certain spine bone to bend too much, you can just decrease it's weight. 

<b>Aiming 2-handed weapons:</b>
<BR> When aiming 2-handed weapons, we can use only the spine bones (common parents for both hands) in the AimIK bone hierarchy. If we used the arm bones, the other hand would loose contact with the object.
Sometimes using just the spine bones is not enough though, as the spine would bend exessively and the character would end up in unnatural poses. We can solve this problem, by adding some of the arm bones (the arm that is holding the object) to AimIK and
then use FullBodyBipedIK or LimbIK to put the other hand back on its place after AimIK is done. Take a look at this <a href="https://www.youtube.com/watch?v=5DlTjasmTLk">tutorial video</a> to see how it could be done.

<b>Redirecting animation:</b>
<BR> AimIK is perfect for keeping objects steadily aimed at the target. Sometimes those objects have a lot of swinging motion in the animation, like swinging a sword for example,
and it is not good to use AimIK to keep the sword oriented at a certain position during that swing. It would keep the sword orientation fixed by bending the rest of the hierarchy and that would interfere with the animation in an unwanted way.
It is still possible to use AimIK to redirect swinging animations like swordplay or punching, take a look at this <a href="https://www.youtube.com/watch?v=OhCtiV5r8HA">tutorial video</a> to see how it could be done.

<b>Recoil/reload animations while aiming:</b>
<BR> While AimIK weight is 1, the solver will maintain the weapon oriented at the target at all times. This might not be the desired behaviour while playing a recoil or reloading animation.
We can dynamically change the Axis of AimIK to overcome this issue.

\code
void LateUpdate() {
        aimIK.solver.axis = aimIK.solver.transform.InverseTransformDirection(character.forward);
}
\endcode

That line is telling AimIK that whatever the direction of the weapon in the animation, it is the default forward aiming direction. 
"Character.forward" is the direction that the weapon is aimed at (keep it in character space so the effect rotates with the character) in the normal aiming animation without any recoil, so If you were currently playing an "aim right" animation, you should set it to "character.right" instead.
*/

/*! \page page2 Arm IK
ArmIK is a wrapper component for VRIK's 4-joint biped arm solver.

<b>Component variables:</b>
    - <b>ArmIK.fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance.

<b>Wrapper variables:</b>
    - <b>ArmIK.solver.chest</b> - the last spine bone, that the arm bones must be parented to.
    - <b>ArmIK.solver.shoulder</b> - the shoulder bone.
    - <b>ArmIK.solver.upperArm</b> - the upper arm bone.
    - <b>ArmIK.solver.forearm</b> - the forearm bone
    - <b>ArmIK.solver.hand</b> - the hand bone.
    - <b>ArmIK.solver.IKPositionWeight</b> - positional weight of the hand target. Note that if you have nulled the target, the hand will still be pulled to the last position of the target until you set this value to 0.
    - <b>ArmIK.solver.IKRotationWeight</b> - rotational weight of the hand target. Note that if you have nulled the target, the hand will still be rotated to the last rotation of the target until you set this value to 0.
    
<b>Solver variables:</b>
    - <b>ArmIK.solver.arm.target</b> - the hand target. This should not be the hand controller itself, but a child GameObject parented to it so you could adjust it's position/rotation to match the orientation of the hand bone. The best practice for setup would be to move the hand controller to the avatar's hand as it it was held by the avatar, duplicate the avatar's hand bone and parent it to the hand controller. Then assign the duplicate to this slot.
    - <b>ArmIK.solver.arm.bendGoal</b> - the elbow will be bent towards this Transform if 'Bend Goal Weight' > 0.
    - <b>ArmIK.solver.arm.shoulderRotationMode</b> - different techniques for shoulder bone rotation.
    - <b>ArmIK.solver.arm.shoulderRotationWeight</b> - the weight of shoulder rotation.
    - <b>ArmIK.solver.arm.shoulderTwistWeight</b> - the weight of twisting the shoulders backwards when arms are lifted up.
    - <b>ArmIK.solver.arm.bendGoalWeight</b> - if greater than 0, will bend the elbow towards the 'Bend Goal' Transform.
    - <b>ArmIK.solver.arm.swivelOffset</b> - angular offset of the elbow bending direction.
    - <b>ArmIK.solver.arm.wristToPalmAxis</b> - local axis of the hand bone that points from the wrist towards the palm. Used for defining hand bone orientation. If you have copied VRIK component from another avatar that has different bone orientations, right-click on VRIK header and select "Guess Hand Orientations" from the context menu.
    - <b>ArmIK.solver.arm.palmToThumbAxis</b> - local axis of the hand bone that points from the palm towards the thumb. Used for defining hand bone orientation If you have copied VRIK component from another avatar that has different bone orientations, right-click on VRIK header and select 'Guess Hand Orientations' from the context menu..
    - <b>ArmIK.solver.arm.armLengthMlp</b> - use this to make the arm shorter/longer. Works by displacement of hand and forearm localPosition.
    - <b>ArmIK.solver.arm.stretchCurve</b> - evaluates stretching of the arm by target distance relative to arm length. Value at time 1 represents stretching amount at the point where distance to the target is equal to arm length. Value at time 2 represents stretching amount at the point where distance to the target is double the arm length. Value represents the amount of stretching. Linear stretching would be achieved with a linear curve going up by 45 degrees. Increase the range of stretching by moving the last key up and right at the same amount. Smoothing in the curve can help reduce elbow snapping (start stretching the arm slightly before target distance reaches arm length). To get a good optimal value for this curve, please go to the 'VRIK (Basic)' demo scene and copy the stretch curve over from the Pilot character.
   
\image html ArmIKComponent.png


<b>Script References:</b>
	- <a href="http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_i_k_solver_arm.html">Solver </a> 
	- <a href="http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_arm_i_k.html">Component</a> 
*/

/*! \page page3 Baker
Baker is a universal tool for recording Humanoid, Generic and Legacy animation clips. Baker.cs is an abstract base class extended by HumanoidBaker.cs and GenericBaker.cs.
<BR>
<b>Baker allows you to:</b>
    - Edit animation clips
    - bake IK into animation
    - combine animation layers
    - bake InteractionSystem interactions
    - bake ragdoll physics
    - blend ragdoll physics with animation (PuppetMaster)
    - bake Timeline
    - fix Humanoid retargeting problems (hand, foot positions)
    - bake outsourced Humanoid animation to specific character model to avoid using Foot IK for performance
    - bake motion capture
    - reduce animation memory footprint with Baker's advanced keyframe reduction tools

<b>Getting started:</b>

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/yF7EyomgZKo" frameborder="0" allowfullscreen></iframe>\endhtmlonly
    - Add the HumanoidBaker or GenericBaker component to the same GameObject as your character's Animator.
    - Click on the button beneath "Save To Folder" to choose a folder for the baked animation clips.
    - Play the scene.
    - Click on the bake button.

<b>Animation Clips Mode</b>
<BR>AnimationClips mode can be used to bake a batch of AnimationClips directly without the need of setting up an AnimatorController. 
<BR>Set the Baker to "Animation Clips" mode and add some animation clips to the "Animation Clips" array. "Append Name" is a string that will be added to each clip's name for the saved clip. For example if your animation clip names were 'Idle' and 'Walk', then with '_Baked' as Append Name, the Baker will create 'Idle_Baked' and 'Walk_Baked' animation clips.

<b>Animation States Mode</b>
<BR>AnimationStates mode can be used to bake a batch of Mecanim Animation States. This is useful for when you need to set up a more complex rig with layers and AvatarMasks in Mecanim.
<BR>Set the Baker to "Animation States" mode and add the Animation State names (only from the base layer) you wish to bake to the "Animation States" array. "Append Name" is a string that will be added to each state's name for the saved clip. For example if your animation state names were 'Idle' and 'Walk', then with '_Baked' as Append Name, the Baker will create 'Idle_Baked' and 'Walk_Baked' animation clips.

<b>Playable Director Mode</b>
<BR>Bakes a PlayableDirector with a Timeline asset. Set the Baker to "Playable Director" mode and make sure the GameObject has a valid PlayableDirector.

<b>Realtime Mode</b>
<BR>Useful for baking an arbitrary range of animation, such as ragdoll simulation or an InteractionSystem interaction, in real-time.

\code
using RootMotion;

public class MyBakingTool {

    public Baker baker;
    public float bakeDuration = 5f;

    void Start()
    {
        baker.mode = Baker.Mode.Realtime;
        StartCoroutine(BakeDuration(bakeDuration));
    }

    private IEnumerator BakeDuration(float duration) 
    {
        baker.StartBaking();

        yield return new WaitForSeconds(duration);
        baker.StopBaking();
    }
}
\endcode


<b>Keyframe Reduction</b>
<BR> While developing a mobile game and running low on RAM, the quality of your animation will suffer a lot should you try to increase the keyframe reduction error thresholds in animation import settings.
The most noticable problem will be with the feet starting to float around in idle animations as there are not enough keyframes to make them appear properly planted. Idle animations are also usually the longest and most memory consuming.
Unity actually already contains a solution for this problem, but it is simply not implemented in the keyframe reduction options. That solution is the humanoid "Foot IK" system ("Foot IK" toggle in the Animator, previewable by enabling the "IK" toggle in the Animation Preview Window), that is normally used for fixing Humanoid retargeting problems.
Foot IK works by storing position and rotation channels of the foot bottoms from the original animation clip, then running a pass of IK on the legs of the retargeted character (that has different leg bone lenghts) to properly plant the feet.
Unity keyfame reduction options do not allow you to define different parameters for those IK curves and so they will be reduced just the same as the muscle channels, causing the feet to float even if you had "Foot IK" enabled.


Baker allows you define "Key Reduction Error" and "IK Key Reduction Error" separately, hence apply extreme key reduction to muscle channels that take most of the memory while maintaining normal reduction for the IK channels.
It also allows you to bake muscle channels at a lower frame rate than the IK channels by increasing the value of "Muscle Frame Rate Div". Those options can help you achieve better animation quality for a memory cost that is multiple times lower.
The same technique can be used for hand IK, making sure your 2-handed prop animations do not suffer from keyframe reduction.

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/sH7OnpOQCg8" frameborder="0" allowfullscreen></iframe>\endhtmlonly

<b>Changing AnimationClipSettings</b>
<BR> When you first bake an animation clip, Baker will apply the default settings. If you modified the settings of the animation clip manually and baked again, Baker will maintain those modified settings so you will not have to apply them again.

<b>HumanoidBaker variables</b>
    - <b>frameRate</b> - in AnimationClips, AnimationStates or PlayableDirector mode - the frame rate at which the animation clip will be sampled. In Realtime mode - the frame rate at which the pose will be sampled. With the latter, the frame rate is not guaranteed if the player is not able to reach it.
    - <b>keyReductionError</b> - maximum allowed error for keyframe reduction.
    - <b>IKKeyReductionError</b> - max keyframe reduction error for the Root.Q/T, LeftFoot IK and RightFoot IK channels. Having a larger error value for 'Key Reduction Error' and a smaller one for this enables you to optimize clip data size without the floating feet effect by enabling 'Foot IK' in the Animator.
    - <b>muscleFrameRateDiv</b> - frame rate divider for the muscle curves. If you had 'Frame Rate' set to 30, and this value set to 3, the muscle curves will be baked at 10 fps. Only the Root Q/T and Hand and Foot IK curves will be baked at 30. This enables you to optimize clip data size without the floating feet effect by enabling 'Foot IK' in the Animator.
    - <b>bakeHandIK</b> - should the hand IK curves be added to the animation? Disable this if the original hand positions are not important when using the clip on another character via Humanoid retargeting.
    - <b>mode</b> - AnimationClips mode can be used to bake a batch of AnimationClips directly without the need of setting up an AnimatorController. AnimationStates mode is useful for when you need to set up a more complex rig with layers and AvatarMasks in Mecanim. PlayableDirector mode bakes a Timeline. Realtime mode is for continuous baking of gameplay, ragdoll phsysics or PuppetMaster dynamics.
    - <b>loop</b> - sets the baked animation clip to loop time and matches the last frame keys with the first. Note that when overwriting a previously baked clip, AnimationClipSettings will be copied from the existing clip.
    - <b>animationClips</b> - AnimationClips to bake.
    - <b>animationStates</b> - the name of the AnimationStates (must be on the base layer) to bake in the Animator (Right-click on this component header and select 'Find Animation States' to have Baker fill those in automatically, required that state names match with the names of the clips used in them).
    - <b>appendName</b> - string that will be added to each clip or animation state name for the saved clip. For example if your animation state/clip names were 'Idle' and 'Walk', then with '_Baked' as Append Name, the Baker will create 'Idle_Baked' and 'Walk_Baked' animation clips.
    - <b>saveToFolder</b> - the folder to save the baked AnimationClips to.

\image html HumanoidBakerComponent.png

<b>GenericBaker variables</b>
    - <b>frameRate</b> - in AnimationClips, AnimationStates or PlayableDirector mode - the frame rate at which the animation clip will be sampled. In Realtime mode - the frame rate at which the pose will be sampled. With the latter, the frame rate is not guaranteed if the player is not able to reach it.
    - <b>keyReductionError</b> - maximum allowed error for keyframe reduction.
    - <b>markAsLegacy</b> - if true, produced AnimationClips will be marked as Legacy and usable with the Legacy animation system.
    - <b>root</b> - root Transform of the hierarchy to bake.
    - <b>rootNode</b> - root node used for root motion.
    - <b>ignoreList</b> - list of Transforms to ignore, rotation curves will not be baked for these Transforms.
    - <b>bakePositionList</b> - localPosition curves will be baked for these Transforms only. If you are baking a character, the pelvis bone should be added to this array.
    - <b>mode</b> - AnimationClips mode can be used to bake a batch of AnimationClips directly without the need of setting up an AnimatorController. AnimationStates mode is useful for when you need to set up a more complex rig with layers and AvatarMasks in Mecanim. PlayableDirector mode bakes a Timeline. Realtime mode is for continuous baking of gameplay, ragdoll phsysics or PuppetMaster dynamics.
    - <b>loop</b> - sets the baked animation clip to loop time and matches the last frame keys with the first. Note that if you are overwriting a previously baked clip, AnimationClipSettings will be copied from the existing clip.
    - <b>animationClips</b> - AnimationClips to bake.
    - <b>animationStates</b> - the name of the AnimationStates (must be on the base layer) to bake in the Animator (Right-click on this component header and select 'Find Animation States' to have Baker fill those in automatically, required that state names match with the names of the clips used in them).
    - <b>appendName</b> - string that will be added to each clip or animation state name for the saved clip. For example if your animation state/clip names were 'Idle' and 'Walk', then with '_Baked' as Append Name, the Baker will create 'Idle_Baked' and 'Walk_Baked' animation clips.
    - <b>saveToFolder</b> - the folder to save the baked AnimationClips to.

\image html GenericBakerComponent.png


<b>Script References:</b>
	- <a href="http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_humanoid_baker.html">HumanoidBaker</a> 
	- <a href="http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_generic_baker.html">GenericBaker</a> 
*/

/*! \page page4 Biped IK

%IK system for standard biped characters that is designed to replicate and enhance the behaviour of the Unity's built-in character %IK setup.

<b>BipedIK or FullBodyBipedIK?</b>
<BR>Originally the only benefit of BipedIK over FullBodyBipedIK was it's much better performance. However, since FinalIK 0.4, we are able to set FBBIK solver iteration count to 0, in which case the full body effect will not be solved and it is almost as fast as BipedIK. 
This allows for much easier optimization of IK on characters in the distance. Therefore, since 0.4, FullBodyBipedIK component is the recommended solution for solving biped characters. 

<b>BipedIK or Unity's Animator %IK?</b>
- Animator %IK does not allow the modifiaction of any of even the most basic solver parameters, such as limb bend direction, 
which makes the system difficult, if not impossible to use or extend in slightly more advanced use cases. Even in the simplest of cases, Animator can produce unnatural poses or bend a limb in unwanted direction and there is nothing that can be done to work around the problem. 
- Animator I%K lacks a spine solver.
- Animator's LookAt functionality can often solve to weird poses such as bending the spine backwards when looking over the shoulder.
- BipedIK also incorporates AimIK.
- BipedIK does NOT require Unity Pro.
	  	
To simplify migration from Unity's built-in Animator %IK, BipedIK supports the same API, so you can just go from animator.SetIKPosition(...) to bipedIK.SetIKPosition(...).
	  	
BipedIK, like any other component in the FinalIK package, goes out of it's way to minimize the work required for set up. 
BipedIK automatically detects the biped bones based on the bone structure of the character and the most common naming conventions, 
so unless you have named your bones in Chinese, you should have BipedIK ready for work as soon as you can drop in the component. If BipedIK fails to recognize the bone references or you just want to change them, you can always manage the references from the inspector.

\image html BipedIK.png

<b>Getting started:</b>
- Add the BipedIK component to the root of your character (the same GameObject that has the Animator/Animation component)
- Make sure the auto-detected biped references are correct
- Press play, weigh in the solvers

<b>Accessing the solvers of Biped IK:</b>

\code
	public BipedIK bipedIK;
	
	void LateUpdate () {
		bipedIK.solvers.leftFoot.IKPosition = something;
		bipedIK.solvers.spine.IKPosition = something;
		...
	}
\endcode

<b>Adding BipedIK in runtime:</b>
- Add the BipedIK component via script
- Assign BipedIK.references
- Optionally call BipedIK.SetToDefaults() to set the parameters of the solvers to default BipedIK values. Otherwise default values of each solver are used.

<b>Component variables:</b>
	- <b>fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance
	- <b>references</b> - references to the character bones that BipedIK needs to build it's solver.
	
<b>Solver variables:</b>
	- <a href="page12.html">Left Foot</a>
	- <a href="page12.html">Right Foot</a>
	- <a href="page12.html">Left Hand</a>
	- <a href="page12.html">Right Hand</a>
	- <a href="page5.html">Spine</a> 
	- <a href="page1.html">Aim</a> 
	- <a href="page13.html">Look At</a> 
	- <b>Pelvis</b> - Pos Offset and Rot Offset can be used to offset the pelvis of the character from it's animated position/rotation. Pos Weight and Rot Weight can be used to translate and rotate the pelvis to bipedIK.solvers.pelvis.position and bipedIK.solvers.pelvis.rotation.


\image html BipedIKComponent.png
*/

/*! \page page5 CCD IK
CCD (Cyclic Coordinate Descent) is one of the simplest and most popular inverse kinematics methods that has been extensively used in the computer games industry. The main idea behind the solver is to align one joint with the end effector and the target at a time, so that the last bone of the chain iteratively gets closer to the target.
CCD is very fast and reliable even with rotation limits applied. CCD tends to overemphasise the rotations of the bones closer to the target position (a very long CCD chain would just roll in around it's target). Reducing bone weight down the hierarchy will compensate for this effect.
It is designed to handle serial chains, thus, it is difficult to extend to problems with multiple end effectors (in this case go with FABRIK). It also takes a lot of iterations to fully extend the chain.

Monitoring and validating the %IK chain each frame would be expensive on the performance, therefore changing the bone hierarchy in runtime has to be done by calling SetChain (Transform[] hierarchy) on the solver. SetChain returns true if the hierarchy is valid.
CCD allows for direct editing of it's bones' rotations (not by the scene view handles though), but not positions, meaning you can write a script that rotates the bones in a CCD chain each frame, but you should not try to change the bone positions like you can do with a FABRIK solver.
You can, however, rescale the bones at will, CCD does not care about bone lengths.

<b>Getting started:</b>
		- Add the CCDIK component to the first GameObject in the chain
		- Assign all the elements in the chain to "Bones" in the component. Parents first, bones can be skipped.
		- Press Play, set weight to 1

<b>Changing the target position:</b>

\code
	public CCDIK ccdIK;
	
	void LateUpdate () {
		ccdIK.solver.IKPosition = something;
	}
\endcode

<b>Adding CCDIK in runtime:</b>
- Add the CCDIK component via script
- Call CCDIK.solver.SetChain()

<b>Using CCD with Rotation Limits:</b>
<BR>Simply add a Rotation Limit component (RotationLimitAngle, RotationLimitHinge, RotationLimitPolygonal or RotationLimitSpline) to a bone that has been assigned to the "Bones" of the CCDIK component.
Note that each rotation limit decreases the stability and continuity of the solver. If CCDIK is unable to solve a highly constrained chain at certain target positions, it is most likely not a bug with FinalIK, 
but a fundamental handicap of the CCD algorithm (remember, no IK algorithm is perfect).

\image html CCD.png "CCD with rotation limits applied"

<b>Component variables:</b>
	- <b>fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance

<b>Solver variables:</b>
	- <b>target</b> - the target Transform. If assigned, solver IKPosition will be automatically set to the position of the target.
	- <b>weight</b> - the solver weight for smoothly blending out the effect of the IK
	- <b>tolerance</b> - minimum distance from last reached position. Will stop solving if difference from previous reached position is less than tolerance. If tolerance is zero, will iterate until maxIterations.
	- <b>maxIterations</b> - max iterations per frame. If tolerance is 0, will always iterate until maxIterations
	- <b>useRotationLimits</b> - if true, will use any RotationLimit component attached to the bones
	- <b>bones</b> - bones used by the solver to reach to the target. All bones need to be sorted in descending order (parents first). Bones can be skipped in the hierarchy. The bone hierarchy can not be branched, meaning you cant assing bones from both hands. Bone weight determines how strongly it is used by the solver.

\image html CCDComponent.png

*/

/*! \page page6 FABRIK
Forward and Backward Reaching Inverse Kinematics solver based on the paper: 
<BR><a href="http://andreasaristidou.com/publications/FABRIK.pdf">"FABRIK: A fast, iterative solver for the inverse kinematics problem." </a> 
<BR>Aristidou, A., Lasenby, J. Department of Engineering, University of Cambridge, Cambridge CB2 1PZ, UK.

FABRIK is a heuristic solver that can be used with any number of bone segments and rotation limits. It is a method based on forward and backward iterative movements by finding a joint's new position along a line to the next joint. 
FABRIK proposes to solve the %IK problem in position space, instead of the orientation space, therefore it demonstrates less continuity under orientation constraints than CCD, although certain modifications have been made to the constraining method described in the original paper to improve solver stability.
It generally takes less iterations to reach the target than CCD, but is slower per iteration especially with rotation limits applied.

FABRIK is extremely flexible, it even allows for direct manipulation of the bone segments in the scene view and the solver will readapt. Bone lengths can also be changed in runtime.
Monitoring and validating the %IK chain each frame would be expensive on the performance, therefore changing the bone hierarchy in runtime has to be done by calling SetChain (Transform[] hierarchy) on the solver. SetChain returns true if the hierarchy is valid.

<b>Getting started:</b>
		- Add the FABRIK component to the first GameObject in the chain
		- Assign all the elements in the chain to "Bones" in the component
		- Press Play, set weight to 1

<b>Changing the target position:</b>

\code
	public FABRIK fabrik;
	
	void LateUpdate () {
		fabrik.solver.IKPosition = something;
	}
\endcode

<b>Adding FABRIK in runtime:</b>
- Add the FABRIK component via script
- Call FABRIK.solver.SetChain()

<b>Using FABRIK with Rotation Limits:</b>
<BR>Simply add a Rotation Limit component (RotationLimitAngle, RotationLimitHinge, RotationLimitPolygonal or RotationLimitSpline) to a bone that has been assigned to the "Bones" of the FABRIK component.
Note that each rotation limit decreases the stability and continuity of the solver. If FABRIK is unable to solve a highly constrained chain at certain target positions, it is most likely not a bug with FinalIK, 
but a fundamental handicap of the FABRIK algorithm (remember, no IK algorithm is perfect).

\image html FABRIK.png

<b>Component variables:</b>
	- <b>fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance

<b>Solver variables:</b>
	- <b>target</b> - the target Transform. If assigned, solver IKPosition will be automatically set to the position of the target.
	- <b>weight</b> - the solver weight for smoothly blending out the effect of the IK
	- <b>tolerance</b> - minimum distance from last reached position. Will stop solving if difference from previous reached position is less than tolerance. If tolerance is zero, will iterate until maxIterations.
	- <b>maxIterations</b> - max iterations per frame. If tolerance is 0, will always iterate until maxIterations
	- <b>useRotationLimits</b> - if true, will use any RotationLimit component attached to the bones
	- <b>bones</b> - bones used by the solver to reach to the target. All bones need to be sorted in descending order (parents first). Bones can be skipped in the hierarchy. The bone hierarchy can not be branched, meaning you cant assing bones from both hands. Bone weight determines how strongly it is used by the solver.

\image html FABRIKComponent.png
*/

/*! \page page7 FABRIK Root
Multi-effector FABRIK system.
<BR>FABRIKRoot is a component that connects FABRIK chains together to form extremely complicated %IK systems with multiple branches, end-effectors and rotation limits.

<b>Getting started:</b>
		- Create multiple FABRIK chains, position them as you want them to be connected. The chains don't have to be parented to each other
		- Make sure the first bone of a child chain is in the same position as the last bone of it's parent
		- Create a new GameObject, add the FABRIKRoot component
		- Add all the FABRIK chains to "Chains" in the FABRIKRoot component
		- Press Play

<b>Accessing the chains of FABRIKRoot:</b>

\code
	public FABRIKRoot fabrikRoot;
	
	void LateUpdate () {
		Debug.Log(fabrikRoot.solver.chains[index].ik.name);
	}
\endcode

<b>Limitations:</b>
- Separate FABRIK chains can not use the same bones, they must be fully independent
- The last bone of a FABRIK chain must be in the same position as it's child chain's first bone
		
\image html FABRIKRoot.png "FABRIK Root chain being pulled"

<b>Component variables:</b>
	- <b>fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance

<b>Solver variables:</b>
	- <b>weight</b> - the solver weight for smoothly blending out the effect of the IK
	- <b>iterations</b> - max iterations per frame.
	- <b>rootPin</b> - the weight of pinning the first chains to the Transform that the component is attached to
	- <b>chains</b> - the list of FABRIK components used by this FABRIKRoot

<b>Chain variables:</b>
	- <b>IK</b> - the FABRIK component used
	- <b>pull</b> - parent pull weight. How much does this chain pull it's parent?
	- <b>pin</b> - resistance to being pulled by child chains. If 1, this chain can not be pulled out of place by it's child chains
	- <b>children</b> - indexes (of the "Chains" array) of the direct children of this chain. Don't add children of the children.
	
\image html FABRIKRootComponent.png

*/

/*! \page page8 Full Body Biped IK
Final %IK includes an extremely flexible and powerful high speed lightweight FBIK solver for biped characters.

FullBodyBipedIK maps any biped character to a low resolution multi-effector IK rig, solves it, and maps the result back to the character. 
This is done each frame in LateUpdate, after Mecanim/Legacy is done animating, so it is completely independent from the animating system.

<b>Chains:</b>
		Internally, each limb and the body are instances of the FBIKChain class. The root chain is the body, consisting of a single node, and the limbs are it's children. 
		This setup forms the multi-effector IK tree around the root node.

<b>Nodes:</b>
		Nodes are members of the Chains. For instance, an Arm chain contains three nodes - upper arm, forearm and the hand. Each node maintains a reference to it's bone (node.transform).
		When the solver is processing or has finished, the solved position of the bone is stored in node.solverPosition.

<b>Effectors:</b>
		FullBodyBipedIK has three types of effectors - end-effectors (hands and feet), mid-body effectors (shoulders and thighs) and multi-effectors (the body). 
		End-effectors can be rotated while changing the rotation of mid-body and multi-effectors has no effect. Changing end-effector rotation also changes the bending direction of the limb (unless you are using bend goals to override it).
		The body effector is a multi-effector, meaning it also drags along both thigh effectors (to simplify positioning of the body).
		Effectors also have the positionOffset property that can be used to very easily manupulate with the underlaying animation. Effectors will reset their positionOffset to Vector3.zero after each solver update.

<b>Pulling, Reaching and Pushing:</b>
		Each chain has the "pull" property. When all chains have pull equal to 1, pull weight is distributed equally between the limbs. That means reaching all effectors is not quaranteed if they are very far from each other. 
		The result can be adjusted or improved by changing the "reach" parameter of the chain, increasing the solver iteration count or updating the solver more than once per frame.
		However, when for instance the left arm chain has pull weight equal to 1 and all others have 0, you can pull the character from it's left hand to Infinity without losing contact.
		The Push and Push Parent values determine how much a limb transfers energy to it's parent nodes when the target is in reach. Experiment with those values in the Scene view to get a better understanding of how they behave.

<b>Mapping:</b>
		IKSolverFullBodyBiped solves a very low resolution high speed armature. Your character probably has a lot more bones in it's spine though, it might have twist bones in the arms and shoulder or hip bones and so on. Therefore, the solver needs to map the high resolution
		skeleton to the low resolution solver skeleton before solving and vice versa after the solver has finished. There are 3 types of mappers - IKMappingSpine for mapping the pelvis and the spine, IKMappingLimb for the limbs (including the clavicle) and IKMappingBone for the head.
		You can access them through IKSolverFullBody.spineMapping, IKSolverFullBody.limbMappings and IKSolverFullBody.boneMappings

\image html FullBodyBipedIK.png "Retargeting a single punching animation with FullBodyBipedIK"

<b>Limitations:</b>
		- FullBodyBipedIK does not have effectors for the fingers and toes. Solving fingers with IK would be an overkill in most cases as there are only so few poses for the hands in a game. 
		Using 10 4-segment constrained CCD or FABRIK chains to position the fingers however is probably something you don't want to waste your precious milliseconds on. 
		See the Driving Rig demo to get an idea how to very quickly (and entirely in Unity) pose the fingers to an object.
		- FullBodyBipedIK samples the initial pose of your character (in Start() and each time you re-initiate the solver) to find out which way the limbs should be bent. Hence the limitation - the limbs of the character at that moment should be bent in their natural directions.
		Some characters however are in geometrically perfect T-Pose, meaning their limbs are completely straight. Some characters even have their limbs bent slightly in the inverse direction (some Mixamo rigs for example).
		FullBodyBipedIK will alarm you should this problem occur. All you will have to do, is rotate the forearm or calf bones in the Scene view slightly in the direction they should be bent. 
		Since those rotations will be overwritten in play mode by animation anyway, you should not be afraid of messing up your character.
		- FullBodyBipedIK does not have elbow/knee effectors. That might change in the future should there be a practical demand for them. Elbow and knee positions can still be modified though as bend goals are supported.
		- Optimize Game Objects should be disabled or at least all the bones needed by the solver (FullBodyBipedIK.references) exposed.
		- Additional bones in the limbs are supported as long as their animation is twisting only. If the additional bones have swing animation, like for example wing bones, FBBIK will not solve the limb correctly.
		- FullBodyBipedIK does not rotate the shoulder bone when the character is pulled by the hand. It will maintain the shoulder bone rotation relative to the chest as it is in the animation. 
		In most cases, it is not a problem, but sometimes, especially when reaching for something above the head, having the shoulder bone rotate along would make it more realistic. 
		In this case you should either have an underlaying reach up (procedural) animation that rotates the shoulder bone or it can also be rotated via script before the IK solver reads the character's pose.
		There is also a workaround script included in the demos, called ShoulderRotator, just add it to the FBBIK game object.
		- When you move a limb end-effector and the effector rotation weight is 0, FBBIK will try to maintain the bending direction of the limb as it is animated. When the limb rotates close to 180 degrees from it's animated direction, you will start experiencing rolling of the limb, meaning, the solver has no way to know at this point of singularity, which way to rotate the limb. 
		Therefore if you for example have a walking animation, where the hands are down and you want to use IK to grab something from directly above the head, you will have to take the inconvenience to also animate the effector rotation or use a bend goal, to make sure the arm does not roll backwards when close to 180 degrees of angular offset. 
		This is not a bug, it is a logical inevitability if we want to maintain the animated bending direction by default.
		- FullBodyBipedIK considers all elbows and knees as 3 DOF joints with swing rotation constrained to the range of a hemisphere (Since 0.22, used to be 1 DOF). That allows for full accuracy mapping of all biped rigs, the only known limitation is that the limbs can't be inverted (broken from the knee/elbow).

<b>Getting started:</b>

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/7__IafZGwvI" frameborder="0" allowfullscreen></iframe>\endhtmlonly
\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/tgRMsTphjJo" frameborder="0" allowfullscreen></iframe>\endhtmlonly
		- Add the FullBodyBipedIK component to the root of your character (the same GameObject that has the Animator/Animation component)
		- Make sure the auto-detected biped references are correct
		- Make sure the Root Node was correctly detected. It should be one of the bones in the lower spine.
		- Take a look at the character in the scene view, make sure you see the FullBodyBipedIK armature on top the character.
		- Press Play, weigh in the effectors

<b>Accessing the Effectors:</b>

\code
	public FullBodyBipedIK ik;
	
	void LateUpdate () {
		ik.solver.leftHandEffector.position = something; // Set the left hand effector position to a point in world space. This has no effect if the effector's positionWeight is 0.
		ik.solver.leftHandEffector.rotation = something; // Set the left hand effector rotation to a point in world space. This has no effect if the effector's rotationWeight is 0.
		ik.solver.leftHandEffector.positionWeight = 1f; // Weighing in the effector position, the left hand will be pinned to ik.solver.leftHandEffector.position.

		// Weighing in the effector rotation, the left hand and the arm will be pinned to ik.solver.leftHandEffector.rotation.
		// Note that if you only wanted to rotate the hand, but not change the arm bending, 
		// it is better to just rotate the hand bone after FBBIK has finished updating (use the OnPostUpdate delegate).
		ik.solver.leftHandEffector.rotationWeight = 1f;

		// Offsets the hand from it's animated position. If effector positionWeight is 1, this has no effect.
		// Note that the effectors will reset their positionOffset to Vector3.zero after each update, so you can (and should) use them additively. 
		//This enables you to easily edit the value by more than one script.
		ik.solver.leftHandEffector.positionOffset += something; 
		
		//The effector mode is for changing the way the limb behaves when not weighed in.
		//Free means the node is completely at the mercy of the solver. 
		//(If you have problems with smoothness, try changing the effector mode of the hands to MaintainAnimatedPosition or MaintainRelativePosition

		//MaintainAnimatedPosition resets the node to the bone's animated position in each internal solver iteration. 
		//This is most useful for the feet, because normally you need them where they are animated.

		//MaintainRelativePositionWeight maintains the limb's position relative to the chest for the arms and hips for the legs. 
		// So if you pull the character from the left hand, the right arm will rotate along with the chest.
		//Normally you would not want to use this behaviour for the legs.
		ik.solver.leftHandEffector.maintainRelativePositionWeight = 1f;

		// The body effector is a multi-effector, meaning it also manipulates with other nodes in the solver, namely the left thigh and the right thigh
		// so you could move the body effector around and the thigh bones with it. If we set effectChildNodes to false, the thigh nodes will not be changed by the body effector.
		ik.solver.body.effectChildNodes = false;

		// Other effectors: rightHandEffector, leftFootEffector, rightFootEffector, leftShoulderEffector, rightShoulderEffector, leftThighEffector, rightThighEffector, bodyEffector

		// You can also find an effector by:
		ik.solver.GetEffector(FullBodyBipedEffector effectorType);
		ik.solver.GetEffector(FullBodyBipedChain chainType);
		ik.solver.GetEndEffector(FullBodyBipedChain chainType); // Returns only hand or feet effectors
	}
\endcode

<b>Accessing the Chains:</b>

\code
	public FullBodyBipedIK ik;
	
	void LateUpdate () {
		ik.solver.leftArmChain.pull = 1f; // Changing the Pull value of the left arm
		ik.solver.leftArmChain.reach = 0f; // Changing the Reach value of the left arm

		// Other chains: rightArmChain, leftLegChain, rightLegChain, chain (the root chain)

		// You can also find a chain by:
		ik.solver.GetChain(FullBodyBipedChain chainType);
		ik.solver.GetChain(FullBodyBipedEffector effectorType);
	}
\endcode

<b>Accessing the Mapping:</b>

\code
	public FullBodyBipedIK ik;
	
	void LateUpdate () {
		ik.solver.spineMapping.iterations = 2; // Changing the Spine Mapping Iterations
		ik.solver.leftArmMapping.maintainRotationWeight = 1f; // Make the left hand maintain it's rotation as animated.
		ik.solver.headMapping.maintainRotationWeight = 1f; // Make the head maintain it's rotation as animated.
	}
\endcode

<b>Adding FullBodyBipedIK in runtime (UMA):</b>
\code
	using RootMotion; // Need to include the RootMotion namespace as well because of the BipedReferences

	FullBodyBipedIK ik;

	// Call this method whenever you need in runtime. 
	// Please note that FBBIK will sample the pose of the character at initiation so at the time of calling this method,
	// the limbs of the character should be bent in their natural directions.
	void AddFBBIK (GameObject go, BipedReferences references = null) {
	
		if (references == null) { // Auto-detect the biped definition if we don't have it yet
			BipedReferences.AutoDetectReferences(ref references, go.transform, BipedReferences.AutoDetectParams.Default);
		}

		ik = go.AddComponent<FullBodyBipedIK>(); // Adding the component

		// Set the FBBIK to the references. You can leave the second parameter (root node) to null if you trust FBBIK to automatically set it to one of the bones in the spine.
		ik.SetReferences(references, null);

		// Using pre-defined limb orientations to safeguard from possible pose sampling problems (since 0.22)
		ik.solver.SetLimbOrientations(BipedLimbOrientations.UMA); // The limb orientations definition for UMA skeletons
		// or...
		ik.solver.SetLimbOrientations(BipedLimbOrientations.MaxBiped); // The limb orientations definition for 3ds Max Biped skeletons
		// or..
		ik.solver.SetLimbOrientations(yourCustomBipedLimbOrientations); // Your custom limb orientations definition

		// To know how to fill in the custom limb orientations definition, you should imagine your character standing in I-pose (not T-pose) with legs together and hands on the sides...
		// The Upper Bone Forward Axis is the local axis of the thigh/upper arm bone that is facing towards character forward.
		// Lower Bone Forward Axis is the local axis of the calf/forearm bone that is facing towards character forward.
		// Last Bone Left Axis is the local axis of the foot/hand that is facing towards character left.
	}
\endcode

<b>Solving the head</b>
<br>
Final IK 0.5 introduced the FBBIKHeadEffector component that enables us to use the FullBodyBipedIK component to map a character to the target position and rotation of the head.
<br>Please take a look at the "Head Effector" demo scene to see how it can be set up.
<br>This is useful in particular for VR developers. You can download the Oculus VR demo scenes for Final IK for <a href="http://www.root-motion.com/addons/FinalIK_OVR.unitypackage">Unity4</a> and <a href="http://www.root-motion.com/addons/FinalIK_OVR_Unity5.unitypackage">Unity5</a>.

<b>Optimizing FullBodyBipedIK:</b>
- You can use renderer.isVisible to weigh out the solver when the character is not visible.
- Most of the time you don't need so many solver iterations and spine mapping iterations. Sine FinalIK 0.4, we are able to set solver iteration count to 0, in which case the full body effect will not be solved. This allows for easy optimization of IK on characters in the distance.
- Keep the "Reach" values at 0 if you don't need them. By default they are 0.05f to improve accuracy.
- Keep the Spine Twist Weight at 0 if you don't see the need for it.
- Also setting the "Spine Stiffness", "Pull Body Vertical" and/or "Pull Body Horizontal" to 0 will slightly help the performance.
- You don't need all the spine bones in the spine array. FBBIK works the fastest if there are 2 bones in the spine, the first one listed as the Root Node, and the other one the last bone in the spine (the last common ancestor of both arms). Having less bones in the Spine makes it more rigid, which in some cases might be even a better, more natural looking solution.

<b>Component variables:</b>
	- <b>fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance
	- <b>references</b> - references to the character bones that FullBodyBipedIK needs to build it's solver. The eyes are not necessary.

<b>Solver variables:</b>
	- <b>rootNode</b> - the central bone in the body. 2 triangles should be visible in the Scene view, the chest and the hips, connected by the rootNode.
	- <b>weight</b> - the solver weight for smoothly blending out the effect of the IK
	- <b>iterations</b> - the solver iteration count. If 0, full body effect will not be calculated. This allows for very easy optimization of IK on character in the distance.

<b>Body variables:</b>
	- <b>Target</b> - the target Transform of the body effector. If assigned, solver.bodyEffector.position will be automatically set to the position of the target.
	- <b>Position Weight</b> - the position weight of the body effector. If weighed in, the body will be pinned to solver.bodyEffector.position. This overrides bodyEffector.positionOffset.
	- <b>Use Thighs</b> - if true, any effect on the body effector will be also applied to the thigh effectors. This makes it easier to move the lower body around.
	- <b>Spine Stiffness</b> - the stiffness of spine constraints. Lower values "crack" the spine.
	- <b>Pull Body Vertical</b> - weight of hand effectors pulling the body vertically (relative to root rotation).
	- <b>Pull Body Horizontal</b> - weight of hand effectors pulling the body horizontally (relative to root rotation).
	- <b>Spine Iterations</b> - the number of iterations of the %FABRIK algorithm. Not used if there are 2 bones assigned to Spine in the References.
	- <b>Spine Twist Weight</b> - the weight of twisting the spine bones gradually to the orientation of the chest triangle. Relatively expensive, so set this to 0 if there is not much spine twisting going on.
	- <b>Maintain Head Rot</b> - if 1, the head will be rotated back to where it was (in world space) before solving FBBIK.

<b>Limb variables:</b>
	- <b>Target</b> - the target Transform of the effector. If assigned, effector.position will be automatically set to the position of the target.
	- <b>Position Weight</b> - the position weight of the effector. If weighed in, the effector bone will be pinned to effector.position. This overrides effector.positionOffset.
	- <b>Rotation Weight</b> - the rotation weight of the effector. If weighed in, the limb will be rotated to effector.rotation. This also changes the bending direction of the limb. If the bending direction assumed by the solver is disagreeable, set rotation weight to 0 and either just rotate the hand/foot after FBBIK is done or use a Bend Goal for full precision.
	- <b>Maintain Relative Pos</b> - if 1, the (unweighed) limb will rotate along with the chest/hip triangle.
	- <b>Pull</b> - the weight of pulling the parent chain. If this limb is the only one to have full pull and the others have none, you will be able to pull the character from that end effector without ever loosing contact.
	- <b>Reach</b> - pulls the first bone of the limb closer to the last bone.
	- <b>Push</b> - the weight of the end-effector pushing the shoulder/thigh when the end-effector is close to it.
	- <b>Push Parent</b> - the amount of push force transferred to the parent (from hand or foot to the body).
	- <b>Reach Smoothing</b> - smoothing the effect of the Reach with the expense of some accuracy.
	- <b>Push Smoothing</b> - smoothing the effect of the Push and Push Parent with the expense of some accuracy.
	- <b>Bend Goal</b> - if assigned, will bend the limb to the direction from the shoulder/thigh to the Bend Goal.
	- <b>Bend Goal Weight</b> - the weight of bending the limb towards the Bend Goal.
	- <b>Mapping Weight</b> - if 0, the limb will not be mapped, meaning the bones of the limb will not be rotated at all even if the effectors are weighed in.
	- <b>Maintain Hand/Foot Rot</b> - if 1, will rotate the hand/foot back to where it was (in world space) before solving FBBIK. This is usually useful for keeping the feet aligned to the surface when changing the position of the body or the height of the feet.

\image html FullBodyBipedIKComponent.png


*/

/*! \page page9 Grounder
Grounder is an automatic vertical foot placement and alignment correction system.

<b>How does it work?</b>

The solver works on a very basic principle: The characters are animated on planar ground. The height and rotation of the feet, as they are animated, should only be offset by the difference of ground height at their ingame positions from the root height.

Let's sample a frame in the animation, where the character's left foot y position is 0.1. Ingame, the Grounding solver will make some raycasting to find the real ground height at the left foot's position.
Let's say that the raycasting returns 0.2. If the character's ingame y position is 0, the ground height at the foot's position relative to the character root is 0.2.
The foot must be vertically offset by that exact value, ending up at 0.3.

This approach guarantees minimal interference with the animation, because the feet will only be offset when the ground height at their positions differ from the character root's height.
If that offset is negative, meaning ground height at the foot position is lower, then the solver needs to pull down the character from the pelvis so that the feet could reach their offset targets.

\image html GrounderHowItWorks.png

<b>Getting started:</b>

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/9MiZiaJorws" frameborder="0" allowfullscreen></iframe>\endhtmlonly
		- Create an empty GameObject, parent it to the root of the character, set it's localPosition and localRotation to zero,
		- Add GrounderFBBIK, GrounderBipedIK, GrounderIK or GrounderQuadruped depending on the type of the character to that GameObject.
		- Assign all the empty fields and the ground layers in the Grounder component
		- Make sure the character collider's layer is not in the walkable layers

<b>Solver variables:</b>
		- <b>layers</b> - the layers to walk on. Make sure to exclude the character's own layer.
		- <b>maxStep</b> - maximum offset for the feet. Note that the foot's animated height is subtracted from that range, meaning if the foot height relative to the root is already animated at 0.4, and the max step is 0.5, it can only be offset by 0.1.
		- <b>heightOffset</b> - offsets the character from the original animated height, this might be useful for minor corrections in that matter.
		- <b>footSpeed</b> - the interpolation speed of the foot. Increasing it will increase accuracy with the cost of smoothness (range: 0 - inf).
		- <b>footRadius</b> - the size of the feet. This has no effect with the fastest quality settings (range: 0.0001 - inf).
		- <b>prediction</b> - the prediction magnitude. The prediction is based on the velocity vector of the feet. Increasing this value makes the feet look for obstacles farther on their path, but with the cost of smoothness (range: 0 - inf).
		- <b>footRotationWeight</b> - the weight off offsetting the rotation of the feet (range: 0 - 1).
		- <b>footRotationSpeed</b> - the speed of interpolating the foot offset (range: 0 - inf).
		- <b>maxFootRotationAngle</b> - the maximum angle of foot offset (range: 0 - 90).
		- <b>rotateSolver</b> - if true, will use the character's local up vector as the vertical vector for the solver and the character will be able to walk on walls and ceilings. Leave this off when not needed for performance considerations.
		- <b>pelvisSpeed</b> - the interpolation speed of the pelvis. Increasing this will make the character's body move up/down faster. If you don't want the pelvis to move (usually spider types) set this to zero (range: 0 - inf).
		- <b>pelvisDamper</b> - dampering the vertical motion of the character's root. This is only useful when the root is moving too violently because of the physics and you wish to make the vertical movement smoother (range: 0 - 1).
		- <b>lowerPelvisWeight</b> - the weight of moving the pelvis down to the lowest foot. This moves the pelvis down when the character walks down the stairs and needs to reach a lower step.
		- <b>liftPelvisWeight</b> - the weight of moving the pelvis up to the highest foot. This moves the pelvis up when the character steps on a higher obstacle. Use this when the character is crouching.
		- <b>rootSphereCastRadius</b> - the radius of the root sphere cast. This has no effect with the "Fastest" and "Simple" quality settings.
		- <b>quality</b> - the quality mainly determines the weight of ray/sphere/acpsule casting. The Fastest will only make a single raycast for each foot, plus a raycast for the root. Simple will make 3 raycasts for each foot, plus a raycast for the root.
The Best quality settings means 1 raycast and a capsule cast for each foot and a sphere cast for the root.

<BR>
\section grounders Grounder Components

\subsection fbbik GrounderFBBIK

GrounderFBBIK uses the FullBodyBipedIK component for offsetting the feet and optionally bending the spine. This is most useful if your character already has some FBBIK functionality and you need the component anyway.
GrounderFBBIK uses the positionOffset of the effectors so you can still pin the feet without a problem if necessary.

<b>Component variables:</b>
		- <b>weight</b> - the master weight. The effect of the Grounder can be smoothly faded out for better performance when not needed.
		- <b>spineBend</b> - the amount of bending the spine. The spine will be bent only when the character is facing a slope or stairs. Negative value will invert the effect.
		- <b>spineSpeed</b> - the speed of bending the spine (range: 0 - inf).
		- <b>spine</b> - the FBBIK effectors involved in bending the spine and their horizontal/vertical weights.

\image html GrounderFBBIKComponent.png

\subsection bipedIK GrounderBipedIK

GrounderBipedIK uses the BipedIK component for offsetting the feet and optionally bending the spine. With BipedIK you will not have to manually set up the IK components for the limbs.

<b>Component variables:</b>
		- <b>weight</b> - the master weight. The effect of the Grounder can be smoothly faded out for better performance when not needed.
		- <b>spineBend</b> - the amount of bending the spine. The spine will be bent only when the character is facing a slope or stairs. Negative value will invert the effect.
		- <b>spineSpeed</b> - the speed of bending the spine (range: 0f - inf).

\image html GrounderBipedIKComponent.png

\subsection ik GrounderIK

GrounderIK can use any number and combination of CCD, FABRIK, LimbIK, or TrigonometricIK components for offsetting the limbs. This is most useful for spider/bot types of characters that have more than 2 legs connected to a single hub.

<b>Component variables:</b>
		- <b>weight</b> - the master weight. The effect of the Grounder can be smoothly faded out for better performance when not needed.
		- <b>legs</b> - the IK components. Can be any number or combination of CCD, FABRIK, LimbIK or TrigonometricIK connected to the same pelvis.
		- <b>pelvis</b> - the pelvis Transform. This should be the upmost common parent of the legs and the spine.
		- <b>characterRoot</b> - this is only necessary if you wish to use the root rotation functionality. Root rotation will rotate the character root to the ground normal.
		- <b>rootRotationWeight</b> - the weight of root rotation (range: 0 - 1).
		- <b>rootRotationSpeed</b> - the speed of root rotation interpolation (range: 0 - inf).
		- <b>maxRootRotationAngle</b> - the maximum angle of rotating the characterRoot.up from the default Vector3.up (range: 0 - 90).

\image html GrounderIKComponent.png

\subsection quadruped GrounderQuadruped

GrounderQuadruped uses 2 Grounding solvers. That means 2 hubs that can have any number of legs.

<b>Component variables:</b>
		- <b>weight</b> - the master weight. The effect of the Grounder can be smoothly faded out for better performance when not needed.
		- <b>forelegSolver</b> - the Grounding solver for the forelegs.
		- <b>characterRoot</b> - this is only necessary if you wish to use the root rotation functionality. Root rotation will rotate the character root up or down depending on the ground.
		- <b>rootRotationWeight</b> - the weight of root rotation. If the quadruped is standing on an edge and leaning down with the forelegs entering the ground, you need to increase this value and also the root rotation limits (range: 0 - 1).
		- <b>rootRotationSpeed</b> - the speed of root rotation interpolation (range: 0 - inf).
		- <b>minRootRotation</b> - the maximum angle of rotating the quadruped downwards (going downhill, range: -90 - 0).
		- <b>maxRootRotation</b> - the maximum angle of rotating the quadruped upwards (going uphill, range: 0 - 90).
		- <b>pelvis</b> - the pelvis Transform. This should be the upmost common parent of the legs and the spine.
		- <b>lastSpineBone</b> - the last bone in the spine that is the common parent of the forelegs.
		- <b>maxLegOffset</b> - the maximum offset of the legs from their animated positions. This enables you to set maxStep higher without the feet getting inverted (range: 0 - inf).
		- <b>maxForelegOffset</b> - the maximum offset of the forelegs from their animated positions. This enables you to set maxStep higher without the forefeet getting inverted (range: 0 - inf).
		- <b>head</b> - the head Transform, if you wish to maintain it's rotation as animated.
		- <b>maintainHeadRotationWeight</b> - the weight of maintaining the head's rotation as animated (range: 0 - 1).

\image html GrounderQuadrupedComponent.png

*/

/*! \page page10 Interaction System

\section overview Overview

The Interaction System is designed for the easy setup of full body IK interactions with the dynamic game environment. 
It requires a character with FullBodyBipedIK and consists of 3 main components: <b>InteractionSystem, InteractionObject</b> and <b>InteractionTarget.</b>

<b>Getting started:</b>

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/r5jiZnsDH3M" frameborder="0" allowfullscreen></iframe>\endhtmlonly
<BR>\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/eP9-zycoHLk" frameborder="0" allowfullscreen></iframe>\endhtmlonly
<BR>\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/sQfB2RcT1T4" frameborder="0" allowfullscreen></iframe>\endhtmlonly
<BR>\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/-TDZpNjt2mk" frameborder="0" allowfullscreen></iframe>\endhtmlonly
	- Add the InteractionSystem component to a FBBIK character
	- Add the InteractionObject component to an object you wish to interact with
	- Create a PositionWeight weight curve, that consists of 3 keyframes {(0, 0), (1, 1), (2, 0)}, where x is horizontal and y is vertical value.
	- Add the InteractionSystemTestGUI component to the character and fill out its fields for quick debugging of the interaction
	- Play the scene and press the GUI button to start the interaction

<b>Getting started with coding:</b>
<BR>

\code
using RootMotion.FinalIK;

public InteractionSystem interactionSystem; // Reference to the InteractionSystem component on the character
public InteractionObject button; // The object to interact with
public bool interrupt; // If true, interactions can be called before the current interaction has finished

void OnGUI() {
	// Starting an interaction
	if (GUILayout.Button("Press Button")) {
		interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, button, interrupt);
	}
}
\endcode

See the <a href="http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_system.html">API reference of the Interaction System </a> for all the interaction functions. 
See the Interaction demo scene and the InteractionDemo.cs for more examples.

\section components Components of the Interaction System

\subsection interactionSystem InteractionSystem
This component should be added to the same game object that has the FBBIK component. It is the main driver and the main interface for controlling the interactions of it's character.

<b>Component variables:</b>
	- <b>targetTag</b> - if not empty, only the targets with the specified tag will be used by this interaction system. This is useful if there are characters in the game with different bone orientations.
	- <b>fadeInTime</b> - the time of fading in all the channels used by the interaction (weight of the effector, reach, pull..)
	- <b>speed</b> - the master speed for all interactions of this character.
	- <b>resetToDefaultsSpeed</b> - if > 0, lerps all the FBBIK channels used by the Interaction System back to their default or initial values when not in interaction.
	- <b>collider</b> the collider that registers OnTriggerEnter and OnTriggerExit events with InteractionTriggers.
	- <b>camera</b> will be used by Interaction Triggers that need the camera's position. Assign the first person view character camera.
	- <b>camRaycastLayers</b> the layers that will be raycasted from the camera (along camera.forward). All InteractionTrigger look at target colliders should be included.
	- <b>camRaycastDistance</b> max distance of raycasting from the camera.
	- <b>fullBody</b> - reference to the FullBodyBipedIK component.
	- <b>lookAt</b> - Look At uses a LookAtIK component to automatically turn the body/head/eyes to the Interaction Object.
	<BR><b>iK</b> - reference to the LookAtIK component,
	<BR><b>lerpSpeed</b> - the speed of interpolating the LookAtIK target to the position of the Interaction Objectm
	<BR><b>weightSpeed</b> - the speed of weighing in/out the LookAtIK weight.

\image html InteractionSystemComponent.png

<BR>
\subsection interactionObject InteractionObject

This component should be added to the game objects that we wish to interact with. It contains most of the information about the nature of the interactions.
It does not specify which body part(s) will be used, but rather the look and feel and the animation of the interaction. 
That way the characteristics of an interaction are defined by the object and can be shared between multiple effectors. 
So for instance a button will be pressed in the same manner, regardless of which effector is used for it.

<b>Animating Interaction Objects:</b>
<BR>
The Interaction System introduces the concept of animating objects rather than animating characters. So instead of animating a character opening a door or pressing a button,
we can just animate the door opening or the button being pressed and have the Interaction System move the character to follow that animation. 
This approach gives us great freedom over the dynamics of the interactions, even allowing for multiple simultaneous animated interactions.
You can animate an Interaction Object with the dope sheet and then call that animation by using the OnStartAnimation, OnTriggerAnimation, OnReleaseAnimation and OnEndAnimation animator events that you can find on the InteractionObject component.

<b>Component variables:</b>
	- <b>otherLookAtTarget</b> - the look at target. If null, will look at this GameObject. This only has an effect when the InteractionLookAt component is used.
	- <b>otherTargetsRoot</b> - the root Transform of the InteractionTargets. That will be used to automatically find all the InteractionTarget components, so all the InteractionTargets should be parented to that.
	- <b>positionOffsetSpace</b> - if assigned, all PositionOffset channels will be applied in the rotation space of this Transform. If not, they will be in the rotation space of the character.
	- <b>weightCurves</b> - The weight curves define the process of the interaction. The interaction will be as long as the longest weight curve in that list. 
	The horizontal value of the curve represents time since the interaction start. The vertical value represents the weight of it's channel. 
	So if we had a weight curve for PositionWeight with 3 keyframes {(0, 0), (1, 1), (2, 0)} where a keyframe represents (time, value), then the positionWeight of an effector will reach 1 at 1 second from the interaction start and fall back to 0 at 2 secods from the interaction start.
	The curve types stand for the following:
	<BR><b>PositionWeight</b> - IKEffector.positionWeight,
	<BR><b>RotationWeight</b> - IKEffector.rotationWeight,
	<BR><b>PositionOffsetX</b> - X offset from the interpolation direction relative to the character rotation,
	<BR><b>PositionOffsetY</b> - Y offset from the interpolation direction relative to the character rotation,
	<BR><b>PositionOffsetZ</b> - Z offset from the interpolation direction relative to the character rotation,
	<BR><b>Pull</b> - pull value of the limb used in the interaction (FBIKChain.pull),
	<BR><b>Reach</b> - reach value of the limb used in the interaction (FBIKChain.reach),
	<BR><b>RotateBoneWeight</b> - rotating the bone after FBBIK is finished. In many cases using this instead of RotationWeight will give you a more stable and smooth looking result.
	<BR><b>Push</b> - push value of the limb used in the interaction (FBIKChain.push),
	<BR><b>PushParent</b> - pushParent value of the limb used in the interaction (FBIKChain.pushParent),
	<BR><b>PoserWeight</b> - weight of the hand/generic Poser.

	- <b>multipliers</b> - the weight curve multipliers are designed for reducing the amount of work with AnimationCurves. If you needed the rotationWeight curve to be the same as the positionWeight curve, you could use a multipier instead of duplicating the curve.
	In that case the multiplier would look like this: Curve = PositionWeight, Multiplier = 1 and Result = RotationWeight.
	If null, will use the InteractionObject game object.

	- <b>events</b> - events can be used to trigger animations, messages, interaction pause or pick-ups at certain time since interaction start.
	<BR><b>time</b> - time of triggering the event (since interaction start),
	<BR><b>pause</b> - if true, will pause the interaction on this event. The interaction can be resumed by calling InteractionSystem.ResumeInteraction(FullBodyBipedEffector effectorType) or InteractionSystem.ResumeAll(),
	<BR><b>pickUp</b> - if true, the Interaction Object will be parented to the bone of the interacting effector. This only works as expected if a single effector is interacting with this object. For 2-handed pick-ups please see the "Interaction PickUp2Handed" demo,
	<BR><b>animations</b> - the list of animations called on this event. The "Animator" or "Animation" refers to the the animator component that the "Animation State" cross-fade (using "Cross Fade Time") will be called upon. "Layer" is the layer of the Animation State and if "Reset Normalized Time" is checked, the animation will always begin from the start.
	<BR><b>messages</b> - the list of messages sent on this event (using <a href="http://docs.unity3d.com/ScriptReference/GameObject.SendMessage.html">GameObject.SendMessage()</a>, all messages require a receiver). "Function" is the name of the function called on the "Recipient" game object.

\image html InteractionObjectComponent.png

<BR>
\subsection interactionTarget InteractionTarget

If the Interaction Object has no Interaction Targets, the position and rotation of the Interaction Object itself will be used for all the effectors as the target.
However if you needed to pose a hand very precisely, you will need to create an Interaction Target. Normally you would first pose a duplicate of the character's hand, parent it to the Interaction Object and add the InteractionTarget component.
The Interaction Objects will automatically find all the Interaction Targets in their hierarchies and use them for the corresponding effectors.

\image html InteractionTarget.png

<b>Working with Interaction Targets:</b>
	- See this <a href="https://www.youtube.com/watch?v=sVWdCNEnxAE">tutorial video </a> 
	- Duplicate your character, position and pose a hand to the Interaction Object
	- Parent the hand hierarchy to the Interaction Object, delete the rest of the character
	- Add the InteractionTarget component to the hand bone, fill out it's fields
	- Add the HandPoser (or GenericPoser) component to the hand bone of the character (not the hand that you just posed). That will make the fingers match the posed target.
	- Play the scene to try out the interaction

<b>Component variables:</b>
	- <b>effectorType</b> - the type of the FBBIK effector that this target corresponds to
	- <b>multipliers</b> - the weight curve multipliers enable you to override weight curve values for different effectors.
	- <b>interactionSpeedMlp</b> - this enables you to change the speed of the interaction for different effectors.
	- <b>pivot</b> - the pivot to twist/swing this interaction target about (The blue dot with an axis and a circle around it on the image above).
	Very often you would want to acces an object from any angle, the pivot enables the Interaction System to rotate the target to face the direction towards the character.
	- <b>twistAxis</b> - the axis of twisting the interaction target (The blue axis on the image above. The circle visualizes the twist rotation).
	- <b>twistWeight</b> - the weight of twisting the interaction target towards the effector bone in the start of the interaction.
	- <b>swingWeight</b> - the weight of swinging the interaction target towards the effector bone in the start of the interaction. This will make the direction from the pivot to the InteractionTarget match the direction from the pivot to the effector bone.
	- <b>rotateOnce</b> - if true, will twist/swing around the pivot only once at the start of the interaction

\image html InteractionTargetComponent.png

<BR>
\subsection interactionTrigger InteractionTrigger

With most Interaction Objects, there is a certain angular and positional range in which they are naturally accessible and reachable to the character. For example, a button can only be pressed with a left hand if the character is within a reasonable range and more or less facing towards it. 
The Interaction Trigger was specifically designed for the purpose of defining those ranges for each effector and object.

\image html InteractionTriggerDoor.png

The image above shows an InteractionTrigger defining the ranges of interaction with a door handle for the left hand and for the right hand."
<BR>The green sphere is the trigger Collider on the game object that will register this InteractionTrigger with the InteractionSystem of the character.
<BR>The circle defines the range of position in which the character is able to interact with the door.
<BR>The purple range defines the angular range of character forward in which it is able to open the door with the right hand, the pink range is the same for the left hand.

<b>Getting started</b>
- Add the InteractionSystem component to the character,
- make sure the game object that has the InteractionSystem component also has a Collider and a Rigidbody,
- create an InteractionObject to interact with, add a weight curve, for example {(0, 0), (1, 1), (2, 0)}, for PositionWeight,
- create an empty game object, name it "Trigger", parent it to the Interaction Object, add the InteractionTrigger component,
- add a trigger collider to the InteractionTrigger component, make sure it is able to trigger OnTriggerEnter calls on the InteractionSystem game object,
- assign the InteractionObject game object to InteractionTrigger's "Target". That will be the reference of direction from the trigger to the object,
- add a "Range", add an "Interaction" to that range, assign the InteractionObject to the "Interaction Objet",
- specify the FBBIK effector that you want to use for the interaction,
- set "Max Distance" to something like 1 (you can see it visualized with a circle in the Scene view),
- set "Max Angle" to 180, so you can trigger the interaction from any angle,
- follow the instructions below to create a script that controls the triggering (see UserControlInteractions.cs for a full example):

The InteractionSystem will automatically maintain a list of triggers that the character's collider is in contact with. That list can be accessed by InteractionSystem.triggersInRange;
That list contains only the triggers that have a suitable effector range for the current position and rotation of the character.

You can find the closest trigger for the character by:
\code
int closestTriggerIndex = interactionSystem.GetClosestTriggerIndex();
\endcode

if GetClosestTriggerIndex returns -1, there are no valid triggers currently in range.
If not, you can trigger the interaction by

\code
interactionSystem.TriggerInteraction(closestTriggerIndex, false);
\endcode

See the Interaction Trigger demo scene and the UserControlInteractions.cs script for a full example on how to make the Interaction Triggers work.

<b>Component variables</b>
	- <b>ranges</b> - the valid ranges of the character's and/or it's camera's position for triggering interaction when the character is in contact with the collider of this trigger.
	- <b>characterPosition</b> - The range for the character's position and rotation.
		<BR><b>use</b> - if false, will not care where the character stands, as long as it is in contact with the trigger collider.
		<BR><b>offset</b> - the offset of the character's position relative to the trigger in XZ plane. Y position of the character is unlimited as long as it is contact with the collider.
		<BR><b>angleOffset</b> - angle offset from the default forward direction.
		<BR><b>maxAngle</b> - max angular offset of the character's forward from the direction of this trigger.
		<BR><b>radius</b> - max offset of the character's position from this range's center.
		<BR><b>orbit</b> - if true, will rotate the trigger around it's Y axis relative to the position of the character, so the object can be interacted with from all sides.
		<BR><b>fixYAxis</b> - fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object. For example a gun will be able to be picked up from the same direction relative to the barrel no matter which side the gun is resting on.
	- <b>cameraPosition</b> - The range for the character camera's position and rotation.
		<BR><b>lookAtTarget</b> - what the camera should be looking at to trigger the interaction?
		<BR><b>direction</b> - the direction from the lookAtTarget towards the camera (in lookAtTarget's space).
		<BR><b>maxDistance</b> - max distance from the lookAtTarget to the camera.
		<BR><b>maxAngle</b> - max angle between the direction and the direction towards the camera.
		<BR><b>fixYAxis</b> - fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object.
	- <b>interactions</b> - the definitions of interactions that will be called on InteractionSystem.TriggerInteraction.

\image html InteractionTriggerComponent.png
\image html InteractionTriggerRightHand.png "Valid position of the character without 'Orbit'"
	\image html InteractionTriggerRightHandOrbit.png "Valid positions of the character with 'Orbit'"
*/

/*! \page page11 Leg IK
LegIK is a wrapper component for VRIK's 4-joint biped leg solver.

<b>Component variables:</b>
    - <b>LegIK.fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance.

<b>Wrapper variables:</b>
    - <b>LegIK.solver.pelvis</b> - the pelvis bone.
    - <b>LegIK.solver.thigh</b> - the upper leg bone.
    - <b>LegIK.solver.calf</b> - the lower leg bone.
    - <b>LegIK.solver.foot</b> - the foot bone
    - <b>LegIK.solver.toe</b> - the toe bone.
    - <b>LegIK.solver.IKPositionWeight</b> - positional weight of the toe/foot target. Note that if you have nulled the target, the foot will still be pulled to the last position of the target until you set this value to 0.
    - <b>LegIK.solver.IKRotationWeight</b> - rotational weight of the toe/foot target. Note that if you have nulled the target, the foot will still be rotated to the last rotation of the target until you set this value to 0.
    
<b>Solver variables:</b>
    - <b>LegIK.solver.leg.target</b> - the foot/toe target. This should not be the foot tracker itself, but a child GameObject parented to it so you could adjust it's position/rotation to match the orientation of the foot/toe bone. If a toe bone is assigned in the References, the solver will match the toe bone to this target. If no toe bone assigned, foot bone will be used instead.
    - <b>LegIK.solver.leg.bendGoal</b> - the knee will be bent towards this Transform if 'Bend Goal Weight' > 0.
    - <b>LegIK.solver.leg.bendGoalWeight</b> - if greater than 0, will bend the knee towards the 'Bend Goal' Transform.
    - <b>LegIK.solver.leg.swivelOffset</b> - angular offset of knee bending direction.
    - <b>LegIK.solver.leg.bendToTargetWeight</b> - if 0, the bend plane will be locked to the rotation of the pelvis and rotating the foot will have no effect on the knee direction. If 1, to the target rotation of the leg so that the knee will bend towards the forward axis of the foot. Values in between will be slerped between the two.
    - <b>LegIK.solver.leg.legLengthMlp</b> - use this to make the leg shorter/longer. Works by displacement of foot and calf localPosition.
    - <b>LegIK.solver.leg.stretchCurve</b> - evaluates stretching of the leg by target distance relative to leg length. Value at time 1 represents stretching amount at the point where distance to the target is equal to leg length. Value at time 1 represents stretching amount at the point where distance to the target is double the leg length. Value represents the amount of stretching. Linear stretching would be achieved with a linear curve going up by 45 degrees. Increase the range of stretching by moving the last key up and right at the same amount. Smoothing in the curve can help reduce knee snapping (start stretching the arm slightly before target distance reaches leg length). To get a good optimal value for this curve, please go to the 'VRIK (Basic)' demo scene and copy the stretch curve over from the Pilot character.

\image html LegIKComponent.png
*/

/*! \page page12 Limb IK
LimbIK extends TrigonometricIK to specialize on the 3-segmented hand and leg character limb types.

LimbIK comes with multiple <b>Bend Modifiers:</b>
	  - Animation: tries to maintain bend direction as it is in the animation
	  - Target: rotates the bend direction with the target IKRotation
	  - Parent: rotates the bend direction along with the parent Transform (pelvis or clavicle)
	  - Arm: keeps the arm bent in a biometrically natural and relaxed way (also most expensive of the above).
	  - Goal: bends the arm towards the "Bend Goal" Transform.

NOTE: Bend Modifiers are only applied if Bend Modfier Weight is greater than 0.
		
The IKSolverLimb.maintainRotationWeight property allows to maintain the world space rotation of the last bone fixed as it was before solving the limb. 
<BR>This is most useful when we need to reposition a foot, but maintain it's rotation as it was animated to ensure proper alignment with the ground surface.

<b>Getting started:</b>
	- Add the LimbIK component to the root of your character (the character should be facing it's forward direction)
	- Assign the limb bones to bone1, bone2 and bone3 in the LimbIK component (bones can be skipped, which means you can also use LimbIK on a 4-segment limb).
	- Press Play

<b>Getting started with scripting:</b>

\code
	public LimbIK limbIK;
	
	void LateUpdate () {
		// Changing the target position, rotation and weights
		limbIK.solver.IKPosition = something;
		limbIK.solver.IKRotation = something;
		limbIK.solver.IKPositionWeight = something;
		limbIK.solver.IKRotationWeight = something;

		// Changing the automatic bend modifier
		limbIK.solver.bendModifier = IKSolverLimb.BendModifier.Animation; // Will maintain the bending direction as it is animated.
		limbIK.solver.bendModifier = IKSolverLimb.BendModifier.Target; // Will bend the limb with the target rotation
		limbIK.solver.bendModifier = IKSolverLimb.BendModifier.Parent; // Will bend the limb with the parent bone (pelvis or shoulder)

		// Will try to maintain the bend direction in the most biometrically relaxed way for the arms. 
		// Will not work for the legs.
		limbIK.solver.bendModifier = IKSolverLimb.BendModifier.Arm; 
	}
\endcode

<b>Adding LimbIK in runtime:</b>
- Add the LimbIK component via script
- Call LimbIK.solver.SetChain()

\image html LimbIK.png

<b>Component variables:</b>
	- <b>fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance

<b>Solver variables:</b>
	- <b>bone1</b> - the first bone (upper arm or thigh)
	- <b>bone2</b> - the second bone (forearm or calf)
	- <b>bone3</b> - the third bone (hand or foot)
	- <b>target</b> - the target Transform. If assigned, solver IKPosition and IKRotation will be automatically set to the position of the target
	- <b>positionWeight</b> - the weight of solving to the target position (IKPosition)
	- <b>rotationWeight</b> - the weight of solving to the target rotation (IKRotation)
	- <b>bendNormal</b> - normal of the plane defined by the positions of the bones. When the limb bends, the second bone will always be positioned somewhere on that plane
	- <b>AvatarIKGoal</b> - the <a href="http://docs.unity3d.com/ScriptReference/AvatarIKGoal.html">AvatarIKGoal</a> of this solver. This is only used by the "Arm" bend modifier
	- <b>maintainRotationWeight</b> - weight of maintaining the rotation of the third bone as it was before solving
	- <b>bendModifier</b> - a selection of automatic modifiers of the bend normal
	- <b>bendModifierWeight</b> - the weight of the bend modifier

\image html LimbIKComponent.png
*/

/*! \page page13 Look At IK
LookAt IK can be used on any character or other hierarchy of bones to rotate a set of bones to face a target. 
<BR>Note that if LookAtIK does not fit you requirements, you can also use AimIK, that is very similar, but provides a different set of parameters, to make characters (especially non-biped) look at targets.

<b>Getting started:</b>
		- Add the LookAtIK component to the root GameObject. That GameObject's forward axis will be the forward direction.
		- Assing Spine, head and eye bones to the component.
		- Press Play

<b>Getting started with scripting:</b>

\code
	public LookAtIK lookAt;
	
	void LateUpdate () {
		lookAt.solver.IKPositionWeight = 1f; // The master weight
		
		lookAt.solver.IKPosition = something; // Changing the look at target

		// Changing the weights of individual body parts
		lookAt.solver.bodyWeight = 1f;
		lookAt.solver.headWeight = 1f;
		lookAt.solver.eyesWeight = 1f;

		// Changing the clamp weight of individual body parts
		lookAt.solver.clampWeight = 1f;
		lookAt.solver.clampWeightHead = 1f;
		lookAt.solver.clampWeightEyes = 1f;
	}
\endcode

<b>Adding LookAtIK in runtime:</b>
- Add the LookAtIK component via script
- Call LookAtIK.solver.SetChain()

\image html LookAtIK.png

<b>Component variables:</b>
	- <b>fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance

<b>Solver variables:</b>
	- <b>target</b> - the target Transform. If assigned, solver IKPosition will be automatically set to the position of the target
	- <b>weight</b> - the master weight of the solver (multiplies all the other weights)
	- <b>bodyWeight</b> - the weight of rotating the spine bones
	- <b>headWeight</b> - the weight of rotating the head bone
	- <b>eyesWeight</b> - the weight of rotating the eye bones
	- <b>clampWeight</b> - clamping rotation of the spine bones. 0 is free rotation, 1 is completely clamped to zero effect
	- <b>clampWeightHead</b> - clamping rotation of the head bone. 0 is free rotation, 1 is completely clamped to zero effect
	- <b>clampWeightEyes</b> - clamping rotation of the eye bones. 0 is free rotation, 1 is completely clamped to zero effect
	- <b>clampSmoothing</b> - the number of sine smoothing iterations applied to clamping to make it smoother
	- <b>spineWeightCurve</b> - normalized weight distribution between the spine bones. The first spine bone is at "time" 0, the last spine bone is at 1
	- <b>head</b> - the head bone
	- <b>spine</b> - the spine bones in descending order (parents first), bones can be skipped
	- <b>eyes</b> - the eye bones

\image html LookAtIKComponent.png

*/

/*! \page page14 Rotation Limits
  
  All rotation limits and other Final %IK components are Quaternion and Axis-Angle based to ensure consistency, continuity and to minimize singularity issues. Final %IK does not contain a single Euler operation. 
  <BR>All rotation limits are based on local rotations and use the initial local rotation as reference just like Physics joints. This makes them axis-independent and intuitive to set up.
  <BR>All rotation limits have undoable Scene view editors. 
  <BR>All rotation limits work with %IK solvers that support rotation limits.

  \image html RotationLimits.png "Rotation Limits"
  
  \subsection angle Angle
  	Simple angular swing and twist limit.
  
  	\image html RotationLimitAngle.png "The anglular rotation limit"
  	\image html RotationLimitAngleComponent.png "The RotationLimitAngle component"
  
  \subsection hinge Hinge
  
  The hinge rotation limit limits the rotation to a single degree of freedom around an axis. This rotation limit is additive which means the hinge limits can exceed 360 degrees either way.
  
  \image html RotationLimitHinge.png "Adjusting hinge limits in the scene view"
  \image html RotationLimitHingeComponent.png "The RotationLimitHinge component"
  
  \subsection polygonal Polygonal
  	Using a spherical polygon to limit the range of rotation on universal and ball-and-socket joints. A reach cone is specified as a spherical polygon 
  	on the surface of a a reach sphere that defines all positions the longitudinal segment axis beyond the joint can take. 
  	<BR>The twist limit parameter specifies the maximum twist around the main axis.
  	
  	This class is based on the paper:
  	<BR><a href="http://users.soe.ucsc.edu/~avg/Papers/jtl.pdf">"Fast and Easy Reach-Cone Joint Limits" </a> 
  	<BR>Jane Wilhelms and Allen Van Gelder. Computer Science Dept., University of California, Santa Cruz, CA 95064. August 2, 2001
  	
  	The polygonal rotation limit is provided with handy scene view tools for quick editing, cloning and modifying of the reach cone points.
  	
  	\image html RotationLimitPolygonal.png "Defining reach cone points on the polygonal rotation limit"
  	\image html RotationLimitPolygonalComponent.png "RotationLimitPolygonal component"
  
   \subsection spline Spline
   
   Using a spline to limit the range of rotation on universal and ball-and-socket joints. 
   <BR>Reachable area is defined by an AnimationCurve orthogonally mapped onto a sphere, which provides a very smooth and fast result.
   <BR>The twist limit parameter specifies the maximum twist around the main axis.
   	
   The spline rotation limit is provided with handy scene view tools for quick editing, cloning and modifying of the spline handles.	
   
   \image html RotationLimitSpline.png "Adjusting spline handles on on the spline rotation limit"
   \image html RotationLimitSplineComponent.png "The RotationLimitSpline component"

*/

/*! \page page15 Trigonometric IK
Trigonometric IK is the most basic IK solver that is based on the Law of Cosines and solves a 3-segmented bone hierarchy.
(IKSolverLimb extends IKSolverTrigonometric just to add a couple of extra parameters like the bend modifiers)

<b>Getting started:</b>
		- Add the TrigonometricIK component to the first bone.
		- Assign bone1, bone2 and bone3 in the TrigonometricIK component
		- Press Play

<b>Getting started with scripting:</b>

\code
	public TrigonometricIK trig;
	
	void LateUpdate () {
		// Changing the target position, rotation and weights
		trig.solver.IKPosition = something;
		trig.solver.IKRotation = something;
		trig.solver.IKPositionWeight = something;
		trig.solver.IKRotationWeight = something;

		trig.solver.SetBendGoalPosition(Vector goalPosition); // Sets the bend goal to a point in world space 
	}
\endcode

<b>Adding TrigonometricIK in runtime:</b>
- Add the TrigonometricIK component via script
- Call TrigonometricIK.solver.SetChain()

\image html TrigonometricIK.png

<b>Component variables:</b>
	- <b>fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance

<b>Solver variables:</b>
	- <b>bone1</b> - the first bone (upper arm or thigh)
	- <b>bone2</b> - the second bone (forearm or calf)
	- <b>bone3</b> - the third bone (hand or foot)
	- <b>target</b> - the target Transform. If assigned, solver IKPosition and IKRotation will be automatically set to the position of the target
	- <b>positionWeight</b> - the weight of solving to the target position (IKPosition)
	- <b>rotationWeight</b> - the weight of solving to the target rotation (IKRotation)
	- <b>bendNormal</b> - normal of the plane defined by the positions of the bones. When the limb bends, the second bone will always be positioned somewhere on that plane

\image html TrigonometricIKComponent.png
*/

/*! \page page16 VRIK
VRIK is a high speed full body solver dedicated to animating VR avatars. The solver was originally developed for the game "Dead and Buried" by Oculus Studios.
<BR>A detailed description of the inner workings of VRIK can be found in <a href="http://root-motion.com/2016/06/inverse-kinematics-in-dead-and-buried/">this article originally published in Oculus Developer Blog.</a> 

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/4DjwC-0aoKE" frameborder="0" allowfullscreen></iframe>\endhtmlonly


<b>Getting started:</b>

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/6Pfx7lYQiIA" frameborder="0" allowfullscreen></iframe>\endhtmlonly
	- Add VRIK to your character root. If it is a Humanoid, References will be automatically filled in using the Animator. If not, the bones used by the solvers will have to be manually assigned to the "References".
	- Move the camera to the avatar's eyes, move hand controllers to the avatar's hands as if they were worn by the avatar.
	- Make duplicates of the avatar's head and hand bones.
	- Parent the duplicates to the camera/hand controllers.
	- Assign the duplicates as Head/Hand Targets in VRIK.
    - VRIK samples the pose of the avatar at Start to find out which way to bend the limbs. Make sure that the elbow/knee bones are rotated so that the limbs are slightly bent in their natural bending directions, they must not be completely straight.
    - Examples of VRIK setup for Oculus and SteamVR can be found in "Plugins/RootMotion/FinalIK/_Integration". Oculus/SteamVR utilities packages from the Asset Store are required to run these demos.

<b>Performance:</b>

VRIK is about 2-3 times faster than FullBodyBipedIK. In Dead and Buried 2, it was used on 6 avatars simultaneously visible on screen and running on Oculus Quest hardware. 
Since Final IK v1.9 VRIK has a 3-level LOD system with level 1 providing roughly 30% gain and level 2 solver disabled with only the root position and rotation updated if the built-in locomotion solver was used.

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/5b_87OuPJxE" frameborder="0" allowfullscreen></iframe>\endhtmlonly


<b>Adding VRIK component in run-time:</b>

\code
	void LateUpdate () {
		var ik = gameObject.AddComponent<VRIK>();
        ik.AutoDetectReferences();
	}
\endcode

<b>Using VRIK with Rotation Limits:</b>
<BR> VRIK has it's own set of built-in constraints and RotationLimits can not be used in the solving process. It is possible however to apply RotationLimits on top of VRIK for instance to make sure that the hand bones do not bend unnaturally beyond reasonable limits.
To do this, we would have to disable the rotation limits in Start to take control of their updating, then update them after VRIK using the VRIK.solver.OnPostUpdate delegate::

\code
public VRIK ik;
public RotationLimit[] rotationLimits;

void Start() {
    foreach (RotationLimit limit in rotationLimits) {
        limit.enabled = false;
    }

    ik.solver.OnPostUpdate += AfterVRIK;
}

private void AfterVRIK() {
    foreach (RotationLimit limit in rotationLimits) {
        limit.Apply();
    }
}
\endcode

<b>Calibration:</b>
<BR> Please take a look at the "VRIK (Calibration)" demo on how to use the VRIKCalibrator to help with calibrating avatars to head and hand controllers plus additional trackers on the body and feet.
If you only need to calibrate the size of the avatar, it is easiest to do so by having the player stand up straight, then comparing the height of the head target to the height of the avatar's head bone:

\code
float sizeF = (ik.solver.spine.headTarget.position.y - ik.references.root.position.y) / (ik.references.head.position.y - ik.references.root.position.y);
ik.references.root.localScale *= sizeF;
\endcode

<b>Locomotion:</b>
<BR> The built-in procedural locomotion was developed in the early days of VR and designed for foot shuffling around a few square meters space. It does not work well for room-scale or thumbstick locomotion. 
For that it would be best to use an animated approach - make a simple 8-direction strafing animation blend tree and use that to make the character follow the horizontal direction towards the HMD by root motion or scripted transformation.

<b>Component variables:</b>
	- <b>VRIK.fixTransforms</b> - if true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance
    - <b>VRIK.references</b> - bone mapping. Right-click on the component header and select 'Auto-detect References' of fill in manually if not a Humanoid character. Chest, neck, shoulder and toe bones are optional. VRIK also supports legless characters. If you do not wish to use legs, leave all leg references empty.

\image html VRIKComponent.png

<b>Solver variables:</b>
	- <b>VRIK.solver.IKPositionWeight</b> - master weight of the solver.
	- <b>VRIK.solver.LOD</b> - LOD 0: Full quality solving. LOD 1: Shoulder solving, stretching plant feet disabled, spine solving quality reduced. This provides about 30% of performance gain. LOD 2: Culled, but updating root position and rotation if locomotion is enabled.
	- <b>VRIK.solver.plantFeet</b> - if true, will keep the toes planted even if head target is out of reach, so this can cause the camera to exit the head if it is too high for the model to reach. Enabling this increases the cost of the solver as the legs will have to be solved multiple times.

\image html VRIKSolver.png

<b>Spine variables:</b>
	- <b>VRIK.solver.spine.headTarget</b> - the head target. This should not be the camera Transform itself, but a child GameObject parented to it so you could adjust it's position/rotation to match the orientation of the head bone. The best practice for setup would be to move the camera to the avatar's eyes, duplicate the avatar's head bone and parent it to the camera. Then assign the duplicate to this slot.
	- <b>VRIK.solver.spine.pelvisTarget</b> - the pelvis target (optional), useful for seated rigs or if you had an additional tracker on the backpack or belt are. The best practice for setup would be to duplicate the avatar's pelvis bone and parenting it to the pelvis tracker. Then assign the duplicate to this slot.
	- <b>VRIK.solver.spine.positionWeight</b> - positional weight of the head target. Note that if you have nulled the headTarget, the head will still be pulled to the last position of the headTarget until you set this value to 0.
    - <b>VRIK.solver.spine.rotationWeight</b> - rotational weight of the head target. Note that if you have nulled the headTarget, the head will still be rotated to the last rotation of the headTarget until you set this value to 0.
    - <b>VRIK.solver.spine.pelvisPositionWeight</b> - positional weight of the pelvis target. Note that if you have nulled the pelvisTarget, the pelvis will still be pulled to the last position of the pelvisTarget until you set this value to 0.
    - <b>VRIK.solver.spine.pelvisRotationWeight</b> - Rotational weight of the pelvis target. Note that if you have nulled the pelvisTarget, the pelvis will still be rotated to the last rotation of the pelvisTarget until you set this value to 0.
    - <b>VRIK.solver.spine.chestGoal</b> - if chestGoalWeight is greater than 0, the chest will be turned towards this Transform.
    - <b>VRIK.solver.spine.chestGoalWeight</b> - weight of turning the chest towards the chestGoal.
    - <b>VRIK.solver.spine.minHeadHeight</b> - minimum height of the head from the root of the character.
    - <b>VRIK.solver.spine.bodyPosStiffness</b> - determines how much the body will follow the position of the head.
    - <b>VRIK.solver.spine.bodyRotStiffness</b> - determines how much the body will follow the rotation of the head.
    - <b>VRIK.solver.spine.neckStiffness</b> - determines how much the chest will rotate to the rotation of the head.
    - <b>VRIK.solver.spine.rotateChestByHands</b> - the amount of rotation applied to the chest based on hand positions.
    - <b>VRIK.solver.spine.chestClampWeight</b> - clamps chest rotation. Value of 0.5 allows 90 degrees of rotation for the chest relative to the head. Value of 0 allows 180 degrees and value of 1 means the chest will be locked relative to the head.
    - <b>VRIK.solver.spine.headClampWeight</b> - clamps head rotation. Value of 0.5 allows 90 degrees of rotation for the head relative to the headTarget. Value of 0 allows 180 degrees and value of 1 means head rotation will be locked to the target.
    - <b>VRIK.solver.spine.moveBodyBackWhenCrouching</b> - moves the body horizontally along -character.forward axis by that value when the player is crouching.
    - <b>VRIK.solver.spine.maintainPelvisPosition</b> - how much will the pelvis maintain it's animated position?
    - <b>VRIK.solver.spine.maxRootAngle</b> - will automatically rotate the root of the character if the head target has turned past this angle.
    - <b>VRIK.solver.spine.rootHeadingOffset</b> - angular offset for root heading. Adjust this value to turn the root relative to the HMD around the vertical axis. Usefulf for fighting or shooting games where you would sometimes want the avatar to stand at an angled stance.

\image html VRIKSpine.png

<b>Arm variables:</b>
	- <b>VRIK.solver.leftArm.target</b> - the hand target. This should not be the hand controller itself, but a child GameObject parented to it so you could adjust it's position/rotation to match the orientation of the hand bone. The best practice for setup would be to move the hand controller to the avatar's hand as it it was held by the avatar, duplicate the avatar's hand bone and parent it to the hand controller. Then assign the duplicate to this slot.
    - <b>VRIK.solver.leftArm.bendGoal</b> - the elbow will be bent towards this Transform if 'Bend Goal Weight' > 0.
    - <b>VRIK.solver.leftArm.positionWeight</b> - positional weight of the hand target. Note that if you have nulled the target, the hand will still be pulled to the last position of the target until you set this value to 0.
    - <b>VRIK.solver.leftArm.rotationWeight</b> - rotational weight of the hand target. Note that if you have nulled the target, the hand will still be rotated to the last rotation of the target until you set this value to 0.
    - <b>VRIK.solver.leftArm.shoulderRotationMode</b> - different techniques for shoulder bone rotation.
    - <b>VRIK.solver.leftArm.shoulderRotationWeight</b> - the weight of shoulder rotation.
    - <b>VRIK.solver.leftArm.shoulderTwistWeight</b> - the weight of twisting the shoulders backwards when arms are lifted up.
    - <b>VRIK.solver.leftArm.bendGoalWeight</b> - if greater than 0, will bend the elbow towards the 'Bend Goal' Transform.
    - <b>VRIK.solver.leftArm.swivelOffset</b> - angular offset of the elbow bending direction.
    - <b>VRIK.solver.leftArm.wristToPalmAxis</b> - local axis of the hand bone that points from the wrist towards the palm. Used for defining hand bone orientation. If you have copied VRIK component from another avatar that has different bone orientations, right-click on VRIK header and select "Guess Hand Orientations" from the context menu.
    - <b>VRIK.solver.leftArm.palmToThumbAxis</b> - local axis of the hand bone that points from the palm towards the thumb. Used for defining hand bone orientation If you have copied VRIK component from another avatar that has different bone orientations, right-click on VRIK header and select 'Guess Hand Orientations' from the context menu..
    - <b>VRIK.solver.leftArm.armLengthMlp</b> - use this to make the arm shorter/longer. Works by displacement of hand and forearm localPosition.
    - <b>VRIK.solver.leftArm.stretchCurve</b> - evaluates stretching of the arm by target distance relative to arm length. Value at time 1 represents stretching amount at the point where distance to the target is equal to arm length. Value at time 2 represents stretching amount at the point where distance to the target is double the arm length. Value represents the amount of stretching. Linear stretching would be achieved with a linear curve going up by 45 degrees. Increase the range of stretching by moving the last key up and right at the same amount. Smoothing in the curve can help reduce elbow snapping (start stretching the arm slightly before target distance reaches arm length). To get a good optimal value for this curve, please go to the 'VRIK (Basic)' demo scene and copy the stretch curve over from the Pilot character.

\image html VRIKArm.png

<b>Leg variables:</b>
	- <b>VRIK.solver.leftLeg.target</b> - the foot/toe target. This should not be the foot tracker itself, but a child GameObject parented to it so you could adjust it's position/rotation to match the orientation of the foot/toe bone. If a toe bone is assigned in the References, the solver will match the toe bone to this target. If no toe bone assigned, foot bone will be used instead.
    - <b>VRIK.solver.leftLeg.bendGoal</b> - the knee will be bent towards this Transform if 'Bend Goal Weight' > 0.
    - <b>VRIK.solver.leftLeg.positionWeight</b> - positional weight of the toe/foot target. Note that if you have nulled the target, the foot will still be pulled to the last position of the target until you set this value to 0.
    - <b>VRIK.solver.leftLeg.rotationWeight</b> - rotational weight of the toe/foot target. Note that if you have nulled the target, the foot will still be rotated to the last rotation of the target until you set this value to 0.
    - <b>VRIK.solver.leftLeg.bendGoalWeight</b> - if greater than 0, will bend the knee towards the 'Bend Goal' Transform.
    - <b>VRIK.solver.leftLeg.swivelOffset</b> - angular offset of knee bending direction.
    - <b>VRIK.solver.leftLeg.bendToTargetWeight</b> - if 0, the bend plane will be locked to the rotation of the pelvis and rotating the foot will have no effect on the knee direction. If 1, to the target rotation of the leg so that the knee will bend towards the forward axis of the foot. Values in between will be slerped between the two.
    - <b>VRIK.solver.leftLeg.legLengthMlp</b> - use this to make the leg shorter/longer. Works by displacement of foot and calf localPosition.
    - <b>VRIK.solver.leftLeg.stretchCurve</b> - evaluates stretching of the leg by target distance relative to leg length. Value at time 1 represents stretching amount at the point where distance to the target is equal to leg length. Value at time 1 represents stretching amount at the point where distance to the target is double the leg length. Value represents the amount of stretching. Linear stretching would be achieved with a linear curve going up by 45 degrees. Increase the range of stretching by moving the last key up and right at the same amount. Smoothing in the curve can help reduce knee snapping (start stretching the arm slightly before target distance reaches leg length). To get a good optimal value for this curve, please go to the 'VRIK (Basic)' demo scene and copy the stretch curve over from the Pilot character.

\image html VRIKLeg.png

<b>Locomotion variables:</b>
    - <b>VRIK.solver.locomotion.weight</b> - used for blending in/out of procedural locomotion.
    - <b>VRIK.solver.locomotion.footDistance</b> - tries to maintain this distance between the legs.
    - <b>VRIK.solver.locomotion.stepThreshold</b> - makes a step only if step target position is at least this far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past the 'Angle Threshold'.
    - <b>VRIK.solver.locomotion.angleThreshold</b> - makes a step only if step target position is at least 'Step Threshold' far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past this value.
    - <b>VRIK.solver.locomotion.comAngleMlp</b> - multiplies angle of the center of mass - center of pressure vector. Larger value makes the character step sooner if losing balance.
    - <b>VRIK.solver.locomotion.maxVelocity</b> - maximum magnitude of head/hand target velocity used in prediction.
    - <b>VRIK.solver.locomotion.velocityFactor</b> - the amount of head/hand target velocity prediction.
    - <b>VRIK.solver.locomotion.maxLegStretch</b> - how much can a leg be extended before it is forced to step to another position? 1 means fully stretched.
    - <b>VRIK.solver.locomotion.rootSpeed</b> - the speed of lerping the root of the character towards the horizontal mid-point of the footsteps.
    - <b>VRIK.solver.locomotion.stepSpeed</b> - the speed of moving a foot to the next position.
    - <b>VRIK.solver.locomotion.stepHeight</b> - the height of the foot by normalized step progress (0 - 1).
    - <b>VRIK.solver.locomotion.heelHeight</b> - the height offset of the heel by normalized step progress (0 - 1).
    - <b>VRIK.solver.locomotion.relaxLegTwistMinAngle</b> - rotates the foot while the leg is not stepping to relax the twist rotation of the leg if ideal rotation is past this angle.
    - <b>VRIK.solver.locomotion.relaxLegTwistSpeed</b> - the speed of rotating the foot while the leg is not stepping to relax the twist rotation of the leg.
    - <b>VRIK.solver.locomotion.stepInterpolation</b> - tnterpolation mode of the step.
    - <b>VRIK.solver.locomotion.offset</b> - offset for the approximated center of mass.

\image html VRIKLocomotion.png


<b>Script References:</b>
	- <a href="http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_i_k_solver_v_r.html">Solver </a> 
	- <a href="http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_v_r_i_k.html">Component</a> 

*/

/*! \page page17 Extending Final IK
The %IK solvers and rotation limits of FinalIK were built from the ground up with extendability in mind. 
<BR>Some of the components of FinalIK, such as BipedIK, are essentially little more than just collections of %IK solvers.

\section customcomponents Writing Custom IK Components
  		Before you can exploit the full power of FinalIK, it is important to know a few things about it's architecture.
  		
  		<b>The difference between %IK components and %IK solvers:</b>
  		<BR> By architecture, %IK solver is a class that actually contains the inverse kinematics functionality, while the function of an %IK component is only to harbor, initiate and update it's solver and provide helpful scene view handles as well as custom inspectors.  
  		<BR> Therefore, %IK solvers are fully independent of their components and can even be used without them through direct reference:
  		
\code
using RootMotion.FinalIK;

public IKSolverCCD spine = new IKSolverCCD();
public IKSolverLimb limb = new IKSolverLimb();

void Start() {
	// The root transform reference is used in the initiation of IK solvers for multiple reasons depending on the solver.
	// heuristic solvers IKSolverCCD, IKSolverFABRIK and IKSolverAim only need it as context for logging warnings, 
	// character solvers IKSolverLimb, IKSolverLookAt, BipedIK and IKSolverFullBodyBiped use it to define their orientation relative to the character,
	// IKSolverFABRIKRoot uses it as the root of all of it's FABRIK chains.
	spine.Initiate(transform);
	limb.Initiate(transform);
}

void LateUpdate() {
	// Updating the IK solvers in a specific order.
	// In the case of multiple IK solvers handling a bone hierarchy, it is usually wise to solve the parents first.
	spine.Update();
	limb.Update();
}
\endcode
		You now have essentially a custom %IK component.
		<BR>This can be helpful if you needed to keep all the functionality of your %IK system in a single component, like BipedIK, so you would not have to manage many different %IK components in your scene.
  	
  	\section customrotationlimits Writing Custom Rotation Limits
  	All rotation limits in Final %IK extend from the abstract RotationLimit class. To compose your own, you would as well need to extend from this base class and override the abstract method 
  	\code 
  	protected abstract Quaternion LimitRotation(Quaternion rotation); 
  	\endcode
  	
  	In this method you will have to apply the constraint to and return the input Quaternion.
  	<BR>It is important to note that the input Quaternion is already converted to the default local rotation space of the gameobject, meaning if you return Quaternion.identity, the gameobject will always remain fixed to it's initial local rotation.
  	
  	The following code could be a template for a custom rotation limit:
  	
\code
using RootMotion.FinalIK;

// Declaring the class and extending from RotationLimit.cs
public class RotationLimitCustom: RotationLimit {
	
	// Limits the rotation in the local space of this instance's Transform.
	protected override Quaternion LimitRotation(Quaternion rotation) {		
		return MyLimitFunction(rotation);
	}

}

\endcode
	The new rotation limit gets recognized and applied automatically by all constrainable %IK solvers.
  	

\section combining Combining IK Components
  		When creating more complex %IK systems, you will probably need full control over the updating order of your solvers. To do that, you can just disable their components and manage their solvers from an external script.
  		<BR>All %IK components extend from the abstract IK class and all %IK solvers extend from the abstract IKSolver class. This enables you to easily handle or replace the solvers even without needing to know the specific type of the solver.
  		<BR>Controlling the updating order of multiple %IK components:

        \htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/VhXuMoGA1D0" frameborder="0" allowfullscreen></iframe>\endhtmlonly
        <BR>

\code
using RootMotion.FinalIK;

// Array of IK components that you can assign from the inspector. 
// IK is abstract, so it does not matter which specific IK component types are used.
public IK[] components;
  	
void Start() {
	// Disable all the IK components so they won't update their solvers. Use Disable() instead of enabled = false, the latter does not guarantee solver initiation.
	foreach (IK component in components) component.Disable();
}

void LateUpdate() {
	// Updating the IK solvers in a specific order. 
	foreach (IK component in components) component.GetIKSolver().Update();
}
\endcode

<b>Animate Physics</b>
<BR>When your character has Animate Physics checked (UpdateMode.AnimatePhysics since Unity 4.5), you will need to check if a FixedUpdate has been called before updating the IK solvers in LateUpdate.
Otherwise the IK solvers will be updated multiple times before the Animator/Animation overwrites the pose and accumulate resulting in a high frequency flicker. So the way to update the solvers with Animate Physics would be like this;

\code
using RootMotion.FinalIK;

// Array of IK components that you can assign from the inspector. 
// IK is abstract, so it does not matter which specific IK component types are used.
public IK[] components;

private bool updateFrame;
  	
void Start() {
	// Disable all the IK components so they won't update their solvers. Use Disable() instead of enabled = false, the latter does not guarantee solver initiation.
	foreach (IK component in components) component.Disable();
}

void FixedUpdate() {
	updateFrame = true;
}

void LateUpdate() {
	// Do nothing if FixedUpdate has not been called since the last LateUpdate
	if (!updateFrame) return;
	updateFrame = false;
	
	// Updating the IK solvers in a specific order. 
	foreach (IK component in components) component.GetIKSolver().Update();
}
\endcode

*/

/*! \page page18 FAQ

\section FullBodyBipedIK

<b>- Can I use FBBIK to create animation clips for my character, how does it save the animation clips?</b>
<BR>FBBIK is not designed for animation authoring, but for runtime animation modification, meaning it will run on top of whatever animation you have to provide you more flexibility and accuracy when interacting with ingame objects and other character. 
<BR>For example if your character is climbing a wall, you can apply Full Body IK on top of it to make sure your hands and feet always remain fixed to the ledges.
<BR>Since Final IK v1.9 the package includes a baking tool that can be used to create Humanoid, Generic and Legacy animation clips from your custom IK procedures, Timeline, Mecanim layer setup, etc...

<b>- Hope that this will not only work with a special 3d program. Is there any information on how to export from a 3D animation package?</b>
<BR>Final IK has nothing to do with any 3D animation packages. You would animate and export your animation clips just like you are doing it now, 
<BR>the IK components will just provide a library of possibilities for extending, modifying and characterising those animations. 
<BR>The primary function of Full Body IK is not providing a rig for creating animations in Unity from scratch, it is modifying the existing animations 
<BR>and retargeting specific body parts to match the dynamic realtime environment. 
<BR>It will dramatically reduce the amount of animation work you will have to do for your game. 

<b>- This system actually allows to have completly non-animated animations that are run only from scripts right? </b>
<BR> Yes.

<b>- Compared to mocap, what can I expect with the IK-based animations from your system?</b>
<BR>IK animation can not be compared to mocap, ideally they should work together, not replace each other.
<BR>Procedural IK only takes you so far. If you are aiming for real AAA quality and you want to compare your game with the Assassins Creed or Max Payne 3, you'll still need top notch mocap base animation.

<b>- Does Mecanim work better than Legacy in conjunction with Final IK or if it doesn't matter which system to use all?</b>
<BR>It does not matter at all, go with what you prefer.

<b>- Can we tween in and out of an IK'd pose? </b>
<BR>Yes, we can!

<b>- Can I remove/add limbs to or from FBBIK?</b>
<BR>No, not yet at least. In the current version, you can't detach a limb AND continue using Full Body IK. You can disable FBBIK and then detach the limb (which is fine in 99% of the use cases because the character would usually just die), but you can't use the solver after you have deleted the bones.
If there is a way to get rid of the limb geometry, but maintain the bones, then there is no problem, but remember, no removing the bones. I hope to overcome that limitation in the full release. Same goes for adding extra limbs. You can still use a seperate CCD/FABRIK or Limb IK component on the extra limb if you need to.

<b>- Can it actually work perfectly upside down?</b>
<BR>Yes it works upside down, you can rotate the characters however you like and still use IK.

<b>- Does Final IK allow you to rig a 3D mesh that does not have any bones?</b>
<BR>No, it needs to have bones and skinning.

<b>- Can I use Rotation Limits with Full Body Biped IK?</b>
<BR>Rotation Limits can be used with the CCD, FABRIK and Aim IK components, FBBIK has it's own dedicated constraints that keep the limbs bending in the right direction and the spine from collapsing into itself.

\section Misc
<b>- Do you think FABRIK can be used for simulating hair or a piece of cloth on a biped?</b>
<BR>Yes FABRIK theoretically can be used to simulate cloth or hair. But in a future release, I'm planning to include Particle IK, which would probably be more suitable for this, as it is the de facto standard for those kinds of simulations. 

<b>- Does Final IK work with 2D?</b>
<BR>AimIK, CCDIK, FABRIK, TrigonometricIK, LimbIK, ArmIK and LegIK can be constrained to 2D. FullBodyBipedIK and VRIK can not be used on 2D characters.
 */

/*! \page page19 3rd Party Support

Itegration packages to 3rd party assets can be found in "Assets/Plugins/RootMotion/FinalIK/_Integration".
 */