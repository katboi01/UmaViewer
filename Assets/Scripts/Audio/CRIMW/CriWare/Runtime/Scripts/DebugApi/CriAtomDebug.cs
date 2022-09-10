/****************************************************************************
 *
 * Copyright (c) 2016 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/
using System;
using System.Runtime.InteropServices;


/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * <summary>A class that contains the APIs for debugging the CriAtomEx application.</summary>
 */
public static class CriAtomExDebug
{
	/**
	 * <summary>Status of various resources inside CriAtomEx</summary>
	 */
	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct ResourcesInfo
	{
		/** Virtual Voice usage status */
		public CriAtomEx.ResourceUsage virtualVoiceUsage;

		/** Sequence usage status */
		public CriAtomEx.ResourceUsage sequenceUsage;

		/** Sequence track usage status */
		public CriAtomEx.ResourceUsage sequenceTrackUsage;

		/** Sequence track item usage status */
		public CriAtomEx.ResourceUsage sequenceTrackItemUsage;
	}

	/**
	 * <summary>Gets the status of various resources inside CriAtomEx</summary>
	 * <param name='resourcesInfo'>Status of various resources inside CriAtomEx</param>
	 * <remarks>
	 * <para header='Description'>Gets the status of various resources inside CriAtomEx.<br/></para>
	 * </remarks>
	 */
	public static void GetResourcesInfo (out ResourcesInfo resourcesInfo)
	{
		criAtomExDebug_GetResourcesInfo (out resourcesInfo);
	}

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExDebug_GetResourcesInfo (out ResourcesInfo resourcesInfo);
	#else
	private static void criAtomExDebug_GetResourcesInfo (out ResourcesInfo resourcesInfo) { resourcesInfo = new ResourcesInfo(); }
	#endif
	#endregion
}


/**
 * <summary>A class that contains the APIs for debugging the CriAtomExAcf application.</summary>
 */
public static class CriAtomExAcfDebug
{
	/**
	 * <summary>Category information</summary>
	 */
	public struct CategoryInfo
	{
		/** Group number */
		public uint groupNo;

		/** Category ID */
		public uint id;

		/** Category name */
		public string name;

		/** Cue limit count */
		public uint numCueLimits;

		/** Volume */
		public float volume;
	}

	/**
	 * <summary>DSP bus setting information</summary>
	 */
	public struct DspBusInfo
	{
		/** Name */
		public string name;

		/** Volume */
		public float volume;

		/** Pan3D volume */
		public float pan3dVolume;

		/** Pan3D Angle */
		public float pan3dAngle;

		/** Pan3D interior distance */
		public float pan3dDistance;

		/** Pan3D spread */
		public float pan3dSpread;

		[MarshalAs (UnmanagedType.ByValArray, SizeConst = 8)]
		/** DSP FX index array */
		public ushort[] fxIndexes;

		[MarshalAs (UnmanagedType.ByValArray, SizeConst = 64)]
		/** DSP bus link index array */
		public ushort[] busLinkIndexes;

		/** DSP bus number in setting */
		public ushort busNo;

		/** The number of DSP FXs */
		public byte numFxes;

		/** Number of DSP bus links */
		public byte numBusLinks;
	}

	/**
	 * <summary>AISAC control information</summary>
	 */
	public struct AisacControlInfo
	{
		/** Name */
		public string name;

		/** ID */
		public uint id;
	}

	/**
	 * <summary>AISAC type</summary>
	 */
	public enum AisacType
	{
		/** Normal type */
		Normal,

		/** Auto modulation type */
		AutoModulation,
	}

	/**
	 * <summary>Global AISAC information</summary>
	 */
	public struct GlobalAisacInfo
	{
		/** Global AISAC name */
		public string name;

		/** Data index */
		public ushort index;

		/** Number of graphs */
		public ushort numGraphs;

		/** AISAC type */
		public AisacType type;

