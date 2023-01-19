using System;
using System.Runtime.InteropServices;
using UnityEngine;

public struct CySpringNative
{
    private const string LibraryName = "CySpringPlugin";

    public static bool isNative = true;

    public NativeClothWorking _clothWorking;

    [DllImport("CySpringPlugin")]
    private static extern void NativeClothUpdate(IntPtr cond, int nCond, IntPtr collisions, IntPtr clothParams, IntPtr parentWorldPosition, IntPtr parentWorldRotation);
    /*
    private unsafe static void UpdateNativeClothInternal(NativeClothWorking[] clothWorking, NativeClothCollision[] collisions, NativeClothParameter[] clothParam, Vector3[] parentWorldPosition, Quaternion[] parentWorldRotation)
    {
        int nCond = clothWorking.Length;
        fixed (NativeClothWorking* ptr = &clothWorking[0])
        {
            fixed (NativeClothCollision* ptr2 = collisions)
            {
                fixed (NativeClothParameter* ptr3 = &clothParam[0])
                {
                    fixed (Vector3* ptr4 = parentWorldPosition)
                    {
                        fixed (Quaternion* ptr5 = parentWorldRotation)
                        {
                            NativeClothUpdate((IntPtr)ptr, nCond, (IntPtr)ptr2, (IntPtr)ptr3, (IntPtr)ptr4, (IntPtr)ptr5);
                        }
                    }
                }
            }
        }
    }
    */
    public static void UpdateNativeCloth(NativeClothWorking[] clothWorkings, NativeClothCollision[] collisions, NativeClothParameter[] clothParam, Vector3[] parentCollisionWorldPosition, Quaternion[] _collisionParentWorldRotation)
    {
        if (clothWorkings[0]._isSkip)
        {
            return;
        }
        int num = clothWorkings.Length;

        /* Native(dll)Ç…ÇÊÇÈï®óùââéZÇÕñ≥å¯âª
        if (isNative)
        {
            UpdateNativeClothInternal(clothWorkings, collisions, clothParam, parentCollisionWorldPosition, _collisionParentWorldRotation);
            return;
        }
        */

        for (int i = 1; i < num; i++)
        {
            clothWorkings[i]._oldPosition = clothWorkings[i - 1]._currentPosition;
            clothWorkings[i]._oldRotation = clothWorkings[i]._initLocalRotation * clothWorkings[i - 1]._finalRotation;
        }
        for (int j = 0; j < num; j++)
        {
            UpdateNativeClothInternalMono(ref clothWorkings[j], collisions, ref clothParam[0], parentCollisionWorldPosition, _collisionParentWorldRotation);
        }
    }


