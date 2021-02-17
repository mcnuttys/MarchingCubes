using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable()]
public class Chunk
{
    public Vector3 position;
    public int sizeX, sizeY, sizeZ;

    public Node[,,] nodes;
    public Cube[,,] cubes;

    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector2> uv;

    public bool updating = true;
    public bool generating = true;
    public bool meshGenerated;

    private int offset = 10000;

    public Chunk()
    {
        position = Vector3.zero;
        sizeX = 16; sizeY = 16; sizeZ = 16;
    }

    public Chunk(Vector3 position, Vector3 chunkSize)
    {
        this.position = new Vector3(position.x * chunkSize.x, position.y * chunkSize.y, position.z * chunkSize.z);
        sizeX = (int)chunkSize.x;
        sizeY = (int)chunkSize.y;
        sizeZ = (int)chunkSize.z;
    }

    public void Generate()
    {
        updating = true;

        // Initialize the arrays and lists.
        nodes = new Node[sizeX + 1, sizeY + 1, sizeZ + 1];
        cubes = new Cube[sizeX, sizeY, sizeZ];
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uv = new List<Vector2>();

        // Loop through the nodes and cubes array to create the nodes and cubes...
        for (int x = 0; x <= sizeX; x++)
        {
            for (int y = 0; y <= sizeY; y++)
            {
                for (int z = 0; z <= sizeZ; z++)
                {
                    // First compute the node at this position (this will later be proper world gen info, you know what I mean?)
                    float perlin = Mathf.PerlinNoise((position.x + x + offset) / 20, (position.z + z + offset) / 20) * 8;
                    float weight = (position.y + y) - perlin - 32;

                    nodes[x, y, z] = new Node(new Vector3(x, y, z), weight);

                    // Now determine if we can make a cube in this cycle of the loop
                    if (x > 0 && y > 0 && z > 0)
                    {
                        // We can so create that cube and assign its values
                        Cube c = new Cube(
                            new Node[] {
                            nodes[x-1,y-1,z-1],
                            nodes[x-1,y-1,z],
                            nodes[x,y-1,z],
                            nodes[x,y-1,z-1],
                            nodes[x-1,y,z-1],
                            nodes[x-1,y,z],
                            nodes[x,y,z],
                            nodes[x,y,z-1]
                            }
                            );

                        // Add it into the cubes array
                        cubes[x - 1, y - 1, z - 1] = c;

                        if (c.active)
                        {
                            // Now for efficiencys sake we can also start calculating mesh stuff here.
                            // First get the triangles from the triangle table stored in the marching cubes area.
                            int[] tris = MarchingCubes.triTable[c.cubeIndex];
                            int vertexCount = vertices.Count;
                        
                            // Then loop through the values in that tritable to assign the verticies in use
                            for (int i = 0; i < tris.Length; i++)
                            {
                                // If the value in the table is -1 it is not in use so skip
                                if (tris[i] != -1)
                                {
                                    // We do have a valid value so add that vertex to the array.
                                    // We can also add the uv and triangle information in pretty much the same way.
                                    vertices.Add(c.antiNodes[tris[i]].pos);
                                    triangles.Add(vertexCount + i);
                                    uv.Add(MarchingCubes.uv[c.cubeIndex][tris[i]]);
                                }
                            }
                            meshGenerated = true;
                        }
                    }
                }
            }
        }

        updating = false;
        generating = false;
        // Theoretically this should be storing all the information it needs so its onto you now mr world generator!
        // In the future I am going to want to add an update method which will go through the cubes and update them and the lists.
    }

    public void Update()
    {
        updating = true;

        if (generating)
            return;

        // Initialize the arrays and lists.
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uv = new List<Vector2>();

        // Loop through the cubes array to update the mesh...
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    Cube c = cubes[x, y, z];
                    c.ComputeCube();

                    if (c.active)
                    {
                        // First get the triangles from the triangle table stored in the marching cubes area.
                        int[] tris = MarchingCubes.triTable[c.cubeIndex];
                        int vertexCount = vertices.Count;
                    
                        // Then loop through the values in that tritable to assign the verticies in use
                        for (int i = 0; i < tris.Length; i++)
                        {
                            // If the value in the table is -1 it is not in use so skip
                            if (tris[i] != -1)
                            {
                                // We do have a valid value so add that vertex to the array.
                                // We can also add the uv and triangle information in pretty much the same way.
                                vertices.Add(c.antiNodes[tris[i]].pos);
                                triangles.Add(vertexCount + i);
                                uv.Add(MarchingCubes.uv[c.cubeIndex][tris[i]]);
                            }
                        }
                        meshGenerated = true;
                    }
                }
            }
        }
        meshGenerated = false;
        updating = false;
    }

    public void GenerateMesh()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    Cube c = cubes[x, y, z];
                    
                    if (c.active)
                    {
                        // First get the triangles from the triangle table stored in the marching cubes area.
                        int[] tris = MarchingCubes.triTable[c.cubeIndex];
                        int vertexCount = vertices.Count;

                        // Then loop through the values in that tritable to assign the verticies in use
                        for (int i = 0; i < tris.Length; i++)
                        {
                            // If the value in the table is -1 it is not in use so skip
                            if (tris[i] != -1)
                            {
                                // We do have a valid value so add that vertex to the array.
                                // We can also add the uv and triangle information in pretty much the same way.
                                vertices.Add(c.antiNodes[tris[i]].pos);
                                triangles.Add(vertexCount + i);
                                uv.Add(MarchingCubes.uv[c.cubeIndex][tris[i]]);
                            }
                        }
                    }
                }
            }
        }

        meshGenerated = true;
    }

    public void ModifyNode(int x, int y, int z, float weight)
    {
        if (nodes != null)
        {
            // Make sure that the input is valid.
            if (x < 0 || y < 0 || z < 0) return;
            if (x > sizeX || y > sizeY || z > sizeZ) return;

            // It is so modify the value.
            if (nodes[x, y, z] != null)
                nodes[x, y, z].weight += weight;
        }
    }

    public Vector3[] GetVertices()
    {
        if (vertices != null)
            return vertices.ToArray();
        else return new Vector3[0];
    }
    public int[] GetTriangles()
    {
        if (triangles != null)
            return triangles.ToArray();
        else return new int[0];
    }
    public Vector2[] GetUv()
    {
        if (uv != null)
            return uv.ToArray();
        else return new Vector2[0];
    }

    public override string ToString()
    {
        return $"Chunk ({position.x}, {position.y}, {position.z})";
    }
}

