using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideOnStart : MonoBehaviour
{
    // 在此列表中添加要在开始时隐藏的游戏对象
    [SerializeField] private List<GameObject> gameObjectsToHide;

    private void Start()
    {
        HideObjects();
    }

    private void HideObjects()
    {
        foreach (GameObject go in gameObjectsToHide)
        {
            if (go != null)
            {
                go.SetActive(false);
            }
        }
    }
}
