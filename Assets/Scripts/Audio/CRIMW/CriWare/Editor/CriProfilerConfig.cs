/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_EDITOR

namespace CriWare {

public partial class CriProfiler
{
	/* CRI Atom Preview connection ver. */
	private const int CRI_ATOM_PREVIEW_CONNECTION_VERSION = 0x00600000;

	/* the size of the parameter "packet size" in bytes */
	private const int DATA_LENGTH_PARAM_SIZE = 4;

	protected enum TcpCommandId {
		CRITCP_MAIL_OPEN = 1,
		CRITCP_MAIL_OPEN_RESULT,
		CRITCP_MAIL_RECV,
		CRITCP_MAIL_CHANGE,
		CRITCP_MAIL_EXIT,
		CRITCP_MAIL_SEND_CUESHEET_BINARY,
		CRITCP_MAIL_SEND_CUESHEET_BINARY_RESULT,
		CRITCP_MAIL_SEND_CUESHEET_BINARYDATA,
		CRITCP_MAIL_SEND_CUESHEET_BINARYDATA_RESULT,
		CRITCP_MAIL_START_MONITOR,                                      /* 10 */
		CRITCP_MAIL_START_MONITOR_RESULT,
		CRITCP_MAIL_STOP_MONITOR,
		CRITCP_MAIL_STOP_MONITOR_RESULT,
		CRITCP_MAIL_MONITOR_AUOBJ_STATUS,
		CRITCP_MAIL_MONITOR_AUOBJ_STATUS_RESULT,
		CRITCP_MAIL_SEND_LOG_REQUEST,
		CRITCP_MAIL_SEND_LOG_REQUEST_RESULT,
		CRITCP_MAIL_START_LOG_PLAYBACK,
		CRITCP_MAIL_START_LOG_PLAYBACK_RESULT,
		CRITCP_MAIL_STOP_LOG_PLAYBACK,                                  /* 20 */
		CRITCP_MAIL_STOP_LOG_PLAYBACK_RESULT,
		CRITCP_MAIL_START_LOG_RECORD,
		CRITCP_MAIL_START_LOG_RECORD_RESULT,
		CRITCP_MAIL_STOP_LOG_RECORD,
		CRITCP_MAIL_STOP_LOG_RECORD_RESULT,
		CRITCP_MAIL_DO_AUDIO_API,
		CRITCP_MAIL_DO_AUDIO_API_RESULT,
		CRITCP_MAIL_GET_COMMAND,
		CRITCP_MAIL_NON,
		CRITCP_MAIL_NUM_AUOBJ,                                          /* 30 */
		CRITCP_MAIL_SEND_LOG,
		CRITCP_MAIL_SEND_LOG_RESULT,
		CRITCP_MAIL_PREVIEW_FLAG,
		CRITCP_MAIL_PREVIEW_FLAG_RESULT,
		CRITCP_MAIL_SEND_FILE_INFORMATION,
		CRITCP_MAIL_SEND_FILE_INFORMATION_RESULT,
		CRITCP_MAIL_SEND_FILE_DATA,
		CRITCP_MAIL_SEND_FILE_DATA_RESULT,
		CRITCP_MAIL_SEND_LOG_FOR_PLAYBACK,
		CRITCP_MAIL_SEND_LOG_FOR_PLAYBACK_RESULT,                       /* 40 */
		CRITCP_MAIL_PREVIEW_SEND_DETA_RESULT,
		CRITCP_MAIL_PREVIEW_SEND_DETA_RESULT_RESULT,
		CRITCP_MAIL_PREVIEW_CPU_LOAD,
		CRITCP_MAIL_PREVIEW_CPU_LOAD_RESULT,
		CRITCP_MAIL_PREVIEW_PLAYER_STATUS,
		CRITCP_MAIL_PREVIEW_PLAYER_STATUS_RESULT,
		CRITCP_MAIL_DO_FILE_SYSTEM_API,
		CRITCP_MAIL_DO_FILE_SYSTEM_API_RESULT,
		CRITCP_MAIL_SEND_REGISTER_ATOM_CONFIG_BINARY,
		CRITCP_MAIL_SEND_REGISTER_ATOM_CONFIG_BINARY_RESULT,            /* 50 */
		CRITCP_MAIL_SEND_ATOM_CONFIG_BINARYDATA,
		CRITCP_MAIL_SEND_ATOM_CONFIG_BINARYDATA_RESULT,
		CRITCP_MAIL_SEND_ATOM_CUESHEET_BINARY,
		CRITCP_MAIL_SEND_ATOM_CUESHEET_BINARY_RESULT,
		CRITCP_MAIL_SEND_ATOM_CUESHEET_BINARYDATA,
		CRITCP_MAIL_SEND_ATOM_CUESHEET_BINARYDATA_RESULT,
		CRITCP_MAIL_MONITOR_ATOM_EXPLAYBACKINFO_PLAY_POSITION,
		CRITCP_MAIL_MONITOR_ATOM_EXPLAYBACKINFO_PLAY_POSITION_RESULT,
		CRITCP_MAIL_MONITOR_ATOM_EXPLAYBACKINFO_PLAY_END,
		CRITCP_MAIL_MONITOR_ATOM_EXPLAYBACKINFO_PLAY_END_RESULT,        /* 60 */
		CRITCP_MAIL_MONITOR_ATOMEXPLAYERS_STATUS,
		CRITCP_MAIL_MONITOR_ATOMEXPLAYERS_STATUS_RESULT,
		CRITCP_MAIL_MONITOR_ATOM_AUTO_MODULATION_AISAC_CONTROL,
		CRITCP_MAIL_MONITOR_ATOM_AUTO_MODULATION_AISAC_CONTROL_RESULT,
		CRITCP_MAIL_UPDATE_ATOM_CUESHEET_BINARY,
		CRITCP_MAIL_UPDATE_ATOM_CUESHEET_BINARY_RESULT,
		CRITCP_MAIL_UPDATE_ATOM_CUESHEET_BINARYDATA,
		CRITCP_MAIL_UPDATE_ATOM_CUESHEET_BINARYDATA_RESULT,
		CRITCP_MAIL_REQUEST_SEND_ACB,
		CRITCP_MAIL_REQUEST_SEND_ACB_RESULT,                            /* 70 */
		CRITCP_MAIL_SHOW_WINDOW,
		CRITCP_MAIL_SHOW_WINDOW_RESULT,
		CRITCP_MAIL_MONITOR_OVERWRITE_ACB,
		CRITCP_MAIL_MONITOR_OVERWRITE_ACB_RESULT,
		CRITCP_MAIL_OVERWRITE_ACB,
		CRITCP_MAIL_OVERWRITE_ACB_RESULT,
		CRITCP_MAIL_OVERWRITE_ACB_DATA,
		CRITCP_MAIL_OVERWRITE_ACB_DATA_RESULT,
		CRITCP_MAIL_MONITOR_RELEASE_ACB,
		CRITCP_MAIL_MONITOR_RELEASE_ACB_RESULT,                         /* 80 */
		CRITCP_MAIL_SEND_DUMMY,
		CRITCP_MAIL_SEND_DUMMY_RESULT,
		CRITCP_MAIL_UPDATE_PART_OF_ACF,
		CRITCP_MAIL_UPDATE_PART_OF_ACF_RESULT,
		CRITCP_MAIL_UPDATE_PART_OF_ACF_DATA,
		CRITCP_MAIL_UPDATE_PART_OF_ACF_DATA_RESULT,
		CRITCP_MAIL_MONITOR_OVERWRITE_ACF,
		CRITCP_MAIL_MONITOR_OVERWRITE_ACF_RESULT,
		CRITCP_MAIL_OVERWRITE_ACF,
		CRITCP_MAIL_OVERWRITE_ACF_RESULT,                               /* 90 */
		CRITCP_MAIL_OVERWRITE_ACF_DATA,
		CRITCP_MAIL_OVERWRITE_ACF_DATA_RESULT,
		CRITCP_MAIL_MONITOR_ATOMEXASR_BUS_INFO,
		CRITCP_MAIL_MONITOR_ATOMEXASR_BUS_INFO_RESULT,
		CRITCP_MAIL_MONITOR_ATOMEX_ERR_INFO,
		CRITCP_MAIL_MONITOR_ATOMEX_ERR_INFO_RESULT,
		CRITCP_MAIL_MONITOR_ATOMEXASR_DSP_SPECTRA,
		CRITCP_MAIL_MONITOR_ATOMEXASR_DSP_SPECTRA_RESULT,
		CRITCP_MAIL_START_BOUNCE,
		CRITCP_MAIL_START_BOUNCE_RESULT,                                /* 100 */
		CRITCP_MAIL_STOP_BOUNCE,
		CRITCP_MAIL_STOP_BOUNCE_RESULT,
		CRITCP_MAIL_MONITOR_SEQUENCER_CALLBACK,
		CRITCP_MAIL_MONITOR_SEQUENCER_CALLBACK_RESULT,
		CRITCP_MAIL_MONITOR_ATOM_TRACK_MUTE,
		CRITCP_MAIL_MONITOR_ATOM_TRACK_MUTE_RESULT,
		CRITCP_MAIL_MONITOR_PREVIEW_INFO,
		CRITCP_MAIL_MONITOR_PREVIEW_INFO_RESULT,
		CRITCP_MAIL_MONITOR_STREAM_AWB_INFO,
		CRITCP_MAIL_MONITOR_STREAM_AWB_INFO_RESULT,                     /* 110 */
		CRITCP_MAIL_CONNECTION_VERSION,
		CRITCP_MAIL_CONNECTION_VERSION_RESULT,
		CRITCP_MAIL_MONITOR_ATOM_EXPLAYBACKINFO_PLAY_POSITION_WITH_PLAY_STATUS,
		CRITCP_MAIL_MONITOR_REJECT_OVERWRITE_ACB,
	};