		/** Random range */
		public float randomRange;

		/** Control ID */
		public ushort controlId;
	}

	/**
	 * <summary>Selector information</summary>
	 */
	public struct SelectorInfo
	{
		/** Selector name */
		public string name;

		/** Selector index */
		public ushort index;

		/** The number of Selector Labels */
		public ushort numLabels;

		/** Global reference label index */
		public ushort globalLabelIndex;
	}

	/**
	 * <summary>Selector Label information</summary>
	 */
	public struct SelectorLabelInfo
	{
		/** Selector name */
		public string selectorName;

		/** Selector Label name */
		public string labelName;
	}

	/**
	 * <summary>Gets the number of Categories</summary>
	 * <returns>The number of Categories</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of Categories contained in the registered ACF.</para>
	 * </remarks>
	 */
	public static int GetNumCategories ()
	{
		return criAtomExAcf_GetNumCategories ();
	}

	/**
	 * <summary>Gets the Category information (index specified)</summary>
	 * <param name='index'>Category index</param>
	 * <param name='categoryInfo'>Category information</param>
	 * <returns>Whether the information could be obtained</returns>
	 * <remarks>
	 * <para header='Description'>Gets information relative to a category by specifying its index.<br/>
	 * If the category at the specified index does not exist, false is returned.</para>
	 * </remarks>
	 */
	public static bool GetCategoryInfoByIndex (ushort index, out CategoryInfo categoryInfo)
	{
		CategoryInfoForMarshaling x;
		bool result = criAtomExAcf_GetCategoryInfo (index, out x) != 0;
		x.Convert (out categoryInfo);
		return result;
	}

	/**
	 * <summary>Gets the Category information (name specified)</summary>
	 * <param name='name'>Category name</param>
	 * <param name='categoryInfo'>Category information</param>
	 * <returns>Whether the information could be obtained</returns>
	 * <remarks>
	 * <para header='Description'>Gets information relative to a category by specifying its index.<br/>
	 * If the category at the specified index does not exist, false is returned.</para>
	 * </remarks>
	 */
	public static bool GetCategoryInfoByName (string name, out CategoryInfo categoryInfo)
	{
		CategoryInfoForMarshaling x;
		bool result = criAtomExAcf_GetCategoryInfoByName (name, out x) != 0;
		x.Convert (out categoryInfo);
		return result;
	}

	/**
	 * <summary>Gets the Category information (ID specified)</summary>
	 * <param name='id'>Category ID</param>
	 * <param name='categoryInfo'>Category information</param>
	 * <returns>Whether the information could be obtained</returns>
	 * <remarks>
	 * <para header='Description'>Gets information relative to a category by specifying its index.<br/>
	 * If the category at the specified index does not exist, false is returned.</para>
	 * </remarks>
	 */
	public static bool GetCategoryInfoById (uint id, out CategoryInfo categoryInfo)
	{
		CategoryInfoForMarshaling x;
		bool result = criAtomExAcf_GetCategoryInfoById (id, out x) != 0;
		x.Convert (out categoryInfo);
		return result;
	}

	/**
	 * <summary>Gets the number of DSP buses</summary>
	 * <returns>Number of DSP buses</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of buses contained in the registered ACF.</para>
	 * </remarks>
	 */
	public static int GetNumBuses()
	{
		return criAtomExAcf_GetNumBuses();
	}

	/**
	 * <summary>Gets the DSP bus</summary>
	 * <param name='index'>Bus index</param>
	 * <param name='dspBusInfo'>Bus information</param>
	 * <returns>Whether the bus information could be acquired</returns>
	 * <remarks>
	 * <para header='Description'>Gets information relative to a DSP Bus by specifying its index.<br/>
	 * If the DSP bus with the specified index does not exist, false is returned.</para>
	 * </remarks>
	 */
	public static bool GetDspBusInformation (ushort index, out DspBusInfo dspBusInfo)
	{
		DspBusInfoForMarshaling x;
		bool result = criAtomExAcf_GetDspBusInformation (index, out x) != 0;
		x.Convert (out dspBusInfo);
		return result;
	}

