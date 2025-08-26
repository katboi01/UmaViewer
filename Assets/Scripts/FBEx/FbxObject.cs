
namespace FBEx
{
    public class FbxObject
    {
        public int ID;
        public int Version;
        public string Name;
        public string PropertyTemplate;

        public static string Indent(int indent)
        {
            var str = "";
            for (int i = 0; i < indent; i++)
            {
                str += "\t";
            }
            return str;
        }

        public virtual string Get() { return "Not implemented"; }
    }
}