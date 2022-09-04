Dynamic Bone apply physics to character's bones or joints.
With simple setup, your character's hair, cloth, breasts or any part will move realistically.

Open Assets/DynamicBone/Demo/Demo1 to see how it works.
If you have any questions or suggestions, please contact willhongcom@gmail.com.


-------------------------------------------------------------------------
Basic setup:

1. Prepare a properly setup character, both Mecanim and legacy rigs are supported.
2. Select the game object you want to apply Dynamic Bone.
3. In the component menu, select Dynamic Bone -> Dynamic Bone.
4. In the inspector, select root object.
5. Adjust dynamic bone parameters (see detail descriptions in the following section).


You can add collider objects if required:

1. Select game object the collider will attached.
2. In the component menu, select Dynamic Bone -> Dynamic Bone Collider.
3. Adjust position and size of the collider.
4. In Dynamic Bone component, increase size of colliders and append corresponding object.


-------------------------------------------------------------------------
Dynamic Bone component description:

- Root
  The root of the transform hierarchy to apply physics.

- Update Rate
  Internal physics simulation rate, measures in frames per seconds.

- Update Mode
  Normal: Update physics in fixed timestamp as specified rate.
  AnimatePhysics: Updates during the physic loop in order to synchronized with the physics engine.
  UnscaledTime: Updates independently of Time.timeScale.
  Default: Update physics every frame instead of specified rate, recommended.

- Damping
  How much the bones slowed down.

- Elasticity
  How much the force applied to return each bone to original orientation.

- Stiffness
  How much bone's original orientation are preserved.

- Inert
  How much character's position change is ignored in physics simulation.

- Friction
  How much the bones slowed down when collide.

- Radius
  Each bone can be a sphere to collide with colliders. Radius describe sphere's size.

- Damping Distrib, Elasticity Distrib, Stiffness Distrib, Inert Distrib, Radius Distrib
  How parameters change over hierarchy chain. Curve values are multiplied to corresponding parameters. 

- End Length
  If End Length is not zero, an extra bone is generated at the end of transform hierarchy, 
  length is multiplied by last two bone's distance.

- End Offset
  If End Offset is not zero, an extra bone is generated at the end of transform hierarchy, 
  offset is in character's local space.

- Gravity
  The force apply to bones, in world space. Partial force apply to character's initial pose is cancelled out.

- Force
  The force apply to bones, in world space.

- Colliders
  Collider objects interact with the bones.

- Exclusions
  Bones exclude from physics simulation.
     
- Freeze Axis
  Constrain bones to move on specified plane.

- Distant Disable, Reference Object, Distance To Object
  Disable physics simulation automatically if character is far from camera or player.
  If there is no reference object, default main camera is used.


Dynamic Bone Collider component description:

- Center
  The center of the sphere or capsule, in the object's local space.

- Radius
  The radius of the sphere or capsule, will be scaled by the transform's scale.

- Height
  The height of the capsule, including two half-spheres, will be scaled by the transform's scale.

- Direction
  The axis of the capsule's height.

- Bound
  Constrain bones to outside bound or inside bound.

-------------------------------------------------------------------------
Dynamic Bone script reference:

- public void SetWeight(float w);
  Control how physics blend with existing animation.

- public void UpdateParameters();
  Update parameters at runtime, call this funtion after modifing parameters.

-------------------------------------------------------------------------
Version History

1.0.1 Initial release.
1.0.2 Improve inspector UI.
1.0.3 Fix inert unstable when enabled / disabled.
1.1.0 Use curve to setup parameters over hierarchy chain.
      Collider can configured to constrain bones inside bound.
1.1.1 Add exclusion setting.
1.1.2 Deal with negative scale problem.
1.1.3 Fix bug with bones contain scale.
1.1.4 Add freeze axis.
      Fix bug when added via script.
1.1.5 Add distant disable.
      Reduce GC alloc.
1.1.6 Fix capsule collider bug.
1.1.7 Unity 5 support.
1.1.8 Fix problems caused by negative scale.
1.1.9 Improve detecting negative scale.
      Fix bug if collider is set as inside.
      Add UpdateMode setting.
1.1.10 Fix problems caused by negative scale after Unity 5.4.
1.2.0 Add tool tips.
      Add plane collider.
      Add function to update parameters at runtime.
1.2.1 Add friction parameter.
      Update UNITY_5 to compatible with newer version.
1.2.2 Add "Default" update mode, fix some jitter issue.
