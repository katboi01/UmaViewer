using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Gallop
{
    public struct CySpringNative
    {
#if (UNITY_IOS || UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
        // WebGL never calls these (isNative is false there; the managed CySpringSolver runs),
        // but the P/Invoke symbols must still resolve at link -- Assets/Plugins/WebGL/CySpring.jslib
        // provides no-op stubs.
        private const string DLL_NAME = "__Internal";
#else
        private const string DLL_NAME = "CySpringPlugin";
#endif

        // When false, the managed CySpringSolver runs instead of the native plugin.
        // WebGL has no native plugin, so default to managed there.
        public static bool isNative =
#if UNITY_WEBGL && !UNITY_EDITOR
            false;
#else
            true;
#endif
        public NativeClothWorking _clothWorking;
        public static float SpringRate = 1.0f;

        // Probe the native plugin once at startup: a zero-work call (nCond = 0) forces the DLL
        // to load without touching memory. If it can't load (missing/wrong-arch, e.g. no plugin
        // outside Windows), fall back to the managed CySpringSolver for the rest of the session.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ProbeNativePlugin()
        {
            if (!isNative) return; // already managed (WebGL, or set by the caller)
            // One real, skipped bone. The plugin reads bone 0 and its root parent before it
            // looks at the count (0x18000918a, 0x1800091ac), so null pointers are a fault, not
            // a no-op.
            var cloth = new NativeClothWorking[1];
            cloth[0].IsSkip = 1;
            cloth[0].InitLocalRotation = cloth[0].ParentRotation = cloth[0].AnimationRotation = cloth[0].FinalRotation = Quaternion.identity;
            var parents = new NativeRootParentWork[1];
            parents[0].WorldRotation = Quaternion.identity;
            var colliders = new NativeClothCollision[1];
            PinnedArray<NativeClothWorking> clothPin = null;
            PinnedArray<NativeClothCollision> collPin = null;
            PinnedArray<NativeRootParentWork> parentPin = null;
            try
            {
                clothPin = new PinnedArray<NativeClothWorking>(cloth);
                collPin = new PinnedArray<NativeClothCollision>(colliders);
                parentPin = new PinnedArray<NativeRootParentWork>(parents);
                NativeClothUpdate(clothPin.Ptr, 1, collPin.Ptr, parentPin.Ptr,
                    0f, 0f, 0f, 0f, 0f, 0f, 0f, false, 1f, false, 1f, 1f, 1f);
            }
            catch (Exception e)
            {
                isNative = false;
                Debug.LogWarning("[CySpring] native plugin not loadable (" + e.GetType().Name + "); using the managed C# solver.");
            }
            finally
            {
                parentPin?.Dispose(); collPin?.Dispose(); clothPin?.Dispose();
            }
        }


        public static bool UseNativePlugin
        {
            get { return isNative; }
            set { isNative = value; }
        }

        public static int TargetFrameRate
        {
            get
            {
                if (Config.Instance != null)
                    return Config.Instance.GetTargetFrameRate();

                return Application.targetFrameRate == 30 ? 30 : 60;
            }
        }

        public static bool Is60FpsPhysics
        {
            get { return TargetFrameRate == 60; }
        }

        [DllImport(DLL_NAME, EntryPoint = "NativeClothUpdate", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeClothUpdate(
            IntPtr cond,
            int nCond,
            IntPtr collisions,
            IntPtr pRootParentWork,
            float stiffnessForceRate,
            float dragForceRate,
            float gravityRate,
            float windX,
            float windY,
            float windZ,
            float windStrength,
            bool bCollisionSwitch,
            float timescale,
            bool is60FPS,
            float moveRate,
            float addMoveRate,
            float springRate);
            

        [DllImport(DLL_NAME, EntryPoint = "NativeClothSkirtUpdate", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeClothSkirtUpdate(
            IntPtr cond,
            int nCond,
            IntPtr collisions,
            IntPtr pWorking,
            IntPtr pArg,
            IntPtr pRootParentWork,
            float stiffnessForceRate,
            float dragForceRate,
            float gravityRate,
            float windX,
            float windY,
            float windZ,
            float windStrength,
            bool bCollisionSwitch,
            float timescale,
            bool is60FPS,
            float moveRate,
            float addMoveRate,
            float springRate);

        [DllImport(DLL_NAME, EntryPoint = "NativeSkirtUpdate", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeSkirtUpdate(
            IntPtr pWorking,
            IntPtr pArg);

        public static void UpdateNativeCloth(
            NativeClothWorking[] clothWorkingArray,
            int clothWorkingCount,
            NativeClothCollision[] collisionArray,
            int linkSkirtIndex,
            SkirtController skirtCtrl,
            NativeRootParentWork[] nativeRootParentArray,
            float stiffnessForceRate,
            float dragForceRate,
            float gravityRate,
            float windX,
            float windY,
            float windZ,
            float windStrength,
            bool bCollisionSwitch,
            float timescale = 1.0f,
            bool is60FPS = false,
            float moveRate = 1.0f,
            float addMoveRate = 1.0f,
            float springRate = 1.0f)
        {
            if (clothWorkingArray == null || clothWorkingArray.Length == 0)
                return;

            if (clothWorkingCount <= 0)
                return;

            if (clothWorkingCount > clothWorkingArray.Length)
                clothWorkingCount = clothWorkingArray.Length;

            if (linkSkirtIndex >= 0 && skirtCtrl != null && skirtCtrl.IsEnableSkirt)
            {
                NativeSkirtWorking[] skirtWorkingArray = skirtCtrl.NativeWorkingArray;

                if (skirtWorkingArray != null && (uint)linkSkirtIndex < (uint)skirtWorkingArray.Length)
                {
                    NativeSkirtArg skirtArg = skirtCtrl.NativeArg;

                    UpdateNativeClothSkirtInternal(
                        clothWorkingArray,
                        clothWorkingCount,
                        collisionArray,
                        skirtWorkingArray,
                        linkSkirtIndex,
                        ref skirtArg,
                        nativeRootParentArray,
                        stiffnessForceRate,
                        dragForceRate,
                        gravityRate,
                        windX,
                        windY,
                        windZ,
                        windStrength,
                        bCollisionSwitch,
                        timescale,
                        is60FPS,
                        moveRate,
                        addMoveRate,
                        springRate);

                    skirtCtrl.NativeArg = skirtArg;
                    return;
                }
            }

            UpdateNativeClothInternal(
                clothWorkingArray,
                clothWorkingCount,
                collisionArray,
                nativeRootParentArray,
                stiffnessForceRate,
                dragForceRate,
                gravityRate,
                windX,
                windY,
                windZ,
                windStrength,
                bCollisionSwitch,
                timescale,
                is60FPS,
                moveRate,
                addMoveRate,
                springRate);
        }

        private static void UpdateNativeClothInternal(
            NativeClothWorking[] clothWorkingArray,
            int nClothWorking,
            NativeClothCollision[] collisionArray,
            NativeRootParentWork[] rootParentWorkArray,
            float stiffnessForceRate,
            float dragForceRate,
            float gravityRate,
            float windX,
            float windY,
            float windZ,
            float windStrength,
            bool bCollisionSwitch,
            float timescale,
            bool is60FPS,
            float moveRate,
            float addMoveRate,
            float springRate)
        {
            if (!isNative)
            {
                var tracePre = CySpringTrace.Armed ? CySpringDiff.Snapshot(clothWorkingArray) : null;
                CySpringSolver.NativeClothUpdate(clothWorkingArray, nClothWorking, collisionArray,
                    rootParentWorkArray, stiffnessForceRate, dragForceRate, gravityRate,
                    windX, windY, windZ, windStrength, bCollisionSwitch, timescale, is60FPS,
                    moveRate, addMoveRate, springRate);
                CySpringTrace.Record(tracePre, clothWorkingArray, nClothWorking);
                return;
            }

            // Differential: run the managed solver on a copy of the pre-step state, let the plugin
            // advance the real state, compare. The simulation continues on the plugin's result.
            NativeClothCollision[] savedRadii = CySpringDiff.ScaleColliders(collisionArray);
            int[] savedChara = CySpringDiff.ScaleBones(clothWorkingArray, nClothWorking);
            NativeClothWorking[] nativeTracePre = CySpringTrace.Armed ? CySpringDiff.Snapshot(clothWorkingArray) : null;
            NativeClothWorking[] managedOut = null, preState = null;
            if (CySpringDiff.Armed)
            {
                preState = CySpringDiff.Snapshot(clothWorkingArray);
                managedOut = CySpringDiff.Snapshot(clothWorkingArray);
                CySpringSolver.NativeClothUpdate(managedOut, nClothWorking, collisionArray,
                    rootParentWorkArray, stiffnessForceRate, dragForceRate, gravityRate,
                    windX, windY, windZ, windStrength, bCollisionSwitch, timescale, is60FPS,
                    moveRate, addMoveRate, springRate);
            }

            PinnedArray<NativeClothWorking> clothPin = null;
            PinnedArray<NativeClothCollision> collisionPin = null;
            PinnedArray<NativeRootParentWork> parentPin = null;

            try
            {
                clothPin = new PinnedArray<NativeClothWorking>(clothWorkingArray);
                collisionPin = new PinnedArray<NativeClothCollision>(collisionArray);
                parentPin = new PinnedArray<NativeRootParentWork>(rootParentWorkArray);

                if (clothPin.Ptr == IntPtr.Zero)
                    return;

                NativeClothUpdate(
                    clothPin.Ptr,
                    nClothWorking,
                    collisionPin.Ptr,
                    parentPin.Ptr,
                    stiffnessForceRate,
                    dragForceRate,
                    gravityRate,
                    windX,
                    windY,
                    windZ,
                    windStrength,
                    bCollisionSwitch,
                    timescale,
                    is60FPS,
                    moveRate,
                    addMoveRate,
                    springRate);
            }
            finally
            {
                if (parentPin != null)
                    parentPin.Dispose();

                if (collisionPin != null)
                    collisionPin.Dispose();

                if (clothPin != null)
                    clothPin.Dispose();
            }

            // Compare after the pin is released, so clothWorkingArray holds the plugin's output.
            CySpringTrace.Record(nativeTracePre, clothWorkingArray, nClothWorking);
            if (managedOut != null)
                CySpringDiff.Compare(clothWorkingArray, managedOut, preState, nClothWorking);
            CySpringDiff.RestoreColliders(collisionArray, savedRadii);
            CySpringDiff.RestoreBones(clothWorkingArray, savedChara);
        }

        private static void UpdateNativeClothSkirtInternal(
            NativeClothWorking[] clothWorkingArray,
            int nClothWorking,
            NativeClothCollision[] collisionArray,
            NativeSkirtWorking[] skirtWorkingArray,
            int skirtWorkingIndex,
            ref NativeSkirtArg arg,
            NativeRootParentWork[] rootParentWorkArray,
            float stiffnessForceRate,
            float dragForceRate,
            float gravityRate,
            float windX,
            float windY,
            float windZ,
            float windStrength,
            bool bCollisionSwitch,
            float timescale,
            bool is60FPS,
            float moveRate,
            float addMoveRate,
            float springRate)
        {
            if (clothWorkingArray == null || clothWorkingArray.Length == 0)
                return;

            if (skirtWorkingArray == null || skirtWorkingArray.Length == 0)
                return;

            if ((uint)skirtWorkingIndex >= (uint)skirtWorkingArray.Length)
                return;

            if (!isNative)
            {
                var tracePre = CySpringTrace.Armed ? CySpringDiff.Snapshot(clothWorkingArray) : null;
                CySpringSolver.NativeClothSkirtUpdate(clothWorkingArray, nClothWorking, collisionArray,
                    skirtWorkingArray, skirtWorkingIndex, ref arg, rootParentWorkArray,
                    stiffnessForceRate, dragForceRate, gravityRate, windX, windY, windZ, windStrength,
                    bCollisionSwitch, timescale, is60FPS, moveRate, addMoveRate, springRate);
                CySpringTrace.Record(tracePre, clothWorkingArray, nClothWorking);
                return;
            }

            // Differential for the skirt-linked entry point, as above; the skirt working state is
            // compared as well as the cloth bones.
            NativeClothCollision[] skSavedRadii = CySpringDiff.ScaleColliders(collisionArray);
            int[] skSavedChara = CySpringDiff.ScaleBones(clothWorkingArray, nClothWorking);
            NativeClothWorking[] skClothPre = null, skClothMan = null, skTracePre = null;
            NativeSkirtWorking skPre = default, skMan = default;
            bool skArmed = CySpringDiff.Armed;
            if (CySpringTrace.Armed) skTracePre = CySpringDiff.Snapshot(clothWorkingArray);
            if (skArmed)
            {
                skClothPre = CySpringDiff.Snapshot(clothWorkingArray);
                skClothMan = CySpringDiff.Snapshot(clothWorkingArray);
                skPre = skirtWorkingArray[skirtWorkingIndex];
                var skirtCopy = (NativeSkirtWorking[])skirtWorkingArray.Clone();
                var argCopy = arg;
                CySpringSolver.NativeClothSkirtUpdate(skClothMan, nClothWorking, collisionArray,
                    skirtCopy, skirtWorkingIndex, ref argCopy, rootParentWorkArray,
                    stiffnessForceRate, dragForceRate, gravityRate, windX, windY, windZ, windStrength,
                    bCollisionSwitch, timescale, is60FPS, moveRate, addMoveRate, springRate);
                skMan = skirtCopy[skirtWorkingIndex];
            }

            PinnedArray<NativeClothWorking> clothPin = null;
            PinnedArray<NativeClothCollision> collisionPin = null;
            PinnedArray<NativeRootParentWork> parentPin = null;
            PinnedArray<NativeSkirtWorking> skirtWorkPin = null;
            PinnedValue<NativeSkirtArg> argPin = null;

            try
            {
                clothPin = new PinnedArray<NativeClothWorking>(clothWorkingArray);
                collisionPin = new PinnedArray<NativeClothCollision>(collisionArray);
                parentPin = new PinnedArray<NativeRootParentWork>(rootParentWorkArray);
                skirtWorkPin = new PinnedArray<NativeSkirtWorking>(skirtWorkingArray);
                argPin = new PinnedValue<NativeSkirtArg>(arg);

                IntPtr clothPtr = clothPin.Ptr;
                IntPtr collisionPtr = collisionPin.Ptr;
                IntPtr parentPtr = parentPin.Ptr;
                IntPtr workingPtr = skirtWorkPin.ElementPtr(skirtWorkingIndex);
                IntPtr argPtr = argPin.Ptr;

                if (clothPtr == IntPtr.Zero || workingPtr == IntPtr.Zero || argPtr == IntPtr.Zero)
                    return;

                NativeClothSkirtUpdate(
                    clothPtr,
                    nClothWorking,
                    collisionPtr,
                    workingPtr,
                    argPtr,
                    parentPtr,
                    stiffnessForceRate,
                    dragForceRate,
                    gravityRate,
                    windX,
                    windY,
                    windZ,
                    windStrength,
                    bCollisionSwitch,
                    timescale,
                    is60FPS,
                    moveRate,
                    addMoveRate,
                    springRate);

                arg = argPin.Value;
                CySpringTrace.Record(skTracePre, clothWorkingArray, nClothWorking);
                if (skArmed)
                {
                    CySpringDiff.Compare(clothWorkingArray, skClothMan, skClothPre, nClothWorking);
                    CySpringDiff.CompareSkirt(skPre, skirtWorkingArray[skirtWorkingIndex], skMan, arg);
                }
                CySpringDiff.RestoreColliders(collisionArray, skSavedRadii);
                CySpringDiff.RestoreBones(clothWorkingArray, skSavedChara);
            }
            finally
            {
                if (argPin != null)
                    argPin.Dispose();

                if (skirtWorkPin != null)
                    skirtWorkPin.Dispose();

                if (parentPin != null)
                    parentPin.Dispose();

                if (collisionPin != null)
                    collisionPin.Dispose();

                if (clothPin != null)
                    clothPin.Dispose();
            }
        }

        public static void UpdateNativeClothNoSkirt(
            NativeClothWorking[] clothWorkingArray,
            int clothWorkingCount,
            NativeClothCollision[] collisionArray,
            NativeRootParentWork[] rootParentWorkArray,
            float stiffnessForceRate,
            float dragForceRate,
            float gravityRate,
            float windX,
            float windY,
            float windZ,
            float windStrength,
            bool bCollisionSwitch,
            float timescale = 1.0f,
            bool is60FPS = false,
            float moveRate = 1.0f,
            float addMoveRate = 1.0f,
            float springRate = 1.0f)
        {
            UpdateNativeCloth(
                clothWorkingArray,
                clothWorkingCount,
                collisionArray,
                -1,
                null,
                rootParentWorkArray,
                stiffnessForceRate,
                dragForceRate,
                gravityRate,
                windX,
                windY,
                windZ,
                windStrength,
                bCollisionSwitch,
                timescale,
                is60FPS,
                moveRate,
                addMoveRate,
                springRate);
        }

        /// <summary>
        /// Official SkirtController.UpdateSkirt path:
        /// working pointer = NativeWorkingArray + index * sizeof(NativeSkirtWorking)
        /// arg pointer     = NativeArg
        /// </summary>
        public static void UpdateSkirtNativePluginOne(
            NativeSkirtWorking[] workingArray,
            int workingIndex,
            ref NativeSkirtArg arg)
        {
            if (workingArray == null || workingArray.Length == 0)
                return;

            if ((uint)workingIndex >= (uint)workingArray.Length)
                return;

            if (!isNative)
            {
                CySpringSolver.NativeSkirtUpdate(ref workingArray[workingIndex], ref arg);
                return;
            }

            // Differential for the standalone skirt entry point.
            NativeSkirtWorking skPre = default, skMan = default;
            bool skArmed = CySpringDiff.Armed;
            if (skArmed)
            {
                skPre = workingArray[workingIndex];
                skMan = workingArray[workingIndex];
                var argCopy = arg;
                CySpringSolver.NativeSkirtUpdate(ref skMan, ref argCopy);
            }

            PinnedArray<NativeSkirtWorking> workingPin = null;
            PinnedValue<NativeSkirtArg> argPin = null;

            try
            {
                workingPin = new PinnedArray<NativeSkirtWorking>(workingArray);
                argPin = new PinnedValue<NativeSkirtArg>(arg);

                IntPtr workingPtr = workingPin.ElementPtr(workingIndex);
                IntPtr argPtr = argPin.Ptr;

                if (workingPtr == IntPtr.Zero || argPtr == IntPtr.Zero)
                    return;

                NativeSkirtUpdate(
                    workingPtr,
                    argPtr);

                arg = argPin.Value;
                if (skArmed) CySpringDiff.CompareSkirt(skPre, workingArray[workingIndex], skMan, arg);
            }
            finally
            {
                if (argPin != null)
                    argPin.Dispose();

                if (workingPin != null)
                    workingPin.Dispose();
            }
        }

        /// <summary>
        /// Compatibility wrapper.
        /// Prefer the array + index overload when calling from SkirtController.UpdateSkirt,
        /// because official code passes the address of NativeWorkingArray[index].
        /// </summary>
        public static void UpdateSkirtNativePluginOne(
            ref NativeSkirtWorking working,
            ref NativeSkirtArg arg)
        {
            NativeSkirtWorking[] tempWorkingArray = new NativeSkirtWorking[1];
            tempWorkingArray[0] = working;

            UpdateSkirtNativePluginOne(
                tempWorkingArray,
                0,
                ref arg);

            working = tempWorkingArray[0];
        }

        private sealed class PinnedArray<T> : IDisposable where T : struct
        {
            private GCHandle _handle;
            private readonly int _elementSize;

            public IntPtr Ptr { get; private set; }
            public int Length { get; private set; }

            public PinnedArray(T[] array)
            {
                if (array == null || array.Length == 0)
                {
                    Ptr = IntPtr.Zero;
                    Length = 0;
                    _elementSize = Marshal.SizeOf(typeof(T));
                    return;
                }

                _handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                Ptr = _handle.AddrOfPinnedObject();
                Length = array.Length;
                _elementSize = Marshal.SizeOf(typeof(T));
            }

            public IntPtr ElementPtr(int index)
            {
                if (Ptr == IntPtr.Zero)
                    return IntPtr.Zero;

                if ((uint)index >= (uint)Length)
                    return IntPtr.Zero;

                return IntPtr.Add(Ptr, _elementSize * index);
            }

            public void Dispose()
            {
                if (_handle.IsAllocated)
                    _handle.Free();

                Ptr = IntPtr.Zero;
                Length = 0;
            }
        }

        private sealed class PinnedValue<T> : IDisposable where T : struct
        {
            private GCHandle _handle;
            private readonly T[] _array;

            public IntPtr Ptr { get; private set; }

            public PinnedValue(T value)
            {
                _array = new T[1];
                _array[0] = value;

                _handle = GCHandle.Alloc(_array, GCHandleType.Pinned);
                Ptr = _handle.AddrOfPinnedObject();
            }

            public T Value
            {
                get { return _array[0]; }
            }

            public void Dispose()
            {
                if (_handle.IsAllocated)
                    _handle.Free();

                Ptr = IntPtr.Zero;
            }
        }

        public static Vector3 RoundAngle(Vector3 angle)
        {
            return new Vector3(
                RoundAngle(angle.x),
                RoundAngle(angle.y),
                RoundAngle(angle.z));
        }

        public static float RoundAngle(float angle)
        {
            while (angle > 180.0f)
                angle -= 360.0f;

            while (angle < -180.0f)
                angle += 360.0f;

            return angle;
        }

        public static Vector3 MakePositive(Vector3 angle)
        {
            return new Vector3(
                MakePositive(angle.x),
                MakePositive(angle.y),
                MakePositive(angle.z));
        }

        public static float MakePositive(float angle)
        {
            while (angle < 0.0f)
                angle += 360.0f;

            while (angle >= 360.0f)
                angle -= 360.0f;

            return angle;
        }
    }
}