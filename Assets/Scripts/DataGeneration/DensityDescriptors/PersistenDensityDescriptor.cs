using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/PersistentDensityDescriptor")]
public class PersistenDensityDescriptor : BaseDensityDescriptor
{
    float[,,] density;

    public override float GetDensity(Vector3Int point)
    {
        return density[point.x, point.y, point.z];
    }

    public override float GetDensity(Vector3Int point, Vector3Int position)
    {
        return GetDensity(point+position);
    }

    public void Init()
    {
        density = new float[size.x, size.y, size.z];
    }

    public void AddDensity(Vector3Int point,float value)
    {
        if (point.x >= 0 && point.x < size.x && point.y >= 0 && point.y < size.y && point.z >= 0 && point.z < size.z)
        {
            density[point.x, point.y, point.z] = value;
        }
    }

}
