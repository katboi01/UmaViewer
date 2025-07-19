using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using Gallop;

//初期ポーズ(T,Aポーズ)の時点でアタッチ、有効化されている必要がある
public class UnityHumanoidVMDRecorder : MonoBehaviour
{
    public const string FileSavePath = "/../VMDRecords";
    public bool UseParentOfAll = true;
    public bool UseCenterAsParentOfAll = true;
    /// <summary>
    /// 全ての親の座標・回転を絶対座標系で計算する
    /// UseParentOfAllがTrueでないと意味がない
    /// </summary>
    public bool UseAbsoluteCoordinateSystem = false;
    public bool IgnoreInitialPosition = false;
    public bool IgnoreInitialRotation = false;
    /// <summary>
    /// 一部のモデルではMMD上ではセンターが足元にある
    /// Start前に設定されている必要がある
    /// </summary>
    public bool UseBottomCenter = false;
    /// <summary>
    /// Unity上のモーフ名に1.まばたきなど番号が振られている場合、番号を除去する
    /// </summary>
    public bool TrimMorphNumber = false;
    public int KeyReductionLevel = 2;
    public bool IsRecording { get; private set; } = false;
    public int FrameNumber { get; private set; } = 0;
    int frameNumberSaved = 0;
    const float FPSs = 0.03333f;
    const string CenterNameString = "センター";
    const string GrooveNameString = "グルーブ";

    public enum BoneNames
    {
        全ての親, センター, 左足ＩＫ, 右足ＩＫ, 上半身, 上半身2, 首, 頭,
        左肩, 左腕, 左ひじ, 左手首, 右肩, 右腕, 右ひじ, 右手首,
        左親指１, 左親指２, 左人指１, 左人指２, 左人指３, 左中指１, 左中指２, 左中指３,
        左薬指１, 左薬指２, 左薬指３, 左小指１, 左小指２, 左小指３, 右親指１, 右親指２,
        右人指１, 右人指２, 右人指３, 右中指１, 右中指２, 右中指３, 右薬指１, 右薬指２,
        右薬指３, 右小指１, 右小指２, 右小指３, 左足, 右足, 左ひざ, 右ひざ,
        左足首, 右足首, 左足先EX, 右足先EX, None
    }
    //コンストラクタにて初期化
    //全てのボーンを名前で引く辞書
    Dictionary<string, Transform> transformDictionary = new Dictionary<string, Transform>();
    public Dictionary<BoneNames, Transform> BoneDictionary { get; private set; }
    Vector3 parentInitialPosition = Vector3.zero;
    Quaternion parentInitialRotation = Quaternion.identity;
    Dictionary<BoneNames, List<Vector3>> positionDictionary = new Dictionary<BoneNames, List<Vector3>>();
    Dictionary<BoneNames, List<Vector3>> positionDictionarySaved = new Dictionary<BoneNames, List<Vector3>>();
    Dictionary<BoneNames, List<Quaternion>> rotationDictionary = new Dictionary<BoneNames, List<Quaternion>>();
    Dictionary<BoneNames, List<Quaternion>> rotationDictionarySaved = new Dictionary<BoneNames, List<Quaternion>>();
    Dictionary<int, bool> visitableDictionary = new Dictionary<int, bool>();
    //ボーン移動量の補正係数
    //この値は大体の値、正確ではない
    const float DefaultBoneAmplifier = 12.5f;

    public Vector3 ParentOfAllOffset = new Vector3(0, 0, 0);
    public Vector3 LeftFootIKOffset = Vector3.zero;
    public Vector3 RightFootIKOffset = Vector3.zero;

    BoneGhost boneGhost;
    public MorphRecorder morphRecorder;
    public MorphRecorder morphRecorderSaved;

    private UmaContainer container;
    float aposeDegress = 38.5f;

