using UnityEngine;

public class LyricsManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _lyricsObject;

    private LyricsController _lyricsController;

    private static bool checkResource()
    {
        //設定を確認して歌詞を表示するか切り替え
        int save = SaveManager.GetInt("Lyrics");
        if (save == 1)
        {
            return true;
        }
        return false;

    }

    public static LyricsManager getCompornent()
    {
        if (!checkResource())
        {
            return null;
        }
        return Object.FindObjectOfType<LyricsManager>();
    }

    public bool CreateLyricsObject(int id)
    {
        if (_lyricsObject == null)
        {
            return false;
        }
        if (!checkResource())
        {
            return false;
        }
        /* prefabから読むのではなく、Sceneにべた置きするため不要
        _lyricsObject = Object.Instantiate(_lyricsObject);
        _lyricsObject.transform.parent = base.gameObject.transform;
        */
        _lyricsController = _lyricsObject.GetComponent<LyricsController>();
        return _lyricsController.LoadLyrics(id);
    }

    public void UpdateTime(float time)
    {
        if (_lyricsController != null)
        {
            _lyricsController.UpdateTime(time);
        }
    }
}
