using UnityEngine;
using UnityEditor;

class SelectAllOfTagHash : ScriptableWizard
{

    [MenuItem("Hash/Get Hash")]
    static void SelectAllOfTagHashWizard()
    {
        ScriptableWizard.DisplayWizard(
            "Hash/Get Hash",
            typeof(SelectAllOfTagHash),
            "Get Hash");
    }

    void OnWizardCreate()
    {
        //AssetDatabase.RemoveObjectFromAsset(Selection.objects[0]);
        //AssetDatabase.CreateAsset(Selection.objects[0], "Assets/"+ Selection.objects[0].name + ".asset");

        UnityEngine.GameObject obj = (GameObject)Selection.activeObject;
        if (obj == null)
        {
            Debug.LogError("You must select GameObj first!");
            return;
        }
        string result = "";

        var children = obj.GetComponentsInChildren<Transform>();

        foreach(var selectChild in children)
        {
            var tempChild = selectChild;

            if (tempChild != null && tempChild != obj.transform)
            {
                result = tempChild.name;
                while (tempChild.parent != obj.transform && tempChild.parent != null)
                {
                    tempChild = tempChild.parent;
                    result = string.Format("{0}/{1}", tempChild.name, result);
                }
            }


            Debug.Log(string.Format("GameObject:{0}, Hash:{1}", result, (uint)Animator.StringToHash(result)));
        }
        


    }
}