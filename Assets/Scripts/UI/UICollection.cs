using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICollection : Singleton<UICollection>
{
    public GameObject[] m_uiElements;

    private static Dictionary<string, GameObject> m_dict = new Dictionary<string, GameObject>();

    void Awake()
    {
        AddUIElements(m_uiElements);
    }

    public static void AddUIElements(GameObject[] array)
    {
        foreach(var element in array)
        {
            if(m_dict.ContainsKey(element.name))
                Debug.Log("Duplicate UI element: " + element.name);
            else
                m_dict.Add(element.name, element);
        }
    }

    public static void RemoveUIElements(GameObject[] array)
    {
        foreach(var element in array)
        {
            m_dict.Remove(element.name);
        }
    }

    public GameObject GetGameObject(string name)
    {
        GameObject value;
        m_dict.TryGetValue(name, out value);
        return value;
    }
}
