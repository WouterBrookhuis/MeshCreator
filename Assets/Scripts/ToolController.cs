using UnityEngine;

public class ToolController : Singleton<ToolController>
{
    private MeshManager m_meshManager;
    private UVTool m_uvTool;
    private UIBehaviour m_buildingMenu;
    private UIBehaviour m_uvMenu;

    void Start()
    {
        GetReferences();
        m_buildingMenu.Show();
        m_uvMenu.Hide();
        CameraController.main.SetPerspective(45.0f);
    }

    void GetReferences()
    {
        m_meshManager = FindObjectOfType<MeshManager>();
        m_uvTool = FindObjectOfType<UVTool>();
        m_buildingMenu = UICollection.instance.GetGameObject("Menu Building").GetComponent<UIBehaviour>();
        m_uvMenu = UICollection.instance.GetGameObject("Menu UV").GetComponent<UIBehaviour>();
    }

    void Update()
    {
        if(m_uvTool.m_active)
        {
            if(m_meshManager.m_invalidated)
            {
                m_uvTool.CreateHandles(m_meshManager.Triangles);
                m_meshManager.m_invalidated = false;
            }
        }
    }

    public void ToggleUVTool()
    {
        m_uvTool.m_active = !m_uvTool.m_active;
        m_meshManager.m_active = !m_uvTool.m_active;
        if(m_uvTool.m_active)
        {
            m_uvMenu.Show();
            m_buildingMenu.Hide();
        }
        else
        {
            m_uvMenu.Hide();
            m_buildingMenu.Show();
        }
    }

    public void Unwrap()
    {
        m_meshManager.m_invalidated = true;
        var triangles = m_meshManager.Triangles;
        m_uvTool.Unwrap(ref triangles);
    }

    public void ActionPreviewMesh()
    {
        var mesh = m_meshManager.GenerateMesh();
        MeshPreviewer.instance.m_mesh = mesh;
        MeshPreviewer.instance.m_material.mainTexture = m_uvTool.m_texture;
        UICollection.instance.GetGameObject("Mesh Preview Panel").GetComponent<MeshPreviewPanelBehaviour>().Show();
    }

    public void QuitApplication()
    {
        UIManager.instance.ShowModal("Mesh Creator", "Are you sure you want to quit?", new string[] { "Yes", "No" }, (i) => {
            if(i == 0)
            {
                Application.Quit();
            }
        });
    }
}
