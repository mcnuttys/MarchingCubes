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
    public float sealevel = 32f;
    public float x, y;
    public Block[] blocks;
    public PerlinLayer[] perlinLayers;

    private float offset;
    private VisualChunkManager vcm;

    void Awake()
    {
        vcm = GetComponent<VisualChunkManager>();

        Random.InitState(seed);
        offset = Random.Range(1000, 999999);
    }

    public float GetHeight(float x, float y)
    {
        float pValue = 0;
        for (int i = 0; i < perlinLayers.Length; i++)
        {
            pValue += perlinLayers[i].Evaluate(x, y, offset);
        }
        return pValue + sealevel;
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

[System.Serializable()]
public class PerlinLayer
{
    public float resolution;
    public float scale;

    public float Evaluate(float x, float y, float offset)
    {
        return Mathf.PerlinNoise((x + offset) * (1f / resolution), (y + offset) * (1f / resolution)) * scale;
    }
}