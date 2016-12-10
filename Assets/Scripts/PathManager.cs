using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PathManager : Singleton<PathManager>
{
    public string m_relativeSavePath = "Buildings";
    public string m_relativeTexturePath = "Textures";
    public string m_packageExtension = ".build";
    public string[] m_textureExtensions = new string[] { ".jpg", ".png" };

    public bool m_usePersistentDataPath = false;

    public string SavePath
    {
        get
        {
            return BasePath + "/" + m_relativeSavePath;
        }
    }

    public string TexturePath
    {
        get
        {
            return BasePath + "/" + m_relativeTexturePath;
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
        if(!Directory.Exists(TexturePath))
        {
            Directory.CreateDirectory(TexturePath);
        }
    }
}