	/**
	 * <summary>Gets the number of AISAC controls</summary>
	 * <returns>The number of AISAC controls</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of AISAC controls contained in the registered ACF.<br/>
	 * Returns -1 if no ACF file is registered.</para>
	 * </remarks>
	 */
	public static int GetNumAisacControls ()
	{
		return criAtomExAcf_GetNumAisacControls ();
	}

	/**
	 * <summary>Gets the AISAC control information</summary>
	 * <param name='index'>AISAC control index</param>
	 * <param name='info'>AISAC control information</param>
	 * <returns>Whether the information could be obtained</returns>
	 * <remarks>
	 * <para header='Description'>Obtains the AISAC control information from the AISAC control index.<br/>
	 * Returns false if there is no AISAC control at the specified index.</para>
	 * </remarks>
	 */
	public static bool GetAisacControlInfo (ushort index, out AisacControlInfo info)
	{
		AisacControlInfoForMarshaling x;
		bool result = criAtomExAcf_GetAisacControlInfo (index, out x) != 0;
		x.Convert (out info);
		return result;
	}

	/**
	 * <summary>Getting the AISAC control ID (specifying the AISAC control name)</summary>
	 * <param name='name'>AISAC control name</param>
	 * <returns>AISAC control ID</returns>
	 * <remarks>
	 * <para header='Description'>Gets the AISAC control ID from the AISAC control name.<br/>
	 * Returns CriAtomEx.InvalidAisacControlId
	 * if ACF is not registered or the AISAC control with the specified AISAC control name does not exist.</para>
	 * </remarks>
	 */
	public static uint GetAisacControlIdByName (string name)
	{
		return criAtomExAcf_GetAisacControlIdByName (name);
	}

	/**
	 * <summary>Gets the AISAC control name (specifying the AISAC control ID)</summary>
	 * <param name='id'>AISAC control ID</param>
	 * <returns>AISAC control ID</returns>
	 * <remarks>
	 * <para header='Description'>Gets the AISAC control name from the AISAC control ID.<br/>
	 * Returns null if ACF is not registered or the AISAC control with the specified AISAC control ID does not exist.</para>
	 * </remarks>
	 */
	public static string GetAisacControlNameById (uint id)
	{
		IntPtr namePtr = criAtomExAcf_GetAisacControlNameById (id);
		return CriAtomDebugDetail.Utility.PtrToStringAutoOrNull (namePtr);
	}

	/**
	 * <summary>Gets the number of global AISACs</summary>
	 * <returns>The number of global AISACs</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of global AISAC contained in the registered ACF.<br/>
	 * Returns -1 if no ACF file is registered.</para>
	 * </remarks>
	 */
	public static int GetNumGlobalAisacs ()
	{
		return criAtomExAcf_GetNumGlobalAisacs ();
	}

	/**
	 * <summary>Gets the global AISAC information</summary>
	 * <param name='index'>Global AISAC index</param>
	 * <param name='info'>Global AISAC information</param>
	 * <returns>Whether the information could be obtained</returns>
	 * <remarks>
	 * <para header='Description'>Gets information relative to a global AISAC by specifying its index.<br/>
	 * If the global AISAC with the specified index does not exist, false is returned.</para>
	 * </remarks>
	 */
	public static bool GetGlobalAisacInfo (ushort index, out GlobalAisacInfo info)
	{
		GlobalAisacInfoForMarshaling x;
		bool result = criAtomExAcf_GetGlobalAisacInfo (index, out x) != 0;
		x.Convert (out info);
		return result;
	}

