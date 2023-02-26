using UnityEngine;

public class LiveTimelineCamera : MonoBehaviour
{
    private bool _applyNoise = true;

    public float positionFrequency = 0.2f;

    public float rotationFrequency = 0.2f;

    public float positionAmount = kPositionAmountInitVal;

    public float rotationAmount = kRotationAmountInitVal;

    public Vector3 positionComponents = kPositionComponentsInitVal;

    public Vector3 rotationComponents = kRotationComponentsInitVal;

    private int positionOctave = 2;

    private int rotationOctave = 2;

    private float timePosition;

    private float timeRotation;

    private float initTimePosition;

    private float initTimeRotation;

    private Vector2[] noiseVectors;

    private Camera _attachedCamera;

    private Vector3 posOffset = Vector3.zero;

    private Quaternion rotOffset = Quaternion.identity;

    public bool applyNoise
    {
        get
        {
            return _applyNoise;
        }
        set
        {
            if (_applyNoise != value && attachedCamera != null)
            {
                attachedCamera.ResetWorldToCameraMatrix();
            }
            _applyNoise = value;
        }
    }

    public static float kPositionAmountInitVal
    {
        get
        {
            return 0f;
        }
    }

    public static float kRotationAmountInitVal
    {
        get
        {
            return 3f;
        }
    }

    public static Vector3 kPositionComponentsInitVal
    {
        get
        {
            return Vector3.one;
        }
    }

    public static Vector3 kRotationComponentsInitVal
    {
        get
        {
            return new Vector3(1f, 1f, 0f);
        }
    }

    private Camera attachedCamera
    {
        get
        {
            if (_attachedCamera == null)
            {
                _attachedCamera = GetComponent<Camera>();
            }
            return _attachedCamera;
        }
    }

    public void AlterAwake()
    {
        applyNoise = false;
        initTimePosition = Random.value * 10f;
        initTimeRotation = Random.value * 10f;
        noiseVectors = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float f = Random.value * 3.14159274f * 2f;
            noiseVectors[i].Set(Mathf.Cos(f), Mathf.Sin(f));
        }
    }

    public void AlterUpdate(float liveTime)
    {
        if (attachedCamera.isActiveAndEnabled && applyNoise)
        {
            timePosition = initTimePosition + liveTime * positionFrequency;
            timeRotation = initTimeRotation + liveTime * rotationFrequency;
            if (positionAmount != 0f)
            {
                Vector3 a = new Vector3(Fbm(noiseVectors[0] * timePosition, positionOctave), Fbm(noiseVectors[1] * timePosition, positionOctave), Fbm(noiseVectors[2] * timePosition, positionOctave));
                a = (posOffset = Vector3.Scale(a, positionComponents) * positionAmount * 2f);
            }
            if (rotationAmount != 0f)
            {
                Vector3 a2 = new Vector3(Fbm(noiseVectors[3] * timeRotation, rotationOctave), Fbm(noiseVectors[4] * timeRotation, rotationOctave), Fbm(noiseVectors[5] * timeRotation, rotationOctave));
                a2 = Vector3.Scale(a2, rotationComponents) * rotationAmount * 2f;
                rotOffset = Quaternion.Euler(a2);
            }
        }
    }

    private static float Fbm(Vector2 coord, int octave)
    {
        float num = 0f;
        float num2 = 1f;
        for (int i = 0; i < octave; i++)
        {
            num += num2 * (Mathf.PerlinNoise(coord.x, coord.y) - 0.5f);
            coord *= 2f;
            num2 *= 0.5f;
        }
        return num;
    }

    private void OnPreCull()
    {
        if (!(attachedCamera == null) && attachedCamera.isActiveAndEnabled && applyNoise)
        {
            Matrix4x4 lhs = Matrix4x4.TRS(posOffset, rotOffset, new Vector3(1f, 1f, -1f));
            attachedCamera.worldToCameraMatrix = lhs * attachedCamera.transform.worldToLocalMatrix;
        }
    }
}