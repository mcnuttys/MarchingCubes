using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;

[System.Serializable]
public class Chunk
{
    // This is the chunk script which just contains the information of each chunk. 
    // When the chunk is put into a visual chunk is when the marching cubes magic occures...

    public Vector3Int pos;
    public Vector3Int size;
    private Node[,,] nodes;
    public Cube[,,] cubes;
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uv;

    public bool generated;
    public bool updated;

    public bool generating;
    public bool updating;
    
    public Chunk(Vector3Int pos, Vector3Int size)
    {
        this.pos = pos;
        this.size = size;
    }

    public void Generate()
    {
        if (!generating)
        {
            generating = true;

            nodes = new Node[size.x + 1, size.y + 1, size.z + 1];
            cubes = new Cube[size.x, size.y, size.z];

            vertices = new List<Vector3>();
            triangles = new List<int>();
            uv = new List<Vector2>();

            for (int x = 0; x <= size.x; x++)
            {
                for (int z = 0; z <= size.z; z++)
                {
                    float perlin = Mathf.PerlinNoise((pos.x + x + 1000) * 0.05f, (pos.z + z + 1000) * 0.05f) * 4f;

                    for (int y = 0; y <= size.y; y++)
                    {
                        float weight = (pos.y + y) - perlin - 32;

                        nodes[x, y, z] = new Node(x, y, z, weight);

                        // Now determine if we can make a cube in this cycle of the loop
                        if (x > 0 && y > 0 && z > 0)
                        {
                            Node n0 = nodes[x - 1, y - 1, z - 1];
                            Node n1 = nodes[x - 1, y - 1, z];
                            Node n2 = nodes[x, y - 1, z];
                            Node n3 = nodes[x, y - 1, z - 1];
                            Node n4 = nodes[x - 1, y, z - 1];
                            Node n5 = nodes[x - 1, y, z];
                            Node n6 = nodes[x, y, z];
                            Node n7 = nodes[x, y, z - 1];

                            int cubeIndex = CubeIndex(n0, n1, n2, n3, n4, n5, n6, n7);

                            if (cubeIndex > 0 && cubeIndex <= 255)
                            {
                                // Now for efficiencys sake we can also start calculating mesh stuff here.
                                // First get the triangles from the triangle table stored in the marching cubes area.
                                int[] tris = MarchingCubes.triTable[cubeIndex];
                                int vertexCount = vertices.Count;

                                // Then loop through the values in that tritable to assign the verticies in use
                                for (int i = 0; i < tris.Length; i++)
                                {
                                    // If the value in the table is -1 it is not in use so skip
                                    if (tris[i] == -1)
                                        break;

                                    // We do have a valid value so add that vertex to the array.
                                    // We can also add the uv and triangle information in pretty much the same way.
                                    vertices.Add(GetAntiNode(tris[i], n0, n1, n2, n3, n4, n5, n6, n7));
                                    triangles.Add(vertexCount + i);
                                    uv.Add(MarchingCubes.uv[cubeIndex][tris[i]]);
                                }
                            }
                        }
                    }
                }
            }

            generated = true;
            generating = false;
        }
    }

    public void Update()
    {
        if (!updating && !generating)
        {
            updating = true;

            if (!generated)
                return;

            // Initialize the arrays and lists.
            vertices = new List<Vector3>();
            triangles = new List<int>();
            uv = new List<Vector2>();

            // Loop through the cubes array to update the mesh...
            for (int x = 0; x <= size.x; x++)
            {
                for (int y = 0; y <= size.y; y++)
                {
                    for (int z = 0; z <= size.z; z++)
                    {
                        if (x > 0 && y > 0 && z > 0)
                        {
                            Node n0 = nodes[x - 1, y - 1, z - 1];
                            Node n1 = nodes[x - 1, y - 1, z];
                            Node n2 = nodes[x, y - 1, z];
                            Node n3 = nodes[x, y - 1, z - 1];
                            Node n4 = nodes[x - 1, y, z - 1];
                            Node n5 = nodes[x - 1, y, z];
                            Node n6 = nodes[x, y, z];
                            Node n7 = nodes[x, y, z - 1];

                            int cubeIndex = CubeIndex(n0, n1, n2, n3, n4, n5, n6, n7);

                            if (cubeIndex > 0 && cubeIndex <= 255)
                            {
                                // Now for efficiencys sake we can also start calculating mesh stuff here.
                                // First get the triangles from the triangle table stored in the marching cubes area.
                                int[] tris = MarchingCubes.triTable[cubeIndex];
                                int vertexCount = vertices.Count;

                                // Then loop through the values in that tritable to assign the verticies in use
                                for (int i = 0; i < tris.Length; i++)
                                {
                                    // If the value in the table is -1 it is not in use so skip
                                    if (tris[i] == -1)
                                        break;

                                    // We do have a valid value so add that vertex to the array.
                                    // We can also add the uv and triangle information in pretty much the same way.
                                    vertices.Add(GetAntiNode(tris[i], n0, n1, n2, n3, n4, n5, n6, n7));
                                    triangles.Add(vertexCount + i);
                                    uv.Add(MarchingCubes.uv[cubeIndex][tris[i]]);
                                }
                            }
                        }
                    }
                }
            }

            updated = true;
            updating = false;
        }
    }

