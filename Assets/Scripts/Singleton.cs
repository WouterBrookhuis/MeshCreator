using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T m_instance;
    public static T instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = GameObject.FindObjectOfType<T>();
                if(m_instance == null)
                {
                    m_instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    //DontDestroyOnLoad(m_instance.gameObject);
                }
            }
            return m_instance;
        }
    }
}
