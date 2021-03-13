using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ChunkThreader : MonoBehaviour
{
    // This is the multithreaded chunk generator...
    public int maxGenerationThreads = 5;
    public int maxUpdateThreads = 5;
    public float maxGenerationTime = 1;
    public float maxUpdateTime = 1;

    private MarchingThread[] generationThreads;
    private MarchingThread[] updateThreads;
    private Queue<Chunk> chunksToGenerate;
    private Queue<Chunk> chunksToUpdate;

    private void Start()
    {
        generationThreads = new MarchingThread[maxGenerationThreads];
        updateThreads = new MarchingThread[maxUpdateThreads];
        chunksToGenerate = new Queue<Chunk>();
        chunksToUpdate = new Queue<Chunk>();
        
        for (int i = 0; i < maxGenerationThreads; i++)
        {
            generationThreads[i] = new MarchingThread();
        }
        for (int i = 0; i < maxUpdateThreads; i++)
        {
            updateThreads[i] = new MarchingThread();
        }
    }

    private void Update()
    {
        for (int i = 0; i < maxGenerationThreads; i++)
        {
            if (!generationThreads[i].Active)
            {
                if (chunksToGenerate.Count > 0)
                    generationThreads[i].GenerateThread(chunksToGenerate.Dequeue());
            }
            else
            {
                generationThreads[i].activeTimer += Time.deltaTime;

                if(generationThreads[i].activeTimer > maxGenerationTime)
                {
                    generationThreads[i].ResetGeneration();
                }
            }
        }
        for (int i = 0; i < maxUpdateThreads; i++)
        {
            if (!updateThreads[i].Active)
            {
                if (chunksToUpdate.Count > 0)
                    generationThreads[i].UpdateThread(chunksToUpdate.Dequeue());
            }
            else
            {
                updateThreads[i].activeTimer += Time.deltaTime;

                if (updateThreads[i].activeTimer > maxUpdateTime)
                {
                    updateThreads[i].ResetUpdate();
                }
            }
        }
    }

    public void RequestGeneration(Chunk c)
    {
        chunksToGenerate.Enqueue(c);
    }
    public void RequestUpdate(Chunk c)
    {
        chunksToUpdate.Enqueue(c);
    }
}

class MarchingThread
{
    public Thread thread;
    public float activeTimer;

    public Chunk c;

    public void GenerateThread(Chunk c)
    {
        this.c = c;
        activeTimer = 0;
        thread = new Thread(c.Generate);
        thread.Start();
    }

    public void UpdateThread(Chunk c)
    {
        this.c = c;
        activeTimer = 0;
        thread = new Thread(c.Update);
        thread.Start();
    }

    public void ResetGeneration()
    {
        thread.Abort();
        GenerateThread(c);
    }
    public void ResetUpdate()
    {
        thread.Abort();
        UpdateThread(c);
    }

    public bool Active { get { return (thread != null) ? thread.IsAlive : false; } }
}