/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AisacControlId = System.UInt32;

namespace CriWare {

public class CriStructMemory <Type> : IDisposable
{
	public byte[] bytes {
		get; private set;
	}
	public IntPtr ptr {
		get { return gch.AddrOfPinnedObject(); }
	}
	GCHandle gch;

	public CriStructMemory()
	{
		this.bytes = new byte[Marshal.SizeOf(typeof(Type))];
		this.gch = GCHandle.Alloc(this.bytes, GCHandleType.Pinned);
	}

	public CriStructMemory(int num)
	{
		this.bytes = new byte[Marshal.SizeOf(typeof(Type)) * num];
		this.gch = GCHandle.Alloc(this.bytes, GCHandleType.Pinned);
	}

	public void Dispose()
	{
		this.gch.Free();
	}
}

} //namespace CriWare

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * <summary>Global class of the Atom library.</summary>
 * <remarks>
 * <para header='Description'>A class that contains setting functions for the Atom library and variable types shared in the Atom library.<br/></para>
 * </remarks>
 */
public static class CriAtomEx
{
	/**
	 * <summary>Invalid value of the AISAC control ID</summary>
	 */
	public const AisacControlId InvalidAisacControlId = 0xffffffffu;

	/**
	 * <summary>Character code</summary>
	 * <remarks>
	 * <para header='Description'>Indicates the character code (character encoding method).</para>
	 * </remarks>
	 */
	public enum CharacterEncoding : int
	{
		/** UTF-8 */
		Utf8,
		/** Shift_JIS */
		Sjis,
	}

	/**
	 * <summary>Sound renderer type</summary>
	 * <remarks>
	 * <para header='Description'>Indicates the type of sound renderer created internally by CriWare.CriAtomExPlayer.</para>
	 * </remarks>
	 */
	 public enum SoundRendererType {
		Default = 0,
		Native = 1,
		Asr = 2,
		Hw1 = 1,
		Hw2 = 9,
		Haptic = 3,
	}

	/**
	 * <summary>Voice allocation method</summary>
	 * <remarks>
	 * <para header='Description'>Data type used to specify the behavior of CriWare.CriAtomExPlayer when it allocates voices.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::CriAtomExPlayer'/>
	 */
	public enum VoiceAllocationMethod {
		Once,                       /**< Allocate a Voice only once */
		Retry,                      /**< Allocate a Voice repeatedly */
	}

	/**
	 * <summary>Biquad Filter type</summary>
	 * <remarks>
	 * <para header='Description'>A data type that specifies the type of Biquad Filter.<br/>
	 * Used in the CriWare.CriAtomExPlayer::SetBiquadFilterParameters .</para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetBiquadFilterParameters'/>
	 */
	public enum BiquadFilterType {
		Off,                        /**< Filter disabled */
		LowPass,                    /**< Low pass filter */
		HighPass,                   /**< High pass filter */
		Notch,                      /**< Notch filter */
		LowShelf,                   /**< Low shelf filter */
		HighShelf,                  /**< High shelf filter */
		Peaking                     /**< Peaking filter */
	}

	/**
	 * <summary>Unpausing method</summary>
	 * <remarks>
	 * <para header='Description'>A data type for specifying the target to unpause.<br/>
	 * Used as an argument to the CriWare.CriAtomExPlayer::Resume and
	 * CriWare.CriAtomExPlayback::Resume functions.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::Resume'/>
	 * <seealso cref='CriAtomExPlayback::Resume'/>
	 */
	public enum ResumeMode {
		AllPlayback = 0,            /**< Resumes playback regardless of how it was paused */
		PausedPlayback = 1,         /**< Resumes playback only for the sound paused by the Pause function */
		PreparedPlayback = 2,       /**< Starts playback of the sound prepared for playback using the Prepare function */
	}

	/**
	 * <summary>Panning type</summary>
	 * <remarks>
	 * <para header='Description'>A data type for specifying how to perform the localization calculation.<br/>
	 * Used in the CriWare.CriAtomExPlayer::SetPanType .<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetPanType'/>
	 */
	public enum PanType {
		Unknown = -1,               /**< Unknown panning type */
		Pan3d = 0,                  /**< Calculates the localization using Panning 3D */
		Pos3d,                      /**< Calculates the localization with the 3D Positioning */
		Auto,                       /**<
									 * Calculates the localization based on the 3D Positioning when the 3D sound source/3D lister is
									 * set to the AtomExPlayer, or on the pan 3D if not set.
									 */
	}

	/**
	 * <summary>Voice control method</summary>
	 * <remarks>
	 * <para header='Description'>A data type used to specify the Voice control method for the sound played by the AtomExPlayer.<br/>
	 * Used in the CriWare.CriAtomExPlayer::SetVoiceControlMethod .<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetVoiceControlMethod'/>
	 */
	public enum VoiceControlMethod {
		PreferLast = 0,             /**< Last-note priority */
		PreferFirst,                /**< First-note priority */
	}

	/**
	 * <summary>Parameter ID</summary>
	 * <remarks>
	 * <para header='Description'>The ID for specifying the parameter.<br/>
	 * Used in functions such as CriWare.CriAtomExPlayer::GetParameterFloat32 .</para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::GetParameterFloat32'/>
	 * <seealso cref='CriAtomExPlayer::GetParameterSint32'/>
	 * <seealso cref='CriAtomExPlayer::GetParameterUint32'/>
	 */
	public enum Parameter {
		Volume                  =  0,   /**< Volume */
		Pitch                   =  1,   /**< Pitch */
		Pan3dAngle              =  2,   /**< Panning 3D angle */
		Pan3dDistance           =  3,   /**< Panning 3D distance */
		Pan3dVolume             =  4,   /**< Panning 3D volume */
		BusSendLevel0           =  9,   /**< Bus Send Level 0 */
		BusSendLevel1           = 10,   /**< Bus Send Level 1 */
		BusSendLevel2           = 11,   /**< Bus Send Level 2 */
		BusSendLevel3           = 12,   /**< Bus Send Level 3 */
		BusSendLevel4           = 13,   /**< Bus Send Level 4 */
		BusSendLevel5           = 14,   /**< Bus Send Level 5 */
		BusSendLevel6           = 15,   /**< Bus Send Level 6 */
		BusSendLevel7           = 16,   /**< Bus Send Level 7 */
		BandPassFilterCofLow    = 17,   /**< Low cutoff frequency of the BandPass Filter */
		BandPassFilterCofHigh   = 18,   /**< High cutoff frequency of the BandPass Filter */
		BiquadFilterType        = 19,   /**< Filter type of the Biquad Filter */
		BiquadFilterFreq        = 20,   /**< Biquad Filter frequency */
		BiquadFIlterQ           = 21,   /**< Q value of the Biquad Filter */
		BiquadFilterGain        = 22,   /**< Biquad Filter gain */
		EnvelopeAttackTime      = 23,   /**< Envelope Attack Time */
		EnvelopeHoldTime        = 24,   /**< Envelope hold time */
		EnvelopeDecayTime       = 25,   /**< Envelope Decay Time */
		EnvelopeReleaseTime     = 26,   /**< Envelope release time */
		EnvelopeSustainLevel    = 27,   /**< Envelope sustain level */
		StartTime               = 28,   /**< Playback start position */
		Priority                = 31,   /**< Voice Priority */
	}

	/**
	 * <summary>Speaker ID</summary>
	 * <remarks>
	 * <para header='Description'>An ID for specifying the speaker that outputs sound.<br/>
	 * Used in the CriWare.CriAtomExPlayer::SetSendLevel .</para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetSendLevel'/>
	 */
	public enum Speaker {
		FrontLeft           = 0,    /**< Front left speaker */
		FrontRight          = 1,    /**< Front right speaker */
		FrontCenter         = 2,    /**< Front center speaker */
		LowFrequency        = 3,    /**< LFE (≒ subwoofer) */
		SurroundLeft        = 4,    /**< Surround left speaker */
		SurroundRight       = 5,    /**< Surround right speaker */
		SurroundBackLeft    = 6,    /**< Surround back left speaker */
		SurroundBackRight   = 7,    /**< Surround back right speaker */
	}

	/**
	 * <summary>Format type</summary>
	 * <remarks>
	 * <para header='Description'>A data type used to specify the sound format to be played by the AtomExPlayer.<br/>
	 * Used in the CriWare.CriAtomExPlayer::SetFormat .<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetFormat'/>
	 */
	public enum Format : uint {
		ADX         = 0x00000001,       /**< ADX */
		HCA         = 0x00000003,       /**< HCA */
		HCA_MX      = 0x00000004,       /**< HCA-MX */
		WAVE        = 0x00000005,       /**< Wave */
		RAW_PCM     = 0x00000006,       /**< RawPCM */
	}

	private enum SpeakerSystem : System.UInt32 {
		Surround_5_1 = 0,
		Surround_7_1 = 1,
	}

