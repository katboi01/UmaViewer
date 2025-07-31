using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UnityCameraVMDRecorder : MonoBehaviour
{
    public const string FileSavePath = "/../VMDRecords";
    const float WorldScaleFix = 12.5f;
    const float FPSs = 0.03333f;

    public bool RecordMainCamera = false;
    /// <summary>
    /// カメラの座標・回転を絶対座標系で計算する
    /// </summary>
    public bool UseAbsoluteCoordinateSystem = true;
    public bool IgnoreInitialPosition;
    public bool IgnoreInitialRotation;
    public Vector3 Offset = Vector3.zero;
    public int KeyReductionLevel = 2;
    public List<Vector3> LocalPositions { get; private set; } = new List<Vector3>();
    public List<Vector3> LocalRotations { get; private set; } = new List<Vector3>();

    public bool IsRecording { get; private set; } = false;
    public int FrameNumber { get; private set; } = 0;

    Transform targetCameraTransform = null;
    Camera targetCamera;
    Vector3 initialPosition = Vector3.zero;
    Quaternion initialRotation = Quaternion.identity;
    int frameNumberSaved = 0;
    List<Vector3> positionsSaved = new List<Vector3>();
    List<Vector3> rotationsSaved = new List<Vector3>();

    Vector3 LastRotation;
    bool firstKey;
    Vector3 finalRotation;

    // Start is called before the first frame update
    public void Initialize()
    {
        Time.fixedDeltaTime = FPSs;

        SetCameraTransform();

        if (targetCameraTransform != null)
        {
            SetInitialPositionAndRotation();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (IsRecording)
        {
            SaveFrame();
            FrameNumber++;
        }
    }

    void SetInitialPositionAndRotation()
    {
        if (UseAbsoluteCoordinateSystem)
        {
            initialPosition = targetCameraTransform.position;
            initialRotation = targetCameraTransform.rotation;
        }
        else
        {
            initialPosition = targetCameraTransform.localPosition;
            initialRotation = targetCameraTransform.localRotation;
        }
    }

    void SetCameraTransform()
    {
        if (RecordMainCamera)
        {
            if (Camera.main != null)
            {
                targetCameraTransform = Camera.main.transform;
                targetCamera = Camera.main;
            }
            else
            {
                enabled = false;
            }
        }
        else
        {
            targetCamera = GetComponent<Camera>();

            if (targetCamera != null)
            {
                targetCameraTransform = transform;
            }
            else
            {
                enabled = false;
            }
        }
    }

    void SaveFrame()
    {
        Vector3 position = Vector3.zero;
        if (targetCameraTransform != null)
        {
            position = UseAbsoluteCoordinateSystem ? targetCameraTransform.position : targetCameraTransform.localPosition;
        }
        if (IgnoreInitialPosition) { position -= initialPosition; }
        Quaternion rotation = Quaternion.identity;
        if (targetCameraTransform != null)
        {
            rotation = UseAbsoluteCoordinateSystem ? targetCameraTransform.rotation : targetCameraTransform.localRotation;
        }
        if (IgnoreInitialRotation) { rotation = rotation.MinusRotation(initialRotation); }

        //座標系の変換
        Vector3 vmdPosition = new Vector3(-position.x, position.y, -position.z);
        Quaternion fixedRotation = new Quaternion(-rotation.x, rotation.y, -rotation.z, rotation.w);
        Vector3 vmdRotation = new Vector3(fixedRotation.eulerAngles.x, 180 - fixedRotation.eulerAngles.y, fixedRotation.eulerAngles.z);

        if (!firstKey)
        {
            finalRotation = vmdRotation;
            firstKey = true;
        }
        else
        {
            finalRotation += DeltaVector(vmdRotation, LastRotation);
        }

        LocalPositions.Add(vmdPosition);
        LocalRotations.Add(finalRotation);

        LastRotation = vmdRotation;
    }

    public static Vector3 DeltaVector(Vector3 val ,Vector3 lastVal)
    {
        return new Vector3(DeltaDegree(val.x, lastVal.x), DeltaDegree(val.y, lastVal.y), DeltaDegree(val.z, lastVal.z));
    }

    public static float DeltaDegree(float val,float lastVal)
    {
        var deltaVal = val - lastVal;
        if (Mathf.Abs(deltaVal) < 180)
        {
            return deltaVal;
        }
        else
        {
            if (deltaVal >= 0)
            {
                return deltaVal - 360;
            }
            else
            {
                return 360 + deltaVal;
            }
        }
    }

    public static void SetFPS(int fps)
    {
        Time.fixedDeltaTime = 1 / (float)fps;
    }

    public void SetCameraTransform(Camera camera)
    {
        targetCameraTransform = camera.transform;
        this.targetCamera = camera;
    }

    /// <summary>
    /// レコーディングを開始または再開
    /// </summary>
    public void StartRecording()
    {
        SetInitialPositionAndRotation();
        IsRecording = true;
    }

    /// <summary>
    /// レコーディングを一時停止
    /// </summary>
    public void PauseRecording() { IsRecording = false; }

    /// <summary>
    /// レコーディングを終了
    /// </summary>
    public void StopRecording()
    {
        IsRecording = false;
        frameNumberSaved = FrameNumber;
        positionsSaved = LocalPositions;
        rotationsSaved = LocalRotations;

        FrameNumber = 0;
        LocalPositions = new List<Vector3>();
        LocalRotations = new List<Vector3>();
    }

    /// <summary>
    /// VMDを作成する
    /// 呼び出す際は先にStopRecordingを呼び出すこと
    /// </summary>
    /// <param name="filePath">保存先の絶対ファイルパス</param>
    public void SaveVMD()
    {
        string fileName = $"{Application.dataPath}{FileSavePath}/{string.Format("Camera_{0}.vmd", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"))}";
        Directory.CreateDirectory(Application.dataPath + FileSavePath);
        if (IsRecording)
        {
            Debug.Log(targetCameraTransform.name + "VMD保存前にレコーディングをストップしてください。");
            return;
        }

        if (KeyReductionLevel <= 0) { KeyReductionLevel = 1; }

        const string modelName = "カメラ・照明";

        float fieldOfView = targetCamera.fieldOfView;
        bool isOrthographic = targetCamera.orthographic;

        Debug.Log(targetCameraTransform.name + "VMDファイル作成開始");

        //ファイルの書き込み
        using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
        using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
        {
            try
            {
                const string ShiftJIS = "shift_jis";
                const int intByteLength = 4;

                //ファイルタイプの書き込み
                const int fileTypeLength = 30;
                const string RightFileType = "Vocaloid Motion Data 0002";
                byte[] fileTypeBytes = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes(RightFileType);
                binaryWriter.Write(fileTypeBytes, 0, fileTypeBytes.Length);
                binaryWriter.Write(new byte[fileTypeLength - fileTypeBytes.Length], 0, fileTypeLength - fileTypeBytes.Length);

                //モーション名の書き込み、Shift_JISで保存
                const int motionNameLength = 20;
                byte[] motionNameBytes = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes(modelName);
                binaryWriter.Write(motionNameBytes, 0, Mathf.Min(motionNameBytes.Length, 20));
                if (motionNameLength - motionNameBytes.Length > 0)
                {
                    binaryWriter.Write(new byte[motionNameLength - motionNameBytes.Length], 0, motionNameLength - motionNameBytes.Length);
                }

                //全ボーンフレーム数の書き込み
                byte[] allKeyFrameNumberByte = BitConverter.GetBytes(0);
                binaryWriter.Write(allKeyFrameNumberByte, 0, intByteLength);

                //全モーフフレーム数の書き込み
                byte[] faceFrameCount = BitConverter.GetBytes(0);
                binaryWriter.Write(faceFrameCount, 0, intByteLength);

                //カメラの書き込み
                byte[] cameraFrameCount = BitConverter.GetBytes((uint)(frameNumberSaved / KeyReductionLevel));
                binaryWriter.Write(cameraFrameCount, 0, intByteLength);
                for (int i = 0; i < frameNumberSaved; i++)
                {
                    if ((i % KeyReductionLevel) != 0) { continue; }

                    byte[] frameNumberByte = BitConverter.GetBytes((ulong)i);
                    binaryWriter.Write(frameNumberByte, 0, intByteLength);

                    byte[] cameraDistanceByte = BitConverter.GetBytes(0);
                    binaryWriter.Write(cameraDistanceByte, 0, intByteLength);

                    Vector3 position = positionsSaved[i] * WorldScaleFix;
                    byte[] positionX = BitConverter.GetBytes(position.x);
                    binaryWriter.Write(positionX, 0, intByteLength);
                    byte[] positionY = BitConverter.GetBytes(position.y);
                    binaryWriter.Write(positionY, 0, intByteLength);
                    byte[] positionZ = BitConverter.GetBytes(position.z);
                    binaryWriter.Write(positionZ, 0, intByteLength);

                    Vector3 rotation = rotationsSaved[i] * Mathf.Deg2Rad;
                    byte[] rotationX = BitConverter.GetBytes(rotation.x);
                    binaryWriter.Write(rotationX, 0, intByteLength);
                    byte[] rotationY = BitConverter.GetBytes(rotation.y);
                    binaryWriter.Write(rotationY, 0, intByteLength);
                    byte[] rotationZ = BitConverter.GetBytes(rotation.z);
                    binaryWriter.Write(rotationZ, 0, intByteLength);

                    byte[] interpolateBytes = new byte[24];
                    binaryWriter.Write(interpolateBytes, 0, 24);

                    byte[] viewAngleByte = BitConverter.GetBytes((ulong)fieldOfView);
                    binaryWriter.Write(viewAngleByte, 0, intByteLength);

                    byte perspectiveByte = Convert.ToByte(isOrthographic);
                    byte[] perspectiveBytes = new byte[] { perspectiveByte };
                    binaryWriter.Write(new byte[] { perspectiveByte }, 0, perspectiveBytes.Length);
                }

                //照明の書き込み
                byte[] lightFrameCount = BitConverter.GetBytes(0);
                binaryWriter.Write(lightFrameCount, 0, intByteLength);

                //セルフシャドウの書き込み
                byte[] selfShadowCount = BitConverter.GetBytes(0);
                binaryWriter.Write(selfShadowCount, 0, intByteLength);

                //IKの書き込み
                byte[] ikCount = BitConverter.GetBytes(0);
                binaryWriter.Write(ikCount, 0, intByteLength);
            }
            catch (Exception ex)
            {
                Debug.Log("VMD書き込みエラー" + ex.Message);
            }
            finally
            {
                binaryWriter.Close();
            }
        }
        Debug.Log(targetCameraTransform.name + "VMDファイル作成終了");
        Destroy(this);
    }


    public static void SaveLiveCameraVMD(LiveEntry liveEntry,DateTime time, List<LiveCameraFrame> frames, int MultiCameraIndex = -1)
    {
        string directory = $"{Application.dataPath}{FileSavePath}/Live{liveEntry.MusicId}_{time.ToString("yyyy-MM-dd_HH-mm-ss")}/";
        Directory.CreateDirectory(directory);
        string fileName = $"{directory}LiveCamera_POS.vmd";
        string fovFileName = $"{directory}LiveCamera_FOV.vmd";
        if(MultiCameraIndex >= 0)
        {
            fileName = $"{directory}LiveMultiCamera{MultiCameraIndex}_POS.vmd";
            fovFileName = $"{directory}LiveMultiCamera{MultiCameraIndex}_FOV.vmd";
        }

        const string modelName = "カメラ・照明";

        Debug.Log("Live Camera VMDファイル作成開始");

        //ファイルの書き込み
        using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
        using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
        {
            try
            {
                const string ShiftJIS = "shift_jis";
                const int intByteLength = 4;

                //ファイルタイプの書き込み
                const int fileTypeLength = 30;
                const string RightFileType = "Vocaloid Motion Data 0002";
                byte[] fileTypeBytes = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes(RightFileType);
                binaryWriter.Write(fileTypeBytes, 0, fileTypeBytes.Length);
                binaryWriter.Write(new byte[fileTypeLength - fileTypeBytes.Length], 0, fileTypeLength - fileTypeBytes.Length);

                //モーション名の書き込み、Shift_JISで保存
                const int motionNameLength = 20;
                byte[] motionNameBytes = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes("CameraAttach to 全ての親");
                binaryWriter.Write(motionNameBytes, 0, Mathf.Min(motionNameBytes.Length, 20));
                if (motionNameLength - motionNameBytes.Length > 0)
                {
                    binaryWriter.Write(new byte[motionNameLength - motionNameBytes.Length], 0, motionNameLength - motionNameBytes.Length);
                }

                //全ボーンフレーム数の書き込み
                byte[] allKeyFrameNumberByte = BitConverter.GetBytes((uint)frames.Count);
                binaryWriter.Write(allKeyFrameNumberByte, 0, intByteLength);

                foreach (var frame in frames)
                {
                    const int boneNameLength = 15;
                    string boneNameString = "全ての親";

                    byte[] boneNameBytes = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes(boneNameString);
                    binaryWriter.Write(boneNameBytes, 0, boneNameBytes.Length);
                    binaryWriter.Write(new byte[boneNameLength - boneNameBytes.Length], 0, boneNameLength - boneNameBytes.Length);

                    byte[] frameNumberByte = BitConverter.GetBytes((ulong)(frame.frameIndex / 2)); //Set 30 FPS
                    binaryWriter.Write(frameNumberByte, 0, intByteLength);

                    Vector3 position = frame.Position * WorldScaleFix;
                    byte[] positionX = BitConverter.GetBytes(position.x);
                    binaryWriter.Write(positionX, 0, intByteLength);
                    byte[] positionY = BitConverter.GetBytes(position.y);
                    binaryWriter.Write(positionY, 0, intByteLength);
                    byte[] positionZ = BitConverter.GetBytes(position.z);
                    binaryWriter.Write(positionZ, 0, intByteLength);
                    Quaternion rotation = frame.Rotation;
                    byte[] rotationX = BitConverter.GetBytes(rotation.x);
                    binaryWriter.Write(rotationX, 0, intByteLength);
                    byte[] rotationY = BitConverter.GetBytes(rotation.y);
                    binaryWriter.Write(rotationY, 0, intByteLength);
                    byte[] rotationZ = BitConverter.GetBytes(rotation.z);
                    binaryWriter.Write(rotationZ, 0, intByteLength);
                    byte[] rotationW = BitConverter.GetBytes(rotation.w);
                    binaryWriter.Write(rotationW, 0, intByteLength);

                    byte[] interpolateBytes = new byte[64];
                    binaryWriter.Write(interpolateBytes, 0, 64);
                }

                //全モーフフレーム数の書き込み
                byte[] faceFrameCount = BitConverter.GetBytes(0);
                binaryWriter.Write(faceFrameCount, 0, intByteLength);

                //カメラの書き込み
                byte[] cameraFrameCount = BitConverter.GetBytes(0);
                binaryWriter.Write(cameraFrameCount, 0, intByteLength);

                //照明の書き込み
                byte[] lightFrameCount = BitConverter.GetBytes(0);
                binaryWriter.Write(lightFrameCount, 0, intByteLength);

                //セルフシャドウの書き込み
                byte[] selfShadowCount = BitConverter.GetBytes(0);
                binaryWriter.Write(selfShadowCount, 0, intByteLength);

                //IKの書き込み
                byte[] ikCount = BitConverter.GetBytes(0);
                binaryWriter.Write(ikCount, 0, intByteLength);
            }
            catch (Exception ex)
            {
                Debug.Log("VMD書き込みエラー" + ex.Message);
            }
            finally
            {
                binaryWriter.Close();
            }
        }

        using (FileStream fileStream = new FileStream(fovFileName, FileMode.Create))
        using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
        {
            try
            {
                const string ShiftJIS = "shift_jis";
                const int intByteLength = 4;

                //ファイルタイプの書き込み
                const int fileTypeLength = 30;
                const string RightFileType = "Vocaloid Motion Data 0002";
                byte[] fileTypeBytes = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes(RightFileType);
                binaryWriter.Write(fileTypeBytes, 0, fileTypeBytes.Length);
                binaryWriter.Write(new byte[fileTypeLength - fileTypeBytes.Length], 0, fileTypeLength - fileTypeBytes.Length);

                //モーション名の書き込み、Shift_JISで保存
                const int motionNameLength = 20;
                byte[] motionNameBytes = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes(modelName);
                binaryWriter.Write(motionNameBytes, 0, Mathf.Min(motionNameBytes.Length, 20));
                if (motionNameLength - motionNameBytes.Length > 0)
                {
                    binaryWriter.Write(new byte[motionNameLength - motionNameBytes.Length], 0, motionNameLength - motionNameBytes.Length);
                }

                //全ボーンフレーム数の書き込み
                byte[] allKeyFrameNumberByte = BitConverter.GetBytes(0);
                binaryWriter.Write(allKeyFrameNumberByte, 0, intByteLength);

                //全モーフフレーム数の書き込み
                byte[] faceFrameCount = BitConverter.GetBytes(0);
                binaryWriter.Write(faceFrameCount, 0, intByteLength);

                var fovkeys = frames.FindAll(k => k.FovVaild);
                //カメラの書き込み
                byte[] cameraFrameCount = BitConverter.GetBytes((uint)fovkeys.Count);
                binaryWriter.Write(cameraFrameCount, 0, intByteLength);

                foreach (var frame in fovkeys)
                {
                    byte[] frameNumberByte = BitConverter.GetBytes((ulong)frame.frameIndex/2);
                    binaryWriter.Write(frameNumberByte, 0, intByteLength);

                    byte[] cameraDistanceByte = BitConverter.GetBytes(0);
                    binaryWriter.Write(cameraDistanceByte, 0, intByteLength);

                    Vector3 position = Vector3.zero;
                    byte[] positionX = BitConverter.GetBytes(position.x);
                    binaryWriter.Write(positionX, 0, intByteLength);
                    byte[] positionY = BitConverter.GetBytes(position.y);
                    binaryWriter.Write(positionY, 0, intByteLength);
                    byte[] positionZ = BitConverter.GetBytes(position.z);
                    binaryWriter.Write(positionZ, 0, intByteLength);

                    Vector3 rotation = new Vector3(0,180,0) * Mathf.Deg2Rad;
                    byte[] rotationX = BitConverter.GetBytes(rotation.x);
                    binaryWriter.Write(rotationX, 0, intByteLength);
                    byte[] rotationY = BitConverter.GetBytes(rotation.y);
                    binaryWriter.Write(rotationY, 0, intByteLength);
                    byte[] rotationZ = BitConverter.GetBytes(rotation.z);
                    binaryWriter.Write(rotationZ, 0, intByteLength);

                    byte[] interpolateBytes = new byte[24];
                    binaryWriter.Write(interpolateBytes, 0, 24);

                    byte[] viewAngleByte = BitConverter.GetBytes((ulong)frame.Fov);
                    binaryWriter.Write(viewAngleByte, 0, intByteLength);

                    byte perspectiveByte = Convert.ToByte(frame.isOrthographic);
                    byte[] perspectiveBytes = new byte[] { perspectiveByte };
                    binaryWriter.Write(new byte[] { perspectiveByte }, 0, perspectiveBytes.Length);
                }

                //照明の書き込み
                byte[] lightFrameCount = BitConverter.GetBytes(0);
                binaryWriter.Write(lightFrameCount, 0, intByteLength);

                //セルフシャドウの書き込み
                byte[] selfShadowCount = BitConverter.GetBytes(0);
                binaryWriter.Write(selfShadowCount, 0, intByteLength);

                //IKの書き込み
                byte[] ikCount = BitConverter.GetBytes(0);
                binaryWriter.Write(ikCount, 0, intByteLength);
            }
            catch (Exception ex)
            {
                Debug.Log("FOV VMD書き込みエラー" + ex.Message);
            }
            finally
            {
                binaryWriter.Close();
            }
        }
    }
    /// <summary>
    /// VMDを作成する
    /// 呼び出す際は先にStopRecordingを呼び出すこと
    /// </summary>
    /// <param name="filePath">保存先の絶対ファイルパス</param>
    /// <param name="keyReductionLevel">キーの書き込み頻度を減らして容量を減らす</param>
    public void SaveVMD(string filePath, int keyReductionLevel)
    {
        KeyReductionLevel = keyReductionLevel;
        SaveVMD();
    }
}