    public bool IsLive;
    public void Initialize()
    {
        Time.fixedDeltaTime = FPSs;
        container = GetComponentInParent<UmaContainer>();
        List<Transform> objs = GetComponentsInChildren<Transform>().ToList();
        BoneDictionary = new Dictionary<BoneNames, Transform>()
            {
                //下半身などというものはUnityにはない
                { BoneNames.全ての親, transform},
                { BoneNames.センター, objs.Find(a=>a.name.Equals("Hip"))},
                { BoneNames.上半身,   objs.Find(a=>a.name.Equals("Spine"))},
                { BoneNames.上半身2,  objs.Find(a=>a.name.Equals("Chest"))},
                { BoneNames.頭,       objs.Find(a=>a.name.Equals("Head"))},
                { BoneNames.首,       objs.Find(a=>a.name.Equals("Neck"))},
                { BoneNames.左肩,     objs.Find(a=>a.name.Equals("Shoulder_L"))},
                { BoneNames.右肩,     objs.Find(a=>a.name.Equals("Shoulder_R"))},
                { BoneNames.左腕,     objs.Find(a=>a.name.Equals("Arm_L"))},
                { BoneNames.右腕,     objs.Find(a=>a.name.Equals("Arm_R"))},
                { BoneNames.左ひじ,   objs.Find(a=>a.name.Equals("Elbow_L"))},
                { BoneNames.右ひじ,   objs.Find(a=>a.name.Equals("Elbow_R"))},
                { BoneNames.左手首,   objs.Find(a=>a.name.Equals("Wrist_L"))},
                { BoneNames.右手首,   objs.Find(a=>a.name.Equals("Wrist_R"))},
                { BoneNames.左親指１, objs.Find(a=>a.name.Equals("Thumb_02_L"))},
                { BoneNames.右親指１, objs.Find(a=>a.name.Equals("Thumb_02_R"))},
                { BoneNames.左親指２, objs.Find(a=>a.name.Equals("Thumb_03_L"))},
                { BoneNames.右親指２, objs.Find(a=>a.name.Equals("Thumb_03_R"))},
                { BoneNames.左人指１, objs.Find(a=>a.name.Equals("Index_01_L"))},
                { BoneNames.右人指１, objs.Find(a=>a.name.Equals("Index_01_R"))},
                { BoneNames.左人指２, objs.Find(a=>a.name.Equals("Index_02_L"))},
                { BoneNames.右人指２, objs.Find(a=>a.name.Equals("Index_02_R"))},
                { BoneNames.左人指３, objs.Find(a=>a.name.Equals("Index_03_L"))},
                { BoneNames.右人指３, objs.Find(a=>a.name.Equals("Index_03_R"))},
                { BoneNames.左中指１, objs.Find(a=>a.name.Equals("Middle_01_L"))},
                { BoneNames.右中指１, objs.Find(a=>a.name.Equals("Middle_01_R"))},
                { BoneNames.左中指２, objs.Find(a=>a.name.Equals("Middle_02_L"))},
                { BoneNames.右中指２, objs.Find(a=>a.name.Equals("Middle_02_R"))},
                { BoneNames.左中指３, objs.Find(a=>a.name.Equals("Middle_03_L"))},
                { BoneNames.右中指３, objs.Find(a=>a.name.Equals("Middle_03_R"))},
                { BoneNames.左薬指１, objs.Find(a=>a.name.Equals("Ring_01_L"))},
                { BoneNames.右薬指１, objs.Find(a=>a.name.Equals("Ring_01_R"))},
                { BoneNames.左薬指２, objs.Find(a=>a.name.Equals("Ring_02_L"))},
                { BoneNames.右薬指２, objs.Find(a=>a.name.Equals("Ring_02_R"))},
                { BoneNames.左薬指３, objs.Find(a=>a.name.Equals("Ring_03_L"))},
                { BoneNames.右薬指３, objs.Find(a=>a.name.Equals("Ring_03_R"))},
                { BoneNames.左小指１, objs.Find(a=>a.name.Equals("Pinky_01_L"))},
                { BoneNames.右小指１, objs.Find(a=>a.name.Equals("Pinky_01_R"))},
                { BoneNames.左小指２, objs.Find(a=>a.name.Equals("Pinky_02_L"))},
                { BoneNames.右小指２, objs.Find(a=>a.name.Equals("Pinky_02_R"))},
                { BoneNames.左小指３, objs.Find(a=>a.name.Equals("Pinky_03_L"))},
                { BoneNames.右小指３, objs.Find(a=>a.name.Equals("Pinky_03_R"))},
                { BoneNames.左足ＩＫ, objs.Find(a=>a.name.Equals("Ankle_L"))},
                { BoneNames.右足ＩＫ, objs.Find(a=>a.name.Equals("Ankle_R"))},
                { BoneNames.左足,     objs.Find(a=>a.name.Equals("Thigh_L"))},
                { BoneNames.右足,     objs.Find(a=>a.name.Equals("Thigh_R"))},
                { BoneNames.左ひざ,   objs.Find(a=>a.name.Equals("Knee_L"))},
                { BoneNames.右ひざ,   objs.Find(a=>a.name.Equals("Knee_R"))},
                { BoneNames.左足首,   objs.Find(a=>a.name.Equals("Ankle_L"))},
                { BoneNames.右足首,   objs.Find(a=>a.name.Equals("Ankle_R"))},
                { BoneNames.左足先EX, objs.Find(a=>a.name.Equals("Toe_L"))},
                { BoneNames.右足先EX, objs.Find(a=>a.name.Equals("Toe_R"))}
        };

        foreach (KeyValuePair<BoneNames, Transform> pair in BoneDictionary)
        {
            transformDictionary.Add(pair.Key.ToString(), pair.Value);
        }

        var characterContainer = GetComponentInParent<UmaContainerCharacter>();
        var animator = characterContainer.UmaAnimator;
        var state = animator.GetCurrentAnimatorStateInfo(0);
        animator.enabled = false;

        // Set to T-Pose
        characterContainer.ResetBodyPose();
        characterContainer.UpBodyReset();

        BoneDictionary[BoneNames.左腕].Rotate(0, 0, -aposeDegress);
        BoneDictionary[BoneNames.右腕].Rotate(0, 0, aposeDegress);

        SetInitialPositionAndRotation();

        foreach (BoneNames boneName in BoneDictionary.Keys)
        {
            if (BoneDictionary[boneName] == null) { continue; }

            positionDictionary.Add(boneName, new List<Vector3>());
            rotationDictionary.Add(boneName, new List<Quaternion>());
        }

        if (BoneDictionary[BoneNames.左足ＩＫ] != null)
        {
            LeftFootIKOffset = Quaternion.Inverse(transform.rotation) * (BoneDictionary[BoneNames.左足ＩＫ].position - transform.position);
        }

        if (BoneDictionary[BoneNames.右足ＩＫ] != null)
        {
            RightFootIKOffset = Quaternion.Inverse(transform.rotation) * (BoneDictionary[BoneNames.右足ＩＫ].position - transform.position);
        }

        boneGhost = new BoneGhost(BoneDictionary, UseBottomCenter);
        morphRecorder = new MorphRecorder(transform);

        BoneDictionary[BoneNames.左腕].Rotate(0, 0, aposeDegress);
        BoneDictionary[BoneNames.右腕].Rotate(0, 0, -aposeDegress);
        animator.enabled = true;
        animator.Play(state.shortNameHash, 0, state.normalizedTime);
    }

