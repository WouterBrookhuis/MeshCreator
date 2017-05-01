using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalPanelBehaviour : UIBehaviour
{
    public GameObject m_buttonTemplate;
    public delegate void OnButtonClicked(int index);

    private List<GameObject> m_buttons = new List<GameObject>();
    private OnButtonClicked m_delegate;
    private Text m_message;
    private Text m_title;

    protected override void Awake()
    {
		base.Awake();
        m_message = FindNested("Message").GetComponent<Text>();
        m_title = FindNested("Title").GetComponent<Text>();
        var buttons = GetComponentsInChildren<Button>();
        foreach(var butt in buttons)
        {
            m_buttons.Add(butt.gameObject);
            butt.gameObject.SetActive(false);
        }
    }

    protected override void Start()
    {
		base.Start();
        gameObject.SetActive(false);
    }

    public void Show(string title, string message, string[] buttons, OnButtonClicked callback)
    {
        base.Show();
        transform.SetAsLastSibling();
        m_delegate = callback;
        m_title.text = title;
        m_message.text = message;
        foreach(var button in m_buttons)
        {
            button.SetActive(false);
        }

        for(int i = 0; i < buttons.Length; i++)
        {
            if(i == m_buttons.Count)
            {
                var button = Instantiate(m_buttonTemplate);
                button.transform.SetParent(FindNested("Panel").transform);
                m_buttons.Add(button);
            }
            m_buttons[i].SetActive(true);
            m_buttons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            m_buttons[i].GetComponentInChildren<Text>().text = buttons[i];
            var curIndex = i;
            m_buttons[i].GetComponent<Button>().onClick.AddListener(() => {
                OnButtonClickedInt(curIndex);
            });
        }
    }

    void OnButtonClickedInt(int index)
    {
        Debug.Log("Modal button " + index + " was clicked");
        if(m_delegate != null)
        {
            m_delegate.Invoke(index);
        }
        Hide();
    }
}