	/* ログ用API識別番号 */
	protected const int logFuncBaseNum = 2000;
	protected enum LogFuncId {
		LOG_COMMAND_Non = 0,                                /* Non */
		LOG_COMMAND_Ex_Initialize,                          /* criAtomEx_Initialize */
		LOG_COMMAND_Ex_Finalize,                            /* criAtomEx_Finalize */
		LOG_COMMAND_ExAsr_Initialize,                       /* criAtomExAsr_Initialize */
		LOG_COMMAND_ExAsr_Finalize,                         /* criAtomExAsr_Finalize */
		LOG_COMMAND_ExHcaMx_Initialize,                     /* criAtomExHcaMx_Initialize */
		LOG_COMMAND_ExHcaMx_Finalize,                       /* criAtomExHcaMx_Finalize */
		LOG_COMMAND_Dbas_Create,                            /* criAtomDbas_Create */
		LOG_COMMAND_Dbas_Destroy,                           /* criAtomDbas_Destroy */
		LOG_COMMAND_StreamingCache_Create,                  /* criAtomStreamingCache_Create */
		LOG_COMMAND_StreamingCache_Destroy,                 /* criAtomStreamingCache_Destroy */
		LOG_COMMAND_ExVoicePool_AllocateStandardVoicePool,  /* criAtomExVoicePool_AllocateStandardVoicePool */
		LOG_COMMAND_ExVoicePool_AllocateAdxVoicePool,       /* criAtomExVoicePool_AllocateAdxVoicePool */
		LOG_COMMAND_ExVoicePool_AllocateAhxVoicePool,       /* criAtomExVoicePool_AllocateAhxVoicePool */
		LOG_COMMAND_ExVoicePool_AllocateHcaVoicePool,       /* criAtomExVoicePool_AllocateHcaVoicePool */
		LOG_COMMAND_ExVoicePool_AllocateHcaMxVoicePool,     /* criAtomExVoicePool_AllocateHcaMxVoicePool */
		LOG_COMMAND_ExVoicePool_AllocateWaveVoicePool,      /* criAtomExVoicePool_AllocateWaveVoicePool */
		LOG_COMMAND_ExVoicePool_AllocateRawPcmVoicePool,    /* criAtomExVoicePool_AllocateRawPcmVoicePool */
		LOG_COMMAND_ExVoicePool_AllocateAdpcmVoicePool_WII, /* criAtomExVoicePool_AllocateAdpcmVoicePool_WII */
		LOG_COMMAND_ExVoicePool_AllocateVagVoicePool_PSP,   /* criAtomExVoicePool_AllocateVagVoicePool_PSP */
		LOG_COMMAND_ExVoicePool_AllocateAdpcmVoicePool_3DS, /* criAtomExVoicePool_AllocateAdpcmVoicePool_3DS */
		LOG_COMMAND_ExVoicePool_AllocateVagVoicePool_VITA,  /* criAtomExVoicePool_AllocateVagVoicePool_VITA */
		LOG_COMMAND_ExVoicePool_AllocateAtrac3VoicePool_PSP,/* criAtomExVoicePool_AllocateAtrac3VoicePool_PSP */
		LOG_COMMAND_ExVoicePool_AllocateAt9VoicePool_VITA,  /* criAtomExVoicePool_AllocateAt9VoicePool_VITA */
		LOG_COMMAND_ExVoicePool_Free,                       /* criAtomExVoicePool_Free */
		LOG_COMMAND_ExPlayer_Create,                        /* criAtomExPlayer_Create */
		LOG_COMMAND_ExPlayer_Destroy,                       /* criAtomExPlayer_Destroy */
		LOG_COMMAND_ExTween_Create,                         /* criAtomExTween_Create */
		LOG_COMMAND_ExTween_Destroy,                        /* criAtomExTween_Destroy */
		LOG_COMMAND_Decrypter_Create,                       /* criAtomDecrypter_Create */
		LOG_COMMAND_Decrypter_Destroy,                      /* criAtomDecrypter_Destroy */
		LOG_COMMAND_Ex3dSource_Create,                      /* criAtomEx3dSource_Create */
		LOG_COMMAND_Ex3dSource_Destroy,                     /* criAtomEx3dSource_Destroy */
		LOG_COMMAND_Ex3dListener_Create,                    /* criAtomEx3dListener_Create */
		LOG_COMMAND_Ex3dListener_Destroy,                   /* criAtomEx3dListener_Destroy */
		LOG_COMMAND_ExPlayer_AttachFader,                   /* criAtomExPlayer_AttachFader */
		LOG_COMMAND_ExPlayer_DetachFader,                   /* criAtomExPlayer_DetachFader */
		LOG_COMMAND_Ex_RegisterAcfConfig,                   /* criAtomEx_RegisterAcfConfig */
		LOG_COMMAND_Ex_RegisterAcfData,                     /* criAtomEx_RegisterAcfData */
		LOG_COMMAND_Ex_RegisterAcfFile,                     /* criAtomEx_RegisterAcfFile */
		LOG_COMMAND_Ex_RegisterAcfFileById,                 /* criAtomEx_RegisterAcfFileById */
		LOG_COMMAND_Ex_UnregisterAcf,                       /* criAtomEx_UnregisterAcf */
		LOG_COMMAND_ExAcb_LoadAcbData,                      /* criAtomExAcb_LoadAcbData */
		LOG_COMMAND_ExAcb_LoadAcbDataById,                  /* criAtomExAcb_LoadAcbDataById */
		LOG_COMMAND_ExAcb_LoadAcbFile,                      /* criAtomExAcb_LoadAcbFile */
		LOG_COMMAND_ExAcb_LoadAcbFileById,                  /* criAtomExAcb_LoadAcbFileById */
		LOG_COMMAND_ExAcb_Release,                          /* criAtomExAcb_Release */
		LOG_COMMAND_ExAcb_ReleaseAll,                       /* criAtomExAcb_ReleaseAll */
		LOG_COMMAND_ExPlayer_Start,                         /* criAtomExPlayer_Start */
		LOG_COMMAND_ExPlayer_Prepare,                       /* criAtomExPlayer_Prepare */
		LOG_COMMAND_ExPlayer_Stop,                          /* criAtomExPlayer_Stop */
		LOG_COMMAND_ExPlayer_StopWithoutReleaseTime,        /* criAtomExPlayer_StopWithoutReleaseTime */
		LOG_COMMAND_ExPlayback_Stop,                        /* criAtomExPlayback_Stop */
		LOG_COMMAND_ExPlayback_StopWithoutReleaseTime,      /* criAtomExPlayback_StopWithoutReleaseTime */
		LOG_COMMAND_ExPlayer_Pause,                         /* criAtomExPlayer_Pause */
		LOG_COMMAND_ExPlayer_Resume,                        /* criAtomExPlayer_Resume */
		LOG_COMMAND_ExPlayback_Pause,                       /* criAtomExPlayback_Pause */
		LOG_COMMAND_ExPlayback_Resume,                      /* criAtomExPlayback_Resume */
		LOG_COMMAND_ExPlaybackInfo_AllocateInfo,            /* criAtomExPlaybackInfo_AllocateInfo */
		LOG_COMMAND_ExPlaybackInfo_FreeInfo,                /* criAtomExPlaybackInfo_FreeInfo */
		LOG_COMMAND_Error,                                  /* criError */
		LOG_COMMAND_ExPlaybackInfo_AddSound,                /* criAtomExPlaybackInfo_AddSound */
		LOG_COMMAND_ExPlaybackSound_FreeSound,              /* criAtomExPlaybackSound_FreeSound */
		LOG_COMMAND_SoundPlayer_Start,                      /* criAtomSoundPlayer_Start */
		LOG_COMMAND_SoundPlayer_Stop,                       /* criAtomSoundPlayer_Stop */
		LOG_COMMAND_SoundPlayer_StopWithoutRelease,         /* criAtomSoundPlayer_StopWithoutRelease */
		LOG_COMMAND_SoundPlayer_PausePlayback,              /* criAtomSoundPlayer_PausePlayback */
		LOG_COMMAND_SoundPlayer_SetWaveId,                  /* criAtomSoundPlayer_SetWaveId */
		LOG_COMMAND_SoundPlayer_SetContentId,               /* criAtomSoundPlayer_SetContentId */
		LOG_COMMAND_SoundPlayer_SetData,                    /* criAtomSoundPlayer_SetData */
		LOG_COMMAND_SoundPlayer_SetFileStringPointer,       /* criAtomSoundPlayer_SetFileStringPointer */
		LOG_COMMAND_ExPlayer_SetCueId,                      /* criAtomExPlayer_SetCueId */
		LOG_COMMAND_ExPlayer_SetCueName,                    /* criAtomExPlayer_SetCueName */
		LOG_COMMAND_ExPlayer_SetCueIndex,                   /* criAtomExPlayer_SetCueIndex */
		LOG_COMMAND_ExPlayer_SetData,                       /* criAtomExPlayer_SetData */
		LOG_COMMAND_ExPlayer_SetFile,                       /* criAtomExPlayer_SetFile */
		LOG_COMMAND_ExPlayer_SetContentId,                  /* criAtomExPlayer_SetContentId */
		LOG_COMMAND_ExPlayer_SetWaveId,                     /* criAtomExPlayer_SetWaveId */
		LOG_COMMAND_DbasId,                                 /* CriAtomDbasId */
		LOG_COMMAND_StreamingCacheId,                       /* CriAtomStreamingCacheId */
		LOG_COMMAND_ExVoicePoolHn,                          /* CriAtomExVoicePoolHn */
		LOG_COMMAND_ExPlayerHn,                             /* CriAtomExPlayerHn */
		LOG_COMMAND_ExTweenHn,                              /* CriAtomExTweenHn */
		LOG_COMMAND_DecrypterHn,                            /* CriAtomDecrypterHn */
		LOG_COMMAND_Ex3dSourceHn,                           /* CriAtomEx3dSourceHn */
		LOG_COMMAND_Ex3dListenerHn,                         /* CriAtomEx3dListenerHn */
		LOG_COMMAND_ExPlaybackId,                           /* CriAtomExPlaybackId */
		LOG_COMMAND_ExConfig,                               /* CriAtomExConfig */
		LOG_COMMAND_ExAsrConfig,                            /* CriAtomExAsrConfig */
		LOG_COMMAND_ExHcaMxConfig,                          /* CriAtomExHcaMxConfig */
		LOG_COMMAND_DbasConfig,                             /* CriAtomDbasConfig */
		LOG_COMMAND_StreamingCacheConfig,                   /* CriAtomStreamingCacheConfig */
		LOG_COMMAND_ExStandardVoicePoolConfig,              /* CriAtomExStandardVoicePoolConfig */
		LOG_COMMAND_ExAdxVoicePoolConfig,                   /* CriAtomExAdxVoicePoolConfig */
		LOG_COMMAND_ExAhxVoicePoolConfig,                   /* CriAtomExAhxVoicePoolConfig */
		LOG_COMMAND_ExHcaVoicePoolConfig,                   /* CriAtomExHcaVoicePoolConfig */
		LOG_COMMAND_ExHcaMxVoicePoolConfig,                 /* CriAtomExHcaMxVoicePoolConfig */
		LOG_COMMAND_ExWaveVoicePoolConfig,                  /* CriAtomExWaveVoicePoolConfig */
		LOG_COMMAND_ExRawPcmVoicePoolConfig,                /* CriAtomExRawPcmVoicePoolConfig */
		LOG_COMMAND_ExAdpcmVoicePoolConfig_3DS,             /* CriAtomExAdpcmVoicePoolConfig_3DS */
		LOG_COMMAND_ExAdpcmVoicePoolConfig_WII,             /* CriAtomExAdpcmVoicePoolConfig_WII */
		LOG_COMMAND_ExVagVoicePoolConfig_PSP,               /* CriAtomExVagVoicePoolConfig_PSP */
		LOG_COMMAND_ExAtrac3VoicePoolConfig_PSP,            /* CriAtomExAtrac3VoicePoolConfig_PSP */
		LOG_COMMAND_ExVagVoicePoolConfig_VITA,              /* CriAtomExVagVoicePoolConfig_VITA */
		LOG_COMMAND_ExAt9VoicePoolConfig_VITA,              /* CriAtomExAt9VoicePoolConfig_VITA */
		LOG_COMMAND_ExPlayerConfig,                         /* CriAtomExPlayerConfig */
		LOG_COMMAND_ExTweenConfig,                          /* CriAtomExTweenConfig */
		LOG_COMMAND_DecrypterConfig,                        /* CriAtomDecrypterConfig */
		LOG_COMMAND_ExAcfConfig,                            /* CriAtomExAcfConfig */
		LOG_COMMAND_Ex3dSourceConfig,                       /* CriAtomEx3dSourceConfig */
		LOG_COMMAND_Ex3dListenerConfig,                     /* CriAtomEx3dListenerConfig */
		LOG_COMMAND_ExFaderConfig,                          /* CriAtomExFaderConfig */
		LOG_COMMAND_ExAcbHn,                                /* CriAtomExAcbHn */
		LOG_COMMAND_ExFaderHn,                              /* CriAtomExFaderHn */
		LOG_COMMAND_PlayerPoolPlayerInfo,                   /* CriAtomPlayerPoolPlayerInfo */
		LOG_COMMAND_PlayerPool_ReleasePlayer,               /* criAtomPlayerPool_ReleasePlayer */
		LOG_COMMAND_SoundPlayer_Allocate_Element,           /* criatomsoundplayer_allocate_element */
		LOG_COMMAND_ExCategory_CancelCuePlayback,           /* criAtomExCategory_CancelCuePlayback */
		LOG_COMMAND_ExCue_StopByLimit,                      /* criAtomExCue_StopByLimit */
		LOG_COMMAND_ExCue_CancelCuePlayback,                /* criAtomExCue_CancelCuePlayback */
		LOG_COMMAND_CancelPlaybackByProbability,            /* criAtomExCue_CancelPlaybackByProbability */
		LOG_COMMAND_CancelPlaybackByCueTypeRandom,          /* criAtomExCue_CancelPlaybackByyCueTypeRandom */
		LOG_COMMAND_CancelPlaybackByCueTypeSwitch,          /* criAtomExCue_CancelPlaybackByyCueTypeSwitch */
		LOG_COMMAND_ExCategory_IncrementNumPlaybackCues,    /* criAtomExCategory_IncrementNumPlaybackCues */
		LOG_COMMAND_ExCategory_DecrementNumPlaybackCues,    /* criAtomExCategory_DecrementNumPlaybackCues */
		LOG_COMMAND_SoundVoice_Volume,                      /* criatomsoundvoice_parameter_update_on_changed:volume */
		LOG_COMMAND_SoundVoice_FreeVoice,                   /* criatomsoundvoice_FreeVoice */
		LOG_COMMAND_ExSequence_GetFreeBlock,                /* criAtomExSequence_GetFreeSequenceBlock */
		LOG_COMMAND_ExSequence_SetFreeBlock,                /* criAtomExSequence_SetFreeSequenceBlock */
		LOG_COMMAND_ExSequence_GetFreeSeqInfo,              /* criAtomExSequence_GetFreeSequenceInfo */
		LOG_COMMAND_ExSequence_SetFreeSeqInfo,              /* criAtomExSequence_SetFreeSequenceInfo */
		LOG_COMMAND_SoundVoice_Allocate,                    /* criAtomSoundVoice_AllocateVoice */
		LOG_COMMAND_GetAisacDestinationValue,               /* criAtomCueSheet_GetAisacDestinationValue */
		LOG_COMMAND_SequenceTrack_Mute,                     /* criAtomSequenceTrack_Mute */
		LOG_COMMAND_Preview_RequestSendLog,                 /* CriAtomMonitorLoc::MakeLogSendRequestPacket */
		LOG_COMMAND_RequestSendAcb,                         /* CriAtomMonitorLoc::MakeRequestPacket4SendAcb */
		LOG_COMMAND_Monitor_MakeClosePacket,                /* CriAtomMonitorLoc::MakeClosePacket */
		LOG_COMMAND_Monitor_MakeSendDataResultPacket,       /* CriAtomMonitorLoc::MakeSendDataResultPacket */
		LOG_COMMAND_CpuLoadAndNumUsedVoices,                /* CriAtomMonitorLoc::MakePerformanceInfoPacket */
		LOG_COMMAND_SequenceCallback,                       /* SequenceCallback */
		LOG_COMMAND_OverwriteAcf,                           /* CriAtomMonitorLoc::MakeRequestPacket4OverwriteAcf */
		LOG_COMMAND_StartLogging,                           /* CriAtomMonitorLoc::MakeLoggingStartTimePacket */
		LOG_COMMAND_SequenceLoopInfo,                       /* SequenceLoopInfo */
		LOG_COMMAND_Ex3dSource_Update,                      /* criAtomEx3dSource_Update */
		LOG_COMMAND_Ex3dListener_Update,                    /* criAtomEx3dListener_Update */
		LOG_COMMAND_ExVoicePool_AllocateAiffVoicePool,      /* criAtomExVoicePool_AllocateAiffVoicePool */
		LOG_COMMAND_ExAiffVoicePoolConfig,                  /* CriAtomExAiffVoicePoolConfig */
		LOG_COMMAND_ExVoicePool_AllocateAt9VoicePool_PS4,   /* criAtomExVoicePool_AllocateAt9VoicePool_PS4 */
		LOG_COMMAND_ExAt9VoicePoolConfig_PS4,               /* CriAtomExAt9VoicePoolConfig_PS4 */
		LOG_COMMAND_UserLog,                                /* criAtomExMonitor_OutputUserLog */
		LOG_COMMAND_ExCategoryConfig,                       /* CriAtomExCategoryConfig */
		LOG_COMMAND_SoundVoice_KillByLimit,                 /* criatomsoundvoice_KillByLimit */
		LOG_COMMAND_SoundVoice_Virtualize,                  /* criatomsoundvoice_Virtualize */
		LOG_COMMAND_SoundVoice_UnVirtualize,                /* criatomsoundvoice_UnVirtualize */
		LOG_COMMAND_AsrBusAnalyzeInfo,                      /* criAtomPreview_MakeAsrBusInfoPacket */
		LOG_COMMAND_LoudnessInfo,                           /* criAtomPreview_MakeLoudnessInfoPacket */
		LOG_COMMAND_StreamingInfo,                          /* criAtomPreview_MakeStreamingInfoPacket */
		LOG_COMMAND_PlayerPool_NumVoices,                   /* criAtomPreview_MakeLogPacketNumVoiceInVoicePool */
		LOG_COMMAND_ExVoicePool_AllocateAdpcmVoicePool_WIIU,/* criAtomExVoicePool_AllocateAdpcmVoicePool_WIIU */
		LOG_COMMAND_ExAdpcmVoicePoolConfig_WIIU,            /* CriAtomExAdpcmVoicePoolConfig_WIIU */
		LOG_COMMAND_StreamTypeMemory,                       /* StreamTypeMemory */
		LOG_COMMAND_StreamTypeStream,                       /* StreamTypeStream */
		LOG_COMMAND_StreamTypeZeroLatencyStream,            /* StreamTypeZeroLatencyStream */
		LOG_COMMAND_AsrBusAnalyzeInfoAllCh,                 /* criAtomPreview_MakeAsrBusInfoPacketForProfiler */
		LOG_COMMAND_ExPlayer_SetAisacControlValue,          /* criAtomExPlayer_SetAisacControlByName, ById */
		LOG_COMMAND_SequenceTrack_Start,                    /* SetupSeuenceTrack */
		LOG_COMMAND_SequenceTrack_Stop,                     /* criAtomSequence_FreeSequenceTrack */
		LOG_COMMAND_SoundPlayer_SetVibrationId,             /* criAtomSoundPlayer_SoundPlayer_SetVibrationId */
		LOG_COMMAND_ExGameVariableConfig,                   /* CriAtomExGameVariableConfig */
		LOG_COMMAND_SetGameVariable,                        /* SetGameVariableByname, ById */
		LOG_COMMAND_ExVibrationVoicePoolConfig,             /* CriAtomExVibrationVoicePoolConfig */
		LOG_COMMAND_ExVoicePool_AllocateVibrationVoicePool, /* criAtomExVoicePool_AllocateVibrationVoicePool */
		LOG_COMMAND_ExVoicePlayer_Pause,                    /* criAtomExPlayer_Pause */
		LOG_COMMAND_SoundPlayer_SetVibrationName,           /* criAtomSoundPlayer_SoundPlayer_SetVibrationName */
		LOG_COMMAND_ExCategory_Stop,                        /* criAtomExCategory_Stop */
		LOG_COMMAND_ExCategory_StopWithoutReleaseTime,      /* criAtomExCategory_StopWithoutReleaseTime */
		LOG_COMMAND_Ex3dTransceiver_Create,					/* criAtomEx3dTransceiver_Create */
		LOG_COMMAND_Ex3dTransceiver_Destroy,				/* criAtomEx3dTransceiver_Destroy */
		LOG_COMMAND_Ex3dTransceiverHn,						/* criAtomEx3dTransceiverHn */
		LOG_COMMAND_Ex3dTransceiverConfig,					/* criAtomEx3dTransceiverConfig */
		LOG_COMMAND_Ex3dTransceiver_UpdateInput,			/* criAtomEx3dTransceiver_UpdateInput */
		LOG_COMMAND_Ex3dTransceiver_UpdateOutput,			/* criAtomEx3dTransceiver_UpdateOutput */
		LOG_COMMAND_SoundVoice_CalcFinalVoiceParamAndSilentVolumeCore,	/* criAtomVoice_CalcFinalVoiceParamAndSilentVolumeCore */
		LOG_COMMAND_AdmPlayer_Create,						/* criAtomAdmPlayer_Create */
		LOG_COMMAND_AdmPlayer_Destroy,						/* criAtomAdmPlayer_Destroy */
		LOG_COMMAND_AdmPlayer_InternalStart,				/* criatomadmplayer_internal_start */	/* フェーズ単位の再生処理 */
		LOG_COMMAND_AdmPlayer_Start,						/* criAtomAdmPlayer_Start */
		LOG_COMMAND_AdmPlayer_Stop,							/* criAtomAdmPlayer_Stop */
		LOG_COMMAND_AdmPlayer_StopByPhraseEnd,				/* criAtomAdmPlayer_StopByPhraseEnd */
	};