    private int CubeIndex(Node n0, Node n1, Node n2, Node n3, Node n4, Node n5, Node n6, Node n7)
    {
        int cubeIndex = 0;
        if (n0.weight > 0) cubeIndex += 1;
        if (n1.weight > 0) cubeIndex += 2;
        if (n2.weight > 0) cubeIndex += 4;
        if (n3.weight > 0) cubeIndex += 8;
        if (n4.weight > 0) cubeIndex += 16;
        if (n5.weight > 0) cubeIndex += 32;
        if (n6.weight > 0) cubeIndex += 64;
        if (n7.weight > 0) cubeIndex += 128;
        return cubeIndex;
    }

    public Vector3 GetAntiNode(int index, Node n0, Node n1, Node n2, Node n3, Node n4, Node n5, Node n6, Node n7)
    {
        switch (index)
        {
            case 0:
                return Interpolate(n0, n1);
            case 1:
                return Interpolate(n1, n2);
            case 2:
                return Interpolate(n2, n3);
            case 3:
                return Interpolate(n3, n0);

            case 4:
                return Interpolate(n4, n5);
            case 5:
                return Interpolate(n5, n6);
            case 6:
                return Interpolate(n6, n7);
            case 7:
                return Interpolate(n7, n4);

            case 8:
                return Interpolate(n0, n4);
            case 9:
                return Interpolate(n1, n5);
            case 10:
                return Interpolate(n2, n6);
            case 11:
                return Interpolate(n3, n7);
        }

        return Vector3.zero;
    }

    public Vector3 Interpolate(Node n0, Node n1)
    {
        float t = Mathf.Clamp01(-n0.weight * (1 / (n1.weight - n0.weight)));
        return Vector3.Lerp(n0.pos, n1.pos, t);
    }

    public void ModifyNode(int x, int y, int z, float weight)
    {
        if (nodes != null)
        {
            // Make sure that the input is valid.
            if (x < 0 || y < 0 || z < 0) return;
            if (x > size.x || y > size.y || z > size.z) return;

            // It is so modify the value.
            if (nodes[x, y, z] != null)
                nodes[x, y, z].weight += weight;
        }
    }
}

public class Cube
{
    public int cubeIndex;
    public bool active;
    
    public Node n0, n1, n2, n3, n4, n5, n6, n7;

    public Cube(Node n0, Node n1, Node n2, Node n3, Node n4, Node n5, Node n6, Node n7)
    {
        this.n0 = n0;
        this.n1 = n1;
        this.n2 = n2;
        this.n3 = n3;
        this.n4 = n4;
        this.n5 = n5;
        this.n6 = n6;
        this.n7 = n7;

        ComputeCube();
    }

    public void ComputeCube()
    {
        cubeIndex = 0;
        if (n0.weight > 0) cubeIndex += 1;
        if (n1.weight > 0) cubeIndex += 2;
        if (n2.weight > 0) cubeIndex += 4;
        if (n3.weight > 0) cubeIndex += 8;
        if (n4.weight > 0) cubeIndex += 16;
        if (n5.weight > 0) cubeIndex += 32;
        if (n6.weight > 0) cubeIndex += 64;
        if (n7.weight > 0) cubeIndex += 128;
        if (cubeIndex > 0 && cubeIndex <= 255)
            active = true;
    }

    public Vector3 GetAntiNode(int index)
    {
        switch(index)
        {
            case 0:
                return Interpolate(n0, n1);
            case 1:
                return Interpolate(n1, n2);
            case 2:
                return Interpolate(n2, n3);
            case 3:
                return Interpolate(n3, n0);

            case 4:
                return Interpolate(n4, n5);
            case 5:
                return Interpolate(n5, n6);
            case 6:
                return Interpolate(n6, n7);
            case 7:
                return Interpolate(n7, n4);

            case 8:
                return Interpolate(n0, n4);
            case 9:
                return Interpolate(n1, n5);
            case 10:
                return Interpolate(n2, n6);
            case 11:
                return Interpolate(n3, n7);
        }

        return Vector3.zero;
    }

    public Vector3 Interpolate(Node n0, Node n1)
    {
        float t = Mathf.Clamp01(-n0.weight * (1 / (n1.weight - n0.weight)));
        return Vector3.Lerp(n0.pos, n1.pos, t);
    }
}

public class Node
{
    public int x, y, z;
    public float weight;

    public Node(int x, int y, int z, float weight)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.weight = weight;
    }

    public Vector3 pos { get { return new Vector3(x, y, z); } }
}
