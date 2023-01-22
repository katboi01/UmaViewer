using System;
using System.Runtime.InteropServices;
using UnityEngine;

public struct CySpringNative {
  private const string LibraryName = "cyspringandroid";

  public static bool isNative = true;

  public NativeClothWorking _clothWorking;

  [DllImport("cyspringandroid")]
  private static extern void NativeClothUpdate(IntPtr cond, int nCond, IntPtr collisions, float stiffnessForceRate, float dragForceRate, float gravityRate, bool bCollisionSwitch);

  //private unsafe static void UpdateNativeClothInternal(NativeClothWorking[] clothWorking, int nClothWorking, NativeClothCollision[] collisions, float stiffnessForceRate, float dragForceRate, float gravityRate, bool bCollisionSwitch)
  //{
  /*
		//IL_001f: Incompatible stack types: I vs Ref
		fixed (NativeClothWorking* value = &clothWorking[0])
		{
			fixed (NativeClothCollision* value2 = &((collisions != null && collisions.Length != 0) ? ref collisions[0] : ref *(NativeClothCollision*)null))
			{
				NativeClothUpdate((IntPtr)(void*)value, nClothWorking, (IntPtr)(void*)value2, stiffnessForceRate, dragForceRate, gravityRate, bCollisionSwitch);
			}
		}
        */
  //}

  public static void UpdateNativeCloth(NativeClothWorking[] clothWorkings, int clothWorkingCount, NativeClothCollision[] collisions, float stiffnessForceRate, float dragForceRate, float gravityRate, bool bCollisionSwitch) {
    if (!clothWorkings[0]._isSkip) {
      /*
			if (isNative)
			{
				UpdateNativeClothInternal(clothWorkings, clothWorkingCount, collisions, stiffnessForceRate, dragForceRate, gravityRate, bCollisionSwitch);
			}
			else
			{
            */
      for (int i = 1; i < clothWorkingCount; i++) {
        clothWorkings[i]._oldPosition = clothWorkings[i - 1]._currentPosition;
        clothWorkings[i]._oldRotation = clothWorkings[i]._initLocalRotation * clothWorkings[i - 1]._finalRotation;
      }
      for (int j = 0; j < clothWorkingCount; j++) {
        UpdateNativeClothInternalMono(ref clothWorkings[j], collisions, stiffnessForceRate, dragForceRate, gravityRate, bCollisionSwitch);
      }
      //}
    }
  }

  private static void UpdateNativeClothInternalMono(ref NativeClothWorking clothWorking, NativeClothCollision[] collisions, float stiffnessForceRate, float dragForceRate, float gravityRate, bool bCollisionSwitch) {
    clothWorking._diff = clothWorking._prevPosition - clothWorking._currentPosition;
    clothWorking._prevPosition = clothWorking._currentPosition;
    clothWorking._aimVector = clothWorking._oldRotation * clothWorking._boneAxis;
    clothWorking._force = clothWorking._aimVector * clothWorking._stiffnessForce * stiffnessForceRate;
    clothWorking._force += clothWorking._diff * clothWorking._dragForce * dragForceRate;
    clothWorking._force += clothWorking._springForce;
    clothWorking._force.y -= clothWorking._gravity * gravityRate;
    clothWorking._currentPosition = clothWorking._currentPosition - clothWorking._diff + clothWorking._force;
    clothWorking._diff = (clothWorking._currentPosition - clothWorking._oldPosition).normalized;
    clothWorking._currentPosition = clothWorking._diff * clothWorking._initBoneDistance + clothWorking._oldPosition;
    if (clothWorking._existCollision && clothWorking._activeCollision > 0 && bCollisionSwitch) {
      int[] array = new int[16] {
        clothWorking._cIndex0,
        clothWorking._cIndex1,
        clothWorking._cIndex2,
        clothWorking._cIndex3,
        clothWorking._cIndex4,
        clothWorking._cIndex5,
        clothWorking._cIndex6,
        clothWorking._cIndex7,
        clothWorking._cIndex8,
        clothWorking._cIndex9,
        clothWorking._cIndex10,
        clothWorking._cIndex11,
        clothWorking._cIndex12,
        clothWorking._cIndex13,
        clothWorking._cIndex14,
        clothWorking._cIndex15
      };

      for (int i = 0; i < clothWorking._activeCollision; i++) {
        int num = array[i];
        NativeClothCollision nativeClothCollision = collisions[num];
        switch (nativeClothCollision._type) {
          case 0:
            if (Vector3.Distance(clothWorking._currentPosition, nativeClothCollision._position) <= clothWorking._collisionRadius + nativeClothCollision._radius) {
              Vector3 normalized3 = (clothWorking._currentPosition - nativeClothCollision._position).normalized;
              clothWorking._currentPosition = nativeClothCollision._position + normalized3 * (clothWorking._collisionRadius + nativeClothCollision._radius);
              clothWorking._currentPosition = (clothWorking._currentPosition - clothWorking._oldPosition).normalized * clothWorking._initBoneDistance + clothWorking._oldPosition;
            }
            break;
          case 2:
            {
              Vector3 a = nativeClothCollision._position2 - nativeClothCollision._position;
              float magnitude = a.magnitude;
              a /= magnitude;
              Vector3 vector = clothWorking._currentPosition - nativeClothCollision._position;
              float num2 = Vector3.Dot(vector, a);
              bool flag = false;
              if (0f <= num2 && num2 < magnitude) {
                Vector3 b = a * num2;
                Vector3 a2 = vector - b;
                float magnitude2 = a2.magnitude;
                if (magnitude2 < clothWorking._collisionRadius + nativeClothCollision._radius) {
                  clothWorking._currentPosition = nativeClothCollision._position + b + a2 * ((clothWorking._collisionRadius + nativeClothCollision._radius) / magnitude2);
                  clothWorking._currentPosition = (clothWorking._currentPosition - clothWorking._oldPosition).normalized * clothWorking._initBoneDistance + clothWorking._oldPosition;
                  flag = true;
                }
              }
              if (!flag) {
                if (Vector3.Distance(clothWorking._currentPosition, nativeClothCollision._position) <= clothWorking._collisionRadius + nativeClothCollision._radius) {
                  Vector3 normalized = (clothWorking._currentPosition - nativeClothCollision._position).normalized;
                  clothWorking._currentPosition = nativeClothCollision._position + normalized * (clothWorking._collisionRadius + nativeClothCollision._radius);
                  clothWorking._currentPosition = (clothWorking._currentPosition - clothWorking._oldPosition).normalized * clothWorking._initBoneDistance + clothWorking._oldPosition;
                } else if (Vector3.Distance(clothWorking._currentPosition, nativeClothCollision._position2) <= clothWorking._collisionRadius + nativeClothCollision._radius) {
                  Vector3 normalized2 = (clothWorking._currentPosition - nativeClothCollision._position2).normalized;
                  clothWorking._currentPosition = nativeClothCollision._position2 + normalized2 * (clothWorking._collisionRadius + nativeClothCollision._radius);
                  clothWorking._currentPosition = (clothWorking._currentPosition - clothWorking._oldPosition).normalized * clothWorking._initBoneDistance + clothWorking._oldPosition;
                }
              }
              break;
            }
        }
      }
    }
    clothWorking._finalRotation = Quaternion.FromToRotation(clothWorking._aimVector, clothWorking._diff) * clothWorking._oldRotation;
  }
}