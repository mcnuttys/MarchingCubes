using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum WorldMaterial
{
    dirt,
    stone
}

public class MeshGenerator : MonoBehaviour
{
    [Header("World Settings")]
    public int sizeX = 16;
    public int sizeY = 16;
    public int sizeZ = 16;
    public float surfaceHeight;
    public bool generate;

    [Header("Perlin Settings")]
    public float perlinScale = 16;
    public float perlinMagnitude = 8;

    [Header("Texture Stuff")]
    public WorldMat[] worldMaterials;

    private Node[,,] nodes;
    private Cube[,,] cubes;
    private List<Vector3> verticies;
    private List<int> triangles;
    private List<Vector2> uv;
    private List<Color> colors;

    public Texture2D textureAtlas;

    private Mesh mesh;
    private MeshFilter mf;
    private MeshCollider mc;

    // Start is called before the first frame update
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();

        // Pack the materials textures into an array
        List<Texture2D> textures = new List<Texture2D>();
        for (int i = 0; i < worldMaterials.Length; i++)
        {
            textures.Add(worldMaterials[i].texture);
        }

        textureAtlas = new Texture2D(16, 16);
        textureAtlas.PackTextures(textures.ToArray(), 0);
        textureAtlas.filterMode = FilterMode.Point;

        GetComponent<MeshRenderer>().material.mainTexture = textureAtlas;
    }

    // Update is called once per frame
    void Update()
    {
        if (generate)
        {
            mesh = GenerateMesh(transform.position);
            mf.mesh = mesh;
            mc.sharedMesh = mesh;

            generate = false;
        }

        if(Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if(hit.transform != null)
                {
                    Debug.DrawLine(Camera.main.transform.position - Vector3.up, hit.point, Color.red, 10f);
                    ReduceTerrain(hit.point, 5f);
                }
            }
        }
    }

    void ReduceTerrain(Vector3 pos, float r, float weight = 0)
    {
        Vector3 nodePos = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
        Debug.DrawLine(Camera.main.transform.position - Vector3.up, nodePos, Color.green, 10f);

        for (int x = (int)(nodePos.x - r); x <= (int)(nodePos.x + r); x++)
        {
            for (int y = (int)(nodePos.y - r); y <= (int)(nodePos.y + r); y++)
            {
                for (int z = (int)(nodePos.z - r); z <= (int)(nodePos.z + r); z++)
                {
                    if (x >= 0 && y >= 0 && z >= 0 && x <= sizeX && y <= sizeY && z <= sizeZ)
                    {
                        Node n = nodes[x, y, z];
                        float dist = Vector3.SqrMagnitude(transform.position + n.pos - pos);

                        if (weight != 0)
                        {
                            n.weight = weight;
                        }
                        else
                        {
                            if (dist < r)
                            {
                                n.weight += (r * r) / dist;
                            }
                        }
                    }
                }
            }
        }
        
        mesh = UpdateMesh();
        mf.mesh = mesh;
        mc.sharedMesh = mesh;
    }

    Mesh GenerateMesh(Vector3 startPos)
    {
        // Initialize mesh parameters...
        Mesh m = new Mesh();
        nodes = new Node[sizeX + 1, sizeY + 1, sizeZ + 1];
        cubes = new Cube[sizeX, sizeY, sizeZ];

        verticies = new List<Vector3>();
        triangles = new List<int>();
        uv = new List<Vector2>();
        colors = new List<Color>();

        // Start the generation process...
        // Generate the nodes and cubes.
        for (int x = 0; x <= sizeX; x++)
        {
            for (int y = 0; y <= sizeY; y++)
            {
                for (int z = 0; z <= sizeZ; z++)
                {
                    float perlin = Mathf.PerlinNoise((startPos.x + x) / perlinScale, (startPos.z + z) / perlinScale) * perlinMagnitude;
                    float weight = (startPos.y + y) - perlin - surfaceHeight;

                    nodes[x, y, z] = new Node(new Vector3(x, y, z), weight);
                    if (x > 0 && y > 0 && z > 0)
                    {
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
                            },
                        worldMaterials[Random.Range(0, 2)]
                        );
                        cubes[x - 1, y - 1, z - 1] = c;

                        int[] tris = MarchingCubes.triTable[c.cubeIndex];
                        int vertexCount = verticies.Count;

                        // What we are given is an array of ints that coorlate to the verticies of the cube.
                        for (int i = 0; i < tris.Length; i++)
                        {
                            if (tris[i] != -1)
                            {
                                Vector3 vert = c.antiNodes[tris[i]].pos;
                                verticies.Add(vert);
                                triangles.Add(vertexCount + i);
                                uv.Add((new Vector2((int)c.mat.mat, 0) + MarchingCubes.uv[c.cubeIndex][tris[i]]) / worldMaterials.Length);
                                colors.Add(c.mat.c);
                            }
                        }
                    }
                }
            }
        }

        // Apply the mesh parameters and return
        m.vertices = verticies.ToArray();
        m.triangles = triangles.ToArray();
        m.uv = uv.ToArray();
        m.colors = colors.ToArray();
        m.RecalculateNormals();
        return m;
    }

    Mesh UpdateMesh()
    {
        Mesh m = new Mesh();
        List<Vector3> v = new List<Vector3>();
        List<int> t = new List<int>();
        List<Vector2> u = new List<Vector2>();
        
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    Cube c = cubes[x, y, z];
                    c.ComputeCube();

                    int[] tris = MarchingCubes.triTable[c.cubeIndex];
                    int vertexCount = v.Count;

                    // What we are given is an array of ints that coorlate to the verticies of the cube.
                    for (int i = 0; i < tris.Length; i++)
                    {
                        if (tris[i] != -1)
                        {
                            Vector3 vert = c.antiNodes[tris[i]].pos;
                            v.Add(vert);
                            t.Add(vertexCount + i);
                            u.Add((new Vector2((int)c.mat.mat, 0) + MarchingCubes.uv[c.cubeIndex][tris[i]]) / worldMaterials.Length);
                        }
                    }
                }
            }
        }

        m.vertices = v.ToArray();
        m.triangles = t.ToArray();
        m.uv = u.ToArray();
        m.RecalculateNormals();
        return m;
    }
}

