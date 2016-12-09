using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISlidingAnimator : MonoBehaviour
{
    public enum StartPosition
    {
        Open, Closed, Default
    }

    public Vector3 m_openOffset;
    public Vector3 m_closeOffset;
    public float m_moveSpeed = 1.0f;
    public StartPosition m_startMode = StartPosition.Closed;

    private Transform m_transform;
    private Vector3 m_startPosition;
    private float m_position = 0.0f;
    private float m_targetPosition = 0.0f;

    void Awake()
    {
        m_transform = GetComponent<Transform>();
        m_startPosition = m_transform.position;
        if(m_startMode == StartPosition.Closed)
        {
            Jump(0.0f);
        }
        else if(m_startMode == StartPosition.Open)
        {
            Jump(1.0f);
        }
    }

    void Update()
    {
        if(Mathf.Abs(m_position - m_targetPosition) > 0.005f)
        {
            float delta = Mathf.Clamp(m_targetPosition - m_position, -m_moveSpeed * Time.deltaTime, m_moveSpeed * Time.deltaTime);
            m_position = Mathf.Clamp01(m_position + delta);
            ApplyPosition();
        }
    }

    void ApplyPosition()
    {
        m_transform.position = Vector3.Lerp(m_startPosition + m_closeOffset, m_startPosition + m_openOffset, m_position);
    }

    void Jump(float position)
    {
        m_position = Mathf.Clamp01(position);
        m_targetPosition = m_position;
        ApplyPosition();
    }

    public void Open()
    {
        m_targetPosition = 0.0f;
    }

    public void Close()
    {
        m_targetPosition = 1.0f;
    }

    public void Toggle()
    {
        if(m_targetPosition < 0.5f)
            m_targetPosition = 1.0f;
        else
            m_targetPosition = 0.0f;
    }
}