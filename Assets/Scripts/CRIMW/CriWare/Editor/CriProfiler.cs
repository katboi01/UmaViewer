/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/
#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using UnityEngine;

namespace CriWare {

public partial class CriProfiler
{
	private static CriProfiler singleton = new CriProfiler();
	public static CriProfiler GetSingleton() { return singleton; }

	#region Structs and Classes
	// TCP connection parameters to be send to the thread
	protected struct TcpParams {
		public readonly IPAddress ipAddress;
		public readonly int port;
		public readonly int chunkSize;
		public readonly int connectionTimeoutMillisec;
		public readonly int connectionRetryIntervalMillisec;
		public readonly int connectionRetryLimit;
		public readonly int enBufferIntervalMillisec;
		public TcpParams(
			IPAddress ipAddress,
			int port,
			int chunkSize,
			int connectionTimeoutMillisec,
			int connectionRetryIntervalMillisec,
			int connectionRetryLimit,
			int enBufferIntervalMillisec)
		{
			this.ipAddress = ipAddress;
			this.port = port;
			this.chunkSize = chunkSize;
			this.connectionTimeoutMillisec = connectionTimeoutMillisec;
			this.connectionRetryIntervalMillisec = connectionRetryIntervalMillisec;
			this.connectionRetryLimit = connectionRetryLimit;
			this.enBufferIntervalMillisec = enBufferIntervalMillisec;
		}
	}

	// TCP data buffer
	private class RingBuffer<T> {
		protected T[] data;
		protected int size;
		protected int head;
		protected int tail;
		protected int mask;   /* for remainder operation */

		static private int RegulateToPow2(uint n) {
			int res = 0;
			for (n -= 1; n != 0; n >>= 1) {
				res = (res << 1) + 1;
			}
			return res + 1;
		}

		// Specify the size of the buffer.
		// <para>The resulted size will be rounded up to the nearest 2^n number for the sake of performance.</para>
		public RingBuffer(int _size) {
			size = RegulateToPow2((uint)_size);
			data = new T[size];
			head = 0;
			tail = 0;
			mask = size - 1;
		}

		// Get the number of elements in the buffer
		public int Count {
			get {
				int _count = this.tail - this.head;
				if (_count < 0) _count += this.size;
				return _count;
			}
		}

		public int Capacity {
			get {
				return size - 1;
			}
		}

		// Index access to the buffer.
		public T this[int i] {
			get {
				return this.data[(i + this.head) & this.mask];
			}
			private set {
				this.data[(i + this.head) & this.mask] = value;
			}
		}

		// Copy [inputSize] of elements from inputData to the tail of the buffer.
		// return false if the buffer is full.
		public bool EnBuffer(T[] inputData, int inputSize) {
			if (this.Count + inputSize >= this.size) {
				return false;
			}

			for (int i = 0; i < inputSize; ++i) {
				this.data[this.tail] = inputData[i];
				this.tail = (this.tail + 1) & this.mask;
			}

			return true;
		}

		// Read [outputSize] of elements and delete it from the head of the buffer.
		// <para>Return null if there is nothing to read.</para>
		// <para>The resulted array may be smaller in length than requested.</para>
		public T[] DeBuffer(int outputSize) {
			if (outputSize <= 0 || this.Count <= 0) {
				return null;
			}
			if (outputSize > this.Count) {
				return null;
			}

			T[] res = new T[outputSize];
			for (int i = 0; i < outputSize; ++i) {
				res[i] = this[i];
			}
			this.head = (this.head + outputSize) & this.mask;

			return res;
		}

		// Clear the buffer. Memory spaces will still be occupied.
		public void Clear() {
			this.head = 0;
			this.tail = 0;
		}
	}

	private class RingBufferAutoDequeue<T> : RingBuffer<T> {
		public RingBufferAutoDequeue(int size) : base(size) { }

		public new bool EnBuffer(T[] inputData, int inputSize) {
			if (this.Count + inputSize >= this.size) {
				this.DeBuffer(this.Count + inputSize - this.size + 1);
			}

			for (int i = 0; i < inputSize; ++i) {
				this.data[this.tail] = inputData[i];
				this.tail = (this.tail + 1) & this.mask;
			}

			return true;
		}

		public void FillWithZeros() {
			T[] zeros = new T[Capacity];
			this.EnBuffer(zeros, Capacity);
		}
	}

	#region Loading / Saving data from big-endian data stream
	public static Byte LoadBigEndianByte(Byte[] data, int ofst) {
		return data[ofst];
	}
	public static void SaveBigEndianByte(Byte[] data, int ofst, Byte input) {
		data[ofst] = input;
	}

	public static UInt16 LoadBigEndianUInt16(Byte[] data, int ofst) {
		return (UInt16)( ((data[ofst + 0]) << 8)
					   |  (data[ofst + 1]));
	}
	public static void SaveBigEndianUInt16(Byte[] data, int ofst, UInt16 input) {
		data[ofst + 0] = (byte)((input >> 8) & 0xff);
		data[ofst + 1] = (byte)( input       & 0xff);
	}

	public static UInt32 LoadBigEndianUInt32(Byte[] data, int ofst) {
		return (UInt32)( ((data[ofst + 0]) << 24)
					   | ((data[ofst + 1]) << 16)
					   | ((data[ofst + 2]) << 8)
					   |  (data[ofst + 3]));
	}
	public static void SaveBigEndianUInt32(Byte[] data, int ofst, UInt32 input) {
		data[ofst + 0] = (byte)((input >> 24) & 0xff);
		data[ofst + 1] = (byte)((input >> 16) & 0xff);
		data[ofst + 2] = (byte)((input >> 8)  & 0xff);
		data[ofst + 3] = (byte)( input        & 0xff);
	}

	public static Single LoadBigEndianSingle(Byte[] data, int ofst) {
		/* convert to little-endian for System.BitConverter */
		byte[] byteVals = new[] { data[ofst + 3], data[ofst + 2], data[ofst + 1], data[ofst + 0] };
		return BitConverter.ToSingle(byteVals, 0);
	}

	public static UInt64 LoadBigEndianUInt64(Byte[] data, int ofst) {
		return (UInt64)( ((data[ofst + 0]) << 56)
					   | ((data[ofst + 1]) << 48)
					   | ((data[ofst + 2]) << 40)
					   | ((data[ofst + 3]) << 32)
					   | ((data[ofst + 4]) << 24)
					   | ((data[ofst + 5]) << 16)
					   | ((data[ofst + 6]) << 8)
					   |  (data[ofst + 7]));
	}
	public static void SaveBigEndianUInt64(Byte[] data, int ofst, UInt64 input) {
		data[ofst + 0] = (byte)((input >> 56) & 0xff);
		data[ofst + 1] = (byte)((input >> 48) & 0xff);
		data[ofst + 2] = (byte)((input >> 40) & 0xff);
		data[ofst + 3] = (byte)((input >> 32) & 0xff);
		data[ofst + 4] = (byte)((input >> 24) & 0xff);
		data[ofst + 5] = (byte)((input >> 16) & 0xff);
		data[ofst + 6] = (byte)((input >> 8)  & 0xff);
		data[ofst + 7] = (byte)( input        & 0xff);
	}
	#endregion

