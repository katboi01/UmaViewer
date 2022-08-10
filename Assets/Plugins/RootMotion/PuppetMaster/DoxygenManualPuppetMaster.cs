/*! \mainpage

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/on7wAz0fsGg" frameborder="0" allowfullscreen></iframe>\endhtmlonly
<br>Welcome to PuppetMaster, an advanced character physics tool for Unity!

\section contains PuppetMaster Contains 

 - Tools for animating ragdolls of any kind
 - Automated, easy to use ragdoll generator for all biped characters
 - Ragdoll Editor - enabling you to intuitively edit colliders and joints in the Scene View.
 - Joint Inspectors - visualizing and editing Character and Configurable Joints in the Scene View

\section technicaloverview Technical Overview

 - Does NOT require Unity Pro
 - Works with Humanoid, Generic and Legacy characters.
 - Written in C#, all scripts are namespaced under %RootMotion and %RootMotion.Dynamics to avoid any naming conflicts with Your existing assets.
 - Tested on Standalone, Web Player, IOS and Android
 - Custom undoable inspectors and scene view handles
 - Warning system to safeguard from null references and invalid setups (will not overflow your console with warnings)
 - Optimized for great performance
 - Modular, easily extendable. Compose your own custom character rigs
 - User manual, HTML documentation, fully documented code
 - Demo scenes and example scripts for all components
 - Tested on a wide range of characters
 
*/

/*! \page page1 Creating Ragdolls

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/y-luLRVmL7E?list=PLVxSIA1OaTOuE2SB9NUbckQ9r2hTg4mvL" frameborder="0" allowfullscreen></iframe>\endhtmlonly

<b>BipedRagdollCreator</b>
<br>
PuppetMaster contains a very easy to use visual ragdoll creation tool for biped characters (can actually be used on any character with at least 4 limbs) called BipedRagdollCreator.
When the BipedRagdollCreator.cs component is enabled on a character, the ragdoll components will be destroyed and recreated each time you change the references or options. 
This enables for fast and intuitive live editing.

<b>Getting started</b>
	- Drag a character model to the scene and attach the BipedRagdollCreator.cs component. Biped bone references will be filled in automatically if it is a Humanoid character. In other cases, they can be filled in manually. 
	- Click on "Create a Ragdoll" to start live-updating and editing the ragdoll. You should then see the Rigidbody, Collider and Joint components added to the character. 
	- Note that with all adjustments to the parameters of BipedRagdollCreator the ragdoll components will be deleted and recreated, so do not make any references to them until the BipedRagdollCreator has been removed.
	- Only ConfigurableJoints are supported by the PuppetMaster at this time so use "Configurable" as "Joints".
	- Once you are happy with the basic Collider and Joint types and other settings in the "Options", click on "Start Editing Manually" to make final manual adjustments using the RagdollEditor. This will remove the BipedRagdollCreator component.

<b>Component variables:</b>
	- <b>references</b> - references to the biped bones.
	- <b>weight</b> - the total weight of the character. Will be distributed biometrically between the ragdoll Rigidbodies.
	- <b>spine</b> - toggle the spine bone included in the ragdoll.
	- <b>chest</b> - toggle the chest bone included in the ragdoll.
	- <b>hands</b> - toggle the hand bones included in the ragdoll.
	- <b>feet</b> - toggle the foot bones included in the ragdoll.
	- <b>joints</b> - build the ragdoll with ConfigurableJoints or CharacterJoints? NB! PuppetMaster can use only ConfigurableJoints.
	- <b>jointRange</b> - multiplies joint limit ranges.
	- <b>colliderLengthOverlap</b> - determines how much the colliders overlap each other, supports also negative values.
	- <b>torsoColliders</b> - use BoxColliders or CapsuleColliders for the body part.
	- <b>headCollider</b> - use BoxCollider or CapsuleCollider for the body part.
	- <b>armColliders</b> - use BoxColliders or CapsuleColliders for the body part.
	- <b>handColliders</b> - use BoxColliders or CapsuleColliders for the body part.
	- <b>legColliders</b> - use BoxColliders or CapsuleColliders for the body part.
	- <b>footColliders</b> - use BoxColliders or CapsuleColliders for the body part.

\image html BipedRagdollCreator2.png

<b>Creating ragdolls in runtime:</b>

\code
		[Tooltip("The character prefab/FBX.")]
		public GameObject prefab;

		void Start() {
			// Instantiate the character
			GameObject instance = GameObject.Instantiate(prefab);

			// Find bones (Humanoids)
			BipedRagdollReferences r = BipedRagdollReferences.FromAvatar(instance.GetComponent<Animator>());

			// How would you like your ragdoll?
			BipedRagdollCreator.Options options = BipedRagdollCreator.AutodetectOptions(r);

			// Edit options here if you need to
			//options.headCollider = RagdollCreator.ColliderType.Box;
			//options.weight *= 2f;
			//options.joints = RagdollCreator.JointType.Character;

			// Create the ragdoll
			BipedRagdollCreator.Create(r, options);

			Debug.Log("A ragdoll was successfully created.");
		}

		void Update() {
			// If bone proportions have changed, just clear and recreate:
			//BipedRagdollCreator.ClearBipedRagdoll(r); //ClearAll if you have changed references
			//BipedRagdollCreator.Create(r, options);
		}
\endcode
 */

