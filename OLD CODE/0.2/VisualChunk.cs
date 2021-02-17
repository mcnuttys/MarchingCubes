using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualChunk : MonoBehaviour
{
    private Chunk chunk;
    private Mesh m;
    private MeshFilter mf;
    private MeshCollider mc;
    private bool updating;

    void Start()
    {
        m = new Mesh();
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();
    }
    private void Update()
    {
        if (chunk != null)
        {
            if (chunk.updating)
                updating = true;

            if (updating)
            {
                if (chunk.updating == false)
                {
                    transform.position = new Vector3(chunk.position.x, chunk.position.y, chunk.position.z);

                    m = new Mesh();
                    m.vertices = chunk.GetVertices();
                    m.triangles = chunk.GetTriangles();
                    m.uv = chunk.GetUv();
                    m.RecalculateNormals();
                    mf.mesh = m;
                    mc.sharedMesh = m;
                    updating = false;
                }
            }
        }
    }

    public void UpdateMesh(Chunk c)
    {
        chunk = c;
        updating = true;
    }
}
