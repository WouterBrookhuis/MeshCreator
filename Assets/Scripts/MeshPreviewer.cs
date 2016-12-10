using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshPreviewer : Singleton<MeshPreviewer>
{
    public RenderTexture m_targetTexture;
    public Mesh m_mesh;
    public Material m_material;
    public bool m_active;
    public float m_rotationSpeed = 45;

    private Camera m_camera;
    private float m_angle = 0.0f;

    void Awake()
    {
        m_camera = new GameObject("Camera").AddComponent<Camera>();
        m_camera.transform.SetParent(transform);
        m_camera.cullingMask = LayerMask.GetMask(new string[] { LayerMask.LayerToName(gameObject.layer) });
        m_camera.clearFlags = CameraClearFlags.SolidColor;
        m_camera.backgroundColor = Color.gray;
        m_camera.targetTexture = m_targetTexture;
        m_camera.transform.Rotate(new Vector3(30, 0, 0));
        m_camera.transform.Translate(Vector3.back * 10, Space.Self);
    }

    void UpdateCameraPosition()
    {
        m_camera.transform.position = m_mesh.bounds.center;
        m_camera.transform.rotation = Quaternion.Euler(30, 0, 0);
        m_camera.transform.Translate(Vector3.back * (m_mesh.bounds.extents.magnitude * 2), Space.Self);
    }

    void Update()
    {
        if(m_active && m_mesh != null && m_material != null)
        {
            UpdateCameraPosition();
            m_camera.enabled = true;
            m_angle = (m_angle + m_rotationSpeed * Time.deltaTime) % 360.0f;

            Vector3 pos = m_mesh.bounds.center;
            pos.y = 0;
            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, m_angle, 0), Vector3.one);
            Graphics.DrawMesh(m_mesh, matrix, m_material, gameObject.layer);
        }
        else
        {
            m_camera.enabled = false;
        }
    }
}