	protected enum LogParamId {
		LOG_STRINGS_ITEM_CRIATOMDEF = 0,                /* #CRIATOMDEF */
		LOG_STRINGS_ITEM_CRIATOM,                       /* #CRIATOM */
		LOG_STRINGS_ITEM_TimeStamp,                     /* TimeStamp */
		LOG_STRINGS_ITEM_ThreadId,                      /* ThreadId */
		LOG_STRINGS_ITEM_ThreadModel,                   /* thread_model */
		LOG_STRINGS_ITEM_ServerFrequency,               /* server_frequency */
		LOG_STRINGS_ITEM_ParameterUpdateInterval,       /* parameter_update_interval */
		LOG_STRINGS_ITEM_MaxVirtualVoices,              /* max_virtual_voices */
		LOG_STRINGS_ITEM_MaxVoiceLimitGroups,           /* max_voice_limit_groups */
		LOG_STRINGS_ITEM_MaxCategories,                 /* max_categories */
		LOG_STRINGS_ITEM_MaxSequences,                  /* max_sequences */
		LOG_STRINGS_ITEM_MaxTracks,                     /* max_tracks */
		LOG_STRINGS_ITEM_MaxTrackItems,                 /* max_track_items */
		LOG_STRINGS_ITEM_MaxAisacAutoModulations,       /* max_aisac_auto_modulations */
		LOG_STRINGS_ITEM_MaxPitch,                      /* max_pitch */
		LOG_STRINGS_ITEM_CoordinateSystem,              /* coordinate_system */
		LOG_STRINGS_ITEM_RngIf,                         /* rng_if */
		LOG_STRINGS_ITEM_FsConfig,                      /* fs_config */
		LOG_STRINGS_ITEM_Context,                       /* context */
		LOG_STRINGS_ITEM_OutputChannels,                /* output_channels */
		LOG_STRINGS_ITEM_OutputSamplingRate,            /* output_sampling_rate */
		LOG_STRINGS_ITEM_SoundRendererType,             /* sound_renderer_type */
		LOG_STRINGS_ITEM_NumMixers,                     /* num_mixers */
		LOG_STRINGS_ITEM_MaxVoices,                     /* max_voices */
		LOG_STRINGS_ITEM_MaxInputChannels,              /* max_input_channels */
		LOG_STRINGS_ITEM_MaxSamplingRate,               /* max_sampling_rate */
		LOG_STRINGS_ITEM_Identifier,                    /* identifier */
		LOG_STRINGS_ITEM_MaxStreams,                    /* max_streams */
		LOG_STRINGS_ITEM_MaxBps,                        /* max_bps */
		LOG_STRINGS_ITEM_DbasId,                        /* CriAtomDbasId */
		LOG_STRINGS_ITEM_MaxPath,                       /* max_path */
		LOG_STRINGS_ITEM_MaxFiles,                      /* max_files */
		LOG_STRINGS_ITEM_CacheSize,                     /* cache_size */
		LOG_STRINGS_ITEM_StreamingCacheId,              /* CriAtomStreamingCacheId */
		LOG_STRINGS_ITEM_NumVoices,                     /* num_voices */
		LOG_STRINGS_ITEM_MaxChannels,                   /* max_channels */
		LOG_STRINGS_ITEM_StreamingFlag,                 /* streaming_flag */
		LOG_STRINGS_ITEM_DecodeLatency,                 /* decode_latency */
		LOG_STRINGS_ITEM_ExVoicePoolHn,                 /* CriAtomExVoicePoolHn */
		LOG_STRINGS_ITEM_AllocationMethod,              /* allocation_method */
		LOG_STRINGS_ITEM_MaxPathStrings,                /* max_path_strings */
		LOG_STRINGS_ITEM_UpdatesTime,                   /* updates_time */
		LOG_STRINGS_ITEM_ExPlayerHn,                    /* CriAtomExPlayerHn */
		LOG_STRINGS_ITEM_Id,                            /* id */
		LOG_STRINGS_ITEM_ParameterType,                 /* parameter_type */
		LOG_STRINGS_ITEM_Key,                           /* key */
		LOG_STRINGS_ITEM_DecrypterHn,                   /* CriAtomDecrypterHn */
		LOG_STRINGS_ITEM_Work,                          /* work */
		LOG_STRINGS_ITEM_WorkSize,                      /* work_size */
		LOG_STRINGS_ITEM_Ex3dSourceHn,                  /* CriAtomEx3dSourceHn */
		LOG_STRINGS_ITEM_Ex3dListenerHn,                /* CriAtomEx3dListenerHn */
		LOG_STRINGS_ITEM_ExPlaybackId,                  /* CriAtomExPlaybackId */
		LOG_STRINGS_ITEM_ExFaderConfig,                 /* CriAtomExFaderConfig */
		LOG_STRINGS_ITEM_ExAcfConfig,                   /* CriAtomExAcfConfig */
		LOG_STRINGS_ITEM_AcfData,                       /* acf_data */
		LOG_STRINGS_ITEM_AcfDataSize,                   /* acf_data_size */
		LOG_STRINGS_ITEM_FsBinderHn,                    /* CriFsBinderHn */
		LOG_STRINGS_ITEM_Path,                          /* path */
		LOG_STRINGS_ITEM_AcbData,                       /* acb_data */
		LOG_STRINGS_ITEM_AcbDataSize,                   /* acb_data_size */
		LOG_STRINGS_ITEM_AwbPath,                       /* awb_path */
		LOG_STRINGS_ITEM_AwbId,                         /* awb_id */
		LOG_STRINGS_ITEM_AcbPath,                       /* acb_path */
		LOG_STRINGS_ITEM_AcbId,                         /* acb_id */
		LOG_STRINGS_ITEM_ExAcbHn,                       /* CriAtomExAcbHn */
		LOG_STRINGS_ITEM_Sw,                            /* sw */
		LOG_STRINGS_ITEM_ExResumeMode,                  /* CriAtomExResumeMode */
		LOG_STRINGS_ITEM_ErrorString,                   /* error_string */
		LOG_STRINGS_ITEM_SoundPlaybackId,               /* CriAtomSoundPlaybackId */
		LOG_STRINGS_ITEM_SoundPlayerHn,                 /* CriAtomSoundPlayerHn */
		LOG_STRINGS_ITEM_AwbHn,                         /* CriAtomAwbHn */
		LOG_STRINGS_ITEM_ExCueId,                       /* CriAtomExCueId */
		LOG_STRINGS_ITEM_CueName,                       /* cue_name */
		LOG_STRINGS_ITEM_ExCueIndex,                    /* CriAtomExCueIndex */
		LOG_STRINGS_ITEM_Buffer,                        /* buffer */
		LOG_STRINGS_ITEM_Size,                          /* size */
		LOG_STRINGS_ITEM_ExWaveId,                      /* CriAtomExWaveId */
		LOG_STRINGS_ITEM_MemoryAwbHn,                   /* CriAtomAwbHn for Memory */
		LOG_STRINGS_ITEM_StreamAwbHn,                   /* CriAtomAwbHn for Stream */
		LOG_STRINGS_ITEM_ExTweenHn,                     /* CriAtomExTweenHn */
		LOG_STRINGS_ITEM_ExConfig,                      /* CriAtomExConfig */
		LOG_STRINGS_ITEM_ExAsrConfig,                   /* CriAtomExAsrConfig */
		LOG_STRINGS_ITEM_ExHcaMxConfig,                 /* CriAtomExHcaMxConfig */
		LOG_STRINGS_ITEM_ExDbasConfig,                  /* CriAtomExDbasConfig */
		LOG_STRINGS_ITEM_ExStreamingCacheConfig,        /* CriAtomExStreamingCacheConfig */
		LOG_STRINGS_ITEM_ExStandardVoicePoolConfig,     /* CriAtomExStandardVoicePoolConfig */
		LOG_STRINGS_ITEM_ExAdxVoicePoolConfig,          /* CriAtomExAdxVoicePoolConfig */
		LOG_STRINGS_ITEM_ExAhxVoicePoolConfig,          /* CriAtomExAhxVoicePoolConfig */
		LOG_STRINGS_ITEM_ExHcaVoicePoolConfig,          /* CriAtomExHcaVoicePoolConfig */
		LOG_STRINGS_ITEM_ExHcaMxVoicePoolConfig,        /* CriAtomExHcaMxVoicePoolConfig */
		LOG_STRINGS_ITEM_ExWaveVoicePoolConfig,         /* CriAtomExWaveVoicePoolConfig */
		LOG_STRINGS_ITEM_ExRawPcmVoicePoolConfig,       /* CriAtomExRawPcmVoicePoolConfig */
		LOG_STRINGS_ITEM_ExPlayerConfig,                /* CriAtomExPlayerConfig */
		LOG_STRINGS_ITEM_ExTweenConfig,                 /* CriAtomExTweenConfig */
		LOG_STRINGS_ITEM_DecrypterConfig,               /* CriAtomDecrypterConfig */
		LOG_STRINGS_ITEM_Ex3dSourceConfig,              /* CriAtomEx3dSourceConfig */
		LOG_STRINGS_ITEM_Ex3dListenerConfig,            /* CriAtomEx3dListenerConfig */
		LOG_STRINGS_ITEM_ExAdpcmVoicePoolConfig_3DS,    /* CriAtomExAdpcmVoicePoolConfig_3DS */
		LOG_STRINGS_ITEM_ExAdpcmVoicePoolConfig_WII,    /* CriAtomExAdpcmVoicePoolConfig_WII */
		LOG_STRINGS_ITEM_ExVagVoicePoolConfig_PSP,      /* CriAtomExVagVoicePoolConfig_PSP */
		LOG_STRINGS_ITEM_ExAtrac3VoicePoolConfig_PSP,   /* CriAtomExAtrac3VoicePoolConfig_PSP */
		LOG_STRINGS_ITEM_ExVagVoicePoolConfig_VITA,     /* CriAtomExVagVoicePoolConfig_VITA */
		LOG_STRINGS_ITEM_ExAt9VoicePoolConfig_VITA,     /* CriAtomExAt9VoicePoolConfig_VITA */
		LOG_STRINGS_ITEM_NumGroups,                     /* num_groups */
		LOG_STRINGS_ITEM_VoicesPerGroup,                /* voices_per_group */
		LOG_STRINGS_ITEM_NumCategoryGroups,             /* num_category_groups */
		LOG_STRINGS_ITEM_NumCategories,                 /* num_categories */
		LOG_STRINGS_ITEM_reserved,                      /* reserved */
		LOG_STRINGS_ITEM_ExFaderHn,                     /* CriAtomExFaderHn */
		LOG_STRINGS_ITEM_Guid,                          /* Guid */
		LOG_STRINGS_ITEM_parent_info_id,                /* parent CriAtomExPlaybackId */
		LOG_STRINGS_ITEM_PlayerPoolPlayerInfo,          /* CriAtomPlayerPoolPlayerInfo */
		LOG_STRINGS_ITEM_SoundElementHn,                /* CriAtomSoundElementHn */
		LOG_STRINGS_ITEM_SoundVoiceHn,                  /* CriAtomSoundVoiceHn */
		LOG_STRINGS_ITEM_ExPlaybackId_Cause,            /* Cause CriAtomExPlaybackId */
		LOG_STRINGS_ITEM_Index,                         /* Index */
		LOG_STRINGS_ITEM_NumAllPlaybacks,               /* NumAllPlaybacks */
		LOG_STRINGS_ITEM_NumPlaybacks,                  /* NumPlaybacks */
		LOG_STRINGS_ITEM_CategoriesPerPlayback,         /* categories_per_playback */
		LOG_STRINGS_ITEM_EnableVoicePriorityDecay,      /* enable_voice_priority_decay */
		LOG_STRINGS_ITEM_Volume,                        /* volume */
		LOG_STRINGS_ITEM_SoundElementId,                /* CriAtomSoundElementId */
		LOG_STRINGS_ITEM_SoundVoiceId,                  /* CriAtomSoundVoiceId */
		LOG_STRINGS_ITEM_ExAcbName,                     /* Acb Name */
		LOG_STRINGS_ITEM_PlayerPoolPlayerInfoId,        /* CriAtomPlayerPoolPlayerInfoId */
		LOG_STRINGS_ITEM_AisacControl,                  /* Aisac Control */
		LOG_STRINGS_ITEM_TrackNo,                       /* track no */
		LOG_STRINGS_ITEM_Mute,                          /* mute */
		LOG_STRINGS_ITEM_Result,                        /* Result */
		LOG_STRINGS_ITEM_LogRecordMode,                 /* log record mode */
		LOG_STRINGS_ITEM_NumCh,                         /* Num Ch */
		LOG_STRINGS_ITEM_NumLoaders,                    /* Num Loaders */
		LOG_STRINGS_ITEM_NumPlayers,                    /* Num Players */
		LOG_STRINGS_ITEM_Status,                        /* Status */
		LOG_STRINGS_ITEM_PlayingTime,                   /* PlayingTime */
		LOG_STRINGS_ITEM_DspBusSpectra,                 /* DspBusSpectra */
		LOG_STRINGS_ITEM_CpuLoad,                       /* CpuLoad */
		LOG_STRINGS_ITEM_NumUsedVoices,                 /* NumUsedVoices */
		LOG_STRINGS_ITEM_SequencePlaybackPosition,      /* SequencePlaybackPosition */
		LOG_STRINGS_ITEM_CallbackValue,                 /* CallbackValue */
		LOG_STRINGS_ITEM_CallbackString,                /* CallbackString */
		LOG_STRINGS_ITEM_PeakLevel,                     /* PeakLevel */
		LOG_STRINGS_ITEM_RmsLevel,                      /* RmsLevel */
		LOG_STRINGS_ITEM_PeakHoldLevel,                 /* PeakHoldLevel */
		LOG_STRINGS_ITEM_RequestId,                     /* RequestId */
		LOG_STRINGS_ITEM_TargetId,                      /* TargetId */
		LOG_STRINGS_ITEM_Md5,                           /* Md5 */
		LOG_STRINGS_ITEM_GameVariable,                  /* GameVariable */
		LOG_STRINGS_ITEM_GameVariableName,              /* GameVariableName */
		LOG_STRINGS_ITEM_TimeMs,                        /* TimeMs */
		LOG_STRINGS_ITEM_SnapShotName,                  /* SnapShotName */
		LOG_STRINGS_ITEM_AisacControlId,                /* AisacControlId */
		LOG_STRINGS_ITEM_StartTimeMs,                   /* StartTimeMs */
		LOG_STRINGS_ITEM_SelectorName,                  /* SelectorName */
		LOG_STRINGS_ITEM_LabelName,                     /* LabelName */
		LOG_STRINGS_ITEM_BlockName,                     /* BlockName */
		LOG_STRINGS_ITEM_CategoryName,                  /* CategoryName */
		LOG_STRINGS_ITEM_AisacControlName,              /* AisacControlName */
		LOG_STRINGS_ITEM_SettingName,                   /* SettingName */
		LOG_STRINGS_ITEM_CueSheetId,                    /* CueSheetId */
		LOG_STRINGS_ITEM_BusNo,                         /* BusNo */
		LOG_STRINGS_ITEM_FxType,                        /* FxType */
		LOG_STRINGS_ITEM_RemainedLoopCount,             /* RemainedLoopCount */
		LOG_STRINGS_ITEM_SequenceLoopId,                /* SequenceLoopId */
		LOG_STRINGS_ITEM_Ex3dPosVector_Position,        /* 3dPosVector_Position */
		LOG_STRINGS_ITEM_Ex3dPosVector_Velocity,        /* 3dPosVector_Velocity */
		LOG_STRINGS_ITEM_Ex3dPosVector_Forward,         /* 3dPosVector_Forward */
		LOG_STRINGS_ITEM_Ex3dPosVector_Upward,          /* 3dPosVector_Upward */
		LOG_STRINGS_ITEM_Ex3dPosVector_FocusPoint,      /* 3dPosVector_FocusPoint */
		LOG_STRINGS_ITEM_Ex3dPosVector_Cone,            /* 3dPosVector_Cone */
		LOG_STRINGS_ITEM_Ex3dMaxAngleAisacDelta,        /* 3dMaxAngleAisacDelta */
		LOG_STRINGS_ITEM_Ex3dEnablePriorityDecay,       /* 3dEnablePriorityDecay */
		LOG_STRINGS_ITEM_Ex3dDistanceFactor,            /* 3dDistanceFactor */
		LOG_STRINGS_ITEM_Ex3dDistanceFocusLevel,        /* 3dDistanceFocusLevel */
		LOG_STRINGS_ITEM_Ex3dDirectionFocusLevel,       /* 3dDirectionFocusLevel */
		LOG_STRINGS_ITEM_Result3dPos,                   /* Result3dPos */
		LOG_STRINGS_ITEM_ExAiffVoicePoolConfig,         /* CriAtomExAiffVoicePoolConfig */
		LOG_STRINGS_ITEM_SrType,                        /* SoundRendererTyoe */
		LOG_STRINGS_ITEM_ExAt9VoicePoolConfig_PS4,      /* CriAtomExAt9VoicePoolConfig_PS4 */
		LOG_STRINGS_ITEM_AverageServerTime,             /* AverageServerTime */
		LOG_STRINGS_ITEM_AverageServerInterval,         /* AverageServerInterval */
		LOG_STRINGS_ITEM_MaxServerTime,                 /* MaxServerTime */
		LOG_STRINGS_ITEM_MaxServerInterval,             /* MaxServerInterval */
		LOG_STRINGS_ITEM_UserLog,                       /* UserLog */
		LOG_STRINGS_ITEM_ByVoiceGroupLimitation,        /* ByVoiceGroupLimitation */
		LOG_STRINGS_ITEM_ByVoicePoolLimitation,         /* ByVoicePoolLimitation */
		LOG_STRINGS_ITEM_RetryFlag,                     /* RetryFlag */
		LOG_STRINGS_ITEM_BusName,                       /* BusName */
		LOG_STRINGS_ITEM_StreamType,                    /* StreamType */
		LOG_STRINGS_ITEM_MomentaryValue,                /* MomentaryValue */
		LOG_STRINGS_ITEM_ShortTermValue,                /* ShortTermValue */
		LOG_STRINGS_ITEM_IntegratedValue,               /* IntegratedValue */
		LOG_STRINGS_ITEM_TotalBps,                      /* TotalBps */
		LOG_STRINGS_ITEM_NumCues,                       /* num_cues */
		LOG_STRINGS_ITEM_SoundFormat,                   /* SoundFormat *//* (CriAtomSoundFormatのこと) */
		LOG_STRINGS_ITEM_ExAdpcmVoicePoolConfig_WIIU,   /* CriAtomExAdpcmVoicePoolConfig_WIIU */
		LOG_STRINGS_ITEM_AwbName,                       /* AwbName */
		LOG_STRINGS_ITEM_NumStreamAwb,                  /* NumStreamAwb */
		LOG_STRINGS_ITEM_ExPlayback_AllocateModule,     /* ExPlayback_AllocateModule */
		LOG_STRINGS_ITEM_AisacControlValue,             /* AisacControlValue */
		LOG_STRINGS_ITEM_NumAllPlaybacksForReact,       /* NumAllPlaybacksForReact */
		LOG_STRINGS_ITEM_PreviewContext,                /* PreviewContext */
		LOG_STRINGS_ITEM_MaxParameterBlocks,            /* max_parameter_blocks */
		LOG_STRINGS_ITEM_MaxFaders,                     /* max_faders */
		LOG_STRINGS_ITEM_NumBuses,                      /* num_buses */
		LOG_STRINGS_ITEM_MaxRacks,                      /* max_racks */
		LOG_STRINGS_ITEM_OutputChannels4HcaMx,          /* output_channels(HcaMxConfig用) */
		LOG_STRINGS_ITEM_OutputSamplingRate4HcaMx,      /* output_sampling_rate(HcaMxConfig用) */
		LOG_STRINGS_ITEM_SoundRendererType4HcaMx,       /* sound_renderer_type(HcaMxConfig用) */
		LOG_STRINGS_ITEM_SPEAKER_SYSTEM,                /* speaker_system */
		LOG_STRINGS_ITEM_SPEAKER_ANGLE_LEFT,            /* left_speaker_angle */
		LOG_STRINGS_ITEM_SPEAKER_ANGLE_RIGHT,           /* right_speaker_angle */
		LOG_STRINGS_ITEM_SPEAKER_ANGLE_CENTER,          /* center_speaker_angle */
		LOG_STRINGS_ITEM_SPEAKER_ANGLE_LFE,             /* lfe_speaker_angle */
		LOG_STRINGS_ITEM_SPEAKER_ANGLE_SURROUND_LEFT,   /* surround_left_speaker_angle */
		LOG_STRINGS_ITEM_SPEAKER_ANGLE_SURROUND_RIGHT,  /* surround_right_speaker_angle */
		LOG_STRINGS_ITEM_SPEAKER_ANGLE_SURROUND_BACK_LEFT,      /* surround_back_left_speaker_angle */
		LOG_STRINGS_ITEM_SPEAKER_ANGLE_SURROUND_BACK_RIGHT,     /* surround_back_right_speaker_angle */
		LOG_STRINGS_ITEM_PAN_SPEAKER_TYPE,              /* pan_speaker_type */
		LOG_STRINGS_ITEM_VOICE_STOP_REASON,             /* CriAtomVoiceStopReason */
		LOG_STRINGS_ITEM_ExVibrationVoicePoolConfig,    /* CriAtomExVibrationVoicePoolConfig */
		LOG_STRINGS_ITEM_TouchSenceEffectName,          /* TouchSenceEffectName */
		LOG_STRINGS_ITEM_DspName,                       /* DspName */
		LOG_STRINGS_ITEM_DspObject,                     /* DspObject */
		LOG_STRINGS_ITEM_DspSlotNo,                     /* DspSlotNo */
		LOG_STRINGS_ITEM_DspPluginType,                 /* DspPluginType */
		LOG_STRINGS_ITEM_Ex3dPosVector_ListenerTop,     /* 3dPosVector_ListenerTop */
		LOG_STRINGS_ITEM_Playback_Status,               /* Playback_Status */
		LOG_STRINGG_ITEM_InstrumentInstanceCbFunc,
		LOG_STRINGG_ITEM_InstrumentInstanceCbObj,
		LOG_STRINGG_ITEM_AttachInstrumentInstancePlayerHn,
		LOG_STRINGG_ITEM_InstrumentInstance,
		LOG_STRINGS_ITEM_Ex3dTransceiverHn,             /* CriAtomEx3dTransceiverHn */
		LOG_STRINGS_ITEM_Ex3dTransceiverConfig,         /* CriAtomEx3dTransceiverConfig */
		LOG_STRINGS_ITEM_Ex3dRegionHn,                  /* CriAtomExRegionHn */
		LOG_STRINGS_ITEM_Ex3dTransceiverDirectAudioRadius,  /* TransceiverDirectAudioRadius */
		LOG_STRINGS_ITEM_Ex3dTransceiverCrossFadeDistance,  /* TransceiverCrossFadeDistance */
		LOG_STRINGS_ITEM_ProgramNo,                     /* サウンドプログラムバングプレビュー用Program No */
		LOG_STRINGS_ITEM_KeyNo,                         /* サウンドプログラムバングプレビュー用Key No */
		LOG_STRINGS_ITEM_Velocity,                      /* サウンドプログラムバングプレビュー用Velocity */
		LOG_STRINGS_ITEM_PitchBend,                     /* サウンドプログラムバングプレビュー用PitchBend */
		LOG_STRINGS_ITEM_Format,						/* コーデックフォーマット：HCA, ADX, HCA-MX... */
		LOG_STRINGS_ITEM_MaxRhythmTracks,				/* CriAtomAdmPlayerConfig.max_rhythm_tracks */
		LOG_STRINGS_ITEM_MaxMelodyTracks,				/* CriAtomAdmPlayerConfig.max_melody_tracks */
		LOG_STRINGS_ITEM_MaxVocalTracks,				/* CriAtomAdmPlayerConfig.max_vocal_tracks */
		LOG_STRINGS_ITEM_AdmPlayerHn,					/* CriAtomAdmPlayerHn */
	};

