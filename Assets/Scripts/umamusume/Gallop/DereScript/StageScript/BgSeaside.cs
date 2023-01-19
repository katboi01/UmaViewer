using System;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 波打ち際の波モーション用
    /// Go Just Goで使用
    /// </summary>
    [ExecuteInEditMode]
    public class BgSeaside : MonoBehaviour
    {
        [Header("波全体スピード")]
        [SerializeField]
        [Range(0f, 5f)]
        public float _wavePatternSpeed = 1f;

        public void OnWillRenderObject()
        {
            Material sharedMaterial = GetComponent<Renderer>().sharedMaterial;
            float num = (float)((double)Time.timeSinceLevelLoad / 20.0) * _wavePatternSpeed;
            sharedMaterial.SetFloat("_GlobalPhase", num);
            float num2 = (0f - num) * 6f * 0.5f;
            float num3 = num2 + 0.5f;
            float value = num2 * 2f % 1f;
            float value2 = (0f - Mathf.Cos(num2 * (float)Math.PI * 2f)) * 0.5f + 0.5f;
            float value3 = (0f - Mathf.Cos(num3 * (float)Math.PI * 2f)) * 0.5f + 0.5f;
            sharedMaterial.SetFloat("_Phase0_Linear", num2);
            sharedMaterial.SetFloat("_Phase1_Linear", num3);
            sharedMaterial.SetFloat("_Phase0_Pos", value2);
            sharedMaterial.SetFloat("_Phase1_Pos", value3);
            sharedMaterial.SetFloat("_PhaseWet_Linear", value);
            float value4 = num * _wavePatternSpeed;
            sharedMaterial.SetFloat("_SurfPatternPos", value4);
        }
    }
}
