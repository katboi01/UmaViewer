using UnityEngine;

public class MonitorController
{
    public bool m_Multi;

    public Vector2 m_CenterPosition = new Vector2(0f, 0f);

    public Vector2 m_Scale = new Vector2(1f, 1f);

    public Vector2 m_Offset = new Vector2(0f, 0f);

    public void UpdateParam()
    {
        if (m_Multi)
        {
            m_Scale.x = Mathf.Round(m_Scale.x);
            m_Scale.y = Mathf.Round(m_Scale.y);
            if (m_Scale.x == 0f)
            {
                m_Scale.x = 1f;
            }
            if (m_Scale.y == 0f)
            {
                m_Scale.y = 1f;
            }
        }
        else
        {
            m_Scale.x = Mathf.Clamp(m_Scale.x, -1f, 1f);
            m_Scale.y = Mathf.Clamp(m_Scale.y, -1f, 1f);
        }
        float num;
        if (m_Multi)
        {
            num = 0f;
        }
        else
        {
            num = m_Scale.x % 1f;
            if (num == 0f)
            {
                num = 1f;
            }
        }
        m_CenterPosition.x = Mathf.Clamp(m_CenterPosition.x, num - 1f, 1f - num);
        if (m_Multi)
        {
            num = 0f;
        }
        else
        {
            num = m_Scale.y % 1f;
            if (num == 0f)
            {
                num = 1f;
            }
        }
        m_CenterPosition.y = Mathf.Clamp(m_CenterPosition.y, num - 1f, 1f - num);
        Vector2 vector = default(Vector2);
        vector.x = m_CenterPosition.x * 0.5f + 0.5f;
        vector.y = m_CenterPosition.y * 0.5f + 0.5f;
        m_Offset.x = vector.x - m_Scale.x * 0.5f;
        m_Offset.y = vector.y - m_Scale.y * 0.5f;
    }
}