	/**
	 * <summary>Output speaker angle (5.1ch)</summary>
	 * <remarks>
	 * <para header='Description'>Sets the angle (arrangement) of the output speakers used<br/>
	 * when calculating Pan3D or 3D Positioning for virtual speakers.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::SetSpeakerAngle'/>
	 * <seealso cref='CriAtomEx::SetVirtualSpeakerAngle'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct SpeakerAngles6ch {
		public float frontLeft;                /**< Front left speaker */
		public float frontRight;               /**< Front right speaker */
		public float frontCenter;              /**< Front center speaker */
		public float lowFrequency;             /**< LFE (≒ subwoofer) */
		public float surroundLeft;             /**< Surround left speaker */
		public float surroundRight;            /**< Surround right speaker */
		public static SpeakerAngles6ch Default(){
			return new SpeakerAngles6ch
			{
				frontLeft     = -30.0f,
				frontRight    = 30.0f,
				frontCenter   = 0.0f,
				lowFrequency  = 0.0f,
				surroundLeft  = -120.0f,
				surroundRight = 120.0f,
			};
		}
	}

	/**
	 * <summary>Output speaker angle (7.1ch)</summary>
	 * <remarks>
	 * <para header='Description'>Sets the angle (arrangement) of the output speakers used<br/>
	 * when calculating Pan3D or 3D Positioning for virtual speakers.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::SetSpeakerAngle'/>
	 * <seealso cref='CriAtomEx::SetVirtualSpeakerAngle'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct SpeakerAngles8ch {
		public float frontLeft;                /**< Front left speaker */
		public float frontRight;               /**< Front right speaker */
		public float frontCenter;              /**< Front center speaker */
		public float lowFrequency;             /**< LFE (≒ subwoofer) */
		public float surroundLeft;             /**< Surround left speaker */
		public float surroundRight;            /**< Surround right speaker */
		public float surroundBackLeft;         /**< Surround back left speaker */
		public float surroundBackRight;        /**< Surround back right speaker */
		public static SpeakerAngles8ch Default() {
			return new SpeakerAngles8ch
			{
				frontLeft         = -30.0f,
				frontRight        = 30.0f,
				frontCenter       = 0.0f,
				lowFrequency      = 0.0f,
				surroundLeft      = -110.0f,
				surroundRight     = 110.0f,
				surroundBackLeft  = -150.0f,
				surroundBackRight = 150.0f,
			};
		}
	}

	/**
	 * <summary>Speaker angle setting (5.1ch)</summary>
	 * <param name='speakerAngle'>Output speaker angle</param>
	 * <remarks>
	 * <para header='Description'>Sets the angle (arrangement) of the output speakers used when calculating Pan 3D or 3D Positioning. <br/>
	 * Set the angle between -180 degrees and 180 degrees with the forward direction as 0 degrees. <br/></para>
	 * frontLeftとfrontRightの位置を入れ替えるような設定をした場合、意図しない挙動になる可能性があります。
	 * <para header='Note'>Changing the lowFrequency angle does not change the Pan3D or 3D Positioning calculation results. <br/>
	 * The angle is set independently for each speaker system. <br/></para>
	 * </remarks>
	 */
	public static void SetSpeakerAngle(SpeakerAngles6ch speakerAngle) {
		criAtomEx_SetSpeakerAngleArray(SpeakerSystem.Surround_5_1, ref speakerAngle);
	}

	/**
	 * <summary>Speaker angle setting (7.1ch)</summary>
	 * <param name='speakerAngle'>Output speaker angle</param>
	 * <remarks>
	 * <para header='Description'>Sets the angle (arrangement) of the output speakers used when calculating Pan 3D or 3D Positioning. <br/>
	 * Set the angle between -180 degrees and 180 degrees with the forward direction as 0 degrees. <br/></para>
	 * frontLeftとfrontRightの位置を入れ替えるような設定をした場合、意図しない挙動になる可能性があります。
	 * <para header='Note'>Changing the lowFrequency angle does not change the Pan3D or 3D Positioning calculation results. <br/>
	 * The angle is set independently for each speaker system. <br/></para>
	 * </remarks>
	 */
	public static void SetSpeakerAngle(SpeakerAngles8ch speakerAngle) {
		criAtomEx_SetSpeakerAngleArray(SpeakerSystem.Surround_7_1, ref speakerAngle);
	}

	/**
	 * <summary>Virtual speaker angle setting (5.1ch)</summary>
	 * <param name='speakerAngle'>Output speaker angle</param>
	 * <remarks>
	 * <para header='Description'>Sets the angle (arrangement) of the output speakers used<br/>
	 * when calculating Pan3D or 3D Positioning for virtual speakers.<br/></para>
	 * <para header='Note'>The setting in this function will not be applied to the Pan3D or 3D Positioning calculations<br/>
	 * unless the virtual speaker setting is enabled<br/>
	 * by calling the CriWare.CriAtomEx::ControlVirtualSpeakerSetting function. <br/></para>
	 * </remarks>
	 */
	public static void SetVirtualSpeakerAngle(SpeakerAngles6ch speakerAngle) {
		criAtomEx_SetVirtualSpeakerAngleArray(SpeakerSystem.Surround_5_1, ref speakerAngle);
	}

	/**
	 * <summary>Virtual speaker angle setting (7.1ch)</summary>
	 * <param name='speakerAngle'>Output speaker angle</param>
	 * <remarks>
	 * <para header='Description'>Sets the angle (arrangement) of the output speakers used<br/>
	 * when calculating Pan3D or 3D Positioning for virtual speakers.<br/></para>
	 * <para header='Note'>The setting in this function will not be applied to the Pan3D or 3D Positioning calculations<br/>
	 * unless the virtual speaker setting is enabled<br/>
	 * by calling the CriWare.CriAtomEx::ControlVirtualSpeakerSetting function. <br/></para>
	 * </remarks>
	 */
	public static void SetVirtualSpeakerAngle(SpeakerAngles8ch speakerAngle) {
		criAtomEx_SetVirtualSpeakerAngleArray(SpeakerSystem.Surround_7_1, ref speakerAngle);
	}

	/**
	 * <summary>Virtual speaker setting ON / OFF</summary>
	 * <param name='sw'>Switch (False = disable, True = enable)</param>
	 * <remarks>
	 * <para header='Description'>Turn On/Off the function that uses the virtual speaker settings when calculating Pan3D or 3D Positioning.<br/>
	 * When enabled, multi-channel sounds will be played from the virtual speaker angles<br/>
	 * set by the CriWare.CriAtomEx::SetVirtualSpeakerAngle function.</para>
	 * <para header='Note'>The default state is "disabled". <br/>
	 * Also, if any voice is set to "enabled" during playback, the change will not be immediately reflected in the Pan3D or 3D Positioning calculations.
	 * The change will be applied from the playback of the next Voice.</para>
	 * </remarks>
	 */
	public static void ControlVirtualSpeakerSetting(bool sw) {
		criAtomEx_ControlVirtualSpeakerSetting(sw);
	}

	/**
	 * <summary>Audio data format information</summary>
	 * <remarks>
	 * <para header='Description'>Audio data format information.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayback::GetFormatInfo'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct FormatInfo {
		public Format format;           /**< Format type */
		public int samplingRate;        /**< Sampling frequency */
		public long numSamples;         /**< Total number of samples */
		public long loopOffset;         /**< Loop start sample */
		public long loopLength;         /**< The number samples in the loop section */
		public int numChannels;         /**< The number of channels */
		public uint reserved;           /**< Reserved area */
	}

	/**
	 * <summary>A structure for getting the AISAC control information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting the AISAC control information.<br/>
	 * Passed to the CriWare.CriAtomExAcb::GetUsableAisacControl function as an argument.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::GetUsableAisacControl'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct AisacControlInfo {
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string  name;       /**< AISAC control name */
		public AisacControlId   id;         /**< AISAC control ID */

		public AisacControlInfo(byte[] data, int startIndex)
		{
			if (IntPtr.Size == 4) {
				this.name   = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 0)));
				this.id = BitConverter.ToUInt32(data, startIndex + 4);
			} else {
				this.name   = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 0)));
				this.id = BitConverter.ToUInt32(data, startIndex + 8);
			}
		}
	}

	/**
	 * <summary>Gets the method used to calculate the random coordinates of a 3D sound source</summary>
	 * <remarks>
	 * <para header='Description'>This is the definition of the method used to calculate the random coordinates of a 3D sound source.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::Randomize3dConfig'/>
	 */
	public enum Randomize3dCalcType : int
	{
		None = -1,               /**< No settings */
		Rectangle = 0,           /**< Rectangle */
		Cuboid = 1,              /**< Cuboid */
		Circle = 2,              /**< Circle */
		Cylinder = 3,            /**< Cylinder */
		Sphere = 4,              /**< Sphere */
		List = 6                 /**< List of coordinates */
	}

	/**
	 * <summary>Definition of the parameters used to calculate coordinates for the randomization of the position of 3D sound sources</summary>
	 * <remarks>
	 * <para header='Description'>Definition of the parameters that determine the calculation of a random position (i.e., shape indicating the coordinates space).<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::randomize3dParamTable'/>
	 * <seealso cref='CriAtomEx::Randomize3dConfig::GetParamByType'/>
	 * <seealso cref='CriAtomEx::Randomize3dConfig::SetParamByType'/>
	 */
	public enum Randomize3dParamType
	{
		None,       /**< Unused */
		Width,      /**< Width */
		Depth,      /**< Depth */
		Height,     /**< Height */
		Radius      /**< Radius */
	}

	static public readonly Dictionary<Randomize3dCalcType, Randomize3dParamType[]> randomize3dParamTable = new Dictionary<Randomize3dCalcType, Randomize3dParamType[]>()
	{
		{ Randomize3dCalcType.None,      new Randomize3dParamType[]{ Randomize3dParamType.None,   Randomize3dParamType.None,   Randomize3dParamType.None } },
		{ Randomize3dCalcType.Rectangle, new Randomize3dParamType[]{ Randomize3dParamType.Width,  Randomize3dParamType.Depth,  Randomize3dParamType.None } },
		{ Randomize3dCalcType.Cuboid,    new Randomize3dParamType[]{ Randomize3dParamType.Width,  Randomize3dParamType.Depth,  Randomize3dParamType.Height } },
		{ Randomize3dCalcType.Circle,    new Randomize3dParamType[]{ Randomize3dParamType.Radius, Randomize3dParamType.None,   Randomize3dParamType.None } },
		{ Randomize3dCalcType.Cylinder,  new Randomize3dParamType[]{ Randomize3dParamType.Radius, Randomize3dParamType.Height, Randomize3dParamType.None } },
		{ Randomize3dCalcType.Sphere,    new Randomize3dParamType[]{ Randomize3dParamType.Radius, Randomize3dParamType.None,   Randomize3dParamType.None } },
		{ Randomize3dCalcType.List,      new Randomize3dParamType[]{ Randomize3dParamType.None,   Randomize3dParamType.None,   Randomize3dParamType.None } },
	};

	/**
	 * <summary>Config structure used to randomize the positions of the 3D sound sources.</summary>
	 * <remarks>
	 * <para header='Description'>This is the structure that contains the settings related to the randomization of the 3D sound source's position. <br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::Randomize3dCalcType'/>
	 */
	[Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Randomize3dConfig
	{
		/* static */
		public const int NumOfCalcParams = 3; 
		
		/* fields */
		[UnityEngine.SerializeField]
		private bool followsOriginalSource;             /**< Whether to follow the position and orientation of the 3D sound source */
		[UnityEngine.SerializeField]
		private Randomize3dCalcType calculationType;    /**< Shape (calculation method of coordinates) */
		[UnityEngine.SerializeField, MarshalAs(UnmanagedType.ByValArray, SizeConst = NumOfCalcParams)]
		private float[] calculationParameters;          /**< Various parameters for the shape (coordinates calculation method) */

		/* properties */
		/** <summary> Returns whether the position and orientation of the 3D sound source must be followed (read only) </summary> */
		public bool FollowsOriginalSource { get { return followsOriginalSource; } }
		/** <summary> Gets the shape used for 3D sound source randomization (read only) </summary> */
		public Randomize3dCalcType CalculationType { get { return calculationType; } }
		/** <summary> Gets Parameter 1 related to the shape used for 3D sound source randomization (read only) </summary> */
		public float CalculationParameter1 { get { return calculationParameters[0]; } }
		/** <summary> Gets Parameter 2 related to the shape used for 3D sound source randomization (read only) </summary> */
		public float CalculationParameter2 { get { return calculationParameters[1]; } }
		/** <summary> Gets Parameter 3 related to the shape used for 3D sound source randomization (read only) </summary> */
		public float CalculationParameter3 { get { return calculationParameters[2]; } }

		internal Randomize3dConfig(byte[] data, int startIndex)
		{
			this.followsOriginalSource = BitConverter.ToInt32(data, startIndex + 0) != 0;
			this.calculationType = (Randomize3dCalcType)BitConverter.ToInt32(data, startIndex + 4);
			this.calculationParameters = new float[NumOfCalcParams];
			for (int i = 0; i < NumOfCalcParams; ++i) {
				calculationParameters[i] = BitConverter.ToSingle(data, startIndex + 8 + (4 * i));
			}
		}

		/** <summary> Initializes the settings related to 3D sound source position randomization </summary> */
		public Randomize3dConfig(bool followsOriginalSource, Randomize3dCalcType calculationType, float param1 = 0, float param2 = 0, float param3 = 0)
		{
			this.followsOriginalSource = followsOriginalSource;
			this.calculationType = calculationType;
			this.calculationParameters = new float[NumOfCalcParams];
			this.calculationParameters[0] = param1;
			this.calculationParameters[1] = param2;
			this.calculationParameters[2] = param3;
		}

		public Randomize3dConfig(int dummy) /* for initiating */
		{
			followsOriginalSource = false;
			calculationType = Randomize3dCalcType.Rectangle;
			this.calculationParameters = new float[NumOfCalcParams];
			this.ClearCalcParams();
		}

		/** <summary> Sets all parameters to the specified values. Sets all parameters to 0 if no argument is passed </summary> */
		public void ClearCalcParams(float initVal = 0f)
		{
			for (int i = 0; i < NumOfCalcParams; ++i) {
				calculationParameters[i] = initVal;
			}
		}
		
		/** <summary> Gets the value of a parameter by specifying the type </summary> */
		public bool GetParamByType(Randomize3dParamType paramType, ref float paramVal)
		{
			int index = Array.IndexOf(randomize3dParamTable[calculationType], paramType);
			if (index < 0) {
				UnityEngine.Debug.LogWarningFormat("[CRIWARE] Parameter {0} not available for 3d randomize calculation type {1}", paramType.ToString(), calculationType.ToString());
				return false;
			}
			paramVal = calculationParameters[index];
			return true;
		}

		/** <summary> Sets the parameter by specifying the type </summary> */
		public bool SetParamByType(Randomize3dParamType paramType, float paramVal)
		{
			int index = Array.IndexOf(randomize3dParamTable[calculationType], paramType);
			if (index < 0) {
				UnityEngine.Debug.LogWarningFormat("[CRIWARE] Parameter {0} not available for 3d randomize calculation type {1}", paramType.ToString(), calculationType.ToString());
				return false;
			}
			calculationParameters[index] = paramVal;
			return true;
		}
	}

	/**
	 * <summary>The 3D information of the Cue</summary>
	 * <remarks>
	 * <para header='Description'>Waveform information is the 3D details of the Cue.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::CueInfo'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct CuePos3dInfo
	{
		public float coneInsideAngle;                            /**< Cone internal angle */
		public float coneOutsideAngle;                           /**< Cone external angle */
		public float minAttenuationDistance;                     /**< Minimum attenuation distance */
		public float maxAttenuationDistance;                     /**< Maximum attenuation distance */
		public float sourceRadius;                               /**< Zero distance InteriorPan applicable distance */
		public float interiorDistance;                           /**< InteriorPan application boundary distance */
		public float dopplerFactor;                              /**< Doppler coefficient */
		public Randomize3dConfig randomPos;                      /**< Configuration information of the 3D sound source randomization */
		public AisacControlId distanceAisacControl;              /**< Distance attenuation AISAC control */
		public AisacControlId listenerBaseAngleAisacControl;     /**< Listener reference angle AISAC control */
		public AisacControlId sourceBaseAngleAisacControl;       /**< Sound source reference angle AISAC control */
		public AisacControlId listenerBaseElevationAisacControl; /**< Listener reference elevation AISAC control */
		public AisacControlId sourceBaseElevationAisacControl;   /**< Sound source reference elevation AISAC control */

		public CuePos3dInfo(byte[] data, int startIndex)
		{
			this.coneInsideAngle = BitConverter.ToSingle(data, startIndex + 0);
			this.coneOutsideAngle = BitConverter.ToSingle(data, startIndex + 4);
			this.minAttenuationDistance = BitConverter.ToSingle(data, startIndex + 8);
			this.maxAttenuationDistance = BitConverter.ToSingle(data, startIndex + 12);
			this.sourceRadius = BitConverter.ToSingle(data, startIndex + 16);
			this.interiorDistance = BitConverter.ToSingle(data, startIndex + 20);
			this.dopplerFactor = BitConverter.ToSingle(data, startIndex + 24);
			this.randomPos = new Randomize3dConfig(data, startIndex + 28);
			this.distanceAisacControl = BitConverter.ToUInt32(data, startIndex + 48);
			this.listenerBaseAngleAisacControl = BitConverter.ToUInt32(data, startIndex + 52);
			this.sourceBaseAngleAisacControl = BitConverter.ToUInt32(data, startIndex + 56);
			this.listenerBaseElevationAisacControl = BitConverter.ToUInt32(data, startIndex + 60);
			this.sourceBaseElevationAisacControl = BitConverter.ToUInt32(data, startIndex + 64);
		}
	}

	/**
	 * <summary> Structure for acquiring game variable information</summary>
	 * <remarks>
	 * <para header='Description'>This is the structure used to get information on a game variable. <br/>
	 * Pass it to the CriWare.CriAtomEx::GetGameVariableInfo function as an argument. <br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::GetGameVariableInfo'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct GameVariableInfo
	{
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string  name;       /**< Game variable name */
		public uint             id;         /**< Game variable ID */
		public float            gameValue;  /**< Game variable value */

		public GameVariableInfo(byte[] data, int startIndex)
		{
			if (IntPtr.Size == 4) {
				this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 0)));
				this.id = BitConverter.ToUInt32(data, startIndex + 4);
				this.gameValue  = BitConverter.ToSingle(data, startIndex + 8);
			} else {
				this.name   = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 0)));
				this.id = BitConverter.ToUInt32(data, startIndex + 8);
				this.gameValue  = BitConverter.ToSingle(data, startIndex + 12);
			}
		}

		public GameVariableInfo(string name, uint id, float gameValue)
		{
			this.name       = name;
			this.id         = id;
			this.gameValue  = gameValue;
		}
	}

	/**
	 * <summary>Cue type</summary>
	 * <seealso cref='CriAtomEx::CueInfo'/>
	 */
	public enum CueType
	{
		Polyphonic,             /**< Polyphonic */
		Sequential,             /**< Sequential */
		Shuffle,                /**< Shuffle playback */
		Random,                 /**< Random */
		RandomNoRepeat,         /**< Random discontinued (randomly play sounds other than the one played previously) */
		SwitchGameVariable,     /**< Switch playback (switches tracks to be played by referencing the game variable) */
		ComboSequential,        /**< Combo Sequential (Sequential when consecutive combo is successful within the "combo time", and returns to "combo loopback" point when the end is reached) */
		SwitchSelector,         /**< Selector */
		TrackTransitionBySelector,
	}

	/**
	 * <summary>Cue information</summary>
	 * <remarks>
	 * <para header='Description'>Detailed information of the Cue.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::GetCueInfo'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct CueInfo
	{
		public int              id;                         /**< Cue ID */
		public CueType          type;                       /**< Type */
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string  name;                       /**< Cue name */
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string  userData;                   /**< User data */
		public long             length;                     /**< Length (msec) */

		/* 最大再生毎カテゴリ参照数:CRIATOMEXCATEGORY_MAX_CATEGORIES_PER_PLAYBACKは16 */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public ushort[]         categories;                 /**< Category index */

		public short            numLimits;                  /**< Cue limit */
		public ushort           numBlocks;                  /**< Number of Blocks */
		public ushort           numTracks;                  /**< The number of tracks */
		public ushort           numRelatedWaveForms;        /**< The number of related waveforms */
		public byte             priority;                   /**< Category Cue Priority */
		public byte             headerVisibility;           /**< Header publication flag */
		public byte             ignore_player_parameter;    /**< Player parameter invalidation flag */
		public byte             probability;                /**< Playback probability */
		public PanType          panType;                    /**< Panning type */
		public CuePos3dInfo     pos3dInfo;                  /**< 3D information */
		public GameVariableInfo gameVariableInfo;           /**< Game variables */

		public CueInfo(byte[] data, int startIndex)
		{
			if (IntPtr.Size == 4) {
				this.id = BitConverter.ToInt32(data, startIndex + 0);
				this.type   = (CueType)BitConverter.ToInt32(data, startIndex + 4);
				this.name   = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 8)));
				this.userData   = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 12)));
				this.length = BitConverter.ToInt64(data, startIndex + 16);
				this.categories = new ushort[16];
				for (int i = 0; i < 16; ++i) {
					categories[i] = BitConverter.ToUInt16(data, startIndex + 24 + (2 * i));
				}
				this.numLimits  = BitConverter.ToInt16(data, startIndex + 56);
				this.numBlocks  = BitConverter.ToUInt16(data, startIndex + 58);
				this.numTracks  = BitConverter.ToUInt16(data, startIndex + 60);
				this.numRelatedWaveForms        = BitConverter.ToUInt16(data, startIndex + 62);
				this.priority                   = data[startIndex + 64];
				this.headerVisibility           = data[startIndex + 65];
				this.ignore_player_parameter    = data[startIndex + 66];
				this.probability                = data[startIndex + 67];
				this.panType                    = (PanType)BitConverter.ToInt32(data, startIndex + 68);
				this.pos3dInfo  = new CuePos3dInfo(data, startIndex + 72);
				this.gameVariableInfo   = new GameVariableInfo(data, startIndex + 140);
			} else {
				this.id = BitConverter.ToInt32(data, startIndex + 0);
				this.type   = (CueType)BitConverter.ToInt32(data, startIndex + 4);
				this.name   = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 8)));
				this.userData   = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 16)));
				this.length = BitConverter.ToInt64(data, startIndex + 24);
				this.categories = new ushort[16];
				for (int i = 0; i < 16; ++i) {
					categories[i] = BitConverter.ToUInt16(data, startIndex + 32 + (2 * i));
				}
				this.numLimits  = BitConverter.ToInt16(data, startIndex + 64);
				this.numBlocks  = BitConverter.ToUInt16(data, startIndex + 66);
				this.numTracks  = BitConverter.ToUInt16(data, startIndex + 68);
				this.numRelatedWaveForms        = BitConverter.ToUInt16(data, startIndex + 70);
				this.priority                   = data[startIndex + 72];
				this.headerVisibility           = data[startIndex + 73];
				this.ignore_player_parameter    = data[startIndex + 74];
				this.probability                = data[startIndex + 75];
				this.panType                    = (PanType)BitConverter.ToInt32(data, startIndex + 76);
				this.pos3dInfo  = new CuePos3dInfo(data, startIndex + 80);
				/* padded by 4 bytes */
				this.gameVariableInfo   = new GameVariableInfo(data, startIndex + 152);
			}
		}
	}

	/**
	 * <summary>Sound waveform information</summary>
	 * <remarks>
	 * <para header='Description'>Waveform information is a detailed information about the sound waveform played from each Cue.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAcb::GetWaveformInfo'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct WaveformInfo
	{
		public int      waveId;             /**< Waveform data ID */
		public uint     format;             /**< Format type */
		public int      samplingRate;       /**< Sampling frequency */
		public int      numChannels;        /**< The number of channels */
		public long     numSamples;         /**< Total number of samples */
		public bool     streamingFlag;      /**< Streaming flag */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public uint[]   reserved;           /**< Reserved area */

		public WaveformInfo(byte[] data, int startIndex)
		{
			this.waveId = BitConverter.ToInt32(data, startIndex + 0);
			this.format = BitConverter.ToUInt32(data, startIndex + 4);
			this.samplingRate   = BitConverter.ToInt32(data, startIndex + 8);
			this.numChannels    = BitConverter.ToInt32(data, startIndex + 12);
			this.numSamples = BitConverter.ToInt64(data, startIndex + 16);
			this.streamingFlag  = BitConverter.ToInt32(data, startIndex + 24) != 0;
			this.reserved   = new uint[1];
			for (int i = 0; i < 1; ++i) {
				reserved[i] = BitConverter.ToUInt32(data, startIndex + 28 + (4 * i));
			}
		}
	}

	/**
	 * <summary>A structure for getting the AISAC information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting the AISAC information.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::GetAttachedAisacInfo'/>
	 * <seealso cref='CriAtomExCategory::GetAttachedAisacInfoById'/>
	 * <seealso cref='CriAtomExCategory::GetAttachedAisacInfoByName'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct AisacInfo
	{
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string name;            /**< AISAC name */
		public bool defaultControlFlag;         /**< Whether the default control value is set */
		public float defaultControlValue;       /**< Default AISAC control value */
		public AisacControlId controlId;        /**< ControlId */
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string controlName;     /**< ControlName */

		public AisacInfo(byte[] data, int startIndex)
		{
			if (IntPtr.Size == 4) {
				this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 0)));
				this.defaultControlFlag = BitConverter.ToInt32(data, startIndex + 4) != 0;
				this.defaultControlValue = BitConverter.ToSingle(data, startIndex + 8);
				this.controlId = BitConverter.ToUInt32(data, startIndex + 12);
				this.controlName = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt32(data, startIndex + 16)));
			} else {
				this.name = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 0)));
				this.defaultControlFlag = BitConverter.ToInt32(data, startIndex + 8) != 0;
				this.defaultControlValue = BitConverter.ToSingle(data, startIndex + 12);
				this.controlId = BitConverter.ToUInt32(data, startIndex + 16);
				this.controlName = Marshal.PtrToStringAnsi(new IntPtr(BitConverter.ToInt64(data, startIndex + 20)));
			}
		}
	}

	/**
	 * <summary>Performance information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting the performance information.<br/>
	 * Used in the CriWare.CriAtomEx::GetPerformanceInfo .</para>
	 * </remarks>
	 * <seealso cref='CriWare.CriAtomEx::GetPerformanceInfo'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct PerformanceInfo
	{
		public uint serverProcessCount;     /**< The number of server process executions */
		public uint lastServerTime;         /**< Last measured server process time (in microseconds) */
		public uint maxServerTime;          /**< Maximum server process time (in microseconds) */
		public uint averageServerTime;      /**< Average server process time (in microseconds) */
		public uint lastServerInterval;     /**< Last measured server process execution interval (in microseconds) */
		public uint maxServerInterval;      /**< Maximum server process execution interval (in microseconds) */
		public uint averageServerInterval;  /**< Average server process execution interval (in microseconds) */
	}

	/**
	 * <summary>Usage of various resources</summary>
	 * <remarks>
	 * <para header='Description'>A structure indicating the usage status of various resources.</para>
	 * </remarks>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct ResourceUsage
	{
		public uint useCount;       /**< Current usage of the target resource */
		public uint limit;          /**< Limit for target resources */
	}

	/**
	 * <summary>3D vector structure</summary>
	 * <remarks>
	 * <para header='Description'>A structure that represents the position, direction, etc. in 3D space.</para>
	 * </remarks>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct NativeVector
	{
		public float x;
		public float y;
		public float z;

		public NativeVector(float x, float y, float z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public NativeVector(UnityEngine.Vector3 vector) {
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
		}
	}

	/**
	 * <summary>Information about the Cue Link callback</summary>
	 * <remarks>
	 * <para header='Description'>A structure for retrieving information from Cue Link callbacks.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::OnCueLinkCallback'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct CueLinkInfo
	{
		public IntPtr nativePlayerHn;   /**< Player handle */
		public uint basePlaybackId;     /**< Playback ID of the link's source */
		public uint targetPlaybackId;   /**< Playback ID of the link's target */
		public int cueLinkType;         /**< Type of the Cue Link callback(0:static, 1:dynamic) */
	}

	/**
	 * <summary>Cue Link callback</summary>
	 * <param name='info'>Information about the Cue Link callback</param>
	 * <remarks>
	 * <para header='Description'>This is the delegate type for Cue Link callbacks.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::OnCueLinkCallback'/>
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void CueLinkCbFunc(ref CueLinkInfo info);

	/**
	 * <summary>Registering Cue Link callbacks</summary>
	 * <remarks>
	 * <para header='Description'>Registers a callback function that receives Cue Link information when processing a Cue Link
	 * during the Cue's playback.<br/>
	 * The registered callback function is executed when the application's main thread updates
	 * right after the callback event is processed.<br/></para>
	 * </remarks>
	 */
	public static event CueLinkCbFunc OnCueLinkCallback {
		add {
			CriAtom.OnCueLinkCallback += value;
		}
		remove {
			CriAtom.OnCueLinkCallback -= value;
		}
	}

	/**
	 * <summary>Registers the ACF file</summary>
	 * <param name='binder'>Binder</param>
	 * <param name='acfPath'>File path of the ACF file</param>
	 * <remarks>
	 * <para header='Description'>Loads the ACF file and incorporates it into the library.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::UnregisterAcf'/>
	 */
	public static void RegisterAcf(CriFsBinder binder, string acfPath)
	{
		IntPtr binderHandle = (binder != null) ? binder.nativeHandle : IntPtr.Zero;
		criAtomEx_RegisterAcfFile(binderHandle, acfPath, IntPtr.Zero, 0);
	}

	/**
	 * <summary>Registers the ACF data</summary>
	 * <param name='acfData'>ACF data</param>
	 * <param name='dataSize'>ACF data size</param>
	 * <remarks>
	 * <para header='Description'>Loads the ACF data placed on the memory and captures it to the library.<br/></para>
	 * <para header='Note'>The buffer address of the data to be passed as an argument should be fixed beforehand
	 * by the application so that it is not moved to the garbage collector.<br/>
	 * Also, release the memory lock after unregistering the ACF file or after finalizing the library.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::UnregisterAcf'/>
	 */
	public static void RegisterAcf(IntPtr acfData, int dataSize)
	{
		criAtomEx_RegisterAcfData(acfData, dataSize, IntPtr.Zero, 0);
	}

	/**
	 * \deprecated
	 * This is a deprecated API that will be removed.
	 * Please consider using CriAtomEx.RegisterAcf(IntPtr) instead.
	*/
	[Obsolete("Use RegisterAcf(IntPtr) instead")]
	public static void RegisterAcf(byte[] acfData) {
		criAtomEx_RegisterAcfData(acfData, acfData.Length, IntPtr.Zero, 0);
	}

	/**
	 * <summary>Unregisters the ACF file</summary>
	 * <remarks>
	 * <para header='Description'>Unregisters the ACF file.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::RegisterAcf'/>
	 */
	public static void UnregisterAcf()
	{
		criAtomEx_UnregisterAcf();
	}

	/**
	 * <summary>Gets the snapshot's name</summary>
	 * <remarks>
	 * <para header='Description'>Gets the current snapshot name. Returns null if none was set.<br/></para>
	 * </remarks>
	 */
	public static string GetAppliedDspBusSnapshotName()
	{
		string snapshotName = null;
		IntPtr ptr = criAtomEx_GetAppliedDspBusSnapshotName();
		if (ptr == IntPtr.Zero) {
			return null;
		}
		snapshotName = Marshal.PtrToStringAnsi(ptr);
		return snapshotName;
	}

	/**
	 * <summary>Attaching the DSP bus settings</summary>
	 * <param name='settingName'>Name of the DSP bus setting</param>
	 * <remarks>
	 * <para header='Description'>Builds a DSP bus from the DSP bus settings and attaches it to the sound renderer.<br/>
	 * Before calling this function, you must register the ACF information
	 * using the CriAtomEx::RegisterAcf function<br/><code>
	 *      ：
	 *  // ACFファイルの読み込みと登録
	 *  CriAtomEx.RegisterAcf("Sample.acf");
	 *
	 *  // DSPバス設定の適用
	 *  CriAtomEx.AttachDspBusSetting("DspBusSetting_0");
	 *      ：
	 * </code>
	 * </para>
	 * <para header='Note'>This function is a return-on-complete function.<br/>
	 * Calling this function blocks the server processing of the Atom library for a while.<br/>
	 * If this function is called during sound playback, problems such as sound interruption may occur,
	 * so call this function at a timing when load fluctuations is accepted such as when switching scenes.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::DetachDspBusSetting'/>
	 * <seealso cref='CriAtomEx::RegisterAcf'/>
	 */
	public static void AttachDspBusSetting(string settingName)
	{
		criAtomEx_AttachDspBusSetting(settingName, IntPtr.Zero, 0);
	}

	/**
	 * <summary>Detaches the DSP bus settings</summary>
	 * <remarks>
	 * <para header='Description'>Detaches the DSP bus settings.<br/></para>
	 * <para header='Note'>This function is a return-on-complete function.<br/>
	 * Calling this function blocks the server processing of the Atom library for a while.<br/>
	 * If this function is called during sound playback, problems such as sound interruption may occur,
	 * so call this function at a timing when load fluctuations is accepted such as when switching scenes.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::AttachDspBusSetting'/>
	 */
	public static void DetachDspBusSetting()
	{
		criAtomEx_DetachDspBusSetting();
	}

	/**
	 * <summary>Applies the DSP bus snapshot</summary>
	 * <param name='snapshot_name'>DSP bus snapshot name</param>
	 * <param name='time_ms'>The time (in milliseconds) until the snapshot is fully reflected</param>
	 * <remarks>
	 * <para header='Description'>Applies a DSP bus snapshot.<br/>
	 * Calling this function changes the snapshot parameters.
	 * It takes time_ms milliseconds for the change to complete.</para>
	 * </remarks>
	 */
	public static void ApplyDspBusSnapshot(string snapshot_name, int time_ms)
	{
		criAtomEx_ApplyDspBusSnapshot(snapshot_name, time_ms);
	}

	/**
	 * <summary>Gets the total number of game variables</summary>
	 * <returns>The total number of game variables</returns>
	 * <remarks>
	 * <para header='Description'>Gets the total number of game variables registered in the ACF file.<br/></para>
	 * <para header='Note'>It is necessary to register the ACF file before calling this function.<br/>
	 * If the ACF file is not registered, -1 is returned.</para>
	 * </remarks>
	 */
	public static int GetNumGameVariables()
	{
		return criAtomEx_GetNumGameVariables();
	}

	/**
	 * <summary>Gets game variable information (index specified)</summary>
	 * <param name='index'>Game variable index</param>
	 * <param name='info'>Game variable information</param>
	 * <returns>Whether the information could be obtained</returns>
	 * <remarks>
	 * <para header='Description'>Gets the game variable information from the game variable index.<br/>
	 * Returns False if there is no game variable with the specified index.</para>
	 * <para header='Note'>It is necessary to register the ACF file before calling this function.<br/></para>
	 * </remarks>
	 */
	public static bool GetGameVariableInfo(ushort index, out GameVariableInfo info)
	{
		using (var mem = new CriStructMemory<GameVariableInfo>()) {
			bool result = criAtomEx_GetGameVariableInfo(index, mem.ptr);
			info = new GameVariableInfo(mem.bytes, 0);
			return result;
		}
	}

	/**
	 * <summary>Gets game variables</summary>
	 * <param name='game_variable_id'>Game variable ID</param>
	 * <returns>Game variable value</returns>
	 * <remarks>
	 * <para header='Description'>Gets the value of the game variable registered in the ACF file.<br/></para>
	 * <para header='Note'>It is necessary to register the ACF file before calling this function.<br/></para>
	 * </remarks>
	 */
	public static float GetGameVariable(uint game_variable_id)
	{
		return criAtomEx_GetGameVariableById(game_variable_id);
	}

	/**
	 * <summary>Gets game variables</summary>
	 * <param name='game_variable_name'>Game variable name</param>
	 * <returns>Game variable value</returns>
	 * <remarks>
	 * <para header='Description'>Gets the value of the game variable registered in the ACF file.<br/></para>
	 * <para header='Note'>It is necessary to register the ACF file before calling this function.<br/></para>
	 * </remarks>
	 */
	public static float GetGameVariable(string game_variable_name)
	{
		return criAtomEx_GetGameVariableByName(game_variable_name);
	}

	/**
	 * <summary>Sets game variables</summary>
	 * <param name='game_variable_id'>Game variable ID</param>
	 * <param name='game_variable_value'>Game variable value</param>
	 * <remarks>
	 * <para header='Description'>Sets the value to the game variable registered in the ACF file.<br/>
	 * Acceptable values are between 0.0f and 1.0f.</para>
	 * <para header='Note'>It is necessary to register the ACF file before calling this function.<br/></para>
	 * </remarks>
	 */
	public static void SetGameVariable(uint game_variable_id, float game_variable_value)
	{
		criAtomEx_SetGameVariableById(game_variable_id, game_variable_value);
	}

	/**
	 * <summary>Sets game variables</summary>
	 * <param name='game_variable_name'>Game variable name</param>
	 * <param name='game_variable_value'>Game variable value</param>
	 * <remarks>
	 * <para header='Description'>Sets the value to the game variable registered in the ACF file.<br/>
	 * Acceptable values are between 0.0f and 1.0f.</para>
	 * <para header='Note'>It is necessary to register the ACF file before calling this function.<br/></para>
	 * </remarks>
	 */
	public static void SetGameVariable(string game_variable_name, float game_variable_value)
	{
		criAtomEx_SetGameVariableByName(game_variable_name, game_variable_value);
	}

	/**
	 * <summary>Sets the random number seed</summary>
	 * <param name='seed'>Random number seed</param>
	 * <remarks>
	 * <para header='Description'>Sets the random number seed in the pseudo random number generator shared in entire CRI Atom library.<br/>
	 * By setting the random number seed, it is possible to add reproducibility to various random playback processes.<br/>
	 * If you want reproducibility in each AtomExPlayer, use the CriWare.CriAtomExPlayer::SetRandomSeed function.
	 * <br/>
	 * If you do not need reproducibility and want to change the random number seed for each execution,
	 * use the CriWare.CriAtomConfig::useRandomSeedWithTime property instead of this function.
	 * <br/></para>
	 * <para header='Note'>This function must be called before generating CriWare.CriAtomSource or CriWare.CriAtomExPlayer .
	 * Those created before setting the random number seed are not affected.</para>
	 * </remarks>
	 * <seealso cref='CriWare.CriAtomExPlayer::SetRandomSeed'/>
	 */
	public static void SetRandomSeed(uint seed)
	{
		criAtomEx_SetRandomSeed(seed);
	}

	/**
	 * <summary>Resets the performance monitor</summary>
	 * <remarks>
	 * <para header='Description'>Discards the measurement results up to now.<br/>
	 * The performance monitor starts collecting performance information immediately after
	 * initializing the library and accumulates the measurement results.<br/>
	 * If you do not want to include the previous measurement result in the future measurement,
	 * you need to call this function to discard the accumulated measurement.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::GetPerformanceInfo'/>
	 */
	public static void ResetPerformanceMonitor()
	{
		criAtom_ResetPerformanceMonitor();
	}

	/**
	 * <summary>Gets the performance information</summary>
	 * <remarks>
	 * <para header='Description'>Gets the performance information.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::PerformanceInfo'/>
	 * <seealso cref='CriAtomEx::ResetPerformanceMonitor'/>
	 */
	public static void GetPerformanceInfo(out PerformanceInfo info)
	{
		criAtom_GetPerformanceInfo(out info);
	}

	/**
	 * <summary>Sets the global reference label for a selector</summary>
	 * <param name='selector_index'>Selector index</param>
	 * <param name='label_index'>Label index</param>
	 * <remarks>
	 * <para header='Description'>Set the globally referenced label for the selector registered in the ACF file.<br/></para>
	 * <para header='Note'>It is necessary to register the ACF file before calling this function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::SetGlobalLabelToSelectorByName'/>
	 */
	public static void SetGlobalLabelToSelectorByIndex(ushort selector_index, ushort label_index)
	{
		criAtomExAcf_SetGlobalLabelToSelectorByIndex(selector_index, label_index);
	}

	/**
	 * <summary>Sets the global reference label for a selector</summary>
	 * <param name='selector_name'>Selector name</param>
	 * <param name='label_name'>Label name</param>
	 * <remarks>
	 * <para header='Description'>Set the globally referenced label for the selector registered in the ACF file.<br/></para>
	 * <para header='Note'>It is necessary to register the ACF file before calling this function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::SetGlobalLabelToSelectorByIndex'/>
	 */
	public static void SetGlobalLabelToSelectorByName(string selector_name, string label_name)
	{
		criAtomExAcf_SetGlobalLabelToSelectorByName(selector_name, label_name);
	}

	/**
	 * <summary>Prevents interruption to the server process</summary>
	 * <remarks>
	 * <para header='Description'>Prevents the lock of interruption to server processing.<br/>
	 * The interruption to the server processing is prevented after this function is called until the CriWare.CriAtomEx::Unlock function is called.<br/>
	 * If you want to reliably call multiple APIs in the same audio frame, use this function to prevent interruption
	 * to the server processing and then call those functions.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::Unlock'/>
	 */
	public static void Lock()
	{
		criAtomEx_Lock();
	}

	/**
	 * <summary>Allow interruption to the server process</summary>
	 * <remarks>
	 * <para header='Description'>Do not prevent the interruption of the server processing (disable the effect of the CriWare.CriAtomEx::Lock function).</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::Lock'/>
	 */
	public static void Unlock()
	{
		criAtomEx_Unlock();
	}

	/**
	 * <summary>[PC] Output device setting(by device ID)</summary>
	 * <param name='deviceId'>Device ID</param>
	 * <remarks>
	 * <para header='Description'>Sets the ID of the device to be used as Atom's audio output destination.</para>
	 * </remarks>
	 */
	public static void SetOutputAudioDevice_PC(string deviceId)
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		string audioDevice;
		if (string.IsNullOrEmpty(deviceId) || deviceId.Contains("{")) {
			audioDevice = deviceId;
		} else {
			audioDevice = "{0.0.0.00000000}.{" + deviceId + "}";
		}
		criAtom_SetDeviceId_WASAPI(SoundRendererType.Native, audioDevice);
