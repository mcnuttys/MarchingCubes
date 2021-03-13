using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualChunkManager : MonoBehaviour
{
    public Material chunkMaterial;

    private WorldGenerator worldGenerator;
    private Dictionary<Vector3, bool> drawnChunks;
    private List<VisualChunk> visualChunks;
    private Queue<VisualChunk> visualChunkPool;

    private int renderDistance;

    private void Awake()
    {
        visualChunks = new List<VisualChunk>();
        visualChunkPool = new Queue<VisualChunk>();
        drawnChunks = new Dictionary<Vector3, bool>();

        worldGenerator = GetComponent<WorldGenerator>();
        renderDistance = worldGenerator.renderDistance + 1;

        // Setup the visual chunk pool
        for (int i = 0; i < renderDistance * renderDistance * renderDistance; i++)
        {
            GameObject visualChunk = new GameObject();
            visualChunk.gameObject.name = $"Visual Chunk {i}";
            visualChunk.transform.position = Vector3.zero;
            visualChunk.transform.parent = transform;
            visualChunk.AddComponent<MeshFilter>();
            visualChunk.AddComponent<MeshRenderer>().material = chunkMaterial;
            visualChunk.AddComponent<MeshCollider>();
            visualChunkPool.Enqueue(visualChunk.AddComponent<VisualChunk>());
        }
    }

    public void DrawChunk(Chunk c)
    {
        if (!drawnChunks.ContainsKey(c.pos))
        {
            // First determine if we are going to draw this chunk
            if (Distance(c) < (worldGenerator.chunkSize.x * worldGenerator.renderDistance) * 0.45)
            {
                // Dequeue from the chunk pool
                VisualChunk vc = visualChunkPool.Dequeue();

                // Set the vc chunk, and tell it to update mesh
                vc.c = c;
                vc.chunkMeshUpdated = false;

                // Then finally add the chunk to the active visual chunk list
                visualChunks.Add(vc);

                drawnChunks.Add(c.pos, true);
            }
        } else
        {
            if(!drawnChunks[c.pos])
            {
                // Dequeue from the chunk pool
                VisualChunk vc = visualChunkPool.Dequeue();

                // Set the vc chunk, and tell it to update mesh
                vc.c = c;
                vc.chunkMeshUpdated = false;

                // Then finally add the chunk to the active visual chunk list
                visualChunks.Add(vc);

                drawnChunks[c.pos] = true;
            }
        }
    }

    public void UpdateVisualChunks()
    {
        for (int i = 0; i < visualChunks.Count; i++)
        {
            VisualChunk vc = visualChunks[i];

            if (vc != null)
            {
                if (Distance(vc) >= (worldGenerator.chunkSize.x * renderDistance) * 0.45)
                {
                    if (drawnChunks.ContainsKey(visualChunks[i].c.pos))
                        drawnChunks[visualChunks[i].c.pos] = false;
                    else
                        drawnChunks.Add(visualChunks[i].c.pos, false);


                    visualChunks[i].c = null;
                    visualChunks[i].UnloadMesh();

                    visualChunks.RemoveAt(i);
                    visualChunkPool.Enqueue(vc);
                    i--;
                }
            } else
            {
                visualChunks[i].c = null;
                visualChunks[i].UnloadMesh();

                visualChunks.RemoveAt(i);
                visualChunkPool.Enqueue(vc);
                i--;
            }
        }
    }

    float Distance(VisualChunk vc)
    {
        if (vc.c != null)
            return Vector3.Distance(worldGenerator.currentChunk + worldGenerator.chunkSize / 2, vc.c.pos + worldGenerator.chunkSize / 2);

        return 100000;
    }
    float Distance(Chunk c)
    {
        if (c != null)
            return Vector3.Distance(worldGenerator.currentChunk + worldGenerator.chunkSize / 2, c.pos + worldGenerator.chunkSize / 2);

        return 100000;
    }
}
