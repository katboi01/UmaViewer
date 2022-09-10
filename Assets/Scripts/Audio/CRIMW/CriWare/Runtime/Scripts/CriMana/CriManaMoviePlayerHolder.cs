/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;

namespace CriWare {

public class CriManaMoviePlayerHolder : CriMonoBehaviour
{
    private CriMana.Player _player;
    public CriMana.Player player { set { _player = value; } }

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public override void CriInternalUpdate() { }

	public override void CriInternalLateUpdate() { }

    void Start()
    {
        if (_player == null)
        {
            Debug.LogAssertion("Error: No CriMana.Player held by CriManaMoviePlayerHolder.");
        }
    }
}

} //namespace CriWare
