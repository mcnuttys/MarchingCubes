using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    public int renderDistance = 8;
    public Transform target;

    [Header("Chunk Settings")]
    public Vector3 chunkSize = new Vector3(16, 128, 16);
    public int subChunkCount = 8;
    public Material chunkMaterial;

    private Dictionary<Vector2, Chunk> chunks;
    private Vector2 currentChunk;
    private Vector2 lastChunk;

    private MeshGenerator meshGenerator;

    private void Start()
    {
        meshGenerator = GetComponent<MeshGenerator>();
        chunks = new Dictionary<Vector2, Chunk>();

        StartCoroutine(Generate(Vector2.zero, 4));
    }

    private void Update()
    {
        // Get the current chunk of the generation target...
        currentChunk = ConvertToChunkPosition(target.position);

        // If we have moved chunks then we need to generate the new chunks that we are in
        if(lastChunk != currentChunk)
        {
            // So start the Generate function at our new position and set our last position to our current one
            StartCoroutine(Generate(currentChunk, renderDistance));
            UpdateChunks();
            lastChunk = currentChunk;
        }
    }

    private Vector2 ConvertToChunkPosition(Vector3 pos)
    {
        return new Vector2(Mathf.Floor(pos.x / chunkSize.x) * chunkSize.x, Mathf.Floor(pos.z / chunkSize.z) * chunkSize.z);
    }

    IEnumerator Generate(Vector2 pos, int generationDistance)
    {
        // This is our chunk generation code...
        // The goal of this method is to generate chunks around a given position in an expanding square off of the starting position
        // As we want to generate about a specific point start the gDistance at 0
        int gDistance = 0;

        // In the loop we want to go in a square about the center.
        // To do that we need a x and a y value which will all us to determine where a chunk is being generated
        int x = 0, y = 0;

        // Loop while the gDistance is less then the generation distance, each increase is another section of our square
        // At the end of each iteration we want to wait for a moment, this will either be a fixed time step or the end of frame...
        while (gDistance < generationDistance)
        {
            if (gDistance == 0)
            {
                GenerateChunk(pos + new Vector2(x * chunkSize.x, y * chunkSize.z));
            }
            else
            {
                // In each iteration we want to start at the top left cornor of the square so set x and y accordingly
                x = -gDistance+1;
                y = -gDistance;

                // Now to move in said square we need to loop which will end when we have closed said sqaure which will be at the spot imediately below the one we are at.
                while (x != -gDistance || y != -gDistance)
                {
                    // Save the position
                    Vector2 cPos = pos + new Vector2(x * chunkSize.x, y * chunkSize.z);

                    // Now that we have a spot generate the chunk.
                    GenerateChunk(cPos);

                    // Move around the square depending on where you are
                    if (y == -gDistance && x < gDistance)
                        x++;
                    else if (x == gDistance && y < gDistance)
                        y++;
                    else if (y == gDistance && x > -gDistance)
                        x--;
                    else if (x == -gDistance && y > -gDistance)
                        y--;

                    // Once again inside this loop just wait a bit before the next
                    yield return new WaitForSeconds(0.05f);
                }
                GenerateChunk(pos + new Vector2(x * chunkSize.x, y * chunkSize.z));
            }

            gDistance++;
            yield return new WaitForSeconds(0.05f);
        }
    }

    void GenerateChunk(Vector2 pos)
    {
        // Determine if there is already a chunk at this position...
        // And if not then create and add it
        // Otherwise do nothing
        if(!chunks.ContainsKey(pos))
        {
            GameObject chunk = new GameObject();
            chunk.name = $"Chunk @{pos}";
            chunk.transform.position = new Vector3(pos.x, 0, pos.y);
            chunk.transform.parent = transform;

            Chunk c = chunk.AddComponent<Chunk>();
            c.chunkSize = chunkSize;
            c.subChunkCount = subChunkCount;
            c.chunkMaterial = chunkMaterial;
            c.meshGenerator = meshGenerator;
            chunks.Add(pos, c);
        }
    }

    void UpdateChunks()
    {
        // Loop through the surrounding chunks...
        for (int x = -renderDistance - 5; x <= renderDistance + 5; x++)
        {
            for (int y = -renderDistance - 5; y <= renderDistance + 5; y++)
            {
                Vector2 cPos = new Vector2(lastChunk.x + x * chunkSize.x, lastChunk.y + y * chunkSize.z);
                if (chunks.ContainsKey(cPos))
                {
                    if (Vector2.Distance(currentChunk, cPos) > renderDistance * chunkSize.x)
                        chunks[cPos].gameObject.SetActive(false);
                    else
                        chunks[cPos].gameObject.SetActive(true);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (chunks != null)
        {
            foreach (Chunk chunk in chunks.Values)
            {
                Gizmos.color = Color.white;
                if (new Vector3(currentChunk.x, 0, currentChunk.y) == chunk.transform.position)
                    Gizmos.color = Color.green;

                Gizmos.DrawWireCube(chunk.transform.position + chunkSize / 2, chunkSize - Vector3.one);
            }
        }
    }
}
