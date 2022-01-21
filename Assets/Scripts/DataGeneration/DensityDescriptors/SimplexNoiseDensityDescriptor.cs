using System.Collections;
using System.Collections.Generic;
using NoiseTest;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "Densities/SimplexNoiseDensity")]
public class SimplexNoiseDensityDescriptor : BaseDensityDescriptor
{
    public int octaves;
    public float persistence;
    public float amplitude;
    public float lacunarity;
    public float frequency;
    public Vector3 frequencyScale = Vector3.one;
    public bool threeDimensional;
    public bool refreshSeed;

    OpenSimplexNoise openSimplexNoise;
    float[,,] cache;
    public int seed;
    public float randomSeedDivider = 10000;


    private void OnEnable()
    {
        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    public void Update()
    {
        if(refreshSeed)
        {
            refreshSeed = false;
            seed = UnityEngine.Random.Range(0, int.MaxValue);
        }
    }

    float CreateSimplexDensity(Vector3Int point)
    {
        float value = 0;
        if (threeDimensional)
        {
            float4 freq = new float4(frequency * frequencyScale.x, frequency * frequencyScale.y, frequency * frequencyScale.z, 1);
            float3 fpoint = new float3(point.x, point.y, point.z);
            Utilities.CalculateSNoise3D(seed / randomSeedDivider, octaves, freq, amplitude, lacunarity, persistence, fpoint, out value);
        }
        else
        {
            float2 fpoint = new float2(point.x, point.z);
            float3 freq = new float3(frequency * frequencyScale.x,frequency * frequencyScale.y,1);
            Utilities.CalculateSNoise2D(seed / randomSeedDivider, octaves, freq, amplitude, lacunarity, persistence, fpoint,out value);
        }
        return value;
    }

    public override float GetDensity(Vector3Int point)
    {
        return CreateSimplexDensity(point);
    }

    public override float GetDensity(Vector3Int point, Vector3Int position)
    {
        return CreateSimplexDensity(point + position);
    }

}
