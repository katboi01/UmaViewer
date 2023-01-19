using UnityEngine;

namespace Cute
{
    public class AssetObject
    {
        public string basePath
        {
            get;
            private set;
        }

        public object baseObject
        {
            get;
            private set;
        }

        public AssetObject(string path, object obj)
        {
            basePath = path;
            baseObject = obj;
        }

        public void DestroyImmediate()
        {
            if (baseObject is Object)
            {
                Object.DestroyImmediate((Object)baseObject, true);
            }
            baseObject = null;
        }
    }
}
