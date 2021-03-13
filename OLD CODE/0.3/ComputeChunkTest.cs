using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeChunkTest : MonoBehaviour
{
    public ComputeShader testShader;
    public int res = 16;
    public node[] nodes;
    public cube[] cubes;
    public bool test;

    private int nodesKernal;
    private int cubesKernal;

    void Start()
    {
        nodesKernal = testShader.FindKernel("ComputeNodes");
        cubesKernal = testShader.FindKernel("ComputeCubes");
    }

    // Update is called once per frame
    void Update()
    {
        if (test)
        {
            TestShader();
            test = false;
        }
    }

    void TestShader()
    {
        nodes = new node[res * res * res];
        cubes = new cube[res * res * res];
        ComputeBuffer nodeResult = new ComputeBuffer(res * res * res, 16);
        ComputeBuffer cubeResult = new ComputeBuffer(res * res * res, 20);
        testShader.SetBuffer(nodesKernal, "Nodes", nodeResult);
        testShader.SetBuffer(nodesKernal, "Cubes", cubeResult);
        testShader.SetInt("res", res);
        testShader.Dispatch(nodesKernal, res / 8, res / 8, res / 8);
        testShader.Dispatch(nodesKernal, res / 8, res / 8, res / 8);
        nodeResult.GetData(nodes);
        cubeResult.GetData(cubes);

        nodeResult.Dispose();
        cubeResult.Dispose();
    }

    private void OnDrawGizmos()
    {
        if(nodes != null)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].weight > 0.5f)
                    Gizmos.DrawSphere(nodes[i].pos, 0.1f);
            }
        }
    }
}

[System.Serializable]
public struct node
{
    public float weight;
    public Vector3 pos;
}

[System.Serializable]
public struct cube
{
    public Vector3 pos;
    public int nodes;
    public int cost;
}
