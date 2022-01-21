using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/FlatDensityDescriptor")]
public class FlatDensityDescriptor : BaseDensityDescriptor
{
    public float value;
    public override float GetDensity(Vector3Int point)
    {
        return value;
    }
}
