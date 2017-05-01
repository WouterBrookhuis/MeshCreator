using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBehaviour : MonoBehaviour
{
	private bool m_isVisible;
	
	public bool isVisible
	{
		get
		{
			return m_isVisible;
		}
		set
		{
			if(value != m_isVisible)
			{
				if(value) {
					Show();
				} else {
					Hide();
				}
			}
		}
	}
	
    protected virtual void Start()
    {

    }

    protected virtual void Awake()
    {

    }

    public virtual void Show()
    {
		m_isVisible = true;
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
		m_isVisible = false;
        gameObject.SetActive(false);
    }


    public GameObject Find(string name)
    {
        return Find(name, transform);
    }

    public GameObject Find(string name, Transform transform)
    {
        var t = transform.FindChild(name);
        if(t != null)
        {
            return t.gameObject;
        }
        return null;
    }

    public GameObject FindNested(string name)
    {
        return FindNested(name, transform);
    }

    public GameObject FindNested(string name, Transform transform)
    {
        GameObject result = null;

        int childCount = transform.childCount;
        for(int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i);
            if(child.name.Equals(name))
            {
                result =  child.gameObject;
            }
            else
            {
                result = FindNested(name, child);
            }
            if(result != null)
            {
                break;
            }
        }
        return result;
    }
}
