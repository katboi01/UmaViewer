using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;

/// <summary>
/// 3d_live.unity3dの3d/Live/3d_live_dataを読み込んで生成する
/// 3d/Live/3d_live_dataはcsv形式のためArrayList形式で入力する
/// </summary>
public class Master3dLive
{
    public enum CsvColumn
    {
        ID,
        DebugMusicDataID,
        Cutt,
        Bg,
        DelayTime,
        AutoLip,
        Camera,
        CharaMotion,
        PostEffect,
        UvMovie00,
        UvMovie01,
        UvMovie02,
        UvMovie03,
        UvMovie04,
        UvMovie05,
        UvMovie06,
        UvMovie07,
        ImgResource00,
        ImgResource01,
        ImgResource02,
        ImgResource03,
        ImgResource04,
        ImgResource05,
        ImgResource06,
        ImgResource07,
        MirrorScanMat00,
        MirrorScanMat01,
        MirrorScanMat02,
        MirrorScanMat03,
        MirrorScanMat04,
        MirrorScanMat05,
        MirrorScanMat06,
        MirrorScanMat07,
        CharaMotionNum,
        OverrideMotionNum,
        CharaHeightMotionNum,
        MobCyalume3DResourceID,
        MobCyalume3DMotionID
    }

    public enum ExtensionCsvColumn
    {
        ID,
        DebugMusicDataID,
        Cutt,
        Bg,
        DelayTime,
        AutoLip,
        Camera,
        CharaMotion,
        PostEffect,
        UvMovie00,
        UvMovie01,
        UvMovie02,
        UvMovie03,
        UvMovie04,
        UvMovie05,
        UvMovie06,
        UvMovie07,
        UvMovie08,
        UvMovie09,
        UvMovie10,
        UvMovie11,
        UvMovie12,
        UvMovie13,
        UvMovie14,
        UvMovie15,
        ImgResource00,
        ImgResource01,
        ImgResource02,
        ImgResource03,
        ImgResource04,
        ImgResource05,
        ImgResource06,
        ImgResource07,
        MirrorScanMat00,
        MirrorScanMat01,
        MirrorScanMat02,
        MirrorScanMat03,
        MirrorScanMat04,
        MirrorScanMat05,
        MirrorScanMat06,
        MirrorScanMat07,
        CharaMotionNum,
        OverrideMotionNum,
        CharaHeightMotionNum,
        MobCyalume3DResourceID,
        MobCyalume3DMotionID
    }

    public class Live3dData
    {
        private int _id;

        private int _debug_music_data_id;

        private string _cutt;

        private int _bg;

        private float _delay;

        private string _autolip;

        private string _camera;

        private string _charaMotion;

        private string _postEffect;

        private string[] _uvMovies;

        private string[] _imgResources;

        private int[] _mirrorScanMatIDs;

        private string[] _mirrorScanMatNames;

        private int _charaMotionNum;

        private int _overrideMotionNum;

        private int _charaHeightMotionNum;

        private int _mobCyalume3DResourceID;

        private int _mobCyalume3DMotionID;

        public int id => _id;

        public int debug_music_data_id => _debug_music_data_id;

        public string cutt => _cutt;

        public int bg => _bg;

        public float delay => _delay;

        public string autolip => _autolip;

        public string camera => _camera;

        public string charaMotion => _charaMotion;

        public string postEffect => _postEffect;

        public string[] uvMovies => _uvMovies;

        public string[] imgResources => _imgResources;

        public int[] mirrorScanMatIDs => _mirrorScanMatIDs;

        public string[] mirrorScanMatNames => _mirrorScanMatNames;

        public int charaMotionNum => _charaMotionNum;

        public int overrideMotionNum => _overrideMotionNum;

        public int charaHeightMotionNum => _charaHeightMotionNum;

        public int mobCyalume3DResourceID => _mobCyalume3DResourceID;

        public int mobCyalume3DMotionID => _mobCyalume3DMotionID;