#endif
	}

	/**
	 * <summary>[PC] Initializing the list of output devices</summary>
	 * <remarks>
	 * <para header='Description'>Initializes the list of devices which can be used as Atom's audio output destination.<br/>
	 * After calling this function, the number of devices can be obtained from CriAtomEx::GetNumAudioDevices_PC,
	 * and the names of the devices from CriAtomEx::GetAudioDeviceName_PC.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::GetNumAudioDevices_PC'/>
	 * <seealso cref='CriAtomEx::GetAudioDeviceName_PC'/>
	 */
	public static bool LoadAudioDeviceList_PC()
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		return criAtomUnity_LoadAudioDeviceList_PC();
#else
		return false;
#endif
	}

	/**
	 * <summary>[PC] Getting the number of the output devices</summary>
	 * <returns>Amount of output devices</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of devices that can be selected as the audio output destination from Atom.<br/>
	 * Please call this function after using
	 * CriAtomEx::LoadAudioDeviceList_PC function to initialize the device list inside the plugin.
	 * The maximum return value is 32.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::LoadAudioDeviceList_PC'/>
	 */
	public static int GetNumAudioDevices_PC()
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		return criAtomUnity_GetNumAudioDevices_PC();
#else
		return 0;
#endif
	}

	/**
	 * <summary>[PC] Getting the output device's name(by index)</summary>
	 * <param name='index'>Device index</param>
	 * <returns>Output device's name</returns>
	 * <remarks>
	 * <para header='Description'>Gets the names of devices that can be selected as the audio output destination from Atom.<br/>
	 * Please call this function after using
	 * CriAtomEx::LoadAudioDeviceList_PC function to initialize the device list inside the plugin.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::LoadAudioDeviceList_PC'/>
	 */
	public static string GetAudioDeviceName_PC(int index)
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		IntPtr ptr = criAtomUnity_GetAudioDeviceName_PC(index);

		if (ptr == IntPtr.Zero) {
			return null;
		}

		string ret = null;
		try {
			ret = Marshal.PtrToStringAnsi(ptr);
		} catch (Exception) {
			ret = null;
		}
		if (string.IsNullOrEmpty(ret)) {
			try {
				var array = new byte[1024];
				Marshal.Copy(ptr, array, 0, 1024);
				var encoding = Encoding.GetEncoding("shift-jis");
				int deviceNameLength = 0;
				for (int i = 0; i < array.Length; i++) {
					if (array[i] == '\0') {
						deviceNameLength = i;
						break;
					}
				}
				ret = encoding.GetString(array, 0, deviceNameLength);
			} catch (Exception exception) {
				if (exception is System.NotSupportedException) {
					throw;
				}
				ret = null;
			}
		}
		return ret;