    private static void UpdateNativeClothInternalMono(ref NativeClothWorking clothWorking, NativeClothCollision[] collisions, ref NativeClothParameter clothParam, Vector3[] parentWorldPosition, Quaternion[] parentWorldRotation)
    {
        clothWorking._diff = clothWorking._prevPosition - clothWorking._currentPosition;
        clothWorking._prevPosition = clothWorking._currentPosition;
        clothWorking._aimVector = clothWorking._oldRotation * clothWorking._boneAxis;
        clothWorking._force = clothWorking._aimVector * clothWorking._stiffnessForce * clothParam._stiffnessForceRate;
        clothWorking._force += clothWorking._diff * clothWorking._dragForce * clothParam._dragForceRate;
        clothWorking._force += clothWorking._springForce;
        if ((clothWorking._capability & CySpring.eCapability.GravityControl) != 0)
        {
            bool flag = false;
            Vector3 vector = Vector3.down;
            switch (clothWorking._capsGroupIndex)
            {
                case 0:
                    flag = (clothParam._capability0 & CySpring.eCapability.GravityControl) != 0;
                    vector = clothParam._gravityDirection0;
                    break;
                case 1:
                    flag = (clothParam._capability1 & CySpring.eCapability.GravityControl) != 0;
                    vector = clothParam._gravityDirection1;
                    break;
                case 2:
                    flag = (clothParam._capability2 & CySpring.eCapability.GravityControl) != 0;
                    vector = clothParam._gravityDirection2;
                    break;
                case 3:
                    flag = (clothParam._capability3 & CySpring.eCapability.GravityControl) != 0;
                    vector = clothParam._gravityDirection3;
                    break;
                case 4:
                    flag = (clothParam._capability4 & CySpring.eCapability.GravityControl) != 0;
                    vector = clothParam._gravityDirection4;
                    break;
            }
            if (flag)
            {
                float num = clothWorking._gravity * clothParam._gravityRate;
                vector = clothWorking._oldRotation * vector * num;
                clothWorking._force -= vector;
            }
        }
        else
        {
            clothWorking._force.y -= clothWorking._gravity * clothParam._gravityRate;
        }
        if (((uint)clothWorking._wind & (true ? 1u : 0u)) != 0)
        {
            clothWorking._force = Vector3.Scale(clothWorking._force, clothParam._forceScale);
            clothWorking._force += clothWorking._windPower;
        }
        clothWorking._currentPosition = clothWorking._currentPosition - clothWorking._diff + clothWorking._force;
        clothWorking._diff = (clothWorking._currentPosition - clothWorking._oldPosition).normalized;
        clothWorking._currentPosition = clothWorking._diff * clothWorking._initBoneDistance + clothWorking._oldPosition;
        if (clothWorking._existCollision && clothWorking._activeCollision > 0 && clothParam._collisionSwitch)
        {
            int[] array = new int[20]
            {
                clothWorking._cIndex0, clothWorking._cIndex1, clothWorking._cIndex2, clothWorking._cIndex3, clothWorking._cIndex4, clothWorking._cIndex5, clothWorking._cIndex6, clothWorking._cIndex7, clothWorking._cIndex8, clothWorking._cIndex9,
                clothWorking._cIndex10, clothWorking._cIndex11, clothWorking._cIndex12, clothWorking._cIndex13, clothWorking._cIndex14, clothWorking._cIndex15, clothWorking._cIndex16, clothWorking._cIndex17, clothWorking._cIndex18, clothWorking._cIndex19
            };
            for (int i = 0; i < clothWorking._activeCollision; i++)
            {
                int num2 = array[i];
                NativeClothCollision nativeClothCollision = collisions[num2];
                int parentIndex = nativeClothCollision._parentIndex;
                Vector3 vector2;
                Vector3 vector3;
                switch (nativeClothCollision._type)
                {
                    default:
                        vector2 = Vector3.zero;
                        vector3 = clothWorking._currentPosition;
                        break;
                    case 0:
                        {
                            Vector3 vector11;
                            Vector3 vector4;
                            if (nativeClothCollision._purpose == 2)
                            {
                                vector11 = nativeClothCollision._position;
                                vector3 = clothWorking._currentPosition;
                                vector4 = clothWorking._oldPosition;
                                vector2 = Vector3.zero;
                            }
                            else
                            {
                                vector2 = parentWorldPosition[parentIndex];
                                vector3 = clothWorking._currentPosition - vector2;
                                vector4 = clothWorking._oldPosition - vector2;
                                vector11 = parentWorldRotation[parentIndex] * nativeClothCollision._position;
                            }
                            if (Vector3.Distance(vector3, vector11) <= clothWorking._collisionRadius + nativeClothCollision._radius)
                            {
                                Vector3 normalized3 = (vector3 - vector11).normalized;
                                vector3 = vector11 + normalized3 * (clothWorking._collisionRadius + nativeClothCollision._radius);
                                vector3 = (vector3 - vector4).normalized * clothWorking._initBoneDistance + vector4;
                            }
                            break;
                        }
                    case 2:
                        {
                            vector2 = parentWorldPosition[parentIndex];
                            vector3 = clothWorking._currentPosition - vector2;
                            Vector3 vector4 = clothWorking._oldPosition - vector2;
                            Vector3 vector5 = parentWorldRotation[parentIndex] * nativeClothCollision._position;
                            Vector3 vector6 = parentWorldRotation[parentIndex] * nativeClothCollision._position2;
                            Vector3 vector7 = vector6 - vector5;
                            float magnitude = vector7.magnitude;
                            vector7 /= magnitude;
                            Vector3 vector8 = vector3 - vector5;
                            float num3 = Vector3.Dot(vector8, vector7);
                            bool flag2 = false;
                            if (0f <= num3 && num3 < magnitude)
                            {
                                Vector3 vector9 = vector7 * num3;
                                Vector3 vector10 = vector8 - vector9;
                                float magnitude2 = vector10.magnitude;
                                if (magnitude2 < clothWorking._collisionRadius + nativeClothCollision._radius)
                                {
                                    vector3 = vector5 + vector9 + vector10 * ((clothWorking._collisionRadius + nativeClothCollision._radius) / magnitude2);
                                    vector3 = (vector3 - vector4).normalized * clothWorking._initBoneDistance + vector4;
                                    flag2 = true;
                                }
                            }
                            if (!flag2)
                            {
                                if (Vector3.Distance(vector3, vector5) <= clothWorking._collisionRadius + nativeClothCollision._radius)
                                {
                                    Vector3 normalized = (vector3 - vector5).normalized;
                                    vector3 = vector5 + normalized * (clothWorking._collisionRadius + nativeClothCollision._radius);
                                    vector3 = (vector3 - vector4).normalized * clothWorking._initBoneDistance + vector4;
                                }
                                else if (Vector3.Distance(vector3, vector6) <= clothWorking._collisionRadius + nativeClothCollision._radius)
                                {
                                    Vector3 normalized2 = (vector3 - vector6).normalized;
                                    vector3 = vector6 + normalized2 * (clothWorking._collisionRadius + nativeClothCollision._radius);
                                    vector3 = (vector3 - vector4).normalized * clothWorking._initBoneDistance + vector4;
                                }
                            }
                            break;
                        }
                }
                clothWorking._currentPosition = vector3 + vector2;
            }
        }
        clothWorking._finalRotation = Quaternion.FromToRotation(clothWorking._aimVector, clothWorking._diff) * clothWorking._oldRotation;
    }
}