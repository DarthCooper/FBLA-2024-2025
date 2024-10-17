using UnityEngine;
using System.Collections;

public class FPSCounter : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    public static float m_lastFramerate = 0.0f;
    public float m_refreshTime = 0.5f;

    void Update()
    {
        if (m_timeCounter < m_refreshTime)
        {
            m_timeCounter += Time.deltaTime;
            m_frameCounter++;
        }
        else
        {
            //This code will break if you set your m_refreshTime to 0, which makes no sense.
            m_lastFramerate = (float)m_frameCounter / m_timeCounter;
            m_frameCounter = 0;
            m_timeCounter = 0.0f;
        }
    }
}