#else
		return null;
#endif
	}

	/**
	 * <summary>[PC] Output device setting(by index)</summary>
	 * <param name='index'>Device index</param>
	 * <remarks>
	 * <para header='Description'>Sets the device to be used as Atom's audio output destination.<br/>
	 * Please call this function after using
	 * CriAtomEx::LoadAudioDeviceList_PC function to initialize the device list inside the plugin.</para>
	 * </remarks>
	 * <seealso cref='CriAtomEx::LoadAudioDeviceList_PC'/>
	 */
	public static void SetOutputAudioDevice_PC(int index)
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		IntPtr id = criAtomUnity_GetAudioDeviceId_PC(index);
		criAtom_SetDeviceId_WASAPI(SoundRendererType.Native, id);
#endif
	}

	/* @cond excludele */
	/**
	 * <summary>[VITA] Specifies output volume</summary>
	 * <remarks>
	 * <para header='Description'>Specify the final output volume (master volume) of the sound.<br/>
	 * The volume value specified in this function is multiplied to all the sounds played by the Atom library.<br/>
	 * The default volume is 1.0f.<br/></para>
	 * <para header='Note'>This function is an API dedicated VITA.</para>
	 * </remarks>
	 */
	public static void SetOutputVolume_VITA(float volume)
	{
	#if !UNITY_EDITOR && UNITY_PSP2
		criAtom_SetOutputVolume_VITA(volume);
	#endif
	}

	/**
	 * <summary>[VITA] Checks the BGM output right</summary>
	 * <remarks>
	 * <para header='Description'>Checks the BGM output right.<br/>
	 * If True, it means that you have a right to output BGM, and that no other application is using the BGM port.<br/>
	 * If False, it means that you cannot acquire the BGM output right, and that another application is using the BGM port.<br/>
	 * <br/>
	 * After checking the BGM output right with this function, request the BGM output right or mute the BGM sound if necessary.<br/></para>
	 * <para header='Note'>This function is an API dedicated VITA.</para>
	 * </remarks>
	 */
	public static bool IsBgmPortAcquired_VITA()
	{
	#if !UNITY_EDITOR && UNITY_PSP2
		return criAtomUnity_IsBgmPortAcquired_VITA();
	#else
		return true;
	#endif
	}
	/* @endcond */

	/**
	 * <summary>[iOS] Confirms stoppage of sound output</summary>
	 * <remarks>
	 * <para header='Description'>Checks if the sound output is stopped.<br/>
	 * If True, the sound output is stopped.<br/>
	 * If this function returns True even if the application is not paused,
	 * the sound output is blocked by a system interrupt etc. that is not detected by the application.<br/>
	 * For synchronizing the application with the sound, check the sound output status
	 * using this function, and add pausing if necessary.</para>
	 * <para header='Note'>This function is an API dedicated iOS.</para>
	 * </remarks>
	 */
	public static bool IsSoundStopped_IOS()
	{
	#if !UNITY_EDITOR && UNITY_IOS
		return criAtomUnity_IsSoundStopped_IOS();
	#else
		return false;
	#endif
	}

	/**
	 * <summary>[iOS] Enables AudioSession Restoration</summary>
	 * <param name='flag'>Whether to enable the restoration process</param>
	 * <remarks>
	 * <para header='Description'>Sets whether to automatically restore the audio after interruptions by other applications or the OS.<br/>
	 * When the audio output is performed exclusively (i.e. CriAtomConfig.iosOverrideIPodMusic is true),
	 * this setting will determine whether to resume the audio after interruptions from other applications.<br/>
	 * It is enabled by default, which means audio will immediately resume after an interruption.<br/>
	 * You can set the argument to false and call this function when you want to temporarily disable the audio restoration,
	 * when dealing with one of these situations: 
	 * - When you want to support voice input when the user enters information<br/>
	 * - When you want to temporarily use a module that offers audio playback, other than CRIWARE (example: video playback by WebView)<br/>
	 * Make sure to call this function again with "true" for the argument once you are done, in order to avoid that
	 * the audio of the application does not resume.<br/></para>
	 * <para header='Note'>This function is an API dedicated iOS.</para>
	 * </remarks>
	 */
	public static void EnableAudioSessionRestoration_IOS(bool flag)
	{
	#if !UNITY_EDITOR && UNITY_IOS
		criAtomUnity_EnableAudioSessionRestoration_IOS(flag);
	#endif
	}

#if !UNITY_EDITOR && UNITY_ANDROID
	public static SoundRendererType androidDefaultSoundRendererType = SoundRendererType.Default;