    private void FixedUpdate()
    {
        if (IsRecording && !IsLive)
        {
            SaveFrame();
            FrameNumber++;
        }
    }

    bool lastvisable;
    void SaveFrame()
    {
        if (boneGhost != null) { boneGhost.GhostAll(); }
        if (morphRecorder != null) { morphRecorder.RecrodAllMorph(); }

        bool visable = container.LiveVisible;
        if (visitableDictionary.Count == 0)
        {
            lastvisable = visable;
            visitableDictionary.Add(0, visable);
        }
        else if(visable != lastvisable)
        {
            lastvisable = visable;
            visitableDictionary.Add(FrameNumber, visable);
        }

        foreach (BoneNames boneName in BoneDictionary.Keys)
        {
            if (BoneDictionary[boneName] == null)
            {
                continue;
            }

            if (boneName == BoneNames.右足ＩＫ || boneName == BoneNames.左足ＩＫ)
            {
                Vector3 targetVector = Vector3.zero;
                if (UseCenterAsParentOfAll)
                {
                    if ((!UseAbsoluteCoordinateSystem && transform.parent != null) && IgnoreInitialPosition)
                    {
                        targetVector
                            = Quaternion.Inverse(transform.parent.rotation)
                            * (BoneDictionary[boneName].position - transform.parent.position)
                            - parentInitialPosition;
                    }
                    else if ((!UseAbsoluteCoordinateSystem && transform.parent != null) && !IgnoreInitialPosition)
                    {
                        targetVector
                            = Quaternion.Inverse(transform.parent.rotation)
                            * (BoneDictionary[boneName].position - transform.parent.position);
                    }
                    else if ((UseAbsoluteCoordinateSystem || transform.parent == null) && IgnoreInitialPosition)
                    {
                        targetVector = BoneDictionary[boneName].position - parentInitialPosition;
                    }
                    else if ((UseAbsoluteCoordinateSystem || transform.parent == null) && transform.parent && !IgnoreInitialPosition)
                    {
                        targetVector = BoneDictionary[boneName].position;
                    }
                }
                else
                {
                    targetVector = BoneDictionary[boneName].position - transform.position;
                    targetVector = Quaternion.Inverse(transform.rotation) * targetVector;
                }
                targetVector -= (boneName == BoneNames.左足ＩＫ ? LeftFootIKOffset : RightFootIKOffset);
                Vector3 ikPosition = new Vector3(-targetVector.x, targetVector.y, -targetVector.z);
                positionDictionary[boneName].Add(ikPosition * DefaultBoneAmplifier);
                //回転は全部足首に持たせる
                Quaternion ikRotation = Quaternion.identity;
                rotationDictionary[boneName].Add(ikRotation);
                continue;
            }

            if (boneGhost != null && boneGhost.GhostDictionary.Keys.Contains(boneName))
            {
                if (boneGhost.GhostDictionary[boneName].ghost == null || !boneGhost.GhostDictionary[boneName].enabled)
                {
                    rotationDictionary[boneName].Add(Quaternion.identity);
                    positionDictionary[boneName].Add(Vector3.zero);
                    continue;
                }

                Vector3 boneVector = boneGhost.GhostDictionary[boneName].ghost.localPosition;
                Quaternion boneQuatenion = boneGhost.GhostDictionary[boneName].ghost.localRotation;
                rotationDictionary[boneName].Add(new Quaternion(-boneQuatenion.x, boneQuatenion.y, -boneQuatenion.z, boneQuatenion.w));

                boneVector -= boneGhost.GhostOriginalLocalPositionDictionary[boneName];

                positionDictionary[boneName].Add(new Vector3(-boneVector.x, boneVector.y, -boneVector.z) * DefaultBoneAmplifier);
                continue;
            }

            Quaternion fixedQuatenion = Quaternion.identity;
            Quaternion vmdRotation = Quaternion.identity;

            if (boneName == BoneNames.全ての親 && UseAbsoluteCoordinateSystem)
            {
                fixedQuatenion = BoneDictionary[boneName].rotation;
            }
            else
            {
                fixedQuatenion = BoneDictionary[boneName].localRotation;
            }

            if (boneName == BoneNames.全ての親 && IgnoreInitialRotation)
            {
                fixedQuatenion = BoneDictionary[boneName].localRotation.MinusRotation(parentInitialRotation);
            }

            vmdRotation = new Quaternion(-fixedQuatenion.x, fixedQuatenion.y, -fixedQuatenion.z, fixedQuatenion.w);

            rotationDictionary[boneName].Add(vmdRotation);

            Vector3 fixedPosition = Vector3.zero;
            Vector3 vmdPosition = Vector3.zero;

            if (boneName == BoneNames.全ての親 && UseAbsoluteCoordinateSystem)
            {
                fixedPosition = BoneDictionary[boneName].position;
            }
            else
            {
                fixedPosition = BoneDictionary[boneName].localPosition;
            }

            if (boneName == BoneNames.全ての親 && IgnoreInitialPosition)
            {
                fixedPosition -= parentInitialPosition;
            }

            vmdPosition = new Vector3(-fixedPosition.x, fixedPosition.y, -fixedPosition.z);

            if (boneName == BoneNames.全ての親)
            {
                positionDictionary[boneName].Add(vmdPosition * DefaultBoneAmplifier + ParentOfAllOffset);
            }
            else
            {
                positionDictionary[boneName].Add(vmdPosition * DefaultBoneAmplifier);
            }
        }
    }

