using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MeshFilter))]
public class MeshFilterEditorExtension : Editor
{
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if(GUILayout.Button("Dump Mesh"))
        {
			MeshFilter mf = (MeshFilter)target;
			ObjExporter.MeshToFile(mf, Application.dataPath + "/" + mf.name + "-" + mf.mesh.name + ".obj");
        }
    }
}