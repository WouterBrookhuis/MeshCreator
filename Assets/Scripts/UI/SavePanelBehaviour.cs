using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SavePanelBehaviour : UIBehaviour
{
    private Button m_saveButton;
    private Button m_cancelButton;
    private InputField m_nameField;
    private MeshManager m_meshManager;
    private string m_savePath;

    protected override void Awake()
    {
		base.Awake();
        m_meshManager = GameObject.FindObjectOfType<MeshManager>();
        if(m_meshManager == null)
        {
            Debug.LogWarning("Could not find MeshManager!");
        }
    }

    protected override void Start()
    {
		base.Start();
        m_nameField = Find("Name").GetComponent<InputField>();
        m_saveButton = Find("Save").GetComponent<Button>();
        m_cancelButton = Find("Cancel").GetComponent<Button>();

        m_nameField.onEndEdit.AddListener(OnEndEdit);
        m_saveButton.onClick.AddListener(OnSave);
        m_cancelButton.onClick.AddListener(OnCancel);

        m_saveButton.interactable = false;
        gameObject.SetActive(false);
    }

    void OnCancel()
    {
        gameObject.SetActive(false);
    }

    void OnSave()
    {
        if(File.Exists(m_savePath))
        {
            UIManager.instance.ShowModal("Save", "A file with that name already exists, do you want to overwrite it?",
                new string[] { "Yes", "No" },
                (i) => {
                    if(i == 0)
                    {
                        m_meshManager.Save(m_savePath);
                        gameObject.SetActive(false);
                    }
                });
        }
        else
        {
            m_meshManager.Save(m_savePath);
            gameObject.SetActive(false);
        }        
    }

    void DoSave()
    {
        
    }

    void OnEndEdit(string text)
    {
        string trimmed = text.Trim().ToLower();
        m_saveButton.interactable = !string.IsNullOrEmpty(trimmed);
        m_savePath = PathManager.instance.SavePath + "/" + trimmed + PathManager.instance.m_packageExtension;
    }
}
