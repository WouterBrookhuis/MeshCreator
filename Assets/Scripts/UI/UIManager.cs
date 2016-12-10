using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    private ModalPanelBehaviour m_modal;

    void FindPanel()
    {
        if(m_modal == null)
        {
            var go = UICollection.instance.GetGameObject("Modal");
            if(go != null)
            {
                m_modal = go.GetComponent<ModalPanelBehaviour>();
            }
        }
    }

    /// <summary>
    /// Shows a modal window with the given parameters.
    /// </summary>
    /// <param name="title">Title of the window</param>
    /// <param name="message">Message to show</param>
    /// <param name="buttons">Text for each button</param>
    /// <param name="callback">Callback for button pressed. Index corresponds to the index of the buttons array.</param>
    public void ShowModal(string title, string message, string[] buttons, ModalPanelBehaviour.OnButtonClicked callback)
    {
        FindPanel();
        m_modal.Show(title, message, buttons, callback);
    }

    /// <summary>
    /// Shows a modal window with an "Ok" button and no callback.
    /// </summary>
    public void ShowModal(string title, string message)
    {
        FindPanel();
        m_modal.Show(title, message, new string[] { "Ok" }, null);
    }
}
