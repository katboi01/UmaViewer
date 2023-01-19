using UnityEngine;

/// <summary>
/// ビームライトを管理する
/// 星環世界にて使用
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class BgBeamLight : MonoBehaviour
{
    [SerializeField]
    private float cameraDistance = 15f;

    private void Start()
    {
        MeshFilter component = GetComponent<MeshFilter>();
        Mesh mesh = ConvertBeamLightMesh(component.sharedMesh);
        if ((bool)mesh)
        {
            component.sharedMesh = mesh;
        }
    }

    private Mesh ConvertBeamLightMesh(Mesh originalMesh)
    {
        if (!originalMesh.isReadable)
        {
            return null;
        }
        Vector3[] vertices = originalMesh.vertices;
        int num = vertices.Length;
        Vector4[] array = new Vector4[num];
        for (int i = 0; i < num; i++)
        {
            array[i] = new Vector4(0f - vertices[i].x, cameraDistance, 0f - vertices[i].z, 1f);
        }
        return new Mesh
        {
            vertices = vertices,
            triangles = originalMesh.triangles,
            uv = originalMesh.uv,
            uv2 = originalMesh.uv2,
            normals = originalMesh.normals,
            tangents = array
        };
    }
}