#endif

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomEx_RegisterAcfFile(
		IntPtr binder, string path, IntPtr work, int workSize);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_RegisterAcfData(
		IntPtr acfData, int acfDataSize, IntPtr work, int workSize);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_RegisterAcfData(
		byte[] acfData, int acfDataSize, IntPtr work, int workSize);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_UnregisterAcf();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_AttachDspBusSetting(
		string settingName, IntPtr work, int workSize);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_DetachDspBusSetting();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_ApplyDspBusSnapshot(string snapshot_name, int time_ms);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomEx_GetAppliedDspBusSnapshotName();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomEx_GetNumGameVariables();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomEx_GetGameVariableInfo(ushort index, IntPtr game_variable_info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern float criAtomEx_GetGameVariableById(uint game_variable_id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern float criAtomEx_GetGameVariableByName(string game_variable_name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_SetGameVariableById(uint game_variable_id, float game_variable_value);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_SetGameVariableByName(string game_variable_name, float game_variable_value);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_SetRandomSeed(uint seed);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_Lock();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_Unlock();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtom_ResetPerformanceMonitor();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtom_GetPerformanceInfo(out PerformanceInfo info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAcf_SetGlobalLabelToSelectorByIndex(ushort selector_index, ushort label_index);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAcf_SetGlobalLabelToSelectorByName(string selector_name, string label_name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_SetSpeakerAngleArray(SpeakerSystem speaker_system, ref SpeakerAngles6ch angle_array);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_SetSpeakerAngleArray(SpeakerSystem speaker_system, ref SpeakerAngles8ch angle_array);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_SetVirtualSpeakerAngleArray(SpeakerSystem speaker_system, ref SpeakerAngles6ch angle_array);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_SetVirtualSpeakerAngleArray(SpeakerSystem speaker_system, ref SpeakerAngles8ch angle_array);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomEx_ControlVirtualSpeakerSetting(bool sw);

	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtom_SetDeviceId_WASAPI(SoundRendererType soundRendererType, [MarshalAs(UnmanagedType.LPWStr)]string deviceId);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtom_SetDeviceId_WASAPI(CriAtomEx.SoundRendererType type, IntPtr deviceId);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomUnity_LoadAudioDeviceList_PC();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomUnity_GetNumAudioDevices_PC();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomUnity_GetAudioDeviceName_PC(int index);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern IntPtr criAtomUnity_GetAudioDeviceId_PC(int index);
	#endif

	#if !UNITY_EDITOR && UNITY_PSP2
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtom_SetOutputVolume_VITA(float volume);
	#endif

	#if !UNITY_EDITOR && UNITY_PSP2
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomUnity_IsBgmPortAcquired_VITA();
	#endif

	#if !UNITY_EDITOR && UNITY_ANDROID
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void criAtom_EnableSlLatencyCheck_ANDROID(bool sw);
	#endif

	#if !UNITY_EDITOR && UNITY_ANDROID
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern int criAtom_GetSlBufferConsumptionLatency_ANDROID();
	#endif

	#if !UNITY_EDITOR && UNITY_IOS
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern bool criAtomUnity_IsSoundStopped_IOS();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void criAtomUnity_EnableAudioSessionRestoration_IOS(bool flag);
	#endif

	#if !UNITY_EDITOR && UNITY_SWITCH
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void criAtom_SetServerThreadAffinityMask_SWITCH(ulong mask);
	#endif
	#else
	private static bool criAtomEx_RegisterAcfFile(
		IntPtr binder, string path, IntPtr work, int workSize) { return true; }
	private static void criAtomEx_RegisterAcfData(
		IntPtr acfData, int acfDataSize, IntPtr work, int workSize) { }
	private static void criAtomEx_RegisterAcfData(
		byte[] acfData, int acfDataSize, IntPtr work, int workSize) { }
	private static void criAtomEx_UnregisterAcf() { }
	private static void criAtomEx_AttachDspBusSetting(
		string settingName, IntPtr work, int workSize) { }
	private static void criAtomEx_DetachDspBusSetting() { }
	private static void criAtomEx_ApplyDspBusSnapshot(string snapshot_name, int time_ms) { }
	private static IntPtr criAtomEx_GetAppliedDspBusSnapshotName() { return IntPtr.Zero; }
	private static int criAtomEx_GetNumGameVariables() { return 0; }
	private static bool criAtomEx_GetGameVariableInfo(ushort index, IntPtr game_variable_info) { return false; }
	private static float criAtomEx_GetGameVariableById(uint game_variable_id) { return 0.0f; }
	private static float criAtomEx_GetGameVariableByName(string game_variable_name) { return 0.0f; }
	private static void criAtomEx_SetGameVariableById(uint game_variable_id, float game_variable_value) { }
	private static void criAtomEx_SetGameVariableByName(string game_variable_name, float game_variable_value) { }
	private static void criAtomEx_SetRandomSeed(uint seed) { }
	private static void criAtomEx_Lock() { }
	private static void criAtomEx_Unlock() { }
	private static void criAtom_ResetPerformanceMonitor() { }
	private static void criAtom_GetPerformanceInfo(out PerformanceInfo info) { info = new PerformanceInfo(); }
	private static void criAtomExAcf_SetGlobalLabelToSelectorByIndex(ushort selector_index, ushort label_index) { }
	private static void criAtomExAcf_SetGlobalLabelToSelectorByName(string selector_name, string label_name) { }
	private static void criAtomEx_SetSpeakerAngleArray(SpeakerSystem speaker_system, ref SpeakerAngles6ch angle_array) { }
	private static void criAtomEx_SetSpeakerAngleArray(SpeakerSystem speaker_system, ref SpeakerAngles8ch angle_array) { }
	private static void criAtomEx_SetVirtualSpeakerAngleArray(SpeakerSystem speaker_system, ref SpeakerAngles6ch angle_array) { }
	private static void criAtomEx_SetVirtualSpeakerAngleArray(SpeakerSystem speaker_system, ref SpeakerAngles8ch angle_array) { }
	private static void criAtomEx_ControlVirtualSpeakerSetting(bool sw) { }
	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	private static void criAtom_SetDeviceId_WASAPI(SoundRendererType soundRendererType, string deviceId) { }
	private static void criAtom_SetDeviceId_WASAPI(CriAtomEx.SoundRendererType type, IntPtr deviceId) { }
	private static bool criAtomUnity_LoadAudioDeviceList_PC() { return false; }
	private static int criAtomUnity_GetNumAudioDevices_PC() { return 0; }
	private static IntPtr criAtomUnity_GetAudioDeviceName_PC(int index) { return IntPtr.Zero; }
	private static IntPtr criAtomUnity_GetAudioDeviceId_PC(int index) { return IntPtr.Zero; }
	#endif
	#if !UNITY_EDITOR && UNITY_PSP2
	private static void criAtom_SetOutputVolume_VITA(float volume) { }
	#endif
	#if !UNITY_EDITOR && UNITY_PSP2
	private static bool criAtomUnity_IsBgmPortAcquired_VITA() { return false; }
	#endif
	#if !UNITY_EDITOR && UNITY_ANDROID
	public static void criAtom_EnableSlLatencyCheck_ANDROID(bool sw) { }
	#endif
	#if !UNITY_EDITOR && UNITY_ANDROID
	public static int criAtom_GetSlBufferConsumptionLatency_ANDROID() { return 0; }
	#endif
	#if !UNITY_EDITOR && UNITY_IOS
	public static bool criAtomUnity_IsSoundStopped_IOS() { return false; }
	public static void criAtomUnity_EnableAudioSessionRestoration_IOS(bool flag) { }
	#endif
	#if !UNITY_EDITOR && UNITY_SWITCH
	public static void criAtom_SetServerThreadAffinityMask_SWITCH(ulong mask) { }
	#endif
	#endif
	#endregion
}

/**
 * <summary>A class for controlling parameters for each Category.</summary>
 * <remarks>
 * <para header='Description'>A class for controlling parameters for each Category.<br/></para>
 * </remarks>
 */
public static class CriAtomExCategory
{
	/**
	 * <summary>REACT type</summary>
	 * <remarks>
	 * <para header='Description'>The type of REACT.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::GetReactParameter'/>
	 * <seealso cref='CriAtomExCategory::SetReactParameter'/>
	 */
	public enum ReactType : int
	{
		Ducker = 0,                     /**< Ducker */
		AisacModulationTrigger,         /**< AISAC modulation trigger */
	}

	/**
	 * <summary>Ducking target by REACT</summary>
	 * <remarks>
	 * <para header='Description'>Ducking target type by REACT.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::GetReactParameter'/>
	 * <seealso cref='CriAtomExCategory::SetReactParameter'/>
	 */
	public enum ReactDuckerTargetType : int
	{
		Volume = 0,                     /**< Volume ducker */
		AisacControlValue,              /**< Ducker of the AISAC control value */
	}

	/**
	 * <summary>Ducking Curve Type by REACT</summary>
	 * <remarks>
	 * <para header='Description'>Ducking Curve Type by REACT.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::GetReactParameter'/>
	 * <seealso cref='CriAtomExCategory::SetReactParameter'/>
	 */
	public enum ReactDuckerCurveType : int
	{
		Linear = 0,                     /**< straight line */
		Square,                         /**< Slow transition */
		SquareReverse,                  /**< high speed change */
		SCurve,                         /**< S curve */
		FlatAtHalf,                     /**< Inverted S curve */
	}

	/**
	 * <summary>REACT fade parameter structure</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting/setting the fade drive parameter information of REACT.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::GetReactParameter'/>
	 * <seealso cref='CriAtomExCategory::SetReactParameter'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct ReactFadeParameter
	{
		public ReactDuckerCurveType     curveType;      /**< Curve type */
		public float                    curveStrength;  /**< Curve strength (0.0f - 2.0f) */
		public System.UInt16            fadeTimeMs;     /**< Fading time (milliseconds) */
	}

	/**
	 * <summary>REACT hold type</summary>
	 * <remarks>
	 * <para header='Description'>REACT hold type (for maintaining the decay time).<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::GetReactParameter'/>
	 * <seealso cref='CriAtomExCategory::SetReactParameter'/>
	 */
	public enum ReactHoldType
	{
		WhilePlaying,                   /**< Do holding while playing */
		FixedTime,                      /**< Hold for a fixed time */
	}

	/**
	 * <summary>Ducker parameter structure by REACT</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting/setting the ducker driving parameter information by REACT.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::GetReactParameter'/>
	 * <seealso cref='CriAtomExCategory::SetReactParameter'/>
	 */
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
	public struct ReactDuckerParameter
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct Volume
		{
            public float level;             /**< Attenuation volume level */
		}
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct AisacControl
		{
			public AisacControlId id;       /**< AISAC control ID */
			public float value;             /**< AISAC control value */
		}
		[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
		public struct Target
		{
			[FieldOffset(0)]
			public Volume volume;
			[FieldOffset(0)]
			public AisacControl aisacControl;
		}
        [FieldOffset(0)]
        public Target target;
        [FieldOffset(8)]
        public ReactDuckerTargetType targetType;    /**< Target of the Ducker */
        [FieldOffset(12)]
        public ReactFadeParameter entry;            /**< Fading parameter when start to change */
        [FieldOffset(24)]
        public ReactFadeParameter exit;             /**< Fading parameter when finished changing */
        [FieldOffset(36)]
        public ReactHoldType holdType;              /**< Hold type */
        [FieldOffset(40)]
        public System.UInt16 holdTimeMs;            /**< Hold time (milliseconds) */
	}

	/**
	 * <summary>AISAC modulation trigger parameter structure</summary>
	 * <remarks>
	 * <para header='Description'>A structure for setting/getting the driving parameter information for AISAC modulation trigger.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::GetReactParameter'/>
	 * <seealso cref='CriAtomExCategory::SetReactParameter'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct ReactAisacModulationParameter
	{
		private System.Int32 _enableDecrementAisacModulationKey;
		public System.UInt32 decrementAisacModulationKey;            /**< Decrement AISAC modulation key */
		private System.Int32 _enableIncrementAisacModulationKey;
		public System.UInt32 incrementAisacModulationKey;            /**< Increment AISAC modulation key */
		public bool enableDecrementAisacModulationKey                /**< Whether the decrement AISAC modulation key is enabled */
		{
			get {return _enableDecrementAisacModulationKey != 0 ? true : false;}
		}
		public bool enableIncrementAisacModulationKey                /**< Whether the increment AISAC modulation key is enabled */
		{
			get {return _enableIncrementAisacModulationKey != 0 ? true : false; }
		}
	}

	/**
	 * <summary>REACT drive parameter structure</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting/setting the drive parameter information of REACT.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::GetReactParameter'/>
	 * <seealso cref='CriAtomExCategory::SetReactParameter'/>
	 */
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
	public struct ReactParameter
	{
		[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
		public struct Parameter
		{
			[FieldOffset(0)]
			public ReactDuckerParameter ducker;                     /**< Ducker parameters */
			[FieldOffset(0)]
			public ReactAisacModulationParameter aisacModulation;   /**< AISAC modulation trigger parameter */
		}
        [FieldOffset(0)]
        public Parameter parameter;
        [FieldOffset(44)]
        public ReactType type;              /**< REACT type */
        [FieldOffset(48)]
        public bool enablePausingCue;       /**< Whether the paused Cue is applied or not */
	}

	/**
	 * <summary>Execution status of REACT</summary>
	 * <remarks>
	 * <para header='Description'>This is the value indicating the execution status of REACT<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::GetReactStatus'/>
	 */
	public enum ReactStatus
	{
		Stop = 0,    /**< Stopped */
		FadeOut,     /**< Execution starting */
		Hold,        /**< Executing */
		FadeIn,      /**< Execution ending */
		Error,       /**< Error */
	}

	/**
	 * <summary>Sets volume for the Category by specifying the name</summary>
	 * <param name='name'>Category name</param>
	 * <param name='volume'>Volume value</param>
	 * <remarks>
	 * <para header='Description'>Sets the volume for the Category by specifying the name.</para>
	 * </remarks>
	 */
	public static void SetVolume(string name, float volume)
	{
		criAtomExCategory_SetVolumeByName(name, volume);
	}

	/**
	 * <summary>Sets volume for the Category by specifying the ID</summary>
	 * <param name='id'>Category ID</param>
	 * <param name='volume'>Volume value</param>
	 * <remarks>
	 * <para header='Description'>Sets the volume for the Category by specifying the ID.</para>
	 * </remarks>
	 */
	public static void SetVolume(int id, float volume)
	{
		criAtomExCategory_SetVolumeById(id, volume);
	}

	/**
	 * <summary>Gets the Category volume by specifying the name</summary>
	 * <param name='name'>Category name</param>
	 * <returns>Category volume</returns>
	 * <remarks>
	 * <para header='Description'>Gets the volume value applied to the Category by specifying the name.</para>
	 * </remarks>
	 */
	public static float GetVolume(string name)
	{
		return criAtomExCategory_GetVolumeByName(name);
	}

	/**
	 * <summary>Gets the Category volume by specifying the ID</summary>
	 * <param name='id'>Category ID</param>
	 * <returns>Category volume</returns>
	 * <remarks>
	 * <para header='Description'>Gets the volume value applied to the Category by specifying the ID.</para>
	 * </remarks>
	 */
	public static float GetVolume(int id)
	{
		return criAtomExCategory_GetVolumeById(id);
	}

	/**
	 * <summary>Sets the Category mute status by specifying the name</summary>
	 * <param name='name'>Category name</param>
	 * <param name='mute'>Mute status (False = unmute, True = mute)</param>
	 * <remarks>
	 * <para header='Description'>Sets the mute status of the Category by specifying the name.</para>
	 * </remarks>
	 */
	public static void Mute(string name, bool mute)
	{
		criAtomExCategory_MuteByName(name, mute);
	}

	/**
	 * <summary>Sets the Category mute status by specifying the ID</summary>
	 * <param name='id'>Category ID</param>
	 * <param name='mute'>Mute status (False = unmute, True = mute)</param>
	 * <remarks>
	 * <para header='Description'>Sets the mute status of the Category by specifying the ID.</para>
	 * </remarks>
	 */
	public static void Mute(int id, bool mute)
	{
		criAtomExCategory_MuteById(id, mute);
	}

	/**
	 * <summary>Gets the Category mute status by specifying the name</summary>
	 * <param name='name'>Category name</param>
	 * <returns>Mute status (False = not muted, True = muted)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the mute status of the Category by specifying the name.</para>
	 * </remarks>
	 */
	public static bool IsMuted(string name)
	{
		return criAtomExCategory_IsMutedByName(name);
	}

	/**
	 * <summary>Gets the Category mute status by specifying the ID</summary>
	 * <param name='id'>Category ID</param>
	 * <returns>Mute status (False = not muted, True = muted)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the mute status of the Category by specifying the ID.</para>
	 * </remarks>
	 */
	public static bool IsMuted(int id)
	{
		return criAtomExCategory_IsMutedById(id);
	}

	/**
	 * <summary>Sets the solo status of the Category by specifying the name</summary>
	 * <param name='name'>Category name</param>
	 * <param name='solo'>Solo status (False = un-solo, True = solo)</param>
	 * <param name='muteVolume'>The mute volume value applied to other Categories</param>
	 * <remarks>
	 * <para header='Description'>Set the Category solo status by specifying the name.<br/>
	 * The volume specified by muteVolume
	 * is applied to the Categories belonging to the same Category group.</para>
	 * </remarks>
	 */
	public static void Solo(string name, bool solo, float muteVolume)
	{
		criAtomExCategory_SoloByName(name, solo, muteVolume);
	}

	/**
	 * <summary>Sets the solo status of the Category by specifying the ID</summary>
	 * <param name='id'>Category ID</param>
	 * <param name='solo'>Solo status (False = un-solo, True = solo)</param>
	 * <param name='muteVolume'>The mute volume value applied to other Categories</param>
	 * <remarks>
	 * <para header='Description'>Sets the solo status of the Category by specifying the ID.<br/>
	 * The volume specified by muteVolume
	 * is applied to the Categories belonging to the same Category group.</para>
	 * </remarks>
	 */
	public static void Solo(int id, bool solo, float muteVolume)
	{
		criAtomExCategory_SoloById(id, solo, muteVolume);
	}

	/**
	 * <summary>Gets the solo status of the Category by specifying the name</summary>
	 * <param name='name'>Category name</param>
	 * <returns>Solo status (False = not solo, True = solo)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the solo status of the Category by specifying the name.</para>
	 * </remarks>
	 */
	public static bool IsSoloed(string name)
	{
		return criAtomExCategory_IsSoloedByName(name);
	}

	/**
	 * <summary>Gets the solo status of the Category by specifying the ID</summary>
	 * <param name='id'>Category ID</param>
	 * <returns>Solo status (False = not solo, True = solo)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the solo status of the Category by specifying the ID.</para>
	 * </remarks>
	 */
	public static bool IsSoloed(int id)
	{
		return criAtomExCategory_IsSoloedById(id);
	}

	/**
	 * <summary>Pauses/unpauses the Category by specifying the name</summary>
	 * <param name='name'>Category name</param>
	 * <param name='pause'>Switch (False = unpause, True = pause)</param>
	 * <remarks>
	 * <para header='Description'>Pauses/unpauses the Category by specifying the name.<br/>
	 * The specification is the same as the CriWare.CriAtomExCategory::Pause(int, bool) function, except that
	 * the Category is specified by the name.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::Pause(int, bool)'/>
	 */
	public static void Pause(string name, bool pause)
	{
		criAtomExCategory_PauseByName(name, pause);
	}

	/**
	 * <summary>Pauses/unpauses the Category by specifying the ID</summary>
	 * <param name='id'>Category ID</param>
	 * <param name='pause'>Switch (False = unpause, True = pause)</param>
	 * <remarks>
	 * <para header='Description'>Pauses/unpauses the Category by specifying the ID.<br/></para>
	 * <para header='Note'>Pausing a Category is treated independently of pausing
	 * the AtomExPlayer or playback sound (pause with CriWare.CriAtomExPlayer::Pause or CriWare.CriAtomExPlayback::Pause function),
	 * and the final pause status of a sound is determined by considering these pause states.<br/>
	 * In other words, paused if either of them is paused, and unpaused if all of the are unpaused.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::Pause(string, bool)'/>
	 */
	public static void Pause(int id, bool pause)
	{
		criAtomExCategory_PauseById(id, pause);
	}

	/**
	 * <summary>Gets the pause status of the Category by specifying the ID</summary>
	 * <param name='name'>Category ID</param>
	 * <returns>Paused status (False = not paused, True = paused)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the pause status of the Category by specifying the ID.</para>
	 * </remarks>
	 */
	public static bool IsPaused(string name)
	{
		return criAtomExCategory_IsPausedByName(name);
	}

	/**
	 * <summary>Pauses/unpauses the Category by specifying the name</summary>
	 * <param name='id'>Category name</param>
	 * <returns>Paused status (False = not paused, True = paused)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the pause status of the Category by specifying the name.</para>
	 * </remarks>
	 */
	public static bool IsPaused(int id)
	{
		return criAtomExCategory_IsPausedById(id);
	}

	/**
	 * <summary>Sets the AISAC control value for the Category by specifying the name</summary>
	 * <param name='name'>Category name</param>
	 * <param name='controlName'>AISAC control name</param>
	 * <param name='value'>AISAC control value</param>
	 * <remarks>
	 * <para header='Description'>Sets the AISAC control value for the Category by specifying the name.<br/>
	 * The specification is the same as the CriWare.CriAtomExCategory::SetAisacControl function, except that
	 * the Category and AISAC control are specified by the name.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::SetAisacControl(int, int, float)'/>
	 */
	public static void SetAisacControl(string name, string controlName, float value)
	{
		criAtomExCategory_SetAisacControlByName(name, controlName, value);
	}

	/**
	 * \deprecated
	 * This is a deprecated API that will be removed.
	 * Please consider using CriAtomExCategory.SetAisacControl instead.
	*/
	[System.Obsolete("Use CriAtomExCategory.SetAisacControl")]
	public static void SetAisac(string name, string controlName, float value)
	{
		SetAisacControl(name, controlName, value);
	}

	/**
	 * <summary>Sets the AISAC control value for the Category by specifying the ID</summary>
	 * <param name='id'>Category ID</param>
	 * <param name='controlId'>AISAC control ID</param>
	 * <param name='value'>AISAC control value</param>
	 * <remarks>
	 * <para header='Description'>Sets the AISAC control value for the Category by specifying the ID.<br/></para>
	 * <para header='Note'>For the AISACs set to a Cue or Track,
	 * the <b>AISAC control value set to the Category is given priority</b>
	 * over the AISAC control value setting to the player<br/>
	 * For the AISACs attached to the Category,
	 * <b>only the AISACcontrol value set to the Category</b> is always referenced.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::SetAisacControl(string, string, float)'/>
	 */
	public static void SetAisacControl(int id, int controlId, float value)
	{
		criAtomExCategory_SetAisacControlById(id, (ushort)controlId, value);
	}

	/**
	 * \deprecated
	 * This is a deprecated API that will be removed.
	 * Please consider using CriAtomExCategory.SetAisacControl instead.
	*/
	[System.Obsolete("Use CriAtomExCategory.SetAisacControl")]
	public static void SetAisac(int id, int controlId, float value)
	{
		SetAisacControl(id, controlId, value);
	}

	/**
	 * <summary>Sets the REACT drive parameter</summary>
	 * <param name='name'>REACT name</param>
	 * <param name='parameter'>REACT drive parameter structure</param>
	 * <remarks>
	 * <para header='Description'>Sets the parameters that drive REACT.<br/></para>
	 * <para header='Note'>Parameters cannot be set while REACT is running (warning is issued).<br/>
	 * If you specify a REACT name that does not exist, an error callback is returned.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::GetReactParameter'/>
	 * <seealso cref='CriAtomExCategory::GetReactStatus'/>
	 */
	public static void SetReactParameter(string name, ReactParameter parameter)
	{
		criAtomExCategory_SetReactParameter(name, ref parameter);
	}

	/**
	 * <summary>Gets the REACT drive parameters</summary>
	 * <param name='name'>REACT name</param>
	 * <param name='parameter'>REACT drive parameter structure</param>
	 * <remarks>
	 * <para header='Description'>Gets the current value of the parameter that drives REACT.<br/></para>
	 * <para header='Note'>If you specify a REACT name that does not exist, an error callback is called and CRI_False is returned.<br/>
	 * If you specify a REACT name that does not exist, an error callback is returned.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExCategory::SetReactParameter'/>
	 */
	public static bool GetReactParameter(string name, out ReactParameter parameter)
	{
		return criAtomExCategory_GetReactParameter(name, out parameter);
	}

	/**
	 * <summary>Gets the AISAC information by specifying the ID</summary>
	 * <param name='id'>Category ID</param>
	 * <param name='aisacAttachedIndex'>Index of the attached AISAC</param>
	 * <param name='aisacInfo'>A structure for getting the AISAC information</param>
	 * <remarks>
	 * <para header='Description'>Gets the information on the AISAC attached to the Category by specifying the ID.<br/></para>
	 * <para header='Note'>If you specify a Category that does not exist, or if you specify an invalid index, False is returned.<br/></para>
	 * </remarks>
	 */
	public static bool GetAttachedAisacInfoById(uint id, int aisacAttachedIndex, out CriAtomEx.AisacInfo aisacInfo)
	{
		using (var mem = new CriStructMemory<CriAtomEx.AisacInfo>()) {
			bool result = criAtomExCategory_GetAttachedAisacInfoById(id, aisacAttachedIndex, mem.ptr);
			if (result) {
				aisacInfo = new CriAtomEx.AisacInfo(mem.bytes, 0);
			} else {
				aisacInfo = new CriAtomEx.AisacInfo();
			}
			return result;
		}
	}

	/**
	 * <summary>Gets the AISAC information by specifying the name</summary>
	 * <param name='name'>Category name</param>
	 * <param name='aisacAttachedIndex'>Index of the attached AISAC</param>
	 * <param name='aisacInfo'>A structure for getting the AISAC information</param>
	 * <remarks>
	 * <para header='Description'>Gets the information on the AISAC attached to the Category by specifying the name.<br/></para>
	 * <para header='Note'>If you specify a Category that does not exist, or if you specify an invalid index, False is returned.<br/></para>
	 * </remarks>
	 */
	public static bool GetAttachedAisacInfoByName(string name, int aisacAttachedIndex, out CriAtomEx.AisacInfo aisacInfo)
	{
		using (var mem = new CriStructMemory<CriAtomEx.AisacInfo>()) {
			bool result = criAtomExCategory_GetAttachedAisacInfoByName(name, aisacAttachedIndex, mem.ptr);
			if (result) {
				aisacInfo = new CriAtomEx.AisacInfo(mem.bytes, 0);
			} else {
				aisacInfo = new CriAtomEx.AisacInfo();
			}
			return result;
		}
	}


	/**
	 * <summary>Gets the AISAC information by specifying the name</summary>
	 * <param name='categoryName'>Category name</param>
	 * <param name='aisacControlName'>AISAC control name</param>
	 * <param name='controlValue'>Current value of the AISAC control</param>
	 * <returns>Was the current value acquired? (Acquired: true / Unable to acquire: false)</returns>
	 * \par説明:
	 * カテゴリにアタッチされているAISACコントロールの現在値を取得します。<br/>
	 * \attention
	 * 存在しないカテゴリを指定した場合や、無効なインデックスを指定した場合、falseが返ります。<br/>
	 */
	public static bool GetCurrentAisacControlValue(string categoryName, string aisacControlName, out float controlValue) {
		return criAtomExCategory_GetCurrentAisacControlValueByName(categoryName, aisacControlName, out controlValue);
	}

	/**
	 * <summary>Gets the execution status of REACT<br/></summary>
	 * <param name='reactName'>REACT name</param>
	 * <returns>Execution status of REACT</returns>
	 * <remarks>
	 * <para header='Description'>Gets the execution status of REACT<br/></para>
	 * <para header='Note'>If a non-existing REACT name is specified, an error callback will be triggered and ReactStatus.Error will be returned.</para>
	 * </remarks>
	 */
	public static ReactStatus GetReactStatus(string reactName)
	{
		return criAtomExCategory_GetReactStatus(reactName);
	}

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExCategory_SetVolumeByName(string name, float volume);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern float criAtomExCategory_GetVolumeByName(string name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExCategory_SetVolumeById(int id, float volume);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern float criAtomExCategory_GetVolumeById(int id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExCategory_MuteById(int id, bool mute);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExCategory_IsMutedById(int id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExCategory_MuteByName(string name, bool mute);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExCategory_IsMutedByName(string name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExCategory_SoloById(int id, bool solo, float volume);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExCategory_IsSoloedById(int id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExCategory_SoloByName(string name, bool solo, float volume);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExCategory_IsSoloedByName(string name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExCategory_PauseById(int id, bool pause);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExCategory_IsPausedById(int id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExCategory_PauseByName(string name, bool pause);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExCategory_IsPausedByName(string name);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExCategory_SetAisacControlById(int id, ushort controlId, float value);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExCategory_SetAisacControlByName(string name, string controlName, float value);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExCategory_SetReactParameter(string react_name, ref ReactParameter parameter);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExCategory_GetReactParameter(string react_name, out ReactParameter parameter);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExCategory_GetAttachedAisacInfoById(uint id, int aisacAttachedIndex, IntPtr aisacInfo);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExCategory_GetAttachedAisacInfoByName(string name, int aisacAttachedIndex, IntPtr aisacInfo);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExCategory_GetCurrentAisacControlValueByName(string category_name, string aisac_control_name, out float control_value);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern ReactStatus criAtomExCategory_GetReactStatus(string react_name);

	#else
	private static void criAtomExCategory_SetVolumeByName(string name, float volume) { }
	private static float criAtomExCategory_GetVolumeByName(string name) { return 1.0f; }
	private static void criAtomExCategory_SetVolumeById(int id, float volume) { }
	private static float criAtomExCategory_GetVolumeById(int id) { return 1.0f; }
	private static void criAtomExCategory_MuteById(int id, bool mute) { }
	private static bool criAtomExCategory_IsMutedById(int id) { return false; }
	private static void criAtomExCategory_MuteByName(string name, bool mute) { }
	private static bool criAtomExCategory_IsMutedByName(string name) { return false; }
	private static void criAtomExCategory_SoloById(int id, bool solo, float volume) { }
	private static bool criAtomExCategory_IsSoloedById(int id) { return false; }
	private static void criAtomExCategory_SoloByName(string name, bool solo, float volume) { }
	private static bool criAtomExCategory_IsSoloedByName(string name) { return false; }
	private static void criAtomExCategory_PauseById(int id, bool pause) { }
	private static bool criAtomExCategory_IsPausedById(int id) { return false; }
	private static void criAtomExCategory_PauseByName(string name, bool pause) { }
	private static bool criAtomExCategory_IsPausedByName(string name) { return false; }
	private static void criAtomExCategory_SetAisacControlById(int id, ushort controlId, float value) { }
	private static void criAtomExCategory_SetAisacControlByName(string name, string controlName, float value) { }
	private static void criAtomExCategory_SetReactParameter(string name, ref ReactParameter parameter) { }
	private static bool criAtomExCategory_GetReactParameter(string name, out ReactParameter parameter) { parameter = new ReactParameter();
																										 return false; }
	private static bool criAtomExCategory_GetAttachedAisacInfoById(uint id, int aisacAttachedIndex, IntPtr aisacInfo) { return false; }
	private static bool criAtomExCategory_GetAttachedAisacInfoByName(string name, int aisacAttachedIndex, IntPtr aisacInfo) { return false; }
	private static bool criAtomExCategory_GetCurrentAisacControlValueByName(string category_name, string aisac_control_name, out float control_value) { control_value = 0.0f; return false;}
	private static ReactStatus criAtomExCategory_GetReactStatus(string react_name) { return ReactStatus.Error; }
	#endif

	#endregion
}

/**
 * <summary>A class for controlling the Sequence data.</summary>
 * <remarks>
 * <para header='Description'>A class for using the Sequence data created on CRI Atom Craft.<br/></para>
 * </remarks>
 */
public static class CriAtomExSequencer
{
	/**
	 * <summary>Structure to get information about a sequence event</summary>
	 * <remarks>
	 * <para header='Description'>This is the structure used to get information about a sequence event. <br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExSequencer::EventCallback'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct CriAtomExSequenceEventInfo {
		public ulong position;
		public IntPtr playerHn;
		[MarshalAs(UnmanagedType.LPStr)]
		public readonly string tag;
		public uint playbackId;
		private int type;
		public uint id;
		private uint reserved;
	}
	/**
	 * <summary>Sequence callback (string argument)</summary>
	 * <param name='eventParamsString'>Event parameter string</param>
	 * <remarks>
	 * <para header='Description'>A Sequence callback function type.<br/>
	 * The argument string contains the following information:<br/>
	 *  -# Event position
	 *  -# Event ID
	 *  -# Playback ID
	 *  -# Event type
	 *  -# Event tag string
	 *  .
	 * Each piece of information is concatenated and passed as one string with the specified delimiter character in between.<br/>
	 * Parse the required parameters from the string before use.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExSequencer::SetEventCallback'/>
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void EventCbFunc(string eventParamsString);

	/**
	 * <summary>Sequence callback</summary>
	 * <param name='criAtomExSequenceInfo'>>Event information structure</param>
	 * <remarks>
	 * <para header='Description'>Sequence callback function type. <br/>
	 * The structure passed as argument contains the following information: <br/>
	 *  -# Event position
	 *  -# Player handler pointer
	 *  -# Data embedded string
	 *  -# Playback ID
	 *  -# Event type
	 *  -# Data embedded value
	 *  .</para>
	 * </remarks>
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void EventCallback(ref CriAtomExSequenceEventInfo criAtomExSequenceInfo);

	/**
	 * <summary>Registers the Sequence event callback</summary>
	 * <remarks>
	 * <para header='Description'>Registers a callback function to be executed by the callback marker synchronization embedded in the Cue. <br/>
	 * The registered callback function will be executed when the application main thread is updated
	 * immediately after processing the callback event.<br/></para>
	 * </remarks>
	 */
	public static event EventCallback OnCallback {
		add {
			CriAtom.OnEventSequencerCallback += value;
		}
		remove {
			CriAtom.OnEventSequencerCallback -= value;
		}
	}

	/**
	 * \deprecated
	 * 削除予定の非推奨APIです。
	 * CriAtomExSequencer.OnCallback event の使用を検討してください。
	 * <summary>Registering sequence event callbacks (for string argument delegates)</summary>
	 * <param name='func'>Sequence callback function</param>
	 * <param name='separator'>Event parameter delimiting string (up to 15 characters)</param>
	 * <remarks>
	 * <para header='Description'>Registers the callback function that receives the callback information embedded in the Sequence data.<br/>
	 * The registered callback function is called at the update timing of the application main thread
	 * immediately after processing the callback event.<br/></para>
	 * <para header='Note'>Only one callback function can be registered.<br/>
	 * If you do the registration multiple times, the callback function
	 * already registered is overwritten by the one registered later.<br/></para>
	 * </remarks>
	 */
	[Obsolete("SetEventCallback is deprecated. Use CriAtomExSequencer.OnCallback event", false)]
	public static void SetEventCallback(CriAtomExSequencer.EventCbFunc func, string separator = "\t")
	{
		/* MonoBehaviour側に登録 */
		CriAtom.SetEventCallback(func, separator);
	}
}

/**
 * <summary>A class for using beat synchronization data.</summary>
 * <remarks>
 * <para header='Description'>A class for using the beat synchronization data set on CRI Atom Craft.<br/></para>
 * </remarks>
 */
public static class CriAtomExBeatSync
{
	/**
	 * <summary>Beat synchronization position detection callback information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for getting information from the beat synchronization callback and beat synchronization information getting method.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExBeatSync::SetCallback'/>
	 * <seealso cref='CriAtomExPlayback::GetBeatSyncInfo'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Info {
		public IntPtr   playerHn;           /**< Player handle */
		public uint     playbackId;         /**< Playback ID */
		public uint     barCount;           /**< The number of Measures */
		public uint     beatCount;          /**< The current count of beats */
		public float    beatProgress;       /**< Beat progress (0.0f to 1.0f) */
		public float    bpm;                /**< Tempo (beats/minute) */
		public int      offset;             /**< Sync offset (ms) */
		public uint     numBeats;           /**< The number of beats */
	}

	/**
	 * <summary>Beat synchronization callback</summary>
	 * <param name='info'>Beat synchronization position detection information</param>
	 * <remarks>
	 * <para header='Description'>A beat synchronization callback function type.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExBeatSync::SetCallback'/>
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void CbFunc(ref Info info);

	/**
	 * <summary>Registers the beat synchronization callback</summary>
	 * <remarks>
	 * <para header='Description'>Registers the callback function that receives the beat synchronization position information embedded in the Cue.<br/>
	 * The registered callback function is called at the update timing of the application main thread
	 * immediately after processing the callback event.<br/></para>
	 * </remarks>
	 */
	public static event CbFunc OnCallback {
		add {
			CriAtom.OnBeatSyncCallback += value;
		}
		remove {
			CriAtom.OnBeatSyncCallback -= value;
		}
	}

	/**
	 * \deprecated
	 * 削除予定の非推奨APIです。
	 * CriAtomExPlayer.OnBeatSyncCallback または CriAtomExBeatSync.OnCallback の使用を検討してください。
	 * <summary>Registers the beat synchronization callback</summary>
	 * <param name='func'>Callback function</param>
	 * <remarks>
	 * <para header='Description'>Registers the callback function that receives the beat synchronization position information embedded in the Cue.<br/>
	 * The registered callback function is called at the update timing of the application main thread
	 * immediately after processing the callback event.<br/></para>
	 * <para header='Note'>Only one callback function can be registered.<br/>
	 * If you do the registration multiple times, the callback function
	 * already registered is overwritten by the one registered later.</para>
	 * </remarks>
	 */
	[Obsolete("SetCallback is deprecated. Use OnBeatSyncCallback event", false)]
	public static void SetCallback(CriAtomExBeatSync.CbFunc func)
	{
		/* MonoBehaviour側に登録 */
		CriAtom.SetBeatSyncCallback(func);
	}
}

/**
 * <summary>A class that controls the bus output of the Atom sound renderer.</summary>
 * <remarks>
 * <para header='Description'>In this class, you can change the volume or measure the level by operating the bus output of the Atom sound renderer.<br/></para>
 * </remarks>
 */
public class CriAtomExAsr
{
	[StructLayout(LayoutKind.Sequential)]
	private struct BusAnalyzerConfig
	{
		public int interval;
		public int peakHoldTime;
	}

	/**
	 * <summary>Level measurement information</summary>
	 * <remarks>
	 * <para header='Description'>A structure for acquiring the level measurement information of the DSP bus.<br/>
	 * Used in the CriAtomExAsr::GetBusAnalyzerInfo.</para>
	 * <para header='Note'>Each level value is a scale factor for the amplitude of the sound data (the unit is not decibel).<br/>
	 * You can convert to decibel notation using the following code.<br/>
	 * dB = 10.0f * log10f(level);</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsr::GetBusAnalyzerInfo'/>
	 */
	[StructLayout(LayoutKind.Sequential)]
	public struct BusAnalyzerInfo
	{
		public int numChannels;                 /**< The number of effective channels */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public float[] rmsLevels;               /**< RMS level */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public float[] peakLevels;              /**< Peak level */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public float[] peakHoldLevels;          /**< Peak hold level */

		public BusAnalyzerInfo(byte[] data)
		{
			if (data != null) {
				this.numChannels = BitConverter.ToInt32(data, 0);
				this.rmsLevels = new float[8];
				for (int i = 0; i < 8; i++) {
					this.rmsLevels[i] = BitConverter.ToSingle(data, 4 + i * 4);
				}
				this.peakLevels = new float[8];
				for (int i = 0; i < 8; i++) {
					this.peakLevels[i] = BitConverter.ToSingle(data, 36 + i * 4);
				}
				this.peakHoldLevels = new float[8];
				for (int i = 0; i < 8; i++) {
					this.peakHoldLevels[i] = BitConverter.ToSingle(data, 68 + i * 4);
				}
			} else {
				this.numChannels = 0;
				this.rmsLevels = new float[8];
				this.peakLevels = new float[8];
				this.peakHoldLevels = new float[8];
			}
		}
	}

	/**
	 * <summary>Adds the level measurement function</summary>
	 * <param name='busName'>DSP bus name</param>
	 * <param name='interval'>Measurement interval (ms)</param>
	 * <param name='peakHoldTime'>Hold time of the peak hold level (ms)</param>
	 * <remarks>
	 * <para header='Description'>Adds the level measurement function to the DSP bus and starts the level measurement.<br/>
	 * After calling this function, you can get the RMS level (sound pressure),
	 * peak level (maximum amplitude), and peak hold level by calling the
	 * CriAtomExAsr::GetBusAnalyzerInfo function.
	 * In order to measure the level of multiple DSP buses, this function must be called for each DSP bus.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsr::GetBusAnalyzerInfo'/>
	 * <seealso cref='CriAtomExAsr::DetachBusAnalyzer'/>
	 */
	public static void AttachBusAnalyzer(string busName, int interval, int peakHoldTime)
	{
		BusAnalyzerConfig config;
		config.interval = interval;
		config.peakHoldTime = peakHoldTime;
		criAtomExAsr_AttachBusAnalyzerByName(busName, ref config);
	}

	/**
	 * <summary>Adds the level measurement feature to all DSP buses</summary>
	 * <param name='interval'>Measurement interval (ms)</param>
	 * <param name='peakHoldTime'>Hold time of the peak hold level (ms)</param>
	 * <remarks>
	 * <para header='Description'>Adds the level measurement function to the DSP bus and starts the level measurement.<br/>
	 * After calling this function, you can get the RMS level (sound pressure),
	 * peak level (maximum amplitude), and peak hold level by calling the
	 * CriAtomExAsr::GetBusAnalyzerInfo function.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsr::GetBusAnalyzerInfo'/>
	 * <seealso cref='CriAtomExAsr::DetachBusAnalyzer'/>
	 */
	public static void AttachBusAnalyzer(int interval, int peakHoldTime)
	{
		BusAnalyzerConfig config;
		config.interval = interval;
		config.peakHoldTime = peakHoldTime;
		for (int i = 0; i < 8; i++) {
			criAtomExAsr_AttachBusAnalyzer(i, ref config);
		}
	}

	/**
	 * <summary>Removes the level measurement function</summary>
	 * <param name='busName'>DSP bus name</param>
	 * <remarks>
	 * <para header='Description'>Removes the level measurement function from the specified DSP bus.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsr::AttachBusAnalyzer'/>
	 */
	public static void DetachBusAnalyzer(string busName)
	{
		criAtomExAsr_DetachBusAnalyzerByName(busName);
	}

	/**
	 * <summary>Removes the level measurement feature from all DSP buses</summary>
	 * <remarks>
	 * <para header='Description'>Removes the level measurement feature from all DSP buses.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsr::AttachBusAnalyzer'/>
	 */
	public static void DetachBusAnalyzer()
	{
		for (int i = 0; i < 8; i++) {
			criAtomExAsr_DetachBusAnalyzer(i);
		}
	}

	/**
	 * <summary>Gets the level measurement result</summary>
	 * <param name='busName'>DSP bus name</param>
	 * <param name='info'>Level measurement result</param>
	 * <remarks>
	 * <para header='Description'>Gets the result of the level measurement function from the DSP bus.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsr::AttachBusAnalyzer'/>
	 */
	public static void GetBusAnalyzerInfo(string busName, out BusAnalyzerInfo info)
	{
		using (var mem = new CriStructMemory<BusAnalyzerInfo>()) {
			criAtomExAsr_GetBusAnalyzerInfoByName(busName, mem.ptr);
			info = new BusAnalyzerInfo(mem.bytes);
		}
	}

	/**
	 * \deprecated
	 * This is a deprecated API that will be removed.
	 * Please consider using CriAtomExAsr.GetBusAnalyzerInfo(string busName, out BusAnalyzerInfo info) instead.
	*/
	[System.Obsolete("Use CriAtomExAsr.GetBusAnalyzerInfo(string busName, out BusAnalyzerInfo info)")]
	public static void GetBusAnalyzerInfo(int busId, out BusAnalyzerInfo info)
	{
		using (var mem = new CriStructMemory<BusAnalyzerInfo>()) {
			criAtomExAsr_GetBusAnalyzerInfo(busId, mem.ptr);
			info = new BusAnalyzerInfo(mem.bytes);
		}
	}

	/**
	 * <summary>Sets the volume of the DSP bus</summary>
	 * <param name='busName'>DSP bus name</param>
	 * <param name='volume'>Volume value</param>
	 * <remarks>
	 * <para header='Description'>Sets the volume of the DSP bus.<br/>
	 * This is valid for the send destinations whose send type is post-volume or post-pan.<br/>
	 * <br/>
	 * For the volume value, specify a real value in the range of 0.0f to 1.0f.<br/>
	 * The volume value is a scale factor for the amplitude of the sound data (the unit is not decibel).<br/>
	 * For example, if you specify 1.0f, the original sound is played at its unmodified volume.<br/>
	 * If you specify 0.5f, the sound is played at the volume by halving the amplitude (-6dB)
	 * of the original waveform.<br/>
	 * If you specify 0.0f, the sound is muted (silent).<br/>
	 * The default volume is the value set on CRI Atom Craft.<br/></para>
	 * </remarks>
	 */
	public static void SetBusVolume(string busName, float volume)
	{
		criAtomExAsr_SetBusVolumeByName(busName, volume);
	}

	/**
	 * \deprecated
	 * This is a deprecated API that will be removed.
	 * Please consider using CriAtomExAsr.SetBusVolume(string busName, float volume) instead.
	*/
	[System.Obsolete("Use CriAtomExAsr.SetBusVolume(string busName, float volume)")]
	public static void SetBusVolume(int busId, float volume)
	{
		criAtomExAsr_SetBusVolume(busId, volume);
	}

	/**
	 * <summary>Sets the send level of the DSP bus</summary>
	 * <param name='busName'>DSP bus name</param>
	 * <param name='sendTo'>Send destination DSP bus name</param>
	 * <param name='level'>Level value</param>
	 * <remarks>
	 * <para header='Description'>Sets the level for sending sound data to the send destination DSP bus.<br/>
	 * <br/>
	 * Specify a real value in the range of 0.0f to 1.0f for the level value.<br/>
	 * The level value is a scale factor for the amplitude of sound data (in decibels).<br/>
	 * For example, if you set it to 1.0f, the sound is played at its original level.<br/>
	 * If you specify 0.5f, the sound is played at the volume by halving the amplitude (-6dB)
	 * of the original waveform.<br/>
	 * If you specify 0.0f, the sound is muted (silent).<br/>
	 * The default level is the value set on CRI Atom Craft.<br/></para>
	 * </remarks>
	 */
	public static void SetBusSendLevel(string busName, string sendTo, float level)
	{
		criAtomExAsr_SetBusSendLevelByName(busName, sendTo, level);
	}

	/**
	 * \deprecated
	 * This is a deprecated API that will be removed.
	 * Please consider using CriAtomExAsr.SetBusSendLevel(string busName, string sendTo, float level) instead.
	*/
	[System.Obsolete("Use CriAtomExAsr.SetBusSendLevel(string busName, string sendTo, float level)")]
	public static void SetBusSendLevel(int busId, int sendTo, float level)
	{
		criAtomExAsr_SetBusSendLevel(busId, sendTo, level);
	}

	/**
	 * <summary>Sets the level matrix for the DSP bus</summary>
	 * <param name='busName'>DSP bus name</param>
	 * <param name='inputChannels'>The number of input channels</param>
	 * <param name='outputChannels'>The number of output channels</param>
	 * <param name='matrix'>An array of level values that represents the level matrix in one dimension</param>
	 * <remarks>
	 * <para header='Description'>Sets the level matrix of the DSP bus.<br/>
	 * Valid for the send destination whose send type is post-pan.<br/>
	 * <br/>
	 * The level matrix is a mechanism for specifying which speaker outputs
	 * the sound of each channel of the sound data at which volume.<br/>
	 * matrix is an array of [input_channels * output_channels].<br/>
	 * You set the level sent from the input channel ch_in to the output channel ch_out
	 * inmatrix[ch_in * output_channels + ch_out].<br/>
	 * The default of the level matrix is the identity matrix.<br/>
	 * <br/>
	 * Specify a real value in the range of 0.0f to 1.0f for the level value.<br/>
	 * The level value is a scale factor for the amplitude of sound data (in decibels).<br/>
	 * For example, if you set it to 1.0f, the sound is played at its original level.<br/>
	 * If you specify 0.5f, the sound is played at the volume by halving the amplitude (-6dB)
	 * of the original waveform.<br/>
	 * If you specify 0.0f, the sound is muted (silent).<br/></para>
	 * </remarks>
	 */
	public static void SetBusMatrix(string busName, int inputChannels, int outputChannels, float[] matrix)
	{
		criAtomExAsr_SetBusMatrixByName(busName, inputChannels, outputChannels, matrix);
	}

	/**
	 * \deprecated
	 * This is a deprecated API that will be removed.
	 * Please consider using CriAtomExAsr.SetBusMatrix(string busName, int inputChannels, int outputChannels, float[] matrix) instead.
	*/
	[System.Obsolete("Use CriAtomExAsr.SetBusMatrix(string busName, int inputChannels, int outputChannels, float[] matrix)")]
	public static void SetBusMatrix(int busId, int inputChannels, int outputChannels, float[] matrix)
	{
		criAtomExAsr_SetBusMatrix(busId, inputChannels, outputChannels, matrix);
	}

	/**
	 * <summary>DSP bus effect Bypass setting</summary>
	 * <param name='busName'>Bus name</param>
	 * <param name='effectName'>Effect name</param>
	 * <param name='bypass'>Bypass setting (True: bypass, False: not bypass)</param>
	 * <remarks>
	 * <para header='Description'>Sets the Bypass of the effect.<br/>
	 * Bypassed effects will be passed through during sound processing.<br/>
	 * Before bypassing an effect, a DSP bus setting must be attached
	 * before calling this function.<br/>
	 * Which effect is on which bus depends on the DSP bus settings you attached.<br/>
	 * The function fails if there is no effect with the specified ID on the specified bus.<br/></para>
	 * <para header='Note'>If you set Bypass while the sound is played, a noise may occur.<br/></para>
	 * </remarks>
	 */
	public static void SetEffectBypass(string busName, string effectName, bool bypass)
	{
		criAtomExAsr_SetEffectBypass(busName, effectName, bypass);
	}

	/**
	 * <summary>Sets the DSP bus effect operating parameter</summary>
	 * <param name='busName'>Bus name</param>
	 * <param name='effectName'>Effect name</param>
	 * <param name='parameterIndex'>Effect run-time parameter index</param>
	 * <param name='parameterValue'>Effect run-time parameter settings</param>
	 * <remarks>
	 * <para header='Description'>Sets the operating parameters of the DSP bus effect.<br/>
	 * <br/>
	 * Which effect is on which bus depends on the DSP bus settings you attached.<br/>
	 * The function fails if there is no effect with the specified ID on the specified bus.<br/></para>
	 * </remarks>
	 */
	public static void SetEffectParameter(string busName, string effectName, uint parameterIndex, float parameterValue)
	{
		criAtomExAsr_SetEffectParameter(busName, effectName, parameterIndex, parameterValue);
		criAtomExAsr_UpdateEffectParameters(busName, effectName);
	}

	/**
	 * <summary>Gets the DSP bus effect operating parameter</summary>
	 * <param name='busName'>Bus name</param>
	 * <param name='effectName'>Effect name</param>
	 * <param name='parameterIndex'>Effect run-time parameter index</param>
	 * <remarks>
	 * <para header='Description'>Gets the operating parameter value of the DSP bus effect.<br/>
	 * <br/>
	 * Which effect is on which bus depends on the DSP bus settings you attached.<br/>
	 * The function fails if there is no effect with the specified ID on the specified bus.<br/></para>
	 * </remarks>
	 */
	public static float GetEffectParameter(string busName, string effectName, uint parameterIndex)
	{
		return criAtomExAsr_GetEffectParameter(busName, effectName, parameterIndex);
	}


	/**
	 * <summary>Registers the user-defined effect interface</summary>
	 * <param name='afx_interface'>Interface with version information for user-defined effects</param>
	 * <returns>Was the registration successful? (True: registration successful, False: registration failed)</returns>
	 * <remarks>
	 * <para header='Description'>Registers the user-defined effects interface with ASR.<br/>
	 * The effects for which the user-defined effects interface has been registered can be used when attaching the DSP bus settings.<br/>
	 * If the following conditions are met, the registration of the user-defined effects interface fails and an error callback is returned:
	 *  - A user defined effect interface with the same effect name is already registered
	 *  - It is different from the user-defined effects interface used by Atom
	 *  - The maximum number of user-defined effect interface registrations is reached</para>
	 * <para header='Note'>This API can be used only when registering an user-defined effect in CRI ADX2 Audio Effect Plugin SDK.<br/>
	 * Be sure to call this function after calling CriAtomPlugin::InitializeLibrary
	 * and before calling CriAtomEx::AttachDSPBusSetting.<br/>
	 * Once registered, the interface pointer continues to be referenced until CriAtomEx::DetachDSPBusSetting is called.<br/>
	 * If you want to unregister the interface while using the library, use CriAtomExAsr::UnregisterEffectInterface.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsr::UnregisterEffectInterface'/>
	 * <seealso cref='CriAtomEx::AttachDspBusSetting'/>
	 * <seealso cref='CriAtomEx::DetachDspBusSetting'/>
	 */
	static public bool RegisterEffectInterface(IntPtr afx_interface)
	{
		return criAtomExAsr_RegisterEffectInterface(afx_interface);
	}

	/**
	 * <summary>Unregisters the user-defined effects interface</summary>
	 * <param name='afx_interface'>Interface with version information for user-defined effects</param>
	 * <remarks>
	 * <para header='Description'>Unregister the user-defined effect interface.<br/>
	 * Unregistered effects will no longer be available when attaching the DSP bus settings.<br/>
	 * You cannot unregister a user-defined effect interface that has not been
	 * registered (error callback is returned).</para>
	 * <para header='Note'>This API can be used only when unregistering an effect in CRI ADX2 Audio Effect Plugin SDK.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsr::RegisterEffectInterface'/>
	 */
	static public void UnregisterEffectInterface(IntPtr afx_interface)
	{
	criAtomExAsr_UnregisterEffectInterface(afx_interface);
	}

	/**
	 * <summary>Getting the volume of the Bus</summary>
	 * <param name='busName'>Bus name</param>
	 * <param name='volume'>Volume value</param>
	 * <remarks>
	 * <para header='Description'>Gets the volume of the bus. <br/>
	 * <br/>
	 * The raw volume value will be returned. <br/>
	 * The default value of the volume is the value set in CRI Atom Craft.</para>
	 * </remarks>
	 */
	static public void GetBusVolume(string busName, out float volume)
	{
		criAtomExAsr_GetBusVolumeByName(busName, out volume);
	}

	/**
	 * <summary>Gets the PCM output</summary>
	 * <param name='outputChannels'>Number of channels to acquire</param>
	 * <param name='outputSamples'>Number of PCM samples to acquire</param>
	 * <param name='buffer'>Buffer for writing PCM samples</param>
	 * <returns>Number of PCM samples acquired</returns>
	 * <remarks>
	 * <para header='Description'>Gets the PCM samples output by Atom.<br/></para>
	 * <para header='Note'>This function is for execution in the Editor only.<br/>
	 * It works only if the user PCM output mode is enabled and initialized.<br/></para>
	 * </remarks>
	 */
	static public int GetPcmOutput(int outputChannels, int outputSamples, float[][] buffer)
	{
	#if UNITY_EDITOR
		GCHandle[] gchs = new GCHandle[outputChannels];
		IntPtr[] intPtrs = new IntPtr[outputChannels];
		for (int i = 0; i < outputChannels; i++) {
			gchs[i] = GCHandle.Alloc(buffer[i], GCHandleType.Pinned);
			intPtrs[i] = gchs[i].AddrOfPinnedObject();
		}
		int ret = criAtomExAsr_GetPcmDataFloat32(outputChannels, outputSamples, intPtrs);
		for (int i = 0; i < outputChannels; i++) {
			gchs[i].Free();
		}
		return ret;
	#else
		return 0;
	#endif
	}

	/**
	 * <summary>Gets the number of output PCM samples available</summary>
	 * <returns>Number of PCM output samples that can be acquired</returns>
	 * <remarks>
	 * <para header='Description'>Gets the number of PCM samples that will be output from the available Atom.<br/></para>
	 * <para header='Note'>This function is for execution in the Editor only.<br/>
	 * It works only if the user PCM output mode is enabled and initialized.<br/></para>
	 * </remarks>
	 */
	static public int GetNumBufferedPcmOutputSamples()
	{
	#if UNITY_EDITOR
		return criAtomExAsr_GetNumBufferedSamples();
	#else
		return 0;
	#endif
	}

	/**
	 * <summary>Sets the size of the buffer for the output of the PCM samples</summary>
	 * <param name='numSamples'>Size of the buffer containing the PCM samples output</param>
	 * <remarks>
	 * <para header='Description'>Sets the size of the buffer used by Atom for the PCM sample data.<br/> A small size will reduce the latency of the output obtained via CriWare.CriAtomExAsr::GetPcmOutput . However, it may cause audio interruptions and other issues if the calling rate is insufficient.</para>
	 * <para header='Note'>This function is for execution in the Editor only.<br/>
	 * It works only if the user PCM output mode is enabled and initialized.<br/></para>
	 * </remarks>
	 */
	static public void SetPcmBufferSize(int numSamples)
	{
	#if UNITY_EDITOR
		criAtomExAsr_SetPcmBufferSize(numSamples);
	#endif
	}

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_AttachBusAnalyzerByName(
		string busName, ref BusAnalyzerConfig config);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_AttachBusAnalyzer(
		int busNo, ref BusAnalyzerConfig config);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_DetachBusAnalyzerByName(string busName);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_DetachBusAnalyzer(int busNo);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_GetBusAnalyzerInfoByName(
		string busName, IntPtr info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_GetBusAnalyzerInfo(
		int busNo, IntPtr info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_SetBusVolumeByName(
		string busName, float volume);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_SetBusVolume(
		int busNo, float volume);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_SetBusSendLevelByName(
		string busName, string sendtoName, float level);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_SetBusSendLevel(
		int busNo, int sendtoNo, float level);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_SetBusMatrixByName(
		string busName,
		int inputChannels,
		int outputChannels,
		[MarshalAs(UnmanagedType.LPArray)] float[] matrix
		);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_SetBusMatrix(
		int busNo,
		int inputChannels,
		int outputChannels,
		[MarshalAs(UnmanagedType.LPArray)] float[] matrix
		);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_SetEffectBypass(string busName, string effectName, bool bypass);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_UpdateEffectParameters(string busName, string effectName);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_SetEffectParameter(string busName, string effectName, uint parameterIndex, float parameterValue);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern float criAtomExAsr_GetEffectParameter(string busName, string effectName, uint parameterIndex);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExAsr_RegisterEffectInterface(IntPtr afx_interface);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_UnregisterEffectInterface(IntPtr afx_interface);

#if UNITY_EDITOR
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAsr_GetPcmDataFloat32(int outputChannels, int outputSamples, IntPtr[] samples);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExAsr_GetNumBufferedSamples();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_SetPcmBufferSize(int numSamples);
#endif

	#else
	private static void criAtomExAsr_AttachBusAnalyzerByName(
		string busName, ref BusAnalyzerConfig config) { }
	private static void criAtomExAsr_AttachBusAnalyzer(
		int busNo, ref BusAnalyzerConfig config) { }
	private static void criAtomExAsr_DetachBusAnalyzerByName(string busName) { }
	private static void criAtomExAsr_DetachBusAnalyzer(int busNo) { }
	private static void criAtomExAsr_GetBusAnalyzerInfoByName(
		string busName, IntPtr info) { }
	private static void criAtomExAsr_GetBusAnalyzerInfo(int busNo, IntPtr info) { }
	private static void criAtomExAsr_SetBusVolumeByName(string busName, float volume) { }
	private static void criAtomExAsr_SetBusVolume(int busNo, float volume) { }
	private static void criAtomExAsr_SetBusSendLevelByName(string busName, string sendtoName, float level) { }
	private static void criAtomExAsr_SetBusSendLevel(int busNo, int sendtoNo, float level) { }
	private static void criAtomExAsr_SetBusMatrixByName(
		string busName,
		int inputChannels,
		int outputChannels,
		[MarshalAs(UnmanagedType.LPArray)] float[] matrix
		) { }
	private static void criAtomExAsr_SetBusMatrix(
		int busNo,
		int inputChannels,
		int outputChannels,
		[MarshalAs(UnmanagedType.LPArray)] float[] matrix
		) { }
	private static void criAtomExAsr_SetEffectBypass(string busName, string effectName, bool bypass) { }
	private static void criAtomExAsr_UpdateEffectParameters(string busName, string effectName) { }
	private static void criAtomExAsr_SetEffectParameter(string busName, string effectName, uint parameterIndex, float parameterValue) { }
	private static float criAtomExAsr_GetEffectParameter(string busName, string effectName, uint parameterIndex) { return 0.0f; }
	private static bool criAtomExAsr_RegisterEffectInterface(IntPtr afx_interface) { return true; } // fixme
	private static void criAtomExAsr_UnregisterEffectInterface(IntPtr afx_interface) { }
#if UNITY_EDITOR
	private static int criAtomExAsr_GetPcmDataFloat32(int outputChannels, int outputSamples, IntPtr[] samples) { return 0; }
	private static int criAtomExAsr_GetNumBufferedSamples() { return 0; }
	private static void criAtomExAsr_SetPcmBufferSize(int numSamples) { }
#endif
	#endif

	#if !CRIWARE_ENABLE_HEADLESS_MODE && !UNITY_WEBGL
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsr_GetBusVolumeByName(string busName, out float volume);
	#else
	private static void criAtomExAsr_GetBusVolumeByName(string busName, out float volume) { volume = 0.0f; }
	#endif

	#endregion
}

/**
 * <summary>A class for getting the estimated delay value for sound playback.</summary>
 * <remarks>
 * <para header='Description'>In this class, you can change the volume or measure the level by operating the
 * bus output of the Atom sound renderer.<br/></para>
 * </remarks>
 */
public static class CriAtomExLatencyEstimator
{
	/**
	 * <summary>Status</summary>
	 * <remarks>
	 * <para header='Description'>A value that indicates the status of the delay estimation.<br/>
	 * It can get using the CriWare.CriAtomExLatencyEstimator::GetCurrentInfo function.<br/>
	 * <br/></para>
	 * </remarks>
	 */
	public enum Status {
		Stop,                       /**< Initial state/stopped state */
		Processing,                 /**< Estimating delay time */
		Done,                       /**< Delay time estimation completed */
		Error,                      /**< Error */
	}

	/**
	 * <summary>Status</summary>
	 * <remarks>
	 * <para header='Description'>A structure that represents the information on delay estimation.<br/>
	 * It holds the execution status of the inference module and the latency (estimated value). The unit is millisecond.
	 * It can get using the CriWare.CriAtomExLatencyEstimator::GetCurrentInfo function.<br/>
	 * <br/></para>
	 * </remarks>
	 */
	[StructLayout(LayoutKind.Sequential)]
	public struct EstimatorInfo
	{
		public Status status;   /**< Status of the delay estimation module */
		public uint estimated_latency;  /**< The result of estimated latency (in milliseconds) */
	}

	/**
	 * <summary>Initialize the delay estimation process</summary>
	 * <remarks>
	 * <para header='Call condition'>Call this function after initializing the plug-in.</para>
	 * <para header='Description'>Starts the delay estimation process for sound playback.<br/>
	 * <br/>
	 * If you use the CriAtomExLatencyEstimator to obtain the sound delay estimation value,
	 * you must always do initialization using this function.<br/>
	 * If you could get the estimated value or if an error occurs, call the
	 * CriAtomExLatencyEstimator.Finalize function.<br/></para>
	 * <para header='Note'>Multiple calls to this function are allowed, but the number of calls is internally counted,
	 * and the actual initialization done only at the first call.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExLatencyEstimator.FinalizeModule'/>
	 */
	public static void InitializeModule()
	{
	#if !UNITY_EDITOR && UNITY_ANDROID
		criAtomLatencyEstimator_Initialize_ANDROID();
	#endif
	}

	/**
	 * <summary>Terminates the delay estimation process</summary>
	 * <remarks>
	 * <para header='Description'>Finishes the delay estimation for sound playback.<br/>
	 * <br/>
	 * When the delay estimation value is acquired, call this function to end the estimation process.
	 * Also, call this function if an error occurs or if you want to interrupt the estimation process.</para>
	 * <para header='Note'>When the CriAtomExLatencyEstimator.InitializeModule function is called multiple times, the number of calls (reference count)
	 * is incremented internally. The termination process is not invoked until the reference count reaches 0, so when you performed initialization multiple times,
	 * call this function until the reference count becomes 0.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExLatencyEstimator.InitializeModule'/>
	 */
	public static void FinalizeModule()
	{
	#if !UNITY_EDITOR && UNITY_ANDROID
		criAtomLatencyEstimator_Finalize_ANDROID();
	#endif
	}

	/**
	 * <summary>Gets information on delay estimation</summary>
	 * <remarks>
	 * <para header='Description'>Gets the current information on the delay estimation.<br/>
	 * The information that can be acquired is "delay estimator status" and "estimated delay time (ms)".<br/>
	 * <br/>
	 * The estimated_latency obtained when the status changes to Status.Done is the estimated value.<br/>
	 * Note that the estimated delay value cannot be obtained immediately. Before it changes from Status.Processsing to Status.Done, it takes
	 * tens to hundreds of milliseconds. (The time it takes depends on the Atom initialization settings and device)
	 * <br/>
	 * The value of estmated_latency when the status is not Status.Done is invalid. Make sure that the status is Status.Done
	 * before recording the value of estimated_latency.<br/>
	 * <br/>
	 * After calling this function to get the estimated value, call riAtomExLatencyEstimator.Finalize to finalize the process.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExLatencyEstimator.InitializeModule'/>
	 * <seealso cref='CriAtomExLatencyEstimator.FinalizeModule'/>
	 */
	public static CriAtomExLatencyEstimator.EstimatorInfo GetCurrentInfo()
	{
	#if !UNITY_EDITOR && UNITY_ANDROID
		return criAtomLatencyEstimator_GetCurrentInfo_ANDROID();
	#else
		EstimatorInfo info = new EstimatorInfo();
		info.status = CriAtomExLatencyEstimator.Status.Stop;
		info.estimated_latency = 0;
		return info;
	#endif
	}

	#region DLL Import
	#if !UNITY_EDITOR && UNITY_ANDROID
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomLatencyEstimator_Initialize_ANDROID();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomLatencyEstimator_Finalize_ANDROID();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern EstimatorInfo criAtomLatencyEstimator_GetCurrentInfo_ANDROID();
	#else
	private static void criAtomLatencyEstimator_Initialize_ANDROID() { }
	private static void criAtomLatencyEstimator_Finalize_ANDROID() { }
	private static EstimatorInfo criAtomLatencyEstimator_GetCurrentInfo_ANDROID()
	{
		EstimatorInfo info = new EstimatorInfo();
		info.status = CriAtomExLatencyEstimator.Status.Stop;
		info.estimated_latency = 0;
		return info;
	}
	#endif
	#endif
	#endregion
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
