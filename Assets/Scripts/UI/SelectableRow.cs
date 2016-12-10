using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableRow : Button
{
    public delegate void OnClicked(SelectableRow row);
    public event OnClicked eventClicked;

    private Color m_normalColor;
    private Text m_text;

    protected override void Awake()
    {
        base.Awake();
        onClick.AddListener(OnClickListener);
        m_normalColor = colors.normalColor;
        m_text = GetComponentInChildren<Text>();
    }

    public void SetText(string text)
    {
        m_text.text = text;
    }

    public string GetText()
    {
        return m_text.text;
    }

    private void OnClickListener()
    {
        var c = colors;
        c.normalColor = c.pressedColor;
        colors = c;
        if(eventClicked != null)
        {
            eventClicked.Invoke(this);
        }
    }

    public void Deselect()
    {
        var c = colors;
        c.normalColor = m_normalColor;
        colors = c;
    }
}
