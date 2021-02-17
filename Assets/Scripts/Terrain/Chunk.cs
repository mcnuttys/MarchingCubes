using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Vector3 chunkSize;
    public int subChunkCount = 16;
    public Material chunkMaterial;
    public MeshGenerator meshGenerator;

    private Vector3 pos;
    private SubChunk[] subChunks;
    private Vector3 subChunkSize;

    private void Start()
    {
        pos = transform.position;
        subChunks = new SubChunk[subChunkCount];
        subChunkSize = new Vector3(chunkSize.x, chunkSize.y / subChunkCount, chunkSize.z);

        GenerateSubChunks();
    }

    void GenerateSubChunks()
    {
        for (int i = 0; i < subChunkCount; i++)
        {
            float y = i * subChunkSize.y;
            Vector3 scPos = new Vector3(pos.x, y, pos.z);

            GameObject subChunk = new GameObject();
            subChunk.name = $"SubChunk @{scPos}";
            subChunk.transform.position = scPos;
            subChunk.transform.parent = transform;

            SubChunk sc = subChunk.AddComponent<SubChunk>();
            sc.position = scPos;
            sc.SetSize(subChunkSize);
            sc.SetMeshFilter(subChunk.AddComponent<MeshFilter>());
            MeshRenderer mr = subChunk.AddComponent<MeshRenderer>();
            mr.material = chunkMaterial;
            sc.SetMeshCollider(subChunk.AddComponent<MeshCollider>());
            meshGenerator.RequestGeneration(sc);
            subChunks[i] = sc;
        }
    }
}
