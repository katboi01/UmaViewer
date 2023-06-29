using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityGLTF;


public class ModelExporter
{
    public static string RetrieveTexturePath(UnityEngine.Texture texture)
    {
        string m_Path = Application.dataPath;
        var path = m_Path + "/../unknown/" + texture.name; ;
        return path;
    }

    public static void Export(Transform[] transforms, string sceneName)
    {
        var exportOptions = new UnityGLTF.ExportOptions { TexturePathRetriever = RetrieveTexturePath };
        var exporter = new GLTFSceneExporter(transforms, exportOptions);

        string m_Path = Application.dataPath;

        string path = m_Path + "/../Exported/" + sceneName;

        var resultFile = GLTFSceneExporter.GetFileName(path, sceneName, ".gltf");

        exporter.SaveGLTFandBin(path, sceneName);

    }

    public static void exportModel(GameObject gameobj)
    {
        string sceneName = gameobj.name;
        Transform[] rootTransforms = new Transform[] { gameobj.transform };

        Export(rootTransforms, sceneName);
    }
}
