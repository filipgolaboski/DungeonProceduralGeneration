using System.Collections;
using System.Collections.Generic;
using NoiseTest;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/SphereDensity")]
public class SphereDensityDescriptor : BaseDensityDescriptor
{
    public int radius = 3;
    public Vector3 offset;


    float SDF_Sphere(Vector3 position, float radius){
        return position.magnitude-radius;
    }

    public override float GetDensity(Vector3Int point)
    {
        float length = Vector3.Distance(point+offset, position);
        return Mathf.Clamp01(((length-radius)/radius+1)*0.5f);
    }

    public override float GetDensity(Vector3Int point, Vector3Int position)
    {
        float length = Vector3.Distance(point+offset, position);
        return Mathf.Clamp01(((length - radius) / radius + 1) * 0.5f);
    }

}