class Cube
{
    public Vector3 pos;
    public Node[] nodes;
    public AntiNode[] antiNodes;
    public int cubeIndex;

    public WorldMat mat;

    public Cube(Node[] nodes, WorldMat mat)
    {
        this.nodes = nodes;
        this.mat = mat;

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
        // Using the nodes compute the antinodes (The antinodes are going to be the vertex positions of the mesh)...
        antiNodes = new AntiNode[12];
        antiNodes[0] = new AntiNode(Vector3.Lerp(nodes[0].pos, nodes[1].pos, Mathf.Clamp01(-nodes[0].weight / (nodes[1].weight - nodes[0].weight))));
        antiNodes[1] = new AntiNode(Vector3.Lerp(nodes[1].pos, nodes[2].pos, Mathf.Clamp01(-nodes[1].weight / (nodes[2].weight - nodes[1].weight))));
        antiNodes[2] = new AntiNode(Vector3.Lerp(nodes[2].pos, nodes[3].pos, Mathf.Clamp01(-nodes[2].weight / (nodes[3].weight - nodes[2].weight))));
        antiNodes[3] = new AntiNode(Vector3.Lerp(nodes[3].pos, nodes[0].pos, Mathf.Clamp01(-nodes[3].weight / (nodes[0].weight - nodes[3].weight))));

        antiNodes[4] = new AntiNode(Vector3.Lerp(nodes[4].pos, nodes[5].pos, Mathf.Clamp01(-nodes[4].weight / (nodes[5].weight - nodes[4].weight))));
        antiNodes[5] = new AntiNode(Vector3.Lerp(nodes[5].pos, nodes[6].pos, Mathf.Clamp01(-nodes[5].weight / (nodes[6].weight - nodes[5].weight))));
        antiNodes[6] = new AntiNode(Vector3.Lerp(nodes[6].pos, nodes[7].pos, Mathf.Clamp01(-nodes[6].weight / (nodes[7].weight - nodes[6].weight))));
        antiNodes[7] = new AntiNode(Vector3.Lerp(nodes[7].pos, nodes[4].pos, Mathf.Clamp01(-nodes[7].weight / (nodes[4].weight - nodes[7].weight))));

        antiNodes[8] = new AntiNode(Vector3.Lerp(nodes[0].pos, nodes[4].pos, Mathf.Clamp01(-nodes[0].weight / (nodes[4].weight - nodes[0].weight))));
        antiNodes[9] = new AntiNode(Vector3.Lerp(nodes[1].pos, nodes[5].pos, Mathf.Clamp01(-nodes[1].weight / (nodes[5].weight - nodes[1].weight))));
        antiNodes[10] = new AntiNode(Vector3.Lerp(nodes[2].pos, nodes[6].pos, Mathf.Clamp01(-nodes[2].weight / (nodes[6].weight - nodes[2].weight))));
        antiNodes[11] = new AntiNode(Vector3.Lerp(nodes[3].pos, nodes[7].pos, Mathf.Clamp01(-nodes[3].weight / (nodes[7].weight - nodes[3].weight))));

        // Finally compute the cubes cubeIndex
        cubeIndex = 0;
        if (nodes[0].weight > 0) cubeIndex += 1;
        if (nodes[1].weight > 0) cubeIndex += 2;
        if (nodes[2].weight > 0) cubeIndex += 4;
        if (nodes[3].weight > 0) cubeIndex += 8;
        if (nodes[4].weight > 0) cubeIndex += 16;
        if (nodes[5].weight > 0) cubeIndex += 32;
        if (nodes[6].weight > 0) cubeIndex += 64;
        if (nodes[7].weight > 0) cubeIndex += 128;
    }
}

class AntiNode
{
    public Vector3 pos;
    public AntiNode(Vector3 pos)
    {
        this.pos = pos;
    }
}

class Node : AntiNode
{
    public float weight;
    public Node(Vector3 pos, float weight) : base(pos)
    {
        this.weight = weight;
    }
}

[System.Serializable]
public struct WorldMat
{
    public WorldMaterial mat;
    public Texture2D texture;
    public Color c;
}