    void LiveSaveFrame()
    {
        if (IsRecording && IsLive)
        {
            SaveFrame();
            FrameNumber++;
        }
    }

    void SetInitialPositionAndRotation()
    {
        if (UseAbsoluteCoordinateSystem)
        {
            parentInitialPosition = transform.position;
            parentInitialRotation = transform.rotation;
        }
        else
        {
            parentInitialPosition = transform.localPosition;
            parentInitialRotation = transform.localRotation;
        }
    }

    public static void SetFPS(int fps)
    {
        Time.fixedDeltaTime = 1 / (float)fps;
    }

    /// <summary>
    /// レコーディングを開始または再開
    /// </summary>
    public void StartRecording(bool islive = false)
    {
        SetInitialPositionAndRotation();
        IsRecording = true;
        IsLive = islive;

        if (islive)
        {
            var director = Gallop.Live.Director.instance;
            director._liveTimelineControl.RecordUma += LiveSaveFrame;
        }
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
        morphRecorderSaved = morphRecorder;
        FrameNumber = 0;
        positionDictionarySaved = positionDictionary;
        positionDictionary = new Dictionary<BoneNames, List<Vector3>>();
        rotationDictionarySaved = rotationDictionary;
        rotationDictionary = new Dictionary<BoneNames, List<Quaternion>>();
        foreach (BoneNames boneName in BoneDictionary.Keys)
        {
            if (BoneDictionary[boneName] == null) { continue; }

            positionDictionary.Add(boneName, new List<Vector3>());
            rotationDictionary.Add(boneName, new List<Quaternion>());
        }
        morphRecorder = new MorphRecorder(transform);
        
        if (IsLive)
        {
            var director = Gallop.Live.Director.instance;
            director._liveTimelineControl.RecordUma -= LiveSaveFrame;
        }
    }

