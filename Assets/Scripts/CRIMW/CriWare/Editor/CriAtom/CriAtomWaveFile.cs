/****************************************************************************
 *
 * Copyright (c) 2021 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System.Runtime.InteropServices;

/**
 * \addtogroup CRIWARE_EDITOR_UTILITY
 * @{
 */

namespace CriWare.Editor {

/**
 * <summary>This is a class used for bouncing the audio output to a file.</summary>
 * <remarks>
 * <para header='Description'>This class can be used to bounce the audio output from the ASR.</para>
 * </remarks>
 */
public static class CriAtomWaveFile
{

	/**
	 * <summary>Starts the bouncing</summary>
	 * <param name='path'>File path for the bouncing output</param>
	 * <param name='numChannels'>Number of channels in the bounced output file (1, 2, 4, 6 or 8)</param>
	 * <returns>Whether the bouncing has been started</returns>
	 * <remarks>
	 * <para header='Description'>Starts bouncing with the specified parameters.<br/>
	 * 1, 2, 4, 6 or 8 can be specified for numChannels.<br/>
	 * If numChannels is less than the number of ASR output channels, it will be automatically downmixed to the specified number of channels before bouncing.<br/>
	 * If the number of numChannels is larger than the number of output channels of ASR, silence will be recorded for the extra channels.<br/></para>
	 * </remarks>
	 * <seealso cref='CriWare.Editor.CriAtomWaveFile.StopBounce'/>
	 */
	public static bool StartBounce(string path, uint numChannels)
	{
		return criAtomWaveFile_StartBounce(path, numChannels);
	}

	/**
	 * <summary>Stops the bouncing</summary>
	 * <remarks>
	 * <para header='Description'>Stops the bouncing initiated by CriWare.Editor.CriAtomWaveFile.StartBounce .</para>
	 * </remarks>
	 * <seealso cref='CriWare.Editor.CriAtomWaveFile.StartBounce'/>
	 */
	public static void StopBounce()
	{
		criAtomWaveFile_StopBounce();
	}

	/**
	 * <summary>Acquiring the duration (expressed in ms) of the bounced waveform</summary>
	 * <returns>Duration of the bounced waveform</returns>
	 * <remarks>
	 * <para header='Description'>Gets the duration of the bounced waveform, expressed in milliseconds.</para>
	 * </remarks>
	 */
	public static uint GetBounceTime()
	{
		return criAtomWaveFile_GetBounceTime();
	}

	#region DLL Import
#if (UNITY_EDITOR && !UNITY_EDITOR_LINUX) && !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomWaveFile_StartBounce(string path, System.UInt32 num_channels);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomWaveFile_StopBounce();
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern System.UInt32 criAtomWaveFile_GetBounceTime();
#else
	private static bool criAtomWaveFile_StartBounce(string path, System.UInt32 num_channels) { return false; }
	private static void criAtomWaveFile_StopBounce() { }
	private static System.UInt32 criAtomWaveFile_GetBounceTime() { return 0; }
#endif
		#endregion
	}

} //namespace CriWare.Editor

/**
 * @}
 */

/* end of file */