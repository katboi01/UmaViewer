using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;

public class ScriptableObjectTest
{
    //Asset文件保存路径
    private const string assetPath = "Assets/Resources/Asset/";

    [MenuItem("MyTools/ScriptableObjectTest")]
    public static void CreateTestAsset()
    {
        //创建数据
        TestData testData = ScriptableObject.CreateInstance<TestData>();
        //赋值
        testData.testName = "name";
        testData.level = 1;

        //检查保存路径
        if (!Directory.Exists(assetPath))
            Directory.CreateDirectory(assetPath);

        //删除原有文件，生成新文件
        string fullPath = assetPath + "/" + "TestData.asset";
        UnityEditor.AssetDatabase.DeleteAsset(fullPath);
        UnityEditor.AssetDatabase.CreateAsset(testData, fullPath);
        UnityEditor.AssetDatabase.Refresh();
    }
}

//测试数据类
[CreateAssetMenu(fileName = "TestData", menuName = "Create ScriptableObject : TestData", order = 1)]
//类名与C#文件名一直
public class TestData : ScriptableObject
{
    public string testName;
    public int level;
}

#endif