	protected struct TcpLogHeader {
		public const int properSize
			  /* = sizeof(UInt32) + sizeof(UInt16) + sizeof(Byte) + sizeof(Byte) + sizeof(UInt64)
			   * + sizeof(UInt16) + sizeof(UInt16) + sizeof(UInt32) + sizeof(UInt64);
			   */ = 32;
		public readonly UInt32 packetSize;
		public readonly UInt16 command;
		public readonly Byte ptr_size;
		public readonly Byte log_type;
		public readonly UInt64 time;
		public readonly UInt16 function_id;
		public readonly UInt16 padding_size;
		public readonly UInt32 control_id;
		public readonly UInt64 thread_id;
		public readonly bool isPtr64;
		public readonly int size;

		public TcpLogHeader(byte[] data) {
			int offset = 0;
			isPtr64 = false;

			packetSize = LoadBigEndianUInt32(data, offset);
			offset += sizeof(UInt32);

			command = LoadBigEndianUInt16(data, offset);
			offset += sizeof(UInt16);

			ptr_size = LoadBigEndianByte(data, offset);
			isPtr64 = ((ptr_size - sizeof(UInt32)) == 0) ? false : true;
			offset += sizeof(Byte);

			log_type = LoadBigEndianByte(data, offset);
			offset += sizeof(Byte);

			time = LoadBigEndianUInt64(data, offset);
			offset += sizeof(UInt64);

			function_id = LoadBigEndianUInt16(data, offset);
			offset += sizeof(UInt16);

			padding_size = LoadBigEndianUInt16(data, offset);
			offset += sizeof(UInt16);

			control_id = LoadBigEndianUInt32(data, offset);
			offset += sizeof(UInt32);

			if(isPtr64 == true) {
				thread_id = LoadBigEndianUInt64(data, offset);
				offset += sizeof(UInt64);
			} else {
				offset += sizeof(UInt32);
				thread_id = LoadBigEndianUInt32(data, offset);
				offset += sizeof(UInt32);
			}

			size = offset;
            if (size != properSize) {
                UnityEngine.Debug.LogError("[CRIWARE] (Profiler) Corrupted log header");
            }
		}
	}

	private class ParamIdDoesNotMatchException : Exception {
		public ParamIdDoesNotMatchException() { }

		public ParamIdDoesNotMatchException(string message) : base(message) { }

		public ParamIdDoesNotMatchException(string message, Exception inner) : base(message, inner) { }
	}

	public class PlaybackInfo {
		public readonly UInt32 playbackId;
		public readonly String cueName;
		public readonly String cueSheetName;
		public readonly UInt64 timestamp;
		public int? StreamType;
		public PlaybackInfo(UInt32 playbackId, String cueName, String cueSheetName, UInt64 timestamp)
		{
			this.playbackId = playbackId;
			this.cueName = cueName;
			this.cueSheetName = cueSheetName;
			this.timestamp = timestamp;
			this.StreamType = null;
		}
	}

	public class VoicePoolInfo {
		public UInt32 soundFormat;
		public int numUsedVoices;
		public int? StreamType;
		public VoicePoolInfo(UInt32 soundFormat, int? StreamType = null)
		{
			this.soundFormat = soundFormat;
			this.StreamType = StreamType;
			this.numUsedVoices = -1;    /* <0: no data yet */
		}
	}

	public struct LevelInfo {
		public Single levelPeak;
		public Single levelRms;
		public Single levelPeakHold;
		public LevelInfo(Single levelPeak, Single levelRms, Single levelPeakHold)
		{
			this.levelPeak = levelPeak;
			this.levelRms = levelRms;
			this.levelPeakHold = levelPeakHold;
		}
	}
	#endregion

	#region Internal data
	private Thread threadTcpClient;
	private Thread threadPacketReading;
	private ManualResetEvent threadTerminator = new ManualResetEvent(false);
	private RingBuffer<byte> TcpBuffer = new RingBuffer<byte>(16384);
	private List<byte[]> eventLogList = new List<byte[]>();
	private bool isProfiling = false;
	private bool isConnected = false;
	private bool isBufferCorrupted = false;
	private bool doSaveLog;
	private FileStream logStream = null;
	private const int cRwLockTimeoutMs = 100;
	public bool IsProfiling {
		get { return isProfiling; }
	}
	#endregion