/*! \page page2 Editing Ragdolls

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/y-luLRVmL7E?list=PLVxSIA1OaTOuE2SB9NUbckQ9r2hTg4mvL" frameborder="0" allowfullscreen></iframe>\endhtmlonly

<b>RagdollEditor</b>
<br>
RagdollEditor is an intuitive visualized and Scene View tool for editing Colliders, Joints and Rigidbody settings. Symmetry is supported for collider position, size and joint limit editing. All actions with the RagdollEditor are undoable.

\image html RagdollEditorScene.png

<b>Getting started</b>
	- Add the RagdollEditor.cs component to the root of the ragdoll.
	- Click on the green buttons on the Colliders/Joints to select them. 
	- Use the Move/Scale handles to edit the Colliders and Joints in the Scene View.
	- Remove the component when you are done.

<b>Component interface:</b>
	- <b>Mass Multiplier</b> - multiplies the mass of all the Rigidbodies with the value.
	- <b>Set All Kinematic</b> - set all Rigidbodies kinematic.
	- <b>Set All Non-Kinematic</b> - set all Rigidbodies non-kinematic.
	- <b>Disable Preprocessing</b> - uncheck 'enablePreprocessing' on all Joints. This can make the ragdoll more stable under certain circumstances.
	- <b>Enable Preprocessing</b> - check 'enablePreprocessing' on all Joints.

\image html RagdollEditor.png

<b>Collider tool:</b>
	- <b>Select GameObject</b> - changes Unity selection to the currently edited Collider's GameObject
	- <b>Edit Mode</b> - switch to Joint editing mode.
	- <b>Symmetry</b> - enables symmetric editing. Symmetry works for the closest Collider (within a threshold) on the other side of the ZY plane (local space of the GameObject of RagdollEditor).
	- <b>Convert To Capsule Collider</b> - converts the selected collider to CapsuleCollider.
	- <b>Convert To Sphere Collider</b> - converts the selected collider to SphereCollider.
	- <b>Rotate Collider</b> - rotates the collider size vector orthogonally.

\image html RagdollEditorColliders.png

<b>Joint tool:</b>
	- <b>Select GameObject</b> - changes Unity selection to the currently edited Joint's GameObject
	- <b>Edit Mode</b> - switch to Collider editing mode.
	- <b>Symmetry</b> - enables symmetric editing. Symmetry works for the closest Joint (within a threshold) on the other side of the ZY plane (local space of the GameObject of RagdollEditor).
	- <b>Connected Body</b> - select or change the connectedBody of the selected Joint.
	- <b>Switch Yellow/Green</b> - switches the respective Joint axes.
	- <b>Switch Yellow/Blue</b> - switches the respective Joint axes.
	- <b>Switch Green/blue</b> - switches the respective Joint axes.
	- <b>Invert Yellow</b> - inverts Joint.axis.

\image html RagdollEditorJoints.png

 */

/*! \page page3 PuppetMaster Overview

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/on7wAz0fsGg" frameborder="0" allowfullscreen></iframe>\endhtmlonly

<br>
<b>Terminology</b>
<br>
	PuppetMaster uses a <b>"Dual Rig"</b> that consists of the normal animated character (from hereby: <b>"Target"</b>) and the simplified ragdoll structure (<b>"Puppet"</b>). 
	The main purpose of PuppetMaster is to make the Puppet ragdoll physically follow the motion and animation of the animated Target character. 
	It will do so by two means, first is articulating the joints (<b>"Muscles"</b>), the other is pinning the rigidbodies to their targets using AddForce commands (<b>"Pins"</b>).
	The classes that control the behaviour of the Muscles and handle their strength, pinning and other properties are called <b>"Puppet Behaviours"</b>.
	The dual rig and the Behaviours will share a common parent GameObject (<b>"Root"</b>) that serves as a container for the PuppetMaster character rig.
	The kinematic Target character will be mapped to the dynamic Puppet ragdoll by the means of <b>"Mapping"</b>, which can also be smoothly blended in and out for the entire character or for each Muscle separately.

<b>Dual Rig</b>
<br>
	\image html DualRigExplained.png
	The main advantage of using a dual rig over a single character setup is performance. It is much less expensive mostly due to not having the necessity of performing costly transformations with objects that have Colliders attached.
	The performance win becomes even greater when having to do IK/FK procedures on the target pose. The PuppetMaster can be smoothly blended to run in Kinematic or Disabled mode. The former will simply make the ragdoll kinematic and match it with the target, the latter will completely deactivate the ragdoll when you don't need it.
	The dual rig also allows for choosing between flat or tree hierarchy modes for the ragdoll (click "Flat/Tree Hierarhcy" from the PuppetMaster's context menu) and will allow for sharing ragdolls between multiple characters in a future release of PuppetMaster.
	The PuppetMaster will automatically set up the dual rig for you when provided with a reference to a traditional ragdoll character.

<b>Target</b>
<br>
	\image html Target.png
	The Target can be thought of as the normal animated character that you would see in any game, it usually has a character controller and any other gameplay components attached. 
	Each frame, PuppetMaster will read the pose of the Target and feed it to the Muscles of the Puppet for following. 
	After physics has solved, the Target will be mapped to the pose of the Puppet based on the mapping settings and stay there until animation overwrites it the next frame.

<b>Puppet</b>
<br>
	\image html Puppet.png
	The simplified ragdoll structure (Puppet) is essentially a duplicate of the Target, but with ragdoll Components attached and all other Components and GameObjects unrelated to physics stripped.
	The PuppetMaster requires for the ragdoll to be set up using ConfigurableJoints only. Any other Joint can be converted to a ConfigurableJoint by selecting the (root) GameObject and clicking on "GameObject/Convert to ConfigurableJoints" from the menu bar.
	The Puppet ragdoll can be set up in tree or flat hierarchy modes (click "Flat/Tree Hierarhcy" from the PuppetMaster's context menu). 

<b>Muscles</b>
<br>
	\image html Muscles.png
	The PuppetMaster will turn each ConfigurableJoint of the Puppet into a Muscle that maintains a reference to it's animated target and automatically calculates Joint target rotations, pinning forces and other values.
	When the Puppet is unpinned, the ragdoll will follow the animation in muscle space and the result is physically authentic and accurate. 
	When pinned, the pinning forces will move the ragdoll bones to the world space position of their targets, they can be imagined as spring joints pulling each ragdoll bone towards their animated target.
	Pinning is therefore an unnatural force that can be managed to make the ragdoll simulate game character motion that physically would be almost unachievable. 
	The Muscles are listed in the PuppetMaster's inspector, each having individual properties, enabling you to specify the physical behaviour of each Muscle or Muscle Group.

<b>Modes</b>
<br>
	\image html Modes.png
	The PuppetMaster can run in 3 Modes. <b>Active</b> is the active ragdoll mode that makes the Puppet physically follow it's Target by employing the muscle forces, pins or both at the same time. The Puppet is able to collide with and be affected by objects in the scene.
	The <b>Kinematic</b> mode makes the Rigidbodies of the Puppet kinematic and the Muscles will not be used anymore. The Puppet is still able to collide with objects and receive raycast hits.
	The <b>Disabled</b> mode completely deactivates the Puppet along with it's Rigidbodies and Colliders. In Disabled mode the PuppetMaster will not have any effect on the performance. All modes can be blended in/out smoothly in respect to the "Blend Time" parameter.

<b>Puppet Behaviours</b>
<br>
	\image html Behaviours2.png
	The Puppet Behaviours are classes that inherit from the abstract BehaviourBase.cs and that's main idea is to provide a pattern for developing functionalities that dynamically adjust muscle and pin weights, strength and other properties or make kinematic adjustments to the target pose.
	The most important Puppet Behaviour is the BehaviourPuppet.cs that handles the pinning of Puppets to target animation, releasing those pins in case of collision and re-tightening them when getting up from the ground.
	The Puppet Behaviours can be switched, for example when the BehaviourPuppet looses balance, it can theoretically switch to BehaviourCatchFall or BehaviourWindmill (both missing from the initial release, but will be developed in the future).
	The Puppet Behaviours are designed so tha they would not contain a single external object reference. That means they can be simply duplicated and moved to another Puppet.

*/