	/* ログのパラメータ引数の型 */
	protected enum LogParamTypes {
		TYPE_INT8 = 0,
		TYPE_INT16,
		TYPE_INT32,
		TYPE_INT64,
		TYPE_FLOAT32,
		TYPE_CHAR,
		TYPE_UINTPTR,
		TYPE_GUID,
		TYPE_128,
		TYPE_VECTOR
	}
	static protected readonly int[] ParamTypeSizes32 = new int[] {
		1,      /* SIZE_INT8 */
		2,      /* SIZE_INT16 */
		4,      /* SIZE_INT32 */
		8,      /* SIZE_INT64 */
		4,      /* SIZE_FLOAT32 */
		2,      /* SIZE_CHAR (String Size) */
		4,      /* SIZE_UINTPTR (Handles) */
		16,     /* SIZE_GUID */
		128,    /* SIZE_128 */
		12      /* SIZE_VECTOR (Vector3f) */
	};
	static protected readonly int[] ParamTypeSizes64 = new int[] {
		1,      /* SIZE_INT8 */
		2,      /* SIZE_INT16 */
		4,      /* SIZE_INT32 */
		8,      /* SIZE_INT64 */
		4,      /* SIZE_FLOAT32 */
		2,      /* SIZE_CHAR (String Size) */
		8,      /* SIZE_UINTPTR (Handles) */
		16,     /* SIZE_GUID */
		128,    /* SIZE_128 */
		12      /* SIZE_VECTOR (Vector3f) */
	};

