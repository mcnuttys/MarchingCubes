using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class WorldGenerator : MonoBehaviour
{
    public int renderDistance = 8;
    public Vector3 chunkSize = new Vector3(16, 16, 16);
    public Transform focus;
    public Material chunkMaterial;

    private Dictionary<Vector3Int, Chunk> chunks;
    private VisualChunk[,,] visualChunks;

    private Vector3 chunkPos;
    private Vector3 lastChunkPos = Vector3.negativeInfinity;

    private Thread chunkGenerationThread;
    private Thread chunkUpdateThread;
    private Queue<Chunk> chunksToGenerate;
    private Queue<Chunk> chunksToUpdate;
    private Dictionary<Vector3, Chunk> dcGenerate;
    private Dictionary<Vector3, Chunk> dcUpdate;
    private Chunk cgChunk;
    private Chunk cuChunk;
    private bool cgRunning;
    private bool cuRunning;
    private float updateTimer;
    private float generationTimer;

    // Start is called before the first frame update
    void Start()
    {
        chunks = new Dictionary<Vector3Int, Chunk>();
        chunksToGenerate = new Queue<Chunk>();
        chunksToUpdate = new Queue<Chunk>();
        dcGenerate = new Dictionary<Vector3, Chunk>();
        dcUpdate = new Dictionary<Vector3, Chunk>();

        // Create the visualChunks... There will be no assigned chunks yet but that should be fine!
        visualChunks = new VisualChunk[renderDistance * 2, renderDistance * 2, renderDistance * 2];
        for (int x = 0; x < renderDistance * 2; x++)
        {
            for (int y = 0; y < renderDistance * 2; y++)
            {
                for (int z = 0; z < renderDistance * 2; z++)
                {
                    // Create the visual chunk object and set initial settings
                    GameObject vChunk = new GameObject();
                    vChunk.name = $"Visual Chunk: ({x}, {y}, {z})";
                    vChunk.transform.parent = transform;

                    // Add conponents and add to the array
                    visualChunks[x, y, z] = vChunk.AddComponent<VisualChunk>();
                    vChunk.AddComponent<MeshFilter>();
                    vChunk.AddComponent<MeshRenderer>().material = chunkMaterial;
                    vChunk.AddComponent<MeshCollider>();
                }
            }
        }

        StartCoroutine(Generate(ContainedChunk(new Vector3(0, 32, 0)), 1));
    }

    // Update is called once per frame
    void Update()
    {
        chunkPos = ContainedChunk(focus.position);
        
        // Determine if the focus has moved chunks.
        if(lastChunkPos != chunkPos)
        {
            // It has so generate/update the serrounding chunks.
            StartCoroutine(Generate(chunkPos, renderDistance));
            lastChunkPos = chunkPos;
        }

        if(chunksToGenerate.Count > 0 && !cgRunning)
        {
            cgChunk = chunksToGenerate.Dequeue();
            dcGenerate.Remove(cgChunk.position);
            chunkGenerationThread = new Thread(ChunkGenerationThread);
            chunkGenerationThread.Start();
            cgRunning = true;
            generationTimer = 0;
        }

        if (chunksToUpdate.Count > 0 && !cuRunning)
        {
            cuChunk = chunksToUpdate.Dequeue();
            dcUpdate.Remove(cuChunk.position);
            chunkUpdateThread = new Thread(ChunkUpdateThread);
            chunkUpdateThread.Start();
            cuRunning = true;
            updateTimer = 0;
        }

        if (cgRunning == true)
            generationTimer += Time.deltaTime;
        if (cuRunning == true)
            updateTimer += Time.deltaTime;

        if(updateTimer > 0.25)
        {
            chunkUpdateThread.Abort();
            chunksToUpdate.Enqueue(cuChunk);
            dcUpdate.Add(cuChunk.position, cuChunk);
            cuRunning = false;
        }
    }

    void ChunkGenerationThread()
    {
        cgChunk.Generate();
        cgRunning = false;
    }

    void ChunkUpdateThread()
    {
        cuChunk.Update();
        cuRunning = false;
    }
    
    /// <summary>
    /// Determines the chunk that contains the point.
    /// </summary>
    /// <param name="pos">The position to figure out which chunk contains.</param>
    /// <returns>The chunk that contains the position.</returns>
    public Vector3Int ContainedChunk(Vector3 pos)
    {
        return new Vector3Int(Mathf.FloorToInt(pos.x / chunkSize.x), Mathf.FloorToInt(pos.y / chunkSize.y), Mathf.FloorToInt(pos.z / chunkSize.z));
    }

    IEnumerator Generate(Vector3 pos, int renderDistance)
    {
        chunksToGenerate.Clear();
        dcGenerate.Clear();
        int rDistance = 1;
        while (rDistance <= renderDistance)
        {
            int i = 0, j = 0, k = 0;
            for (int x = (int)pos.x - rDistance; x < (int)pos.x + rDistance; x++)
            {
                for (int y = (int)pos.y - rDistance; y < (int)pos.y + rDistance; y++)
                {
                    for (int z = (int)pos.z - rDistance; z < (int)pos.z + rDistance; z++)
                    {
                        Vector3Int chunkPos = new Vector3Int(x, y, z);

                        // Check if this chunk has been generated before...
                        if (chunks.ContainsKey(chunkPos))
                        {
                            // It has so not much to do
                        }
                        else
                        {
                            // Generate it!
                            GenerateChunk(chunkPos);
                        }

                        if (i > 0 && j > 0 && k > 0 && i < renderDistance * 2 && j < renderDistance * 2 && k < renderDistance * 2)
                            visualChunks[i, j, k].UpdateMesh(chunks[chunkPos]);
                        k++;
                    }
                    k = 0;
                    j++;
                }
                k = 0;
                j = 0;
                i++;
            }

            rDistance++;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void GenerateChunk(Vector3Int pos)
    {
        if (!dcGenerate.ContainsKey(pos))
        {
            Chunk c = new Chunk(pos, chunkSize);
            chunks.Add(pos, c);
            c.updating = true;
            chunksToGenerate.Enqueue(c);
            dcGenerate.Add(pos, c);
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
                        Vector3Int cPos = ContainedChunk(nPos);

                        if (chunks.ContainsKey(cPos))
                        {
                            Chunk c = chunks[cPos];
                            Vector3 inCPos = nPos - c.position;
                            float w = mw;

                            c.ModifyNode((int)inCPos.x, (int)inCPos.y, (int)inCPos.z, w);

                            if (!cToU.ContainsKey(c.position))
                                cToU.Add(c.position, c);

                            if ((int)inCPos.x == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-1, 0, 0), w);
                                if(c2 != null)
                                if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if ((int)inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, -1, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if ((int)inCPos.z == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, 0, -1), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if ((int)inCPos.x == chunkSize.x)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(1, 0, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if ((int)inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, 1, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if ((int)inCPos.z == chunkSize.z)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, 0, -1), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }

                            if (inCPos.x == 0 && inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-1, -1, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if (inCPos.x == chunkSize.x && inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(1, -1, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if (inCPos.x == 0 && inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-1, 1, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if (inCPos.x == chunkSize.x && inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(1, 1, 0), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if (inCPos.z == 0 && inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, -1, -1), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if (inCPos.z == chunkSize.z && inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, -1, 1), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if (inCPos.z == 0 && inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, 1, -1), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if (inCPos.z == chunkSize.z && inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(0, 1, 1), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }

                            if (inCPos.x == 0 && inCPos.z == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-1, 0, -1), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if (inCPos.x == 0 && inCPos.z == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-1, 0, 1), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }

                            if (inCPos.x == 0 && inCPos.z == 0 && inCPos.y == 0)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-1, -1, -1), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                            if (inCPos.x == 0 && inCPos.z == 0 && inCPos.y == chunkSize.y)
                            {
                                Chunk c2 = ModifyChunk(nPos, cPos + new Vector3Int(-1, 1, 1), w);
                                if (c2 != null)
                                    if (!cToU.ContainsKey(c2.position))
                                    cToU.Add(c2.position, c2);
                            }
                        }
                    }
                }
            }
        }

        foreach (Chunk chunk in cToU.Values)
        {
            chunksToUpdate.Enqueue(chunk);
        }
    }

    Chunk ModifyChunk(Vector3 nPos, Vector3Int cPos, float w)
    {
        if (chunks.ContainsKey(cPos))
        {
            Chunk c = chunks[cPos];
            Vector3 inChunk = nPos - c.position;

            c.ModifyNode((int)inChunk.x, (int)inChunk.y, (int)inChunk.z, w);
            return c;
        }

        return null;
    }
    private void OnDrawGizmos()
    {
        if(chunks != null)
        {
            foreach (Chunk chunk in chunks.Values)
            {
                Gizmos.color = Color.green * 0.25f;
                Gizmos.DrawCube(chunk.position + chunkSize / 2f, chunkSize);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        GUILayout.Box($"Generation Timer: {generationTimer}\nUpdate Timer: {updateTimer}");

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
