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

    void Awake()
    {
        m_meshManager = GameObject.FindObjectOfType<MeshManager>();
        if(m_meshManager == null)
        {
            Debug.LogWarning("Could not find MeshManager!");
        }
    }

    void Start()
    {
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
        m_meshManager.Save(m_savePath);
        gameObject.SetActive(false);
    }

    void OnEndEdit(string text)
    {
        string trimmed = text.Trim().ToLower();
        m_saveButton.interactable = !string.IsNullOrEmpty(trimmed);
        m_savePath = PathManager.instance.SavePath + "/" + trimmed + PathManager.instance.m_packageExtension;
    }
}
