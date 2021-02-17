using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MeshGenerator : MonoBehaviour
{
    public Queue<SubChunk> generationRequests = new Queue<SubChunk>();
    public float generationTimeout = 0.25f;
    public int maxGenerationThreads = 4;

    private Thread[] generationThreads;
    private SubChunk[] generatingChunks;
    private float generationTimer;

    private void Start()
    {
        generationThreads = new Thread[maxGenerationThreads];
        generatingChunks = new SubChunk[maxGenerationThreads];
    }

    private void Update()
    {
        if(generationRequests.Count > 0 && generationTimer <= 0)
        {
            for (int i = 0; i < maxGenerationThreads; i++)
            {
                if (generationRequests.Count > 0)
                {
                    if (generationThreads[i] == null)
                    {
                        StartThread(i);
                    }
                    else if (!generationThreads[i].IsAlive)
                    {
                        StartThread(i);
                    }
                }
            }
            generationTimer = 0.01f;
        }

        if (generationTimer > 0)
            generationTimer -= Time.deltaTime;
    }

    void StartThread(int i)
    {
        generatingChunks[i] = generationRequests.Dequeue();
        generationThreads[i] = new Thread(generatingChunks[i].Generate);
        generationThreads[i].Start();
    }

    public void RequestGeneration(SubChunk chunk)
    {
        generationRequests.Enqueue(chunk);
    }
}