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

    static public string[] ExportAllTexture(string path, GameObject gameobj)
    {
        string savePath = path + "/Texture2D/";
        List<string> textureNames = new List<string>();
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        foreach (Renderer renderer in gameobj.transform.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in renderer.sharedMaterials)
            {
                //»ñµÃ²ÄÖÊµÄËùÓÐÊôÐÔÃû£¬Èç¹û¸ÄÊôÐÔÃû¶ÔÓ¦µÄÊÇÌùÍ¼£¬ÌáÈ¡¸ÃÌùÍ¼
                Shader mat_shader = mat.shader;
                int p_num = mat_shader.GetPropertyCount();
                for (int i = 0; i < p_num; i++)
                {
                    string p_name = mat_shader.GetPropertyName(i);
                    if (mat_shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
                    {
                        Texture2D mat_texture = (Texture2D)mat.GetTexture(p_name);

                        if (mat_texture)
                        {
                            string tex_name = mat_texture.name;
                            string tex_path = savePath + tex_name + ".png";
                            var dpath = "Texture2D/" + tex_name + ".png";
                            if (!textureNames.Contains(dpath))
                            {
                                textureNames.Add(dpath);
                            }
                            if (!File.Exists(tex_path))
                            {   

                                mat_texture = duplicateTexture(mat_texture);

                                if (tex_name.EndsWith("eye0"))
                                {
                                    var clip_texture = duplicateTexture(mat_texture);
                                    var pixels = clip_texture.GetPixels(0, 0, clip_texture.width / 4, clip_texture.height);
                                    clip_texture.Reinitialize(clip_texture.width / 4, clip_texture.height);
                                    clip_texture.SetPixels(pixels);


                                    byte[] clip = clip_texture.EncodeToPNG();
                                    File.WriteAllBytes(tex_path, clip);
                                    tex_path = savePath + tex_name + "_all" + ".png";
                                }

                                byte[] bytes = mat_texture.EncodeToPNG();
                                File.WriteAllBytes(tex_path, bytes);
                            }
                        }
                    }
                }
            }
        }
        Debug.Log("ÌáÈ¡³É¹¦");
        return textureNames.ToArray();
    }
}
