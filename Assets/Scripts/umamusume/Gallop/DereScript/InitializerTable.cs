using UnityEngine;

[CreateAssetMenu(menuName = "MyGame/Create InitializerTable", fileName = "InitializerTable")]
public class InitializerTable : ScriptableObject
{
    private static readonly string RESOURCE_PATH = "InitializerTable";

    private static InitializerTable s_instance = null;
    public static InitializerTable Instance
    {
        get
        {
            if (s_instance == null)
            {
                var asset = Resources.Load(RESOURCE_PATH) as InitializerTable;
                if (asset == null)
                {
                    Debug.AssertFormat(false, "Missing ParameterTable! path={0}", RESOURCE_PATH);
                    asset = CreateInstance<InitializerTable>();
                }

                s_instance = asset;
            }

            return s_instance;
        }
    }

    [SerializeField]
    public GameObject AssetManager;
    [SerializeField]
    public GameObject ViewLauncher;
    [SerializeField]
    public GameObject UIManager;
    [SerializeField]
    public GameObject ResourcesManager;

}

