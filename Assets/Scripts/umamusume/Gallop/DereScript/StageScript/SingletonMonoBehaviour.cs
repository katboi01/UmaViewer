using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    protected bool _isRedy;

    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)Object.FindObjectOfType(typeof(T));
                _ = _instance == null;
            }
            return _instance;
        }
    }

    public bool isRedy => _isRedy;

    public static bool IsInstanceEmpty()
    {
        return _instance == null;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)Object.FindObjectOfType(typeof(T));
            _ = _instance == null;
        }
    }
}
