using Cutt;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public enum Status
    {
        Stop,
        Playing,
        Pause
    }

    private struct ParticleData
    {
        public ParticleSystem particleSystem;

        public float startEmissionRate;

        public ParticleSystem.MinMaxCurve rateOverTime;

        public ParticleSystem.MinMaxCurve rateOverDistance;

        public Renderer renderer;
    }

    [SerializeField]
    [Range(0f, 100f)]
    private float _playSpeed = 1f;

    private ParticleData[] _particleDatas;

    private ParticleSystem[] _particleSystems;

    private int _materialPropID_Timer;

    private int _materialPropID_VertexLerpColor;

    private int _materialPropID_ColorLerpRate;

    private int _nameHash;

    private float _timer;

    private float _lastTime;

    private bool _isPause;

    private Dictionary<int, ParticleData> _particleDictionary = new Dictionary<int, ParticleData>();

    [SerializeField]
    public Status playStatus { get; protected set; }

    private void Start()
    {
        _particleSystems = base.gameObject.GetComponentsInChildren<ParticleSystem>();
        if (_particleSystems == null || _particleSystems.Length == 0)
        {
            return;
        }
        _particleDatas = new ParticleData[_particleSystems.Length];
        if (_particleDatas != null)
        {
            for (int i = 0; i < _particleDatas.Length; i++)
            {
                _particleDatas[i].particleSystem = _particleSystems[i];
                _particleDatas[i].renderer = _particleSystems[i].GetComponent<Renderer>();
                int key = FNVHash.Generate(_particleSystems[i].name);
                _particleDictionary.Add(key, _particleDatas[i]);
                ParticleSystem.MainModule main = _particleSystems[i].main;
                main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            }
            PlayEmit();
            PauseEmit();
            _materialPropID_Timer = Shader.PropertyToID("_Timer");
            _materialPropID_VertexLerpColor = Shader.PropertyToID("_VertexLerpColor");
            _materialPropID_ColorLerpRate = Shader.PropertyToID("_ColorLerpRate");
            string seed = base.gameObject.name.Replace("(Clone)", "");
            _nameHash = FNVHash.Generate(seed);
        }
    }

    public void Update()
    {
        if (playStatus != 0)
        {
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            _timer += (_isPause ? 0f : (realtimeSinceStartup - _lastTime));
            _lastTime = realtimeSinceStartup;
            for (int i = 0; i < _particleDatas.Length; i++)
            {
                _particleDatas[i].renderer.sharedMaterial.SetFloat(_materialPropID_Timer, _timer);
            }
        }
    }

    public void PlayEmit()
    {
        if (_particleDatas == null || playStatus != 0)
        {
            return;
        }
        for (int i = 0; i < _particleDatas.Length; i++)
        {
            if (!_particleDatas[i].particleSystem.isPlaying)
            {
                ParticleSystem.MainModule main = _particleDatas[i].particleSystem.main;
                main.simulationSpeed = _playSpeed;
                _particleDatas[i].rateOverTime = _particleSystems[i].emission.rateOverTime;
                _particleDatas[i].rateOverDistance = _particleSystems[i].emission.rateOverDistance;
                ParticleSystem.EmissionModule emission = _particleDatas[i].particleSystem.emission;
                emission.enabled = true;
                _particleDatas[i].particleSystem.Play();
            }
        }
        playStatus = Status.Playing;
    }

    public void PauseEmit()
    {
        if (_particleDatas == null || playStatus != Status.Playing)
        {
            return;
        }
        for (int i = 0; i < _particleDatas.Length; i++)
        {
            if (_particleDatas[i].particleSystem.isPlaying)
            {
                ParticleSystem.EmissionModule emission = _particleDatas[i].particleSystem.emission;
                _particleDatas[i].rateOverTime = new ParticleSystem.MinMaxCurve(0f);
                _particleDatas[i].rateOverDistance = new ParticleSystem.MinMaxCurve(0f);
                emission.enabled = false;
            }
        }
        playStatus = Status.Pause;
    }

    public void ResumeEmit()
    {
        if (_particleDatas == null || playStatus != Status.Pause)
        {
            return;
        }
        for (int i = 0; i < _particleDatas.Length; i++)
        {
            if (_particleDatas[i].particleSystem.isPlaying)
            {
                ParticleSystem.EmissionModule emission = _particleDatas[i].particleSystem.emission;
                _particleDatas[i].rateOverTime = _particleSystems[i].emission.rateOverTime;
                _particleDatas[i].rateOverDistance = _particleSystems[i].emission.rateOverDistance;
                emission.enabled = true;
            }
        }
        playStatus = Status.Playing;
    }

    public void StopEmit()
    {
        if (_particleDatas == null || playStatus == Status.Stop)
        {
            return;
        }
        for (int i = 0; i < _particleDatas.Length; i++)
        {
            if (_particleDatas[i].particleSystem.isPlaying)
            {
                _particleDatas[i].particleSystem.Stop();
                ParticleSystem.EmissionModule emission = _particleDatas[i].particleSystem.emission;
                emission.enabled = false;
            }
        }
        playStatus = Status.Stop;
    }

    public void Pause()
    {
        if (_particleDatas != null && playStatus != 0 && !_isPause)
        {
            for (int i = 0; i < _particleDatas.Length; i++)
            {
                _particleDatas[i].particleSystem.Pause();
            }
            _isPause = true;
        }
    }

    public void Resume()
    {
        if (_particleDatas != null && playStatus != 0 && _isPause)
        {
            for (int i = 0; i < _particleDatas.Length; i++)
            {
                _particleDatas[i].particleSystem.Play();
            }
            _lastTime = Time.realtimeSinceStartup;
            _isPause = false;
        }
    }

    public void UpdateFromTimeline(ref ParticleUpdateInfo updateInfo)
    {
        if (_particleDictionary.ContainsKey(updateInfo.data.nameHash))
        {
            ParticleSystem.EmissionModule emission = _particleDictionary[updateInfo.data.nameHash].particleSystem.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(updateInfo.emissionRate);
            emission.rateOverDistance = new ParticleSystem.MinMaxCurve(updateInfo.emissionRate);
            emission.enabled = true;
        }
    }

    public void UpdateGroupFromTimeline(ref ParticleGroupUpdateInfo updateInfo)
    {
        if (_nameHash == updateInfo.data.nameHash && _particleDatas != null)
        {
            for (int i = 0; i < _particleDatas.Length; i++)
            {
                _particleDatas[i].renderer.sharedMaterial.SetColor(_materialPropID_VertexLerpColor, updateInfo.lerpColor);
                _particleDatas[i].renderer.sharedMaterial.SetFloat(_materialPropID_ColorLerpRate, updateInfo.lerpColorRate);
            }
        }
    }
}
