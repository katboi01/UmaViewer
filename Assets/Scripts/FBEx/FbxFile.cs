using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace FBEx
{
    public class FbxFile
    {
        public FBXHeaderExtension FBXHeaderExtension;
        public GlobalSettings GlobalSettings = new GlobalSettings();
        public Definitions Definitions = new Definitions();
        public List<FbxObject> Objects = new List<FbxObject>();
        public List<Connection> Connections = new List<Connection>();

        public FbxFile()
        {
        }

        public void Write(string filePath)
        {
            FBXHeaderExtension = new FBXHeaderExtension();
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.ASCII))
            {
                writer.WriteLine(FBXHeaderExtension.Get());
                writer.WriteLine(GlobalSettings.Get());
                writer.WriteLine(Definitions.Get(Objects));
                writer.WriteLine("Objects:  {");
                foreach (var ob in Objects)
                {
                    writer.WriteLine(ob.Get());
                }
                writer.WriteLine("}");
                writer.WriteLine("Connections: {");
                foreach (var ob in Connections)
                {
                    writer.WriteLine(ob.Get());
                }
                writer.WriteLine("}");
            }

            Debug.Log("FBX Exported: " + "test.fbx");
        }
    }
}