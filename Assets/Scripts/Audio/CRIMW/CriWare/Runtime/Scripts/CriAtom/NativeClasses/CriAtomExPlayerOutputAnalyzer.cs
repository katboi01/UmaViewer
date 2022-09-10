/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */

namespace CriWare {

/**
 * \deprecated
 * 削除予定の非推奨APIです。
 * CriAtomExOutputAnalyzerクラスの使用を検討してください。
 * <summary>Sound output data analysis module (for each player/source)</summary>
 * <remarks>
 * <para header='Description'>Performs sound output analysis for each CriAtomSource/CriAtomExPlayer.<br/>
 * Provides features such as Level Meter.<br/></para>
 * <para header='Note'>This class is discontinued in the future. Use CriAtomExOutputAnalyzer .<br/>
 * Analysis is not possible when using HCA-MX or a platform-specific sound compression codec.<br/>
 * Use HCA or ADX codecs.</para>
 * </remarks>
*/
[System.Obsolete("Use CriAtomExOutputAnalyzer")]
public class CriAtomExPlayerOutputAnalyzer : CriAtomExOutputAnalyzer
{
	/**
	 * <summary>Analysis processing type</summary>
	 * <remarks>
	 * <para header='Description'>A value indicating the type of analysis specified when creating the analysis module.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExOutputAnalyzer'/>
	 */
	public enum Type {
		LevelMeter = 0,         /**< Level Meter (RMS level measurement) */
		SpectrumAnalyzer = 1,   /**< Spectrum analyzer */
		PcmCapture = 2,         /**< Gets the waveform data */
	}

	/**
	 * <summary>Sound output data analysis module config structure</summary>
	 * <remarks>
	 * <para header='Description'>A config specified when creating the analysis module.<br/>
	 * num_spectrum_analyzer_bands: The number of spectrum analyzer bands<br/>
	 * num_stored_output_data: The number of output data samples to be recorded<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayerOutputAnalyzer'/>
	 */
	public new struct Config {
		public int num_spectrum_analyzer_bands;
		public int num_stored_output_data;

		public Config(int num_spectrum_analyzer_bands = 8, int num_stored_output_data = 4096)
		{
			this.num_spectrum_analyzer_bands = num_spectrum_analyzer_bands;
			this.num_stored_output_data = num_stored_output_data;
		}
	}

	/**
	 * <summary>Creates a sound output data analysis module</summary>
	 * <returns>Sound output data analysis module</returns>
	 * <remarks>
	 * <para header='Description'>Creates an output sound data analysis module for CriAtomSource/CriAtomExPlayer.<br/>
	 * You use the analysis module you created by attaching it to CriAtomSource or CriAtomExPlayer.<br/>
	 * You perform analysis on the attached sound output such as a Level Meter.<br/></para>
	 * <para header='Note'>Only one CriAtomSource/CriAtomExPlayer can be attached to the analysis module.<br/>
	 * If you want to reuse the analysis module, detach it.<br/></para>
	 * <para header='Note'>This class is discontinued in the future. Use CriAtomExOutputAnalyzer .<br/>
	 * Unmanaged resources are reserved when creating a sound output data analysis module.<br/>
	 * When you no longer need the analysis module, be sure to call the CriAtomExPlayerOutputAnalyzer.Dispose method.</para>
	 * </remarks>
	 */
	public CriAtomExPlayerOutputAnalyzer(Type[] types, Config[] configs = null)
		: base()
	{
		CriAtomExOutputAnalyzer.Config config = new CriAtomExOutputAnalyzer.Config();
		for (int i = 0; i < types.Length; i++) {
			switch (types[i]) {
				case Type.LevelMeter:
				{
					config.enableLevelmeter = true;
					break;
				}
				case Type.SpectrumAnalyzer:
				{
					config.enableSpectrumAnalyzer = true;
					if (configs != null && configs.Length > i) {
						config.numSpectrumAnalyzerBands = configs[i].num_spectrum_analyzer_bands;
					} else {
						config.numSpectrumAnalyzerBands = 8;
					}
					break;
				}
				case Type.PcmCapture:
				{
					config.enablePcmCapture = true;
					if (configs != null && configs.Length > i) {
						config.numCapturedPcmSamples = configs[i].num_stored_output_data;
					} else {
						config.numCapturedPcmSamples = 4096;
					}
					break;
				}
			}
		}
		this.InitializeWithConfig(config);
	}
}

} //namespace CriWare
/**
 * @}
 */

/* --- end of file --- */
