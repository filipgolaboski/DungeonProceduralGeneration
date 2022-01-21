using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/CubeDensity")]
public class CubeDensityDescriptor : BaseDensityDescriptor
{
    float SDF_Cube(Vector3 p, Vector3 b){
        Vector3 q = absVector(p)-b;
        return Vector3.Magnitude(Vector3.Max(q, Vector3.zero)) + Mathf.Min(Mathf.Max(q.x,Mathf.Max(q.y,q.z)),0.0f);
    }

    Vector3 absVector(Vector3 v){
        return new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z));
    }


    public override float GetDensity(Vector3Int point)
    {
        float cube = SDF_Cube(point - position, size);
        cube = (cube + 1) * 0.5f;
        return Mathf.Clamp01(cube);
    }


}