/*! \page page4 PuppetMaster Setup

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/mIN9bxJgfOU?list=PLVxSIA1OaTOuE2SB9NUbckQ9r2hTg4mvL" frameborder="0" allowfullscreen></iframe>\endhtmlonly

To set up a character with ragdoll components as a Puppet, the easiest way is to simply add the PuppetMaster component to it and click on the "Set Up PuppetMaster" button.
That will make a duplicate of the character and set the duplicate up as the Puppet while the selected instance will become the Target. Both will be parented to a new GameObject that is the root container for the entire character rig.
A GameObject with the character's name + " Behaviours" will also be added. All Puppet Behaviours (BehaviourPuppet, BehaviourFall...) should be parented to it.

Alternately you can assign another character hierarchy to become the Target. PuppetMaster supports sharing ragdolls between multiple characters as long as the positions of the muscles match with their targets. 
Rotations can be different, so you can even use another rig with different bone orientations.

"Character Controller Layer" and "Ragdoll Layer" will be applied to the Target and Puppet respectively to avoid collisions between the character controller and the ragdoll colliders. 
Make sure collisions between the two layers are disabled in the Edit/Project Settings/Physics/Layer Collision Matrix.

\image html PuppetMasterSetup.png

<b>Creating puppets in run-time</b>
<br>
	The procedure and code for setting up puppets in run-time is demonstrated in the "Creating Puppets In Runtime" demo scene and CreatePuppetInRuntime.cs script.
	It requires for the character to be already set up with ragdoll components.

\code
		Transform ragdoll = GameObject.Instantiate(ragdollPrefab, transform.position, transform.rotation) as Transform;
		ragdoll.name = instanceName;

		// This will duplicate the "ragdoll" instance, remove the ragdoll components from the original and use it as the animated target, setting the duplicate up as a puppet.
		PuppetMaster.SetUp(ragdoll, characterControllerLayer, ragdollLayer);

		// or...
		//PuppetMaster.SetUp(target, ragdoll, characterControllerLayer, ragdollLayer);

		// or if you want the "ragdoll" instance to become the puppet, not the target:
		//PuppetMaster.SetUpTo(target, characterControllerLayer, ragdollLayer);

		Debug.Log("A ragdoll was successfully converted to a Puppet.");
\endcode

*/