	/**
	 * <summary>Gets the global AISAC information</summary>
	 * <param name='name'>Global AISAC name</param>
	 * <param name='info'>Global AISAC information</param>
	 * <returns>Whether the information could be obtained</returns>
	 * <remarks>
	 * <para header='Description'>Gets information relative to a global AISAC by specifying its name.<br/>
	 * If the global AISAC with the specified name does not exist, false is returned.</para>
	 * </remarks>
	 */
	public static bool GetGlobalAisacInfoByName(string name, out GlobalAisacInfo info)
	{
		GlobalAisacInfoForMarshaling x;
		bool result = criAtomExAcf_GetGlobalAisacInfoByName (name, out x) != 0;
		x.Convert(out info);
		return result;
	}

	/**
	 * <summary>Gets the number of selectors</summary>
	 * <returns>The number of selectors</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of selectors contained in the registered ACF.<br/>
	 * Returns -1 if no ACF file is registered.</para>
	 * </remarks>
	 */
	public static int GetNumSelectors()
	{
		return criAtomExAcf_GetNumSelectors();
	}

	/**
	 * <summary>Gets the selector information</summary>
	 * <param name='index'>Selector index</param>
	 * <param name='info'>Selector information</param>
	 * <returns>Whether the information could be obtained</returns>
	 * <remarks>
	 * <para header='Description'>Gets information relative to a selector by specifying its index. <br/>
	 * If no selector exists at the specified index, false is returned.</para>
	 * </remarks>
	 */
	public static bool GetSelectorInfoByIndex(ushort index, out SelectorInfo info)
	{
		SelectorInfoForMarshaling x;
		bool result = criAtomExAcf_GetSelectorInfoByIndex(index, out x) != 0;
		x.Convert(out info);
		return result;
	}

	/**
	 * <summary>Gets the selector information</summary>
	 * <param name='name'>Selector name</param>
	 * <param name='info'>Selector information</param>
	 * <returns>Whether the information could be obtained</returns>
	 * <remarks>
	 * <para header='Description'>Gets information relative to a selector by specifying its name. <br/>
	 * If no selector with the specified name exists, false is returned.</para>
	 * </remarks>
	 */
	public static bool GetSelectorInfoByName(string name, out SelectorInfo info)
	{
		SelectorInfoForMarshaling x;
		bool result = criAtomExAcf_GetSelectorInfoByName(name, out x) != 0;
		x.Convert(out info);
		return result;
	}

	/**
	 * <summary>Gets the Selector Label information</summary>
	 * <param name='selectorInfo'>Selector information</param>
	 * <param name='index'>Label index</param>
	 * <param name='labelInfo'>Selector Label information</param>
	 * <returns>Whether the information could be obtained</returns>
	 * <remarks>
	 * <para header='Description'>Gets information relative to a selector label by specifying its index.<br/>
	 * If no selector label with the specified index exists, false will be returned.</para>
	 * </remarks>
	 */
	public static bool GetSelectorLabelInfo(ref SelectorInfo selectorInfo, ushort index, out SelectorLabelInfo labelInfo)
	{
		var selectorInfoInput = new SelectorInfoForMarshaling();
		selectorInfoInput.index = selectorInfo.index;
		selectorInfoInput.numLabels = selectorInfo.numLabels;
		SelectorLabelInfoForMarshaling x;
		bool result = criAtomExAcf_GetSelectorLabelInfo(ref selectorInfoInput, index, out x) != 0;
		x.Convert(out labelInfo);
		return result;
	}

