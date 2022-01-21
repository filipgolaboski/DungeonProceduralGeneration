using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/ComplementDensityDescriptor")]
public class ComplementDensityDescriptor : BaseDensityDescriptor
{
    public BaseDensityDescriptor density;


    public override float GetDensity(Vector3Int point)
    {
        return 1 - density.GetDensity(point);
    }

    public override float GetDensity(Vector3Int point, Vector3Int position)
    {
        return 1 - density.GetDensity(point, position);
    }
}
