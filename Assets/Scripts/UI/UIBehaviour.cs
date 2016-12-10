using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBehaviour : MonoBehaviour
{
    protected virtual void Start()
    {

    }

    protected virtual void Awake()
    {

    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }


    protected GameObject Find(string name)
    {
        return Find(name, transform);
    }

    protected GameObject Find(string name, Transform transform)
    {
        var t = transform.FindChild(name);
        if(t != null)
        {
            return t.gameObject;
        }
        return null;
    }

    protected GameObject FindNested(string name)
    {
        return FindNested(name, transform);
    }

    protected GameObject FindNested(string name, Transform transform)
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
