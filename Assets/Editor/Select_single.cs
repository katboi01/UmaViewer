using UnityEngine;
using UnityEditor;

class SelectAllOfTagSingle : ScriptableWizard
{
    [MenuItem("Example/Get")]
    static void SelectAllOfTagSingleWizard()
    {
        ScriptableWizard.DisplayWizard(
            "Example/Get",
            typeof(SelectAllOfTagSingle),
            "Make Selection");
    }

    void OnWizardCreate()
    {
        AssetDatabase.RemoveObjectFromAsset(Selection.objects[0]);
        AssetDatabase.CreateAsset(Selection.objects[0], "Assets/"+ Selection.objects[0].name + ".asset");
    }
}