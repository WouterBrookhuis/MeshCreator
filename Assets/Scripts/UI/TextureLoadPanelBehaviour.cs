using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TextureLoadPanelBehaviour : UIBehaviour
{
    public GameObject m_rowTemplate;

    private Button m_openFolderButton;
    private Button m_loadButton;
    private Button m_cancelButton;
    private UVTool m_uvTool;
    private string m_savePath;
    private RectTransform m_contentList;
    private SelectableRow m_selectedRow;

    private List<SelectableRow> m_rows = new List<SelectableRow>();

    protected override void Awake()
    {
        m_uvTool = FindObjectOfType<UVTool>();
        if(m_uvTool == null)
        {
            Debug.LogWarning("Could not find UVTool!");
        }
    }

    public override void Show()
    {
        base.Show();
        UpdateListing();
    }

    protected override void Start()
    {
        m_loadButton = Find("Load").GetComponent<Button>();
        m_cancelButton = Find("Cancel").GetComponent<Button>();
        m_openFolderButton = Find("Open").GetComponent<Button>();
        m_contentList = FindNested("Content List").GetComponent<RectTransform>();

        m_loadButton.onClick.AddListener(OnLoad);
        m_cancelButton.onClick.AddListener(OnCancel);
        m_openFolderButton.onClick.AddListener(OnOpen);

        m_loadButton.interactable = false;
        gameObject.SetActive(false);
    }

    void OnOpen()
    {
        System.Diagnostics.Process.Start(Path.GetFullPath(PathManager.instance.TexturePath));
    }

    void OnCancel()
    {
        Hide();
    }

    void OnLoad()
    {
        if(m_selectedRow != null)
        {
            string trimmed = m_selectedRow.GetText().Trim().ToLower();
            m_savePath = PathManager.instance.TexturePath + "/" + trimmed;
            if(File.Exists(m_savePath))
            {
                Texture2D texture = new Texture2D(2, 2);
                using(var stream = File.Open(m_savePath, FileMode.Open))
                {
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    texture.LoadImage(bytes);
                    m_uvTool.m_texture = texture;
                }
                m_selectedRow.Deselect();
                gameObject.SetActive(false);
            }
            else
            {
                // Show popup?
                UIManager.instance.ShowModal("Load", "The requested file does not exist.");
            }
        }
    }

    void OnRowSelected(SelectableRow newRow)
    {
        if(m_selectedRow != null)
            m_selectedRow.Deselect();

        m_loadButton.interactable = true;
        m_selectedRow = newRow;
    }

    void UpdateListing()
    {
        m_selectedRow = null;
        m_loadButton.interactable = false;
        foreach(var go in m_rows)
        {
            go.gameObject.SetActive(false);
        }

        var files = new List<string>();
        foreach(var extension in PathManager.instance.m_textureExtensions)
        {
            Debug.Log(extension);
            files.AddRange(Directory.GetFiles(PathManager.instance.TexturePath, "*" + extension));
        }
        Debug.Log(files.Count);
        m_contentList.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, files.Count * 30);
        for(int i = 0; i < files.Count; i++)
        {
            if(m_rows.Count == i)
            {
                var row = Instantiate(m_rowTemplate).GetComponent<SelectableRow>();
                row.transform.SetParent(m_contentList);
                row.eventClicked += OnRowSelected;
                m_rows.Add(row);
            }
            m_rows[i].gameObject.SetActive(true);
            m_rows[i].SetText(Path.GetFileName(files[i]));
        }
    }
}
