
namespace FBEx
{
    public class Connection
    {
        public FbxObject Object1;
        public FbxObject Object2;
        public string Mode;
        public string TextureMode;

        public Connection(FbxObject one, FbxObject two, string mode = "OO", string textureMode = "")
        {
            Mode = mode;
            TextureMode = textureMode;
            Object1 = one;
            Object2 = two;
        }

        public string Get()
        {
            if (Object2 == null)
            {
                return $"\t;{Object1.GetType().Name}::{Object1.Name}, Model::RootNode\n\tC: \"{Mode}\",{Object1.ID},0";
            }
            else
            {
                if (TextureMode != "")
                    return $"\t;{Object1.GetType().Name}::{Object1.Name}, {Object2.GetType().Name}::{Object2.Name}\n\tC: \"{Mode}\",{Object1.ID},{Object2.ID},\"{TextureMode}\"";
                else
                    return $"\t;{Object1.GetType().Name}::{Object1.Name}, {Object2.GetType().Name}::{Object2.Name}\n\tC: \"{Mode}\",{Object1.ID},{Object2.ID}";
            }
        }
    }
}