	protected struct LogParam {
		public readonly string name;
		public readonly LogParamTypes type;
		public readonly int size32;
		public readonly int size64;
		public LogParam(string name, LogParamTypes type)
		{
			this.name = name;
			this.type = type;
			this.size32 = CriProfiler.ParamTypeSizes32[(int)type];
			this.size64 = CriProfiler.ParamTypeSizes64[(int)type];
		}
	}

	static protected readonly LogParam[] LogParams = new LogParam[] {
		new LogParam( "#CRIATOMDEF",                    LogParamTypes.TYPE_CHAR ),                  /*   0 */
		new LogParam( "#CRIATOM",                       LogParamTypes.TYPE_CHAR ),                  /*   1 */
		new LogParam( "TimeStamp[usec]",                LogParamTypes.TYPE_INT64 ),                 /*   2 */
		new LogParam( "ThreadId",                       LogParamTypes.TYPE_UINTPTR ),               /*   3 */
		new LogParam( "thread_model",                   LogParamTypes.TYPE_INT8 ),                  /*   4 */
		new LogParam( "server_frequency",               LogParamTypes.TYPE_FLOAT32 ),               /*   5 */
		new LogParam( "parameter_update_interval",      LogParamTypes.TYPE_INT16 ),                 /*   6 */
		new LogParam( "max_virtual_voices",             LogParamTypes.TYPE_INT16 ),                 /*   7 */
		new LogParam( "max_voice_limit_groups",         LogParamTypes.TYPE_INT16 ),                 /*   8 */
		new LogParam( "max_categories",                 LogParamTypes.TYPE_INT16 ),                 /*   9 */
		new LogParam( "max_sequences",                  LogParamTypes.TYPE_INT16 ),                 /*  10 */
		new LogParam( "max_tracks",                     LogParamTypes.TYPE_INT16 ),                 /*  11 */
		new LogParam( "max_track_items",                LogParamTypes.TYPE_INT16 ),                 /*  12 */
		new LogParam( "max_aisac_auto_modulations",     LogParamTypes.TYPE_INT16 ),                 /*  13 */
		new LogParam( "max_pitch",                      LogParamTypes.TYPE_FLOAT32 ),               /*  14 */
		new LogParam( "coordinate_system",              LogParamTypes.TYPE_INT8 ),                  /*  15 */
		new LogParam( "rng_if",                         LogParamTypes.TYPE_UINTPTR ),               /*  16 */
		new LogParam( "fs_config",                      LogParamTypes.TYPE_UINTPTR ),               /*  17 */
		new LogParam( "context",                        LogParamTypes.TYPE_UINTPTR ),               /*  18 */
		new LogParam( "output_channels",                LogParamTypes.TYPE_INT8 ),                  /*  19 */
		new LogParam( "output_sampling_rate",           LogParamTypes.TYPE_INT32 ),                 /*  20 */
		new LogParam( "sound_renderer_type",            LogParamTypes.TYPE_INT8 ),                  /*  21 */
		new LogParam( "num_mixers",                     LogParamTypes.TYPE_INT16 ),                 /*  22 */
		new LogParam( "max_voices",                     LogParamTypes.TYPE_INT16 ),                 /*  23 */
		new LogParam( "max_input_channels",             LogParamTypes.TYPE_INT8 ),                  /*  24 */
		new LogParam( "max_sampling_rate",              LogParamTypes.TYPE_INT32 ),                 /*  25 */
		new LogParam( "identifier",                     LogParamTypes.TYPE_INT32 ),                 /*  26 */
		new LogParam( "max_streams",                    LogParamTypes.TYPE_INT32 ),                 /*  27 */
		new LogParam( "max_bps",                        LogParamTypes.TYPE_INT32 ),                 /*  28 */
		new LogParam( "CriAtomDbasId",                  LogParamTypes.TYPE_INT32 ),                 /*  29 */
		new LogParam( "max_path",                       LogParamTypes.TYPE_INT16 ),                 /*  30 */
		new LogParam( "max_files",                      LogParamTypes.TYPE_INT32 ),                 /*  31 */
		new LogParam( "cache_size",                     LogParamTypes.TYPE_INT32 ),                 /*  32 */
		new LogParam( "CriAtomStreamingCacheId",        LogParamTypes.TYPE_UINTPTR ),               /*  33 */
		new LogParam( "num_voices",                     LogParamTypes.TYPE_INT32 ),                 /*  34 */
		new LogParam( "max_channels",                   LogParamTypes.TYPE_INT8 ),                  /*  35 */
		new LogParam( "streaming_flag",                 LogParamTypes.TYPE_INT8 ),                  /*  36 */
		new LogParam( "decode_latency",                 LogParamTypes.TYPE_INT32 ),                 /*  37 */
		new LogParam( "CriAtomExVoicePoolHn",           LogParamTypes.TYPE_UINTPTR ),               /*  38 */
		new LogParam( "allocation_method",              LogParamTypes.TYPE_INT8 ),                  /*  39 */
		new LogParam( "max_path_strings",               LogParamTypes.TYPE_INT16 ),                 /*  40 */
		new LogParam( "updates_time",                   LogParamTypes.TYPE_INT8 ),                  /*  41 */
		new LogParam( "CriAtomExPlayerHn",              LogParamTypes.TYPE_UINTPTR ),               /*  42 */
		new LogParam( "id",                             LogParamTypes.TYPE_INT32 ),                 /*  43 */
		new LogParam( "parameter_type",                 LogParamTypes.TYPE_INT8 ),                  /*  44 */
		new LogParam( "key",                            LogParamTypes.TYPE_INT64 ),                 /*  45 */
		new LogParam( "CriAtomDecrypterHn",             LogParamTypes.TYPE_UINTPTR ),               /*  46 */
		new LogParam( "work",                           LogParamTypes.TYPE_UINTPTR ),               /*  47 */
		new LogParam( "work_size",                      LogParamTypes.TYPE_INT32 ),                 /*  48 */
		new LogParam( "CriAtomEx3dSourceHn",            LogParamTypes.TYPE_UINTPTR ),               /*  49 */
		new LogParam( "CriAtomEx3dListenerHn",          LogParamTypes.TYPE_UINTPTR ),               /*  50 */
		new LogParam( "CriAtomExPlaybackId",            LogParamTypes.TYPE_INT32 ),                 /*  51 */
		new LogParam( "CriAtomExFaderConfig",           LogParamTypes.TYPE_UINTPTR ),               /*  52 */
		new LogParam( "CriAtomExAcfConfig",             LogParamTypes.TYPE_UINTPTR ),               /*  53 */
		new LogParam( "acf_data",                       LogParamTypes.TYPE_UINTPTR ),               /*  54 */
		new LogParam( "acf_data_size",                  LogParamTypes.TYPE_INT32 ),                 /*  55 */
		new LogParam( "CriFsBinderHn",                  LogParamTypes.TYPE_UINTPTR ),               /*  56 */
		new LogParam( "path",                           LogParamTypes.TYPE_CHAR ),                  /*  57 */
		new LogParam( "acb_data",                       LogParamTypes.TYPE_UINTPTR ),               /*  58 */
		new LogParam( "acb_data_size",                  LogParamTypes.TYPE_INT32 ),                 /*  59 */
		new LogParam( "awb_path",                       LogParamTypes.TYPE_CHAR ),                  /*  60 */
		new LogParam( "awb_id",                         LogParamTypes.TYPE_INT32 ),                 /*  61 */
		new LogParam( "acb_path",                       LogParamTypes.TYPE_CHAR ),                  /*  62 */
		new LogParam( "acb_id",                         LogParamTypes.TYPE_INT32 ),                 /*  63 */
		new LogParam( "CriAtomExAcbHn",                 LogParamTypes.TYPE_UINTPTR ),               /*  64 */
		new LogParam( "sw",                             LogParamTypes.TYPE_INT8 ),                  /*  65 */
		new LogParam( "CriAtomExResumeMode",            LogParamTypes.TYPE_INT8 ),                  /*  66 */
		new LogParam( "error_string",                   LogParamTypes.TYPE_CHAR ),                  /*  67 */
		new LogParam( "CriAtomSoundPlaybackId",         LogParamTypes.TYPE_INT32 ),                 /*  68 */
		new LogParam( "CriAtomSoundPlayerHn",           LogParamTypes.TYPE_UINTPTR ),               /*  69 */
		new LogParam( "CriAtomAwbHn",                   LogParamTypes.TYPE_UINTPTR ),               /*  70 */
		new LogParam( "CriAtomExCueId",                 LogParamTypes.TYPE_INT32 ),                 /*  71 */
		new LogParam( "cue_name",                       LogParamTypes.TYPE_CHAR ),                  /*  72 */
		new LogParam( "CriAtomExCueIndex",              LogParamTypes.TYPE_INT32 ),                 /*  73 */
		new LogParam( "buffer",                         LogParamTypes.TYPE_UINTPTR ),               /*  74 */
		new LogParam( "size",                           LogParamTypes.TYPE_INT32 ),                 /*  75 */
		new LogParam( "CriAtomExWaveId",                LogParamTypes.TYPE_INT32 ),                 /*  76 */
		new LogParam( "CriAtomAwbHn for Memory",        LogParamTypes.TYPE_UINTPTR ),               /*  77 */
		new LogParam( "CriAtomAwbHn for Stream",        LogParamTypes.TYPE_UINTPTR ),               /*  78 */
		new LogParam( "CriAtomExTweenHn",               LogParamTypes.TYPE_UINTPTR ),               /*  79 */
		new LogParam( "CriAtomExConfig",                LogParamTypes.TYPE_UINTPTR ),               /*  80 */
		new LogParam( "CriAtomExAsrConfig",             LogParamTypes.TYPE_UINTPTR ),               /*  81 */
		new LogParam( "CriAtomExHcaMxConfig",           LogParamTypes.TYPE_UINTPTR ),               /*  82 */
		new LogParam( "CriAtomDbasConfig",              LogParamTypes.TYPE_UINTPTR ),               /*  83 */
		new LogParam( "CriAtomStreamingCacheConfig",    LogParamTypes.TYPE_UINTPTR ),               /*  84 */
		new LogParam( "CriAtomExStandardVoicePoolConfig",LogParamTypes.TYPE_UINTPTR ),              /*  85 */
		new LogParam( "CriAtomExAdxVoicePoolConfig",    LogParamTypes.TYPE_UINTPTR ),               /*  86 */
		new LogParam( "CriAtomExAhxVoicePoolConfig",    LogParamTypes.TYPE_UINTPTR ),               /*  87 */
		new LogParam( "CriAtomExHcaVoicePoolConfig",    LogParamTypes.TYPE_UINTPTR ),               /*  88 */
		new LogParam( "CriAtomExHcaMxVoicePoolConfig",  LogParamTypes.TYPE_UINTPTR ),               /*  89 */
		new LogParam( "CriAtomExWaveVoicePoolConfig",   LogParamTypes.TYPE_UINTPTR ),               /*  90 */
		new LogParam( "CriAtomExRawPcmVoicePoolConfig", LogParamTypes.TYPE_UINTPTR ),               /*  91 */
		new LogParam( "CriAtomExPlayerConfig",          LogParamTypes.TYPE_UINTPTR ),               /*  92 */
		new LogParam( "CriAtomExTweenConfig",           LogParamTypes.TYPE_UINTPTR ),               /*  93 */
		new LogParam( "CriAtomDecrypterConfig",         LogParamTypes.TYPE_UINTPTR ),               /*  94 */
		new LogParam( "CriAtomEx3dSourceConfig",        LogParamTypes.TYPE_UINTPTR ),               /*  95 */
		new LogParam( "CriAtomEx3dListenerConfig",      LogParamTypes.TYPE_UINTPTR ),               /*  96 */
		new LogParam( "CriAtomExAdpcmVoicePoolConfig_3DS",LogParamTypes.TYPE_UINTPTR ),             /*  97 */
		new LogParam( "CriAtomExAdpcmVoicePoolConfig_WII",LogParamTypes.TYPE_UINTPTR ),             /*  98 */
		new LogParam( "CriAtomExVagVoicePoolConfig_PSP",LogParamTypes.TYPE_UINTPTR ),               /*  99 */
		new LogParam( "CriAtomExAtrac3VoicePoolConfig_PSP",LogParamTypes.TYPE_UINTPTR ),            /* 100 */
		new LogParam( "CriAtomExVagVoicePoolConfig_VITA",LogParamTypes.TYPE_UINTPTR ),              /* 101 */
		new LogParam( "CriAtomExAt9VoicePoolConfig_VITA",LogParamTypes.TYPE_UINTPTR ),              /* 102 */
		new LogParam( "num_groups",                     LogParamTypes.TYPE_INT16 ),                 /* 103 */
		new LogParam( "voices_per_group",               LogParamTypes.TYPE_INT16 ),                 /* 104 */
		new LogParam( "num_category_groups",            LogParamTypes.TYPE_INT16 ),                 /* 105 */
		new LogParam( "num_categories",                 LogParamTypes.TYPE_INT16 ),                 /* 106 */
		new LogParam( "reserved",                       LogParamTypes.TYPE_INT32 ),                 /* 107 */
		new LogParam( "CriAtomExFaderHn",               LogParamTypes.TYPE_UINTPTR ),               /* 108 */
		new LogParam( "Guid",                           LogParamTypes.TYPE_GUID ),                  /* 109 */
		new LogParam( "parent CriAtomExPlaybackId",     LogParamTypes.TYPE_INT32 ),                 /* 110 */
		new LogParam( "CriAtomPlayerPoolPlayerInfo",    LogParamTypes.TYPE_UINTPTR ),               /* 111 */
		new LogParam( "CriAtomSoundElementHn",          LogParamTypes.TYPE_UINTPTR ),               /* 112 */
		new LogParam( "CriAtomSoundVoiceHn",            LogParamTypes.TYPE_UINTPTR ),               /* 113 */
		new LogParam( "cause CriAtomExPlaybackId",      LogParamTypes.TYPE_INT32 ),                 /* 114 */
		new LogParam( "Index",                          LogParamTypes.TYPE_INT16 ),                 /* 115 */
		new LogParam( "NumAllPlaybacks",                LogParamTypes.TYPE_INT16 ),                 /* 116 */
		new LogParam( "NumPlaybacks",                   LogParamTypes.TYPE_INT16 ),                 /* 117 */
		new LogParam( "categories_per_playback",        LogParamTypes.TYPE_INT8 ),                  /* 118 */
		new LogParam( "enable_voice_priority_decay",    LogParamTypes.TYPE_INT8 ),                  /* 119 */
		new LogParam( "volume",                         LogParamTypes.TYPE_FLOAT32 ),               /* 120 */
		new LogParam( "CriAtomSoundElementId",          LogParamTypes.TYPE_INT32 ),                 /* 121 */
		new LogParam( "CriAtomSoundVoiceId",            LogParamTypes.TYPE_INT32 ),                 /* 122 */
		new LogParam( "Acb Name",                       LogParamTypes.TYPE_CHAR ),                  /* 123 */
		new LogParam( "CriAtomPlayerPoolPlayerInfoId",  LogParamTypes.TYPE_INT32 ),                 /* 124 */
		new LogParam( "Aisac Control",                  LogParamTypes.TYPE_FLOAT32 ),               /* 125 */
		new LogParam( "Track No",                       LogParamTypes.TYPE_INT16 ),                 /* 126 */
		new LogParam( "Mute",                           LogParamTypes.TYPE_INT8 ),                  /* 127 */
		new LogParam( "Result",                         LogParamTypes.TYPE_INT8 ),                  /* 128 */
		new LogParam( "Log Record Mode",                LogParamTypes.TYPE_INT32 ),                 /* 129 */
		new LogParam( "NumCh",                          LogParamTypes.TYPE_INT8 ),                  /* 130 */
		new LogParam( "NumLoaders",                     LogParamTypes.TYPE_INT32 ),                 /* 131 */
		new LogParam( "NumPlayers",                     LogParamTypes.TYPE_INT32 ),                 /* 132 */
		new LogParam( "Status",                         LogParamTypes.TYPE_INT8 ),                  /* 133 */
		new LogParam( "PlayingTime",                    LogParamTypes.TYPE_INT32 ),                 /* 134 */
		new LogParam( "DspBusSpectra",                  LogParamTypes.TYPE_128 ),                   /* 135 */
		new LogParam( "CpuLoad",                        LogParamTypes.TYPE_FLOAT32 ),               /* 136 */
		new LogParam( "NumUsedVoices",                  LogParamTypes.TYPE_INT32 ),                 /* 137 */
		new LogParam( "SequencePlaybackPosition",       LogParamTypes.TYPE_INT64 ),                 /* 138 */
		new LogParam( "CallbackValue",                  LogParamTypes.TYPE_INT32 ),                 /* 139 */
		new LogParam( "CallbackString",                 LogParamTypes.TYPE_CHAR ),                  /* 140 */
		new LogParam( "PeakLevel",                      LogParamTypes.TYPE_FLOAT32 ),               /* 141 */
		new LogParam( "RmsLevel",                       LogParamTypes.TYPE_FLOAT32 ),               /* 142 */
		new LogParam( "PeakHoldLevel",                  LogParamTypes.TYPE_FLOAT32 ),               /* 143 */
		new LogParam( "RequestId",                      LogParamTypes.TYPE_INT32 ),                 /* 144 */
		new LogParam( "TargetId",                       LogParamTypes.TYPE_INT32 ),                 /* 145 */
		new LogParam( "Md5",                            LogParamTypes.TYPE_GUID ),                  /* 146 */
		new LogParam( "GameVariable",                   LogParamTypes.TYPE_FLOAT32 ),               /* 147 */
		new LogParam( "GameVariableName",               LogParamTypes.TYPE_CHAR ),                  /* 148 */
		new LogParam( "TimeMs",                         LogParamTypes.TYPE_INT32 ),                 /* 149 */
		new LogParam( "SnapShotName",                   LogParamTypes.TYPE_CHAR ),                  /* 150 */
		new LogParam( "AisacControlId",                 LogParamTypes.TYPE_INT32 ),                 /* 151 */
		new LogParam( "StartTimeMs",                    LogParamTypes.TYPE_INT64 ),                 /* 152 */
		new LogParam( "SelectorName",                   LogParamTypes.TYPE_CHAR ),                  /* 153 */
		new LogParam( "LabelName",                      LogParamTypes.TYPE_CHAR ),                  /* 154 */
		new LogParam( "BlockName",                      LogParamTypes.TYPE_CHAR ),                  /* 155 */
		new LogParam( "CategoryName",                   LogParamTypes.TYPE_CHAR ),                  /* 156 */
		new LogParam( "AisacControlName",               LogParamTypes.TYPE_CHAR ),                  /* 157 */
		new LogParam( "SettingName",                    LogParamTypes.TYPE_CHAR ),                  /* 158 */
		new LogParam( "CueSheetId",                     LogParamTypes.TYPE_INT32 ),                 /* 159 */
		new LogParam( "BusNo",                          LogParamTypes.TYPE_INT8 ),                  /* 160 */
		new LogParam( "FxType",                         LogParamTypes.TYPE_INT32 ),                 /* 161 */
		new LogParam( "RemainedLoopCount",              LogParamTypes.TYPE_INT32 ),                 /* 162 */
		new LogParam( "SequenceLoopId",                 LogParamTypes.TYPE_INT16 ),                 /* 163 */
		new LogParam( "3dPosVector_Position",           LogParamTypes.TYPE_VECTOR ),                /* 164 */
		new LogParam( "3dPosVector_Velocity",           LogParamTypes.TYPE_VECTOR ),                /* 165 */
		new LogParam( "3dPosVector_Forward",            LogParamTypes.TYPE_VECTOR ),                /* 166 */
		new LogParam( "3dPosVector_Upward",             LogParamTypes.TYPE_VECTOR ),                /* 167 */
		new LogParam( "3dPosVector_FocusPoint",         LogParamTypes.TYPE_VECTOR ),                /* 168 */
		new LogParam( "3dPosVector_Cone",               LogParamTypes.TYPE_VECTOR ),                /* 169 */
		new LogParam( "3dMaxAngleAisacDelta",           LogParamTypes.TYPE_FLOAT32 ),               /* 170 */
		new LogParam( "3dEnablePriorityDecay",          LogParamTypes.TYPE_FLOAT32 ),               /* 171 */
		new LogParam( "3dDistanceFactor",               LogParamTypes.TYPE_FLOAT32 ),               /* 172 */
		new LogParam( "3dDistanceFocusLevel",           LogParamTypes.TYPE_FLOAT32 ),               /* 173 */
		new LogParam( "3dDirectionFocusLevel",          LogParamTypes.TYPE_FLOAT32 ),               /* 174 */
		new LogParam( "Result3dPos",                    LogParamTypes.TYPE_INT8 ),                  /* 175 */
		new LogParam( "CriAtomExAiffVoicePoolConfig",   LogParamTypes.TYPE_UINTPTR ),               /* 176 */
		new LogParam( "SoundRendererTyoe",              LogParamTypes.TYPE_INT8 ),                  /* 177 */
		new LogParam( "CriAtomExAt9VoicePoolConfig_PS4",LogParamTypes.TYPE_UINTPTR ),               /* 178 */
		new LogParam( "AverageServerTime",              LogParamTypes.TYPE_INT32 ),                 /* 179 */
		new LogParam( "AverageServerInterval",          LogParamTypes.TYPE_INT32 ),                 /* 180 */
		new LogParam( "MaxServerTime",                  LogParamTypes.TYPE_INT32 ),                 /* 181 */
		new LogParam( "MaxServerInterval",              LogParamTypes.TYPE_INT32 ),                 /* 182 */
		new LogParam( "UserLog",                        LogParamTypes.TYPE_CHAR ),                  /* 183 */
		new LogParam( "ByVoiceGroupLimitation",         LogParamTypes.TYPE_INT8 ),                  /* 184 */
		new LogParam( "ByVoicePoolLimitation",          LogParamTypes.TYPE_INT8 ),                  /* 185 */
		new LogParam( "RetryFlag",                      LogParamTypes.TYPE_INT8 ),                  /* 186 */
		new LogParam( "BusName",                        LogParamTypes.TYPE_CHAR ),                  /* 187 */
		new LogParam( "StreamType",                     LogParamTypes.TYPE_INT8 ),                  /* 188 */
		new LogParam( "MomentaryValue",                 LogParamTypes.TYPE_FLOAT32 ),               /* 189 */
		new LogParam( "ShortTermValue",                 LogParamTypes.TYPE_FLOAT32 ),               /* 190 */
		new LogParam( "IntegratedValue",                LogParamTypes.TYPE_FLOAT32 ),               /* 191 */
		new LogParam( "TotalBps",                       LogParamTypes.TYPE_FLOAT32 ),               /* 192 */
		new LogParam( "num_cues",                       LogParamTypes.TYPE_INT32 ),                 /* 193 */
		new LogParam( "SoundFormat",                    LogParamTypes.TYPE_INT32 ),                 /* 194 */
		new LogParam( "CriAtomExAdpcmVoicePoolConfig_WIIU",LogParamTypes.TYPE_UINTPTR ),            /* 195 */
		new LogParam( "AwbName",                        LogParamTypes.TYPE_CHAR ),                  /* 196 */
		new LogParam( "NumStreamAwb",                   LogParamTypes.TYPE_INT32 ),                 /* 197 */
		new LogParam( "ExPlayback_AllocateModule",      LogParamTypes.TYPE_INT8 ),                  /* 198 */
		new LogParam( "AisacControlValue",              LogParamTypes.TYPE_FLOAT32 ),               /* 199 */
		new LogParam( "NumAllPlaybacksForReact",        LogParamTypes.TYPE_INT16 ),                 /* 200 */
		new LogParam( "PreviewContext",                 LogParamTypes.TYPE_INT32 ),                 /* 201 */
		new LogParam( "max_parameter_blocks",           LogParamTypes.TYPE_INT16 ),                 /* 202 */
		new LogParam( "max_faders",                     LogParamTypes.TYPE_INT16 ),                 /* 203 */
		new LogParam( "num_buses",                      LogParamTypes.TYPE_INT16 ),                 /* 204 */
		new LogParam( "max_racks",                      LogParamTypes.TYPE_INT16 ),                 /* 205 */
		new LogParam( "output_channels_4_hcamx",        LogParamTypes.TYPE_INT8 ),                  /* 206 */
		new LogParam( "output_sampling_rate_4_hcamx",   LogParamTypes.TYPE_INT32 ),                 /* 207 */
		new LogParam( "sound_renderer_type_4_hcamx",    LogParamTypes.TYPE_INT8 ),                  /* 208 */
		new LogParam( "speaker_system",                 LogParamTypes.TYPE_INT8 ),                  /* 209 */
		new LogParam( "left_speaker_angle",             LogParamTypes.TYPE_FLOAT32 ),               /* 210 */
		new LogParam( "right_speaker_angle",            LogParamTypes.TYPE_FLOAT32 ),               /* 211 */
		new LogParam( "center_speaker_angle",           LogParamTypes.TYPE_FLOAT32 ),               /* 212 */
		new LogParam( "lfe_speaker_angle",              LogParamTypes.TYPE_FLOAT32 ),               /* 213 */
		new LogParam( "surround_left_speaker_angle",    LogParamTypes.TYPE_FLOAT32 ),               /* 214 */
		new LogParam( "surround_right_speaker_angle",   LogParamTypes.TYPE_FLOAT32 ),               /* 215 */
		new LogParam( "surround_back_left_speaker_angle",LogParamTypes.TYPE_FLOAT32 ),              /* 216 */
		new LogParam( "surround_back_right_speaker_angle",LogParamTypes.TYPE_FLOAT32 ),             /* 217 */
		new LogParam( "pan_speaker_type",               LogParamTypes.TYPE_INT8 ),                  /* 218 */
		new LogParam( "VoiceStopReason",                LogParamTypes.TYPE_INT16 ),                 /* 219 */
		new LogParam( "CriAtomExVibrationVoicePoolConfig",LogParamTypes.TYPE_UINTPTR ),             /* 220 */
		new LogParam( "TouceSenceEffectName",           LogParamTypes.TYPE_CHAR ),                  /* 221 */
		new LogParam( "dsp_name",                       LogParamTypes.TYPE_CHAR ),                  /* 222 */
		new LogParam( "dsp_object",                     LogParamTypes.TYPE_UINTPTR ),               /* 223 */
		new LogParam( "dsp_slot_no",                    LogParamTypes.TYPE_INT32 ),                 /* 224 */
		new LogParam( "dsp_plugin_type",                LogParamTypes.TYPE_INT32 ),                 /* 225 */
		new LogParam( "3dPosVector_ListenerTop",        LogParamTypes.TYPE_VECTOR ),                /* 226 */
		new LogParam( "playback_status",                LogParamTypes.TYPE_INT32 ),                 /* 227 */
		new LogParam( "instrument_instance_callback",   LogParamTypes.TYPE_UINTPTR ),               /* 228 */
		new LogParam( "instrument_instance_callback_obj", LogParamTypes.TYPE_UINTPTR ),             /* 229 */
		new LogParam( "instrument_instance_attach_player",LogParamTypes.TYPE_UINTPTR ),             /* 230 */
		new LogParam( "instrument_instance",            LogParamTypes.TYPE_UINTPTR ),               /* 231 */
		new LogParam( "CriAtomEx3dTransceiverHn",       LogParamTypes.TYPE_UINTPTR ),			    /* 232 */
		new LogParam( "CriAtomEx3dTransceiverConfig",   LogParamTypes.TYPE_UINTPTR ),			    /* 233 */
		new LogParam( "CriAtomEx3dRegionHn",            LogParamTypes.TYPE_UINTPTR ),				/* 234 */
		new LogParam( "CriAtomEx3dTransceiverDirectAudioRadius", LogParamTypes.TYPE_FLOAT32 ),	    /* 235 */
		new LogParam( "CriAtomEx3dTransceiverCrossFadeDistance", LogParamTypes.TYPE_FLOAT32 ),	    /* 236 */
		new LogParam( "program_no",                     LogParamTypes.TYPE_INT32 ),                 /* 237 */
		new LogParam( "key_no",                         LogParamTypes.TYPE_INT32 ),                 /* 238 */
		new LogParam( "Velocity",                       LogParamTypes.TYPE_INT16 ),					/* 239 */
		new LogParam( "PitchBend",                      LogParamTypes.TYPE_INT16 ),					/* 240 */

		new LogParam("Format",                          LogParamTypes.TYPE_INT32 ),					/* 241 */	/* CriAtomFormat */
		new LogParam("MaxRhythmTracks",                 LogParamTypes.TYPE_INT16 ),					/* 242 */
		new LogParam("MaxMelodyTracks",                 LogParamTypes.TYPE_INT16 ),					/* 243 */
		new LogParam("MaxVocalTracks",                  LogParamTypes.TYPE_INT16 ),					/* 244 */
		new LogParam("AdmPlayerHn",                     LogParamTypes.TYPE_UINTPTR ),				/* 245 */
	};

