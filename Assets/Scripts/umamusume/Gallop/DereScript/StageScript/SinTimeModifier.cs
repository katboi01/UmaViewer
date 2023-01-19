using UnityEngine;

public class SinTimeModifier : MonoBehaviour
{
    public float[] Speeds;

    private Material[] targetMaterials;

    private const string PROPERTY_NAME = "_ModSinTime";

    private int propertyID;

    private void Start()
    {
        Renderer component = base.gameObject.GetComponent<Renderer>();
        if (component != null)
        {
            targetMaterials = component.sharedMaterials;
        }
        propertyID = Shader.PropertyToID(PROPERTY_NAME);
    }

    private void Update()
    {
        float[] array = new float[Speeds.Length];
        for (int i = 0; i < Speeds.Length; i++)
        {
            float f = Speeds[i] * Time.time;
            array[i] = Mathf.Sin(f);
        }
        Material[] array2 = targetMaterials;
        for (int j = 0; j < array2.Length; j++)
        {
            array2[j].SetFloatArray(propertyID, array);
        }
    }
}
