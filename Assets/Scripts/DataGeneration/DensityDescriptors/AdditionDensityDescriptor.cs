using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/Addition")]
public class AdditionDensityDescriptor : BaseDensityDescriptor
{
    public BaseDensityDescriptor lhsDensityDescriptor;
    public BaseDensityDescriptor rhsDensityDescriptor;

    public override float GetDensity(Vector3Int point)
    {
        return lhsDensityDescriptor.GetDensity(point) + rhsDensityDescriptor.GetDensity(point);
    }

    public override float GetDensity(Vector3Int point, Vector3Int position)
    {
        return lhsDensityDescriptor.GetDensity(point,position) + rhsDensityDescriptor.GetDensity(point,position);
    }

}