/*! \page page5 PuppetMaster Component

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/LYusqeqHAUc" frameborder="0" allowfullscreen></iframe>\endhtmlonly

<b>Simulation</b>
	- <b>state</b> - sets/sets the state of the puppet (Alive, Dead or Frozen). Frozen means the ragdoll will be deactivated once it comes to stop in dead state.
	- <b>stateSettings</b> - settings for killing and freezing the puppet.
	<BR><b>killDuration</b> - how much does it take to weigh out muscle weight to deadMuscleWeight?
	<BR><b>deadMuscleWeight</b> - the muscle weight mlp while the puppet is Dead.
	<BR><b>deadMuscleDamper</b> - the muscle damper add while the puppet is Dead.
	<BR><b>maxFreezeSqrVelocity</b> - the max square velocity of the ragdoll bones for freezing the puppet.
	<BR><b>freezePermanently</b> - if true, PuppetMaster, all it's behaviours and the ragdoll will be destroyed when the puppet is frozen.
	<BR><b>enableAngularLimitsOnKill</b> - if true, will enable angular limits when killing the puppet.
	<BR><b>enableInternalCollisionsOnKill</b> - if true, will enable internal collisions when killing the puppet.
	- <b>mode</b> - Active mode means all muscles are active and the character is physically simulated. Kinematic mode sets rigidbody.isKinematic to true for all the muscles and simply updates their position/rotation to match the target's. Disabled mode disables the ragdoll. Switching modes is done by simply changing this value, blending in/out will be handled automatically by the PuppetMaster.
	- <b>blendTime</b> - the time of blending when switching from Active to Kinematic/Disabled or from Kinematic/Disabled to Active. Switching from Kinematic to Disabled or vice versa will be done instantly.
	- <b>fixTargetTransforms</b> - if true, will fix the target character's Transforms to their default local positions and rotations in each update cycle to avoid drifting from additive reading-writing. Use this only if the target contains unanimated bones.
	- <b>solverIterationCount</b> - Rigidbody.solverIterationCount for the muscles of this Puppet.
	- <b>visualizeTargetPose</b> - if true, will draw the target's pose as green lines in the Scene view. This runs in the Editor only. If you wish to profile PuppetMaster, switch this off.

<b>Master Weights</b>
	- <b>mappingWeight</b> - the weight of mapping the animated character to the ragdoll pose.
	- <b>pinWeight</b> - the weight of pinning the muscles to the position of their animated targets using simple AddForce.
	- <b>muscleWeight</b> - the normalized strength of the muscles.

<b>Joint and Muscle Settings</b>
	- <b>muscleSpring</b> - the positionSpring of the ConfigurableJoints' Slerp Drive.
	- <b>muscleDamper</b> - the positionDamper of the ConfigurableJoints' Slerp Drive.
	- <b>pinPow</b> - adjusts the slope of the pinWeight curve. Has effect only while interpolating pinWeight from 0 to 1 and back.
	- <b>pinDistanceFalloff</b> - reduces pinning force the farther away the target is. Bigger value loosens the pinning, resulting in sloppier behaviour.
	- <b>updateJointAnchors</b> - when the target has animated bones between the muscle bones, the joint anchors need to be updated in every update cycle because the muscles' targets move relative to each other in position space. This gives much more accurate results, but is computationally expensive so consider leaving it off.
	- <b>supportTranslationAnimation</b> - enable this if any of the target's bones has translation animation.
	- <b>angularLimits</b> - should the joints use angular limits? If the PuppetMaster fails to match the target's pose, it might be because the joint limits are too stiff and do not allow for such motion. Uncheck this to see if the limits are clamping the range of your puppet's animation. Since the joints are actuated, most PuppetMaster simulations will not actually require using joint limits at all.
	- <b>internalCollisions</b> - should the muscles collide with each other? Consider leaving this off while the puppet is pinned for performance and better accuracy.  Since the joints are actuated, most PuppetMaster simulations will not actually require internal collisions at all.

<b>Individual Muscle Settings:</b>
	- <b>joint</b> - the ConfigurableJoint used by this muscle.
	- <b>target</b> - the target Transform that this muscle tries to follow.
	- <b>props</b> - the main properties of the muscle.
	<BR><b>group</b> - which body part does this muscle belong to? This might be used by some behaviours (BehaviourPuppet).
	<BR><b>mappingWeight</b> - the weight (multiplier) of mapping this muscle's target to the muscle.
	<BR><b>pinWeight</b> - the weight (multiplier) of pinning this muscle to it's target's position using a simple AddForce command.
	<BR><b>muscleWeight</b> - the muscle strength (multiplier).
	<BR><b>muscleDamper</b> - multiplier of the positionDamper of the ConfigurableJoints' Slerp Drive.
	<BR><b>mapPosition</b> - if true, will map the target to the world space position of the muscle. Normally this should be true for only the root muscle (the hips).

\image html PuppetMaster.png

*/