    /// <summary>
    /// VMDを作成する
    /// 呼び出す際は先にStopRecordingを呼び出すこと
    /// </summary>
    /// <param name="modelName">VMDファイルに記載される専用モデル名</param>
    /// <param name="filePath">保存先の絶対ファイルパス</param>
    public void SaveVMD(string modelName, string filePath)
    {
        if (IsRecording)
        {
            Debug.Log(transform.name + "VMD保存前にレコーディングをストップしてください。");
            return;
        }

        if (KeyReductionLevel <= 0) { KeyReductionLevel = 1; }

        Debug.Log(transform.name + "VMDファイル作成開始");
        //ファイルの書き込み
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
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

                //モデル名の書き込み、Shift_JISで保存
                const int modelNameLength = 20;
                byte[] modelNameBytes = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes(modelName);
                //モデル名が長すぎたとき
                modelNameBytes = modelNameBytes.Take(Mathf.Min(modelNameLength, modelNameBytes.Length)).ToArray();
                binaryWriter.Write(modelNameBytes, 0, modelNameBytes.Length);
                binaryWriter.Write(new byte[modelNameLength - modelNameBytes.Length], 0, modelNameLength - modelNameBytes.Length);

                //全ボーンフレーム数の書き込み
                void LoopWithBoneCondition(Action<BoneNames, int> action)
                {
                    for (int i = 0; i < frameNumberSaved; i++)
                    {
                        foreach (BoneNames boneName in Enum.GetValues(typeof(BoneNames)))
                        {
                            if ((i % KeyReductionLevel) != 0 && boneName != BoneNames.全ての親) { continue; }
                            if (!BoneDictionary.Keys.Contains(boneName)) { continue; }
                            if (BoneDictionary[boneName] == null) { continue; }
                            if (!UseParentOfAll && boneName == BoneNames.全ての親) { continue; }

                            action(boneName, i);
                        }
                    }
                }
                uint allKeyFrameNumber = 0;
                LoopWithBoneCondition((a, b) => { allKeyFrameNumber++; });
                byte[] allKeyFrameNumberByte = BitConverter.GetBytes(allKeyFrameNumber);
                binaryWriter.Write(allKeyFrameNumberByte, 0, intByteLength);

                //人ボーンの書き込み
                LoopWithBoneCondition((boneName, i) =>
                {
                    const int boneNameLength = 15;
                    string boneNameString = boneName.ToString();
                    if (boneName == BoneNames.全ての親 && UseCenterAsParentOfAll)
                    {
                        boneNameString = CenterNameString;
                    }
                    if (boneName == BoneNames.センター && UseCenterAsParentOfAll)
                    {
                        boneNameString = GrooveNameString;
                    }

                    byte[] boneNameBytes = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes(boneNameString);
                    binaryWriter.Write(boneNameBytes, 0, boneNameBytes.Length);
                    binaryWriter.Write(new byte[boneNameLength - boneNameBytes.Length], 0, boneNameLength - boneNameBytes.Length);

                    byte[] frameNumberByte = BitConverter.GetBytes((ulong)i);
                    binaryWriter.Write(frameNumberByte, 0, intByteLength);

                    Vector3 position = positionDictionarySaved[boneName][i];
                    byte[] positionX = BitConverter.GetBytes(position.x);
                    binaryWriter.Write(positionX, 0, intByteLength);
                    byte[] positionY = BitConverter.GetBytes(position.y);
                    binaryWriter.Write(positionY, 0, intByteLength);
                    byte[] positionZ = BitConverter.GetBytes(position.z);
                    binaryWriter.Write(positionZ, 0, intByteLength);
                    Quaternion rotation = rotationDictionarySaved[boneName][i];
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
                });

                //全モーフフレーム数の書き込み
                morphRecorderSaved.DisableIntron();
                if (TrimMorphNumber) { morphRecorderSaved.TrimMorphNumber(); }
                void LoopWithMorphCondition(Action<string, int> action)
                {
                    for (int i = 0; i < frameNumberSaved; i++)
                    {
                        foreach (string morphName in morphRecorderSaved.MorphDrivers.Keys)
                        {
                            if (morphRecorderSaved.MorphDrivers[morphName].ValueList.Count == 0) { continue; }
                            if (i > morphRecorderSaved.MorphDrivers[morphName].ValueList.Count) { continue; }
                            //変化のない部分は省く
                            if (!morphRecorderSaved.MorphDrivers[morphName].ValueList[i].enabled) { continue; }
                            const int boneNameLength = 15;
                            string morphNameString = morphName.ToString();
                            byte[] morphNameBytes = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes(morphNameString);
                            //名前が長過ぎた場合書き込まない
                            if (boneNameLength - morphNameBytes.Length < 0) { continue; }

                            action(morphName, i);
                        }
                    }
                }
                uint allMorphNumber = 0;
                LoopWithMorphCondition((a, b) => { allMorphNumber++; });
                byte[] faceFrameCount = BitConverter.GetBytes(allMorphNumber);
                binaryWriter.Write(faceFrameCount, 0, intByteLength);

                //モーフの書き込み
                LoopWithMorphCondition((morphName, i) =>
                {
                    const int boneNameLength = 15;
                    string morphNameString = morphName.ToString();
                    byte[] morphNameBytes = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes(morphNameString);

                    binaryWriter.Write(morphNameBytes, 0, morphNameBytes.Length);
                    binaryWriter.Write(new byte[boneNameLength - morphNameBytes.Length], 0, boneNameLength - morphNameBytes.Length);

                    byte[] frameNumberByte = BitConverter.GetBytes((ulong)i);
                    binaryWriter.Write(frameNumberByte, 0, intByteLength);

                    byte[] valueByte = BitConverter.GetBytes(morphRecorderSaved.MorphDrivers[morphName].ValueList[i].value);
                    binaryWriter.Write(valueByte, 0, intByteLength);
                });

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
                //0フレームにキーフレーム一つだけ置く
                byte[] ikCount = BitConverter.GetBytes(visitableDictionary.Count);
                binaryWriter.Write(ikCount, 0, intByteLength);

                foreach(var visable in visitableDictionary)
                {
                    byte[] ikFrameNumber = BitConverter.GetBytes(visable.Key);
                    byte modelDisplay = Convert.ToByte(visable.Value ? 1 : 0);
                    binaryWriter.Write(ikFrameNumber, 0, intByteLength);
                    binaryWriter.Write(modelDisplay);

                    //右足IKと左足IKと右足つま先IKと左足つま先IKの4つ
                    byte[] ikNumber = BitConverter.GetBytes(4);
                    const int IKNameLength = 20;
                    byte[] leftIKName = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes("左足ＩＫ");
                    byte[] rightIKName = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes("右足ＩＫ");
                    byte[] leftToeIKName = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes("左つま先ＩＫ");
                    byte[] rightToeIKName = System.Text.Encoding.GetEncoding(ShiftJIS).GetBytes("右つま先ＩＫ");
                    byte ikOn = Convert.ToByte(1);
                    byte ikOff = Convert.ToByte(0);
                        
                    binaryWriter.Write(ikNumber, 0, intByteLength);
                    binaryWriter.Write(leftIKName, 0, leftIKName.Length);
                    binaryWriter.Write(new byte[IKNameLength - leftIKName.Length], 0, IKNameLength - leftIKName.Length);
                    binaryWriter.Write(ikOff);
                    binaryWriter.Write(leftToeIKName, 0, leftToeIKName.Length);
                    binaryWriter.Write(new byte[IKNameLength - leftToeIKName.Length], 0, IKNameLength - leftToeIKName.Length);
                    binaryWriter.Write(ikOff);
                    binaryWriter.Write(rightIKName, 0, rightIKName.Length);
                    binaryWriter.Write(new byte[IKNameLength - rightIKName.Length], 0, IKNameLength - rightIKName.Length);
                    binaryWriter.Write(ikOff);
                    binaryWriter.Write(rightToeIKName, 0, rightToeIKName.Length);
                    binaryWriter.Write(new byte[IKNameLength - rightToeIKName.Length], 0, IKNameLength - rightToeIKName.Length);
                    binaryWriter.Write(ikOff);
                }
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
        if (boneGhost != null)
        {
            foreach(var pair in boneGhost.GhostDictionary)
            {
                Destroy(pair.Value.ghost.gameObject);
            }
        }
        Destroy(this);
    }