	#region Fields / Properties
	public UInt64 timestamp { get; private set; }
    public UInt64 timestampStart { get; private set; }
	public Single cpu_load { get; private set; }
	private RingBufferAutoDequeue<Single> cpuLoadHistory = new RingBufferAutoDequeue<float>(100);
	public Single[] CpuLoadHistory {
		get {
			lock (cpuLoadHistory) {
				Single[] histArray = new Single[cpuLoadHistory.Count];
				for (int i = 0; i < cpuLoadHistory.Count; ++i) {
					histArray[i] = cpuLoadHistory[i];
				}
				return histArray;
			}
		}
	}
	public UInt32 num_used_voices { get; private set; }
	private RingBufferAutoDequeue<UInt32> voiceUsageHistory = new RingBufferAutoDequeue<UInt32>(100);
	public UInt32[] VoiceUsageHistory {
		get {
			lock (voiceUsageHistory) {
				UInt32[] histArray = new UInt32[voiceUsageHistory.Count];
				for (int i = 0; i < voiceUsageHistory.Count; ++i) {
					histArray[i] = voiceUsageHistory[i];
				}
				return histArray;
			}
		}
	}
	public UInt32 num_used_streams { get; private set; }
	public Single total_bps { get; private set; }
	public UInt32 maxVoice_stdOnMemory { get; private set; }
	public UInt32 maxVoice_stdStreaming { get; private set; }
	public UInt32 maxVoice_hcamxOnMemory { get; private set; }
	public UInt32 maxVoice_hcamxStreaming { get; private set; }
	public Single loudness_momentary { get; private set; }
	public Single loudness_shortTerm { get; private set; }
	public Single loudness_integrated { get; private set; }
	public int numChMasterOut { get; private set; }
	private List<LevelInfo> levels = new List<LevelInfo>();     /* Channel => Levels */
	public LevelInfo[] LevelsAllCh {
		get {
			if(levels.Count > 0) {
				LevelInfo[] levelsCopy;
				lock (levels) {
					levelsCopy = new LevelInfo[levels.Count];
					levels.CopyTo(levelsCopy);
				}
				return levelsCopy;
			} else {
				return null;
			}
		}
	}
	public String cuename_lastPlayed { get; private set; }
    public String cuesheetName_lastPlayed { get; private set; }
	private Hashtable playbackHashtable = new Hashtable();      /* Playback ID => Playback Info */
	public PlaybackInfo[] PlaybackList {
		get {
			lock (playbackHashtable) {
				PlaybackInfo[] playbackList = new PlaybackInfo[playbackHashtable.Count];
				playbackHashtable.Values.CopyTo(playbackList, 0);
				return playbackList.OrderBy(x => x.timestamp).ToArray();
			}
		}
	}
	private Dictionary<string, CueSheetGroup> multiLaneTimeline = new Dictionary<string, CueSheetGroup>();
	private Hashtable voicePoolInfoTable = new Hashtable();     /* VoicePool Handle => VoicePool Info */
	public enum VoicePoolFormat {
		Standard,
		HcaMx
	}
	public int GetVoicePoolUsage(VoicePoolFormat format) {
		int usage = 0;
		bool haveData = false;
		UInt32 formatBits = FORMAT_NONE;
		switch (format) {
			case VoicePoolFormat.Standard:
				formatBits = FORMAT_ADX | FORMAT_HCA;
				break;
			case VoicePoolFormat.HcaMx:
				formatBits = FORMAT_HCA_MX;
				break;
			default:
				break;
		}
		lock (voicePoolInfoTable) {
			foreach(VoicePoolInfo elem in voicePoolInfoTable.Values) {
				if (elem.soundFormat == formatBits && elem.numUsedVoices >= 0) {
					usage += (int)elem.numUsedVoices;
					haveData = true;
				}
			}
		}
		if(haveData) {
			return usage;
		} else {
			return -1;
		}
	}
	public List<byte[]> GetLogList() {
		return eventLogList;
	}
	public List<byte[]> GetLogList(int offset, int count) {
		try {
			offset = Mathf.Clamp(offset, 0, eventLogList.Count - 1);
			count = Mathf.Clamp(count, 0, eventLogList.Count - offset);
			return eventLogList.GetRange(offset, count);
		} catch {
			return new List<byte[]>();
		}
	}
	public void ClearLog() {
		eventLogList.Clear();
	}
	#endregion Fields / Properties

	#region Data Initialization
	private void ResetVals() {
		this.InitVals();
	}

	private void InitVals() {
		this.timestamp = 0;
		this.timestampStart = 0;
		this.cpu_load = 0;
		this.cpuLoadHistory.FillWithZeros();
		this.num_used_voices = 0;
		this.voiceUsageHistory.FillWithZeros();
		this.num_used_streams = 0;
		this.total_bps = 0.0f;
		this.maxVoice_stdOnMemory = 0;
		this.maxVoice_stdStreaming = 0;
		this.maxVoice_hcamxOnMemory = 0;
		this.maxVoice_hcamxStreaming = 0;
		this.loudness_momentary = Mathf.NegativeInfinity;
		this.loudness_shortTerm = Mathf.NegativeInfinity;
		this.loudness_integrated = Mathf.NegativeInfinity;
		this.cuename_lastPlayed = "";
		this.numChMasterOut = 0;
		this.voicePoolInfoTable.Clear();
		this.playbackHashtable.Clear();
		this.multiLaneTimeline.Clear();
		this.levels.Clear();
		this.eventLogList.Clear();
	}
	#endregion

	#region Profiler Control
	/* connection settings */
	private const int TCP_PORT = 2002;
	private const int TCP_CHUNK = 2048;
	private const int TCP_CONNECT_TIMEOUT_MS = 500;
	private const int TCP_RETRY_INTERVAL_MS = 2000;
	private const int TCP_RETRY = 5;
	private const int BUFFERING_INTERVAL_MS = 10;
	public const string LOG_FILE_EXTENSION = "crimon";

	public string ipAddressString = "127.0.0.1";
	public string LogFileSavePath { get; private set; }
	// Starting the TCP sub thread.
	public void StartProfiling(bool saveLog, string logDirPath = null) {
		IPAddress validIp;
		if(IPAddress.TryParse(ipAddressString, out validIp) == false) {
			UnityEngine.Debug.LogWarning("[CRIWARE] (Profiler) IP address not valid - connection aborted. ");
			return;
		}

		this.doSaveLog = saveLog;
		if (this.doSaveLog) {
			if (string.IsNullOrEmpty(logDirPath)) {
				/* default log file path */
				logDirPath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "CriWareProfilerLog");
			}
			this.LogFileSavePath = logDirPath;
			if (Directory.Exists(LogFileSavePath) == false) {
				try {
					Directory.CreateDirectory(LogFileSavePath);
				} catch (Exception e) {
					UnityEngine.Debug.LogError("[CRIWARE] Profiler: failed to create directory for log files. Msg: " + e.Message);
				}
			}
			var filePath = Path.Combine(LogFileSavePath, "CriProfilerLog-" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + "." + LOG_FILE_EXTENSION);
			try {
				this.logStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
			} catch (Exception e) {
				UnityEngine.Debug.LogError("[CRIWARE] Profiler: failed to create log file. Msg: " + e.Message);
				this.logStream = null;
				this.doSaveLog = false;
			}
		}

		TcpParams tcpParams = new TcpParams(
			validIp,
			TCP_PORT,
			TCP_CHUNK,
			TCP_CONNECT_TIMEOUT_MS,
			TCP_RETRY_INTERVAL_MS,
			TCP_RETRY,
			BUFFERING_INTERVAL_MS);
		const int READ_INTERVAL_MS = 0;

		if ((threadTcpClient == null || threadTcpClient.IsAlive == false) &&
			(threadPacketReading == null || threadPacketReading.IsAlive == false)
			) {
			InitVals();
			threadTerminator.Reset();
			threadTcpClient = new Thread(() => TaskTcpClient(tcpParams));
			threadPacketReading = new Thread(() => TaskPacketReading(READ_INTERVAL_MS));
			threadTcpClient.Start();
			threadPacketReading.Start();
		}
	}

	// Stopping the TCP sub thread.
	public void StopProfiling() {
		threadTerminator.Set();
		ResetVals();

		if (this.logStream != null) {
			this.doSaveLog = false;
			try {
				this.logStream.Flush();
				this.logStream.Close();
				this.logStream = null;
			} catch (Exception e) {
				UnityEngine.Debug.LogError("[CRIWARE] Profiler: failed to flush log file. Msg: " + e.Message);
			}
		}
	}