/*! \page page6 Props

PuppetMaster includes a helpful tool for attaching, detatching and managing physical props - the <b>PropMuscle</b> and <b>PuppetMasterProp</b> classes. 
PropMuscle is a special type of muscle that the PuppetMasterProp objects can be attached to.
For an example of prop usage, please see the "Prop" and "Melee" demo scenes.

<b>Getting Started:</b>

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/Bkw1gmrDr1I" frameborder="0" allowfullscreen></iframe>\endhtmlonly


<b>Prop Setup:</b>

Because of the dual rig structure, the prop needs to be set up so that it's root GameObject is the Muscle with the Rigidbody and ConfigurableJoint components and parented to it the Target along with it's mesh and renderer(s).
When the prop is picked up, the prop will be split up so that the "Mesh Root" will be parented to the Target root hierarchy and the rest of the prop with colliders to the Prop Muscle. When the prop is dropped, original hierarchy will be restored.

When the prop is picked up, the Rigidbody component on it will be destroyed as the PropMuscle already has a Rigidbody. The colliders of the prop will act as compound colliders for the PropMuscle Rigidbody. When the prop is dropped, the original Rigidbody will be restored.

\image html Prop.png
\image html PropScene.png

<br>
<b>PropMuscle component:</b>

PropMuscle is a special type of muscle, which all prop objects can be attached to. You can create a PropMuscle in the Editor by selecting the PuppetMaster GameObject and clicking on "Add Prop Muscle" on the bottom of the PuppetMaster component. 
Click on the blue button in the scene representing the PropMuscle and move/rotate it to where you need it to be.
If your puppet does not have hand muscles and the PropMuscle was attached to the forearm muscle, find the Prop Muscle Target GameObject in the Target Root hierarchy and parent it to the hand bone.
    - <b>Remove This Prop Muscle</b> - destroy this PropMuscle object.
	- <b>currentProp</b> - the PuppetMasterProp currently held by this Prop Muscle. To pick up a prop, just assign it as propMuscle.currentProp. To drop, set propMuscle.currentProp to null. Replacing this value with another prop drops any previously held props.
	- <b>additionalPinOffset</b> - offset of the additional pin from this Prop Muscle in local space.
    - <b>Add/Remove Additional Pin</b> - adds or removes the additional pin object. Additional Pins help to pin this prop muscle to animation from another point. Without it, the prop muscle would be actuated by Muscle Spring only, which is likely not enough to follow fast swinging animations. Move the additional pin by 'Additional Pin Offset' towards the end of the prop (tip of the sword).

\image html PropMuscle.png

Picking up, dropping and switching props with the PropMuscle is done by simply changing the propMuscle.currentProp value. When you assign a prop to it by propMuscle.currentProp = myProp, any props held by the PropMuscle will be dropped and the new myProp will be attached instead.
If you set currentProp to null, any props held by the PropMuscle will be dropped and the PropMuscle itself deactivated.

\code
		// Dropping props
		propMuscle.currentProp = null;

		// Dropping any props held, picking up myProp
		propMuscle.currentProp = myProp;
\endcode

When a prop is attached to the PropMuscle, it's localPosition and localRotation are set to zero/identity, so the best practice to adjust prop holding pivot would be to parent the prop to the PropMuscle, set localPosition/Rotation to zero and adjust the positions of the mesh and colliders until they fit perfectly at hand.

<br>
<b>PuppetMasterProp component:</b>
    - <b>meshRoot</b> - Mesh Root will be parented to Prop Muscle's target when this prop is picked up. To make sure the mesh and the colliders match up, Mesh Root's localPosition/Rotation must be zero.
    - <b>muscleProps</b> - the muscle properties that will be applied to the Prop Muscle when this prop is picked up.
    - <b>internalCollisionIgnores</b> - defines which muscles or muscle groups internal collisions will always be ignored with regardless of PuppetMaster.internalCollisions state.
    - <b>animatedTargetChildren</b> - list of animated bones parented to this muscle's Target, except for the bones that are targets or target children of any child muscles. This is used for stopping animation on those bones when the muscle has been disconnected using PuppetMaster.DisconnectMuscleRecursive(). For example if you disconnected the spine02 muscle, you would want to have spine03 and clavicles in this list to stop them from animating.
	- <b>forceLayers</b> - if true, this prop's layer will be forced to PuppetMaster layer and target's layer forced to PuppetMaster's Target Root's layer when the prop is picked up.
    - <b>mass</b> - mass of the prop while picked up. When dropped, mass of the original Rigidbody will be used.
    - <b>propType</b> - this has no other purpose but helping you distinguish props by PropRoot.currentProp.propType.
    - <b>pickedUpMaterial</b> - if assigned, sets prop colliders to this PhysicMaterial when picked up. If no materials assigned here, will maintain the original PhysicMaterial.
	- <b>additionalPinOffsetAdd</b> - adds this to Prop Muscle's 'Additional Pin Offset' when this prop is picked up.
	- <b>additionalPinWeight</b> - the pin weight of the additional pin. Increasing this weight will make the prop follow animation better, but will increase jitter when colliding with objects.
	- <b>additionalPinMass</b> - multiplies the mass of the additional pin by this value when this prop is picked up. The Rigidbody on this prop will be destroyed on pick-up and reattached on drop, so it's mass is not used while picked up.

\image html PuppetMasterProp.png

<br>
<b>Melee Props:</b>

Lengthy melee props are a huge challenge to the PuppetMaster. Swinging them rapidly requires a lot of muscle force and solver iterations to fight the inertia and keep the ragdoll chain intact.
The longer the chain of Joints linked together, the more inaccurate/unstable the simulation and unfortunatelly those melee props tend to be exactly at the ends of very long Joint chains (pelvis/spine/chest/upper arm/forearm/hand/sword).
Besides that, as the props are swinged, they have a lot of linear and angular velocity, a very thin collider and therefore can easily skip the victim when it's collider happens to be at the position between two fixed frames.
It usually takes quite a lot of tweaking and some tricks to get the melee props working right. PropMuscles have the "Additional Pin" functionality which can be used to add another pinning point to the prop, helping to better pin it to high angular velocity animation.

- make the collider thicker when hitting to decrease collider skipping.
- use Continuous/Continuous Dynamic collision detection modes to reduce skipping, this however has a big performance cost.
- reduce the fixed timestep.
- use the "Additional Pin" (Prop.cs).
- adjust the position of the "Additional Pin" and "Additional Pin Target" along the prop.
- reduce PuppetMaster.pinDistanceFalloff.
- increase PuppetMaster.muscleSpring and solverIterationCount.
- disable PuppetMaster.internalCollisions to prevent the big prop collider from colliding with the Puppet.
- get rid of the hand Muscle to make the joint chain shorter.

<b>PropMelee Component:</b> (this component will be made more generic in PuppetMaster 0.4).
- <b>capsuleCollider</b> - switches the collider to a Capsule when the prop is picked up. Capsules collide more smoothly and with less jitter.
- <b>boxCollider</b> - switches back to this BoxCollider when dropped.
- <b>actionColliderRadiusMlp</b> - multiplies the radius of the Capsule when PropMelee.StartAction(float duration) is called.
- <b>actionAdditionalPinWeight</b> - temporarily set (increase) the pin weight of the additional pin when PropMelee.StartAction(float duration) is called.
- <b>actionMassMlp</b> - temporarily increase the mass of the Rigidbody when PropMelee.StartAction(float duration) is called.
- <b>COMOffset</b> - offset to the default center of mass of the Rigidbody (might improve prop handling).

\image html PuppetMasterPropMelee.png

  */

