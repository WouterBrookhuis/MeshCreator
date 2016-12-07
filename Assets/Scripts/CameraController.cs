using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float m_moveSensitivity = 2.0f;
    public float m_rotateSensitivity = 200.0f;
    public string m_rotateButton = "Fire2";

    private Camera m_camera;
    private Transform m_transform;
    
    void Awake()
    {
        m_camera = GetComponent<Camera>();
        if(m_camera != null)
        {
            m_transform = transform;
        }
    }

    void Update()
    {
        if(m_transform != null)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                x *= 3;
                y *= 3;
            }
            m_transform.Translate(new Vector3(x, 0, y) * Time.deltaTime * m_moveSensitivity, Space.Self);
            
            if(Input.GetButton(m_rotateButton))
            {
                m_transform.Rotate(Vector3.up, mx * Time.deltaTime * m_rotateSensitivity, Space.World);
                m_transform.Rotate(Vector3.right, -my * Time.deltaTime * m_rotateSensitivity, Space.Self);
            }
        }
    }
}
