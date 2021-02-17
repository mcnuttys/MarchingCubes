using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class FPSTest : MonoBehaviour
{
    private List<float> times;
    private float averageTime;
    private float lastTime;
    private Stopwatch s;

    // Start is called before the first frame update
    void Start()
    {
        times = new List<float>();
        s = new Stopwatch();
    }

    // Update is called once per frame
    void Update()
    {
        if (times != null)
        {
            if (times.Count > 10)
                times.RemoveAt(0);

            averageTime = 0;
            for (int i = 0; i < times.Count; i++)
            {
                averageTime += times[i];
            }
            averageTime /= times.Count;
            if (times.Count > 0)
                lastTime = times[times.Count - 1];
        }
    }

    void BigLoop()
    {
        s.Start();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                // Do somthing
                                Mathf.PerlinNoise(x * 0.05f, z * 0.05f);
                            }
                        }
                    }
                }
            }
        }

        s.Stop();
        times.Add(s.ElapsedMilliseconds);
        s.Reset();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        string text = $"Time: {lastTime}\nAverage Time: {averageTime}\nPast Times: {times.Count}";
        int c = (times.Count <= 5) ? times.Count : 5;
        for (int i = 0; i < c; i++)
        {
            text += $"\nPast Time {i}: {times[times.Count- 1 - i]}";
        }
        GUILayout.Box(text);

        if (GUILayout.Button("Run Test"))
            BigLoop();

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
