using System.Collections.Generic;
using UnityEngine;

public class Torus : MonoBehaviour {

    [SerializeField]
    private Material _material;

    private Mesh _mesh;

    public float r1, r2;

    void Awake () {
        var n = 20;

        _mesh = new Mesh();

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var normals = new List<Vector3>();

        // (1) トーラスの計算
        for (int i = 0; i <= n; i++) {
            var phi = Mathf.PI * 2.0f * i / n;
            var tr = Mathf.Cos(phi) * r2;
            var y = Mathf.Sin(phi) * r2;

            for (int j = 0; j <= n; j++) {
                var theta = 2.0f * Mathf.PI * j / n;
                var x = Mathf.Cos(theta) * (r1 + tr);
                var z = Mathf.Sin(theta) * (r1 + tr);

                vertices.Add(new Vector3(x, y, z));
                // (2) 法線の計算
                normals.Add(new Vector3(tr * Mathf.Cos(theta), y, tr * Mathf.Sin(theta)));
            }
        }

        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                var count = (n + 1) * j + i;
                // (3) 頂点インデックスを指定
                triangles.Add(count);
                triangles.Add(count + n + 2);
                triangles.Add(count + 1);

                triangles.Add(count);
                triangles.Add(count + n + 1);
                triangles.Add(count + n + 2);
            }
        }

        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.normals = normals.ToArray();

        _mesh.RecalculateBounds();
    }

    void Update () {
        Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);
    }

/*
    private void OnGUI() {
        if (GUI.Button(new Rect(20, 20, 100, 50), "Save Mesh")){
            SaveMesh();
        }
    }

    public string TorusPath;
    private void SaveMesh(){
        UnityEditor.AssetDatabase.CreateAsset (_mesh, TorusPath);
        UnityEditor.AssetDatabase.SaveAssets ();
    }
    */
}