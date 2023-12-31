using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json.Linq;



class SelectAllOfTag : ScriptableWizard
{
    //�˺����ɽ��޷���д��Tex��¡��һ�ݿɶ�д��Tex��������ȡͼƬ��png
    Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

    [MenuItem("Model/Get Select Model")]
    static void CreateWindow()
    {
        ScriptableWizard.DisplayWizard(
            "Get Select Model",
            typeof(SelectAllOfTag),
            "Start!");
    }

    void OnWizardCreate()
    {

        Debug.Log(Selection.objects[0]);

        GameObject gameobj = (GameObject)Selection.objects[0];

        if (!AssetDatabase.IsValidFolder("Assets/Model"))
        {
            AssetDatabase.CreateFolder("Assets", "Model");
        }

        string folder_name = "Assets/Model/" + gameobj.name;

        if (!AssetDatabase.IsValidFolder(folder_name))
        {
            AssetDatabase.CreateFolder("Assets/Model", gameobj.name);
        }

        foreach (SkinnedMeshRenderer skin_mesh in gameobj.transform.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            //��ȡ����Mesh���ļ���
            Mesh m = skin_mesh.sharedMesh;

            string m_parent = folder_name + "/Mesh";
            string m_path = folder_name + "/Mesh/" + m.name + ".mesh";
            if (!AssetDatabase.IsValidFolder(m_parent))
            {
                AssetDatabase.CreateFolder(folder_name, "Mesh");
            }

            Debug.Log(m.name);
            Debug.Log(AssetDatabase.IsValidFolder(m_parent));
            AssetDatabase.RemoveObjectFromAsset(m);
            AssetDatabase.CreateAsset(m, folder_name + "/Mesh/" + m.name + ".mesh");

            //����Texture2D�ļ���
            string tex_parent = folder_name + "/Texture2D";

            if (!AssetDatabase.IsValidFolder(tex_parent))
            {
                AssetDatabase.CreateFolder(folder_name, "Texture2D");
            }

            //����Material�ļ���
            string mat_parent = folder_name + "/Material";

            if (!AssetDatabase.IsValidFolder(mat_parent))
            {
                AssetDatabase.CreateFolder(folder_name, "Material");
            }

            foreach (Material mat in skin_mesh.sharedMaterials)
            {

                JArray p_name_list = new JArray();
                JArray tex_name_list = new JArray();

                //��ò��ʵ��������������������������Ӧ������ͼ����ȡ����ͼ
                Shader mat_shader = mat.shader;
                int p_num = mat_shader.GetPropertyCount();
                Debug.Log(p_num);
                for (int i = 0; i < p_num; i++)
                {
                    //Debug.Log(mat_shader.GetPropertyName(i));
                    //Debug.Log(mat_shader.GetPropertyType(i));
                    string p_name = mat_shader.GetPropertyName(i);
                    if (mat_shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
                    {
                        //Debug.Log("Hello!");
                        Debug.Log(p_name);
                        Texture2D mat_texture = (Texture2D)mat.GetTexture(p_name);
                        if (mat_texture)
                        {
                            //Debug.Log("There is a Name!");
                            string tex_name = mat_texture.name;
                            string tex_path = folder_name + "/Texture2D/" + tex_name + ".png";
                            if (!File.Exists(tex_path))
                            {
                                mat_texture = duplicateTexture(mat_texture);
                                byte[] bytes = mat_texture.EncodeToPNG();
                                File.WriteAllBytes(tex_path, bytes);
                            }
                            //����ͼƬ��������֮��Ĺ���
                            p_name_list.Add(new JValue(p_name));
                            tex_name_list.Add(new JValue(tex_path));
                        }
                    }
                }


                //��ȡ����
                AssetDatabase.RemoveObjectFromAsset(mat);
                AssetDatabase.CreateAsset(mat, folder_name + "/Material/" + mat.name + ".mat");

                //ˢ����Դ����������ȡ������ͼƬ����Assets
                AssetDatabase.Refresh();

                //��ͼƬ���ӽ�����
                for (int i = 0; i < p_name_list.Count; i++)
                {
                    Debug.Log(p_name_list[i]);
                    Debug.Log(tex_name_list[i]);
                    mat.SetTexture(p_name_list[i].ToString(), (Texture2D)AssetDatabase.LoadAssetAtPath(tex_name_list[i].ToString(), typeof(Texture2D)));
                };


            }
        }

        foreach (MeshFilter mf in gameobj.transform.GetComponentsInChildren<MeshFilter>())
        {
            //��ȡ����Mesh���ļ���
            Mesh m = mf.sharedMesh;

            string m_parent = folder_name + "/Mesh";
            string m_path = folder_name + "/Mesh/" + m.name + ".mesh";
            if (!AssetDatabase.IsValidFolder(m_parent))
            {
                AssetDatabase.CreateFolder(folder_name, "Mesh");
            }

            Debug.Log(m.name);
            Debug.Log(AssetDatabase.IsValidFolder(m_parent));
            AssetDatabase.RemoveObjectFromAsset(m);
            AssetDatabase.CreateAsset(m, folder_name + "/Mesh/" + m.name + ".mesh");
        }

        foreach (MeshRenderer mr in gameobj.transform.GetComponentsInChildren<MeshRenderer>())
        {
            //����Texture2D�ļ���
            string tex_parent = folder_name + "/Texture2D";

            if (!AssetDatabase.IsValidFolder(tex_parent))
            {
                AssetDatabase.CreateFolder(folder_name, "Texture2D");
            }

            //����Material�ļ���
            string mat_parent = folder_name + "/Material";

            if (!AssetDatabase.IsValidFolder(mat_parent))
            {
                AssetDatabase.CreateFolder(folder_name, "Material");
            }

            foreach (Material mat in mr.sharedMaterials)
            {

                JArray p_name_list = new JArray();
                JArray tex_name_list = new JArray();

                //��ò��ʵ��������������������������Ӧ������ͼ����ȡ����ͼ
                Shader mat_shader = mat.shader;
                int p_num = mat_shader.GetPropertyCount();
                Debug.Log(p_num);
                for (int i = 0; i < p_num; i++)
                {
                    //Debug.Log(mat_shader.GetPropertyName(i));
                    //Debug.Log(mat_shader.GetPropertyType(i));
                    string p_name = mat_shader.GetPropertyName(i);
                    if (mat_shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
                    {
                        //Debug.Log("Hello!");
                        Debug.Log(p_name);
                        Texture2D mat_texture = (Texture2D)mat.GetTexture(p_name);
                        if (mat_texture)
                        {
                            //Debug.Log("There is a Name!");
                            string tex_name = mat_texture.name;
                            string tex_path = folder_name + "/Texture2D/" + tex_name + ".png";
                            if (!File.Exists(tex_path))
                            {
                                mat_texture = duplicateTexture(mat_texture);
                                byte[] bytes = mat_texture.EncodeToPNG();
                                File.WriteAllBytes(tex_path, bytes);
                            }
                            //����ͼƬ��������֮��Ĺ���
                            p_name_list.Add(new JValue(p_name));
                            tex_name_list.Add(new JValue(tex_path));
                        }
                    }
                }


                //��ȡ����
                AssetDatabase.RemoveObjectFromAsset(mat);
                AssetDatabase.CreateAsset(mat, folder_name + "/Material/" + mat.name + ".mat");

                //ˢ����Դ����������ȡ������ͼƬ����Assets
                AssetDatabase.Refresh();

                //��ͼƬ���ӽ�����
                for (int i = 0; i < p_name_list.Count; i++)
                {
                    Debug.Log(p_name_list[i]);
                    Debug.Log(tex_name_list[i]);
                    mat.SetTexture(p_name_list[i].ToString(), (Texture2D)AssetDatabase.LoadAssetAtPath(tex_name_list[i].ToString(), typeof(Texture2D)));
                };
            }
        }

        foreach (Animator anim in gameobj.transform.GetComponentsInChildren<Animator>())
        {
            //��ȡ����Avatar���ļ���
            Avatar av = anim.avatar;

            string av_parent = folder_name + "/Avatar";
            string av_path = folder_name + "/Avatar/" + av.name + ".asset";
            if (!AssetDatabase.IsValidFolder(av_parent))
            {
                AssetDatabase.CreateFolder(folder_name, "Avatar");
            }
            
            Debug.Log(av.name);
            Debug.Log(AssetDatabase.IsValidFolder(av_parent));
            AssetDatabase.RemoveObjectFromAsset(av);
            
            AssetDatabase.CreateAsset(av, folder_name + "/Avatar/" + av.name + ".asset");
        }

        //��ȡԤ����
        PrefabUtility.SaveAsPrefabAsset(gameobj, folder_name + "/" + gameobj.name + ".prefab");
    }
}