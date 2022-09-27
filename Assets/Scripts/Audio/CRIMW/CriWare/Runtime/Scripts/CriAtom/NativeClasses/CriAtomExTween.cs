/****************************************************************************
 *
 * Copyright (c) 2020 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System.Runtime.InteropServices;
using System;

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/

/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * <summary>AtomExTween Class</summary>
 * <remarks>
 * <para header='Description'>Runs the animation of the parameters by attaching to the player.</para>
 * </remarks>
 * <seealso cref='CriAtomExPlayer.AttachTween(CriAtomExTween)'/>
 * <seealso cref='CriAtomExPlayer.DetachTween(CriAtomExTween)'/>
 * <seealso cref='CriAtomExPlayer.DetachTweenAll'/>
 */
public class CriAtomExTween : CriDisposable
{
    internal IntPtr nativeHandle { get { return this.handle; } }

    /**
     * <summary>Type of Tween parameter</summary>
     */
    public enum ParameterType : System.Int32
    {
        /**
         * <summary>Manipulation of basic parameters such as volume and pitch</summary>
         * <seealso cref='CriAtomEx.Parameter'/>
         */
        Basic,
        /**
         * <summary>Manipulating the AISAC control value</summary>
         */
        Aisac
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Config
    {
        public Target target;
        public ParameterType parameterType;
        [StructLayout(LayoutKind.Explicit)]
        public struct Target
        {
            [FieldOffset(0)]
            public CriAtomEx.Parameter parameterId;
            [FieldOffset(0)]
            public UInt32 aisacIds;
        }
    }

    /**
     * <summary>Creates an AtomExTween</summary>
     * <returns>AtomExTween Object</returns>
     * <remarks>
     * <para header='Note'>The AtomExTween created by this constructor operates on the volume.</para>
     * </remarks>
     */
    public CriAtomExTween() : this(CriAtomEx.Parameter.Volume) { }

    /**
     * <summary>Creates an AtomExTween (control of basic parameters)</summary>
     * <returns>AtomExTween Object</returns>
     * <param name='parameterId'>Parameter ID</param>
     * <seealso cref='CriAtomEx.Parameter'/>
     */
    public CriAtomExTween(CriAtomEx.Parameter parameterId) : this(ParameterType.Basic, (UInt32)parameterId) { }

    /**
     * <summary>Creates an AtomExTween (AISAC control)</summary>
     * <returns>AtomExTween Object</returns>
     * <param name='aisacId'>AISAC control ID</param>
     */
    public CriAtomExTween(uint aisacId) : this(ParameterType.Aisac, aisacId) { }

    public CriAtomExTween(ParameterType parameterType, UInt32 targetId)
    {
        /*  Initialize Library  */
        if (!CriAtomPlugin.IsLibraryInitialized())
            throw new Exception("CriAtomPlugin is not initialized.");
        /* aplly config */
        Config config = new Config();
        config.parameterType = parameterType;
        config.target.parameterId = (CriAtomEx.Parameter)targetId;
        /* create instance */
        handle = criAtomExTween_Create(ref config, IntPtr.Zero, 0);

        CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
    }

    /**
     * <summary>Destroys the AtomExTween</summary>
     * <remarks>
     * <para header='Note'>A reference to the destroyed AtomExTween will occur if this function is excuted <br/>
	 * while the AtomExPlayer with the AtomExTween attached is playing audio. <br/>
	 * Be sure to execute this function after detaching from the AtomExPlayer.</para>
     * </remarks>
     */
    public override void Dispose()
    {
        Dispose(true);
    }

    /**
     * <summary>Gets the current values of the parameters of AtomExTween</summary>
     * <returns>The current value of the parameter</returns>
     */
    public float Value
    {
        get
        {
            return criAtomExTween_GetValue(handle);
        }
    }

    /**
     * <summary>Gets whether it is changing currently</summary>
     * <returns>Whether the parameter is changing</returns>
     */
    public bool IsActive
    {
        get
        {
            return criAtomExTween_IsActive(handle);
        }
    }

    /**
     * <summary>Changes the parameter from the current value to the specified one</summary>
     * <remarks>
     * <para header='Description'>Changes the parameter from the current value when called to the specified value over the specified time. <br/>
	 * The change curve type is linear.</para>
     * </remarks>
     * <param name='durationMs'>Time required for the change (milliseconds)</param>
     * <param name='value'>The final value after the change</param>
     */
    public void MoveTo(ushort durationMs, float value)
    {
        criAtomExTween_MoveTo(handle, durationMs, value);
    }

    /**
     * <summary>Changes the parameter from the specified value to the current one</summary>
     * <remarks>
     * <para header='Description'>Changes the parameter from the specified value to the current value when called over the specified time. <br/>
	 * The change curve type is linear.</para>
     * </remarks>
     * <param name='durationMs'>Time required for the change (milliseconds)</param>
     * <param name='value'>The initial value before the change</param>
     */
    public void MoveFrom(ushort durationMs, float value)
    {
        criAtomExTween_MoveFrom(handle, durationMs, value);
    }

    /**
     * <summary>Stops the AtomExTween</summary>
     * <remarks>
     * <para header='Description'>Stops the time change of parameters caused by AtomExTween. <br/>
	 * The value of the parameter will be its value when stopped.</para>
     * </remarks>
     */
    public void Stop()
    {
        criAtomExTween_Stop(handle);
    }

    /**
     * <summary>Resets the AtomExTween</summary>
     * <remarks>
     * <para header='Description'>Stops the AtomExTween and resets the parameters to their initial values.
	 * For basic parameters: Initial value of each parameter <br/>
	 * For AISAC control values: 0.0</para>
     * </remarks>
     */
    public void Reset()
    {
        criAtomExTween_Reset(handle);
    }

    #region Internal

    ~CriAtomExTween()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        CriDisposableObjectManager.Unregister(this);

        if (handle != IntPtr.Zero)
        {
            criAtomExTween_Destroy(handle);
            handle = IntPtr.Zero;
        }

        if(disposing)
        {
            GC.SuppressFinalize(this);
        }
    }

    IntPtr handle = IntPtr.Zero;

    #endregion

    #region DLL Import
#if !CRIWARE_ENABLE_HEADLESS_MODE
    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern IntPtr criAtomExTween_Create(ref Config config, IntPtr work, int work_size);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern void criAtomExTween_Destroy(IntPtr tween);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern Single criAtomExTween_GetValue(IntPtr tween);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern void criAtomExTween_MoveTo(IntPtr tween, UInt16 time_ms, Single value);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern void criAtomExTween_MoveFrom(IntPtr tween, UInt16 time_ms, Single value);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern void criAtomExTween_Stop(IntPtr tween);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern void criAtomExTween_Reset(IntPtr tween);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExTween_IsActive(IntPtr tween);
#else
    private static IntPtr criAtomExTween_Create(ref Config config, IntPtr work, int work_size) { return IntPtr.Zero; }
    private static void criAtomExTween_Destroy(IntPtr tween) { }
    private static Single criAtomExTween_GetValue(IntPtr tween) { return 0f; }
    private static void criAtomExTween_MoveTo(IntPtr tween, UInt16 time_ms, Single value) { }
    private static void criAtomExTween_MoveFrom(IntPtr tween, UInt16 time_ms, Single value) { }
    private static void criAtomExTween_Stop(IntPtr tween) { }
    private static void criAtomExTween_Reset(IntPtr tween) { }
    private static bool criAtomExTween_IsActive(IntPtr tween) { return false; }
#endif
    #endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
