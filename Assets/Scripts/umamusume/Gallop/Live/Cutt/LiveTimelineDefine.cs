using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
	public class LiveTimelineDefine : MonoBehaviour
	{
		public enum FacialId
		{
			Base = 1001,
			WinkR = 1002,
			WinkL = 1003,
			EyeHalfA = 1004,
			EyeCloseA = 1005,
			WaraiA = 1101,
			WaraiB = 1102,
			WaraiC = 1103,
			WaraiD = 1104,
			IkariA = 1201,
			KanasiA = 1301,
			DoyaA = 1401,
			OdorokiA = 1601,
			OdorokiB = 1602,
			OdorokiC = 1603,
			JitomeA = 1701,
			KomariA = 1801,
			DereA = 1901,
			KusyoAL = 2001,
			KusyoAR = 2002,
			KusyoBL = 2003,
			KusyoBR = 2004,
			EyeWarai = 2101,
			EyeHohoemi = 2201,
			UreiA = 2301,
			IkariB = 1202,
			IkariC = 1203,
			IkariD = 1204,
			KanasiB = 1302,
			KanasiC = 1303,
			KanasiD = 1304,
			DoyaB = 1402,
			FutuA = 1501,
			FutuB = 1502,
			OdorokiD = 1604,
			JitomeB = 1702,
			KomariB = 1802,
			KomariC = 1803,
			KomariD = 1804,
			DereB = 1902,
			MouthClose = 8001,
			MouthAS = 8011,
			MouthAM = 8012,
			MouthAL = 8013,
			MouthIS = 8021,
			MouthIM = 8022,
			MouthIL = 8023,
			MouthUS = 8031,
			MouthUM = 8032,
			MouthUL = 8033,
			MouthES = 8041,
			MouthEM = 8042,
			MouthEL = 8043,
			MouthOS = 8051,
			MouthOM = 8052,
			MouthOL = 8053,
			MouthKanasiClose = 8101,
			MouthKanasiAS = 8111,
			MouthKanasiAM = 8112,
			MouthKanasiAL = 8113,
			MouthKanasiIS = 8121,
			MouthKanasiIM = 8122,
			MouthKanasiIL = 8123,
			MouthKanasiUS = 8131,
			MouthKanasiUM = 8132,
			MouthKanasiUL = 8133,
			MouthKanasiES = 8141,
			MouthKanasiEM = 8142,
			MouthKanasiEL = 8143,
			MouthKanasiOS = 8151,
			MouthKanasiOM = 8152,
			MouthKanasiOL = 8153,
			MouthWaraiClose = 8201,
			MouthWaraiAS = 8211,
			MouthWaraiAM = 8212,
			MouthWaraiAL = 8213,
			MouthWaraiIS = 8221,
			MouthWaraiIM = 8222,
			MouthWaraiIL = 8223,
			MouthWaraiUS = 8231,
			MouthWaraiUM = 8232,
			MouthWaraiUL = 8233,
			MouthWaraiES = 8241,
			MouthWaraiEM = 8242,
			MouthWaraiEL = 8243,
			MouthWaraiOS = 8251,
			MouthWaraiOM = 8252,
			MouthWaraiOL = 8253,
			MouthIkariClose = 8301,
			MouthIkariAS = 8311,
			MouthIkariAM = 8312,
			MouthIkariAL = 8313,
			MouthIkariIS = 8321,
			MouthIkariIM = 8322,
			MouthIkariIL = 8323,
			MouthIkariUS = 8331,
			MouthIkariUM = 8332,
			MouthIkariUL = 8333,
			MouthIkariES = 8341,
			MouthIkariEM = 8342,
			MouthIkariEL = 8343,
			MouthIkariOS = 8351,
			MouthIkariOM = 8352,
			MouthIkariOL = 8353,
			UniqueA = 9001,
			UniqueB = 9002,
			UniqueC = 9003,
			UniqueD = 9004,
			UniqueE = 9005,
			UniqueF = 9006,
			UniqueG = 9007,
			UniqueH = 9008,
			UniqueI = 9009,
			UniqueJ = 9010,
			UniqueK = 9011
		}

		public class FacialPartsSet
		{
			public EarType Ear;
			public int EyebrowL;
			public int EyebrowR;
			public int EyeL;
			public int EyeR;
			public int Mouth;
			public float MouthWeight;
		}

		public enum FacialEarId
		{
			Base = 0,
			Base_N = 1,
			Kanasi = 2,
			Dere_N = 3,
			Dere = 4,
			Yure = 5,
			Biku_N = 6,
			Biku = 7,
			Ikari = 8,
			Tanosi = 9,
			Up_N = 10,
			Up = 11,
			Down = 12,
			Front = 13,
			Side = 14,
			Back = 15,
			Roll = 16
		}

		public enum FacialEyebrowId
		{
			Base = 0,
			WaraiA = 1,
			WaraiB = 2,
			WaraiC = 3,
			WaraiD = 4,
			IkariA = 5,
			KanasiA = 6,
			DoyaA = 7,
			DereA = 8,
			OdorokiA = 9,
			OdorokiB = 10,
			JitoA = 11,
			KomariA = 12,
			KusyoA = 13,
			UreiA = 14,
			RunA = 15,
			RunB = 16,
			SeriousA = 17,
			SeriousB = 18,
			ShiwaA = 19,
			ShiwaB = 20,
			Offset_U = 21,
			Offset_D = 22,
			Offset_L = 23,
			Offset_R = 24
		}

		public enum FacialEyeId
		{
			Base = 0,
			HalfA = 1,
			CloseA = 2,
			HalfB = 3,
			HalfC = 4,
			WaraiA = 5,
			WaraiB = 6,
			WaraiC = 7,
			WaraiD = 8,
			IkariA = 9,
			KanasiA = 10,
			DereA = 11,
			OdorokiA = 12,
			OdorokiB = 13,
			OdorokiC = 14,
			JitoA = 15,
			KusyoA = 16,
			UreiA = 17,
			RunA = 18,
			DrivenA = 19,
			XRange = 20,
			YRange = 21,
			EyeHideA = 22,
			SeriousA = 23,
			PupilA = 24,
			PupilB = 25,
			PupilC = 26,
			EyelidHideA = 27,
			EyelidHideB = 28
		}

		public enum FacialMouthId
		{
			Base = 0,
			Normal = 1,
			CheekA_L = 2,
			CheekA_R = 3,
			WaraiA = 4,
			WaraiB = 5,
			WaraiC = 6,
			WaraiD = 7,
			WaraiE = 8,
			IkariA = 9,
			IkariB = 10,
			KanasiA = 11,
			DoyaA = 12,
			DereA = 13,
			OdorokiA = 14,
			OdorokiB = 15,
			JitoA = 16,
			KomariA = 17,
			KusyoA_L = 18,
			KusyoA_R = 19,
			KusyoB_L = 20,
			KusyoB_R = 21,
			UreiA = 22,
			TalkA_A_S = 23,
			TalkA_A_L = 24,
			TalkA_I_S = 25,
			TalkA_I_L = 26,
			TalkA_U_S = 27,
			TalkA_U_L = 28,
			TalkA_E_S = 29,
			TalkA_E_L = 30,
			TalkA_O_S = 31,
			TalkA_O_L = 32,
			TalkB_A_S = 33,
			TalkB_A_L = 34,
			TalkB_I_S = 35,
			TalkB_I_L = 36,
			TalkB_E_S = 37,
			TalkB_E_L = 38,
			RunA = 39,
			RunB = 40,
			DrivenA = 41,
			ToothHide = 42,
			TalkC_I = 43,
			TanA = 44,
			TanB = 45,
			TanC_L = 46,
			TanD_L = 47,
			TanC_R = 48,
			TanD_R = 49,
			Offset_U = 50,
			Offset_D = 51,
			Offset_L = 52,
			Offset_R = 53,
			Scale_U = 54,
			Scale_D = 55,
			LowAngle = 56
		}

		public enum FacialSpeed
		{
			Direct = 0,
			Normal = 1,
			Fast1 = 10,
			Slow1 = 20,
			Slow2 = 21
		}

		public enum VowelType
		{
			N = 0,
			A = 1,
			I = 2,
			U = 3,
			E = 4,
			O = 5,
			Max = 6
		}

		public enum CheekType
		{
			None = 0,
			Level1 = 1,
			Level2 = 2,
			Max = 3
		}

		public enum TearyType
		{
			NoAnim = 0,
			Teary = 1,
			Twinkle = 2,
			Max = 3
		}

		public enum TearfulType
		{
			Off = 0,
			On = 1,
			Max = 2
		}

		public enum TeardropIndex
		{
			Off = 0,
			Teardrop = 1,
			Watery = 2,
			Max = 3
		}

		public enum MangameIndex
		{
			Off = 0,
			GuruGuru = 1,
			Max = 2
		}

		public enum FacialToonLightType
		{
			Off = 0,
			Original = 1,
			FaceTrace = 2
		}

		public enum SheetIndex
		{
			PreLiveSkit = 0,
			MainLive = 1,
			AfterLiveSkit = 2,
			Max = 3
		}

		public enum FacialEyeTrackTargetType
		{
			Arena = 0,
			Camera = 1,
			CharaForward = 2,
			Chara1 = 3,
			Chara2 = 4,
			Chara3 = 5,
			Chara4 = 6,
			Chara5 = 7,
			Chara6 = 8,
			Chara7 = 9,
			Chara8 = 10,
			Chara9 = 11,
			Chara10 = 12,
			Chara11 = 13,
			Chara12 = 14,
			Chara13 = 15,
			Chara14 = 16,
			Chara15 = 17,
			Chara16 = 18,
			Chara17 = 19,
			Chara18 = 20,
			MultiCamera1 = 30,
			MultiCamera2 = 31,
			MultiCamera3 = 32,
			MultiCamera4 = 33,
			DirectPosition = 40
		}

		public enum FacialEyeTrackVerticalType
		{
			None = 0,
			Up = 1,
			Down = 2,
			Direct = 3
		}

		public enum FacialEyeTrackHorizontalType
		{
			None = 0,
			Left = 1,
			Right = 2,
			Direct = 3
		}

		public enum CornerMouthType
		{
			None = 0,
			Up = 1,
			Down = 2
		}

        public enum OutlineColorBlend
        {
			Blend = 0,
			Multiply = 1,
			Max = 2
		}

        public enum LightBlendMode
        {
			Addition = 0,
			Multiply = 1,
            SoftAddition = 2,
            AlphaBlend = 3,
            Multiply0 = 4,
            Multiply2x = 5,
        }

        public enum ColorType
        {
			Main = 0,
			Sub = 1,
            Training1 = 2,
            Training2 = 3,
            Border = 4,
            Num1 = 5,
            Num2 = 6,
            Turn = 7,
            Wipe1 = 8,
            Wipe2 = 9,
            Wipe3 = 10,
            Speech1 = 11,
            Nameplate1 = 12,
            Nameplate2 = 13,
            Speech2 = 14
        }

		public enum LiveTimelineKeyDataType
        {
            Timescale = 0,
            CameraPos = 1,
            CameraLookAt = 2,
            CameraFov = 3,
            CameraRoll = 4,
            HandShakeCamera = 5,
            Event = 6,
            CharaMotionSequence = 7,
            BgColor1 = 8,
            BgColor2 = 9,
            MonitorControl = 10,
            CameraSwitcher = 11,
            LipSync = 12,
            PostEffectDOF = 13,
            PostEffectBloomDiffusion = 14,
            RadialBlur = 15,
            CameraLayer = 16,
            Projector = 17,
            FacialFace = 18,
            FacialMouth = 19,
            FacialCheek = 20,
            FacialEye = 21,
            FacialEyebrow = 22,
            FacialEyeTrack = 23,
            FacialEar = 24,
            FacialEffect = 25,
            FacialNoise = 26,
            CharaMotionNoise = 27,
            FormationOffset = 28,
            Animation = 29,
            TextureAnimation = 30,
            Transform = 31,
            Renderer = 32,
            Object = 33,
            Audience = 34,
            Props = 35,
            PropsAttach = 36,
            VolumeLight = 37,
            HdrBloom = 38,
            PostFilm = 39,
            Fade = 40,
            Particle = 41,
            ParticleGroup = 42,
            WashLight = 43,
            Laser = 44,
            BlinkLight = 45,
            UVScrollLight = 46,
            FacialToon = 47,
            GlobalLight = 48,
            GlobalFog = 49,
            LightShafts = 50,
            MonitorCameraPos = 51,
            MonitorCameraLookAt = 52,
            MultiCameraPos = 53,
            MultiCameraLookAt = 54,
            EyeCameraPos = 55,
            EyeCameraLookAt = 56,
            LensFlare = 57,
            Environment = 58,
            SweatLocator = 59,
            Effect = 60,
            ColorCorrection = 61,
            PreColorCorrection = 62,
            TiltShift = 63,
            A2U = 64,
            A2UConfig = 65,
            FlashPlayer = 66,
            Title = 67,
            Spotlight3d = 68,
            CharaNode = 69,
            NodeScale = 70,
            Fluctuation = 71,
            CharaFootLight = 72,
            ChromaticAberration = 73,
            LightProjection = 74,
            Billboard = 75,
            MultiCameraPostFilm = 76,
            MultiCameraPostEffectBloomDiffusion = 77,
            MultiCameraColorCorrection = 78,
            MultiCameraTiltShift = 79,
            MultiCameraRadialBlur = 80,
            MultiCameraPostEffectDOF = 81,
            AdditionalLight = 82,
            MultiLightShadow = 83,
            MobControl = 84,
            CyalumeControl = 85,
            CameraMotion = 86,
            WaveObject = 87,
            CharaWind = 88,
            CharaParts = 89,
            CameraCutNo = 90,
            ToneCurve = 91,
            Exposure = 92,
            Vortex = 93,
            CharaCollision = 94,
            TransmittedLight = 95,
            TransmittedLightMask = 96,
            Voice = 97,
            LipSyncPatternRange = 98,
            LipSyncPattern = 99,
            LensDistortion = 100,
            CharaNodeOffset = 101,
            TransparentCamera = 102,
            Max = 103
        }
    }
}

