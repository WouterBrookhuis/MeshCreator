using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MeshPreviewPanelBehaviour : UIBehaviour
{
    private Button m_saveButton;
    private Button m_closeButton;
    private MeshManager m_meshManager;

    protected override void Awake()
    {
        m_meshManager = GameObject.FindObjectOfType<MeshManager>();
        if(m_meshManager == null)
        {
            Debug.LogWarning("Could not find MeshManager!");
        }
    }

    public override void Show()
    {
        base.Show();
        MeshPreviewer.instance.m_active = true;
    }

    public override void Hide()
    {
        base.Hide();
        MeshPreviewer.instance.m_active = false;
    }

    protected override void Start()
    {
        m_saveButton = Find("Save").GetComponent<Button>();
        m_closeButton = Find("Close").GetComponent<Button>();

        m_saveButton.onClick.AddListener(OnSave);
        m_closeButton.onClick.AddListener(OnClose);

        gameObject.SetActive(false);
    }

    private void OnSave()
    {
        string filename = "Export " + MeshPreviewer.instance.m_mesh.name + " " + DateTime.Now.ToFileTimeUtc() + ".obj";
        using(StreamWriter sw = new StreamWriter(PathManager.instance.BasePath + "/" + filename, false))
        {
            sw.Write(ObjExporter.MeshToString(MeshPreviewer.instance.m_mesh, new Material[] { MeshPreviewer.instance.m_material }));
        }
        UIManager.instance.ShowModal("Mesh Exporter", "Mesh exported to \r\n" + PathManager.instance.BasePath + "/" + filename,
            new string[] { "Open Folder", "Close" }, (i) => {
                if(i == 0)
                {
                    System.Diagnostics.Process.Start(Path.GetFullPath(PathManager.instance.BasePath));
                }
            });
    }

    private void OnClose()
    {
        Hide();
    }
}
