using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    dirt,
    stone,
    grass,
    oreIron,
    oreCoal
}

public class TerrainGenerator : MonoBehaviour
{
    public int seed = 1234;
    public int x, y;
    public Block[] blocks;
    public AnimationCurve heightCurve;

    private float offset;
    private VisualChunkManager vcm;

    private System.Random blockRng;

    void Awake()
    {
        vcm = GetComponent<VisualChunkManager>();

        Random.InitState(seed);
        blockRng = new System.Random(seed);
        offset = Random.Range(1000, 999999);
    }

    public float GetHeight(float x, float y)
    {
        return heightCurve.Evaluate(Mathf.PerlinNoise((x + offset) * 0.05f, (y + offset) * 0.05f)) * 15f + 32f;
    }

    public BlockType GetBlock(float y, float surfaceHeight)
    {
        if (y >= surfaceHeight - 1)
            return BlockType.grass;

        if (y <= (surfaceHeight - 5))
        {
            if (y % 25 == 0)
                return BlockType.oreIron;
            if (y % 5 == 0)
                return BlockType.oreCoal;

            return BlockType.stone;
        }

        return BlockType.dirt;
    }
}

[System.Serializable()]
public class Block
{
    public BlockType type;
    public Texture2D texture;
    public Vector2 uvOffset;
}