	private CriProfiler() {
		/* hide constructor for singleton */
		InitVals();
	}

	~CriProfiler() {
		this.StopProfiling();
	}
	#endregion

	#region TCP Connection Management
	// TCP connection sub thread task.
	private void TaskTcpClient(TcpParams tcpParams) {
		int failedConnectCnt = 0;

		this.isProfiling = true;

		using (var tcpClient = new TcpClient()) {
			while (true) {
				try {
					tcpClient.Connect(tcpParams.ipAddress, tcpParams.port);
					this.isConnected = true;
					UnityEngine.Debug.Log("[CRIWARE] CRI Profiler connected.");
					BufferingLoop(tcpClient, tcpParams);
				} catch {
					failedConnectCnt += 1;
					if(failedConnectCnt > tcpParams.connectionRetryLimit) {
						UnityEngine.Debug.LogWarning("[CRIWARE] Retry count exceeded limit(" + tcpParams.connectionRetryLimit + "); Stopped.");
						threadTerminator.Set();
					} else {
						UnityEngine.Debug.LogWarning("[CRIWARE] Unable to connect CRI profiler. (Check if \"Uses In Game Preview\" is turned on)"
							+"\nWaiting for " + (tcpParams.connectionRetryIntervalMillisec / 1000.0f).ToString("F1") + " seconds to reconnect... (" + failedConnectCnt+ "/" + tcpParams.connectionRetryLimit + " tries)");
					}
				}
				this.isConnected = false;

				if (threadTerminator.WaitOne(tcpParams.connectionRetryIntervalMillisec)) {
					UnityEngine.Debug.Log("[CRIWARE] CRI Profiler disconnected.");
					this.isProfiling = false;
					break;
				}
			}
		}
	}

	// Looping process to push the socket input to the buffer.
	private void BufferingLoop(TcpClient tcpClient, TcpParams tcpParams) {
		int numBytesRead = 0;
		byte[] chunk = new byte[tcpParams.chunkSize];
		byte[] sendPacket;
		bool enbufferRes = true;

		this.TcpBuffer.Clear();
		using (var netStream = tcpClient.GetStream()) {
			if (netStream == null) {
				UnityEngine.Debug.LogError("[CRIWARE] (Profiler) TCP connection not available.");
				return;
			}

			if (netStream.CanRead == false) {
				UnityEngine.Debug.LogError("[CRIWARE] (Profiler) TCP connection not readable.");
				return;
			}

			sendPacket = MakeLogPacket4StartStopLog(SendPacketType.StartLog);
			if (sendPacket != null && netStream.CanWrite) {
				netStream.Write(sendPacket, 0, sendPacket.Length);
			}

			while (true) {
				try {
					if (netStream.DataAvailable) {
						numBytesRead = netStream.Read(chunk, 0, chunk.Length);
						if (this.isBufferCorrupted == false) {
							lock (this.TcpBuffer) {
								enbufferRes = this.TcpBuffer.EnBuffer(chunk, numBytesRead);
							}
						}
						if (enbufferRes == false) {
							UnityEngine.Debug.LogWarning("[CRIWARE] TCP reading buffer overflowed.");
							
							// -- buffer corrupted -> prepare to reset --
							// Enbuffer failure may cause incomplete packets in the packet queue.
							// The solution we take is skipping all incoming data until a "tail" chunk (followed by no data) arrives,
							// right after which the buffer will be reset.
							this.isBufferCorrupted = true;
						}
					} else {
						if (this.isBufferCorrupted == true) {
							lock (this.TcpBuffer) {
								/* reset the corrupted buffer when all available data are received */
								this.TcpBuffer.Clear();
							}
							this.isBufferCorrupted = false;
						}
					}
				} catch (System.IO.IOException) {
					threadTerminator.Set();
				}

				if (threadTerminator.WaitOne(tcpParams.enBufferIntervalMillisec)) {
					break;
				}
			}

			sendPacket = MakeLogPacket4StartStopLog(SendPacketType.StopLog);
			if (sendPacket != null && netStream.CanWrite) {
				netStream.Write(sendPacket, 0, sendPacket.Length);
			}
		}
	}

	// Sending control packet to start / stop receiving log data
	private enum SendPacketType {
		StartLog,
		StopLog
	}
	private byte[] MakeLogPacket4StartStopLog(SendPacketType sendType) {
		int ptr_size = IntPtr.Size;
		const LogTypes log_type = LogTypes.NON;
		const UInt64 time = 0;
		const LogFuncId function_id = LogFuncId.LOG_COMMAND_Non;
		const UInt32 control_id = 0;
		const UInt64 thread_id = 0;

		int size;
		TcpCommandId command;
		int padding_size;

		switch (sendType) {
			case SendPacketType.StartLog:
				size = TcpLogHeader.properSize
					   /* + sizeof(UInt16)
						* + CriProfiler.LogParams[(int)LogParamItems.LOG_STRINGS_ITEM_LogRecordMode].size32/64;
						*/ + 6;
				command = TcpCommandId.CRITCP_MAIL_START_LOG_RECORD;
				padding_size = 2; /* 8 - (38 mod 8) */
				size += padding_size;
				break;
			case SendPacketType.StopLog:
				size = TcpLogHeader.properSize;
				command = TcpCommandId.CRITCP_MAIL_STOP_LOG_RECORD;
				padding_size = 0; /* 8 - (32 mod 8) */
				size += padding_size;
				break;
			default:
				return null;
		}

		byte[] packetBytes = new byte[size];
		int offset = 0;

		SaveBigEndianUInt32(packetBytes, offset, (UInt32)size);
		offset += sizeof(UInt32);
		SaveBigEndianUInt16(packetBytes, offset, (UInt16)command);
		offset += sizeof(UInt16);
		SaveBigEndianByte(packetBytes, offset, (Byte)ptr_size);
		offset += sizeof(Byte);
		SaveBigEndianByte(packetBytes, offset, (Byte)log_type);
		offset += sizeof(Byte);
		SaveBigEndianUInt64(packetBytes, offset, (UInt64)time);
		offset += sizeof(UInt64);
		SaveBigEndianUInt16(packetBytes, offset, (UInt16)(function_id + CriProfiler.logFuncBaseNum));
		offset += sizeof(UInt16);
		SaveBigEndianUInt16(packetBytes, offset, (UInt16)padding_size);
		offset += sizeof(UInt16);
		SaveBigEndianUInt32(packetBytes, offset, (UInt32)control_id);
		offset += sizeof(UInt32);
		SaveBigEndianUInt64(packetBytes, offset, (UInt64)thread_id);
		offset += sizeof(UInt64);

		switch (sendType) {
			case SendPacketType.StartLog:
				SaveBigEndianUInt16(packetBytes, offset, (UInt16)LogParamId.LOG_STRINGS_ITEM_LogRecordMode);
				offset += sizeof(UInt16);
				SaveBigEndianUInt32(packetBytes, offset, (UInt32)LOG_MODE_ALL); /* size of LOG_STRINGS_ITEM_LogRecordMode */
				offset += sizeof(UInt32);
				break;
			case SendPacketType.StopLog:
				break;
			default:
				return null;
		}

		for (int i = 0; i < padding_size; ++i) {
			packetBytes[offset] = 0;
			offset += 1;
		}

		if (offset == size) {
			return packetBytes;
		} else {
			return null;
		}
	}
	#endregion

