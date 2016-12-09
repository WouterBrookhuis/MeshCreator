using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBehaviour : MonoBehaviour
{
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }


    public GameObject Find(string name)
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

    private GameObject FindNested(string name, Transform transform)
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