	protected const uint LOG_MODE_OFF = 0;
	protected const uint LOG_MODE_PLAYBACK = 1;
	protected const uint LOG_MODE_ERROR = 1 << 1;
	protected const uint LOG_MODE_LOW_LEVEL_PLAYBACK = 1 << 2;
	protected const uint LOG_MODE_SYSTEM_INFO = 1 << 3;
	protected const uint LOG_MODE_HANDLE_INFO = 1 << 4;
	protected const uint LOG_MODE_CUE_LIMIT = 1 << 5;
	protected const uint LOG_MODE_PROBABILITY = 1 << 6;
	protected const uint LOG_MODE_CATEGORY = 1 << 7;
	protected const uint LOG_MODE_EXECUTING_INFORMATION = 1 << 8;
	protected const uint LOG_MODE_3D_INFO = 1 << 9;
	protected const uint LOG_MODE_USER_LOG = 1 << 10;
	protected const uint LOG_MODE_VOICE_VOLUME = 1 << 11;
	protected const uint LOG_MODE_ALL = 0xFFFFFFFF;

	protected enum LogTypes {
		NON = 0,
		PLAYBACK,
		ERROR,
		LOW_LEVEL_PLAYBACK,
		SYSTEM_INFORMATION,
		HANDLE_INFORMATION,
		CUE_LIMIT,
		PROBABILITY,
		CATEGORY,
		EXECUTING_INFORMATION,
		INFORMATION_3D,
		USER_LOG,
		VOICE_VOLUME
	};

