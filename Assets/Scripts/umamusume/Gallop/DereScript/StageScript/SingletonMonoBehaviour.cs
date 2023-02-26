using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    protected bool _isRedy;

    public static T instance
    {
        get
        {
            if ((Object)_instance == (Object)null)
            {
                _instance = (T)Object.FindObjectOfType(typeof(T));
                _ = (Object)_instance == (Object)null;
            }
            return _instance;
        }
    }

    public bool isRedy => _isRedy;

    public static bool IsInstanceEmpty()
    {
        return (Object)_instance == (Object)null;
    }

    private void Awake()
    {
        if ((Object)_instance == (Object)null)
        {
            _instance = (T)Object.FindObjectOfType(typeof(T));
            _ = (Object)_instance == (Object)null;
        }
    }
}