        public Live3dData(string[] columns)
        {
            int length_uvmovie = 8;
            int image_index = 17;
            int mirrorScanMat_index = 25;
            int charaMotionNum = 33;
            int overrideMotionNum = 34;
            int charaHeightMotionNum = 35;
            int mobCyalume3DResourceID = 36;
            int mobCyalume3DMotionID = 37;
            if (columns.Length >= EXTENSION_CSV_COLUMN_NUM)
            {
                length_uvmovie = 16;
                image_index = 25;
                mirrorScanMat_index = 33;
                charaMotionNum = 41;
                overrideMotionNum = 42;
                charaHeightMotionNum = 43;
                mobCyalume3DResourceID = 44;
                mobCyalume3DMotionID = 45;
            }
            int id = int.Parse(columns[0]);
            string cutt = columns[2];
            int bg = int.Parse(columns[3]);
            int debug_music_data_id = 0;
            string debug_music_data_id_string = columns[1];
            if (!string.IsNullOrEmpty(debug_music_data_id_string))
            {
                debug_music_data_id = int.Parse(debug_music_data_id_string);
            }
            float delay = float.Parse(columns[4]);
            string autolip = columns[5];
            string camera = columns[6];
            string charaMotion = "";
            if (7 < columns.Length)
            {
                charaMotion = columns[7];
            }
            List<string> uvMovies = new List<string>();
            List<string> imgResources = new List<string>();
            List<int> mirrorScanMatIDs = new List<int>();
            List<string> mirrorScanMatNames = new List<string>();
            string postEffect = "";
            if (columns[8].IndexOf("test", StringComparison.Ordinal) >= 0)
            {
                postEffect = columns[8];
                for (int i = 0; i < length_uvmovie; i++)
                {
                    int num13 = 9 + i;
                    if (num13 < columns.Length)
                    {
                        string text7 = columns[num13];
                        if (text7.Length > 0)
                        {
                            uvMovies.Add(text7);
                        }
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    int num13 = image_index + i;
                    if (num13 < columns.Length)
                    {
                        string text7 = columns[num13];
                        if (text7.Length > 0)
                        {
                            imgResources.Add(text7);
                        }
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    int num13 = mirrorScanMat_index + i;
                    if (num13 >= columns.Length)
                    {
                        continue;
                    }
                    string text7 = columns[num13];
                    if (text7.Length > 0)
                    {
                        int result = 0;
                        if (int.TryParse(text7, out result))
                        {
                            mirrorScanMatIDs.Add(result);
                        }
                        else
                        {
                            mirrorScanMatNames.Add(text7);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    int num13 = ADD_UV_MOVIE_NUM + i;
                    if (num13 < columns.Length)
                    {
                        string text7 = columns[num13];
                        if (text7.Length > 0)
                        {
                            uvMovies.Add(text7);
                        }
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    int num13 = image_index - 1 + i;
                    if (num13 < columns.Length)
                    {
                        string text7 = columns[num13];
                        if (text7.Length > 0)
                        {
                            imgResources.Add(text7);
                        }
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    int num13 = mirrorScanMat_index - 1 + i;
                    if (num13 >= columns.Length)
                    {
                        continue;
                    }
                    string text7 = columns[num13];
                    if (text7.Length > 0)
                    {
                        int result2 = 0;
                        if (int.TryParse(text7, out result2))
                        {
                            mirrorScanMatIDs.Add(result2);
                        }
                        else
                        {
                            mirrorScanMatNames.Add(text7);
                        }
                    }
                }
            }
            _id = id;
            _debug_music_data_id = debug_music_data_id;
            _cutt = cutt;
            _bg = bg;
            _delay = delay;
            _autolip = autolip;
            _camera = camera;
            _charaMotion = charaMotion;
            _postEffect = postEffect;
            _uvMovies = uvMovies.ToArray();
            _imgResources = imgResources.ToArray();
            _mirrorScanMatIDs = mirrorScanMatIDs.ToArray();
            _mirrorScanMatNames = mirrorScanMatNames.ToArray();
            int.TryParse(columns[charaMotionNum], out _charaMotionNum);
            int.TryParse(columns[overrideMotionNum], out _overrideMotionNum);
            int.TryParse(columns[charaHeightMotionNum], out _charaHeightMotionNum);
            if (columns.Length >= mobCyalume3DMotionID + 1)
            {
                int.TryParse(columns[mobCyalume3DResourceID], out _mobCyalume3DResourceID);
                int.TryParse(columns[mobCyalume3DMotionID], out _mobCyalume3DMotionID);
            }
        }
    }

    private Dictionary<int, Live3dData> _dictionary = new Dictionary<int, Live3dData>();

    private const int CSV_COLUMN_NUM = 36;

    private const int ADD_UV_MOVIE_NUM = 8;

    private const int EXTENSION_CSV_COLUMN_NUM = 44;

    public Dictionary<int, Live3dData> dictionary => _dictionary;

    private void Parse(ArrayList list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            string[] array = (string[])((ArrayList)list[i]).ToArray(typeof(string));
            int key = int.Parse(array[0]);
            _dictionary.Add(key, new Live3dData(array));
        }
    }

    public Master3dLive(ArrayList list)
    {
        Parse(list);
    }

    public Master3dLive(string csvPath)
    {
        ArrayList list = Utility.ConvertCSV((ResourcesManager.instance.LoadObject(csvPath) as TextAsset).ToString());
        Parse(list);
    }
}
