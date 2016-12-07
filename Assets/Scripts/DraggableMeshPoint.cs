using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DraggableMeshPoint : MonoBehaviour
{
    public int m_id;
    public Color m_defaultColor = Color.white;
    public Color m_highlightColor = Color.blue;
    public Color m_selectColor = Color.green;

    private Renderer m_renderer;

    void Awake()
    {
        m_renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        m_renderer.material.color = m_defaultColor;
    }

    public void Highlight()
    {
        m_renderer.material.color = m_highlightColor;
    }

    public void Select()
    {
        m_renderer.material.color = m_selectColor;
    }
}
