/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * <summary>ACF data</summary>
 * <remarks>
 * <para header='Description'>A class which manages the project settings set on CRI Atom Craft.<br/>
 * Acquires various information described in the ACF file.</para>
 * </remarks>
 */
public class CriAtomExAcf
{
    #region Native Struct Definition

    /**
     * <summary>A structure for getting the DSP bus setting information</summary>
     * <remarks>
     * <para header='Description'>A structure for getting the DSP bus setting information.<br/>
	 * Passed to the CriAtomExAcf::GetDSPSettingInformation function as an argument.<br/></para>
     * </remarks>
     * <seealso cref='CriAtomExAcf::GetDspSettingInformation'/>
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AcfDspSettingInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;                     /**< Setting name */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public ushort[] busIndexes;             /**< DSP bus index array */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public ushort[] extendBusIndexes;       /**< DSP extended bus index array */
        public ushort snapshotStartIndex;       /**< Snapshot start index */
        public byte numBuses;                   /**< The number of effective DSP buses */
        public byte numExtendBuses;             /**< The number of effective extended DSP buses */
        public ushort numSnapshots;             /**< The number of snapshots */
        public ushort snapshotWorkSize;         /**< Snapshot start index */
        public ushort numMixerAisacs;           /**< Number of mixer AISACs */
        public ushort mixerAisacStartIndex;     /**< Mixer AISAC start index */