/*! \page page7 Inverse Kinematics

PuppetMaster can be used together with Unity's own built-in Animator IK as well as Final IK. 
The former is limited to Humanoids only and can be used to alter the target pose programmatically before the PuppetMaster reads it for following.
Final IK opens up much more possibilities such as modifying the pose with full body IK or applying an IK pass on top of the physical simulation for cosmetic corrections or accurate aiming.

<b>Final IK:</b>

To use PuppetMaster with Final IK, import  both packages to the project, then import also "Plugins/RootMotion/PuppetMaster/_Integration/Final-IK.unitypackage".
The package comes with scenes and scripts demonstrating the application of IK both before and after PuppetMaster solves.

<b>IK Before Physics:</b>

We might be interested in using IK to change the Target pose for various reasons, for example keeping balance, aiming, grabbing, protecting from incoming collisions or even procedural locomotion.
When using Unity's built-in IK solution, it will be automatically applied on top of the animation in the Mecanim update cycle.
The components of Final IK will also update by default before PuppetMaster reads.
If you need them to be updated in a specific order, you can use the IKBeforePhysics.cs component 
or disable them and update their solvers manually using the PuppetMaster.OnRead delegate:

\code
		public PuppetMaster puppetMaster;
		public IK[] IKComponents;
		
		void Start() {
			// Register to get a call from PuppetMaster
			puppetMaster.OnRead += OnRead;

			// Take control of updating IK solvers
			foreach (IK ik in IKComponents) ik.enabled = false;
		}

		void OnRead() {
			if (!enabled) return;

			// Solve IK before PuppetMaster reads the pose of the character
			foreach (IK ik in IKComponents) ik.GetIKSolver().Update();
		}

		// Cleaning up the delegates
		void OnDestroy() {
			if (puppetMaster != null) {
				puppetMaster.OnRead -= OnRead;
			}
		}
\endcode

<b>IK After Physics:</b>

Each frame PuppetMaster is done solving and mapping the Target character to the ragdoll, we have a chance to make minor adjustments (that don't change the physics anymore) to the character pose.
This can be useful for example when making sure hands are perfectly fixed to some points or doing aiming correction with AimIK.
Unity's built-in IK can not be applied after PuppetMaster has mapped the Target character to the ragdoll pose. 
Final IK components can be updated whenever necessary and in this case we need to do it in the PuppetMaster.OnWrite delegate:

\code
		public PuppetMaster puppetMaster;
		public IK[] IKComponents;
		
		void Start() {
			// Register to get some calls from PuppetMaster
			puppetMaster.OnWrite += OnWrite;

			// Take control of updating IK solvers
			foreach (IK ik in IKComponents) ik.enabled = false;
		}

		// PuppetMaster calls this when it is done mapping the animated character to the ragdoll so if we can apply our kinematic adjustments to it now
		void OnWrite() {
			if (!enabled) return;

			// Solve IK after PuppetMaster writes the solved pose on the animated character.
			foreach (IK ik in IKComponents) ik.GetIKSolver().Update();
		}

		// Cleaning up the delegates
		void OnDestroy() {
			if (puppetMaster != null) {
				puppetMaster.OnWrite -= OnWrite;
			}
		}
\endcode

*/

/*! \page page8 Performance
 * @todo Description of optimization techniques, PuppetMasterSettings.cs.

<b>Tips for improving the performance</b>
<br>
	- Decrease PuppetMaster "Solver Iteration Count" (changes Rigidbody.solverIterationCount for all the muscles) to the minimum that you can work with. Less solver iterations makes the muscles weaker so you might have to increase PuppetMaster "Muscle Spring".
	- "Fix Target Transforms" should be switched off when your character is animated at all times.
	- "Visualize Target Pose" costs some performance in the Editor, but not in the built game.
	- Leaving "Update Joint Anchors" off improves performance with the cost of simulation accuracy.
	- "Angular Limits" and "Internal Collisions" reduce the workload on the physics engine when disabled.
	- When using BehaviourPuppet, increase PuppetMaster "Collision Threshold". It determines how strong the collisions must be in order to be processed by the BehaviourPuppet, however, increasing the value will make the puppet's behaviour jerkier on collision.
	- Reduce the number of muscles. Do you absolutely need 3 spine muscles or the feet or hand muscles?
	- Reducing "Default Contact Offset" in the Physics settings reduces expensive OnCollisionStay broadcasts when using BehaviourPuppet.
	- Increase the "Fixed Timestep" in Project Settings/Time to the maximum tolerable value.
	- Keep your puppets away from each other. Less collisions means much less work for both the Physics engine and the PuppetMaster.
	- Set PuppetMaster mode to "Disabled" for puppets when they don't need any physics simulation.
	- Set PuppetMaster mode to "Kinematic" for puppets that only need the colliders for collision detection or raycasting.
	- When you do not need to use any Puppet Behaviours nor get a call in case of collisions, comment out the OnCollisionEnter/Stay/Exit functions in MuscleCollisionBroadcaster.cs.
	- PuppetMaster can also run a flat hierarchy ragdoll. Just right-click on the PuppetMaster header and select "Flatten Muscle Hierarchy" from the context menu.
 * */

