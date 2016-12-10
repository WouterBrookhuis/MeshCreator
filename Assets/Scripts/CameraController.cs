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
    private Vector3 m_freePosition;
    private Quaternion m_freeRotation;

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
            float z = Input.GetAxis("Vertical Actual");
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                x *= 3;
                y *= 3;
                z *= 3;
            }
            if(!m_camera.orthographic)
            {
                m_transform.Translate(new Vector3(x, 0, y) * Time.deltaTime * m_moveSensitivity, Space.Self);
                m_transform.Translate(new Vector3(0, z, 0) * Time.deltaTime * m_moveSensitivity, Space.World);

                if(Input.GetButton(m_rotateButton))
                {
                    m_transform.Rotate(Vector3.up, mx * Time.deltaTime * m_rotateSensitivity, Space.World);
                    m_transform.Rotate(Vector3.right, -my * Time.deltaTime * m_rotateSensitivity, Space.Self);
                }

                // Preset cam points
                if(Input.GetKeyDown(KeyCode.Keypad3))
                {
                    float ang = 90;
                    if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                        ang += 180;

                    m_transform.rotation = Quaternion.Euler(0, -ang, 0);
                }
                if(Input.GetKeyDown(KeyCode.Keypad1))
                {
                    float ang = 0;
                    if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                        ang += 180;

                    m_transform.rotation = Quaternion.Euler(0, -ang, 0);
                }
                if(Input.GetKeyDown(KeyCode.Keypad7))
                {
                    float ang = -90;
                    if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                        ang += 180;

                    m_transform.rotation = Quaternion.Euler(-ang, 0, 0);
                }
            }
            else
            {
                m_transform.Translate(new Vector3(x, y, 0) * Time.deltaTime * m_moveSensitivity, Space.Self);
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

    public Camera GetCamera(string name)
    {
        var t = transform.Find(name);
        if(t != null)
        {
            return t.GetComponent<Camera>();
        }
        return null;
    }

    public void SetFixedPoint()
    {
        m_freePosition = transform.position;
        m_freeRotation = transform.rotation;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    public void ReleaseFixedPoint()
    {
        transform.position = m_freePosition;
        transform.rotation = m_freeRotation;
    }
}
