using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PathManager : Singleton<PathManager>
{
    public string m_relativeSavePath = "Buildings";
    public string m_packageExtension = ".build";

    public bool m_usePersistentDataPath = false;

    public string SavePath
    {
        get
        {
            if(m_usePersistentDataPath)
            {
                return Application.persistentDataPath + "/" + m_relativeSavePath;
            }
            else
            {
                return Application.dataPath + "/" + m_relativeSavePath;
            }
        }
    }

    public string BasePath
    {
        get
        {
            if(m_usePersistentDataPath)
            {
                return Application.persistentDataPath;
            }
            else
            {
                return Application.dataPath;
            }
        }
    }

    void Awake()
    {
        if(!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
        }
    }
}
