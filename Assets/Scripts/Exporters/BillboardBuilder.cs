using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardBuilder : MonoBehaviour
{
    public static Mesh CreateSphereMesh(float radius, float uvRadius, Vector2 uvCenter, int longitude = 6, int latitude = 6)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(longitude + 1) * (latitude + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[longitude * latitude * 6];

        int vert = 0, tri = 0;
        for (int lat = 0; lat <= latitude; lat++)
        {
            float a1 = Mathf.PI * lat / latitude;
            float y = Mathf.Cos(a1);
            float sin1 = Mathf.Sin(a1);

            for (int lon = 0; lon <= longitude; lon++)
            {
                float a2 = 2 * Mathf.PI * lon / longitude;
                float x = sin1 * Mathf.Cos(a2);
                float z = sin1 * Mathf.Sin(a2);

                vertices[vert] = new Vector3(x, y, z) * radius;
                //uvs[vert] = new Vector2((float)lon / longitude, (float)lat / latitude);

                Vector3 p = vertices[vert].normalized;
                uvs[vert] = new Vector2(p.x, p.z) * uvRadius + uvCenter;

                //uvs[vert] = new Vector2(p.x, p.z) * 0.5f + new Vector2(0.5f, 0.5f);

                if (lat < latitude && lon < longitude)
                {
                    int current = vert;
                    int next = vert + longitude + 1;

                    triangles[tri++] = current;
                    triangles[tri++] = current + 1;
                    triangles[tri++] = next;

                    triangles[tri++] = current + 1;
                    triangles[tri++] = next + 1;
                    triangles[tri++] = next;
                }

                vert++;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    public static void BuildBillboard(UmaContainerCharacter container)
    {
        int groupSize = 4;

        foreach (SkinnedMeshRenderer s in container.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (s.name.Equals("M_Hair_Billboard") || s.name.Equals("M_Body_Billboard"))
            {
                Mesh sourceMesh = s.sharedMesh;
                Vector3[] vertices = sourceMesh.vertices;
                Vector2[] uv1s = sourceMesh.uv;
                Vector2[] uv2s = sourceMesh.uv2;

                BoneWeight[] boneWeights = sourceMesh.boneWeights;
                Matrix4x4[] bindposes = sourceMesh.bindposes;

                if (uv2s.Length != vertices.Length)
                {
                    Debug.LogError("Unmatch vertices number.");
                    return;
                }

                // List
                List<Vector3> finalVertices = new List<Vector3>();
                List<int> finalTriangles = new List<int>();
                List<Vector3> finalNormals = new List<Vector3>();
                List<Vector2> finalUVs = new List<Vector2>();
                List<BoneWeight> finalBoneWeights = new List<BoneWeight>();

                int sphereCount = 0;

                for (int i = 0; i < vertices.Length; i += groupSize)
                {
                    Vector3 center = vertices[i];
                    float radius = Mathf.Abs(uv2s[i].x);

                    
                    if (radius <= 0.0001f)
                        continue;

                    //Calc
                    float min_x = 1;
                    float max_x = 0;
                    float min_y = 1;
                    float max_y = 0;
                    for (int j = 0; j < 4; j++)
                    {
                        if (uv1s[i + j].x > max_x) max_x = uv1s[i + j].x;
                        if (uv1s[i + j].x < min_x) min_x = uv1s[i + j].x;
                        if (uv1s[i + j].y > max_y) max_y = uv1s[i + j].y;
                        if (uv1s[i + j].y < min_y) min_y = uv1s[i + j].y;
                    }

                    float center_x = (min_x + max_x) / 2;
                    float center_y = (min_y + max_y) / 2;
                    Vector2 uv_center = new Vector2(center_x, center_y);
                    float uv_radius = (max_x - min_x) / 2 * 0.25f;


                    // Create
                    Mesh sphereMesh;

                    if (radius < 0.003)
                    {
                        sphereMesh = CreateSphereMesh(radius, uv_radius, uv_center);
                    }
                    else
                    {
                        sphereMesh = CreateSphereMesh(radius, uv_radius, uv_center, 12, 9);
                    }


                    int vertexOffset = finalVertices.Count;

                    foreach (Vector3 v in sphereMesh.vertices)
                        finalVertices.Add(v + center);

                    finalTriangles.AddRange(System.Array.ConvertAll(sphereMesh.triangles, t => t + vertexOffset));
                    finalNormals.AddRange(sphereMesh.normals);
                    finalUVs.AddRange(sphereMesh.uv);

                    BoneWeight bw = boneWeights[i];
                    for (int j = 0; j < sphereMesh.vertexCount; j++)
                        finalBoneWeights.Add(bw);

                    sphereCount++;
                }

                // Merge
                Mesh finalMesh = new Mesh();
                finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                finalMesh.SetVertices(finalVertices);
                finalMesh.SetTriangles(finalTriangles, 0);
                finalMesh.SetNormals(finalNormals);
                finalMesh.SetUVs(0, finalUVs);
                finalMesh.boneWeights = finalBoneWeights.ToArray();
                finalMesh.bindposes = bindposes;

                // Create
                GameObject resultGO = new GameObject("BoundSpheres");
                resultGO.transform.SetParent(s.transform, false);
                SkinnedMeshRenderer newSMR = resultGO.AddComponent<SkinnedMeshRenderer>();
                newSMR.sharedMesh = finalMesh;
                newSMR.bones = s.bones;
                newSMR.rootBone = s.rootBone;
                newSMR.material = s.sharedMaterial;

                Debug.Log($"Generated {sphereCount} small spheres and combined them into a single mesh.");
            }
        }
    }

    public static void RemoveBillboard(UmaContainerCharacter container)
    {
        foreach (SkinnedMeshRenderer s in container.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (s.name.Equals("BoundSpheres"))
            {
                GameObject.Destroy(s.gameObject);
            }
        }
    }
}