public class LiveCameraFrame
{
    public int frameIndex;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 CameraRotation;
    public float Fov;
    public bool FovVaild;
    public bool isOrthographic;

    public LiveCameraFrame(int frameIndex, Transform transform, float fov, LiveCameraFrame lastframe = null, bool isOrthographic = false)
    {
        this.frameIndex = frameIndex;

        var fixedPosition = transform.position;
        var vmdPosition = new Vector3(-fixedPosition.x, fixedPosition.y, -fixedPosition.z);
        Position = vmdPosition;

        var fixedQuatenion = transform.rotation;
        var vmdQuatenion = new Quaternion(-fixedQuatenion.x, fixedQuatenion.y, -fixedQuatenion.z, fixedQuatenion.w);
        Rotation = vmdQuatenion;

        Vector3 vmdRotation = new Vector3(fixedQuatenion.eulerAngles.x, fixedQuatenion.eulerAngles.y - 180, fixedQuatenion.eulerAngles.z);

        Rotation = Quaternion.Euler(vmdRotation);

        var finalRotation = vmdRotation;
        if (lastframe == null)
        {
            finalRotation = vmdRotation;
        }
        else
        {
            finalRotation = lastframe.CameraRotation + UnityCameraVMDRecorder.DeltaVector(vmdRotation, lastframe.CameraRotation);
        }

        CameraRotation = finalRotation;

        Fov = fov;
        this.isOrthographic = isOrthographic;
    }
}