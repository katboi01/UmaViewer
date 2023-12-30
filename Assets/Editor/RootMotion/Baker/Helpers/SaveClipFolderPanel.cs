using UnityEditor;

namespace RootMotion
{
    public class SaveClipFolderPanel : EditorWindow
    {
        public static string Apply(string currentPath)
        {
            string path = EditorUtility.SaveFolderPanel("Save clip(s) to folder", currentPath, "");

            if (path.Length != 0)
            {
                return path.Substring(path.IndexOf("Assets/"));
            }

            return currentPath;
        }
    }
}