    /// <summary>
    /// VMDを作成する
    /// 呼び出す際は先にStopRecordingを呼び出すこと
    /// </summary>
    /// <param name="modelName">VMDファイルに記載される専用モデル名</param>
    /// <param name="filePath">保存先の絶対ファイルパス</param>
    /// <param name="keyReductionLevel">キーの書き込み頻度を減らして容量を減らす</param>
    public void SaveVMD(string modelName, int keyReductionLevel = 3)
    {
        string fileName = $"{Application.dataPath}{FileSavePath}/{string.Format("UMA_{0}.vmd", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"))}";
        Directory.CreateDirectory(Application.dataPath + FileSavePath);
        KeyReductionLevel = keyReductionLevel;
        SaveVMD(modelName, fileName);
    }

    public void SaveLiveVMD(LiveEntry liveEntry, DateTime time ,string modelName, int keyReductionLevel = 3)
    {
        string fileName = $"{Application.dataPath}{FileSavePath}/Live{liveEntry.MusicId}_{time.ToString("yyyy-MM-dd_HH-mm-ss")}/{modelName}.vmd";
        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
        KeyReductionLevel = keyReductionLevel;
        SaveVMD(modelName, fileName);
    }

    //裏で正規化されたモデル
    //(初期ポーズで各ボーンのlocalRotationがQuaternion.identityのモデル)を疑似的にアニメーションさせる
    class BoneGhost
    {
        public Dictionary<BoneNames, (Transform ghost, bool enabled)> GhostDictionary { get; private set; } = new Dictionary<BoneNames, (Transform ghost, bool enabled)>();
        public Dictionary<BoneNames, Vector3> GhostOriginalLocalPositionDictionary { get; private set; } = new Dictionary<BoneNames, Vector3>();
        public Dictionary<BoneNames, Quaternion> GhostOriginalRotationDictionary { get; private set; } = new Dictionary<BoneNames, Quaternion>();
        public Dictionary<BoneNames, Quaternion> OriginalRotationDictionary { get; private set; } = new Dictionary<BoneNames, Quaternion>();

        public bool UseBottomCenter { get; private set; } = false;

        const string GhostSalt = "Ghost";
        private Dictionary<BoneNames, Transform> boneDictionary = new Dictionary<BoneNames, Transform>();
        float centerOffsetLength = 0;

        public BoneGhost(Dictionary<BoneNames, Transform> boneDictionary, bool useBottomCenter)
        {
            this.boneDictionary = boneDictionary;
            UseBottomCenter = useBottomCenter;

            Dictionary<BoneNames, (BoneNames optionParent1, BoneNames optionParent2, BoneNames necessaryParent)> boneParentDictionary
                = new Dictionary<BoneNames, (BoneNames optionParent1, BoneNames optionParent2, BoneNames necessaryParent)>()
            {
                { BoneNames.センター, (BoneNames.None, BoneNames.None, BoneNames.全ての親) },
                { BoneNames.左足,     (BoneNames.None, BoneNames.None, BoneNames.センター) },
                { BoneNames.左ひざ,   (BoneNames.None, BoneNames.None, BoneNames.左足) },
                { BoneNames.左足首,   (BoneNames.None, BoneNames.None, BoneNames.左ひざ) },
                { BoneNames.左足先EX,   (BoneNames.None, BoneNames.None, BoneNames.左足首) },
                { BoneNames.右足,     (BoneNames.None, BoneNames.None, BoneNames.センター) },
                { BoneNames.右ひざ,   (BoneNames.None, BoneNames.None, BoneNames.右足) },
                { BoneNames.右足首,   (BoneNames.None, BoneNames.None, BoneNames.右ひざ) },
                { BoneNames.右足先EX,   (BoneNames.None, BoneNames.None, BoneNames.右足首) },
                { BoneNames.上半身,   (BoneNames.None, BoneNames.None, BoneNames.センター) },
                { BoneNames.上半身2,  (BoneNames.None, BoneNames.None, BoneNames.上半身) },
                { BoneNames.首,       (BoneNames.上半身2, BoneNames.None, BoneNames.上半身) },
                { BoneNames.頭,       (BoneNames.首, BoneNames.上半身2, BoneNames.上半身) },
                { BoneNames.左肩,     (BoneNames.上半身2, BoneNames.None, BoneNames.上半身) },
                { BoneNames.左腕,     (BoneNames.左肩, BoneNames.上半身2, BoneNames.上半身) },
                { BoneNames.左ひじ,   (BoneNames.None, BoneNames.None, BoneNames.左腕) },
                { BoneNames.左手首,   (BoneNames.None, BoneNames.None, BoneNames.左ひじ) },
                { BoneNames.左親指１, (BoneNames.左手首, BoneNames.None, BoneNames.None) },
                { BoneNames.左親指２, (BoneNames.左親指１, BoneNames.None, BoneNames.None) },
                { BoneNames.左人指１, (BoneNames.左手首, BoneNames.None, BoneNames.None) },
                { BoneNames.左人指２, (BoneNames.左人指１, BoneNames.None, BoneNames.None) },
                { BoneNames.左人指３, (BoneNames.左人指２, BoneNames.None, BoneNames.None) },
                { BoneNames.左中指１, (BoneNames.左手首, BoneNames.None, BoneNames.None) },
                { BoneNames.左中指２, (BoneNames.左中指１, BoneNames.None, BoneNames.None) },
                { BoneNames.左中指３, (BoneNames.左中指２, BoneNames.None, BoneNames.None) },
                { BoneNames.左薬指１, (BoneNames.左手首, BoneNames.None, BoneNames.None) },
                { BoneNames.左薬指２, (BoneNames.左薬指１, BoneNames.None, BoneNames.None) },
                { BoneNames.左薬指３, (BoneNames.左薬指２, BoneNames.None, BoneNames.None) },
                { BoneNames.左小指１, (BoneNames.左手首, BoneNames.None, BoneNames.None) },
                { BoneNames.左小指２, (BoneNames.左小指１, BoneNames.None, BoneNames.None) },
                { BoneNames.左小指３, (BoneNames.左小指２, BoneNames.None, BoneNames.None) },
                { BoneNames.右肩,     (BoneNames.上半身2, BoneNames.None, BoneNames.上半身) },
                { BoneNames.右腕,     (BoneNames.右肩, BoneNames.上半身2, BoneNames.上半身) },
                { BoneNames.右ひじ,   (BoneNames.None, BoneNames.None, BoneNames.右腕) },
                { BoneNames.右手首,   (BoneNames.None, BoneNames.None, BoneNames.右ひじ) },
                { BoneNames.右親指１, (BoneNames.右手首, BoneNames.None, BoneNames.None) },
                { BoneNames.右親指２, (BoneNames.右親指１, BoneNames.None, BoneNames.None) },
                { BoneNames.右人指１, (BoneNames.右手首, BoneNames.None, BoneNames.None) },
                { BoneNames.右人指２, (BoneNames.右人指１, BoneNames.None, BoneNames.None) },
                { BoneNames.右人指３, (BoneNames.右人指２, BoneNames.None, BoneNames.None) },
                { BoneNames.右中指１, (BoneNames.右手首, BoneNames.None, BoneNames.None) },
                { BoneNames.右中指２, (BoneNames.右中指１, BoneNames.None, BoneNames.None) },
                { BoneNames.右中指３, (BoneNames.右中指２, BoneNames.None, BoneNames.None) },
                { BoneNames.右薬指１, (BoneNames.右手首, BoneNames.None, BoneNames.None) },
                { BoneNames.右薬指２, (BoneNames.右薬指１, BoneNames.None, BoneNames.None) },
                { BoneNames.右薬指３, (BoneNames.右薬指２, BoneNames.None, BoneNames.None) },
                { BoneNames.右小指１, (BoneNames.右手首, BoneNames.None, BoneNames.None) },
                { BoneNames.右小指２, (BoneNames.右小指１, BoneNames.None, BoneNames.None) },
                { BoneNames.右小指３, (BoneNames.右小指２, BoneNames.None, BoneNames.None) },
            };

            //Ghostの生成
            foreach (BoneNames boneName in boneDictionary.Keys)
            {
                if (boneName == BoneNames.全ての親 || boneName == BoneNames.左足ＩＫ || boneName == BoneNames.右足ＩＫ)
                {
                    continue;
                }

                if (boneDictionary[boneName] == null)
                {
                    GhostDictionary.Add(boneName, (null, false));
                    continue;
                }

                Transform ghost = new GameObject(boneDictionary[boneName].name + GhostSalt).transform;
                if (boneName == BoneNames.センター && UseBottomCenter)
                {
                    ghost.position = boneDictionary[BoneNames.全ての親].position;
                }
                else
                {
                    ghost.position = boneDictionary[boneName].position;
                }
                GhostDictionary.Add(boneName, (ghost, true));
            }

            //Ghostの親子構造を設定
            foreach (BoneNames boneName in boneDictionary.Keys)
            {
                if (boneName == BoneNames.全ての親 || boneName == BoneNames.左足ＩＫ || boneName == BoneNames.右足ＩＫ)
                {
                    continue;
                }

                if (GhostDictionary[boneName].ghost == null || !GhostDictionary[boneName].enabled)
                {
                    continue;
                }

                if (boneName == BoneNames.センター)
                {
                    GhostDictionary[boneName].ghost.SetParent(boneDictionary[BoneNames.全ての親]);
                    continue;
                }

                if (boneParentDictionary[boneName].optionParent1 != BoneNames.None && boneDictionary[boneParentDictionary[boneName].optionParent1] != null)
                {
                    GhostDictionary[boneName].ghost.SetParent(GhostDictionary[boneParentDictionary[boneName].optionParent1].ghost);
                }
                else if (boneParentDictionary[boneName].optionParent2 != BoneNames.None && boneDictionary[boneParentDictionary[boneName].optionParent2] != null)
                {
                    GhostDictionary[boneName].ghost.SetParent(GhostDictionary[boneParentDictionary[boneName].optionParent2].ghost);
                }
                else if (boneParentDictionary[boneName].necessaryParent != BoneNames.None && boneDictionary[boneParentDictionary[boneName].necessaryParent] != null)
                {
                    GhostDictionary[boneName].ghost.SetParent(GhostDictionary[boneParentDictionary[boneName].necessaryParent].ghost);
                }
                else
                {
                    GhostDictionary[boneName] = (GhostDictionary[boneName].ghost, false);
                }
            }

            //初期状態を保存
            foreach (BoneNames boneName in GhostDictionary.Keys)
            {
                if (GhostDictionary[boneName].ghost == null || !GhostDictionary[boneName].enabled)
                {
                    GhostOriginalLocalPositionDictionary.Add(boneName, Vector3.zero);
                    GhostOriginalRotationDictionary.Add(boneName, Quaternion.identity);
                    OriginalRotationDictionary.Add(boneName, Quaternion.identity);
                }
                else
                {
                    GhostOriginalRotationDictionary.Add(boneName, GhostDictionary[boneName].ghost.rotation);
                    OriginalRotationDictionary.Add(boneName, boneDictionary[boneName].rotation);
                    if (boneName == BoneNames.センター && UseBottomCenter)
                    {
                        GhostOriginalLocalPositionDictionary.Add(boneName, Vector3.zero);
                        continue;
                    }
                    GhostOriginalLocalPositionDictionary.Add(boneName, GhostDictionary[boneName].ghost.localPosition);
                }
            }

            centerOffsetLength = Vector3.Distance(boneDictionary[BoneNames.全ての親].position, boneDictionary[BoneNames.センター].position);
        }

        public void GhostAll()
        {
            foreach (BoneNames boneName in GhostDictionary.Keys)
            {
                if (GhostDictionary[boneName].ghost == null || !GhostDictionary[boneName].enabled) { continue; }
                Quaternion transQuaternion = boneDictionary[boneName].rotation * Quaternion.Inverse(OriginalRotationDictionary[boneName]);
                GhostDictionary[boneName].ghost.rotation = transQuaternion * GhostOriginalRotationDictionary[boneName];
                if (boneName == BoneNames.センター && UseBottomCenter)
                {
                    GhostDictionary[boneName].ghost.position = boneDictionary[boneName].position - centerOffsetLength * GhostDictionary[boneName].ghost.up;
                    continue;
                }
                GhostDictionary[boneName].ghost.position = boneDictionary[boneName].position;
            }
        }
    }

    [Serializable]
    public class MorphRecorder
    {
        public List<FacialMorph> FacialMorphList;
        //キーはunity上のモーフ名
        public Dictionary<string, MorphDriver> MorphDrivers { get; private set; } = new Dictionary<string, MorphDriver>();

        public MorphRecorder(Transform model)
        {
            var facialTarget = model.GetComponentInParent<UmaContainerCharacter>().FaceDrivenKeyTarget;
            FacialMorphList = new List<FacialMorph>();
            FacialMorphList.AddRange(facialTarget.EyeBrowMorphs);
            FacialMorphList.AddRange(facialTarget.EyeMorphs);
            FacialMorphList.AddRange(facialTarget.MouthMorphs);
            for (int i = 0; i < FacialMorphList.Count; i++)
            {
                string morphName = ConvertMorphName(FacialMorphList[i].name);

                if (MorphDrivers.Keys.Contains(morphName))
                {
                    if (!MorphDrivers[morphName].Morphs.Contains(FacialMorphList[i]))
                    {
                        MorphDrivers[morphName].Morphs.Add(FacialMorphList[i]);
                    }
                }
                else
                {
                    List<FacialMorph> morphList = new List<FacialMorph>();
                    morphList.Add(FacialMorphList[i]);
                    var driver = new MorphDriver(morphList, i);
                    MorphDrivers.Add(morphName, driver);
                }
            }
        }


        public string ConvertMorphName(string name)
        {
            if (Config.Instance.VmdMorphConvertSetting.Count > 0)
            {
                var setting = Config.Instance.VmdMorphConvertSetting;
                foreach (var val in setting)
                {
                    foreach (var v in val.UMAMorph)
                    {
                        if(v.Equals(name))
                        {
                            return val.MMDMorph;
                        }
                    }
                }
            }
            return name;
        }

        public void RecrodAllMorph()
        {
            foreach (MorphDriver morphDriver in MorphDrivers.Values)
            {
                morphDriver.RecordMorph();
            }
        }

        public void TrimMorphNumber()
        {
            string dot = ".";
            Dictionary<string, MorphDriver> morphDriversTemp = new Dictionary<string, MorphDriver>();
            foreach (string morphName in MorphDrivers.Keys)
            {
                //正規表現使うより、dot探して整数か見る
                if (morphName.Contains(dot) && int.TryParse(morphName.Substring(0, morphName.IndexOf(dot)), out int dummy))
                {
                    morphDriversTemp.Add(morphName.Substring(morphName.IndexOf(dot) + 1), MorphDrivers[morphName]);
                    continue;
                }
                morphDriversTemp.Add(morphName, MorphDrivers[morphName]);
            }
            MorphDrivers = morphDriversTemp;
        }

        public void DisableIntron()
        {
            foreach (string morphName in MorphDrivers.Keys)
            {
                for (int i = 0; i < MorphDrivers[morphName].ValueList.Count; i++)
                {
                    //情報がなければ次へ
                    if (MorphDrivers[morphName].ValueList.Count == 0) { continue; }
                    //今、前、後が同じなら不必要なので無効化
                    if (i > 0
                        && i < MorphDrivers[morphName].ValueList.Count - 1
                        && floatCompare(MorphDrivers[morphName].ValueList[i].value, MorphDrivers[morphName].ValueList[i - 1].value)
                        && floatCompare(MorphDrivers[morphName].ValueList[i].value, MorphDrivers[morphName].ValueList[i + 1].value))
                    {
                        MorphDrivers[morphName].ValueList[i] = (MorphDrivers[morphName].ValueList[i].value, false);
                    }
                }
            }
        }

        bool floatCompare(float f1, float f2)
        {
            int a = (int)(f1 * 100);
            int b = (int)(f2 * 100);
            return a == b;
        }

        [Serializable]
        public class MorphDriver
        {
            public List<FacialMorph> Morphs;

            public int MorphIndex { get; private set; }

            public List<(float value, bool enabled)> ValueList = new List<(float value, bool enabled)>();

            public MorphDriver(List<FacialMorph> facialMorph, int morphIndex)
            {
                Morphs = facialMorph;
                MorphIndex = morphIndex;
            }

            public void RecordMorph()
            {
                float val = 0;
                foreach (var morph in Morphs)
                {
                    val += morph.weight;
                }
                ValueList.Add((Mathf.Clamp(val, -1, 1), true));
            }
        }
    }
}