using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardBuilder : MonoBehaviour
{
    


    //修改(Billboard的导出相关)
    public static Mesh CreateSphereMesh(float radius, int subdiv, float uvRadius, Vector2 uvCenter)
    {
        var t = (1f + Mathf.Sqrt(5f)) / 2f;
        var baseVerts = new List<Vector3>
        {
            new Vector3(-1,  t,  0), new Vector3( 1,  t,  0), new Vector3(-1, -t,  0), new Vector3( 1, -t,  0),
            new Vector3( 0, -1,  t), new Vector3( 0,  1,  t), new Vector3( 0, -1, -t), new Vector3( 0,  1, -t),
            new Vector3( t,  0, -1), new Vector3( t,  0,  1), new Vector3(-t,  0, -1), new Vector3(-t,  0,  1),
        };
        var faces = new List<int[]>
        {
            new[]{0,11,5}, new[]{0,5,1}, new[]{0,1,7}, new[]{0,7,10}, new[]{0,10,11},
            new[]{1,5,9}, new[]{5,11,4}, new[]{11,10,2}, new[]{10,7,6}, new[]{7,1,8},
            new[]{3,9,4}, new[]{3,4,2}, new[]{3,2,6}, new[]{3,6,8}, new[]{3,8,9},
            new[]{4,9,5}, new[]{2,4,11}, new[]{6,2,10}, new[]{8,6,7}, new[]{9,8,1}
        };
    
        for (int i = 0; i < baseVerts.Count; i++) baseVerts[i] = baseVerts[i].normalized;
    
        var vertList = new List<Vector3>(baseVerts);
        var triList  = new List<int>();
        foreach (var f in faces) { triList.Add(f[0]); triList.Add(f[1]); triList.Add(f[2]); }
    
        var midpointCache = new Dictionary<long, int>();
        int GetMidpoint(int a, int b)
        {
            long key = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
            if (midpointCache.TryGetValue(key, out var idx)) return idx;
            var m = (vertList[a] + vertList[b]).normalized;
            idx = vertList.Count;
            vertList.Add(m);
            midpointCache[key] = idx;
            return idx;
        }
    
        for (int s = 0; s < subdiv; s++)
        {
            var newTris = new List<int>(triList.Count * 4);
            for (int i = 0; i < triList.Count; i += 3)
            {
                int v0 = triList[i], v1 = triList[i+1], v2 = triList[i+2];
                int a = GetMidpoint(v0, v1);
                int b = GetMidpoint(v1, v2);
                int c = GetMidpoint(v2, v0);
                newTris.AddRange(new[]{ v0, a, c,  v1, b, a,  v2, c, b,  a, b, c });
            }
            triList = newTris;
        }
    
        Vector3[] vertices = new Vector3[vertList.Count];
        Vector3[] normals  = new Vector3[vertList.Count];
        Vector2[] uvs      = new Vector2[vertList.Count];
        for (int i = 0; i < vertList.Count; i++)
        {
            Vector3 p = vertList[i];
            vertices[i] = p * radius;
            normals[i]  = p;
            uvs[i]      = new Vector2(p.x, p.z) * uvRadius + uvCenter;
        }
    
        var mesh = new Mesh();
        if (vertices.Length > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices  = vertices;
        mesh.normals   = normals;
        mesh.uv        = uvs;
        mesh.triangles = triList.ToArray();
        mesh.RecalculateBounds();
    
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






                    //修改(Billboard的导出相关)
                    if (radius < 0.003)
                    {
                        sphereMesh = CreateSphereMesh(radius, 1, uv_radius, uv_center);
                    }
                    else
                    {
                        sphereMesh = CreateSphereMesh(radius, 2, uv_radius, uv_center);
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



                //修改(Billboard的导出相关)
                //ApplyBillboardUVOffset(s);



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






    //修改(Billboard的导出相关)
    private static void ApplyBillboardUVOffset(SkinnedMeshRenderer smr)
    {
        var mesh = smr.sharedMesh;
        if (mesh == null || mesh.uv2 == null || mesh.uv2.Length == 0) return;
    
        var newMesh = UnityEngine.Object.Instantiate(mesh);
        var vertices = newMesh.vertices;
        var uv2 = newMesh.uv2;
    
        Matrix4x4 worldToLocal = smr.transform.worldToLocalMatrix;
        Vector3 localXAxis = worldToLocal.MultiplyVector(Vector3.right).normalized;
        Vector3 localYAxis = worldToLocal.MultiplyVector(Vector3.up).normalized;
    
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector2 offset = uv2[i];
            vertices[i] += localXAxis * offset.x + localYAxis * offset.y;
        }
    
        newMesh.vertices = vertices;
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
    
        smr.sharedMesh = newMesh;
    }
}