	#region Parser
	// Sub thread task to extract TCP packets from the buffer.
	private void TaskPacketReading(int readIntervalMillisec) {
		Stopwatch stopwatch = new Stopwatch();
		const int threadMaxLifeMs = 20000;  /* should be longer than total TCP retrying time */
		const int threadRetryLifeMs = 500;

		byte[] data = null;
		TcpLogHeader packetHeader;
		int packetSize = 0;

		stopwatch.Start();
		while (true) {
			if (this.isBufferCorrupted == false) {
				lock (this.TcpBuffer) {
					packetSize = GetPacketSize(TcpBuffer);
					data = this.TcpBuffer.DeBuffer(packetSize);
				}
				if (data != null) {
					stopwatch.Reset();
					stopwatch.Start();
					packetHeader = new TcpLogHeader(data);
					try {
						if (packetHeader.size > 0) {
							if (this.doSaveLog && this.logStream != null) {
								try {
									this.logStream.Write(data, 0, data.Length);
								} catch (Exception e) {
									UnityEngine.Debug.LogError("[CRIWARE] Profiler: failed to write logs. Msg: " + e.Message);
									this.doSaveLog = false;
								}
							}
							this.Parser(data, packetHeader);
						} /* if headerSize > 0 */
					} catch (Exception ex) { /* Any parsing error */
						if(ex is ParamIdDoesNotMatchException) {
							UnityEngine.Debug.LogWarning("[CRIWARE] " + ex.Message.ToString());
						}
						UnityEngine.Debug.LogWarning("[CRIWARE] (Profiler) Packet parsing failed: buffer may be corrupted. Reconstructing buffer..."
							+"\nLog Function: " + GetLogFuncId(packetHeader.function_id).ToString());
						this.isBufferCorrupted = true;
					}
				} else {
					/* kill all threads if waited for too long */
					if (this.isConnected == true) {
						if (stopwatch.ElapsedMilliseconds > threadRetryLifeMs) {
							this.isProfiling = false;
							threadTerminator.Set();
						}
					} else {
						if (stopwatch.ElapsedMilliseconds > threadMaxLifeMs) {
							this.isProfiling = false;
							threadTerminator.Set();
						}
					}
				} /* if data != null */
			} /* if isBufferCorrupted == false */

			if (threadTerminator.WaitOne(readIntervalMillisec)) {
				this.isProfiling = false;
				break;
			}
		}
	}

	// Get the size of the packet from the header.
	// Return 0 if there is no enough data to determine the size.
	private int GetPacketSize(RingBuffer<byte> buffer) {
		if (buffer == null || buffer.Count < DATA_LENGTH_PARAM_SIZE) {
			return 0;
		}

		uint result = buffer[0];
		for (int i = 1; i < DATA_LENGTH_PARAM_SIZE; ++i) {
			result <<= 8;
			result += buffer[i];
		}

		return (int)result;
	}

	private int GetPacketSize(byte[] data, uint offset = 0) {
		if (data == null || data.Length - offset < DATA_LENGTH_PARAM_SIZE) {
			return 0;
		}

		uint result = data[offset];
		for (int i = 0; i < DATA_LENGTH_PARAM_SIZE; i++) {
			result <<= 8;
			result += data[i + offset];
		}

		return (int)result;
	}

	public void LoadLogFromFile(string path) {
		if (string.IsNullOrEmpty(path)) {
			UnityEngine.Debug.LogError("[CRIWARE] Profiler: Log file path cannot be empty");
		}

		try {
			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
				if (fileStream.CanRead) {
					eventLogList.Clear();
					byte[] reusedSizeData = new byte[DATA_LENGTH_PARAM_SIZE];
					while (fileStream.Position + DATA_LENGTH_PARAM_SIZE < fileStream.Length) {
						var read = fileStream.Read(reusedSizeData, 0, DATA_LENGTH_PARAM_SIZE);
						/* roll back read position */
						fileStream.Position -= read;
						/* get actual data size */
						var dataSize = GetPacketSize(reusedSizeData);
						if (dataSize > 0) {
							byte[] data = new byte[dataSize];
							fileStream.Read(data, 0, dataSize);
							this.LogParser(data);
						} else {
							UnityEngine.Debug.LogError("[CRIWARE] Profiler: Error while reading log");
							break;
						}
					}
				} else {
					UnityEngine.Debug.LogError("[CRIWARE] Profiler: Log file cannot be read");
				}
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogError("[CRIWARE] Profiler: Error while opening log: " + e.Message);
		}
	}

	private void LogParser(byte[] data) {
		var header = new TcpLogHeader(data);

		switch ((TcpCommandId)header.command) {
			case TcpCommandId.CRITCP_MAIL_SEND_LOG:
				switch (GetLogFuncId(header.function_id)) {
					case LogFuncId.LOG_COMMAND_ExPlaybackId:
						this.AddEventLog(data);
						break;
					default:
						break;
				}
				break;
			case TcpCommandId.CRITCP_MAIL_MONITOR_ATOM_EXPLAYBACKINFO_PLAY_END:
				switch (GetLogFuncId(header.function_id)) {
					case LogFuncId.LOG_COMMAND_ExPlaybackInfo_FreeInfo:
						this.AddEventLog(data);
						break;
					default:
						break;
				}
				break;
			default:
				break;
		}
	}

