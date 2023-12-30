using UnityEngine;
using UnityEngine.Rendering;

public class TrailAfterImage : AfterImage
{
    private class ControllPoint
    {
        public Quaternion rotation;

        public Vector3 position;

        public float _time;
    }

    private ControllPoint[] _controllPoint;

    protected Mesh _mesh;

    public void Start()
    {
        if (Check())
        {
            _afterImageType = eShaderPass.TrailColoredTex;
            _controllPoint = new ControllPoint[_dimention];
            for (int i = 0; i < _dimention; i++)
            {
                _controllPoint[i] = new ControllPoint();
            }
            UpdateMesh();
            Create();
        }
    }

    public void LateUpdate()
    {
        UpdateControllPoint();
        UpdateMesh();
    }

    public void OnEnable()
    {
        UpdateMesh();
        Create();
        Attach(bSwitch: true);
        Reset();
    }

    public void OnDisable()
    {
        Attach(bSwitch: false);
    }

    private void UpdateControllPoint()
    {
        if (!(_targetTransform == null))
        {
            _lastIndex = _nowIndex;
            _controllPoint[_nowIndex].position = _targetTransform.position;
            _controllPoint[_nowIndex].rotation = _targetTransform.rotation;
            _nowIndex = (_nowIndex + 1) % _dimention;
            if (_cntAfterImage < _dimention)
            {
                _cntAfterImage++;
            }
        }
    }

    private void UpdateMesh()
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "Trail mesh";
            _mesh.MarkDynamic();
        }
        else
        {
            _mesh.Clear();
        }
        Vector3[] array = new Vector3[_dimention * 2];
        Vector3 vector = Vector3.zero;
        Vector3 vector2 = Vector3.zero;
        if (_targetTransform != null)
        {
            vector = _targetTransform.rotation * Vector3.up * _scale;
            vector += _targetTransform.position;
            vector2 = _targetTransform.rotation * Vector3.down * _scale;
            vector2 += _targetTransform.position;
        }
        for (int i = 0; i < _cntAfterImage; i++)
        {
            int num = (_lastIndex - i + _dimention) % _dimention;
            vector = _controllPoint[num].rotation * Vector3.up * _scale;
            vector += _controllPoint[num].position;
            array[i * 2] = vector;
            vector2 = _controllPoint[num].rotation * Vector3.down;
            vector2 += _controllPoint[num].position;
            array[i * 2 + 1] = vector2;
        }
        for (int j = _cntAfterImage; j < _dimention; j++)
        {
            array[j * 2] = vector;
            array[j * 2 + 1] = vector2;
        }
        _mesh.vertices = array;
        if (_afterImageType == eShaderPass.TrailShimmer)
        {
            Vector3[] array2 = new Vector3[_dimention * 2];
            for (int k = 0; k < _dimention - 1; k++)
            {
                Vector3 vector3 = array[(k + 1) * 2] - array[k * 2];
                vector3.Normalize();
                array2[k * 2] = vector3;
                Vector3 vector4 = array[(k + 1) * 2 + 1] - array[k * 2 + 1];
                vector4.Normalize();
                array2[k * 2 + 1] = vector4;
            }
            array2[(_dimention - 1) * 2] = array2[(_dimention - 2) * 2];
            array2[(_dimention - 1) * 2 + 1] = array2[(_dimention - 2) * 2 + 1];
            _mesh.normals = array2;
        }
        int[] array3 = new int[(_dimention - 1) * 6];
        for (int l = 0; l < _dimention - 1; l++)
        {
            array3[l * 6] = l * 2;
            array3[l * 6 + 1] = l * 2 + 1;
            array3[l * 6 + 2] = l * 2 + 2;
            array3[l * 6 + 3] = l * 2 + 1;
            array3[l * 6 + 4] = l * 2 + 3;
            array3[l * 6 + 5] = l * 2 + 2;
        }
        _mesh.SetIndices(array3, MeshTopology.Triangles, 0);
        Color[] array4 = new Color[_dimention * 2];
        for (int m = 0; m < _cntAfterImage; m++)
        {
            array4[m * 2 + 1] = (array4[m * 2] = Color.white * (_cntAfterImage - m) / _cntAfterImage);
        }
        for (int n = _cntAfterImage; n < _dimention; n++)
        {
            array4[n * 2] = Color.black;
            array4[n * 2 + 1] = Color.black;
        }
        _mesh.colors = array4;
        if (_afterImageType == eShaderPass.TrailColoredTex)
        {
            Vector2[] array5 = new Vector2[_dimention * 2];
            for (int num2 = 0; num2 < _cntAfterImage; num2++)
            {
                float x = (_cntAfterImage - num2) / (float)_cntAfterImage;
                array5[num2 * 2].x = x;
                array5[num2 * 2].y = 0f;
                array5[num2 * 2 + 1].x = x;
                array5[num2 * 2 + 1].y = 1f;
            }
            for (int num3 = _cntAfterImage; num3 < _dimention; num3++)
            {
                array5[num3 * 2].x = 1f;
                array5[num3 * 2].y = 0f;
                array5[num3 * 2 + 1].x = 1f;
                array5[num3 * 2 + 1].y = 1f;
            }
            _mesh.uv = array5;
        }
        _mesh.RecalculateBounds();
    }

    public override void Create()
    {
        if (_commandBuffer == null)
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            int num = Shader.PropertyToID("_ScreenCopyTexture");
            commandBuffer.GetTemporaryRT(num, -1, -1, 0, FilterMode.Bilinear);
            commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, num);
            commandBuffer.SetGlobalTexture("_ScreenCopyTexture", num);
            commandBuffer.DrawMesh(_mesh, Matrix4x4.identity, _targetMaterial, 0, (int)_afterImageType);
            _currentTargetCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBuffer);
            commandBuffer.ReleaseTemporaryRT(num);
            _commandBuffer = commandBuffer;
        }
    }

    public override void Destory()
    {
        Attach(bSwitch: false);
        if (_commandBuffer != null)
        {
            _commandBuffer.Clear();
            _commandBuffer.Release();
            _commandBuffer = null;
        }
    }
}
