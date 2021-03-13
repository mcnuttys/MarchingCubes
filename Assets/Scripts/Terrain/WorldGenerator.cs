using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    // This is the world generator that handles the creation of new chunks and such

    [Header("World Settings")]
    public int renderDistance = 8;
    public Transform target;

    [Header("Chunk Settings")]
    public Vector3Int chunkSize = new Vector3Int(16, 16, 16);
    public List<Chunk> chunkList;

    private Dictionary<Vector3Int, Chunk> chunks;
    public Vector3Int currentChunk;
    private Vector3Int lastChunk;
    private ChunkThreader chunkThreader;
    private VisualChunkManager visualChunkManager;

    // Start is called before the first frame update
    void Start()
    {
        chunks = new Dictionary<Vector3Int, Chunk>();
        chunkList = new List<Chunk>();
        chunkThreader = GetComponent<ChunkThreader>();
        visualChunkManager = GetComponent<VisualChunkManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get the current chunk of the generation target...
        currentChunk = ConvertToChunkPosition(target.position);

        // If we have moved chunks then we need to generate the new chunks that we are in
        if (lastChunk != currentChunk)
        {
            // So start the Generate function at our new position and set our last position to our current one
            Generate(currentChunk, renderDistance);
            lastChunk = currentChunk;
        }
    }

    private Vector3Int ConvertToChunkPosition(Vector3 pos)
    {
        return new Vector3Int(Mathf.FloorToInt(pos.x / chunkSize.x) * chunkSize.x, Mathf.FloorToInt(pos.y / chunkSize.y) * chunkSize.y, Mathf.FloorToInt(pos.z / chunkSize.z) * chunkSize.z);
    }

    void Generate(Vector3Int pos, int renderDistance)
    {
        visualChunkManager.UpdateVisualChunks();

        // Loop through the surrounding chunks...
        for (int x = -renderDistance / 2; x < renderDistance / 2; x++)
        {
            for (int y = -renderDistance / 2; y < renderDistance / 2; y++)
            {
                for (int z = -renderDistance / 2; z < renderDistance / 2; z++)
                {
                    // Based off the start position determine what chunk we are refering too...
                    Vector3Int cPos = pos + new Vector3Int(x * chunkSize.x, y * chunkSize.y, z * chunkSize.z);

                    // Determine if there is already a generated chunk at this position...
                    if (!chunks.ContainsKey(cPos))
                    {
                        // If there isnt we need to generate a new one!
                        chunks[cPos] = new Chunk(cPos, chunkSize);
                        chunkList.Add(chunks[cPos]);
                        chunkThreader.RequestGeneration(chunks[cPos]);
                    }

                    // Now add this chunk to the pool to be considered to be drawn if it is not already.
                    visualChunkManager.DrawChunk(chunks[cPos]);
                }
            }
        }
    }

    public void ModifyTerrain(Vector3 pos, float r, float mw)
    {
        Dictionary<Vector3, Chunk> cToU = new Dictionary<Vector3, Chunk>();
        Vector3 nearestNode = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
        for (int x = (int)(nearestNode.x - r); x < (int)nearestNode.x + r; x++)
        {
            for (int y = (int)(nearestNode.y - r); y < (int)nearestNode.y + r; y++)
            {
                for (int z = (int)(nearestNode.z - r); z < (int)nearestNode.z + r; z++)
                {
                    Vector3 nPos = new Vector3(x, y, z);
                    float dist = Vector3.SqrMagnitude(nPos - nearestNode);

                    if (dist < r)
                    {
                        Vector3Int cPos = ConvertToChunkPosition(nPos);

                        if (chunks.ContainsKey(cPos))
                        {
                            Chunk c = chunks[cPos];
                            Vector3 inCPos = nPos - c.pos;
                            float w = mw;

                            c.ModifyNode((int)inCPos.x, (int)inCPos.y, (int)inCPos.z, w);

                            if (!cToU.ContainsKey(c.pos))
                                cToU.Add(c.pos, c);

                            if ((int)inCPos.x == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-chunkSize.x, 0, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if ((int)inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, -chunkSize.y, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if ((int)inCPos.z == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, 0, -chunkSize.z), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if ((int)inCPos.x == chunkSize.x)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(chunkSize.x, 0, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if ((int)inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, chunkSize.y, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if ((int)inCPos.z == chunkSize.z)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, 0, chunkSize.z), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            
                            if (inCPos.x == 0 && inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-chunkSize.x, -chunkSize.y, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if (inCPos.x == chunkSize.x && inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(chunkSize.x, -chunkSize.y, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if (inCPos.x == 0 && inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-chunkSize.x, chunkSize.y, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if (inCPos.x == chunkSize.x && inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(chunkSize.x, chunkSize.y, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if (inCPos.z == 0 && inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, -chunkSize.y, -chunkSize.z), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if (inCPos.z == chunkSize.z && inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, -chunkSize.y, chunkSize.z), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if (inCPos.z == 0 && inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, chunkSize.y, -chunkSize.z), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if (inCPos.z == chunkSize.z && inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, chunkSize.y, chunkSize.z), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            
                            if (inCPos.x == 0 && inCPos.z == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-chunkSize.x, 0, -chunkSize.z), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if (inCPos.x == 0 && inCPos.z == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-chunkSize.x, 0, chunkSize.z), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            
                            if (inCPos.x == 0 && inCPos.z == 0 && inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-chunkSize.x, -chunkSize.y, -chunkSize.z), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                            if (inCPos.x == 0 && inCPos.z == 0 && inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-chunkSize.x, chunkSize.y, chunkSize.z), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.pos))
                                        cToU.Add(c2.pos, c2);
                            }
                        }
                    }
                }
            }
        }

        foreach (Chunk chunk in cToU.Values)
        {
            chunkThreader.RequestUpdate(chunk);
        }
    }

    Chunk ModifyChunk(Vector3 nPos, Vector3Int cPos, float w)
    {
        if (chunks.ContainsKey(cPos))
        {
            Chunk c = chunks[cPos];
            Vector3 inChunk = nPos - c.pos;

            c.ModifyNode((int)inChunk.x, (int)inChunk.y, (int)inChunk.z, w);
            return c;
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        if (chunks != null)
        {
            for (int x = -renderDistance / 2; x < renderDistance / 2; x++)
            {
                for (int y = -renderDistance / 2; y < renderDistance / 2; y++)
                {
                    for (int z = -renderDistance / 2; z < renderDistance / 2; z++)
                    {
                        Vector3Int cPos = currentChunk + new Vector3Int(x * chunkSize.x, y * chunkSize.y, z * chunkSize.z);

                        if (chunks[cPos] != null)
                        {
                            Gizmos.color = (chunks[cPos].generated) ? Color.green : Color.red;

                            if (cPos == currentChunk)
                            {
                                Gizmos.color = Color.yellow;
                            }

                            Gizmos.DrawWireCube(cPos + chunkSize / 2, chunkSize - Vector3Int.one);
                        }
                    }
                }
            }
        }
    }
}
