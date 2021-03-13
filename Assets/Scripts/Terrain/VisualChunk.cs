using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualChunk : MonoBehaviour
{
    public Chunk c;
    public bool chunkMeshUpdated;

    private Transform t;
    private MeshFilter mf;
    private MeshCollider mc;
    private Mesh m;

    private bool unload;

    private void Start()
    {
        t = transform;
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();
    }

    private void Update()
    {
        if(c != null)
        {
            if (!chunkMeshUpdated)
            {
                if (c.generated && !c.generating && !c.updating)
                {
                    UpdateMesh();
                }
            }

            if(c.updated)
            {
                c.updated = false;
                chunkMeshUpdated = false;
            }
        }

        if (unload)
        {
            m = new Mesh();
            mf.mesh = m;
            mc.sharedMesh = m;
            unload = false;
        }
    }

    private void UpdateMesh()
    {
        t.position = c.pos;
        m = new Mesh();
        m.vertices = c.vertices.ToArray();
        m.triangles = c.triangles.ToArray();
        m.uv = c.uv.ToArray();
        m.RecalculateNormals();

        mf.mesh = m;
        mc.sharedMesh = m;

        chunkMeshUpdated = true;
    }

    public void UnloadMesh()
    {
        unload = true;
    }
}
