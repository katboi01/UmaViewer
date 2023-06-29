using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using System;

public class TextureExporter
{
    static public Texture2D duplicateTexture(Texture2D source)
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

    static public void exportAllTexture(GameObject gameobj)
    {
        string m_Path = Application.dataPath;

        string savePath = m_Path + "/../Exported/" + gameobj.name + "/Texture2D/";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        foreach (SkinnedMeshRenderer skin_mesh in gameobj.transform.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            foreach (Material mat in skin_mesh.sharedMaterials)
            {
                //获得材质的所有属性名，如果改属性名对应的是贴图，提取该贴图
                Shader mat_shader = mat.shader;
                int p_num = mat_shader.GetPropertyCount();
                Debug.Log(p_num);
                for (int i = 0; i < p_num; i++)
                {
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
                            string tex_path = savePath + tex_name + ".png";
                            if (!File.Exists(tex_path))
                            {
                                mat_texture = duplicateTexture(mat_texture);
                                byte[] bytes = mat_texture.EncodeToPNG();
                                File.WriteAllBytes(tex_path, bytes);
                            }
                        }
                    }
                }
            }
        }

        foreach (MeshRenderer mr in gameobj.transform.GetComponentsInChildren<MeshRenderer>())
        {
            foreach (Material mat in mr.sharedMaterials)
            {
                //获得材质的所有属性名，如果改属性名对应的是贴图，提取该贴图
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
                            string tex_path = savePath + tex_name + ".png";
                            if (!File.Exists(tex_path))
                            {
                                mat_texture = duplicateTexture(mat_texture);
                                byte[] bytes = mat_texture.EncodeToPNG();
                                File.WriteAllBytes(tex_path, bytes);
                            }
                        }
                    }
                }
            }
        }

        Debug.Log("提取成功");
    }
}