	protected const uint FORMAT_NONE = 0;
	protected const uint FORMAT_ADX = 1 << 0;
	protected const uint FORMAT_HCA = 1 << 2;
	protected const uint FORMAT_HCA_MX = 1 << 3;
	protected const uint FORMAT_PCM = 1 << 4;
	protected const uint FORMAT_WAVE = 1 << 5;
	protected const uint FORMAT_RAW_PCM = 1 << 6;
	protected const uint FORMAT_AIFF = 1 << 7;
	protected const uint FORMAT_VIBRATION = 1 << 8;
	protected const uint FORMAT_AUDIO_BUFFER = 1 << 9;
	protected const uint FORMAT_SOUND_GENERATOR = 1 << 10;
	protected const uint FORMAT_RAW_PCM_FLOAT = 1 << 11;
	protected const uint FORMAT_HW1 = 1 << 16;
	protected const uint FORMAT_HW2 = 1 << 17;
	/* aliases */
	protected const uint FORMAT_XMA = FORMAT_HW1;
	protected const uint FORMAT_NDS_ADPCM = FORMAT_HW1;
	protected const uint FORMAT_WII_ADPCM = FORMAT_HW1;
	protected const uint FORMAT_WIIU_ADPCM = FORMAT_HW1;
	protected const uint FORMAT_3DS_ADPCM = FORMAT_HW1;
	protected const uint FORMAT_VAG = FORMAT_HW1;
	protected const uint FORMAT_ATRAC3 = FORMAT_HW2;
	protected const uint FORMAT_PSP2_HEVAG = FORMAT_HW1;
	protected const uint FORMAT_ATRAC9 = FORMAT_HW2;

	protected enum StreamTypes {
		UNKNOWN = 0,
		ONMEMORY,
		STREAM,
		ZERO_LATENCY_STREAM,
	}
}

} //namespace CriWare

#endif