        public AcfDspSettingInfo(byte[] data, int startIndex)
        {
            if (IntPtr.Size == 4)
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 0)));
                this.busIndexes = new ushort[64];
                for (int i = 0; i < 64; ++i)
                {
                    busIndexes[i] = BitConverter.ToUInt16(data, startIndex + 4 + (2 * i));
                }
                this.extendBusIndexes = new ushort[64];
                for (int i = 0; i < 64; ++i)
                {
                    extendBusIndexes[i] = BitConverter.ToUInt16(data, startIndex + 132 + (2 * i));
                }
                this.snapshotStartIndex = BitConverter.ToUInt16(data, startIndex + 260);
                this.numBuses = data[startIndex + 262];
                this.numExtendBuses = data[startIndex + 263];
                this.numSnapshots = BitConverter.ToUInt16(data, startIndex + 264);
                this.snapshotWorkSize = BitConverter.ToUInt16(data, startIndex + 266);
                this.numMixerAisacs = BitConverter.ToUInt16(data, startIndex + 268);
                this.mixerAisacStartIndex = BitConverter.ToUInt16(data, startIndex + 270);
            }
            else
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 0)));
                this.busIndexes = new ushort[64];
                for (int i = 0; i < 64; ++i)
                {
                    busIndexes[i] = BitConverter.ToUInt16(data, startIndex + 8 + (2 * i));
                }
                this.extendBusIndexes = new ushort[64];
                for (int i = 0; i < 64; ++i)
                {
                    extendBusIndexes[i] = BitConverter.ToUInt16(data, startIndex + 136 + (2 * i));
                }
                this.snapshotStartIndex = BitConverter.ToUInt16(data, startIndex + 264);
                this.numBuses = data[startIndex + 265];
                this.numExtendBuses = data[startIndex + 266];
                this.numSnapshots = BitConverter.ToUInt16(data, startIndex + 268);
                this.snapshotWorkSize = BitConverter.ToUInt16(data, startIndex + 270);
                this.numMixerAisacs = BitConverter.ToUInt16(data, startIndex + 272);
                this.mixerAisacStartIndex = BitConverter.ToUInt16(data, startIndex + 274);
            }
        }
    }

    /**
	 * <summary>A structure for getting the DSP bus setting snapshot information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting the snapshot information of the DSP bus settings.<br/>
	 * Passed to the CriAtomExAcf::GetDSPSettingSnapshotInformation function as an argument.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcf::GetDspSettingSnapshotInformation'/>
	 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AcfDspSettingSnapshotInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;                         /**< Snapshot name */
        public byte numBuses;                       /**< The number of effective DSP buses */
        public byte numExtendBuses;                 /**< The number of effective extended DSP buses */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] reserved;                     /**< Reserved area */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public ushort[] busIndexes;                 /**< DSP bus index array */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public ushort[] extendBusIndexes;           /**< DSP extended bus index array */

        public AcfDspSettingSnapshotInfo(byte[] data, int startIndex)
        {
            if (IntPtr.Size == 4)
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 0)));
                this.numBuses = data[startIndex + 4];
                this.numExtendBuses = data[startIndex + 5];
                this.reserved = new byte[2];
                for (int i = 0; i < 2; ++i)
                {
                    reserved[i] = data[startIndex + 6 + i];
                }
                this.busIndexes = new ushort[64];
                for (int i = 0; i < 64; ++i)
                {
                    busIndexes[i] = BitConverter.ToUInt16(data, startIndex + 8 + (2 * i));
                }
                this.extendBusIndexes = new ushort[64];
                for (int i = 0; i < 64; ++i)
                {
                    extendBusIndexes[i] = BitConverter.ToUInt16(data, startIndex + 136 + (2 * i));
                }
            }
            else
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 0)));
                this.numBuses = data[startIndex + 8];
                this.numExtendBuses = data[startIndex + 9];
                this.reserved = new byte[2];
                for (int i = 0; i < 2; ++i)
                {
                    reserved[i] = data[startIndex + 10 + i];
                }
                this.busIndexes = new ushort[64];
                for (int i = 0; i < 64; ++i)
                {
                    busIndexes[i] = BitConverter.ToUInt16(data, startIndex + 12 + (2 * i));
                }
                this.extendBusIndexes = new ushort[64];
                for (int i = 0; i < 64; ++i)
                {
                    extendBusIndexes[i] = BitConverter.ToUInt16(data, startIndex + 140 + (2 * i));
                }
            }
        }
    }

    /**
	 * <summary>A structure for getting the DSP bus setting information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting the DSP bus setting information.<br/>
	 * Passed to the CriAtomExAcf::GetDSPBusInformation function as an argument.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcf::GetDspBusInformation'/>
	 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AcfDspBusInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;                     /**< Name */
        public float volume;                    /**< Volume */
        public float pan3dVolume;               /**< Pan3D volume */
        public float pan3dAngle;                /**< Pan3D Angle */
        public float pan3dDistance;             /**< Pan3D interior distance */
        public float pan3dSpread;               /**< Pan3D spread */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ushort[] fxIndexes;              /**< DSP FX index array */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public ushort[] busLinkIndexes;         /**< DSP bus link index array */
        public ushort busNo;                    /**< DSP bus number in setting */
        public byte numFxes;                    /**< The number of DSP FXs */
        public byte numBusLinks;                /**< Number of DSP bus links */

        public AcfDspBusInfo(byte[] data, int startIndex)
        {
            if (IntPtr.Size == 4)
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 0)));
                this.volume = BitConverter.ToSingle(data, startIndex + 4);
                this.pan3dVolume = BitConverter.ToSingle(data, startIndex + 8);
                this.pan3dAngle = BitConverter.ToSingle(data, startIndex + 12);
                this.pan3dDistance = BitConverter.ToSingle(data, startIndex + 16);
                this.pan3dSpread = BitConverter.ToSingle(data, startIndex + 20);
                this.fxIndexes = new ushort[8];
                for (int i = 0; i < 8; ++i)
                {
                    fxIndexes[i] = BitConverter.ToUInt16(data, startIndex + 24 + (2 * i));
                }
                this.busLinkIndexes = new ushort[64];
                for (int i = 0; i < 64; ++i)
                {
                    busLinkIndexes[i] = BitConverter.ToUInt16(data, startIndex + 40 + (2 * i));
                }
                this.busNo = BitConverter.ToUInt16(data, startIndex + 168);
                this.numFxes = data[startIndex + 169];
                this.numBusLinks = data[startIndex + 170];
            }
            else
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 0)));
                this.volume = BitConverter.ToSingle(data, startIndex + 8);
                this.pan3dVolume = BitConverter.ToSingle(data, startIndex + 12);
                this.pan3dAngle = BitConverter.ToSingle(data, startIndex + 16);
                this.pan3dDistance = BitConverter.ToSingle(data, startIndex + 20);
                this.pan3dSpread = BitConverter.ToSingle(data, startIndex + 24);
                this.fxIndexes = new ushort[8];
                for (int i = 0; i < 8; ++i)
                {
                    fxIndexes[i] = BitConverter.ToUInt16(data, startIndex + 28 + (2 * i));
                }
                this.busLinkIndexes = new ushort[64];
                for (int i = 0; i < 64; ++i)
                {
                    busLinkIndexes[i] = BitConverter.ToUInt16(data, startIndex + 44 + (2 * i));
                }
                this.busNo = BitConverter.ToUInt16(data, startIndex + 172);
                this.numFxes = data[startIndex + 173];
                this.numBusLinks = data[startIndex + 174];
            }
        }
    }

    /**
	 * <summary>DSP bus link type</summary>
	 * <seealso cref='CriAtomExAcf::AcfDspBusLinkInfo'/>
	 */
    public enum AcfDspBusLinkType : uint
    {
        preVolume = 0,      /**< Pre-volume type */
        postVolume,         /**< Post volume type */
        postPan,            /**< Post Panning type */
    }

    /**
	 * <summary>A structure for getting the DSP bus link information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting the DSP bus link information.<br/>
	 * Passed to the CriAtomExAcf::GetDSPBusLinkInformation function as an argument.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcf::GetDspBusLinkInformation'/>
	 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AcfDspBusLinkInfo
    {
        public AcfDspBusLinkType type;          /**< Type */
        public float sendLevel;                 /**< Send level */
        public ushort busNo;                    /**< DSP bus number in the destination setting */
        public ushort busId;                    /**< DSP bus ID in the destination setting */

        public AcfDspBusLinkInfo(byte[] data, int startIndex)
        {
            {
                this.type = (AcfDspBusLinkType)Enum.ToObject(typeof(AcfDspBusLinkType), BitConverter.ToUInt32(data, startIndex + 0));
                this.sendLevel = BitConverter.ToSingle(data, startIndex + 4);
                this.busNo = BitConverter.ToUInt16(data, startIndex + 8);
                this.busId = BitConverter.ToUInt16(data, startIndex + 10);
            }
        }
    }

    /**
	 * <summary>A structure for getting the Category information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting the Category information.<br/>
	 * Passed to the CriAtomExAcf::GetCategoryInfo function as an argument.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcf::GetCategoryInfo'/>
	 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct CategoryInfo
    {
        public uint groupNo;                /**< Group number */
        public uint id;                     /**< Category ID */
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;                 /**< Category name */
        public uint numCueLimits;           /**< Cue limit count */
        public float volume;                /**< Volume */

        public CategoryInfo(byte[] data, int startIndex)
        {
            if (IntPtr.Size == 4)
            {
                this.groupNo = BitConverter.ToUInt16(data, startIndex + 0);
                this.id = BitConverter.ToUInt16(data, startIndex + 4);
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 8)));
                this.numCueLimits = BitConverter.ToUInt16(data, startIndex + 12);
                this.volume = BitConverter.ToSingle(data, startIndex + 16);
            }
            else
            {
                this.groupNo = BitConverter.ToUInt16(data, startIndex + 0);
                this.id = BitConverter.ToUInt16(data, startIndex + 4);
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 8)));
                this.numCueLimits = BitConverter.ToUInt16(data, startIndex + 16);
                this.volume = BitConverter.ToSingle(data, startIndex + 20);
            }
        }
    }

    /**
	 * <summary>AISAC type</summary>
	 * <seealso cref='CriAtomExAcf::GlobalAisacInfo'/>
	 */
    public enum AcfAisacType : uint
    {
        normal = 0,             /**< Normal type */
        autoModulation,         /**< Auto modulation type */
    }

    /**
	 * <summary>A structure for getting the AISAC information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for acquiring Global AISAC information. <br/>
	 * Can be used as an argument of the CriAtomExAcf::GetGlobalAisacInfo function. <br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcf::GetGlobalAisacInfo'/>
	 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GlobalAisacInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;             /**< Global AISAC name */
        public ushort index;            /**< Data index */
        public ushort numGraphs;        /**< Number of graphs */
        public AcfAisacType type;       /**< AISAC type */
        public float randomRange;       /**< Random range */
        public ushort controlId;        /**< Control Id */
        public ushort dummy;            /**< Unused */

        public GlobalAisacInfo(byte[] data, int startIndex)
        {
            if (IntPtr.Size == 4)
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 0)));
                this.index = BitConverter.ToUInt16(data, startIndex + 4);
                this.numGraphs = BitConverter.ToUInt16(data, startIndex + 6);
                this.type = (AcfAisacType)Enum.ToObject(typeof(AcfAisacType), BitConverter.ToUInt32(data, startIndex + 8));
                this.randomRange = BitConverter.ToSingle(data, startIndex + 12);
                this.controlId = BitConverter.ToUInt16(data, startIndex + 16);
                this.dummy = BitConverter.ToUInt16(data, startIndex + 18);
            }
            else
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 0)));
                this.index = BitConverter.ToUInt16(data, startIndex + 8);
                this.numGraphs = BitConverter.ToUInt16(data, startIndex + 10);
                this.type = (AcfAisacType)Enum.ToObject(typeof(AcfAisacType), BitConverter.ToUInt32(data, startIndex + 12));
                this.randomRange = BitConverter.ToSingle(data, startIndex + 16);
                this.controlId = BitConverter.ToUInt16(data, startIndex + 20);
                this.dummy = BitConverter.ToUInt16(data, startIndex + 22);
            }
        }
    }

    /**
	 * <summary>AISAC graph type</summary>
	 * <seealso cref='CriAtomExAcf::AisacGraphInfo'/>
	 */
    public enum AisacGraphType : int
    {
        none = 0,               /**< Unused */
        volume,                 /**< Volume */
        pitch,                  /**< Pitch */
        bandpassHigh,           /**< High cutoff frequency of the BandPass Filter */
        bandpassLow,            /**< Low cutoff frequency of the BandPass Filter */
        biquadFreq,             /**< Biquad Filter frequency */
        biquadQ,                /**< Q value of the Biquad Filter */
        busSend0,               /**< Bus Send Level 0 */
        busSend1,               /**< Bus Send Level 1 */
        busSend2,               /**< Bus Send Level 2 */
        busSend3,               /**< Bus Send Level 3 */
        busSend4,               /**< Bus Send Level 4 */
        busSend5,               /**< Bus Send Level 5 */
        busSend6,               /**< Bus Send Level 6 */
        busSend7,               /**< Bus Send Level 7 */
        pan3dAngel,             /**< Panning 3D angle */
        pan3dVolume,            /**< Panning 3D volume */
        pan3dInteriorDistance,  /**< Panning 3D distance */
        pan3dCenter,            /**< Not used in ACB Ver.0.11.00 and later. */
        pan3dLfe,               /**< Not used in ACB Ver.0.11.00 and later. */
        aisac0,                 /**< AISAC control ID 0 */
        aisac1,                 /**< AISAC control ID 1 */
        aisac2,                 /**< AISAC control ID 2 */
        aisac3,                 /**< AISAC control ID 3 */
        aisac4,                 /**< AISAC control ID 4 */
        aisac5,                 /**< AISAC control ID 5 */
        aisac6,                 /**< AISAC control ID 6 */
        aisac7,                 /**< AISAC control ID 7 */
        aisac8,                 /**< AISAC control ID 8 */
        aisac9,                 /**< AISAC control ID 9 */
        aisac10,                /**< AISAC control ID 10 */
        aisac11,                /**< AISAC control ID 11 */
        aisac12,                /**< AISAC control ID 12 */
        aisac13,                /**< AISAC control ID 13 */
        aisac14,                /**< AISAC control ID 14 */
        aisac15,                /**< AISAC control ID 15 */
        priority,               /**< Voice Priority */
        preDelayTime,           /**< Pre-delay */
        biquadGain,             /**< Biquad Filter gain */
        pan3dMixdownCenter,     /**< Panning 3D center level */
        pan3dMixdownLfe,        /**< Panning 3D LFE level */
        egAttack,               /**< Envelope Attack */
        egRelease,              /**< Envelope Release */
        playbackRatio,          /**< Sequence playback ratio */
        drySendL,               /**< L ch dry send */
        drySendR,               /**< R ch dry send */
        drySendCenter,          /**< Center ch dry send */
        drySendLfe,             /**< LFE ch dry send */
        drySendSl,              /**< Surround L ch dry send */
        drySendSr,              /**< Surround R ch dry send */
        drySendEx1,             /**< Ex1 ch dry send */
        drySendEx2,             /**< Ex2 ch dry send */
        panSpread,              /**< Panning spread */
    }

    /**
	 * <summary>A structure for getting the AISAC Graph information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting the Global AISAC Graph information.<br/>
	 * Passed to the CriWare.CriAtomExAcf::GetGlobalAisacGraphInfo function as an argument.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcf::GetGlobalAisacGraphInfo'/>
	 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AisacGraphInfo
    {
        public AisacGraphType type;     /**< Graph type */

        public AisacGraphInfo(byte[] data, int startIndex)
        {
            this.type = (AisacGraphType)Enum.ToObject(typeof(AisacGraphType), BitConverter.ToInt32(data, startIndex + 0));
        }
    }

    /**
	 * <summary>Character code</summary>
	 * <remarks>
	 * <para header='Description'>Indicates the character code (character encoding method).<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcf::AcfInfo'/>
	 */
    public enum CharacterEncoding : uint
    {
        utf8 = 0,  /**< UTF-8 */
        sjis = 1,  /**< Shift_JIS */
    }

    /**
	 * <summary>ACF information</summary>
	 * <remarks>
	 * <para header='Description'>Detailed information of the ACF data.<br/>
	 * Passed to the CriWare.CriAtomExAcf::GetAcfInfo function as an argument.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcf::GetAcfInfo'/>
	 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AcfInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;                             /**< Name */
        public uint size;                               /**< Size */
        public uint version;                            /**< ACB version */
        public CharacterEncoding characterEncoding;     /**< Character code */
        public int numDspSettings;                      /**< The number of DSP settings */
        public int numCategories;                       /**< The number of Categories */
        public int numCategoriesPerPlayback;            /**< The number of Category references per playback */
        public int numReacts;                           /**< The number of  REACTs */
        public int numAisacControls;                    /**< The number of AISAC controls */
        public int numGlobalAisacs;                     /**< The number of global AISACs */
        public int numGameVariables;                    /**< The number of game variables */
        public int maxBusesOfDspBusSettings;            /**< Maximum number of buses in the DSP setting */
        public int numBuses;                            /**< The number of buses */
        public int numVoiceLimitGroups;                 /**< The number of Voice limit groups */
        public int numOutputPorts;                      /**< Number of output ports */

        public AcfInfo(byte[] data, int startIndex)
        {
            if (IntPtr.Size == 4)
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 0)));
                this.size = BitConverter.ToUInt32(data, startIndex + 4);
                this.version = BitConverter.ToUInt32(data, startIndex + 8);
                this.characterEncoding = (CharacterEncoding)Enum.ToObject(typeof(CharacterEncoding), BitConverter.ToUInt32(data, startIndex + 12));
                this.numDspSettings = BitConverter.ToInt32(data, startIndex + 16);
                this.numCategories = BitConverter.ToInt32(data, startIndex + 20);
                this.numCategoriesPerPlayback = BitConverter.ToInt32(data, startIndex + 24);
                this.numReacts = BitConverter.ToInt32(data, startIndex + 28);
                this.numAisacControls = BitConverter.ToInt32(data, startIndex + 32);
                this.numGlobalAisacs = BitConverter.ToInt32(data, startIndex + 36);
                this.numGameVariables = BitConverter.ToInt32(data, startIndex + 40);
                this.maxBusesOfDspBusSettings = BitConverter.ToInt32(data, startIndex + 44);
                this.numBuses = BitConverter.ToInt32(data, startIndex + 48);
                this.numVoiceLimitGroups = BitConverter.ToInt32(data, startIndex + 52);
                this.numOutputPorts = BitConverter.ToInt32(data, startIndex + 56);
            }
            else
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 0)));
                this.size = BitConverter.ToUInt32(data, startIndex + 8);
                this.version = BitConverter.ToUInt32(data, startIndex + 12);
                this.characterEncoding = (CharacterEncoding)Enum.ToObject(typeof(CharacterEncoding), BitConverter.ToUInt32(data, startIndex + 16));
                this.numDspSettings = BitConverter.ToInt32(data, startIndex + 20);
                this.numCategories = BitConverter.ToInt32(data, startIndex + 24);
                this.numCategoriesPerPlayback = BitConverter.ToInt32(data, startIndex + 28);
                this.numReacts = BitConverter.ToInt32(data, startIndex + 32);
                this.numAisacControls = BitConverter.ToInt32(data, startIndex + 36);
                this.numGlobalAisacs = BitConverter.ToInt32(data, startIndex + 40);
                this.numGameVariables = BitConverter.ToInt32(data, startIndex + 44);
                this.maxBusesOfDspBusSettings = BitConverter.ToInt32(data, startIndex + 48);
                this.numBuses = BitConverter.ToInt32(data, startIndex + 52);
                this.numVoiceLimitGroups = BitConverter.ToInt32(data, startIndex + 56);
                this.numOutputPorts = BitConverter.ToInt32(data, startIndex + 60);
            }
        }
    }

    /**
	 * <summary>A structure for getting the selector information</summary>
	 * <remarks>
	 * <para header='Description'>A structure to get the selector information.<br/>
	 * Passed to the CriAtomExAcf::GetSelectorInfo function as an argument.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcf::GetSelectorInfo'/>
	 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SelectorInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;                     /**< Selector name */
        public ushort index;                    /**< Data index */
        public ushort numLabels;                /**< The number of labels */
        public ushort globalLabelIndex;         /**< Global reference label index */

        public SelectorInfo(byte[] data, int startIndex)
        {
            if (IntPtr.Size == 4)
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 0)));
                this.index = BitConverter.ToUInt16(data, startIndex + 4);
                this.numLabels = BitConverter.ToUInt16(data, startIndex + 6);
                this.globalLabelIndex = BitConverter.ToUInt16(data, startIndex + 8);
            }
            else
            {
                this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 0)));
                this.index = BitConverter.ToUInt16(data, startIndex + 8);
                this.numLabels = BitConverter.ToUInt16(data, startIndex + 10);
                this.globalLabelIndex = BitConverter.ToUInt16(data, startIndex + 12);
            }
        }
    }

    /**
	 * <summary>A structure for getting the Selector Label information</summary>
	 * <remarks>
	 * <para header='Description'>A a structure for getting the Selector Label information.<br/>
	 * Passed to the CriWare.CriAtomExAcf::GetSelectorLabelInfo function as an argument.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcf::GetSelectorLabelInfo'/>
	 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SelectorLabelInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string selectorName;         /**< Selector name */
        [MarshalAs(UnmanagedType.LPStr)]
        public string labelName;            /**< Selector Label name */

        public SelectorLabelInfo(byte[] data, int startIndex)
        {
            if (IntPtr.Size == 4)
            {
                this.selectorName = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 0)));
                this.labelName = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 4)));
            }
            else
            {
                this.selectorName = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 0)));
                this.labelName = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 8)));
            }
        }
    }
    #endregion

    #region Exported API
    /**
     * <summary>Gets the number of AISAC controls</summary>
     * <returns>The number of AISAC controls</returns>
     * <remarks>
     * <para header='Description'>Gets the number of AISAC controls contained in the registered ACF.<br/>
	 * Returns -1 if no ACF file is registered.</para>
     * </remarks>
     */
    public static int GetNumAisacControls()
    {
        return criAtomExAcf_GetNumAisacControls();
    }

    /**
     * <summary>Gets the AISAC control information</summary>
     * <param name='index'>AISAC control index</param>
     * <param name='info'>AISAC control information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the AISAC control information from the AISAC control index.<br/>
	 * Returns False if there is no AISAC control with the specified index.</para>
     * </remarks>
     */
    public static bool GetAisacControlInfo(ushort index, out CriAtomEx.AisacControlInfo info)
    {
        using (var mem = new CriStructMemory<CriAtomEx.AisacControlInfo>())
        {
            bool result = criAtomExAcf_GetAisacControlInfo(index, mem.ptr);
            info = new CriAtomEx.AisacControlInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the number of DSP bus settings</summary>
     * <returns>The number of DSP bus settings</returns>
     * <remarks>
     * <para header='Description'>Gets the number of DSP bus settings included in the ACF data registered with the library.<br/>
	 * This function returns -1 when ACF data is not registered.<br/></para>
     * </remarks>
     */
    public static int GetNumDspSettings()
    {
        return criAtomExAcf_GetNumDspSettings();
    }
	
	/**
     * <summary>Acquiring the DSP bus setting (from the specified ACF data)</summary>
	 * <param name='acfData'>ACF data pointer</param>
     * <param name='size'>ACF data size</param>
     * <returns>The number of DSP bus settings</returns>
     * <remarks>
     * <para header='Description'>Gets the number of DSP bus settings contained in the specified ACF data.<br/>
	 * This function can be called before the ACF data is registered.<br/></para>
     * </remarks>
     */
	public static int GetNumDspSettings(IntPtr acfData, int size)
    {
        return criAtomExAcf_GetNumDspSettingsFromAcfData(acfData, size);
    }
	
	/**
     * <summary>Acquiring the DSP bus setting's name</summary>
	 * <param name='index'>DSP bus setting index</param>
     * <returns>DSP bus setting name</returns>
     * <remarks>
     * <para header='Description'>Obtains the DSP bus setting name from the ACF data registered with the library.<br/>
	 * Returns null if no ACF data is registered or the DSP bus setting with the specified index does not exist.<br/></para>
     * </remarks>
     */
	public static string GetDspSettingNameByIndex(ushort index)
	{
		var ptr = criAtomExAcf_GetDspSettingNameByIndex(index);
		if (ptr == IntPtr.Zero) return null;
		return Marshal.PtrToStringAnsi(ptr);
	}
	
	/**
     * <summary>Acquiring the DSP bus setting's name (from the specified ACF data)</summary>
	 * <param name='acfData'>ACF data pointer</param>
     * <param name='size'>ACF data size</param>
	 * <param name='index'>DSP bus setting index</param>
     * <returns>DSP bus setting name</returns>
     * <remarks>
     * <para header='Description'>Obtains the DSP bus setting name from the ACF data registered with the library.<br/>
	 * This function can be called before the ACF data is registered.<br/></para>
     * </remarks>
     */
	public static string GetDspSettingNameByIndex(IntPtr acfData, int size, ushort index)
	{
		var ptr = criAtomExAcf_GetDspSettingNameByIndexFromAcfData(acfData, size, index);
		if (ptr == IntPtr.Zero) return null;
		return Marshal.PtrToStringAnsi(ptr);
	}

    /**
     * <summary>Gets the DSP bus setting information</summary>
     * <param name='name'>Setting name</param>
     * <param name='info'>Setting information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the setting information by specifying the setting name.<br/>
	 * Returns false if there is no DSP setting with the specified setting name.<br/></para>
     * </remarks>
     */
    public static bool GetDspSettingInformation(string name, out AcfDspSettingInfo info)
    {
        using (var mem = new CriStructMemory<AcfDspSettingInfo>())
        {
            bool result = criAtomExAcf_GetDspSettingInformation(name, mem.ptr);
            info = new AcfDspSettingInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the DSP bus setting snapshot information</summary>
     * <param name='index'>Snapshot index</param>
     * <param name='info'>Snapshot information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the snapshot information by specifying the snapshot index.<br/>
	 * Returns FALSE if there is no snapshot with the specified setting name.<br/>
	 * For the snapshot index, calculate an appropriate value based on the snapshotStartIndex and numSnapshots members
	 * in the CriAtomExAcf::AcfDspSettingInfo structure of the parent DSP bus setting information.</para>
     * </remarks>
     */
    public static bool GetDspSettingSnapshotInformation(ushort index, out AcfDspSettingSnapshotInfo info)
    {
        using (var mem = new CriStructMemory<AcfDspSettingSnapshotInfo>())
        {
            bool result = criAtomExAcf_GetDspSettingSnapshotInformation(index, mem.ptr);
            info = new AcfDspSettingSnapshotInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the DSP bus</summary>
     * <param name='index'>Bus index</param>
     * <param name='info'>Bus information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the DSP bus information by specifying the index.<br/>
	 * Returns False if there is no DSP bus with the specified index name.<br/></para>
     * </remarks>
     */
    public static bool GetDspBusInformation(ushort index, out AcfDspBusInfo info)
    {
        using (var mem = new CriStructMemory<AcfDspBusInfo>())
        {
            bool result = criAtomExAcf_GetDspBusInformation(index, mem.ptr);
            info = new AcfDspBusInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the DSP bus link</summary>
     * <param name='index'>DSP bus link index</param>
     * <param name='info'>DSP bus link information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the bus link information by specifying the index.<br/>
	 * Returns False if there is no DSP bus link with the specified index name.<br/></para>
     * </remarks>
     */
    public static bool GetDspBusLinkInformation(ushort index, out AcfDspBusLinkInfo info)
    {
        using (var mem = new CriStructMemory<AcfDspBusLinkInfo>())
        {
            bool result = criAtomExAcf_GetDspBusLinkInformation(index, mem.ptr);
            info = new AcfDspBusLinkInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the number of Categories</summary>
     * <returns>The number of Categories</returns>
     * <remarks>
     * <para header='Description'>Gets the number of Categories contained in the registered ACF.</para>
     * </remarks>
     */
    public static int GetNumCategories()
    {
        return criAtomExAcf_GetNumCategories();
    }

    /**
     * <summary>Gets the number of Category references per playback</summary>
     * <returns>The number of Category references per playback</returns>
     * <remarks>
     * <para header='Description'>Gets the number of Category references by playback contained in the registered ACF.</para>
     * </remarks>
     */
    public static int GetNumCategoriesPerPlayback()
    {
        return criAtomExAcf_GetNumCategoriesPerPlayback();
    }

    /**
     * <summary>Gets the Category information (index specified)</summary>
     * <param name='index'>Category index</param>
     * <param name='info'>Category information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the Category information from the Category index.<br/>
	 * Returns False if there is no Category with the specified index.</para>
     * </remarks>
     */
    public static bool GetCategoryInfoByIndex(ushort index, out CategoryInfo info)
    {
        using (var mem = new CriStructMemory<CategoryInfo>())
        {
            bool result = criAtomExAcf_GetCategoryInfo(index, mem.ptr);
            info = new CategoryInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the Category information (Category name specified)</summary>
     * <param name='name'>Specify Category name</param>
     * <param name='info'>Category information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets Category information from the Category name.<br/>
	 * Returns False if there is no Category with the specified Category name.</para>
     * </remarks>
     */
    public static bool GetCategoryInfoByName(string name, out CategoryInfo info)
    {
        using (var mem = new CriStructMemory<CategoryInfo>())
        {
            bool result = criAtomExAcf_GetCategoryInfoByName(name, mem.ptr);
            info = new CategoryInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the Category information (Category ID specified)</summary>
     * <param name='id'>Category ID</param>
     * <param name='info'>Category information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the Category information from the Category ID.<br/>
	 * Returns False if there is no category with the specified category ID.</para>
     * </remarks>
     */
    public static bool GetCategoryInfoById(uint id, out CategoryInfo info)
    {
        using (var mem = new CriStructMemory<CategoryInfo>())
        {
            bool result = criAtomExAcf_GetCategoryInfoById(id, mem.ptr);
            info = new CategoryInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the number of Global AISACs</summary>
     * <returns>The number of Global AISACs</returns>
     * <remarks>
     * <para header='Description'>Gets the number of Global AISAC contained in the registered ACF.</para>
     * </remarks>
     */
    public static int GetNumGlobalAisacs()
    {
        return criAtomExAcf_GetNumGlobalAisacs();
    }

    /**
     * <summary>Gets the Global AISAC information (index specified)</summary>
     * <param name='index'>Global AISAC index</param>
     * <param name='info'>Global AISAC information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets AISAC information from the Global AISAC index.<br/>
	 * Returns False if there is no Global AISAC with the specified index.</para>
     * </remarks>
     */
    public static bool GetGlobalAisacInfoByIndex(ushort index, out GlobalAisacInfo info)
    {
        using (var mem = new CriStructMemory<GlobalAisacInfo>())
        {
            bool result = criAtomExAcf_GetGlobalAisacInfo(index, mem.ptr);
            info = new GlobalAisacInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the Global AISAC information (name specified)</summary>
     * <param name='name'>Global AISAC name</param>
     * <param name='info'>Global AISAC information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the AISAC information from the Global AISAC name<br/>
	 * Returns False if there is no Global AISAC with the specified name.</para>
     * </remarks>
     */
    public static bool GetGlobalAisacInfoByName(string name, out GlobalAisacInfo info)
    {
        using (var mem = new CriStructMemory<GlobalAisacInfo>())
        {
            bool result = criAtomExAcf_GetGlobalAisacInfoByName(name, mem.ptr);
            info = new GlobalAisacInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the Global AISAC Graph information</summary>
     * <param name='aisacInfo'>Global AISAC information</param>
     * <param name='graphIndex'>Global AISAC graph index</param>
     * <param name='graphInfo'>AISAC graph information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the graph information from the Global AISAC information and graph index.<br/>
	 * Returns False if there is no Global AISAC with the specified index.</para>
     * </remarks>
     */
    public static bool GetGlobalAisacGraphInfo(GlobalAisacInfo aisacInfo, ushort graphIndex, out AisacGraphInfo graphInfo)
    {
        bool result = false;
        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(GlobalAisacInfo)));
        Marshal.StructureToPtr(aisacInfo, ptr, false);
        using (var mem = new CriStructMemory<AisacGraphInfo>())
        {
            result = criAtomExAcf_GetGlobalAisacGraphInfo(ptr, graphIndex, mem.ptr);
            graphInfo = new AisacGraphInfo(mem.bytes, 0);
        }
        Marshal.FreeHGlobal(ptr);
        return result;
    }

    /**
     * <summary>Gets the  Global AISAC value</summary>
     * <param name='aisacInfo'>Global AISAC information</param>
     * <param name='control'>AISAC control value</param>
     * <param name='type'>Graph type</param>
     * <param name='value'>AISAC value</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the AISAC value by specifying Global AISAC information, control value and graph type.<br/>
	 * Returns False if there is no Global AISAC with the specified index or there is no graph.</para>
     * </remarks>
     */
    public static bool GetGlobalAisacValue(GlobalAisacInfo aisacInfo, float control, AisacGraphType type, out float value)
    {
        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(GlobalAisacInfo)));
        Marshal.StructureToPtr(aisacInfo, ptr, false);
        bool result = criAtomExAcf_GetGlobalAisacValue(ptr, control, type, out value);
        Marshal.FreeHGlobal(ptr);
        return result;
    }

    /**
     * <summary>Gets the ACF information</summary>
     * <param name='acfInfo'>ACF information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets various information on the ACF data registered with the library.<br/>
	 * Returns False if acquisition of the ACF information failed.</para>
     * </remarks>
     */
    public static bool GetAcfInfo(out AcfInfo acfInfo)
    {
        using (var mem = new CriStructMemory<AcfInfo>())
        {
            bool result = criAtomExAcf_GetAcfInfo(mem.ptr);
            acfInfo = new AcfInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the number of selectors</summary>
     * <returns>The number of selectors</returns>
     * <remarks>
     * <para header='Description'>Gets the number of selectors contained in the registered ACF.</para>
     * </remarks>
     */
    public static int GetNumSelectors()
    {
        return criAtomExAcf_GetNumSelectors();
    }

    /**
     * <summary>Gets the selector information (index specified)</summary>
     * <param name='index'>Selector index</param>
     * <param name='info'>Selector information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the selector information from the selector index.<br/>
	 * Returns False if there is no selector with the specified index.</para>
     * </remarks>
     */
    public static bool GetSelectorInfoByIndex(ushort index, out SelectorInfo info)
    {
        using (var mem = new CriStructMemory<SelectorInfo>())
        {
            bool result = criAtomExAcf_GetSelectorInfoByIndex(index, mem.ptr);
            info = new SelectorInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the selector information (name specified)</summary>
     * <param name='name'>Selector name</param>
     * <param name='info'>Selector information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Get the selector information from the selector name.<br/>
	 * Returns False if there is no selector with the specified name.</para>
     * </remarks>
     */
    public static bool GetSelectorInfoByName(string name, out SelectorInfo info)
    {
        using (var mem = new CriStructMemory<SelectorInfo>())
        {
            bool result = criAtomExAcf_GetSelectorInfoByName(name, mem.ptr);
            info = new SelectorInfo(mem.bytes, 0);
            return result;
        }
    }

    /**
     * <summary>Gets the Selector Label information</summary>
     * <param name='selectorInfo'>Selector information</param>
     * <param name='labelIndex'>Label index</param>
     * <param name='info'>Selector Label information</param>
     * <returns>Whether the information can be acquired. (Could be acquired: True / could not be acquired: False)</returns>
     * <remarks>
     * <para header='Description'>Gets the Selector Label information from the selector information and Selector Label index.<br/>
	 * Returns False if there is no Selector Label with the specified index.</para>
     * </remarks>
     */
    public static bool GetSelectorLabelInfo(SelectorInfo selectorInfo, ushort labelIndex, out SelectorLabelInfo info)
    {
        bool result = false;
        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SelectorInfo)));
        Marshal.StructureToPtr(selectorInfo, ptr, false);
        using (var mem = new CriStructMemory<SelectorLabelInfo>())
        {
            result = criAtomExAcf_GetSelectorLabelInfo(ptr, labelIndex, mem.ptr);
            info = new SelectorLabelInfo(mem.bytes, 0);
        }
        Marshal.FreeHGlobal(ptr);
        return result;
    }

    /**
     * <summary>Gets the number of buses</summary>
     * <returns>The number of buses</returns>
     * <remarks>
     * <para header='Description'>Gets the number of buses contained in the registered ACF.</para>
     * </remarks>
     */
    public static int GetNumBuses()
    {
        return criAtomExAcf_GetNumBuses();
    }

    /**
     * <summary>Gets the maximum number of buses in the DSP bus settings</summary>
     * <returns>Maximum number of buses in the DSP bus settings</returns>
     * <remarks>
     * <para header='Description'>Gets the maximum number of buses in the DSP bus settings contained in the registered ACF.</para>
     * </remarks>
     */
    public static int GetMaxBusesOfDspBusSettings()
    {
        return criAtomExAcf_GetMaxBusesOfDspBusSettings();
    }

    /**
     * <summary>Gets the bus name in ACF</summary>
     * <param name='busName'>Bus name</param>
     * <returns>Bus name in ACF</returns>
     * <remarks>
     * <para header='Description'>Gets the string in the ACF for the specified bus name.<br/>
	 * Returns NULL if you specify a bus name that does not exist.<br/></para>
     * </remarks>
     */
    public static string FindBusName(string busName)
    {
        return criAtomExAcf_FindBusName(busName);
    }
    #endregion

    #region DLL Import
#if !CRIWARE_ENABLE_HEADLESS_MODE
    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetNumAisacControls();

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetAisacControlInfo(ushort index, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern uint criAtomExAcf_GetAisacControlIdByName(string name);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern string criAtomExAcf_GetAisacControlNameById(uint id);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetNumDspSettings();

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetNumDspSettingsFromAcfData(IntPtr acf_data, int acf_data_size);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern IntPtr criAtomExAcf_GetDspSettingNameByIndex(ushort index);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern IntPtr criAtomExAcf_GetDspSettingNameByIndexFromAcfData(IntPtr acf_data, int acf_data_size, ushort index);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetDspSettingInformation(string name, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetDspSettingSnapshotInformation(ushort index, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetDspBusInformation(ushort index, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetDspFxType(ushort index);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern string criAtomExAcf_GetDspFxName(ushort index);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetDspFxParameters(ushort index, IntPtr parameters, int size);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetDspBusLinkInformation(ushort index, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetNumCategoriesFromAcfData(IntPtr acf_data, int acf_data_size);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetNumCategories();

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetNumCategoriesPerPlaybackFromAcfData(IntPtr acf_data, int acf_data_size);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetNumCategoriesPerPlayback();

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetCategoryInfo(ushort index, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetCategoryInfoByName(string name, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetCategoryInfoById(uint id, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetNumGlobalAisacs();

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetGlobalAisacInfo(ushort index, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetGlobalAisacInfoByName(string name, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetGlobalAisacGraphInfo(IntPtr aisac_info, ushort graph_index, IntPtr graph_info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetGlobalAisacValue(IntPtr aisac_info, float control, AisacGraphType type, out float value);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetAcfInfo(IntPtr acf_info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetAcfInfoFromAcfData(IntPtr acf_data, int acf_data_size, IntPtr acf_info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetNumSelectors();

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetSelectorInfoByIndex(ushort index, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetSelectorInfoByName(string name, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern bool criAtomExAcf_GetSelectorLabelInfo(IntPtr selector_info, ushort label_index, IntPtr info);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetNumBusesFromAcfData(IntPtr acf_data, int acf_data_size);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetNumBuses();

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetMaxBusesOfDspBusSettingsFromAcfData(IntPtr acf_data, int acf_data_size);

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern int criAtomExAcf_GetMaxBusesOfDspBusSettings();

    [DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
    private static extern string criAtomExAcf_FindBusName(string bus_name);
#else
    private static int criAtomExAcf_GetNumAisacControls() { return 0; }
    private static bool criAtomExAcf_GetAisacControlInfo(ushort index, IntPtr info) { return false; }
    private static uint criAtomExAcf_GetAisacControlIdByName(string name) { return 0; }
    private static string criAtomExAcf_GetAisacControlNameById(uint id) { return null; }
    private static int criAtomExAcf_GetNumDspSettings() { return 0; }
    private static int criAtomExAcf_GetNumDspSettingsFromAcfData(IntPtr acf_data, int acf_data_size) { return 0; }
    private static IntPtr criAtomExAcf_GetDspSettingNameByIndex(ushort index) { return IntPtr.Zero; }
    private static IntPtr criAtomExAcf_GetDspSettingNameByIndexFromAcfData(IntPtr acf_data, int acf_data_size, ushort index) { return IntPtr.Zero; }
    private static bool criAtomExAcf_GetDspSettingInformation(string name, IntPtr info) { return false; }
    private static bool criAtomExAcf_GetDspSettingSnapshotInformation(ushort index, IntPtr info) { return false; }
    private static bool criAtomExAcf_GetDspBusInformation(ushort index, IntPtr info) { return false; }
    private static int criAtomExAcf_GetDspFxType(ushort index) { return 0; }
    private static string criAtomExAcf_GetDspFxName(ushort index) { return null; }
    private static bool criAtomExAcf_GetDspFxParameters(ushort index, IntPtr parameters, int size) { return false; }
    private static bool criAtomExAcf_GetDspBusLinkInformation(ushort index, IntPtr info) { return false; }
    private static int criAtomExAcf_GetNumCategoriesFromAcfData(IntPtr acf_data, int acf_data_size) { return 0; }
    private static int criAtomExAcf_GetNumCategories() { return 0; }
    private static int criAtomExAcf_GetNumCategoriesPerPlaybackFromAcfData(IntPtr acf_data, int acf_data_size) { return 0; }
    private static int criAtomExAcf_GetNumCategoriesPerPlayback() { return 0; }
    private static bool criAtomExAcf_GetCategoryInfo(ushort index, IntPtr info) { return false; }
    private static bool criAtomExAcf_GetCategoryInfoByName(string name, IntPtr info) { return false; }
    private static bool criAtomExAcf_GetCategoryInfoById(uint id, IntPtr info) { return false; }
    private static int criAtomExAcf_GetNumGlobalAisacs() { return 0; }
    private static bool criAtomExAcf_GetGlobalAisacInfo(ushort index, IntPtr info) { return false; }
    private static bool criAtomExAcf_GetGlobalAisacInfoByName(string name, IntPtr info) { return false; }
    private static bool criAtomExAcf_GetGlobalAisacGraphInfo(IntPtr aisac_info, ushort graph_index, IntPtr graph_info) { return false; }
    private static bool criAtomExAcf_GetGlobalAisacValue(IntPtr aisac_info, float control, AisacGraphType type, out float value) { value = 0.0f; return false; }
    private static bool criAtomExAcf_GetAcfInfo(IntPtr acf_info) { return false; }
    private static bool criAtomExAcf_GetAcfInfoFromAcfData(IntPtr acf_data, int acf_data_size, IntPtr acf_info) { return false; }
    private static int criAtomExAcf_GetNumSelectors() { return 0; }
    private static bool criAtomExAcf_GetSelectorInfoByIndex(ushort index, IntPtr info) { return false; }
    private static bool criAtomExAcf_GetSelectorInfoByName(string name, IntPtr info) { return false; }
    private static bool criAtomExAcf_GetSelectorLabelInfo(IntPtr selector_info, ushort label_index, IntPtr info) { return false; }
    private static int criAtomExAcf_GetNumBusesFromAcfData(IntPtr acf_data, int acf_data_size) { return 0; }
    private static int criAtomExAcf_GetNumBuses() { return 0; }
    private static int criAtomExAcf_GetMaxBusesOfDspBusSettingsFromAcfData(IntPtr acf_data, int acf_data_size) { return 0; }
    private static int criAtomExAcf_GetMaxBusesOfDspBusSettings() { return 0; }
    private static string criAtomExAcf_FindBusName(string bus_name) { return null; }
#endif
    #endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