/*! \page page9 Behaviours

The Puppet Behaviours are classes that inherit from the abstract BehaviourBase.cs and that's main idea is to provide a pattern for developing functionalities that dynamically adjust muscle and pin weights, strength and other properties or make kinematic adjustments to the target pose.
The most important Puppet Behaviour is the BehaviourPuppet.cs that handles the pinning of Puppets to target animation, releasing those pins in case of collision and re-tightening them when getting up from the ground.
The Puppet Behaviours can be switched, for example when the BehaviourPuppet looses balance, it can theoretically switch to BehaviourCatchFall or BehaviourWindmill (both missing from the initial release, but will be developed in the future).
The Puppet Behaviours are designed so tha they would not contain a single external object reference. That means they can be simply duplicated and moved to another Puppet.

\image html Behaviours2.png

<b>Switching Behaviours:</b>

When working with multiple Behaviours, keep only the one that is supposed to run first enabled in the Editor. 
For example when you have the BehaviourPuppet and BehaviourFall, keep the former enabled and the latter disabled if you wish for the Puppet to start from normal animated state, not falling.

Switching between behaviours by code must be done by calling <b>BehaviourBase.Activate();</b> on the Behaviour you wish to switch to. All other Behaviours will be disabled.

<b>Events</b>

Behaviours trigger events on certain occasions (such as losing balance). They can be used to get a message through to your own scripts or switching behaviours.
	- <b>switchToBehaviour</b> - another Puppet Behaviour to switch to on this event. This must be the exact Type of the the Behaviour, careful with spelling.
	- <b>animations</b> - animations to cross-fade to on this event. This is separate from the UnityEvent below because UnityEvents can't handle calls with more than one parameter such as Animator.CrossFade.
	- <b>unityEvent</b> - the UnityEvent to invoke on this event.

\image html PuppetEvent.png

<b>Sub-Behaviours:</b>

Sub-behaviours are reusable self-contained chunks of functionaly that can be easily shared between multiple Puppet Behaviours.
For example SubBehaviourCOM is a module that automatically calculates and updates center of mass related information for the Puppet - data such as center or pressure, direction and angle of the COM vector and detect if the Puppet is grounded or not.
This prevents the necessity to duplicate code for all the Behaviours that need to make COM calculations.
To see how to use a sub-behaviour like that, take a look at the section below and the BehaviourTemplate.cs class.

<b>Creating Custom Behaviours:</b>

PuppetMaster has been built from start up with customization and extendibility in mind. 
To create reusable behaviours of your own, make a class that extends the BehaviourBase abstract class or just make a copy or BehaviourTemplate.cs 
and start adding functionality following the pattern at hand.

 */

/*! \page page10 BehaviourPuppet

The BehaviourPuppet handles pinning and unpinning puppets when they collide with objects or are hit via code, also automates getting up from an unbalanced state.

\image html Puppet.gif

<b>Getting Started</b>
	- Copy the entire gameobject of the BehaviourPuppet from the demo character in "Melee" scene to your own Puppet (parent to the Behaviours root).
	- Collision resistance depends on many things, also the mass of your Rigidbodies, if the Puppet is too easy or difficult to unbalance, tweak the "Collision Resistance" value first.

<b>Troubleshooting</b>
	- <b>The Puppet never falls over, has "snake feet"</b> - decrease "Collision Resistance".
	- <b>The Puppet loses balance on slightest contact</b> - increase "Collision Resistance" and/or "Regain Pin Speed". Increase "Knock Out Distance".
	- <b>The Puppet tries to get up, but repeatedly fails</b> - increase "Get Up Collision Resistance" and/or "Get Up Regain Pin Speed Mlp" and/or "Get Up Knock Out Distance".
	- <b>The Puppet's muscles are too stiff when unbalanced</b> - decrease "Unpinned Muscle Weight Mlp".
	- <b>The Puppet doesn't lose balance when hit hard to the legs</b> - find the group override for the "Hips" and "Leg" and the "Foot" group. Increasing "Unping Parents", "Unpin Children" and "Unpin Group" makes the collisions propagate more heavily to the other body parts. Also try decreasing "Knock Out Distance".

<b>Collision And Recovery</b>
	- <b>normalMode</b> - how does the puppet behave when currently not in contact with anything? Active mode keeps the PuppetMaster Active and mapped at all times. Unmapped blends out mapping to maintain 100% animation quality. Kenamatic keeps the PuppetMaster in Kinematic mode until there is a collision.
	- <b>mappingBlendSpeed</b> - the speed of blending in mapping in case of contact when in Unmapped normal mode.
	- <b>activateOnStaticCollisions</b> - if false, static colliders will not activate the puppet when they collide with the muscles. Note that the static colliders need to have a kinematic Rigidbody attached for this to work. Used only in Kinematic normal mode.
	- <b>activateOnImpulse</b> - minimum collision impulse for activating the puppet. Used only in Kinematic normal mode.
	- <b>groundLayers</b> - the layers that the character controller will be grounded to when unpinned or getting up.
	- <b>collisionLayers</b> - the layers that will unpin the Puppet on collision.
	- <b>collisionThreshold</b> - an optimization. The minimum square magnitude of the impulse that will be processed.
	- <b>collisionResistance</b> - smaller value means more unpinning from collisions so the characters get knocked out more easily. If using a curve, the value will be evaluated by each muscle's target velocity magnitude. This can be used to make collision resistance higher while the character moves or animates faster.
	- <b>collisionResistanceMultipliers</b> - used for multiplying the value of collision resistance based on the layer that collided with the Puppet.
	- <b>maxCollisions</b> - and optimization. The maximum number of collisions that will be processed per physics step. Helps to avoid peaks.
	- <b>regainPinSpeed</b> - how fast will the muscles of this group regain their pin weight?
	- <b>muscleRelativeToPinWeight</b> - Muscle weight multiplier relative to pin weight. It can be used to make muscles weaker/stronger when they are more/less unpinned while in the normal Puppet state.
	- <b>boostFalloff</b> - Boosting is a term used for making muscles temporarily immune to collisions and/or deal more damage to the muscles of other characters. That is done by increasing Muscle.State.immunity and Muscle.State.impulseMlp. For example when you set muscle.state.immunity to 1, boostFalloff will determine how fast this value will fall back to normal (0). Use BehaviourPuppet.BoostImmunity() and BehaviourPuppet.BoostImpulseMlp() for boosting from your own scripts. It is helpful for making the puppet stronger and deliever more punch while playing a melee hitting/kicking animation.

<b>Muscle Group Properties</b>
	- <b>defaults</b> - the default muscle properties. If there are no 'Group Overrides', this will be used for all muscles.
	<BR><b>unpinParents</b> - how much will collisions with muscles of this group unpin parent muscles?
	<BR><b>unpinChildren</b> - how much will collisions with muscles of this group unpin child muscles?
	<BR><b>unpinGroup</b> - how much will collisions with muscles of this group unpin muscles of the same group?
	<BR><b>minMappingWeight</b> - if 1, muscles of this group will always be mapped to the ragdoll.
	<BR><b>maxMappingWeight</b> - if 0, muscles of this group will not be mapped to the ragdoll pose even if they are unpinned.
	<BR><b>disableColliders</b> - if true, muscles of this group will have their colliders disabled while in puppet state (not unbalanced nor getting up).
	<BR><b>regainPinSpeed</b> - how fast will muscles of this group regain their pin weight (multiplier)?
	<BR><b>collisionResistance</b> - smaller value means more unpinning from collisions (multiplier).
	<BR><b>knockOutDistance</b> - if the distance from the muscle to it's target is larger than this value, the character will be knocked out.
	<BR><b>puppetMaterial</b> - the PhysicsMaterial applied to the muscles while the character is in Puppet or GetUp state. Using a lower friction material reduces the risk of muscles getting stuck and pulled out of their joints.
	<BR><b>unpinnedMaterial</b> - the PhysicsMaterial applied to the muscles while the character is in Unpinned state.

	- <b>groupOverrides</b> - overriding default muscle properties for some muscle groups (for example making the feet stiffer or the hands looser).

<b>Losing Balance</b>
	- <b>knockOutDistance</b> - if the distance from the muscle to it's target is larger than this value, the character will be knocked out.
	- <b>unpinnedMuscleWeightMlp</b> - smaller value makes the muscles weaker when the puppet is knocked out.
	- <b>dropProps</b> - if true, all muscles of the 'Prop' group will be detached from the puppet when it loses balance.


<b>Getting Up</b>
	- <b>canGetUp</b> - if true, GetUp state will be triggerred automatically after 'Get Up Delay' and when the velocity of the hip muscle is less than 'Max Get Up Velocity'.
	- <b>getUpDelay</b> - minimum delay for getting up after loosing balance. After that time has passed, will wait for the velocity of the hip muscle to come down below 'Max Get Up Velocity' and then switch to the GetUp state.
	- <b>blendToAnimationTime</b> - the duration of blending the animation target from the ragdoll pose to the getting up animation once the GetUp state has been triggered.
	- <b>maxGetUpVelocity</b> - will not get up before the velocity of the hip muscle has come down to this value.
	- <b>minGetUpDuration</b> - will not get up before this amount of time has passed since loosing balance.
	- <b>getUpCollisionResistanceMlp</b> - collision resistance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.
	- <b>getUpRegainPinSpeedMlp</b> - regain pin weight speed multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.
	- <b>getUpKnockOutDistanceMlp</b> - knock out distance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.
	- <b>getUpOffsetProne</b> - offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a prone pose. Tweak this value if your character slides a bit when starting to get up.
	- <b>getUpOffsetSupine</b> - offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a supine pose. Tweak this value if your character slides a bit when starting to get up.

<b>Events</b>
	- <b>onGetUpProne</b> - called when the character starts getting up from a prone pose (facing down).
	- <b>onGetUpSupine</b> - called when the character starts getting up from a supine pose (facing up).
	- <b>onLoseBalance</b> - called when the character is knocked out (loses balance). Doesn't matter from which state.
	- <b>onLoseBalanceFromPuppet</b> - called when the character is knocked out (loses balance) only from the normal Puppet state.
	- <b>onLoseBalanceFromGetUp</b> - called when the character is knocked out (loses balance) only from the GetUp state.
	- <b>onRegainBalance</b> - called when the character has fully recovered and switched to the Puppet state.

\image html BehaviourPuppet.png

 * */

