using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class MeshManager : MonoBehaviour
{
    private static class Data
    {
        public static void Deserialize(out List<DraggableMeshPoint> points, out List<Edge> edges, out List<Triangle> triangles, BinaryReader br, DraggableMeshPoint prefab)
        {
            int version = br.ReadInt16();
            int count = br.ReadInt32();
            points = new List<DraggableMeshPoint>();
            Debug.Log("Points: " + count);
            Dictionary<int, DraggableMeshPoint> idDict = new Dictionary<int, DraggableMeshPoint>();
            for(int i = 0; i < count; i++)
            {
                var instance = Instantiate(prefab);
                instance.m_id = br.ReadInt32();
                idDict.Add(instance.m_id, instance);
                float x, y, z;
                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                instance.transform.position = new Vector3(x, y, z);
                points.Add(instance);
            }

            count = br.ReadInt32();
            edges = new List<Edge>();
            Debug.Log("Edges: " + count);
            for(int i = 0; i < count; i++)
            {
                int a = br.ReadInt32();
                int b = br.ReadInt32();
                edges.Add(new Edge
                {
                    m_pointA = idDict[a],
                    m_pointB = idDict[b]
                });
            }

            count = br.ReadInt32();
            triangles = new List<Triangle>();
            Debug.Log("Triangles: " + count);
            for(int i = 0; i < count; i++)
            {
                int smoothingGroup = br.ReadInt32();
                int idA = br.ReadInt32();
                int idB = br.ReadInt32();
                int idC = br.ReadInt32();
                triangles.Add(new Triangle
                {
                    m_smoothGroupIndex = smoothingGroup,
                    m_pointA = idDict[idA],
                    m_pointB = idDict[idB],
                    m_pointC = idDict[idC],
                });
            }
        }
        public static void Serialize(List<DraggableMeshPoint> points, List<Edge> edges, List<Triangle> triangles, BinaryWriter bw)
        {
            short version = 1;
            bw.Write(version);
            bw.Write(points.Count);
            foreach(var point in points)
            {
                bw.Write(point.m_id);
                bw.Write(point.transform.position.x);
                bw.Write(point.transform.position.y);
                bw.Write(point.transform.position.z);
            }
            bw.Write(edges.Count);
            foreach(var edge in edges)
            {
                bw.Write(edge.m_pointA.m_id);
                bw.Write(edge.m_pointB.m_id);
            }
            bw.Write(triangles.Count);
            foreach(var triangle in triangles)
            {
                bw.Write(triangle.m_smoothGroupIndex);
                bw.Write(triangle.m_pointA.m_id);
                bw.Write(triangle.m_pointB.m_id);
                bw.Write(triangle.m_pointC.m_id);
            }
        }
    }

    private struct Edge
    {
        public DraggableMeshPoint m_pointA;
        public DraggableMeshPoint m_pointB;
        public LineRenderer m_renderer;
    }

    public class Triangle
    {
        public int m_smoothGroupIndex;
        public DraggableMeshPoint m_pointA
        {
            get
            {
                return m_points[0];
            }
            set
            {
                m_points[0] = value;
            }
        }
        public DraggableMeshPoint m_pointB
        {
            get
            {
                return m_points[1];
            }
            set
            {
                m_points[1] = value;
            }
        }
        public DraggableMeshPoint m_pointC
        {
            get
            {
                return m_points[2];
            }
            set
            {
                m_points[2] = value;
            }
        }
        public DraggableMeshPoint[] m_points;
        public Vector2 m_uvA
        {
            get
            {
                return m_uvs[0];
            }
            set
            {
                m_uvs[0] = value;
            }
        }
        public Vector2 m_uvB
        {
            get
            {
                return m_uvs[1];
            }
            set
            {
                m_uvs[1] = value;
            }
        }
        public Vector2 m_uvC
        {
            get
            {
                return m_uvs[2];
            }
            set
            {
                m_uvs[2] = value;
            }
        }
        public Vector2[] m_uvs;

        public Triangle()
        {
            m_points = new DraggableMeshPoint[3];
            m_uvs = new Vector2[3];
        }
    }

    public LayerMask m_dragPointMask;
    public DraggableMeshPoint m_prefab;
    public Material m_edgeMaterial;
    public Material m_meshMaterial;

    private List<DraggableMeshPoint> m_meshPointHandles = new List<DraggableMeshPoint>();
    private List<Edge> m_edges = new List<Edge>();
    private List<Triangle> m_triangles = new List<Triangle>();

    private DraggableMeshPoint m_selectedPoint;
    private float m_zDistance = 0.0f;

    private string m_buttonPrimary = "Fire1";

    private Camera m_camera;

    void Awake()
    {
        m_camera = Camera.main;
    }

    void Start()
    {
        if(!Load(PathManager.instance.BasePath + "/autosave.build"))
        {
            AddCube();
        }
        CameraController.main.SetPerspective(20f);
    }

    void OnApplicationQuit()
    {
        Save(PathManager.instance.BasePath + "/autosave.build");
    }

    void AddCube()
    {
        AddCube(Vector3.up);
    }

    void AddCube(Vector3 offset)
    {
        var t11 = AddHandle(offset + new Vector3(1, 1, 1));
        var t_11 = AddHandle(offset + new Vector3(-1, 1, 1));
        var t1_1 = AddHandle(offset + new Vector3(1, 1, -1));
        var t_1_1 = AddHandle(offset + new Vector3(-1, 1, -1));

        var triangle = new Triangle();
        triangle.m_smoothGroupIndex = 0;
        triangle.m_pointA = t11;
        triangle.m_pointB = t1_1;
        triangle.m_pointC = t_1_1;
        m_triangles.Add(triangle);

        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 0;
        triangle.m_pointA = t_1_1;
        triangle.m_pointB = t_11;
        triangle.m_pointC = t11;
        m_triangles.Add(triangle);

        m_edges.Add(new Edge
        {
            m_pointA = t11,
            m_pointB = t_11
        });
        m_edges.Add(new Edge
        {
            m_pointA = t11,
            m_pointB = t1_1
        });
        m_edges.Add(new Edge
        {
            m_pointA = t_1_1,
            m_pointB = t1_1
        });
        m_edges.Add(new Edge
        {
            m_pointA = t_1_1,
            m_pointB = t_11
        });

        var t11a = AddHandle(offset + new Vector3(1, -1, 1));
        var t_11a = AddHandle(offset + new Vector3(-1, -1, 1));
        var t1_1a = AddHandle(offset + new Vector3(1, -1, -1));
        var t_1_1a = AddHandle(offset + new Vector3(-1, -1, -1));

        m_edges.Add(new Edge
        {
            m_pointA = t11a,
            m_pointB = t_11a
        });
        m_edges.Add(new Edge
        {
            m_pointA = t11a,
            m_pointB = t1_1a
        });
        m_edges.Add(new Edge
        {
            m_pointA = t_1_1a,
            m_pointB = t1_1a
        });
        m_edges.Add(new Edge
        {
            m_pointA = t_1_1a,
            m_pointB = t_11a
        });

        // Bottom
        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 0;
        triangle.m_pointA = t11a;
        triangle.m_pointB = t_1_1a;
        triangle.m_pointC = t1_1a;
        m_triangles.Add(triangle);

        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 0;
        triangle.m_pointA = t_1_1a;
        triangle.m_pointB = t11a;
        triangle.m_pointC = t_11a;
        m_triangles.Add(triangle);


        // Vertical
        m_edges.Add(new Edge
        {
            m_pointA = t11a,
            m_pointB = t11
        });
        m_edges.Add(new Edge
        {
            m_pointA = t_11,
            m_pointB = t_11a
        });
        m_edges.Add(new Edge
        {
            m_pointA = t_1_1a,
            m_pointB = t_1_1
        });
        m_edges.Add(new Edge
        {
            m_pointA = t1_1,
            m_pointB = t1_1a
        });

        // Negative Z
        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 1;
        triangle.m_pointA = t_1_1;
        triangle.m_pointB = t1_1;
        triangle.m_pointC = t1_1a;
        m_triangles.Add(triangle);

        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 1;
        triangle.m_pointA = t1_1a;
        triangle.m_pointB = t_1_1a;
        triangle.m_pointC = t_1_1;
        m_triangles.Add(triangle);

        // Positive Z
        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 1;
        triangle.m_pointA = t11;
        triangle.m_pointB = t_11;
        triangle.m_pointC = t_11a;
        m_triangles.Add(triangle);

        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 1;
        triangle.m_pointA = t_11a;
        triangle.m_pointB = t11a;
        triangle.m_pointC = t11;
        m_triangles.Add(triangle);

        // Negative X
        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 2;
        triangle.m_pointA = t_11;
        triangle.m_pointB = t_1_1;
        triangle.m_pointC = t_1_1a;
        m_triangles.Add(triangle);

        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 2;
        triangle.m_pointA = t_1_1a;
        triangle.m_pointB = t_11a;
        triangle.m_pointC = t_11;
        m_triangles.Add(triangle);

        // Positive X
        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 2;
        triangle.m_pointA = t11;
        triangle.m_pointB = t1_1a;
        triangle.m_pointC = t1_1;
        m_triangles.Add(triangle);

        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 2;
        triangle.m_pointA = t1_1a;
        triangle.m_pointB = t11;
        triangle.m_pointC = t11a;
        m_triangles.Add(triangle);
    }

    void AddPyramid(Vector3 offset)
    {
        var t11a = AddHandle(offset + new Vector3(1, -1, 1));
        var t_11a = AddHandle(offset + new Vector3(-1, -1, 1));
        var t1_1a = AddHandle(offset + new Vector3(1, -1, -1));
        var t_1_1a = AddHandle(offset + new Vector3(-1, -1, -1));
        var t010 = AddHandle(offset + new Vector3(0, 1, 0));

        //Edges
        //Bottom
        m_edges.Add(new Edge
        {
            m_pointA = t11a,
            m_pointB = t_11a
        });
        m_edges.Add(new Edge
        {
            m_pointA = t11a,
            m_pointB = t1_1a
        });
        m_edges.Add(new Edge
        {
            m_pointA = t_1_1a,
            m_pointB = t1_1a
        });
        m_edges.Add(new Edge
        {
            m_pointA = t_1_1a,
            m_pointB = t_11a
        });

        // Top
        m_edges.Add(new Edge
        {
            m_pointA = t010,
            m_pointB = t11a
        });
        m_edges.Add(new Edge
        {
            m_pointA = t010,
            m_pointB = t1_1a
        });
        m_edges.Add(new Edge
        {
            m_pointA = t010,
            m_pointB = t_11a
        });
        m_edges.Add(new Edge
        {
            m_pointA = t010,
            m_pointB = t_1_1a
        });

        // Tris
        // Bottom
        var triangle = new Triangle();
        triangle.m_smoothGroupIndex = 0;
        triangle.m_pointA = t11a;
        triangle.m_pointB = t_1_1a;
        triangle.m_pointC = t1_1a;
        m_triangles.Add(triangle);

        triangle = new Triangle();
        triangle.m_smoothGroupIndex = 0;
        triangle.m_pointA = t_1_1a;
        triangle.m_pointB = t11a;
        triangle.m_pointC = t_11a;
        m_triangles.Add(triangle);
    
    }

    DraggableMeshPoint AddHandle(Vector3 position)
    {
        var instance = Instantiate(m_prefab, position, Quaternion.identity);
        m_meshPointHandles.Add(instance);
        instance.m_id = m_meshPointHandles.Count;
        return instance;
    }

    void LateUpdate()
    {
        if(m_selectedPoint == null)
        {
            if(!EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, 1000f, m_dragPointMask))
                {
                    DraggableMeshPoint point = hit.collider.GetComponent<DraggableMeshPoint>();
                    if(point != null)
                    {
                        point.Highlight();
                        if(Input.GetButtonDown(m_buttonPrimary))
                        {
                            m_selectedPoint = point;
                            m_zDistance = m_camera.transform.worldToLocalMatrix.MultiplyPoint(m_selectedPoint.transform.position).z;
                        }
                    }
                }
            }
        }
        else
        {
            if(Input.GetButtonUp(m_buttonPrimary))
            {
                m_selectedPoint = null;
            }
            else
            {
                m_selectedPoint.Select();
                if(Input.GetButton(m_buttonPrimary))
                {
                    m_selectedPoint.transform.position = m_camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_zDistance));
                }
            }
        }
        
        for(int i = 0; i < m_edges.Count; i++)
        {
            var edge = m_edges[i];
            if(edge.m_renderer == null)
            {
                var go = new GameObject("Edge Renderer (copy)");
                go.transform.SetParent(transform);
                go.layer = edge.m_pointA.gameObject.layer;
                edge.m_renderer = go.AddComponent<LineRenderer>();
                edge.m_renderer.startWidth = 0.04f;
                edge.m_renderer.endWidth = 0.04f;
                edge.m_renderer.sharedMaterial = m_edgeMaterial;
                m_edges[i] = edge;
            }
            edge.m_renderer.SetPosition(0, edge.m_pointA.transform.position);
            edge.m_renderer.SetPosition(1, edge.m_pointB.transform.position);
        }
    }

    public void ActionUnwrap()
    {
        var tool = GetComponentInChildren<UVTool>();
        tool.Unwrap(ref m_triangles);
    }

    public void ActionClear()
    {
        foreach(var edge in m_edges)
        {
            if(edge.m_renderer != null)
            {
                GameObject.Destroy(edge.m_renderer.gameObject);
            }
        }
        foreach(var point in m_meshPointHandles)
        {
            if(point != null)
            {
                GameObject.Destroy(point.gameObject);
            }
        }
        m_meshPointHandles.Clear();
        m_edges.Clear();
        m_triangles.Clear();
    }

    public void ActionGenerateMesh()
    {
        var go = new GameObject("MESH");
        var mf = go.AddComponent<MeshFilter>();
        mf.mesh = GenerateMesh();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = m_meshMaterial;
        ObjExporter.MeshToFile(mf, Application.dataPath + "/meshdump.obj");
    }

    public void ActionAddCube()
    {
        AddCube();
    }

    public void ActionAddPyramid()
    {
        AddPyramid(Vector3.zero);
    }

    public Mesh GenerateMesh()
    {
        DebugUtil.StartStopwatch("GenerateMesh");

        Mesh mesh = new Mesh();
        Dictionary<int, Dictionary<DraggableMeshPoint, int>> smoothingGroups = new Dictionary<int, Dictionary<DraggableMeshPoint, int>>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        int[] triangles = new int[m_triangles.Count * 3];
        for(int i = 0; i < m_triangles.Count; i++)
        {
            if(!smoothingGroups.ContainsKey(m_triangles[i].m_smoothGroupIndex))
            {
                smoothingGroups.Add(m_triangles[i].m_smoothGroupIndex, new Dictionary<DraggableMeshPoint, int>());
            }
            for(int j = 0; j < m_triangles[i].m_points.Length; j++)
            {
                if(!smoothingGroups[m_triangles[i].m_smoothGroupIndex].ContainsKey(m_triangles[i].m_points[j]))
                {
                    // Add vertex to vertices and smoothing group dictionary
                    vertices.Add(m_triangles[i].m_points[j].transform.position);
                    uvs.Add(m_triangles[i].m_uvs[j]);
                    smoothingGroups[m_triangles[i].m_smoothGroupIndex].Add(m_triangles[i].m_points[j], vertices.Count - 1);
                    triangles[3 * i + j] = vertices.Count - 1;
                }
                else
                {
                    // Use the one that we already have
                    triangles[3 * i + j] = smoothingGroups[m_triangles[i].m_smoothGroupIndex][m_triangles[i].m_points[j]];
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        mesh.uv = uvs.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        DebugUtil.EndStopwatch("GenerateMesh");
        return mesh;
    }

    public void Save(string fullPath)
    {
        Debug.Log("Saving to " + fullPath);
        FileStream file = File.Open(fullPath, FileMode.OpenOrCreate);
        file.Seek(0, SeekOrigin.Begin);
        BinaryWriter bw = new BinaryWriter(file);
        Data.Serialize(m_meshPointHandles, m_edges, m_triangles, bw);
        bw.Close();
        file.Close();
    }

    public bool Load(string fullPath)
    {
        if(File.Exists(fullPath))
        {
            try
            {
                Debug.Log("Loading from " + fullPath);
                using(FileStream file = File.Open(fullPath, FileMode.Open))
                using(BinaryReader br = new BinaryReader(file))
                {
                    ActionClear();
                    Data.Deserialize(out m_meshPointHandles, out m_edges, out m_triangles, br, m_prefab);
                }
                return true;
            }
            catch(Exception e)
            {
                Debug.LogError("Loading error: \r\n"+e);
            }
        }
        return false;
    }
}
