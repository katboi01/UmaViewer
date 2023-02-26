using UnityEngine;

/// <summary>
/// キャラクタ影を投影するオブジェクトに使用
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/CastShadow")]
public class CastShadowEffect : MonoBehaviour
{
    private const int Resolution = 256;

    private static readonly int ProjectedAmbientMatrixId = Shader.PropertyToID("_ProjectedAmbientMatrix");

    private GameObject[] _rendererObj;

    private int[] _backupLayer;

    private int _cullingLayer;

    private Camera _shadowCamera;

    private Shader _shadowShader;

    private RenderTexture _renderTexture;

    private MeshRenderer _targetMesh;

    public void Initialize(Shader shadowShader, Shader shadowReciveShader, CharacterObject characterObj, MeshRenderer revieveMesh, int cullingLayer)
    {
        _shadowShader = shadowShader;
        _targetMesh = revieveMesh;
        _targetMesh.material.shader = shadowReciveShader;
        Renderer[] componentsInChildren = characterObj.GetComponentsInChildren<Renderer>();
        _rendererObj = new GameObject[componentsInChildren.Length];
        _backupLayer = new int[componentsInChildren.Length];
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            _rendererObj[i] = componentsInChildren[i].gameObject;
        }
        _cullingLayer = cullingLayer;
    }

    private void Start()
    {
        _shadowCamera = GetComponent<Camera>();
        _shadowCamera.SetReplacementShader(_shadowShader, "Chara");
        if (Application.isPlaying)
        {
            _renderTexture = new RenderTexture(256, 256, 0);
            _renderTexture.autoGenerateMips = false;
            _renderTexture.useMipMap = false;
            _renderTexture.filterMode = FilterMode.Bilinear;
            _renderTexture.wrapMode = TextureWrapMode.Clamp;
            _renderTexture.antiAliasing = 1;
            _renderTexture.Create();
            _targetMesh.material.SetTexture("_ProjectionTex", _renderTexture);
            _shadowCamera.targetTexture = _renderTexture;
        }
    }

    private void OnDestroy()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            _renderTexture = null;
        }
    }

    private void OnPreCull()
    {
        int num = 0;
        GameObject[] rendererObj = _rendererObj;
        foreach (GameObject gameObject in rendererObj)
        {
            _backupLayer[num] = gameObject.layer;
            gameObject.layer = _cullingLayer;
            num++;
        }
    }

    private void OnPostRender()
    {
        int num = 0;
        GameObject[] rendererObj = _rendererObj;
        for (int i = 0; i < rendererObj.Length; i++)
        {
            rendererObj[i].layer = _backupLayer[num];
            num++;
        }
    }

    private void Update()
    {
        Matrix4x4 value = _shadowCamera.projectionMatrix * _shadowCamera.worldToCameraMatrix;
        _targetMesh.material.SetMatrix(ProjectedAmbientMatrixId, value);
    }
}
