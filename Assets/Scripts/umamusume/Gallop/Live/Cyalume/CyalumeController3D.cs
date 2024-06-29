using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gallop.Cyalume;

namespace Gallop.Live.Cyalume
{

    public class CyalumeController3D : CyalumeControllerBase
    {
        float passedtime = 0.0f;
        float intervaltime = 0.033333333f;
        float y_offset = 0.0f;
        GameObject CyalumeObj;
        Material CyalumeMaterial;
        private void Awake()
        {
            InitializeCyalume();
            gameObject.GetComponent<CyalumeController3D>().enabled = true;
            
        }

        private void FixedUpdate()
        {
            if (CyalumeMaterial == null)
                return;
            if (passedtime >= intervaltime)
            {
                y_offset += (float)0.03125;
                if (y_offset >= 1.0f)
                {
                    y_offset = 0.0f;
                }
                CyalumeMaterial.SetTextureOffset("_MainTex", new Vector2(0, y_offset));
                passedtime = 0;
            }
            passedtime += Time.fixedDeltaTime;
        }

        public void InitializeCyalume()
        {
            AssetHolder CyalumeAssetHolder = gameObject.GetComponent<AssetHolder>();
            List<StringObjectPair> CyalumeObject = CyalumeAssetHolder._assetTable.list;
            foreach (StringObjectPair obj in CyalumeObject)
            {
                GameObject tempObj = Instantiate((GameObject)obj.Value, transform);
                if (obj.Value.name.Contains("cyalume")){
                    CyalumeObj = tempObj;
                    CyalumeMaterial = gameObject.GetComponentInChildren<MeshRenderer>().material;
                }
            }
        }
    }
}

