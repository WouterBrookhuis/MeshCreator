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
        public static void Deserialize(out List<DraggableMeshPoint> points, out List<Edge> edges, out List<DraggableMeshPoint> triangles, BinaryReader br, DraggableMeshPoint prefab)
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
            triangles = new List<DraggableMeshPoint>();
            Debug.Log("Triangles: " + count);
            for(int i = 0; i < count; i++)
            {
                int id = br.ReadInt32();
                triangles.Add(idDict[id]);
            }
        }
        public static void Serialize(List<DraggableMeshPoint> points, List<Edge> edges, List<DraggableMeshPoint> triangles, BinaryWriter bw)
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
                bw.Write(triangle.m_id);
            }
        }
    }

    private struct Edge
    {
        public DraggableMeshPoint m_pointA;
        public DraggableMeshPoint m_pointB;
        public LineRenderer m_renderer;
    }

    public LayerMask m_dragPointMask;
    public DraggableMeshPoint m_prefab;
    public Material m_edgeMaterial;
    public Material m_meshMaterial;

    private List<DraggableMeshPoint> m_meshPointHandles = new List<DraggableMeshPoint>();
    private List<Edge> m_edges = new List<Edge>();
    private List<DraggableMeshPoint> m_triangles = new List<DraggableMeshPoint>();

    private DraggableMeshPoint m_selectedPoint;
    private float m_zDistance = 0.0f;

    private string m_buttonPrimary = "Fire1";

    private Camera m_camera;
    private Plane m_moveToolPlane = new Plane();

    private int m_idOffset;

    void Awake()
    {
        m_idOffset = 0;
        m_camera = Camera.main;
    }

    void Start()
    {
        if(!Load())
        {
            AddCube();
        }
    }

    void OnApplicationQuit()
    {
        Save();
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

        m_triangles.Add(t11);
        m_triangles.Add(t1_1);
        m_triangles.Add(t_1_1);

        m_triangles.Add(t_1_1);
        m_triangles.Add(t_11);
        m_triangles.Add(t11);

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
        m_triangles.Add(t11a);
        m_triangles.Add(t_1_1a);
        m_triangles.Add(t1_1a);

        m_triangles.Add(t_1_1a);
        m_triangles.Add(t11a);
        m_triangles.Add(t_11a);


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
        m_triangles.Add(t_1_1);
        m_triangles.Add(t1_1);
        m_triangles.Add(t1_1a);

        m_triangles.Add(t1_1a);
        m_triangles.Add(t_1_1a);
        m_triangles.Add(t_1_1);

        // Positive Z
        m_triangles.Add(t11);
        m_triangles.Add(t_11);
        m_triangles.Add(t_11a);

        m_triangles.Add(t_11a);
        m_triangles.Add(t11a);
        m_triangles.Add(t11);

        // Negative X
        m_triangles.Add(t_11);
        m_triangles.Add(t_1_1);
        m_triangles.Add(t_1_1a);

        m_triangles.Add(t_1_1a);
        m_triangles.Add(t_11a);
        m_triangles.Add(t_11);

        // Positive X
        m_triangles.Add(t11);
        m_triangles.Add(t1_1a);
        m_triangles.Add(t1_1);

        m_triangles.Add(t1_1a);
        m_triangles.Add(t11);
        m_triangles.Add(t11a);
    }

    DraggableMeshPoint AddHandle(Vector3 position)
    {
        var instance = Instantiate(m_prefab, position, Quaternion.identity);
        m_meshPointHandles.Add(instance);
        instance.m_id = m_idOffset + m_meshPointHandles.Count;
        return instance;
    }

    void LateUpdate()
    {
        if(EventSystem.current.IsPointerOverGameObject())
        {
            m_selectedPoint = null;
            return;
        }

        if(m_selectedPoint == null)
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
        else
        {
            if(Input.GetButtonUp(m_buttonPrimary))
            {
                m_selectedPoint = null;
                return;
            }
            m_selectedPoint.Select();
            if(Input.GetButton(m_buttonPrimary))
            {
                m_selectedPoint.transform.position = m_camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_zDistance));
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

        if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if(Input.GetKeyDown(KeyCode.X))
            {
                ActionGenerateMesh();
            }
            if(Input.GetKeyDown(KeyCode.A))
            {
                ActionAddCube();
            }
            if(Input.GetKeyDown(KeyCode.C))
            {
                ActionClear();
            }
        }
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

    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        // f + v - e = 2
        // f = 2 - v + e
        Dictionary<DraggableMeshPoint, int> vertexIndexDict = new Dictionary<DraggableMeshPoint, int>();
        //List<Vector3> vertices = new List<Vector3>();
        Vector3[] vertices = new Vector3[m_meshPointHandles.Count];
        for(int i = 0; i < vertices.Length; i++)
        {
            vertexIndexDict.Add(m_meshPointHandles[i], i);
            vertices[i] = m_meshPointHandles[i].transform.position;
        }
        int[] triangles = new int[m_triangles.Count];
        for(int i = 0; i < m_triangles.Count; i++)
        {
            triangles[i] = vertexIndexDict[m_triangles[i]];
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    void Save()
    {
        FileStream file = File.Open(Application.dataPath + "/autosave.build", FileMode.OpenOrCreate);
        file.Seek(0, SeekOrigin.Begin);
        BinaryWriter bw = new BinaryWriter(file);
        Data.Serialize(m_meshPointHandles, m_edges, m_triangles, bw);
        bw.Close();
        file.Close();
    }

    bool Load()
    {
        if(File.Exists(Application.dataPath + "/autosave.build"))
        {
            try
            {
                using(FileStream file = File.Open(Application.dataPath + "/autosave.build", FileMode.Open))
                using(BinaryReader br = new BinaryReader(file))
                {
                    Data.Deserialize(out m_meshPointHandles, out m_edges, out m_triangles, br, m_prefab);
                }
                return true;
            }
            catch(Exception e)
            {
                Debug.LogError("Loading error: \r\n"+e);
                try
                {
                    File.Delete(Application.dataPath + "/autosave.build");
                }
                catch(Exception ee)
                {
                    Debug.LogError("Error deleting file: \r\n" + ee);
                }
            }
            
        }
        return false;
    }
}
