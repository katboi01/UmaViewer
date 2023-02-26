using System.Collections.Generic;
using UnityEngine;

namespace Cute
{
    public class AssetBundleObject
    {
        private AssetBundle assetBundle;

        public List<AssetObject> objectArray
        {
            get;
            set;
        }

        public AssetBundleObject()
        {
            assetBundle = null;
            objectArray = new List<AssetObject>();
        }

        public void SetAssetBundle(AssetBundle assetBundle)
        {
            this.assetBundle = assetBundle;
        }

        public void Unload(bool unloadAllLoadedObjects)
        {
            if (assetBundle != null)
            {
                assetBundle.Unload(unloadAllLoadedObjects);
                assetBundle = null;
            }
        }

        public bool HasAssetBundle()
        {
            return assetBundle != null;
        }
    }
}
