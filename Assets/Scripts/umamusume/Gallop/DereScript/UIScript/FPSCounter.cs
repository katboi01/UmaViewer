using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    [SerializeField]
    private float m_updateInterval = 0.4f;

    private GameObject gText = null;

    private Text text = null;

    private float m_accum;

    private int m_frames;

    private float m_timeleft;

    private float m_fps;

    private bool isFPSdraw;

    private void Awake()
    {
        gText = transform.Find("FPSText").gameObject;
        text = gText.GetComponent<Text>();
    }

    private void Start()
    {
        gText.SetActive(false);
    }

    private void Update()
    {
        if (gText != null && text != null)
        {
            m_timeleft -= Time.deltaTime;

            m_accum += Time.timeScale / Time.deltaTime;
            m_frames++;

            if (0 < m_timeleft) return;

            m_fps = m_accum / m_frames;
            text.text = "FPS: " + m_fps.ToString("f2");

            //初期化
            m_timeleft = m_updateInterval;
            m_accum = 0;
            m_frames = 0;
        }
    }

    void OnEnable()
    {
        m_timeleft = m_updateInterval;
        m_accum = 0;
        m_frames = 0;
    }

    public void ChangeVisible()
    {
        if (gText != null)
        {
            //ひっくり返す
            gText.SetActive(!gText.activeSelf);
        }

    }
}