public class Cube
{
    public Vector3 pos;
    public Node[] nodes;
    public AntiNode[] antiNodes;
    public int cubeIndex;
    public bool active;

    public Cube(Node[] nodes)
    {
        this.nodes = nodes;

        // Compute the cubePos using the nodes and use this to compute the cost of the cube
        pos = new Vector3();
        for (int i = 0; i < nodes.Length; i++)
        {
            pos += nodes[i].pos;
        }
        pos /= nodes.Length;

        ComputeCube();
    }

    public void ComputeCube()
    {
        cubeIndex = 0;
        if (nodes[0].weight > 0) cubeIndex += 1;
        if (nodes[1].weight > 0) cubeIndex += 2;
        if (nodes[2].weight > 0) cubeIndex += 4;
        if (nodes[3].weight > 0) cubeIndex += 8;
        if (nodes[4].weight > 0) cubeIndex += 16;
        if (nodes[5].weight > 0) cubeIndex += 32;
        if (nodes[6].weight > 0) cubeIndex += 64;
        if (nodes[7].weight > 0) cubeIndex += 128;
        if (cubeIndex > 0 && cubeIndex <= 255)
            active = true;

        if (active)
        {
            // Using the nodes compute the antinodes (The antinodes are going to be the vertex positions of the mesh)...
            antiNodes = new AntiNode[12];
            antiNodes[0] = new AntiNode(Vector3.Lerp(nodes[0].pos, nodes[1].pos, Interpolate(nodes[0], nodes[1])));
            antiNodes[1] = new AntiNode(Vector3.Lerp(nodes[1].pos, nodes[2].pos, Interpolate(nodes[1], nodes[2])));
            antiNodes[2] = new AntiNode(Vector3.Lerp(nodes[2].pos, nodes[3].pos, Interpolate(nodes[2], nodes[3])));
            antiNodes[3] = new AntiNode(Vector3.Lerp(nodes[3].pos, nodes[0].pos, Interpolate(nodes[3], nodes[0])));

            antiNodes[4] = new AntiNode(Vector3.Lerp(nodes[4].pos, nodes[5].pos, Interpolate(nodes[4], nodes[5])));
            antiNodes[5] = new AntiNode(Vector3.Lerp(nodes[5].pos, nodes[6].pos, Interpolate(nodes[5], nodes[6])));
            antiNodes[6] = new AntiNode(Vector3.Lerp(nodes[6].pos, nodes[7].pos, Interpolate(nodes[6], nodes[7])));
            antiNodes[7] = new AntiNode(Vector3.Lerp(nodes[7].pos, nodes[4].pos, Interpolate(nodes[7], nodes[4])));

            antiNodes[8] = new AntiNode(Vector3.Lerp(nodes[0].pos, nodes[4].pos, Interpolate(nodes[0], nodes[4])));
            antiNodes[9] = new AntiNode(Vector3.Lerp(nodes[1].pos, nodes[5].pos, Interpolate(nodes[1], nodes[5])));
            antiNodes[10] = new AntiNode(Vector3.Lerp(nodes[2].pos, nodes[6].pos, Interpolate(nodes[2], nodes[6])));
            antiNodes[11] = new AntiNode(Vector3.Lerp(nodes[3].pos, nodes[7].pos, Interpolate(nodes[3], nodes[7])));
        }
    }

    float Interpolate(Node n0, Node n1)
    {
        return Mathf.Clamp01(-n0.weight / (n1.weight - n0.weight));
    }
}

public class AntiNode
{
    public Vector3 pos;
    public AntiNode(Vector3 pos)
    {
        this.pos = pos;
    }
}

public class Node : AntiNode
{
    public float weight;
    public Node(Vector3 pos, float weight) : base(pos)
    {
        this.weight = weight;
    }
}
