using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UVTool : MonoBehaviour
{
    struct TriangleIndex
    {
        public MeshManager.Triangle m_triangle;
        public int m_index;
    }

    public Texture2D m_texture;
    public Material m_material;
    public Material m_backgroundMaterial;
    public Material m_lineMaterial;
    public bool m_active;
    public float m_meshSideLength;
    public LayerMask m_dragPointMask;
    public DraggableMeshPoint m_handleTemplate;

    private Mesh m_backgroundMesh;
    private Texture2D m_prevTexture;
    private bool m_wasActive;
    private MeshManager m_meshManager;
    private List<DraggableMeshPoint> m_dragHandles = new List<DraggableMeshPoint>();
    private List<MeshManager.Edge> m_edges = new List<MeshManager.Edge>();
    private Dictionary<DraggableMeshPoint, List<TriangleIndex>> m_dict = new Dictionary<DraggableMeshPoint, List<TriangleIndex>>();
    private DraggableMeshPoint m_selectedPoint;
    private string m_buttonPrimary = "Fire1";
    private float m_zDistance;

    void Awake()
    {
        m_meshManager = FindObjectOfType<MeshManager>();
    }

    void Update()
    {
        if(m_active != m_wasActive)
        {
            m_wasActive = m_active;
            if(!m_active)
            {
                CameraController.main.SetPerspective(45.0f);
                CameraController.main.ReleaseFixedPoint();
                CameraController.main.GetCamera("UV Handles Camera").enabled = false;
            }
            else
            {
                CameraController.main.SetOrthographic(4);
                CameraController.main.SetFixedPoint();
                CameraController.main.GetCamera("UV Handles Camera").enabled = true;
            }
        }
        if(m_active)
        {
            if(m_texture != m_prevTexture)
            {
                m_prevTexture = m_texture;
                m_backgroundMesh = CreateMeshForTexture(m_texture, 0.05f);
                m_material.mainTexture = m_texture;
            }

            if(m_texture != null && m_backgroundMesh != null)
            {
                Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(0, 0, 90), Quaternion.Euler(0, 180, 0), Vector3.one);
                Graphics.DrawMesh(m_backgroundMesh, matrix, m_material, gameObject.layer);
            }
            Matrix4x4 matrix2 = Matrix4x4.TRS(new Vector3(0, 0, 91), Quaternion.Euler(0, 180, 0), Vector3.one * 3);
            Graphics.DrawMesh(m_backgroundMesh,matrix2, m_backgroundMaterial, gameObject.layer);
        }
    }

    void LateUpdate()
    {
        if(!m_active)
            return;

        if(m_selectedPoint == null)
        {
            if(!EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
                            m_zDistance = m_selectedPoint.transform.position.z;
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
                    var vector = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_zDistance));
                    vector.z = m_zDistance;
                    m_selectedPoint.transform.position = vector;
                    foreach(var triangle in m_dict[m_selectedPoint])
                    {
                        triangle.m_triangle.m_uvs[triangle.m_index] = WorldToUv(vector);
                    }
                }
            }
        }

        for(int i = 0; i < m_edges.Count; i++)
        {
            var edge = m_edges[i];
            
            edge.m_renderer.SetPosition(0, edge.m_pointA.transform.position);
            edge.m_renderer.SetPosition(1, edge.m_pointB.transform.position);
        }
    }

    private Mesh CreateMeshForTexture(Texture2D texture, float unitsPerPixel)
    {
        Mesh mesh = new Mesh();
        var vertices = new Vector3[4];
        /*vertices[0] = new Vector3(-texture.width, texture.height) * unitsPerPixel * 0.5f;
        vertices[1] = new Vector3(texture.width, texture.height) * unitsPerPixel * 0.5f;
        vertices[2] = new Vector3(texture.width, -texture.height) * unitsPerPixel * 0.5f;
        vertices[3] = new Vector3(-texture.width, -texture.height) * unitsPerPixel * 0.5f;*/
        vertices[0] = new Vector3(-m_meshSideLength, m_meshSideLength)  * 0.5f;
        vertices[1] = new Vector3(m_meshSideLength, m_meshSideLength)   * 0.5f;
        vertices[2] = new Vector3(m_meshSideLength, -m_meshSideLength)  * 0.5f;
        vertices[3] = new Vector3(-m_meshSideLength, -m_meshSideLength) * 0.5f;
        var uv = new Vector2[4];
        uv[0] = new Vector2(1, 1);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);
        var triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 2;
        triangles[4] = 0;
        triangles[5] = 3;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.uv2 = uv;
        mesh.uv3 = uv;
        mesh.uv4 = uv;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    public void CreateHandles(List<MeshManager.Triangle> triangles)
    {
        m_dict.Clear();
        List<Vector3> vertices;
        List<Vector3> normals;
        Dictionary<int, Dictionary<DraggableMeshPoint, int>> smoothingGroups;
        GetVertices(triangles, out vertices, out normals, out smoothingGroups);

        while(vertices.Count > m_dragHandles.Count)
        {
            var go = Instantiate(m_handleTemplate);
            m_dragHandles.Add(go);
        }

        foreach(var handle in m_dragHandles)
        {
            handle.gameObject.SetActive(false);
        }

        // Lines
        while(triangles.Count * 3 > m_edges.Count)
        {
            MeshManager.Edge edge = new MeshManager.Edge();
            var go = new GameObject("UV Edge Renderer (copy)");
            go.transform.SetParent(transform);
            go.layer = gameObject.layer;
            edge.m_renderer = go.AddComponent<LineRenderer>();
            edge.m_renderer.startWidth = 0.04f;
            edge.m_renderer.endWidth = 0.04f;
            edge.m_renderer.sharedMaterial = m_lineMaterial;
            m_edges.Add(edge);
        }

        foreach(var edge in m_edges)
        {
            edge.m_renderer.gameObject.SetActive(false);
        }

        int edgeIndex = 0;

        foreach(var triangle in triangles)
        {
            for(int i = 0; i < triangle.m_uvs.Length; i++)
            {
                int vertexIndex = smoothingGroups[triangle.m_smoothGroupIndex][triangle.m_points[i]];
                if(!m_dragHandles[vertexIndex].gameObject.activeSelf)
                {
                    m_dragHandles[vertexIndex].gameObject.SetActive(true);
                    m_dragHandles[vertexIndex].transform.position = UvToWorld(triangle.m_uvs[i]);
                    m_dict.Add(m_dragHandles[vertexIndex], new List<TriangleIndex>());
                }
                m_dict[m_dragHandles[vertexIndex]].Add(new TriangleIndex
                {
                    m_index = i,
                    m_triangle = triangle
                });


                if(i % 3 == 0)
                {
                    m_edges[edgeIndex].m_pointA = m_dragHandles[vertexIndex];
                    m_edges[edgeIndex + 2].m_pointB = m_dragHandles[vertexIndex];
                }
                else if(i % 3 == 1)
                {
                    m_edges[edgeIndex].m_pointB = m_dragHandles[vertexIndex];
                    m_edges[edgeIndex + 1].m_pointA = m_dragHandles[vertexIndex];
                }
                else if(i % 3 == 2)
                {
                    m_edges[edgeIndex + 1].m_pointB = m_dragHandles[vertexIndex];
                    m_edges[edgeIndex + 2].m_pointA = m_dragHandles[vertexIndex];
                }
            }
            m_edges[edgeIndex].m_renderer.gameObject.SetActive(true);
            m_edges[edgeIndex + 1].m_renderer.gameObject.SetActive(true);
            m_edges[edgeIndex + 2].m_renderer.gameObject.SetActive(true);
            edgeIndex += 3;
        }
    }

    private Vector3 UvToWorld(Vector2 uv)
    {
        Vector3 pos = new Vector3(uv.x, uv.y) * m_meshSideLength;
        pos -= new Vector3(m_meshSideLength, m_meshSideLength) * 0.5f;
        pos.z = 40;
        return pos;
    }

    private Vector2 WorldToUv(Vector3 world)
    {
        Vector2 pos = new Vector2(world.x, world.y) + new Vector2(m_meshSideLength, m_meshSideLength) * 0.5f;
        pos /= m_meshSideLength;
        Debug.Log(world + " - " + pos);
        return pos;
    }

    public void Unwrap(ref List<MeshManager.Triangle> triangles)
    {
        DebugUtil.StartStopwatch("Unwrap");

        List<Vector3> vertices;
        List<Vector3> normals;
        Dictionary<int, Dictionary<DraggableMeshPoint, int>> smoothingGroups;
        GetVertices(triangles, out vertices, out normals, out smoothingGroups);

        // 6 faces
        Dictionary<int, Vector2>[] uvs = new Dictionary<int, Vector2>[6];
        for(int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Dictionary<int, Vector2>();
        }
        Vector3 min, max;
        //float minF, maxF;
        GetBoundingBox(vertices, out min, out max);
        //GetBoundingBox(vertices, out minF, out maxF);

        for(int i = 0; i < vertices.Count; i++)
        {
            Vector3 vec = vertices[i] - min;
            vec.x /= (max.x - min.x);
            vec.y /= (max.y - min.y);
            vec.z /= (max.z - min.z);

            float[] ang = new float[6];

            ang[0] = Vector3.Angle(Vector3.right, normals[i]);      //+x
            ang[1] = Vector3.Angle(Vector3.left, normals[i]);       //-x
            ang[2] = Vector3.Angle(Vector3.up, normals[i]);         //+y
            ang[3] = Vector3.Angle(Vector3.down, normals[i]);       //-y
            ang[4] = Vector3.Angle(Vector3.forward, normals[i]);    //+z
            ang[5] = Vector3.Angle(Vector3.back, normals[i]);       //-z

            int minIndex = 0;
            float minVal = 9999.0f;
            for(int k = 0; k < ang.Length; k++)
            {
                if(ang[k] < minVal)
                {
                    minVal = ang[k];
                    minIndex = k;
                }
            }

            Vector2 uv;

            switch(minIndex)
            {
                default:
                case 0:
                    uv.x = vec.z;
                    uv.y = vec.y;
                    break;
                case 1:
                    uv.x = 1.0f - vec.z;
                    uv.y = vec.y;
                    break;
                case 2:
                    uv.x = vec.x;
                    uv.y = vec.z;
                    break;
                case 3:
                    uv.x = vec.x;
                    uv.y = 1.0f - vec.z;
                    break;
                case 4:
                    uv.x = 1.0f - vec.x;
                    uv.y = vec.y;
                    break;
                case 5:
                    uv.x = vec.x;
                    uv.y = vec.y;
                    break;
            }

            uvs[minIndex][i] = uv;
        }

        // Pack UVs
        var combinedUvs = PackUVs(uvs, vertices.Capacity, 0.3f);

        // Update triangles with uvs
        foreach(var triangle in triangles)
        {
            for(int i = 0; i < triangle.m_points.Length; i++)
            {
                int index = smoothingGroups[triangle.m_smoothGroupIndex][triangle.m_points[i]];
                triangle.m_uvs[i] = combinedUvs[index];
            }
        }

        DebugUtil.EndStopwatch("Unwrap");
    }

    private Vector2[] PackUVs(Dictionary<int, Vector2>[] uvs, int uvCount, float startScale)
    {
        return PackUVs(uvs, uvCount, startScale, 25);
    }

    private Vector2[] PackUVs(Dictionary<int, Vector2>[] uvs, int uvCount, float startScale, int triesLeft)
    {
        DebugUtil.StartStopwatch("UV Packing");

        Vector2[] combinedUvs = new Vector2[uvCount];

        if(triesLeft == 0)
        {
            Debug.LogWarning("No tries left for UV packing");
            return combinedUvs;
        }

        Dictionary<int, Vector2>[] originalUVs = (Dictionary < int, Vector2>[])uvs.Clone();

        Rect[] rects = new Rect[uvs.Length];
        Rect[] startRects = new Rect[uvs.Length];

        for(int i = 0; i < uvs.Length; i++)
        {
            Vector2 min = new Vector2(9999, 9999);
            Vector2 max = new Vector2(-9999, -9999);
            var keys = new List<int>(uvs[i].Keys);
            foreach(var k in keys)
            {
                var uv = uvs[i][k];
                uv.Scale(new Vector2(startScale, startScale));

                if(uv.x < min.x)
                    min.x = uv.x;
                if(uv.y < min.y)
                    min.y = uv.y;

                if(uv.x > max.x)
                    max.x = uv.x;
                if(uv.y > max.y)
                    max.y = uv.y;

                uvs[i][k] = uv;
            }
            rects[i].min = min;
            rects[i].max = max;
            startRects[i].min = min;
            startRects[i].max = max;

            rects[i].position += new Vector2(0.01f, 0.01f) * i;
        }

        float stepSize = 0.08f;
        bool noOverlap = false;
        int iterations = 0;
        int maxIterations = 1000;
        
        while(!noOverlap && iterations < maxIterations)
        {
            noOverlap = true;
            iterations++;

            for(int i = 0; i < rects.Length; i++)
            {
                for(int k = 0; k < rects.Length; k++)
                {
                    if(k == i)
                        continue;

                    if(rects[i].Overlaps(rects[k]))
                    {
                        noOverlap = false;
                        // From k to i
                        Vector2 dif = rects[i].position - rects[k].position;

                        if(dif.sqrMagnitude > stepSize * stepSize)
                            dif = dif.normalized * stepSize;

                        Vector2 randOffset = Random.insideUnitCircle * 0.01f;
                        dif += randOffset;

                        if(dif.x < 0)
                        {
                            if(rects[i].xMin + dif.x < 0)
                                dif.x = 0 - rects[i].xMin;
                        }
                        else
                        {
                            if(rects[i].xMax + dif.x > 1.0)
                                dif.x = 1 - rects[i].xMax;
                        }
                        if(dif.y < 0)
                        {
                            if(rects[i].yMin + dif.y < 0)
                                dif.y = 0 - rects[i].yMin;
                        }
                        else
                        {
                            if(rects[i].yMax + dif.y > 1.0)
                                dif.y = 1 - rects[i].yMax;
                        }

                        rects[i].position += dif;
                    }
                }
            }
        }

        for(int i = 0; i < uvs.Length; i++)
        {
            foreach(var uv in uvs[i])
            {
                combinedUvs[uv.Key] = uv.Value + rects[i].position - startRects[i].position;
            }
        }
        

        DebugUtil.EndStopwatch("UV Packing");

        if(noOverlap)
        {
            Debug.Log("Succes packing UVs");
        }
        else
        {
            Debug.Log("Packing failed, retrying with smaller scale");
            combinedUvs = PackUVs(originalUVs, uvCount, startScale * 0.8f, triesLeft - 1);
        }

        return combinedUvs;
    }

    private void GetBoundingBox(List<Vector3> vertices, out Vector3 min, out Vector3 max)
    {
        max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        foreach(var vertex in vertices)
        {
            if(vertex.x < min.x)
                min.x = vertex.x;
            if(vertex.y < min.y)
                min.y = vertex.y;
            if(vertex.z < min.z)
                min.z = vertex.z;

            if(vertex.x > max.x)
                max.x = vertex.x;
            if(vertex.y > max.y)
                max.y = vertex.y;
            if(vertex.z > max.z)
                max.z = vertex.z;
        }
    }

    private void GetBoundingBox(List<Vector3> vertices, out float min, out float max)
    {
        max = float.MinValue;
        min = float.MaxValue;
        foreach(var vertex in vertices)
        {
            if(vertex.x < min)
                min = vertex.x;
            if(vertex.y < min)
                min = vertex.y;
            if(vertex.z < min)
                min = vertex.z;

            if(vertex.x > max)
                max = vertex.x;
            if(vertex.y > max)
                max = vertex.y;
            if(vertex.z > max)
                max = vertex.z;
        }
    }

    private void GetVertices(List<MeshManager.Triangle> triangleObjectList, out List<Vector3> vertices, out List<Vector3> normals, out Dictionary<int, Dictionary<DraggableMeshPoint, int>> smoothingGroups)
    {
        smoothingGroups = new Dictionary<int, Dictionary<DraggableMeshPoint, int>>();
        vertices = new List<Vector3>();
        normals = new List<Vector3>();

        for(int i = 0; i < triangleObjectList.Count; i++)
        {
            if(!smoothingGroups.ContainsKey(triangleObjectList[i].m_smoothGroupIndex))
            {
                smoothingGroups.Add(triangleObjectList[i].m_smoothGroupIndex, new Dictionary<DraggableMeshPoint, int>());
            }
            for(int j = 0; j < triangleObjectList[i].m_points.Length; j++)
            {
                if(!smoothingGroups[triangleObjectList[i].m_smoothGroupIndex].ContainsKey(triangleObjectList[i].m_points[j]))
                {
                    // Add vertex to vertices and smoothing group dictionary
                    vertices.Add(triangleObjectList[i].m_points[j].transform.position);
                    normals.Add(CalculateNormal(triangleObjectList[i].m_pointA.transform.position, triangleObjectList[i].m_pointB.transform.position, triangleObjectList[i].m_pointC.transform.position));
                    smoothingGroups[triangleObjectList[i].m_smoothGroupIndex].Add(triangleObjectList[i].m_points[j], vertices.Count - 1);
                }
                else
                {
                    normals[smoothingGroups[triangleObjectList[i].m_smoothGroupIndex][triangleObjectList[i].m_points[j]]] += (CalculateNormal(triangleObjectList[i].m_pointA.transform.position, triangleObjectList[i].m_pointB.transform.position, triangleObjectList[i].m_pointC.transform.position));
                }
            }
        }
    }

    private Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(b - a, c - a);
    }
}
