using System.Collections;

/// <summary>
/// Liveで使用するアセットファイルを取りまとめるクラス
/// キャラクタ関係のアセットはCharaDirectorにて取りまとめる
/// </summary>
public interface LiveDirector
{
    ArrayList MusicScoreCyalumeArray
    {
        get;
    }

    ArrayList MusicScoreLyricsArray
    {
        get;
    }

    float musicLength
    {
        get;
    }

    int MusicID
    {
        get;
    }

    int LiveID
    {
        get;
    }

    int StageID
    {
        get;
    }

    int CyalumeType
    {
        get;
    }

    Master3dLive.Live3dData live3DData
    {
        get;
    }

    string musicScores
    {
        get;
    }

    string cyalumeFile
    {
        get;
    }

    string cuttFile
    {
        get;
    }

    string cuttName
    {
        get;
    }

    string stageFile
    {
        get;
    }

    string stageName
    {
        get;
    }

    string cameraFile
    {
        get;
    }

    string cameraName
    {
        get;
    }

    string musicFile
    {
        get;
    }

    string autoLipFile
    {
        get;
    }

    string autoLipName
    {
        get;
    }

    int stageMemberNumber
    {
        get;
    }

    string[] charaMotionFiles
    {
        get;
    }

    SQMusicData sqMusicData
    {
        get;
    }

    string[] uvMovieFiles
    {
        get;
    }

    string[] uvMovieNames
    {
        get;
    }

    string[] imgResourcesNames
    {
        get;
    }

    string[] mirrorScanFiles
    {
        get;
    }

    string[] mirrorScanMaterialNames
    {
        get;
    }

    bool isHQ
    {
        get;
    }

    bool isAnother
    {
        get;
    }

    /// <summary>
    /// 縦モード
    /// </summary>
    bool isVertical
    {
        get;
    }

    string[] GetAssetFiles();

    void LoadMusicScoreCyalume();

    void LoadMusicScoreLyrics();

    void LoadMusicScore(int level = 1);

    void SetIsSmartMode(bool isSmart);

    void SetAnotherMode(bool isAnother);

    void SetSoloCharaID(int charaID);
}