	#region Private

	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct CategoryInfoForMarshaling
	{
		public uint groupNo;
		public uint id;
		public IntPtr namePtr;
		public uint numCueLimits;
		public float volume;

		public void Convert (out CategoryInfo x)
		{
			x.groupNo = groupNo;
			x.id = id;
			x.name = CriAtomDebugDetail.Utility.PtrToStringAutoOrNull (namePtr);
			x.numCueLimits = numCueLimits;
			x.volume = volume;
		}
	};

	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct DspBusInfoForMarshaling
	{
		public IntPtr namePtr;
		public float volume;
		public float pan3dVolume;
		public float pan3dAngle;
		public float pan3dDistance;
		public float pan3dSpread;
		[MarshalAs (UnmanagedType.ByValArray, SizeConst = 8)]
		public ushort[] fxIndexes;
		[MarshalAs (UnmanagedType.ByValArray, SizeConst = 64)]
		public ushort[] busLinkIndexes;
		public ushort busNo;
		public byte numFxes;
		public byte numBusLinks;

		public void Convert (out DspBusInfo x)
		{
			x.name = CriAtomDebugDetail.Utility.PtrToStringAutoOrNull (namePtr);
			x.volume = volume;
			x.pan3dVolume = pan3dVolume;
			x.pan3dAngle = pan3dAngle;
			x.pan3dDistance = pan3dDistance;
			x.pan3dSpread = pan3dSpread;
			x.fxIndexes = fxIndexes;
			x.busLinkIndexes = busLinkIndexes;
			x.busNo = busNo;
			x.numFxes = numFxes;
			x.numBusLinks = numBusLinks;
		}
	};

	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct AisacControlInfoForMarshaling {
		public IntPtr namePtr;
		public uint id;

		public void Convert (out AisacControlInfo x)
		{
			x.name = CriAtomDebugDetail.Utility.PtrToStringAutoOrNull (namePtr);
			x.id = id;
		}
	}

	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct GlobalAisacInfoForMarshaling {
		public IntPtr namePtr;
		public ushort index;
		public ushort numGraphs;
		public uint type;
		public float randomRange;
		public ushort controlId;
		public ushort dummy;

		public void Convert(out GlobalAisacInfo x)
		{
			x.name = CriAtomDebugDetail.Utility.PtrToStringAutoOrNull(namePtr);
			x.index = index;
			x.numGraphs = numGraphs;
			x.type = (AisacType)type;
			x.randomRange = randomRange;
			x.controlId = controlId;
		}
	};

	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct SelectorInfoForMarshaling {
		public IntPtr namePtr;
		public ushort index;
		public ushort numLabels;
		public ushort globalLabelIndex;

		public void Convert(out SelectorInfo x)
		{
			x.name = CriAtomDebugDetail.Utility.PtrToStringAutoOrNull(namePtr);
			x.index = index;
			x.numLabels = numLabels;
			x.globalLabelIndex = globalLabelIndex;
		}
	};

	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct SelectorLabelInfoForMarshaling {
		public IntPtr selectorNamePtr;
		public IntPtr labelNamePtr;

		public void Convert(out SelectorLabelInfo x)
		{
			x.selectorName = CriAtomDebugDetail.Utility.PtrToStringAutoOrNull(selectorNamePtr);
			x.labelName = CriAtomDebugDetail.Utility.PtrToStringAutoOrNull(labelNamePtr);
		}
	};

	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetNumCategories ();

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetCategoryInfo (ushort index, out CategoryInfoForMarshaling categoryInfo);

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetCategoryInfoByName (string name, out CategoryInfoForMarshaling categoryInfo);

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetCategoryInfoById (uint id, out CategoryInfoForMarshaling categoryInfo);

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetNumBuses();

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetDspBusInformation (ushort index, out DspBusInfoForMarshaling dspBusInfo);

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetNumAisacControls ();

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetAisacControlInfo (ushort index, out AisacControlInfoForMarshaling info);

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern uint criAtomExAcf_GetAisacControlIdByName (string name);

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomExAcf_GetAisacControlNameById (uint id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetNumGlobalAisacs ();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetGlobalAisacInfo (ushort index, out GlobalAisacInfoForMarshaling info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetGlobalAisacInfoByName (string name, out GlobalAisacInfoForMarshaling info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetNumSelectors();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetSelectorInfoByIndex(ushort index, out SelectorInfoForMarshaling info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetSelectorInfoByName(string name, out SelectorInfoForMarshaling info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcf_GetSelectorLabelInfo(ref SelectorInfoForMarshaling info, ushort labelIndex, out SelectorLabelInfoForMarshaling label_info);
	#else
	private static int criAtomExAcf_GetNumCategories () { return 0; }
	private static int criAtomExAcf_GetCategoryInfo (ushort index, out CategoryInfoForMarshaling categoryInfo)
		{ categoryInfo = new CategoryInfoForMarshaling(); return -1; }
	private static int criAtomExAcf_GetCategoryInfoByName (string name, out CategoryInfoForMarshaling categoryInfo)
		{ categoryInfo = new CategoryInfoForMarshaling(); return -1; }
	private static int criAtomExAcf_GetCategoryInfoById (uint id, out CategoryInfoForMarshaling categoryInfo)
		{ categoryInfo = new CategoryInfoForMarshaling(); return -1; }
	private static int criAtomExAcf_GetNumBuses() { return 0; }
	private static int criAtomExAcf_GetDspBusInformation (ushort index, out DspBusInfoForMarshaling dspBusInfo)
		{ dspBusInfo = new DspBusInfoForMarshaling(); return -1; }
	private static int criAtomExAcf_GetNumAisacControls () { return 0; }
	private static int criAtomExAcf_GetAisacControlInfo (ushort index, out AisacControlInfoForMarshaling info)
		{ info = new AisacControlInfoForMarshaling(); return -1; }
	private static uint criAtomExAcf_GetAisacControlIdByName (string name) { return 0u; }
	private static IntPtr criAtomExAcf_GetAisacControlNameById (uint id) { return IntPtr.Zero; }
	private static int criAtomExAcf_GetNumGlobalAisacs () { return 0; }
	private static int criAtomExAcf_GetGlobalAisacInfo (ushort index, out GlobalAisacInfoForMarshaling info)
		{ info = new GlobalAisacInfoForMarshaling(); return -1; }
	private static int criAtomExAcf_GetGlobalAisacInfoByName (string name, out GlobalAisacInfoForMarshaling info)
		{ info = new GlobalAisacInfoForMarshaling(); return -1; }
	private static int criAtomExAcf_GetNumSelectors() { return 0; }
	private static int criAtomExAcf_GetSelectorInfoByIndex(ushort index, out SelectorInfoForMarshaling info)
		{ info = new SelectorInfoForMarshaling(); return -1; }
	private static int criAtomExAcf_GetSelectorInfoByName(string name, out SelectorInfoForMarshaling info)
		{ info = new SelectorInfoForMarshaling(); return -1; }
	private static int criAtomExAcf_GetSelectorLabelInfo(ref SelectorInfoForMarshaling info, ushort labelIndex, out SelectorLabelInfoForMarshaling label_info)
		{ label_info = new SelectorLabelInfoForMarshaling(); return -1; }
	#endif
	#endregion
}


/**
 * <summary>A class that contains the APIs for debugging the CriAtomExAcb application.</summary>
 */
public static class CriAtomExAcbDebug
{
	/**
	 * <summary>ACB information</summary>
	 * <remarks>
	 * <para header='Description'>Various information of the ACB data.</para>
	 * </remarks>
	 */
	public struct AcbInfo
	{
		/** Name */
		public string name;

		/** Size */
		public uint size;

		/** ACB version */
		public uint version;

		/** Character code */
		public CriAtomEx.CharacterEncoding characterEncoding;

		/** Cue Sheet volume */
		public float volume;

		/** Number of Cues */
		public int numCues;
	};

	/**
	 * <summary>Obtains the ACB information</summary>
	 * <param name='acb'>ACB</param>
	 * <param name='acbInfo'>ACB information</param>
	 * <returns>Whether the information could be obtained</returns>
	 * <remarks>
	 * <para header='Description'>Gets various information of the ACB data.</para>
	 * </remarks>
	 */
	public static bool GetAcbInfo (CriAtomExAcb acb, out AcbInfo acbInfo)
	{
		AcbInfoForMarshaling x;
		bool result = criAtomExAcb_GetAcbInfo (acb.nativeHandle, out x) == 1;
		x.Convert (out acbInfo);
		return result;
	}

	#region Private

	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct AcbInfoForMarshaling
	{
		public IntPtr namePtr;
		public uint size;
		public uint version;
		public CriAtomEx.CharacterEncoding characterEncoding;
		public float volume;
		public int numCues;

		public void Convert (out AcbInfo x)
		{
			x.name = CriAtomDebugDetail.Utility.PtrToStringAutoOrNull (namePtr);
			x.size = size;
			x.version = version;
			x.characterEncoding = characterEncoding;
			x.volume = volume;
			x.numCues = numCues;
		}
	};

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAcb_GetAcbInfo (IntPtr acbHn, out AcbInfoForMarshaling acbInfo);

	#endregion
}


/**
 * <summary>A class that contains the APIs for debugging the CriAtomExPlayback application.</summary>
 */
public static class CriAtomExPlaybackDebug
{
	/**
	 * <summary>Gets parameters</summary>
	 * <param name='playback'>CriAtomExPlayback</param>
	 * <param name='parameterId'>Parameter ID</param>
	 * <param name='value'>Parameter value (output)</param>
	 * <returns>Whether the parameter could be get successfully</returns>
	 * <remarks>
	 * <para header='Description'>Gets the values of various parameters of CriAtomExPlayback. <br/>
	 * If the parameters could successfully be acquired, this function returns true.<br/>
	 * If the specified voice has already been deleted, this function returns false.<br/></para>
	 * </remarks>
	 */
	public static bool GetParameter (CriAtomExPlayback playback, CriAtomEx.Parameter parameterId, out float value)
	{
		return criAtomExPlayback_GetParameterFloat32 (playback.id, (int)parameterId, out value) == 1;
	}

	/**
	 * <summary>Gets parameters</summary>
	 * <param name='playback'>CriAtomExPlayback</param>
	 * <param name='parameterId'>Parameter ID</param>
	 * <param name='value'>Parameter value (output)</param>
	 * <returns>Whether the parameter could be get successfully</returns>
	 * <remarks>
	 * <para header='Description'>Gets the values of various parameters of CriAtomExPlayback. <br/>
	 * If the parameters could successfully be acquired, this function returns true.<br/>
	 * If the specified voice has already been deleted, this function returns false.<br/></para>
	 * </remarks>
	 */
	public static bool GetParameter (CriAtomExPlayback playback, CriAtomEx.Parameter parameterId, out uint value)
	{
		return criAtomExPlayback_GetParameterUint32 (playback.id, (int)parameterId, out value) == 1;
	}

	/**
	 * <summary>Gets parameters</summary>
	 * <param name='playback'>CriAtomExPlayback</param>
	 * <param name='parameterId'>Parameter ID</param>
	 * <param name='value'>Parameter value (output)</param>
	 * <returns>Whether the parameter could be get successfully</returns>
	 * <remarks>
	 * <para header='Description'>Gets the values of various parameters of CriAtomExPlayback. <br/>
	 * If the parameters could successfully be acquired, this function returns true.<br/>
	 * If the specified voice has already been deleted, this function returns false.<br/></para>
	 * </remarks>
	 */
	public static bool GetParameter (CriAtomExPlayback playback, CriAtomEx.Parameter parameterId, out int value)
	{
		return criAtomExPlayback_GetParameterSint32 (playback.id, (int)parameterId, out value) == 1;
	}

	/**
	 * <summary>Gets the AISAC control value (specifying the control ID)</summary>
	 * <param name='playback'>Playback</param>
	 * <param name='controlId'>Control ID</param>
	 * <param name='value'>Control value (0.0f to 1.0f), -1.0f when not set</param>
	 * <returns>Whether the AISAC control value was obtained</returns>
	 * <remarks>
	 * <para header='Description'>Returns the AISAC control value by specifying the control ID (for the audio played by CriWare.CriAtomExPlayer::Start).<br/>
	 * Returns true if the AISAC control value was successfully acquired (the value will be -1.0f if no value was set).<br/>
	 * If the specified voice has already been removed, this function returns false.<br/></para>
	 * <para header='Note'>This function can get the AISAC control value only during sound playback.<br/>
	 * Getting the AISAC control value fails after the playback ends or when the Voice is erased by the Voice control.</para>
	 * </remarks>
	 */
	public static bool GetAisacControl (CriAtomExPlayback playback, uint controlId, out float value)
	{
		return criAtomExPlayback_GetAisacControlById (playback.id, controlId, out value) == 1;
	}

	/**
	 * <summary>Gets the AISAC control value (specifying the control name)</summary>
	 * <param name='playback'>Playback</param>
	 * <param name='controlName'>Control name</param>
	 * <param name='value'>Control value (0.0f to 1.0f), -1.0f when not set</param>
	 * <returns>Whether the AISAC control value was obtained</returns>
	 * <remarks>
	 * <para header='Description'>Returns the AISAC control value by specifying the control ID (for the audio played by CriWare.CriAtomExPlayer::Start).<br/>
	 * Returns true if the AISAC control value was successfully acquired (the value will be -1.0f if no value was set).<br/>
	 * If the specified voice has already been removed, this function returns false.<br/></para>
	 * <para header='Note'>This function can get the AISAC control value only during sound playback.<br/>
	 * Getting the AISAC control value fails after the playback ends or when the Voice is erased by the Voice control.</para>
	 * </remarks>
	 */
	public static bool GetAisacControl (CriAtomExPlayback playback, string controlName, out float value)
	{
		return criAtomExPlayback_GetAisacControlByName (playback.id, controlName, out value) == 1;
	}

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExPlayback_GetParameterFloat32 (uint id, int parameterId, out float value);

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExPlayback_GetParameterUint32 (uint id, int parameterId, out uint value);

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExPlayback_GetParameterSint32 (uint id, int parameterId, out int value);

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExPlayback_GetAisacControlById (uint id, uint controlId, out float value);

	[DllImport (CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExPlayback_GetAisacControlByName (uint id, string controlName, out float value);
	#else
	private static int criAtomExPlayback_GetParameterFloat32 (uint id, int parameterId, out float value) { value = 0.0f; return 0; }
	private static int criAtomExPlayback_GetParameterUint32 (uint id, int parameterId, out uint value) { value = 0u; return 0; }
	private static int criAtomExPlayback_GetParameterSint32 (uint id, int parameterId, out int value) { value = 0;return 0; }
	private static int criAtomExPlayback_GetAisacControlById (uint id, uint controlId, out float value) { value = 0.0f; return 0; }
	private static int criAtomExPlayback_GetAisacControlByName (uint id, string controlName, out float value) { value = 0.0f; return 0; }
	#endif
	#endregion
}


namespace CriAtomDebugDetail
{
	public class Utility
	{
		public static string PtrToStringAutoOrNull(IntPtr stringPtr)
		{
#if !UNITY_EDITOR && UNITY_WINRT
            return (stringPtr == IntPtr.Zero) ? null : Marshal.PtrToStringUni(stringPtr);
#elif UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
            return (stringPtr == IntPtr.Zero) ? null : Marshal.PtrToStringAnsi(stringPtr);
#else
            return (stringPtr == IntPtr.Zero) ? null : Marshal.PtrToStringAuto(stringPtr);
#endif
		}
	}
}

} //namespace CriWare
/**
 * @}
 */
