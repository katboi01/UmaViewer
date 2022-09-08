/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CriWare {

public abstract class CriMonoBehaviour : MonoBehaviour
{
    public Guid guid { get; private set; }

    public CriMonoBehaviour()
    {
        guid = Guid.NewGuid();
    }

    protected virtual void OnEnable()
    {
        CriMonoBehaviourManager.instance.Register(this);
    }

    protected virtual void OnDisable()
    {
        CriMonoBehaviourManager.UnRegister(this);
    }

    public abstract void CriInternalUpdate();

    public abstract void CriInternalLateUpdate();
}

public class CriMonoBehaviourManager : MonoBehaviour
{
    private static CriMonoBehaviourManager _instance = null;
    private static List<CriMonoBehaviour> criMonoBehaviourList = new List<CriMonoBehaviour>();

    public static CriMonoBehaviourManager instance
    {
        get {
            CreateInstance();
            return _instance;
        }
    }

    public static void CreateInstance()
    {
        if (_instance == null) {
            CriWare.Common.managerObject.AddComponent<CriMonoBehaviourManager>();
        }
    }

    private static int GetIndex(CriMonoBehaviour criMonoBehaviour)
    {
        for (int i = 0; i < criMonoBehaviourList.Count; i++) {
            if (criMonoBehaviourList[i].guid == criMonoBehaviour.guid) {
                return i;
            }
        }
        return -1;
    }

    public bool Register(CriMonoBehaviour criMonoBehaviour)
    {
        lock (criMonoBehaviourList) {
            if (GetIndex(criMonoBehaviour) >= 0) {
                UnityEngine.Debug.LogWarning("[CRIWARE] Internal: Duplicated CriMonoBehaviour GUID");
                return false;
            }
            criMonoBehaviourList.Add(criMonoBehaviour);
        }
        return true;
    }

    public static bool UnRegister(CriMonoBehaviour criMonoBehaviour)
    {
        lock (criMonoBehaviourList) {
            int index = GetIndex(criMonoBehaviour);
            if (index < 0) {
                return false;
            }
            criMonoBehaviourList.RemoveAt(index);
        }
        return true;
    }

    private void Awake()
    {
        if (_instance == null) {
            _instance = this;
        } else {
            Destroy(this);
        }
    }

    private void Update()
    {
        lock (criMonoBehaviourList) {
            for (int i = 0; i < criMonoBehaviourList.Count; i++) {
                criMonoBehaviourList[i].CriInternalUpdate();
            }
        }
    }

    private void LateUpdate()
    {
        lock (criMonoBehaviourList) {
            for (int i = 0; i < criMonoBehaviourList.Count; i++) {
                criMonoBehaviourList[i].CriInternalLateUpdate();
            }
        }
    }
}

} //namespace CriWare