/*! \page page11 BehaviourFall

BehaviourFall simply blends between two animation clips in a blend tree depending on the height of the ragdoll from the ground. 
As seen from the gif below, when the ragdoll is high above the ground surface, a writhering animation is played, but when it falls down, a protective pose is blended in.

\image html Falling.gif

BehaviourFall requires for the Animator of the character to have a blend tree set up like in the image below. 
You can copy the blend tree from the AnimatorController in the "Falling" demo scene.

\image html BehaviourFallAnimator.png

<b>Component Variables</b>
	- <b>stateName</b> - Animation State to crossfade to when this behaviour is activated.
	- <b>transitionDuration</b> - the duration of crossfading to stateName. Value is in seconds.
	- <b>layer</b> - layer index containing the destination state. If no layer is specified or layer is -1, the first state that is found with the given name or hash will be played.
	- <b>fixedTime</b> - start time of the current destination state. Value is in seconds. If no explicit fixedTime is specified or fixedTime value is float.NegativeInfinity, the state will either be played from the start if it's not already playing, or will continue playing from its current time and no transition will happen.
	- <b>raycastLayers</b> - the layers that will be raycasted against to find colliding objects.
	- <b>blendParameter</b> - the parameter in the Animator that blends between catch fall and writhe animations.
	- <b>writheHeight</b> - the height of the pelvis from the ground at which will blend to writhe animation.
	- <b>writheYVelocity</b> - the vertical velocity of the pelvis at which will blend to writhe animation.
	- <b>blendSpeed</b> - the speed of blendig between the two falling animations.
	- <b>canEnd</b> - if false, this behaviour will never end.
	- <b>minTime</b> - the minimum time since this behaviour activated before it can end.
	- <b>maxEndVelocity</b> - if the velocity of the pelvis falls below this value, can end the behaviour.
	- <b>onEnd</b> - event triggered when all end conditions are met.
	
\image html BehaviourFall.png

 * */

/*! \page page12 Baker
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

\htmlonly <iframe width="854" height="480" src="https://www.youtube.com/embed/uakZqX1hju0" frameborder="0" allowfullscreen></iframe>\endhtmlonly
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