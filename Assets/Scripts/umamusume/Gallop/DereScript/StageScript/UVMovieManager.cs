using UnityEngine;

/// <summary>
/// UVMovieを一括して管理する
/// 必要なリソース名を取得し、コントローラ分生成する
/// </summary>
public class UVMovieManager : MonoBehaviour
{
    [HideInInspector]
    public string[] _MovieResouces;

    [HideInInspector]
    public UVMovieController[] _MovieControllers;

    [HideInInspector]
    private bool _updateTimeline;

    private int[] _charaIdArray;

    public bool updateTimeline
    {
        set
        {
            _updateTimeline = value;
        }
    }

    public int[] charaIdArray
    {
        set
        {
            _charaIdArray = value;
        }
    }

    private void Start()
    {
        if (_MovieResouces != null && _MovieResouces.Length != 0)
        {
            _MovieControllers = new UVMovieController[_MovieResouces.Length];
            for (int i = 0; i < _MovieControllers.Length; i++)
            {
                _MovieControllers[i] = new UVMovieController();
                _MovieControllers[i]._ResouceName = _MovieResouces[i];
                if (_charaIdArray != null && i < _charaIdArray.Length)
                {
                    _MovieControllers[i].charaId = _charaIdArray[i];
                }
                _MovieControllers[i].Start();
            }
        }
    }

    private void Update()
    {
        if (_MovieResouces == null || _MovieResouces.Length == 0 || _updateTimeline)
        {
            return;
        }
        for (int i = 0; i < _MovieControllers.Length; i++)
        {
            if (_MovieControllers[i] != null)
            {
                _MovieControllers[i].UpdateAddTime(Time.deltaTime, false);
            }
        }
    }
}
