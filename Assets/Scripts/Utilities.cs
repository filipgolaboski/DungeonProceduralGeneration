using NoiseTest;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public static class Utilities
{

    [BurstCompile]
    public static void CalculateSNoise2D(float seed,int octaves,in float3 frequency, float amplitude, float lacunarity,float persistence ,in float2 point,out float total)
    {
        total = 0;
        float maxValue = 0;
        float3 freq = frequency;
        float3 value = new float3(point.x, point.y,seed);
        for (int i = 0; i < octaves; i++)
        { 
            total = total + noise.snoise(value*freq) * amplitude;
            maxValue += amplitude;
            amplitude = amplitude * persistence;
            freq = freq * lacunarity;
        }

        total = total / maxValue;
        total = (total + 1) * 0.5f;
    }


    [BurstCompile]
    public static void CalculateSNoise3D(float seed, int octaves,in float4 frequency, float amplitude, float lacunarity, float persistence, in float3 point, out float total)
    {
        float4 noise_input = new float4(point, seed);
        float maxVal = 0;
        float amp = amplitude;
        float4 freq = frequency;
        total = 0;
        for(int i=0;i<octaves;i++)
        {
            total += noise.snoise(noise_input * freq) * amp;
            maxVal += amp;
            amp *= persistence;
            freq *= lacunarity;
        }
        total /= maxVal;
        total = (total + 1) * 0.5f;

    }

    [BurstCompile]
    public static void Union_Smooth(float d1,float d2,float smoothing,out float result)
    {
        float h = Mathf.Clamp(0.5f + .5f * (d2 - d1) / smoothing, 0f, 1f);
        result = Mathf.Lerp(d2, d1, h) + smoothing * h * (1 - h);
    }

    [BurstCompile]
    public static void Substraction_Smooth(float d1, float d2, float smoothing, out float result)
    {
        float h = Mathf.Clamp(0.5f - .5f * (d2 + d1) / smoothing, 0f, 1f);
        result = Mathf.Lerp(d2,1-d1, h) + smoothing * h * (1 - h);
    }

    [BurstCompile]
    public static void Intersection_Smooth(float d1, float d2, float smoothing, out float result)
    {
        float h = Mathf.Clamp(0.5f - .5f * (d2 - d1) / smoothing, 0f, 1f);
        result =  Mathf.Lerp(d2, d1, h) + smoothing * h * (1 - h);
    }

}
