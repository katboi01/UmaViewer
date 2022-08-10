using UnityEngine;
using System.Collections;

public class DynamicBoneDemo1 : MonoBehaviour
{
    public GameObject m_Player;
    float m_weight = 1;
    float m_sleepTime;

    void Update()
    {
        m_Player.transform.Rotate(new Vector3(0, Input.GetAxis("Horizontal") * Time.deltaTime * 200, 0));
        m_Player.transform.Translate(transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * 4);
    }

    void OnGUI()
    {
        float x = 50;
        float y = 50;
        float w1 = 100;
        float w2 = 200;
        float h = 24;

        GUI.Label(new Rect(x, y, w2, h), "Press arrow key to move");
        Animation a = m_Player.GetComponentInChildren<Animation>();
        y += h;
        a.enabled = GUI.Toggle(new Rect(x, y, w2, h), a.enabled, "Play Animation");

        y += h * 2;
        DynamicBone[] dbs = m_Player.GetComponents<DynamicBone>();
        GUI.Label(new Rect(x, y, w2, h), "Choose dynamic bone:");
        y += h;
        dbs[0].enabled = dbs[1].enabled = GUI.Toggle(new Rect(x, y, w1, h), dbs[0].enabled, "Breasts");
        y += h;
        dbs[2].enabled = GUI.Toggle(new Rect(x, y, w1, h), dbs[2].enabled, "Tail");

        y += h;
        GUI.Label(new Rect(x, y, w2, h), "Weight");
        m_weight = GUI.HorizontalSlider(new Rect(x + 50, y, w1, h), m_weight, 0, 1);
        foreach (var db in dbs)
            db.SetWeight(m_weight);
/*
        y += h * 2;
        GUI.Label(new Rect(x, y, w2, h), "Sleep");
        m_sleepTime = GUI.HorizontalSlider(new Rect(x + 50, y, w1, h), m_sleepTime, 0, 1);
        if (m_sleepTime > 0)
            System.Threading.Thread.Sleep((int)(m_sleepTime * 100));

        y += h;
        GUI.Label(new Rect(x, y, w2, h), "Time Scale");
        Time.timeScale = GUI.HorizontalSlider(new Rect(x + 80, y, w1, h), Time.timeScale, 0, 2);
*/
    }
}
