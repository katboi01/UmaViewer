using Cute;
using Stage;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LyricsController : MonoBehaviour
{
    private enum LyricsSize
    {
        SS,
        S,
        M,
        L,
        LL
    }

    private enum LyricsCSV
    {
        Time = 0,
        Lyrics = 1,
        Size = 2,
        MinimumSize = 2
    }

    private struct LyricsData
    {
        public string lyrics;

        public float time;

        public byte size;
    }

    private const string LYRICS_PATH = "MusicScores/m{0:d3}/m{0:d3}_lyrics";

    private static readonly string[] sLyricsSizeNames = Enum.GetNames(typeof(LyricsSize));

    private static readonly byte[] sLyricsSizes = new byte[5]
    {
        16,
        24,
        32,
        48,
        64
    };

    [SerializeField]
    private GameObject[] _lyricsObjects;

    private Text[] _lyricsLabels;

    private LyricsData[] _LyricsDataes;

    private int _dispNo;

    //private Camera _UICamera;

    private void Awake()
    {
        if (_lyricsObjects != null && _lyricsObjects.Length != 0)
        {
            //_UICamera = SingletonMonoBehaviour<TempData>.instance.liveTemp.GetUICamera();
            _lyricsLabels = new Text[_lyricsObjects.Length];
            for (int i = 0; i < _lyricsObjects.Length; i++)
            {
                _lyricsLabels[i] = _lyricsObjects[i].GetComponent<Text>();
                _lyricsLabels[i].text = string.Empty;
                //_lyricsLabels[i].multiLine = true;
                /*
                if (_UICamera != null)
                {
                    UIAnchor component = _lyricsLabels[i].GetComponent<UIAnchor>();
                    component.uiCamera = _UICamera;
                    if (QualityManager.GetAssetQualityLevel() == QualityManager.AssetQualityLevel.Level_1)
                    {
                        component.pixelOffset.y *= 0.5f;
                    }
                }
                */
            }
            _dispNo = -1;
        }
    }

    public bool LoadLyrics(int id, bool isServerResouce = true)
    {
        ResourcesManager instance = SingletonMonoBehaviour<ResourcesManager>.instance;
        if (instance == null)
        {
            return false;
        }
        /*
        string assetName = string.Format("MusicScores/m{0:d3}/m{0:d3}_lyrics", id, id);
        string text = instance.LoadGenericCSV(assetName);
        if (text == null)
        {
            return false;
        }
        ArrayList arrayList = null;
        arrayList = Utility.ConvertCSV(text);
        */
        //MusicDirectorから読み込む
        ArrayList arrayList = ViewLauncher.instance.liveDirector.MusicScoreLyricsArray;
        if (arrayList == null)
        {
            return false;
        }
        _LyricsDataes = new LyricsData[arrayList.Count];
        for (int i = 0; i < arrayList.Count; i++)
        {
            ArrayList arrayList2 = arrayList[i] as ArrayList;
            string[] array = arrayList2.ToArray(typeof(string)) as string[];
            if (array.Length >= 2)
            {
                _LyricsDataes[i] = default(LyricsData);
                _LyricsDataes[i].time = float.Parse(array[0]);
                _LyricsDataes[i].lyrics = array[1].Replace("\\n", "\n");
                _LyricsDataes[i].size = sLyricsSizes[2];
                if (array.Length >= 2)
                {
                    for (int j = 0; j < sLyricsSizeNames.Length; j++)
                    {
                        string assetName = array[2].ToUpper();
                        if (sLyricsSizeNames[j] == assetName)
                        {
                            _LyricsDataes[i].size = sLyricsSizes[j];
                            break;
                        }
                    }
                }
            }
        }
        return true;
    }

    public void UpdateTime(float timer)
    {
        if (_LyricsDataes != null)
        {
            int i;
            for (i = 0; i < _LyricsDataes.Length && (!(timer >= _LyricsDataes[i].time) || (i + 1 < _LyricsDataes.Length && !(timer < _LyricsDataes[i + 1].time))); i++)
            {
            }
            if (i >= _LyricsDataes.Length)
            {
                i = _LyricsDataes.Length - 1;
            }
            if (_dispNo != i)
            {
                _lyricsLabels[0].text = _LyricsDataes[i].lyrics;
                _lyricsLabels[0].fontSize = _LyricsDataes[i].size;
                _dispNo = i;
            }
        }
    }
}
