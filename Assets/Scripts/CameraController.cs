using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController main { get; private set; }
    public float m_moveSensitivity = 2.0f;
    public float m_rotateSensitivity = 200.0f;
    public string m_rotateButton = "Fire2";
    public bool m_enableInput = true;

    private Camera m_camera;
    private Transform m_transform;

    public Transform Transform
    {
        get
        {
            return m_transform;
        }
    }
    
    void Awake()
    {
        m_camera = GetComponent<Camera>();
        if(m_camera != null)
        {
            if(Camera.main == m_camera)
                main = this;

            m_transform = transform;
        }
    }

    void Update()
    {
        if(m_transform != null && m_enableInput)
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

    public void SetOrthographic(float size)
    {
        var cameras = GetComponentsInChildren<Camera>();
        foreach(var cam in cameras)
        {
            cam.orthographic = true;
            cam.orthographicSize = size;
        }
    }

    public void SetPerspective(float fov)
    {
        var cameras = GetComponentsInChildren<Camera>();
        foreach(var cam in cameras)
        {
            cam.orthographic = false;
            cam.fieldOfView = fov;
        }
    }
}