	private void Parser(byte[] data, TcpLogHeader header) {
		int offset = 0;
		int offset2 = 0;
		int dataSize = 0;
		int dataSize2 = 0;

		UInt32 playbackId;
		StreamTypes streamType;
		VoicePoolInfo voicePoolInfo;
		UInt64 voicePoolHn;
		Single levelPeak;
		Single levelRms;
		Single levelPeakHold;
		int numCh = 0;
		int numOutputCh;
		int numMaxCh = 0;

		switch ((TcpCommandId)header.command) {
			case TcpCommandId.CRITCP_MAIL_PREVIEW_CPU_LOAD:
				this.timestamp = header.time;
				if (this.timestampStart == 0) {
					this.timestampStart = this.timestamp;
				}
				if (GetLogFuncId(header.function_id) == LogFuncId.LOG_COMMAND_CpuLoadAndNumUsedVoices) {
					this.cpu_load = LoadBigEndianSingle(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_CpuLoad, out offset));
					this.num_used_voices = LoadBigEndianUInt32(data, GetNextParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_NumUsedVoices, ref offset));
					lock (cpuLoadHistory) {
						cpuLoadHistory.EnBuffer(new Single[] { cpu_load }, 1);
					}
					lock (voiceUsageHistory) {
						voiceUsageHistory.EnBuffer(new UInt32[] { num_used_voices }, 1);
					}
				}
				break;
			case TcpCommandId.CRITCP_MAIL_SEND_LOG:
				this.timestamp = header.time;
				if (this.timestampStart == 0) {
					this.timestampStart = this.timestamp;
				}
				switch (GetLogFuncId(header.function_id)) {
					case LogFuncId.LOG_COMMAND_LoudnessInfo:
						this.loudness_momentary = LoadBigEndianSingle(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_MomentaryValue, out offset));
						this.loudness_shortTerm = LoadBigEndianSingle(data, GetNextParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_ShortTermValue, ref offset));
						this.loudness_integrated = LoadBigEndianSingle(data, GetNextParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_IntegratedValue, ref offset));
						break;
					case LogFuncId.LOG_COMMAND_StreamingInfo:
						this.num_used_streams = LoadBigEndianUInt32(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_NumUsedVoices, out offset));
						this.total_bps = LoadBigEndianSingle(data, GetNextParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_TotalBps, ref offset));
						break;
					case LogFuncId.LOG_COMMAND_AsrBusAnalyzeInfoAllCh:
						if (LoadBigEndianByte(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_BusNo, out offset)) == 0) {    /* if the bus is MasterOut(bus0) */
							numCh = LoadBigEndianByte(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_NumCh, out offset));
							lock (levels) {
								if (numMaxCh == 0) {
									numMaxCh = numCh;
									levels.Clear();
									for (int i = 0; i < numMaxCh; ++i) {
										levels.Add(new LevelInfo(Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity));
									}
								}
								if (numMaxCh == numCh) {
									for (int i = 0; i < numCh; ++i) {
										levelPeak = LoadBigEndianSingle(data, GetNextParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_PeakLevel, ref offset));
										levelRms = LoadBigEndianSingle(data, GetNextParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_RmsLevel, ref offset));
										levelPeakHold = LoadBigEndianSingle(data, GetNextParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_PeakHoldLevel, ref offset));
										levels[i] = new LevelInfo(levelPeak, levelRms, levelPeakHold);
									}
								}
							}
						}
						break;
					case LogFuncId.LOG_COMMAND_ExStandardVoicePoolConfig:
						if (Convert.ToBoolean(LoadBigEndianByte(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_StreamingFlag, out offset))) == false) {
							this.maxVoice_stdOnMemory = LoadBigEndianUInt32(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_NumVoices, out offset));
						} else {
							this.maxVoice_stdStreaming = LoadBigEndianUInt32(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_NumVoices, out offset));
						}
						break;
					case LogFuncId.LOG_COMMAND_ExHcaMxVoicePoolConfig:
						if (Convert.ToBoolean(LoadBigEndianByte(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_StreamingFlag, out offset))) == false) {
							this.maxVoice_hcamxOnMemory = LoadBigEndianUInt32(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_NumVoices, out offset));
						} else {
							this.maxVoice_hcamxStreaming = LoadBigEndianUInt32(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_NumVoices, out offset));
						}
						break;
					case LogFuncId.LOG_COMMAND_ExVoicePoolHn:
						try {
							voicePoolHn = LoadBigEndianUInt64(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_ExVoicePoolHn, out offset));
							voicePoolInfo = new VoicePoolInfo(LoadBigEndianUInt32(data, GetNextParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_SoundFormat, ref offset)), null);
							lock (voicePoolInfoTable) {
								voicePoolInfoTable.Add(voicePoolHn, voicePoolInfo);
							}
						} catch (ArgumentException) {
							/* Voice pool info already exists; Do nothing */
						}
						break;
					case LogFuncId.LOG_COMMAND_ExVoicePool_Free:
						voicePoolHn = LoadBigEndianUInt64(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_ExVoicePoolHn, out offset));
						lock (voicePoolInfoTable) {
							voicePoolInfoTable.Remove(voicePoolHn);
						}
						break;
					case LogFuncId.LOG_COMMAND_PlayerPool_NumVoices:
						voicePoolHn = LoadBigEndianUInt64(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_ExVoicePoolHn, out offset));
						lock (voicePoolInfoTable) {
							if (voicePoolInfoTable[voicePoolHn] != null) {
								(voicePoolInfoTable[voicePoolHn] as VoicePoolInfo).numUsedVoices = (int)LoadBigEndianUInt32(data, GetNextParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_NumUsedVoices, ref offset));
							}
						}
						break;
					case LogFuncId.LOG_COMMAND_ExPlaybackId:
						playbackId = LoadBigEndianUInt32(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_ExPlaybackId, out offset));
						dataSize = LoadBigEndianUInt16(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_CueName, out offset)); /* cue name */
                        dataSize2 = LoadBigEndianUInt16(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_ExAcbName, out offset2)); /* cuesheet name */
						if (dataSize > 1 && dataSize2 > 1) {
							this.cuename_lastPlayed = System.Text.Encoding.Default.GetString(data, GetStrDataOffset(header, offset), dataSize);
                            this.cuesheetName_lastPlayed = System.Text.Encoding.Default.GetString(data, GetStrDataOffset(header, offset2), dataSize2);
							try {
								lock (playbackHashtable) {
									this.playbackHashtable.Add(playbackId, new PlaybackInfo(playbackId, cuename_lastPlayed, cuesheetName_lastPlayed, header.time));
								}
							} catch (ArgumentException) {
								/* Duplicated playback info received; Do nothing */
							}
							this.AddEventLog(data);
							this.AddPlaybackToTimeline(new Playback(playbackId, header.time, true, cuename_lastPlayed, cuesheetName_lastPlayed));
						} else {
							this.cuename_lastPlayed = "";
						}
						break;
					case LogFuncId.LOG_COMMAND_SoundVoice_Allocate:
						playbackId = LoadBigEndianUInt32(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_ExPlaybackId, out offset));
						streamType = (StreamTypes)LoadBigEndianByte(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_StreamType, out offset));
						lock (playbackHashtable) {
							if (playbackHashtable.ContainsKey(playbackId)) {
								(playbackHashtable[playbackId] as PlaybackInfo).StreamType = (int)streamType;
							}
						}
						break;
					case LogFuncId.LOG_COMMAND_ExAsrConfig:
						numOutputCh = LoadBigEndianByte(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_OutputChannels, out offset));
						if (numOutputCh > this.numChMasterOut) {
							numChMasterOut = numOutputCh;
						}
						break;
					default:
						break;
				}
				break;
			case TcpCommandId.CRITCP_MAIL_MONITOR_ATOM_EXPLAYBACKINFO_PLAY_END:
				this.timestamp = header.time;
				if (this.timestampStart == 0) {
					this.timestampStart = this.timestamp;
				}
				switch (GetLogFuncId(header.function_id)) {
					case LogFuncId.LOG_COMMAND_ExPlaybackInfo_FreeInfo:
						playbackId = LoadBigEndianUInt32(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_ExPlaybackId, out offset));
						lock (playbackHashtable) {
							if (playbackHashtable.ContainsKey(playbackId)) {
								var info = playbackHashtable[playbackId] as PlaybackInfo;
								this.AddEventLog(data);
								this.AddPlaybackToTimeline(new Playback(playbackId, header.time, false, info.cueName, info.cueSheetName));
								playbackHashtable.Remove(playbackId);
							}
						}
						break;
					default:
						break;
				}
				break;
			default:
				UserDefinedParser(data, header);
				break;
		}
	}

	// Override this method to add new parsing procedure
	protected virtual void UserDefinedParser(byte[] data, TcpLogHeader packetHeader) {
		// e.g.
		//int offset;
		//switch ((TcpCommandId)packetHeader.command) {
		//	case TcpCommandId.<CRITCP_MAIL_xxx>:
		//		switch (GetLogFuncId(packetHeader.function_id)) {
		//			case LogFuncId.<LOG_COMMAND_xxx>:
		//				<parsing>
		//				break;
		//			default:
		//				break;
		//		}
		//		break;
		//	default:
		//		break;
		//}
	}

	private void AddEventLog(byte[] data) {
		this.eventLogList.Add(data);
	}

	static private LogFuncId GetLogFuncId(UInt16 rawId) {
		return (LogFuncId)(rawId - CriProfiler.logFuncBaseNum);
	}

	public string FilteredLog2String(byte[] data) {
		if (data == null || data.Length < TcpLogHeader.properSize) { return string.Empty; }

		var header = new TcpLogHeader(data);
		var funcId = GetLogFuncId(header.function_id);
		uint playbackId;
		int dataSize, dataSize2, offset, offset2;
		string str1, str2;

		string output = string.Empty;
		switch (funcId) {
			case LogFuncId.LOG_COMMAND_ExPlaybackId:
				playbackId = LoadBigEndianUInt32(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_ExPlaybackId, out offset));
				dataSize = LoadBigEndianUInt16(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_CueName, out offset)); /* cue name */
				dataSize2 = LoadBigEndianUInt16(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_ExAcbName, out offset2)); /* cuesheet name */
				str1 = System.Text.Encoding.Default.GetString(data, GetStrDataOffset(header, offset), dataSize);
				str2 = System.Text.Encoding.Default.GetString(data, GetStrDataOffset(header, offset2), dataSize2);
				output = MicroSec2String(header.time) + " | Start ID : " + playbackId + " | cuename : " + str1 + " | cuesheet : " + str2;
				break;
			case LogFuncId.LOG_COMMAND_ExPlaybackInfo_FreeInfo:
				playbackId = LoadBigEndianUInt32(data, FindParamOffset(data, header, LogParamId.LOG_STRINGS_ITEM_ExPlaybackId, out offset));
				output = MicroSec2String(header.time) + " | End ID : " + playbackId;
				break;
			default:
				break;
		}

		return output;
	}

	static private string MicroSec2String(ulong microSec) {
		ulong hour = microSec / 3600000000;
		ulong min = (microSec % 3600000000) / 60000000;
		ulong sec = (microSec % 60000000) / 1000000;
		ulong msec = (microSec % 1000000) / 1000;
		return string.Format("{0}:{1}:{2}.{3}", hour, min, sec, msec);
	}

	#region Methods for data offset calculation
	private const int paramIdSize = 2;

	private int GetStrDataOffset(TcpLogHeader header, int paramIdOffset) {
		return paramIdOffset + CriProfiler.paramIdSize + (header.isPtr64
			? ParamTypeSizes64[(int)CriProfiler.LogParamTypes.TYPE_CHAR]
			: ParamTypeSizes32[(int)CriProfiler.LogParamTypes.TYPE_CHAR]);
	}

	private int FindParamOffset(byte[] data, TcpLogHeader header, LogParamId logParamId, out int currentOffset) {
		if (data.Length < TcpLogHeader.properSize) {
			throw new ParamIdDoesNotMatchException("Could not find parameter: " + logParamId.ToString());
		}
		int offset = header.size;
		int paramId;
		int dataSize;
		while (offset < header.packetSize) {
			paramId = LoadBigEndianUInt16(data, offset);
			if ((LogParamId)paramId == logParamId) {
				currentOffset = offset;
				return offset + paramIdSize;
			}

			dataSize = header.isPtr64 ? LogParams[paramId].size64 : LogParams[paramId].size32;
			if (LogParams[paramId].type == LogParamTypes.TYPE_CHAR) {
				dataSize += LoadBigEndianUInt16(data, offset + paramIdSize);
			}
			offset += paramIdSize + dataSize;
		}

		throw new ParamIdDoesNotMatchException("Could not find parameter: " + logParamId.ToString());
	}

	private int GetNextParamOffset(byte[] data, TcpLogHeader header, LogParamId logParamId, ref int currentOffset) {
		if (currentOffset >= header.packetSize) {
			throw new ParamIdDoesNotMatchException("Could not find parameter: " + logParamId.ToString());
		}
		int paramId = LoadBigEndianUInt16(data, currentOffset);
		int dataSize = header.isPtr64 ? CriProfiler.LogParams[paramId].size64 : CriProfiler.LogParams[paramId].size32;
		if (CriProfiler.LogParams[paramId].type == LogParamTypes.TYPE_CHAR) {
			dataSize += LoadBigEndianUInt16(data, currentOffset + paramIdSize);
		}
		/* set parameter id offset */
		currentOffset += paramIdSize + dataSize;

		if ((LogParamId)LoadBigEndianUInt16(data, currentOffset) != logParamId) {
			throw new ParamIdDoesNotMatchException("Parsed parameter ID does not match with " + logParamId.ToString());
		}

		/* return parameter data offset */
		return currentOffset + paramIdSize;
	}
	#endregion Methods for data offset calculation

	#endregion Parser


	#region Timeline

	/* CueSheet - Cue - Playback Lane (multiple non-overlapping playback lists) - Timestamp (true = start, false = end) */

	private void AddPlaybackToTimeline(Playback playback) {
		CueSheetGroup group;
		if (multiLaneTimeline.TryGetValue(playback.cuesheet, out group) == false) {
			group = new CueSheetGroup(playback.cuesheet);
			multiLaneTimeline.Add(playback.cuesheet, group);
		}
		group.AddPlayback(playback);
	}

	public Dictionary<string, CueSheetGroup> TimelineDataSlice {
		get {
			var output = new Dictionary<string, CueSheetGroup>();
			foreach (var elem in multiLaneTimeline) {
				output.Add(elem.Key, elem.Value.Clone());
			}
			return output;
		}
	}

	public class CueSheetGroup : ICloneable {
		public string name;
		public Color cuesheetColor = Color.white;
		public Dictionary<string, CueLane> cueLaneDict;

		static private readonly Color baseColor = new Color(0.5412f, 1f, 0.3451f);

		public CueSheetGroup(string name) {
			this.name = name;
			{/* set distinguishing color */
				float h, s, v;
				Color.RGBToHSV(baseColor, out h, out s, out v);
				System.Random rand = new System.Random();
				this.cuesheetColor = Color.HSVToRGB(rand.Next(0, 10000) / 10000f, s, v);
			}
			cueLaneDict = new Dictionary<string, CueLane>();
		}

		public CueSheetGroup(CueSheetGroup group) {
			this.name = group.name;
			this.cuesheetColor = group.cuesheetColor;
			this.cueLaneDict = new Dictionary<string, CueLane>();
			foreach (var elem in group.cueLaneDict) {
				cueLaneDict.Add(elem.Key, elem.Value.Clone());
			}
		}
		public CueSheetGroup Clone() { return new CueSheetGroup(this); }
		object ICloneable.Clone() { return Clone(); }

		public void AddPlayback(Playback playback) {
			CueLane lane;
			if (cueLaneDict.TryGetValue(playback.cuename, out lane) == false) {
				lane = new CueLane(playback.cuename, playback.cuesheet);
				cueLaneDict.Add(playback.cuename, lane);
			}
			lane.AddPlayback(playback);
		}

		public int TotalLaneCount {
			get {
				int cnt = 0;
				foreach (var elem in cueLaneDict) {
					cnt += elem.Value.playbackLaneList.Count;
				}
				return cnt;
			}
		}
	}

	public class CueLane : ICloneable {
		public string name;
		public string cuesheet;
		public Color cueColor;
		public List<PlaybackLane> playbackLaneList;

		static private readonly Color baseColor = new Color(0.537f, 0.922f, 0.424f);
		private Dictionary<uint, int> playingDict; /* <playback id, lane index>; wont be cloned */

		public CueLane(string name, string cuesheet) {
			this.name = name;
			this.cuesheet = cuesheet;
			{/* set distinguishing color */
				float h, s, v;
				Color.RGBToHSV(baseColor, out h, out s, out v);
				System.Random rand = new System.Random();
				this.cueColor = Color.HSVToRGB(rand.Next(0, 10000) / 10000f, s, v);
			}
			playbackLaneList = new List<PlaybackLane>();
			playingDict = new Dictionary<uint, int>();
		}

		public CueLane(CueLane cuelane) {
			this.name = cuelane.name;
			this.cuesheet = cuelane.cuesheet;
			this.cueColor = cuelane.cueColor;
			this.playbackLaneList = new List<PlaybackLane>();
			this.playbackLaneList.AddRange(cuelane.playbackLaneList.Select(i => i.Clone()));
		}

		public CueLane Clone() { return new CueLane(this); }
		object ICloneable.Clone() { return Clone(); }

		public void AddPlayback(Playback playback) {
			if (playback.isStart) { /* playback start */
				bool added = false;
				for (int i = 0; i < playbackLaneList.Count; ++i) {
					if (playbackLaneList[i].isPlaying == false) {
						if (playbackLaneList[i].AddPlayback(playback)) {
							playingDict.Add(playback.id, i);
							added = true;
							break;
						}
					}
				}
				if (added == false) { /* no empty lane */
					playbackLaneList.Add(new PlaybackLane());
					if (playbackLaneList[playbackLaneList.Count - 1].AddPlayback(playback)) {
						playingDict.Add(playback.id, playbackLaneList.Count - 1);
					}
				}
			} else { /* playback end */
				int laneId;
				if (playingDict.TryGetValue(playback.id, out laneId)) {
					playbackLaneList[laneId].AddPlayback(playback);
					playingDict.Remove(playback.id);
				}
			}
		}
	}

	public class PlaybackLane : ICloneable {
		public List<Playback> playbackList;
		private int balance;

		public bool isConsistent {
			get {
				if (balance > 1 || balance < 0) {
					return false;
				}
				return true;
			}
		}

		public PlaybackLane() {
			playbackList = new List<Playback>();
			balance = 0;
		}

		public PlaybackLane(PlaybackLane playbackLane) {
			this.playbackList = new List<Playback>(playbackLane.playbackList);
			this.balance = playbackLane.balance;
		}

		public PlaybackLane Clone() { return new PlaybackLane(this); }
		object ICloneable.Clone() { return Clone(); }

		public bool AddPlayback(Playback playback) {
			if (playbackList.Count > 0 &&
				(playbackList[playbackList.Count - 1].timestamp > playback.timestamp ||
				playbackList[playbackList.Count - 1].isStart == playback.isStart)) {
				UnityEngine.Debug.LogError("[CRIWARE] Profiler internal: playback timestamp order error");
				return false;
			}
			playbackList.Add(playback);
			balance += playback.isStart ? 1 : -1;
			return true;
		}

		public bool isPlaying {
			get {
				return playbackList[playbackList.Count - 1].isStart;
			}
		}
	}

	public struct Playback {
		public uint id;
		public ulong timestamp;
		public bool isStart;
		public string cuename;
		public string cuesheet;
		public Playback(uint id, ulong timestamp, bool isStart, string cuename, string cuesheet) {
			this.id = id;
			this.timestamp = timestamp;
			this.isStart = isStart;
			this.cuename = cuename;
			this.cuesheet = cuesheet;
		}
	}
	#endregion
}

} //namespace CriWare

#endif