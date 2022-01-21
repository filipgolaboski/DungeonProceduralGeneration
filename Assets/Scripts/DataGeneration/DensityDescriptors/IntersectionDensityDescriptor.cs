using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Densities/Intersection")]
public class IntersectionDensityDescriptor : BaseDensityDescriptor
{
    public BaseDensityDescriptor lhsDensityDescriptor;
    public BaseDensityDescriptor rhsDensityDescriptor;
    public float smoothing = 1;

    public override float GetDensity(Vector3Int point)
    {
        float densityOne = lhsDensityDescriptor.GetDensity(point);
        float densityTwo = rhsDensityDescriptor.GetDensity(point);
        float result = 0;
        Utilities.Intersection_Smooth(densityOne, densityTwo, smoothing,out result);
        return result;
    }

    public override float GetDensity(Vector3Int point, Vector3Int position)
    {
        float densityOne = lhsDensityDescriptor.GetDensity(point,position);
        float densityTwo = rhsDensityDescriptor.GetDensity(point,position);
        float result = 0;
        Utilities.Intersection_Smooth(densityOne, densityTwo, smoothing, out result);
        return result;
    }

}
