using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CubeTool : MonoBehaviour
{
    private MeshFilter mf;
    private bool[] active = new bool[12];
    private Vector3[] verts =
    {
        new Vector3(0,    0,     0.5f),
        new Vector3(0.5f, 0,     1f),
        new Vector3(1f,   0,     0.5f),
        new Vector3(0.5f, 0,     0),

        new Vector3(0,    1,     0.5f),
        new Vector3(0.5f, 1,     1f),
        new Vector3(1f,   1,     0.5f),
        new Vector3(0.5f, 1,     0),

        new Vector3(0,    0.5f,  0),
        new Vector3(0,    0.5f,  1),
        new Vector3(1,    0.5f,  1),
        new Vector3(1,    0.5f,  0),
    };
    private Vector2[] uvs = new Vector2[12];
    private List<Vector2[]> savedUVs;

    public int cubeState = 0;
    private int lastCubeState;

    public Vector3 camPos;
    public Vector3 center;
    public Vector3 averageNormal;
    public float camDist = 2;

    private void Start()
    {
        mf = GetComponent<MeshFilter>();
        savedUVs = new List<Vector2[]>();
    }

    public void Update()
    {
        GenerateMesh();
        MoveCamera();

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i].x = Mathf.Clamp(uvs[i].x, 0, 1);
            uvs[i].y = Mathf.Clamp(uvs[i].y, 0, 1);
        }
    }

    public void GenerateMesh()
    {
        Mesh m = new Mesh();
        List<Vector3> v = new List<Vector3>();
        List<int> t = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        int[] tris = MarchingCubes.triTable[cubeState];
        active = new bool[12];
        for (int i = 0; i < tris.Length; i++)
        {
            if(tris[i] != -1)
            {
                active[tris[i]] = true;
                v.Add(verts[tris[i]]);
                t.Add(i);
                uv.Add(uvs[tris[i]]);
            }
        }

        m.vertices = v.ToArray();
        m.triangles = t.ToArray();
        m.uv = uv.ToArray();
        m.RecalculateNormals();
        mf.mesh = m;

        Vector3[] normals = m.normals;
        averageNormal = Vector3.zero;
        for (int i = 0; i < normals.Length; i++)
        {
            averageNormal += normals[i];
        }
        averageNormal /= normals.Length;

        center = Vector3.zero;
        int vertCount = 0;
        for (int i = 0; i < verts.Length; i++)
        {
            if (active[i])
            {
                center += verts[i];
                vertCount++;
            }
        }
        if (vertCount > 0)
            center /= vertCount;
        else
            center = Vector3.zero;

    }

    void MoveCamera()
    {
        if (averageNormal != Vector3.zero)
        {
            camDist += -Input.mouseScrollDelta.y * 0.25f;
            camPos = center + averageNormal.normalized * camDist;
            Camera.main.transform.position = camPos;
            Camera.main.transform.rotation = Quaternion.LookRotation(-averageNormal, Vector3.up);
        }
    }

    void SaveUVs()
    {
        SaveUV();
        StreamWriter sw = new StreamWriter("Saved_UV.txt", false);
        sw.WriteLine("public static Vector2[][] uv = new Vector2[][] {");
        for (int i = 0; i < savedUVs.Count; i++)
        {
            string line = "new Vector2[] { ";
            for (int j = 0; j < savedUVs[i].Length; j++)
            {
                line += $"new Vector2({savedUVs[i][j].x}f, {savedUVs[i][j].y}f)";
                if (j != savedUVs[i].Length - 1)
                    line += ", ";
            }
            line += "}";

            if (i != savedUVs.Count - 1)
                line += ",";

            sw.WriteLine(line);
        }
        sw.WriteLine("};");
        sw.Close();
    }

    void ReadUVs()
    {
        savedUVs = new List<Vector2[]>();
        bool readIn = false;
        StreamReader sr = new StreamReader("Saved_UV.txt");
        sr.ReadLine();
        string line = "";
        while ((line = sr.ReadLine()) != null) {
            if(line[0] != '}')
            {
                Vector2[] readArray = new Vector2[12];

                // Trim the excess of the line...
                line = line.Remove(0, 15);
                if (line[line.Length - 1] == ',')
                    line = line.Remove(line.Length - 1);
                line = line.Remove(line.Length - 1);

                string[] vectors = line.Split(')');
                for (int i = 0; i < vectors.Length; i++)
                {
                    if(vectors[i] != "")
                    {
                        if(vectors[i][0] == ',')
                        {
                            vectors[i] = vectors[i].Remove(0, 1);
                        }
                        vectors[i] = vectors[i].Remove(0, 13);

                        string[] values = vectors[i].Split(',');
                        readArray[i] = new Vector2(float.Parse(values[0].Trim('f')), float.Parse(values[1].Trim('f')));
                    }
                }
                readIn = true;
                savedUVs.Add(readArray);
            }
        }
        sr.Close();

        if(readIn && savedUVs.Count > cubeState)
        {
            uvs = savedUVs[cubeState];
        }
    }

    void SaveUV()
    {
        Vector2[] uvArray = new Vector2[12];
        uvs.CopyTo(uvArray, 0);

        if (cubeState > savedUVs.Count - 1)
        {
            savedUVs.Add(uvArray);
        }
        else
        {
            savedUVs[lastCubeState] = uvArray;
        }
    }

    void ApproxUV()
    {
        for (int i = 0; i < uvs.Length; i++)
        {
            if(active[i])
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(verts[i]);
                screenPos /= new Vector2(Screen.width, Screen.height);

                uvs[i] = screenPos;
            }
        }
    }

    void ApproxAll()
    {
        GenerateMesh();
        MoveCamera();
        ApproxUV();
        SaveUV();
        cubeState++;
        if (cubeState < 255)
        {
            ApproxAll();
        } else
        {
            Debug.Log("Completed Aprox All!");
            cubeState = 0;
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0,0,Screen.width,Screen.height));
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        
        // Cube Index
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-"))
        {
            cubeState--;
            if (cubeState >= 0)
            {
                if (savedUVs[cubeState] != null)
                    uvs = savedUVs[cubeState];
            }
        }
        GUILayout.Box(cubeState.ToString());
        if (GUILayout.Button("+"))
        {
            SaveUV();

            cubeState++;
        
            if(savedUVs.Count > cubeState)
            {
                if(savedUVs[cubeState] != null)
                {
                    uvs = savedUVs[cubeState];
                }
                else
                {
                    uvs = new Vector2[12];
                }
            } else
            {
                uvs = new Vector2[12];
            }
        }
        cubeState = Mathf.Clamp(cubeState, 0, 256);
        GUILayout.EndHorizontal();

        // UVs
        for (int i = 0; i < uvs.Length; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box(uvs[i].ToString());
            uvs[i].x = GUILayout.HorizontalSlider(uvs[i].x, 0, 1, GUILayout.MinWidth(50));
            uvs[i].y = GUILayout.HorizontalSlider(uvs[i].y, 0, 1, GUILayout.MinWidth(50));
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
            SaveUVs();

        if (GUILayout.Button("Read"))
            ReadUVs();
        GUILayout.EndHorizontal();

        if(GUILayout.Button("Approx."))
        {
            ApproxUV();
        }
        if (GUILayout.Button("Approx. All"))
        {
            ApproxAll();
        }

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void OnDrawGizmos()
    {
        Color[] c = new Color[] { Color.red, Color.green, Color.blue, Color.yellow };
        for (int i = 0; i < verts.Length; i++)
        {
            if (active[i])
            {
                Gizmos.color = c[i % c.Length];
                Gizmos.DrawSphere(verts[i], 0.1f);
            }
        }
    }

}
