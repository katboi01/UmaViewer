using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;

public class ScriptableObjectTest
{
    //Asset�ļ�����·��
    private const string assetPath = "Assets/Resources/Asset/";

    [MenuItem("MyTools/ScriptableObjectTest")]
    public static void CreateTestAsset()
    {
        //��������
        TestData testData = ScriptableObject.CreateInstance<TestData>();
        //��ֵ
        testData.testName = "name";
        testData.level = 1;

        //��鱣��·��
        if (!Directory.Exists(assetPath))
            Directory.CreateDirectory(assetPath);

        //ɾ��ԭ���ļ����������ļ�
        string fullPath = assetPath + "/" + "TestData.asset";
        UnityEditor.AssetDatabase.DeleteAsset(fullPath);
        UnityEditor.AssetDatabase.CreateAsset(testData, fullPath);
        UnityEditor.AssetDatabase.Refresh();
    }
}

//����������
[CreateAssetMenu(fileName = "TestData", menuName = "Create ScriptableObject : TestData", order = 1)]
//������C#�ļ���һֱ
public class TestData : ScriptableObject
{
    public string testName;
    public int level;
}

#endif