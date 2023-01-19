using UnityEngine;

public class EyeTraceController : MonoBehaviour
{
    public EyeTraceObject[] _eyeObject = new EyeTraceObject[2];

    private void Awake()
    {
        if (_eyeObject == null || _eyeObject.Length < 2)
        {
            _eyeObject = new EyeTraceObject[2];
        }
        if (_eyeObject[0] == null)
        {
            _eyeObject[0] = new EyeTraceObject();
        }
        if (_eyeObject[1] == null)
        {
            _eyeObject[1] = new EyeTraceObject();
        }
    }

    private void LateUpdate()
    {
        float num = 1f / (float)Application.targetFrameRate;
        float deltaTimeRate = Time.deltaTime / num;
        _eyeObject[0].LateUpdate(deltaTimeRate);
        _eyeObject[1].LateUpdate(deltaTimeRate);
    }

    public void SetDelayRateRate(float rate)
    {
        rate = Mathf.Clamp01(rate * 0.3f);
        _eyeObject[0].delayRate = rate;
        _eyeObject[1].delayRate = rate;
    }
}
